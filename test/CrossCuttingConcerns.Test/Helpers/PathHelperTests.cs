using CrossCuttingConcerns.Helpers;

namespace CrossCuttingConcerns.Test.Helpers
{
    public class PathHelperTests : IDisposable
    {
        // private readonly string _originalWorkingDir;
        private readonly string _tempRoot;

        public PathHelperTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            // _originalWorkingDir = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            // Directory.SetCurrentDirectory(_originalWorkingDir);
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
            GC.SuppressFinalize(this);
        }

        [Theory]
        [InlineData("folder\\subfolder\\file.txt", "folder/subfolder/file.txt")]
        [InlineData("folder\\file.txt", "folder/file.txt")]
        [InlineData("file.txt", "file.txt")]
        public void Normalize_ReplacesBackslashesWithSlashes(string input, string expected)
        {
            var normalized = PathHelper.Normalize(input);
            Assert.Equal(expected, normalized);
        }

        [Theory]
        [InlineData("folder/subfolder/file.txt")]
        [InlineData("folder/file.txt")]
        [InlineData("file.txt")]
        public void Denormalize_ReplacesSlashesWithOSSeparator(string input)
        {
            var expected = input.Replace('/', Path.DirectorySeparatorChar);
            var actual = PathHelper.Denormalize(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetRepositoryRoot_WhenGitDirectoryExists_ReturnsCorrectRoot()
        {
            // Arrange
            var gitRoot = Path.Combine(_tempRoot, "my-repo");
            Directory.CreateDirectory(gitRoot);
            Directory.CreateDirectory(Path.Combine(gitRoot, ".git"));

            var nested = Path.Combine(gitRoot, "nested", "deep");
            Directory.CreateDirectory(nested);

            // Act
            var result = PathHelper.GetRepositoryRoot(nested);

            // Assert
            Assert.Equal(gitRoot, result);
        }
    }
}
