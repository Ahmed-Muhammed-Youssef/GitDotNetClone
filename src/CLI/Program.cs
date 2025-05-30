using CLI.Commands;
using CLI.Services;

namespace CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var commands = new List<IGitCommand>
{
                new InitCommand(),
                new AddCommand(new GitContextProvider())
            };

            var runner = new CommandRunner(commands);
            await runner.RunAsync(args);
        }
    }
}
