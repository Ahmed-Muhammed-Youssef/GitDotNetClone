using CLI.Commands;

namespace CLI.Test.Commands
{
    public class InitCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenCalled_CreatesGitDirectory()
        {
            // Arrange
            var command = new InitCommand();
            var repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(repoPath);

            try
            {
                Directory.SetCurrentDirectory(repoPath);

                // Act
                await command.ExecuteAsync(Array.Empty<string>());

                // Assert
                Assert.True(Directory.Exists(Path.Combine(repoPath, ".git")));
            }
            finally
            {
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                Directory.Delete(repoPath, recursive: true);
            }
        }
    }
}
