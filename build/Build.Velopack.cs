using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

partial class Build
{
    [PathVariable] public Tool Vpk;
    [PathVariable] public Tool Scp;
    [PathVariable] public Tool Ssh;

    [Parameter] readonly int SshPort = 10366;
    [Parameter] readonly string SshUser = "frog";
    [Parameter] readonly string SshServer = "frog02.mikr.us";
    [Parameter] readonly int MaxReleasesOnServer = 2;
    [Parameter] readonly string DirectoryForReleases = "releases";
    string Channel => $"{OperationSystem}-{SystemArchitecture}" + (GitVersion.PreReleaseLabel.IsNullOrWhiteSpace() ? "" : "-alpha");


    AbsolutePath SshPrivateKey = RootDirectory / "ztrtemplates";

    AbsolutePath VelopackDirectory = RootDirectory / "Velopack";
    AbsolutePath VelopackReleaseOnRemoteServer => VelopackDirectory / DirectoryForReleases;
    AbsolutePath VelopackPublish => VelopackDirectory / "publish";
    string NameProjectDirectoryOnTheServer => NameOfProjectToBePublished.ToLower();

    Target CleanVelopack => _ => _
        .TriggeredBy(Clean)
        .Executes(() =>
        {
            VelopackDirectory.DeleteDirectory();
        });

    Target PackWithVelopack => _ => _
        .DependsOn(Publish)
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
        .DependsOn(PackWithVelopack)
        .Executes(() =>
        {
            Ssh.Invoke($"{SshUser}@{SshServer} -i {SshPrivateKey} -p {SshPort} 'mkdir -p /var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases}'");
            Scp.Invoke(
                $"-i {SshPrivateKey} -r -P {SshPort} {SshUser}@{SshServer}:/var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases} {VelopackReleaseOnRemoteServer}");

            Vpk.Invoke($"vpk download local --path {VelopackReleaseOnRemoteServer} --channel {Channel} --outputDir {VelopackPublish}");
        });

    Target UploadLocalToServer => _ => _
        .DependsOn(Tests)
        .DependsOn(DownloadServerToLocal)
        .Executes(() =>
        {
            Vpk.Invoke($"vpk upload local --path {VelopackReleaseOnRemoteServer} --channel {Channel} --keepMaxReleases {MaxReleasesOnServer} --outputDir {VelopackPublish} --regenerate");
            Scp.Invoke($"-i {SshPrivateKey} -r -P {SshPort} {VelopackReleaseOnRemoteServer} {SshUser}@{SshServer}:/var/www/html/{NameProjectDirectoryOnTheServer}/");
            Ssh.Invoke($"{SshUser}@{SshServer} -i {SshPrivateKey} -p {SshPort} 'chmod 755 /var/www/html/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases}'");

        });
}
