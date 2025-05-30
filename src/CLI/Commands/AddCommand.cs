using CLI.Services;
using Core.Stores;
using System.Text.Json;

namespace CLI.Commands
{
    public class AddCommand(IndexStore indexStore, string[] args) : IGitCommand
    {
        private readonly IndexStore _indexStore = indexStore;
        private readonly string[] args = args;

        public static string Name => "add";

        /// <summary>
        /// Handles the execution of the 'git add' command.
        /// This method supports adding a single file to the index by computing its hash, storing the blob,
        /// and updating the index accordingly.
        /// </summary>
        /// <param name="args">An array of command-line arguments. Expects one argument: the path to the file.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ExecuteAsync()
        {
            if (args[0] == ".")
            {
                _indexStore.AddDirectory(Directory.GetCurrentDirectory());
            }
            else
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), args[0]);

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

        public static IGitCommand? Create(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: git add <file>");
                return null;
            }

            GitContextProvider _gitContextProvider = new();

            if (_gitContextProvider.TryGetRepositoryRoot(out string root))
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories).");
                return null;
            }

            JsonSerializerOptions jsonOptions = new()
            {
                WriteIndented = true
            };

            IndexStore indexStore = new(root, jsonOptions);
            return new AddCommand(indexStore, args);
        }
    }
}
