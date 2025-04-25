using Spectre.Console;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using System.Collections.Generic;

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

public class AddAccordingToTasksCommand : AsyncCommand<AddAccordingToTasksSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, AddAccordingToTasksSettings settings)
    {
        
        // Create a list of Items, apply separate styles to each
        var rows = new List<Text>(){
            new Text($"Limit: {settings.Limit}", new Style(Color.Red, Color.Black)),
            new Text($"Directory {settings.Directory}", new Style(Color.Green, Color.Black)),
            new Text($"Leftovers {context.Remaining}", new Style(Color.Blue, Color.Black))
        };

        // Renders each item with own style
        AnsiConsole.Write(new Rows(rows));
        // Implement your logic here
        return Task.FromResult(0);
    }
}
