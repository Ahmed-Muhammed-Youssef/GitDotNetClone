using Core.Constants;
using Core.Index;
using Core.Objects;
using Core.Stores.Interfaces;

namespace Core.Stores
{
    public class TreeStore(IIndexStore indexStore, string rootPath) : ITreeStore
    {
        /// <summary>
        /// Builds the tree structure from the current index entries and saves all Git tree objects.
        /// Returns the root tree's hash.
        /// If the index is empty, returns an empty string.
        /// </summary>
        public string BuildTreeFromIndex()
        {

            indexStore.Load();
            IReadOnlyList<IndexEntry> indexEntries = indexStore.GetEntries();

            if(indexEntries.Count == 0)
            {
              
                return string.Empty;
            }

            // Start recursive tree build from the root
            List<TreeEntry> rootTreeEntries = BuildTreeRecursive(indexEntries, "");

            // Save root tree object and return its hash
            TreeGitObject rootTree = new(rootTreeEntries);
            ObjectStore.Save(rootTree, rootPath);
            return rootTree.GetHash();
        }

        // <summary>
        /// Recursively builds a Git tree from a list of index entries.
        /// </summary>
        private List<TreeEntry> BuildTreeRecursive(IEnumerable<IndexEntry> entries, string currentPath)
        {
            List<TreeEntry> treeEntries = [];
            Dictionary<string, List<IndexEntry>> subdirectoryGroups = [];

            foreach (var entry in entries)
            {
                if (!entry.FilePath.StartsWith(currentPath, StringComparison.Ordinal))
                    continue;

                string relativePath = entry.FilePath[currentPath.Length..].TrimStart(Path.DirectorySeparatorChar);

                if (string.IsNullOrEmpty(relativePath))
                    continue;

                int separatorIndex = relativePath.IndexOf(Path.DirectorySeparatorChar);

                if (separatorIndex == -1)
                {
                    // It's a file in the current directory (a leaf node)
                    var fileName = relativePath;
                    treeEntries.Add(new TreeEntry(GitFileModes.RegularFile, fileName, entry.BlobHash));
                }
                else
                {
                    // It's inside a subdirectory
                    var dirName = relativePath[..separatorIndex];
                    if (!subdirectoryGroups.TryGetValue(dirName, out List<IndexEntry>? list))
                    {
                        list = [];
                        subdirectoryGroups[dirName] = list;
                    }
                    list.Add(entry);
                }
            }

            foreach (KeyValuePair<string, List<IndexEntry>> kvp in subdirectoryGroups)
            {
                string dirName = kvp.Key;
                List<IndexEntry> subEntries = kvp.Value;

                string newPath = string.IsNullOrEmpty(currentPath)
                    ? dirName + Path.DirectorySeparatorChar
                    : currentPath + dirName + Path.DirectorySeparatorChar;

                List<TreeEntry> subtreeEntries = BuildTreeRecursive(subEntries, newPath);
                TreeGitObject subtreeObject = new(subtreeEntries);
                ObjectStore.Save(subtreeObject, rootPath);
                treeEntries.Add(new TreeEntry(GitFileModes.Tree, dirName, subtreeObject.GetHash()));
            }

            treeEntries.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            return treeEntries;
        }
    }
}
