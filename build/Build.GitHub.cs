using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Octokit;
using Octokit.Internal;
using System.IO;

[GitHubActions("For each PR",
    GitHubActionsImage.WindowsLatest, OnPullRequestBranches = new[]
    {
        DevelopBranch, MainBranch, MasterBranch
    },
    PublishArtifacts = false,
    FetchDepth = 0,
    InvokedTargets = new[] { nameof(Tests) },
    CacheKeyFiles = new[] { "**/global.json", "**/*.csproj" },
    CacheIncludePatterns = new[] { ".nuke/temp", "~/.nuget/packages" },
    EnableGitHubToken = true
)]
[GitHubActions(
"Create Realease",
    GitHubActionsImage.WindowsLatest,
    OnPushBranches = new[] { MasterBranch, MainBranch, DevelopBranch },
    PublishArtifacts = true,
    FetchDepth = 0,
    InvokedTargets = new[] { nameof(Tests), nameof(PublishToGithub) },
    CacheKeyFiles = new[] { "**/global.json", "**/*.csproj" },
    CacheIncludePatterns = new[] { ".nuke/temp", "~/.nuget/packages" },
    EnableGitHubToken = true
)]
public partial class Build : NukeBuild
{
    const string MainBranch = "main";
    const string MasterBranch = "master";
    const string DevelopBranch = "develop";

    /// <summary>
    /// It is not heavy tested but should work.
    /// </summary>
    Target PublishToGithub => _ => _
        .DependsOn(Publish)
        .DependsOn(Tests)
        .OnlyWhenStatic(() => GitRepository.IsGitHubRepository())
        .Executes(() =>
        {

            Assert.True(GitRepository.IsGitHubRepository());
            var owner = GitRepository.GetGitHubOwner();
            var repositorytName = GitRepository.GetGitHubName();

            var credentials = new Credentials(GitHubActions.Instance.Token);
            GitHubTasks.GitHubClient = new GitHubClient(
                new ProductHeaderValue(nameof(NukeBuild)),
                new InMemoryCredentialStore(credentials));

            var newRelease = new NewRelease(GitVersion.SemVer)
            {
                TargetCommitish = GitVersion.Sha,
                Draft = true,
                Name = $"{GitVersion.SemVer}",
                Prerelease = !GitVersion.PreReleaseLabel.IsNullOrWhiteSpace(),
                Body = @"It is a release!"
            };

            var repositoryId = GitHubTasks.GitHubClient.Repository.Get(owner, repositorytName).Result.Id;
            var createdRelease = GitHubTasks.GitHubClient.Repository.Release.Create(repositoryId, newRelease).Result;

            UploadReleaseAssetToGithub(createdRelease, PublishedProjectAsZip);
            var _ = GitHubTasks.GitHubClient.Repository.Release
                .Edit(repositoryId, createdRelease.Id, new ReleaseUpdate { Draft = false })
                .Result;
        });

    void UploadReleaseAssetToGithub(Release release, AbsolutePath asset)
    {
        Assert.FileExists(asset);

        var releaseAssetUpload = new ReleaseAssetUpload
        {
            ContentType = "application/x-binary",
            FileName = $"{NameOfProjectToBePublished}-{GitVersion.SemVer}",
            RawData = File.OpenRead(asset)
        };
        var _ = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, releaseAssetUpload).Result;
    }
}
