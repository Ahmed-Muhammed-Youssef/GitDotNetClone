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
    public class IndexStore(string root, JsonSerializerOptions jsonSerializerOptions) : IIndexStore
    {
        private readonly string _indexFilePath = Path.Combine(root, ".git", "index");
        private List<IndexEntry> _entries = [];

        /// <summary>
        /// Gets a read-only list of the current entries in the index.
        /// Note: File paths are normalized
        /// </summary>
        public IReadOnlyList<IndexEntry> GetEntries() => _entries;

        /// <summary>
        /// Adds all files from the specified directory and its subdirectories to the Git index.
        /// Each file is processed as a blob and stored with its relative path.
        /// </summary>
        /// <param name="directoryPath">The absolute path to the directory to add files from.</param>
        /// <remarks>
        /// This method recursively enumerates all files inside the directory. Hidden files, system files,
        /// or files within the `.git` directory are not excluded by default — that should be handled externally
        /// or in a future enhancement.
        ///
        /// Throws exceptions if any file is unreadable or if there is an I/O error.
        /// </remarks>
        public void AddDirectory(string directoryPath)
        {
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                AddFile(file);
            }
        }

        /// <summary>
        /// Adds a file to the index by creating a blob object from the file content,
        /// computing its relative normalized path, and storing an index entry.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the file to be added.</param>
        public void AddFile(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                Console.WriteLine($"Warning: File not found, skipping: {absolutePath}");
                return;
            }
            BlobGitObject blob = new(File.ReadAllBytes(absolutePath));

            var realtivePath = Path.GetRelativePath(root, absolutePath);

            realtivePath = PathHelper.Normalize(realtivePath);

            IndexEntry entry = new(realtivePath, blob.GetHash(), blob.GetContent().LongLength);

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

            byte[] jsonBytes = File.ReadAllBytes(_indexFilePath);
            _entries = JsonSerializer.Deserialize<List<IndexEntry>>(jsonBytes) ?? [];
        }

        /// <summary>
        /// Saves the current index entries to the .git/index file in JSON format.
        /// </summary>
        public void Save()
        {
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(_entries, jsonSerializerOptions);
            File.WriteAllBytes(_indexFilePath, jsonBytes);
        }

        /// <summary>
        /// Adds a new entry or updates an existing one if the file path already exists in the index.
        /// </summary>
        /// <param name="entry">The index entry to add or update.</param>
        private void AddOrUpdate(IndexEntry entry)
        {
            var existing = _entries.FirstOrDefault(e => e.FilePath == entry.FilePath);
            if (existing != null)
            {
                if (existing.BlobHash == entry.BlobHash && existing.Size == entry.Size)
                {
                    // File hasn't changed, skip update
                    return;
                }
                _entries.Remove(existing);
            }

            _entries.Add(entry);
        }

        /// <summary>
        /// Gets untracked files paths, files that doesn't exist in the entries of the index.
        /// Note: file paths are normalized
        /// </summary>
        ///  
        public List<string> GetUntrackedFiles()
        {
            var trackedPaths = new HashSet<string>(GetEntries().Select(e => PathHelper.Denormalize(e.FilePath)));

            var allFiles = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => !path.StartsWith(Path.Combine(root, ".git")))
                .ToList();

            var untracked = new List<string>();

            foreach (var absolutePath in allFiles)
            {
                var relativePath = PathHelper.Normalize(Path.GetRelativePath(root, absolutePath));

                if (!trackedPaths.Contains(relativePath))
                {
                    untracked.Add(relativePath);
                }
            }

            return untracked;
        }
    }
}
