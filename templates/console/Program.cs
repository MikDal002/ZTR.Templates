﻿using ConsoleTemplate.Commands.Base; // Added for GlobalCommandSettings
using ConsoleTemplate.DependencyInjection;
using ConsoleTemplate.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.Threading.Tasks;
using Velopack;

namespace ConsoleTemplate;
class Program
{
    static async Task Main(string[] args)
    {
        // Update the application
        VelopackApp.Build().Run();

        // Get the log console option name dynamically
        var logConsoleOption = CommandOptionExtensions.GetLongOptionName<GlobalCommandSettings, bool>(s => s.LogToConsole);
        var enableConsoleLogging = false;
        if (!string.IsNullOrEmpty(logConsoleOption))
        {
            enableConsoleLogging = args.Contains(logConsoleOption);
        }
        else
        {
            System.Console.Error.WriteLine("[Warning] Could not dynamically retrieve --log-console option name. Console logging may not be enabled as expected.");
        }

        // Set up the command app
        var app = new CommandApp(new TypeRegistrar(enableConsoleLogging));

        // Register commands
        app.Configure(config =>
        {
#if DEBUG
            config.ValidateExamples();
#endif

            config.SetApplicationName("ConsoleTemplate");
            config.SetHelpProvider(new CustomHelpProvider(config.Settings));

            config.AddCommand<ExampleCommand>("commandName");
            config.AddCommand<UpdateCommand>("version")
                .WithExample("version", "--update");

            config.SetExceptionHandler((ex, _) =>
            {
                AnsiConsole.WriteException(ex);
                return -99;
            });

        });

        await app.RunAsync(args);
    }
}
