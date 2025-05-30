using CLI.Services;
using Core.Services;
using Core.Stores;

namespace CLI.Commands
{
    public class AddCommand(IGitContextProvider gitContextProvider) : IGitCommand
    {
        private readonly IGitContextProvider _gitContextProvider = gitContextProvider;

        public string Name => "add";

        /// <summary>
        /// Handles the execution of the 'git add' command.
        /// This method supports adding a single file to the index by computing its hash, storing the blob,
        /// and updating the index accordingly.
        /// </summary>
        /// <param name="args">An array of command-line arguments. Expects one argument: the path to the file.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ExecuteAsync(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: git add <file>");
                return Task.CompletedTask;
            }

            if (_gitContextProvider.TryGetRepositoryRoot(out string root))
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories).");
                return Task.CompletedTask;
            }

            // TODO: Implement support for "git add ." to add all changes recursively
            if (args[0] == ".")
            {
                throw new NotImplementedException("Adding all files is not implemented yet.");
            }

            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), args[0]);

            // TODO: If the provided path is a directory, enumerate and add all contained files recursively
            if (Directory.Exists(absolutePath))
            {
                throw new NotImplementedException("Adding a directory is not implemented yet.");
            }

            IndexStore indexStore = new(root);

            indexStore.AddFile(absolutePath);

            indexStore.Save();

            return Task.CompletedTask;
        }
    }
}
