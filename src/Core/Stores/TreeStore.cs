using Core.Index;
using Core.Objects;
using Core.Stores.Interfaces;

namespace Core.Stores
{
    public class TreeStore(IIndexStore indexStore, string repositoryRoot)
    {
        private readonly string _objectsDirectory = Path.Combine(repositoryRoot, ".git", "objects");

        public string BuildTreeFromIndex()
        {

            indexStore.Load();

            IReadOnlyList<IndexEntry> indexEnteries = indexStore.GetEntries();

            List<TreeEntry> root = BuildTreeRecursive(indexEnteries, "");

            throw new NotImplementedException();
        }

        private List<TreeEntry> BuildTreeRecursive(IEnumerable<IndexEntry> entries, string currentPath)
        {
            throw new NotImplementedException();
        }
        private void SaveGitObject(GitObject obj)
        {
            var hash = obj.GetHash();
            var path = Path.Combine(_objectsDirectory, hash);
            File.WriteAllBytes(path, obj.Serialize());
        }
    }
}
