using Core.Objects;
using System.Text.Json;

namespace Core.Stores
{
    public static class HeadStore
    {
        public static HeadReference? GetHeadReference(string root, JsonSerializerOptions jsonOptions)
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

        public static BranchRef? GetBranchReference(HeadReference head, string root, JsonSerializerOptions jsonOptions)
        {
            string branchRefPath = Path.Combine(root, ".git", head.Ref.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(branchRefPath))
            {
                return null;
            }
            byte[] branchRefBytes = File.ReadAllBytes(branchRefPath);
            return JsonSerializer.Deserialize<BranchRef>(branchRefBytes, jsonOptions);
        }

        public static void UpdateBranchReference(string branchRefPath, string commitHash, JsonSerializerOptions jsonOptions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(branchRefPath)!);
            byte[] branchBytes = JsonSerializer.SerializeToUtf8Bytes(new BranchRef() { Commit = commitHash }, jsonOptions);
            File.WriteAllBytes(branchRefPath, branchBytes);
        }
        public static Dictionary<string, string> LoadHeadTreeSnapshot(string root, JsonSerializerOptions jsonOptions)
        {
            Dictionary<string, string> result = [];

            HeadReference? headRef = HeadStore.GetHeadReference(root, jsonOptions);
            if (headRef == null)
                return result;

            BranchRef? branch = HeadStore.GetBranchReference(headRef, root, jsonOptions);
            if (branch == null)
                return result;

            string commitHash = branch.Commit;
            string commitPath = Path.Combine(root, ".git", "objects", commitHash);
            if (!File.Exists(commitPath))
                return result;

            byte[] commitBytes = File.ReadAllBytes(commitPath);
            CommitGitObject commit = JsonSerializer.Deserialize<CommitGitObject>(commitBytes, jsonOptions)!;

            string treePath = Path.Combine(root, ".git", "objects", commit.TreeHash);
            if (!File.Exists(treePath))
                return result;

            byte[] treeBytes = File.ReadAllBytes(treePath);
            TreeGitObject tree = JsonSerializer.Deserialize<TreeGitObject>(treeBytes, jsonOptions)!;

            foreach (var entry in tree.TreeEntries)
                result[entry.Name] = entry.Hash;

            return result;
        }
    }
}
