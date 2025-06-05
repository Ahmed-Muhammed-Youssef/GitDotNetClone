using Core.Objects;
using Core.Stores;

namespace Core.Test.Stores
{
    public class ObjectStoreTests : IDisposable
    {
        private readonly string _tempRoot;

        public ObjectStoreTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }

        private static BlobGitObject CreateBlob(string content)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return new BlobGitObject(bytes);
        }

        [Fact]
        public void Save_WhenCalled_CreatesGitDirectory()
        {
            // Arrange
            var blob = CreateBlob("Hello world");
            var hash = blob.GetHash();

            // Act
            ObjectStore.Save(blob, _tempRoot);

            // Assert
            var expectedDir = Path.Combine(_tempRoot, ".git", "objects", hash[..2]);
            Assert.True(Directory.Exists(expectedDir));
        }

        [Fact]
        public void Save_WhenCalled_CreatesObjectFile()
        {
            // Arrange
            var blob = CreateBlob("Hello world");
            var hash = blob.GetHash();

            // Act
            ObjectStore.Save(blob, _tempRoot);

            // Assert
            var expectedFile = Path.Combine(_tempRoot, ".git", "objects", hash[..2], hash[2..]);
            Assert.True(File.Exists(expectedFile));
        }

        [Fact]
        public void Load_WhenCalled_ReturnsCorrectData()
        {
            // Arrange
            var blob = CreateBlob("Test blob");
            var hash = blob.GetHash();
            var content = blob.GetContent();

            var dir = Path.Combine(_tempRoot, ".git", "objects", hash[..2]);
            var file = Path.Combine(dir, hash[2..]);
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(file, content);

            // Act
            var result = ObjectStore.Load(hash, _tempRoot);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public void Load_WhenFileMissing_ThrowsFileNotFoundException()
        {
            // Arrange
            var fakeHash = new string('a', 64); // SHA-256 length

            // Act & Assert
            var ex = Record.Exception(() => ObjectStore.Load(fakeHash, _tempRoot));
            Assert.True(
                ex is FileNotFoundException || ex is DirectoryNotFoundException,
                $"Expected FileNotFoundException or DirectoryNotFoundException, but got {ex?.GetType().Name}"
            );
        }

        [Fact]
        public void Save_WhenHashCollisionWithDifferentContent_ThrowsInvalidOperationException()
        {
            // Arrange
            var contentA = "Content A";
            var contentB = "Content B";

            var blobA = CreateBlob(contentA);
            var blobB = CreateBlob(contentB);

            var hash = blobB.GetHash();
            var dir = Path.Combine(_tempRoot, ".git", "objects", hash[..2]);
            var file = Path.Combine(dir, hash[2..]);
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(file, blobA.GetContent());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ObjectStore.Save(blobB, _tempRoot));
        }

        [Fact]
        public void Save_WhenHashExistsWithSameContent_DoesNotThrow()
        {
            // Arrange
            var blob = CreateBlob("Same content");
            var hash = blob.GetHash();

            var dir = Path.Combine(_tempRoot, ".git", "objects", hash[..2]);
            var file = Path.Combine(dir, hash[2..]);
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(file, blob.GetContent());

            // Act & Assert
            var ex = Record.Exception(() => ObjectStore.Save(blob, _tempRoot));
            Assert.Null(ex);
        }
    }
}
