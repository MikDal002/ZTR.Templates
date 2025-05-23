using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities;
using Serilog;
using System.IO;

partial class Build
{
    [NuGetPackage("vpk", "vpk.dll")] public readonly Tool Vpk;

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

            var releaseNotesFilePath = GenerateAndSaveChangelogFile();

            Vpk.Invoke($"vpk [{OperationSystem}] pack -u {NameOfProjectToBePublished} -v {GitVersion.FullSemVer}" +
                       $" -p {PublishDirectory} --icon {iconPath} --outputDir {VelopackPublish}" +
                       $" --runtime {OperationSystem}-{SystemArchitecture} --channel {Channel} --delta none" +
                       $" --releaseNotes {releaseNotesFilePath}");
        });

    AbsolutePath GenerateAndSaveChangelogFile()
    {
        var releaseNotesFile = TemporaryDirectory / "release_notes.md";

        try
        {
            var commitLogOutput = GitTasks.Git($"log HEAD --pretty=\"format:%s\" --no-merges", logOutput: true, workingDirectory: RootDirectory)
                                          .StdToText();
            File.WriteAllText(releaseNotesFile, commitLogOutput.ToString());
        }
        catch (System.Exception ex)
        {
            Log.Warning(ex, "Failed to retrieve raw commit subjects.");
        }

        return releaseNotesFile;
    }
}
