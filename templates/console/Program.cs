using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;
using Velopack;
using ConsoleTemplate.DependencyInjection; // Added for TypeRegistrar namespace

namespace ConsoleTemplate;
class Program
{
    static async Task Main(string[] args)
    {
        // Update the application
        VelopackApp.Build().Run();

        // Set up the command app
        var app = new CommandApp(new TypeRegistrar());

        // Register commands
        app.Configure(config =>
        {
#if DEBUG
            config.ValidateExamples();
#endif

            config.SetApplicationName("ConsoleTemplate");
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
