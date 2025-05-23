using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
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
    [Parameter] string GitHubBrowseUrl => GitRepository.ToString();

    Target DownloadGithubRelease => _ => _
        .Before(PackWithVelopack)
        .Requires(() => GitHubToken)
        .OnlyWhenStatic(() => !GitHubToken.IsNullOrWhiteSpace(), "GitHubToken is not available.")
        .OnlyWhenStatic(() => GitRepository.IsGitHubRepository())
        .Executes(() =>
        {
            var isPrerelease = !GitVersion.PreReleaseLabel.IsNullOrWhiteSpace();

            var arguments = new ArgumentStringHandler()
                .Append("download github")
                .Append("--repoUrl", GitHubBrowseUrl)
                .Append("--token", GitHubToken, ArgumentStringHandlerExtensions.IsSecret.Yes)
                .Append("--outputDir", VelopackPublish)
                .Append("--channel", Channel);

            if (isPrerelease)
            {
                arguments = arguments.Append("--pre");
            }

            var foo = arguments.ToStringAndClear();
            Vpk.Invoke(arguments);
        });

    Target PublishToGitHubWithVelopack => _ => _
        .DependsOn(PackWithVelopack)
        .DependsOn(DownloadGithubRelease)
        .Requires(() => GitHubToken)
        .OnlyWhenStatic(() => !GitHubToken.IsNullOrWhiteSpace(), "GitHubToken is not available.")
        .OnlyWhenStatic(() => GitRepository.IsGitHubRepository())
        .Executes(() =>
        {
            var isPrerelease = !GitVersion.PreReleaseLabel.IsNullOrWhiteSpace();
            var releaseName = GitVersion.FullSemVer;
            var tag = GitVersion.FullSemVer; 
            var arguments = new ArgumentStringHandler()
                .Append("upload github")
                .Append("--outputDir", VelopackPublish)
                .Append("--channel", Channel)
                .Append("--repoUrl", GitHubBrowseUrl)
                .Append("--token", GitHubToken, ArgumentStringHandlerExtensions.IsSecret.Yes)
                .Append("--publish")
                .Append("--releaseName", releaseName)
                .Append("--tag", tag);

            if (isPrerelease)
            {
                arguments = arguments.Append("--pre");
            }

            Vpk.Invoke(arguments);
        });
}

