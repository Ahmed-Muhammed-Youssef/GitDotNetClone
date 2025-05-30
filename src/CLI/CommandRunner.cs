using CLI.Commands;

namespace CLI
{
    public class CommandRunner(Dictionary<string, Func<string[], IGitCommand?>> commandsFactoriesDictionary)
    {
        public async Task RunAsync(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No command provided. Use 'git help' for a list of commands.");
                return;
            }
            string commandName = args[0];

            if (commandsFactoriesDictionary.TryGetValue(commandName, out var commandFactory))
            {

                IGitCommand? command = commandFactory.Invoke(args.Skip(1).ToArray());

                if(command is null)
                {
                    return;
                }
                
                await command.ExecuteAsync();
            }
            else
            {
                Console.WriteLine($"Unknown command: {commandName}. Use 'git help' for a list of commands.");
            }
        }
    }
}
