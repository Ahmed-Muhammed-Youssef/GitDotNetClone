using Core.Index;
using Core.Services;
using Core.Stores;
using System.Security.Cryptography;
using System.Text.Json;

namespace CLI.Commands;

public class StatusCommand(IndexStore indexStore, string root, JsonSerializerOptions jsonSerializerOptions) : IGitCommand
{
    public static string Name => "status";
    public Task ExecuteAsync()
    {
        indexStore.Load();

        Dictionary<string, IndexEntry> indexEntries = indexStore.GetEntries().ToDictionary(e => e.FilePath);

        List<string> workingFiles = Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(p => !p.Contains(".git"))
            .ToList();

        Console.WriteLine("On branch main\n");

        Dictionary<string, string> headSnapshot = HeadStore.LoadHeadTreeSnapshot(root, jsonSerializerOptions);

        List<string> stagedNew = [];
        List<string> stagedModified = [];
        List<string> stagedDeleted = [];
        List<string> notStaged = [];
        List<string> untracked = [];

        foreach (var filePath in workingFiles)
        {
            string relativePath = Path.GetRelativePath(root, filePath).Replace("\\", "/");

            if (!indexEntries.TryGetValue(relativePath, out var indexEntry))
            {
                untracked.Add(relativePath);
                continue;
            }

            byte[] diskContent = File.ReadAllBytes(filePath);
            string diskHash = Convert.ToHexStringLower(SHA256.HashData(diskContent));

            if (diskHash != indexEntry.BlobHash)
            {
                notStaged.Add(relativePath); // working tree != index
            }
            else
            {
                if (!headSnapshot.TryGetValue(relativePath, out var headHash))
                {
                    stagedNew.Add(relativePath); // file is new (not in HEAD)
                }
                else if (headHash != indexEntry.BlobHash)
                {
                    stagedModified.Add(relativePath); // modified and staged
                }
                // else: same in HEAD and index => already committed
            }
        }

        foreach (var entry in indexEntries.Values)
        {
            string fullPath = Path.Combine(root, entry.FilePath);
            if (!File.Exists(fullPath))
            {
                if (!headSnapshot.ContainsKey(entry.FilePath))
                {
                    notStaged.Add(entry.FilePath + " (deleted)");
                }
                else
                {
                    stagedDeleted.Add(entry.FilePath);
                }
            }
        }

        if (stagedNew.Count > 0 || stagedModified.Count > 0 || stagedDeleted.Count > 0)
        {
            Console.WriteLine("Changes to be committed:");
            Console.WriteLine("  (use \"git reset HEAD <file>...\" to unstage)\n");

            foreach (var path in stagedNew)
                Console.WriteLine($"\tnew file:   {path}");

            foreach (var path in stagedModified)
                Console.WriteLine($"\tmodified:   {path}");

            foreach (var path in stagedDeleted)
                Console.WriteLine($"\tdeleted:    {path}");

            Console.WriteLine();
        }

        if (notStaged.Count > 0)
        {
            Console.WriteLine("Changes not staged for commit:");
            Console.WriteLine("  (use \"git add <file>...\" to update what will be committed)\n");
            foreach (var path in notStaged)
                Console.WriteLine($"\tmodified:   {path}");
            Console.WriteLine();
        }

        if (untracked.Count > 0)
        {
            Console.WriteLine("Untracked files:");
            Console.WriteLine("  (use \"git add <file>...\" to include in what will be committed)\n");
            foreach (var path in untracked)
                Console.WriteLine($"\t{path}");
            Console.WriteLine();
        }

        if (stagedNew.Count == 0 && stagedModified.Count == 0 && stagedDeleted.Count == 0
            && notStaged.Count == 0 && untracked.Count == 0)
        {
            Console.WriteLine("Nothing to commit, working tree clean");
        }

        return Task.CompletedTask;
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
        return new StatusCommand(indexStore, root, options);
    }
}
