namespace CLI.Commands
{
    public class InitCommand : IGitCommand
    {
        public string Name => "init";

        public async Task ExecuteAsync(string[] args)
        {
            const string gitDir = ".git";
            var objectsDir = Path.Combine(gitDir, "objects");
            var infoDir = Path.Combine(objectsDir, "info");
            var packDir = Path.Combine(objectsDir, "pack");

            if (Directory.Exists(gitDir))
            {
                Console.WriteLine("A Git repository already exists in this directory.");
                return;
            }

            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(objectsDir);
            Directory.CreateDirectory(infoDir);
            Directory.CreateDirectory(packDir);

            Console.WriteLine("Initialized empty git repository.");

            await Task.CompletedTask;
        }
    }
}
