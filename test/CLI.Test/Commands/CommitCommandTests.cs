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

    [Fact]
    public async Task Commit_Twice_ShouldUpdateBranchHead()
    {
        // Arrange
        InitCommand init = new(_jsonOptions, _root);
        await init.ExecuteAsync();

        string filePath = Path.Combine(_root, "file.txt");
        await File.WriteAllTextAsync(filePath, "First version");

        IndexStore indexStore = new(_root, _jsonOptions);
        indexStore.AddFile(filePath);
        indexStore.Save();

        TreeStore treeStore = new(indexStore, _root);
        CommitCommand commit1 = new(treeStore, ["First commit"], _root, _jsonOptions);
        await commit1.ExecuteAsync();

        string branchRefPath = Path.Combine(_root, ".git", "refs", "heads", "main");
        string? firstCommit = JsonSerializer.Deserialize<BranchRef>(File.ReadAllBytes(branchRefPath), _jsonOptions)?.Commit;

        await File.WriteAllTextAsync(filePath, "Second version");

        indexStore.AddFile(filePath);
        indexStore.Save();

        treeStore = new(indexStore, _root);
        CommitCommand commit2 = new(treeStore, ["Second commit"], _root, _jsonOptions);
        await commit2.ExecuteAsync();

        string? secondCommit = JsonSerializer.Deserialize<BranchRef>(File.ReadAllBytes(branchRefPath), _jsonOptions)?.Commit;

        // Assert
        Assert.NotEqual(firstCommit, secondCommit);
    }

    [Fact]
    public async Task Commit_WithUnchangedTree_ShouldSkipCommit()
    {
        // Arrange
        InitCommand init = new(_jsonOptions, _root);
        await init.ExecuteAsync();

        string filePath = Path.Combine(_root, "file.txt");
        await File.WriteAllTextAsync(filePath, "Same content");

        IndexStore indexStore = new(_root, _jsonOptions);
        indexStore.AddFile(filePath);
        indexStore.Save();

        TreeStore treeStore = new(indexStore, _root);
        CommitCommand commit1 = new(treeStore, ["Initial commit"], _root, _jsonOptions);
        await commit1.ExecuteAsync();

        var branchRefPath = Path.Combine(_root, ".git", "refs", "heads", "main");
        var commitHash = JsonSerializer.Deserialize<BranchRef>(File.ReadAllBytes(branchRefPath), _jsonOptions)?.Commit;

        // Try committing again without changes
        CommitCommand commit2 = new(treeStore, ["No changes"], _root, _jsonOptions);
        await commit2.ExecuteAsync();

        var newCommitHash = JsonSerializer.Deserialize<BranchRef>(File.ReadAllBytes(branchRefPath), _jsonOptions)?.Commit;

        // Assert
        Assert.Equal(commitHash, newCommitHash);
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
