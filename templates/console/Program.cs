using Spectre.Console;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Velopack;

namespace ConsoleTemplate;
class Program
{
    static async Task Main(string[] args)
    {
        // Update the application
        VelopackApp.Build().Run();
        
        // Set up the command app
        var app = new CommandApp();
        // Register commands
        app.Configure(config =>
        {
            #if DEBUG
            config.ValidateExamples();
            #endif   
            
            config.SetApplicationName("ConsoleTemplate");
            config.AddCommand<AddAccordingToTasksCommand>("commandName");
            config.AddCommand<VersionCommand>("version");

            config.SetExceptionHandler((ex, _) =>
            {
                AnsiConsole.WriteException(ex);
                return -99;
            });

        });
        await app.RunAsync(args);
    }
}
