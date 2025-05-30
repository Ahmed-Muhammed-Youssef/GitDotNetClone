namespace CLI.Commands
{
    public class InitCommand() : IGitCommand
    {
        public static string Name => "init";
        private static readonly string _gitDir = ".git";
        private static readonly string _objectsDir = Path.Combine(_gitDir, "objects");
        private static readonly string _infoDir = Path.Combine(_objectsDir, "info");
        private static readonly string _packDir = Path.Combine(_objectsDir, "pack");

        public async Task ExecuteAsync()
        {
            Directory.CreateDirectory(_gitDir);
            Directory.CreateDirectory(_objectsDir);
            Directory.CreateDirectory(_infoDir);
            Directory.CreateDirectory(_packDir);

            Console.WriteLine("Initialized empty git repository.");

            await Task.CompletedTask;
        }
        public static IGitCommand? Create(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: git init");
                return null;
            }

            if (Directory.Exists(_gitDir))
            {
                Console.WriteLine("A Git repository already exists in this directory.");
                return null;
            }
            return new InitCommand();
        }
    }
}
