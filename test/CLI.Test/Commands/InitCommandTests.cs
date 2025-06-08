using CLI.Commands;
using CLI.Test.Fakes;
using Core.Objects;
using Core.Services;
using System.Text.Json;

namespace CLI.Test.Commands
{
    public class InitCommandTests : IAsyncLifetime
    {
        private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_tempRoot);
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, true);
            
            await Task.CompletedTask;
        }

        [Fact]
        public async Task ExecuteAsync_CreatesExpectedGitDirectories()
        {
            // Arrange
            var command = new InitCommand(_jsonOptions, _tempRoot);

            // Act
            await command.ExecuteAsync();

            // Assert
            Assert.True(Directory.Exists(Path.Combine(_tempRoot, ".git")));
            Assert.True(Directory.Exists(Path.Combine(_tempRoot, ".git/objects")));
            Assert.True(Directory.Exists(Path.Combine(_tempRoot, ".git/objects/info")));
            Assert.True(Directory.Exists(Path.Combine(_tempRoot, ".git/objects/pack")));
            Assert.True(Directory.Exists(Path.Combine(_tempRoot, ".git/refs/heads")));
        }

        [Fact]
        public async Task ExecuteAsync_CreatesHEADFileWithDefaultReference()
        {
            // Arrange
            var command = new InitCommand(_jsonOptions, _tempRoot);

            // Act
            await command.ExecuteAsync();

            // Assert
            string headPath = Path.Combine(_tempRoot, ".git", "HEAD");
            Assert.True(File.Exists(headPath));

            string json = await File.ReadAllTextAsync(headPath);
            HeadReference? head = JsonSerializer.Deserialize<HeadReference>(json);

            Assert.NotNull(head);
            Assert.Equal("refs/heads/main", head!.Ref);
        }

        [Fact]
        public void Create_ReturnsNull_IfArgsAreTooMany()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, true);

            // Act
            var command = InitCommand.Create(["extra", "arg"], _gitContextProvider);

            // Assert
            Assert.Null(command);
        }

        [Fact]
        public void Create_ReturnsNull_IfGitDirAlreadyExists()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, true);
            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git"));

            // Act
            var command = InitCommand.Create([], _gitContextProvider);

            // Assert
            Assert.Null(command);
        }

        [Fact]
        public void Create_ReturnsCommand_WithValidArgs()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, false);

            // Act
            var command = InitCommand.Create([], _gitContextProvider);

            // Assert
            Assert.NotNull(command);
            Assert.IsType<InitCommand>(command);
        }
    }
}
