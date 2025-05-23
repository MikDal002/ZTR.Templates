using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities;

[GitHubActions(
    "Create Velopack Release",
    GitHubActionsImage.WindowsLatest,
    OnPushBranches = new[] { MasterBranch, MainBranch, DevelopBranch },
    PublishArtifacts = true,
    FetchDepth = 0,
    InvokedTargets = new[] { nameof(Tests), nameof(PublishToGitHubWithVelopack) },
    CacheKeyFiles = new[] { "**/global.json", "**/*.csproj" },
    CacheIncludePatterns = new[] { ".nuke/temp", "~/.nuget/packages" },
    EnableGitHubToken = true
)]

partial class Build
{
    const string MainBranch = "main";
    const string MasterBranch = "master";
    const string DevelopBranch = "develop";

    [Parameter][Secret] readonly string GitHubToken;

    Target PublishToGitHubWithVelopack => _ => _
        .DependsOn(PackWithVelopack)
        .Requires(() => GitHubToken)
        .OnlyWhenStatic(() => !GitHubToken.IsNullOrWhiteSpace(), "GitHubToken is not available.")
        .OnlyWhenStatic(() => GitRepository.IsGitHubRepository())
        .Executes(() =>
        {
            var gitHubBrowseUrl = GitRepository.GetGitHubBrowseUrl();
            var isPrerelease = !GitVersion.PreReleaseLabel.IsNullOrWhiteSpace();
            var releaseName = GitVersion.FullSemVer;
            var tag = GitVersion.FullSemVer; 
            var arguments = new ArgumentStringHandler()
                .Append("upload github")
                .Append("--outputDir {0}", VelopackPublish)
                .Append("--channel {0}", Channel)
                .Append("--repoUrl {0}", gitHubBrowseUrl)
                .Append("--token {0}", GitHubToken, ArgumentStringHandlerExtensions.IsSecret.Yes)
                .Append("--publish")
                .Append("--releaseName {0}", releaseName)
                .Append("--tag {0}", tag);

            if (isPrerelease)
            {
                arguments.Append("--pre");
            }

            Vpk.Invoke(arguments);
        });
}

