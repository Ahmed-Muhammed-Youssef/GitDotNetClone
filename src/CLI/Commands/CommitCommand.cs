using Core.Objects;
using Core.Services;
using Core.Stores;
using Core.Stores.Interfaces;
using System.Text.Json;

namespace CLI.Commands
{
    public class CommitCommand(ITreeStore treeStore, string[] args, string root, JsonSerializerOptions jsonOptions) : IGitCommand
    {
        public static string Name => "commit";
        private readonly string[] _args = args;
        private readonly string _root = root;
        public Task ExecuteAsync()
        {
            string message = _args[0];
            string treeHash = treeStore.BuildTreeFromIndex();

            if(string.IsNullOrEmpty(treeHash))
            {
                Console.WriteLine("On branch main");
                Console.WriteLine("Nothing to commit, working tree clean");

                return Task.CompletedTask;
            }
         
            // Get the current branch reference
            HeadReference? head = GetHeadReference(_root, jsonOptions);

            if (head == null || string.IsNullOrWhiteSpace(head.Ref))
            {
                Console.WriteLine("Error: HEAD reference is invalid.");
                return Task.CompletedTask;
            }

            string branchRefPath = Path.Combine(_root, ".git", head.Ref.Replace('/', Path.DirectorySeparatorChar));

            BranchRef? branchRef = GetBranchReference(head, _root, jsonOptions);

            string? parentHash = branchRef?.Commit;

            // If a previous commit exists, compare trees
            if (!string.IsNullOrEmpty(parentHash))
            {
                CommitGitObject? parentCommit = ObjectStore.Load<CommitGitObject>(parentHash, _root, jsonOptions);
                if (parentCommit != null && parentCommit.TreeHash == treeHash)
                {
                    Console.WriteLine("On branch main");
                    Console.WriteLine("Nothing to commit, working tree clean");
                    return Task.CompletedTask;
                }
            }

            // Save the commit object
            CommitGitObject commitObject = new(treeHash, parentHash, "Author", "Author", message);

            ObjectStore.Save(commitObject, _root);

            var commitHash = commitObject.GetHash();

            // Update the branch reference to point to the new commit
            UpdateBranchReference(branchRefPath, commitHash, jsonOptions);

            Console.WriteLine($"[main {commitHash[..7]}] {message}");

            return Task.CompletedTask;
        }
        public static IGitCommand? Create(string[] args, IGitContextProvider gitContextProvider)
        {
            if (args.Length == 0)
            {
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

            TreeStore treeStore = new(indexStore, root);

            return new CommitCommand(treeStore, args, root, jsonOptions);
        }

        private static HeadReference? GetHeadReference(string root, JsonSerializerOptions jsonOptions)
        {
            string headFilePath = Path.Combine(root, ".git", "HEAD");
            if (!File.Exists(headFilePath))
            {
                return null;
            }
            string headJson = File.ReadAllText(headFilePath);
            HeadReference? head = JsonSerializer.Deserialize<HeadReference>(headJson, jsonOptions);
            return head;
        }

        private static BranchRef? GetBranchReference(HeadReference head, string root, JsonSerializerOptions jsonOptions)
        {
            string branchRefPath = Path.Combine(root, ".git", head.Ref.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(branchRefPath))
            {
                return null;
            }
            byte[] branchRefBytes = File.ReadAllBytes(branchRefPath);
            return JsonSerializer.Deserialize<BranchRef>(branchRefBytes, jsonOptions);
        }

        private static void UpdateBranchReference(string branchRefPath, string commitHash, JsonSerializerOptions jsonOptions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(branchRefPath)!);
            byte[] branchBytes = JsonSerializer.SerializeToUtf8Bytes(new BranchRef() { Commit = commitHash }, jsonOptions);
            File.WriteAllBytes(branchRefPath, branchBytes);
        }
    }
}
