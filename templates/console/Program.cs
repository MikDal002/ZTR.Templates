using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTemplate;

class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand();
        rootCommand.Description = "<<Application description>>";
        rootCommand.AddCommand(new AddAccordingToTasksCommand());

        await rootCommand.InvokeAsync(args);
    }

    public static void DoSomething(int anInt, string aString, CancellationToken token)
    {
        Console.WriteLine("Hello world!");
    }
}
