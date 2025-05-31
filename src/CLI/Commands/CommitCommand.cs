namespace CLI.Commands
{
    public class CommitCommand : IGitCommand
    {
        public static string Name => "commit";
        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
        public static IGitCommand? Create(string[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            return new CommitCommand();
        }
    }
}
