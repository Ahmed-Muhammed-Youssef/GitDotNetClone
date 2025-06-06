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
            Assert.True(string.IsNullOrWhiteSpace(hash));
        }
    }
}
