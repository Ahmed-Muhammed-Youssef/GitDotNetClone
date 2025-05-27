using CLI.Commands;

namespace CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var commands = new List<IGitCommand>
{
                new InitCommand(),
                new AddCommand()
            };

            var runner = new CommandRunner(commands);
            await runner.RunAsync(args);
        }
    }
}
