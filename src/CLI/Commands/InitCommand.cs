using Core.Objects;
using Core.Services;
using System.Text.Json;

namespace CLI.Commands
{
    public class InitCommand(JsonSerializerOptions jsonOptions, string repoPath) : IGitCommand
    {
        public static string Name => "init";
        private readonly string _gitDir = Path.Combine(repoPath, ".git");
        private readonly string _objectsDir = Path.Combine(repoPath, ".git", "objects");
        private readonly string _infoDir = Path.Combine(repoPath, ".git", "objects", "info");
        private readonly string _packDir = Path.Combine(repoPath, ".git", "objects", "pack");
        private readonly string _refsHeadsDir = Path.Combine(repoPath, ".git", "refs", "heads");
        private readonly string _headFilePath = Path.Combine(repoPath, ".git", "HEAD");

        /// <summary>
        /// Executes the initialization of a new Git repository by creating the required directory structure.
        /// This includes the .git directory and its subdirectories: objects, objects/info, and objects/pack.
        /// If the directories already exist, no exception is thrown.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            Directory.CreateDirectory(_gitDir);
            Directory.CreateDirectory(_objectsDir);
            Directory.CreateDirectory(_infoDir);
            Directory.CreateDirectory(_packDir);
            Directory.CreateDirectory(_refsHeadsDir);

            // Write HEAD.json file
            HeadReference head = new() { Ref = "refs/heads/main" };

            string headJson = JsonSerializer.Serialize(head, jsonOptions);

            await File.WriteAllTextAsync(_headFilePath, headJson);

            Console.WriteLine("Initialized empty git repository.");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InitCommand"/> if the arguments are valid and no Git repository exists.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>
        /// An instance of <see cref="IGitCommand"/> if the command can be created; otherwise, <c>null</c>.
        /// </returns>
        public static IGitCommand? Create(string[] args, IGitContextProvider gitContext)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: git init");
                return null;
            }

            if(gitContext.TryGetRepositoryRoot(out string repoPath))
            {
                Console.WriteLine("A Git repository already exists in the current directory or any of its parent directories.");
                return null;
            }
            
            JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

            return new InitCommand(jsonOptions, repoPath);
        }
    }
}
