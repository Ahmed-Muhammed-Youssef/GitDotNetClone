using CLI.Commands;
using CLI.Services;

namespace CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var commandFactoriesDictionary = new Dictionary<string, Func<IGitCommand?>>
            {
                { InitCommand.Name, InitCommand.Create },
                { AddCommand.Name, AddCommand.Create }
            };

            var runner = new CommandRunner(commandFactoriesDictionary);
            await runner.RunAsync(args);
        }
    }
}
