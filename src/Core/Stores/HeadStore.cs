using Core.Objects;
using System.Text.Json;

namespace Core.Stores
{
    /// <summary>
    /// Provides utility methods for reading and updating references related to HEAD,
    /// branches, commits, and the associated tree snapshots within a Git repository.
    /// </summary>
    public static class HeadStore
    {
        /// <summary>
        /// Reads and deserializes the HEAD reference from the .git/HEAD file.
        /// </summary>
        /// <param name="root">The root path of the Git repository.</param>
        /// <param name="jsonOptions">The JSON serializer options to use for deserialization.</param>
        /// <returns>
        /// The <see cref="HeadReference"/> object representing the current HEAD, 
        /// or null if the HEAD file does not exist or cannot be read.
        /// </returns>
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

        /// <summary>
        /// Reads and deserializes the branch reference file pointed to by the HEAD reference.
        /// </summary>
        /// <param name="head">The HEAD reference object containing the branch ref path.</param>
        /// <param name="root">The root path of the Git repository.</param>
        /// <param name="jsonOptions">The JSON serializer options to use for deserialization.</param>
        /// <returns>
        /// The <see cref="BranchRef"/> object representing the commit reference of the branch,
        /// or null if the branch reference file does not exist or cannot be read.
        /// </returns>
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

        /// <summary>
        /// Updates (writes) the commit hash of a branch reference file.
        /// </summary>
        /// <param name="branchRefPath">The full path to the branch reference file to update.</param>
        /// <param name="commitHash">The new commit hash to write to the branch reference.</param>
        /// <param name="jsonOptions">The JSON serializer options to use for serialization.</param>
        public static void UpdateBranchReference(string branchRefPath, string commitHash, JsonSerializerOptions jsonOptions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(branchRefPath)!);
            byte[] branchBytes = JsonSerializer.SerializeToUtf8Bytes(new BranchRef() { Commit = commitHash }, jsonOptions);
            File.WriteAllBytes(branchRefPath, branchBytes);
        }

        /// <summary>
        /// Loads a snapshot of the current HEAD commit's tree as a dictionary mapping file paths to blob hashes.
        /// </summary>
        /// <param name="root">The root path of the Git repository.</param>
        /// <param name="jsonOptions">The JSON serializer options to use for deserialization.</param>
        /// <returns>
        /// A dictionary where keys are file paths (relative to the repository root)
        /// and values are the corresponding blob hashes in the HEAD commit's tree.
        /// Returns an empty dictionary if HEAD or the commit tree cannot be loaded.
        /// </returns>
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
