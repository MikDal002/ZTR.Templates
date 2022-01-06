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
            var anIntOption = new Option<int>("--an-int", getDefaultValue: () => 42, description: "Description 1");
            var aStringOption = new Option<string>("--a-string");

            var rootCommand = new RootCommand()
            {
                anIntOption, aStringOption,
                new Command("subverb")
            };
            rootCommand.Description = "Application description";
            rootCommand.SetHandler((int myInt, string myString, CancellationToken token) => DoSomething(myInt, myString, token), anIntOption, aStringOption);

            await rootCommand.InvokeAsync(args);
        }

        public static void DoSomething(int anInt, string aString, CancellationToken token)
        {
            Console.WriteLine("Hello world!");
        }
    }
}
