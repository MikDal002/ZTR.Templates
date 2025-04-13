using System;
using System.CommandLine;
using System.Threading.Tasks;
using Velopack;

namespace ConsoleTemplate;

class Program
{
    static async Task Main(string[] args)
    {
        // Aktualizacja aplikacji
        Velopack.VelopackApp.Build().Run();

        await UpdateMyApp();

        var rootCommand = new RootCommand();
        rootCommand.Description = "<<Application description>>";
        rootCommand.AddCommand(new AddAccordingToTasksCommand());

        await rootCommand.InvokeAsync(args);
    }

    private static async Task UpdateMyApp()
    {
        var mgr = new UpdateManager("https://frog02-20366.wykr.es/bee/downloads");

        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion is null) return;

        var version = newVersion.ToString();
        Console.WriteLine(version);

        await mgr.DownloadUpdatesAsync(newVersion);

        mgr.ApplyUpdatesAndRestart(newVersion);

    }
}
