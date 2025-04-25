using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System.IO;

public partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.PackWithVelopack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("System to build, deafults to current system. Change if you want to cross compile")]
    readonly OperationSystem OperationSystem = OperationSystem.GetCurrentConfiguration();

    [Parameter("System architecture to build, deafults to current architecture. Change if you want to cross compile")]
    readonly SystemArchitecture SystemArchitecture = SystemArchitecture.GetCurrentConfiguration();


    string Runtime => $"{OperationSystem}-{SystemArchitecture}";

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
    /// </summary>

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;

    const string NameOfProjectToBePublished = "ConsoleTemplate";
    Project ProjectToPublish => Solution.GetProject(NameOfProjectToBePublished);

    AbsolutePath PublishedProjectAsZip =>
        PackagesDirectory / NameOfProjectToBePublished + ".zip";
    AbsolutePath SourceDirectory => RootDirectory / "templates";
    AbsolutePath PublishDirectory => RootDirectory / "output";
    AbsolutePath PackagesDirectory => RootDirectory / "packages";
    AbsolutePath TestResultDirectory => RootDirectory / "testResults";

    Target Clean => _ => _
        .Before(Restore)
        .DependentFor(Restore)
        .Unlisted()
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            PublishDirectory.DeleteDirectory();
            PackagesDirectory.DeleteDirectory();
            TestResultDirectory.DeleteDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(ProjectToPublish)
                .SetRuntime(Runtime));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information($"Compiling {GitVersion.SemVer} version");

            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(ProjectToPublish)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetRuntime(Runtime)
                .SetTreatWarningsAsErrors(true)
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .DependsOn(Tests)
        .Executes(() =>
        {
            PublishDirectory.CreateOrCleanDirectory();

            Log.Information("Publishing {projectToPublish} project to {filePath} directory.", ProjectToPublish,
                PublishDirectory);

            DotNetTasks.DotNetPublish(s => s.SetProject(ProjectToPublish)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetSelfContained(true)
                .SetPublishTrimmed(true)
                .SetRuntime(Runtime)
                .SetNoBuild(true)
                );
        });

    Target Tests => _ => _
       .DependsOn(Compile)
       .TriggeredBy(Compile)
       .Executes(() =>
       {
           TestResultDirectory.CreateOrCleanDirectory();
           DotNetTasks.DotNetTest(s => s.SetConfiguration(Configuration)
               .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
               .SetRuntime(Runtime)
               .EnableNoBuild()
               .SetProjectFile(Solution));
       });

    Target CreateVersionLabel => _ => _
        .TriggeredBy(Publish)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnDevelopBranch())
        .Executes(() =>
        {
            Log.Information($"Pushing new tag about the version {GitVersion.FullSemVer}");

            if (!IsLocalBuild)
            {
                GitTasks.Git($"config --global user.email \"build@ourcompany.com\"");
                GitTasks.Git($"config --global user.name \"Our Company Build\"");
            }

            GitTasks.Git($"tag -a {GitVersion.FullSemVer} -m \"Setting git tag on commit to '{GitVersion.FullSemVer}'\"");
            GitTasks.Git($"push --set-upstream origin {GitVersion.FullSemVer}");
        });

}
