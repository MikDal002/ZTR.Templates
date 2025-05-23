using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

partial class Build
{
    [NuGetPackage("vpk", "vpk")] public readonly Tool Vpk;

    string Channel => $"{OperationSystem}-{SystemArchitecture}" + (GitVersion.PreReleaseLabel.IsNullOrWhiteSpace() ? "" : "-alpha");

    AbsolutePath VelopackDirectory = RootDirectory / "Velopack";
    AbsolutePath VelopackPublish => VelopackDirectory / "publish";

    Target CleanVelopack => _ => _
        .TriggeredBy(Clean)
        .Executes(() =>
        {
            VelopackDirectory.DeleteDirectory();
        });

    Target PackWithVelopack => _ => _
        .DependsOn(ConfigureAppSettings)
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
}
