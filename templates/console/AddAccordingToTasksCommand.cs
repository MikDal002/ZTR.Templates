using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace ConsoleTemplate;

class AddAccordingToTasksCommand : Command
{
    public AddAccordingToTasksCommand() : base("commandName", "Command Description")
    {
        AddOption(new Option<int?>("--limit", () => 4, "Quite long description."));
        AddOption(new Option<DirectoryInfo>("--directory", "desc") { IsRequired = true });

        Handler = CommandHandler.Create(this.Execute);
    }

    public Task<int> Execute(int? limit, DirectoryInfo katalogZdjec, int startowyEan)
    {
        return Task.FromResult(0);
    }
}
