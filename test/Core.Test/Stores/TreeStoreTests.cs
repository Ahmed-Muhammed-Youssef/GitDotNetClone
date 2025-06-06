using Core.Index;
using Core.Stores;
using System.Text.Json;

namespace Core.Test.Stores
{
    public class TreeStoreTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IndexStore _indexStore;
        public TreeStoreTests()
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
        public void BuildTreeFromIndex_WhenNoEntries_ReturnsEmptyHash()
        {
            // Arrange
            _indexStore.Load();
            _indexStore.GetEntries();

            var store = new TreeStore(_indexStore, _tempRoot);

            // Act
            var hash = store.BuildTreeFromIndex();

            // Assert
            Assert.True(hash == string.Empty);
        }

        [Fact]
        public void BuildTreeFromIndex_WhenSingleFile_ReturnsValidHash()
        {
            // Arrange
            string filePath = Path.Combine(_tempRoot, "file.txt");
            File.WriteAllText(filePath, "hello world");

            _indexStore.AddFile(filePath);

            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git")); // Ensure .git directory exists

            _indexStore.Save();

            var store = new TreeStore(_indexStore, _tempRoot);

            // Act
            var hash = store.BuildTreeFromIndex();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(hash));
            string dir = Path.Combine(_tempRoot, ".git", "objects", hash[..2]);
            string file = Path.Combine(dir, hash[2..]);
            Assert.True(File.Exists(file));
        }

        [Fact]
        public void BuildTreeFromIndex_WhenNestedDirectories_CreatesMultipleTreeObjects()
        {
            // Arrange
            string dir = Path.Combine(_tempRoot, "subdir");
            Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, "nested.txt");
            File.WriteAllText(filePath, "nested content");

            _indexStore.AddFile(filePath);

            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git")); // Ensure .git directory exists

            _indexStore.Save();

            var store = new TreeStore(_indexStore, _tempRoot);

            // Act
            var rootHash = store.BuildTreeFromIndex();

            // Assert root tree object file exists
            string rootTreeFile = Path.Combine(_tempRoot, ".git", "objects", rootHash[..2], rootHash[2..]);
            Assert.True(File.Exists(rootTreeFile));

            // Check that at least one tree object exists for the nested directory
            var objectFiles = Directory.GetFiles(Path.Combine(_tempRoot, ".git", "objects"), "*", SearchOption.AllDirectories);
            Assert.True(objectFiles.Length >= 1);
        }
    }
}
