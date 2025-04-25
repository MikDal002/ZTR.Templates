using NuGet.Versioning;
using System.Threading.Tasks;
using Velopack;

namespace ConsoleTemplate;

public interface IUpdateService
{
    SemanticVersion? GetCurrentVersion();
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task DownloadAndApplyUpdatesAsync(UpdateInfo newVersion);
}

public class UpdateService(string updateUrl) : IUpdateService
{
    private readonly UpdateManager _updateManager = new(updateUrl);

    public SemanticVersion? GetCurrentVersion()
    {
        return _updateManager.CurrentVersion;
    }
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        return await _updateManager.CheckForUpdatesAsync();
    }
    public async Task DownloadAndApplyUpdatesAsync(UpdateInfo newVersion)
    {
        await _updateManager.DownloadUpdatesAsync(newVersion);
        _updateManager.ApplyUpdatesAndRestart(newVersion);
    }
}
