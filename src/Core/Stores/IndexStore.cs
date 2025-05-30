using Core.Index;
using Core.Objects;
using Core.Stores.Interfaces;
using CrossCuttingConcerns.Helpers;
using System.Text.Json;

namespace Core.Stores
{
    /// <summary>
    /// Manages the Git index file (.git/index), which acts as the staging area
    /// in a minimal Git implementation.
    /// </summary>
    public class IndexStore(string root) : IIndexStore
    {
        private readonly string _indexFilePath = Path.Combine(root, ".git", "index");
        private List<IndexEntry> _entries = [];

        /// <summary>
        /// Gets a read-only list of the current entries in the index.
        /// </summary>
        public IReadOnlyList<IndexEntry> GetEntries() =>  _entries;

        /// <summary>
        /// Adds a file to the index by creating a blob object from the file content,
        /// computing its relative normalized path, and storing an index entry.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the file to be added.</param>
        /// <remarks>
        /// - The file at <paramref name="absolutePath"/> must exist and be accessible.
        /// - If the file does not exist or is not accessible, an exception will be thrown.
        /// - This method assumes the path is valid and belongs to the Git working directory.
        /// - It is the caller's responsibility to validate the file existence before calling this method.
        /// </remarks>
        public void AddFile(string absolutePath)
        {
            BlobGitObject blob = new(File.ReadAllBytes(absolutePath));

            var realtivePath = Path.GetRelativePath(root, absolutePath);

            realtivePath = PathHelper.Normalize(realtivePath);

            IndexEntry entry = new(realtivePath, blob.GetHash(), blob.Content.LongLength);

            AddOrUpdate(entry);
        }
        /// <summary>
        /// Loads the index entries from the .git/index file.
        /// If the file does not exist, the index remains empty.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(_indexFilePath))
                return;

            var json = File.ReadAllText(_indexFilePath);
            _entries = JsonSerializer.Deserialize<List<IndexEntry>>(json) ?? [];
        }

        /// <summary>
        /// Saves the current index entries to the .git/index file in JSON format.
        /// </summary>
        public void Save()
        {
            string json = JsonSerializer.Serialize(_entries, options: new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_indexFilePath, json);
        }

        /// <summary>
        /// Adds a new entry or updates an existing one if the file path already exists in the index.
        /// </summary>
        /// <param name="entry">The index entry to add or update.</param>
        private void AddOrUpdate(IndexEntry entry)
        {
            var existing = _entries.FirstOrDefault(e => e.FilePath == entry.FilePath);
            if (existing != null)
                _entries.Remove(existing);

            _entries.Add(entry);
        }
    }
}
