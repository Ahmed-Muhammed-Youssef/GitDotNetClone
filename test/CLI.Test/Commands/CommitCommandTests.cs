using CLI.Commands;
using Core.Objects;
using Core.Stores;
using System.Text.Json;

namespace CLI.Test.Commands;

public class CommitCommandTests : IDisposable
{
    private readonly string _root;
    private readonly JsonSerializerOptions _jsonOptions = new () { WriteIndented = true };

    public CommitCommandTests()
    {
        _root = Path.Combine("tmp", "test_commit_command_kvxs19w3");
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(Path.Combine(_root, ".git"));
    }

    [Fact]
    public async Task Commit_WithNewFiles_ShouldCreateCommitObject()
    {
        // Arrange
        string filePath1 = Path.Combine(_root, "test1.txt");
        await File.WriteAllTextAsync(filePath1, "Hello world1");

        string filePath2 = Path.Combine(_root, "test2.txt");
        await File.WriteAllTextAsync(filePath2, "Hello world2");

        InitCommand initCommit = new(_jsonOptions, _root);
        await initCommit.ExecuteAsync();

        IndexStore indexStore = new(_root, _jsonOptions);
        indexStore.AddFile(filePath1);
        indexStore.AddFile(filePath2);
        indexStore.Save();

        TreeStore treeStore = new(indexStore, _root);
        CommitCommand commitCommand = new(treeStore, ["Initial commit"], _root, _jsonOptions);

        // Act
        await commitCommand.ExecuteAsync();

        // Assert
        string branchRefPath = Path.Combine(_root, ".git", "refs", "heads", "main");
        Assert.True(File.Exists(branchRefPath), "Branch ref should exist after commit");

        byte[] branchBytes = File.ReadAllBytes(branchRefPath);
        BranchRef? branchRef = JsonSerializer.Deserialize<BranchRef>(branchBytes, _jsonOptions);

        Assert.NotNull(branchRef);

        string objectPath = Path.Combine(_root, ".git", "objects", branchRef.Commit[..2], branchRef.Commit[2..]);

        Assert.True(File.Exists(objectPath), "Commit object should be stored in object database");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }
}
