using Core.Index;
using System.Text.Json;

namespace Core.Stores
{
    /// <summary>
    /// Manages the Git index file (.git/index), which acts as the staging area
    /// in a minimal Git implementation.
    /// </summary>
    public class IndexStore(string root)
    {
        private readonly string _indexFilePath = Path.Combine(root, ".git", "index");
        private List<IndexEntry> _entries = [];

        /// <summary>
        /// Gets a read-only list of the current entries in the index.
        /// </summary>
        public IReadOnlyList<IndexEntry> Entries => _entries;

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
        public void AddOrUpdate(IndexEntry entry)
        {
            var existing = _entries.FirstOrDefault(e => e.FilePath == entry.FilePath);
            if (existing != null)
                _entries.Remove(existing);

            _entries.Add(entry);
        }
    }
}
