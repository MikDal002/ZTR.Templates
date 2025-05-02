using Microsoft.Extensions.Options;
using NuGet.Versioning;
using System;
using System.Threading.Tasks;
using Velopack;

namespace ConsoleTemplate;

public interface IUpdateService
{
    SemanticVersion? GetCurrentVersion();
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task DownloadAndApplyUpdatesAsync(UpdateInfo newVersion);
}

public class UpdateService : IUpdateService // Removed primary constructor
{
    private readonly UpdateManager _updateManager;

    // Constructor injection
    public UpdateService(IOptions<ZtrTemplates.Configuration.Shared.UpdateOptions> updateOptions)
    {
        var url = updateOptions.Value.UpdateUrl;
        if (string.IsNullOrWhiteSpace(url))
        {
            // Handle missing URL - throw exception or log error
            throw new InvalidOperationException("Update URL is not configured in appsettings.json under UpdateOptions section.");
        }

        _updateManager = new UpdateManager(url);
    }

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
