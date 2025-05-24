using CLI.Commands;

namespace CLI
{
    public class CommandRunner
    {
        private readonly Dictionary<string, IGitCommand> _commands = [];
        public CommandRunner(IEnumerable<IGitCommand> commands)
        {
            _commands = commands.ToDictionary(cmd => cmd.Name);
        }
        public async Task RunAsync(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No command provided. Use 'git help' for a list of commands.");
                return;
            }
            var commandName = args[0];
            if (_commands.TryGetValue(commandName, out var command))
            {
                await command.ExecuteAsync(args.Skip(1).ToArray());
            }
            else
            {
                Console.WriteLine($"Unknown command: {commandName}. Use 'git help' for a list of commands.");
            }
        }
    }
}
