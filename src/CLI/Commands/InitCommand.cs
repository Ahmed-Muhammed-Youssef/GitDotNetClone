namespace CLI.Commands
{
    public class InitCommand : IGitCommand
    {
        public string Name => "init";

        public async Task ExecuteAsync(string[] args)
        {
            // Your .git directory setup logic here
            if (Directory.Exists(".git"))
            {
                Console.WriteLine("A Git repository already exists in this directory.");
                return;
            }

            Directory.CreateDirectory(".git");
            // You can add more initialization logic here, such as creating default files like HEAD, config, etc.
            await Task.CompletedTask;
        }
    }
}
