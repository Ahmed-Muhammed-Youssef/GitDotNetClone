using CLI.Services;
using Core.Services;
using Core.Stores;
using System.Text.Json;

namespace CLI.Commands
{
    public class StatusCommand(IndexStore indexStore) : IGitCommand
    {
        public static string Name => "status";
        public Task ExecuteAsync()
        {
            indexStore.Load();

            if (indexStore.GetEntries().Count == 0)
            {
                Console.WriteLine("On branch master");
                Console.WriteLine("No changes added to commit (use \"git add\" to track)");
                return Task.CompletedTask;
            }
            else
            {
                Console.WriteLine("On branch master\n");
                Console.WriteLine("Changes to be committed:");
                Console.WriteLine("  (use \"git reset HEAD <file>...\" to unstage)");

                foreach (var entry in indexStore.GetEntries())
                {
                    Console.WriteLine($"\tnew file:   {entry.FilePath}");
                }
                return Task.CompletedTask;
            }
        }
        public static IGitCommand? Create(string[] args, IGitContextProvider gitContextProvider)
        {
            if (!gitContextProvider.TryGetRepositoryRoot(out string root))
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories).");
                return null;
            }

            if (args.Length > 0)
            {
                Console.WriteLine("Usage: git status");
                return null;
            }

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            IndexStore indexStore = new(root, options);
            return new StatusCommand(indexStore);
        }
    }
}
