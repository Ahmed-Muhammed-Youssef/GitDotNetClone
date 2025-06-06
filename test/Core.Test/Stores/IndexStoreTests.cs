using Core.Stores;
using CrossCuttingConcerns.Helpers;
using System.Text.Json;

namespace Core.Test.Stores
{
    public class IndexStoreTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IndexStore _indexStore;

        public IndexStoreTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempRoot);

            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            _indexStore = new IndexStore(_tempRoot, _jsonOptions);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void AddFile_WhenFileExists_AddsEntryToIndex()
        {
            // Arrange
            string filePath = Path.Combine(_tempRoot, "test.txt");
            File.WriteAllText(filePath, "Hello Git!");

            // Act
            _indexStore.AddFile(filePath);
            var entries = _indexStore.GetEntries();

            // Assert
            Assert.Single(entries);
            Assert.Equal("test.txt", entries[0].FilePath); // normalized
            Assert.Equal("Hello Git!".Length, entries[0].Size);
        }

        [Fact]
        public void AddFile_WhenFileDoesNotExist_DoesNotThrowOrAdd()
        {
            // Act
            _indexStore.AddFile(Path.Combine(_tempRoot, "missing.txt"));
            var entries = _indexStore.GetEntries();

            // Assert
            Assert.Empty(entries);
        }

        [Fact]
        public void AddDirectory_WhenCalled_AddsAllFilesRecursively()
        {
            // Arrange
            string subDir = Path.Combine(_tempRoot, "sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(_tempRoot, "root.txt"), "A");
            File.WriteAllText(Path.Combine(subDir, "sub.txt"), "B");

            // Act
            _indexStore.AddDirectory(_tempRoot);

            // Assert
            var entries = _indexStore.GetEntries();
            Assert.Equal(2, entries.Count);
            Assert.Contains(entries, e => e.FilePath == "root.txt");
            Assert.Contains(entries, e => e.FilePath == PathHelper.Normalize("sub/sub.txt"));
        }

        [Fact]
        public void SaveAndLoad_WhenCalled_PersistsEntriesCorrectly()
        {
            // Arrange
            string filePath = Path.Combine(_tempRoot, "file.txt");
            File.WriteAllText(filePath, "Content");
            _indexStore.AddFile(filePath);

            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git")); // Ensure .git directory exists

            _indexStore.Save();

            // Create new store to test loading
            var newStore = new IndexStore(_tempRoot, _jsonOptions);

            // Act
            newStore.Load();
            var entries = newStore.GetEntries();

            // Assert
            Assert.Single(entries);
            Assert.Equal("file.txt", entries[0].FilePath);
        }

        [Fact]
        public void Load_WhenIndexFileMissing_DoesNothing()
        {
            // Act
            _indexStore.Load();

            // Assert
            Assert.Empty(_indexStore.GetEntries());
        }

        [Fact]
        public void GetUntrackedFiles_WhenCalled_ReturnsCorrectUntrackedFiles()
        {
            // Arrange
            var trackedPath = Path.Combine(_tempRoot, "tracked.txt");
            var untrackedPath = Path.Combine(_tempRoot, "untracked.txt");

            File.WriteAllText(trackedPath, "Tracked");
            File.WriteAllText(untrackedPath, "Untracked");

            _indexStore.AddFile(trackedPath);

            // Act
            var result = _indexStore.GetUntrackedFiles();

            // Assert
            Assert.Single(result);
            Assert.Equal("untracked.txt", result[0]);
        }

        [Fact]
        public void GetUntrackedFiles_WhenGitDirExists_ExcludesGitFiles()
        {
            // Arrange
            var gitFilePath = Path.Combine(_tempRoot, ".git", "config");
            Directory.CreateDirectory(Path.GetDirectoryName(gitFilePath)!);
            File.WriteAllText(gitFilePath, "ignore me");

            // Act
            var result = _indexStore.GetUntrackedFiles();

            // Assert
            Assert.DoesNotContain(result, f => f.Contains(".git"));
        }
    }
}
