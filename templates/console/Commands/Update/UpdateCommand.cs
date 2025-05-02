using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ConsoleTemplate;

public class VersionCommandSettings : CommandSettings
{
    [CommandOption("--update")]
    [Description("Checks for a new version and updates if available.")]
    public bool Update { get; set; }
}

public class UpdateCommand(IUpdateService updateService) : AsyncCommand<VersionCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, VersionCommandSettings settings)
    {
        var currentVersion = updateService.GetCurrentVersion();
        if (currentVersion is not null)
        {
            AnsiConsole.MarkupLine($"[green]Current version: {currentVersion}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Unable to determine the current version.[/]");
        }

        var newVersion = await AnsiConsole.Status()
            .StartAsync("Checking for updates...",
                async _ => await updateService.CheckForUpdatesAsync());
        if (newVersion is null)
        {
            AnsiConsole.MarkupLine("[green]You are already using the latest version.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[yellow]A new version is available: {newVersion}[/]");

        if (!settings.Update)
        {
            return 0;
        }

        var confirm = await AnsiConsole.ConfirmAsync("[blue]The application will restart after the update. Do you want to continue?[/]");
        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Update canceled by the user.[/]");
            return 0;
        }

        await AnsiConsole.Status()
            .StartAsync("Downloading and applying updates...",
                async _ => await updateService.DownloadAndApplyUpdatesAsync(newVersion));

        AnsiConsole.MarkupLine("[blue]The application will now exit to apply the updates.[/]");

        return 0;
    }
}
