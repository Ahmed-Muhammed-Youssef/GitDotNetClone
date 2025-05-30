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
        public void AddFile(string absolutePath);

        /// <summary>
        /// Loads the index entries from the .git/index file.
        /// If the file does not exist, the index remains empty.
        /// </summary>
        public void Load();

        /// <summary>
        /// Saves the current index entries to the .git/index file in JSON format.
        /// </summary>
        public void Save();
    }
}
