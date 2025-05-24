namespace CLI.Commands
{
    public interface IGitCommand
    {
        public string Name { get; }
        Task ExecuteAsync(string[] args);
    }
}
