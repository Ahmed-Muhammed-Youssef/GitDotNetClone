using CLI.Commands;

namespace CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var commandFactoriesDictionary = new Dictionary<string, Func<string[], IGitCommand?>>
            {
                { InitCommand.Name, InitCommand.Create },
                { AddCommand.Name, AddCommand.Create },
                { CommitCommand.Name, CommitCommand.Create },
                { StatusCommand.Name, StatusCommand.Create }
            };

            var runner = new CommandRunner(commandFactoriesDictionary);
            await runner.RunAsync(args);
        }
    }
}
