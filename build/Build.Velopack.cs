using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

partial class Build
{
    [PathVariable]
    public Tool Vpk;

    [PathVariable]
    public Tool Scp;

    AbsolutePath VelopackDirectory = RootDirectory / "Velopack";
    AbsolutePath VelopackPublish => VelopackDirectory / "Publish";
    AbsolutePath VelopackRelease => VelopackDirectory / "Release";

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
            
            var channel = "";
            if (GitVersion.PreReleaseLabel.IsNullOrWhiteSpace())
            {
                channel += "-alpha";
            }

            Vpk.Invoke($@"vpk [{OperationSystem}] pack -u {NameOfProjectToBePublished} -v {GitVersion.FullSemVer}" +
                       $@" -p {PublishDirectory} --icon {iconPath} --outputDir {VelopackRelease}" +
                       $@" --runtime {OperationSystem}-{SystemArchitecture} --channel {OperationSystem}-{SystemArchitecture}{channel} --delta none");
        });
}
