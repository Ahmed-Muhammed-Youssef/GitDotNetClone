using Core.Services;
using Core.Stores;
using System.Text.Json;

namespace CLI.Commands
{
    public class AddCommand(IndexStore indexStore, string[] args, IGitContextProvider gitContextProvider) : IGitCommand
    {
        private readonly IndexStore _indexStore = indexStore;
        private readonly string[] args = args;

        public static string Name => "add";

        /// <summary>
        /// Handles the execution of the 'git add' command.
        /// This method supports adding a single file or multiple files to the index by computing its/their hash, storing the blob,
        /// and updating the index accordingly.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ExecuteAsync()
        {
            if (args[0] == ".")
            {
                _indexStore.AddDirectory(gitContextProvider.GetWorkingDirectory());
            }
            else
            {
                string absolutePath = Path.Combine(gitContextProvider.GetWorkingDirectory(), args[0]);

                if (Directory.Exists(absolutePath))
                {
                    _indexStore.AddDirectory(absolutePath);
                }
                else
                {
                    _indexStore.AddFile(absolutePath);
                }
            }

            _indexStore.Save();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates an instance of the <see cref="AddCommand"/> if the provided arguments are valid and the current directory is a Git repository.
        /// Validates the argument count, checks for a valid Git repository root, and initializes the <see cref="IndexStore"/> with appropriate JSON options.
        /// If the arguments are invalid or the repository root is not found, displays an error message and returns null.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the 'git add' command. Expects a single file or directory argument.</param>
        /// <returns>
        /// An instance of <see cref="IGitCommand"/> representing the add command, or null if the arguments are invalid or not in a Git repository.
        /// </returns>
        public static IGitCommand? Create(string[] args, IGitContextProvider gitContextProvider)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: git add <file>");
                return null;
            }

            if (!gitContextProvider.TryGetRepositoryRoot(out string root))
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories).");
                return null;
            }

            JsonSerializerOptions jsonOptions = new()
            {
                WriteIndented = true
            };

            IndexStore indexStore = new(root, jsonOptions);
            return new AddCommand(indexStore, args, gitContextProvider);
        }
    }
}
