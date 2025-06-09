using CLI.Commands;
using Core.Constants;
using Core.Objects;
using Core.Stores;
using System.Text.Json;

namespace CLI.Test.Commands
{
    public class StatusCommandTests : IDisposable
    {
        private readonly string _root;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public StatusCommandTests()
        {
            _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }
        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);

            GC.SuppressFinalize(this);
        }


        [Fact]
        public async Task ExecuteAsync_ShouldPrintCorrectStatus()
        {
            // Arrange
            // Capture output
            var sw = new StringWriter();
            Console.SetOut(sw);

            File.WriteAllText(Path.Combine(_root, "tracked.txt"), "original content");
            File.WriteAllText(Path.Combine(_root, "modified.txt"), "new content");
            File.WriteAllText(Path.Combine(_root, "modifiedStaged.txt"), "new content");
            File.WriteAllText(Path.Combine(_root, "untracked.txt"), "I am new here");
            File.WriteAllText(Path.Combine(_root, "deleted.txt"), "will be deleted");

            InitCommand initCommit = new(_jsonOptions, _root);
            await initCommit.ExecuteAsync();

            // Simulate index
            IndexStore indexStore = new(_root, _jsonOptions);
            indexStore.AddFile(Path.Combine(_root, "modified.txt"));
            indexStore.AddFile(Path.Combine(_root, "modifiedStaged.txt"));
            indexStore.AddFile(Path.Combine(_root, "deleted.txt"));
           
            indexStore.Save();

            File.Delete(Path.Combine(_root, "deleted.txt"));
            
            File.WriteAllText(Path.Combine(_root, "modified.txt"), "modified");
            File.WriteAllText(Path.Combine(_root, "modifiedStaged.txt"), "modified but staged");

            TreeStore treeStore = new(indexStore, _root);
            CommitCommand commitCommand = new(treeStore, ["Initial commit"], _root, _jsonOptions);

            await commitCommand.ExecuteAsync();

            indexStore.AddFile(Path.Combine(_root, "tracked.txt"));
            indexStore.AddFile(Path.Combine(_root, "modifiedStaged.txt"));
            indexStore.Save();

            StatusCommand statusCommand = new(indexStore, _root, _jsonOptions);

            // Act
            await statusCommand.ExecuteAsync();

            // Assert
            string output = sw.ToString();

            Assert.Contains("untracked.txt", output);                // untracked
            Assert.Contains("deleted:    deleted.txt", output);      // staged deleted

            Assert.Contains("modified:   modified.txt", output);     // not staged
            Assert.Contains("modified:   modifiedStaged.txt", output);     // staged
            Assert.Contains("new file:   tracked.txt", output);      // same in index but not in HEAD (if test logic simulates that)

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

    }
}
