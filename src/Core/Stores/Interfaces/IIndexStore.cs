using Core.Index;

namespace Core.Stores.Interfaces
{
    public interface IIndexStore
    {
        /// <summary>
        /// Gets a read-only list of the current entries in the index.
        /// </summary>
        public IReadOnlyList<IndexEntry> GetEntries();

        /// <summary>
        /// Loads the index entries from the .git/index file.
        /// If the file does not exist, the index remains empty.
        /// </summary>
        public void Load();

        /// <summary>
        /// Saves the current index entries to the .git/index file in JSON format.
        /// </summary>
        public void Save();

        /// <summary>
        /// Adds a new entry or updates an existing one if the file path already exists in the index.
        /// </summary>
        /// <param name="entry">The index entry to add or update.</param>
        public void AddOrUpdate(IndexEntry entry);
    }
}
