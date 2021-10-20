using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace ConsoleTemplate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.Description = "Application description";
            rootCommand.Add(new Option<int>("--an-int", getDefaultValue: () => 42, description: "Description 1"));
            rootCommand.Add(new Option<string>("--a-string"));
            rootCommand.Add(new Command("Subcommand"));

            rootCommand.Handler = CommandHandler.Create<int, string, CancellationToken>(DoSomething);


            await rootCommand.InvokeAsync(args);
        }

        public static void DoSomething(int anInt, string aString, CancellationToken token)
        {
            Console.WriteLine("Hello world!");
        }
    }
}
