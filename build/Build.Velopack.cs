using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Renci.SshNet;

partial class Build
{
    [PathVariable] public Tool Vpk;
    [PathVariable] public Tool Scp;
    [PathVariable] public Tool Ssh;

    [Parameter] readonly int SshPort = 10366;
    [Parameter] readonly string SshUser = "frog";
    [Parameter] readonly string SshServer = "frog02.mikr.us";
    ConnectionInfo SshConnectionInfo => new(SshServer, port: SshPort, SshUser, new PrivateKeyAuthenticationMethod(SshUser, new PrivateKeyFile(SshPrivateKey)));

    [Parameter] readonly int MaxReleasesOnServer = 2;
    [Parameter] readonly string DirectoryForReleases = "releases";
    string Channel => $"{OperationSystem}-{SystemArchitecture}" + (GitVersion.PreReleaseLabel.IsNullOrWhiteSpace() ? "" : "-alpha");

    AbsolutePath SshPrivateKey = RootDirectory / "ztrtemplates";

    AbsolutePath VelopackDirectory = RootDirectory / "Velopack";
    AbsolutePath VelopackReleaseMirroredFromRemoteServer => VelopackDirectory / DirectoryForReleases;
    AbsolutePath VelopackPublish => VelopackDirectory / "publish";
    string NameProjectDirectoryOnTheServer => NameOfProjectToBePublished.ToLower();

    Target CleanVelopack => _ => _
        .TriggeredBy(Clean)
        .Executes(() =>
        {
            VelopackDirectory.DeleteDirectory();
        });

    Target PackWithVelopack => _ => _
        .DependsOn(ConfigureAppSettings)
        .DependsOn(CleanVelopack)
        .Executes(() =>
        {
            var iconPath = ProjectToPublish.Directory / "applogo.ico";
            Assert.True(iconPath.FileExists());

            Vpk.Invoke($@"vpk [{OperationSystem}] pack -u {NameOfProjectToBePublished} -v {GitVersion.FullSemVer}" +
                       $@" -p {PublishDirectory} --icon {iconPath} --outputDir {VelopackPublish}" +
                       $@" --runtime {OperationSystem}-{SystemArchitecture} --channel {Channel} --delta none");
        });

    Target DownloadServerToLocal => _ => _
        .Executes(() =>
        {
            using var sshClient = new SshClient(SshConnectionInfo);
            sshClient.Connect();
            using var cmd = sshClient.RunCommand($"mkdir -p /var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases}");

            VelopackReleaseMirroredFromRemoteServer.CreateOrCleanDirectory();
            Scp.Invoke(
                $"-i {SshPrivateKey} -r -P {SshPort} {SshUser}@{SshServer}:/var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases} {VelopackReleaseMirroredFromRemoteServer}");

            Vpk.Invoke($"vpk download local --path {VelopackReleaseMirroredFromRemoteServer} --channel {Channel} --outputDir {VelopackPublish}");
        });

    Target UploadLocalToServer => _ => _
        // .DependsOn(PackWithVelopack) // This target should likely depend on PackWithVelopack
        .DependsOn(ConfigureAppSettings) // Ensure settings are configured before packing
        .DependsOn(PackWithVelopack) // Ensure packing happens after configuration
        .DependsOn(DownloadServerToLocal) // Keep existing dependency
        .Executes(() =>
        {
            // Note: The original PackWithVelopack target was modified to depend on ConfigureAppSettings.
            // This UploadLocalToServer target likely needs to run *after* PackWithVelopack.
            // Let's adjust PackWithVelopack dependency instead.

            Vpk.Invoke($"vpk upload local --path {VelopackReleaseMirroredFromRemoteServer} --channel {Channel} --keepMaxReleases {MaxReleasesOnServer} --outputDir {VelopackPublish} --regenerate");
            Scp.Invoke($"-i {SshPrivateKey} -r -P {SshPort} {VelopackReleaseMirroredFromRemoteServer} {SshUser}@{SshServer}:/var/www/html/{NameProjectDirectoryOnTheServer}/");

            using var sshClient = new SshClient(SshConnectionInfo);
            sshClient.Connect();
            using var cmd = sshClient.RunCommand($"chmod 755 /var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases}'");
        });
}
