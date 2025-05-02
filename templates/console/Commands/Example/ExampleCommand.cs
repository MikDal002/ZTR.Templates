using ConsoleTemplate.Commands.Base;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTemplate;
public class AddAccordingToTasksSettings : CommandSettings
{
    [CommandOption("--limit")]
    [Description("Quite long description.")]
    public int? Limit { get; set; } = 4;

    [CommandOption("--directory")]
    [Description("desc")]
    public DirectoryInfo Directory { get; set; }
}

public class ExampleCommand : CancellableAsyncCommand<AddAccordingToTasksSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, AddAccordingToTasksSettings settings, CancellationToken cancellationToken)
    {
        // Create a list of Items, apply separate styles to each
        var rows = new List<Text>(){
            new($"Limit: {settings.Limit}", new(Color.Red, Color.Black)),
            new($"Directory {settings.Directory}", new(Color.Green, Color.Black)),
            new($"Leftovers {context.Remaining}", new(Color.Blue, Color.Black))
        };

        // Renders each item with own style
        AnsiConsole.Write(new Rows(rows));
        // Implement your logic here
        return Task.FromResult(0);
    }
}
