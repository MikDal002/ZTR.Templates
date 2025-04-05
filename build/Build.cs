using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
//using static Nuke.Common.IO.FileSystemTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.ZipDirectory);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;

    const string ProjectToBePublished = "ConsoleTemplate";
    AbsolutePath PublishedProjectAsZip =>
        PackagesDirectory / ProjectToBePublished + ".zip";
    AbsolutePath SourceDirectory => RootDirectory / "templates";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath PackagesDirectory => RootDirectory / "packages";
    AbsolutePath TestResultDirectory => RootDirectory / "testResults";

    Target Clean => _ => _
        .Before(Restore)
        .Unlisted()
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information($"Compiling {GitVersion.SemVer} version");

            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnDevelopBranch())
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();


            var projectToPublish = Solution.GetProject(ProjectToBePublished);
            Log.Information("Publishing {projectToPublish} project to {filePath} directory.", projectToPublish,
                OutputDirectory);

            DotNetTasks.DotNetPublish(s => s.SetProject(projectToPublish)
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory)
                .SetNoBuild(true));
        });

    Target ZipDirectory => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            OutputDirectory
                .ZipTo(PublishedProjectAsZip,
                    compressionLevel: System.IO.Compression.CompressionLevel.SmallestSize,
                    fileMode: System.IO.FileMode.Create);
        });

    Target Tests => _ => _
       .DependsOn(Compile)
       .TriggeredBy(Compile)
       .Executes(() =>
       {
           TestResultDirectory.CreateOrCleanDirectory();
           DotNetTasks.DotNetTest(s => s.SetConfiguration(Configuration)
               .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
               .EnableNoBuild()
               .SetProjectFile(Solution));
       });

    Target CreateVersionLabel => _ => _
        .TriggeredBy(Publish)
        .Executes(() =>
        {
            Log.Information($"Pushing new tag about the version {GitVersion.FullSemVer}");


            GitTasks.Git($"config --global user.email \"build@ourcompany.com\"");
            GitTasks.Git($"config --global user.name \"Our Company Build\"");

            GitTasks.Git($"tag -a {GitVersion.FullSemVer} -m \"Setting git tag on commit to '{GitVersion.FullSemVer}'\"");
            GitTasks.Git($"push --set-upstream origin {GitVersion.FullSemVer}");
        });

}
