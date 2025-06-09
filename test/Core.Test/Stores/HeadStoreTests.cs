using Core.Constants;
using Core.Objects;
using Core.Stores;
using System.Text.Json;

namespace Core.Test.Stores;

public class HeadStoreTests : IDisposable
{
    private readonly string _testRoot;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public HeadStoreTests()
    {
        // Setup a temp directory as a fake git repo root
        _testRoot = Path.Combine(Path.GetTempPath(), "GitTestRepo_" + Guid.NewGuid());
        Directory.CreateDirectory(_testRoot);
        Directory.CreateDirectory(Path.Combine(_testRoot, ".git"));
    }

    public void Dispose()
    {
        // Cleanup test directory after each test run
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, recursive: true);

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GetHeadReference_ReturnsNull_WhenHeadFileMissing()
    {
        // Act
        var headRef = HeadStore.GetHeadReference(_testRoot, _jsonOptions);

        // Assert
        Assert.Null(headRef);
    }

    [Fact]
    public void GetHeadReference_ReturnsDeserializedHeadReference_WhenHeadFileExists()
    {
        // Arrange
        var head = new HeadReference { Ref = "refs/heads/main" };
        string headPath = Path.Combine(_testRoot, ".git", "HEAD");
        File.WriteAllText(headPath, JsonSerializer.Serialize(head, _jsonOptions));

        // Act
        var result = HeadStore.GetHeadReference(_testRoot, _jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(head.Ref, result.Ref);
    }

    [Fact]
    public void GetBranchReference_ReturnsNull_WhenBranchFileMissing()
    {
        // Arrange
        var head = new HeadReference { Ref = "refs/heads/main" };

        // Act
        var branchRef = HeadStore.GetBranchReference(head, _testRoot, _jsonOptions);

        // Assert
        Assert.Null(branchRef);
    }

    [Fact]
    public void GetBranchReference_ReturnsDeserializedBranchRef_WhenBranchFileExists()
    {
        // Arrange
        var head = new HeadReference { Ref = "refs/heads/main" };
        string branchPath = Path.Combine(_testRoot, ".git", "refs", "heads", "main");
        Directory.CreateDirectory(Path.GetDirectoryName(branchPath)!);

        var branch = new BranchRef { Commit = "123abc" };
        File.WriteAllBytes(branchPath, JsonSerializer.SerializeToUtf8Bytes(branch, _jsonOptions));

        // Act
        var result = HeadStore.GetBranchReference(head, _testRoot, _jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(branch.Commit, result.Commit);
    }

    [Fact]
    public void UpdateBranchReference_CreatesOrOverwritesBranchFile()
    {
        // Arrange
        string branchPath = Path.Combine(_testRoot, ".git", "refs", "heads", "feature");
        string commitHash = "deadbeef";

        // Act
        HeadStore.UpdateBranchReference(branchPath, commitHash, _jsonOptions);

        // Assert
        Assert.True(File.Exists(branchPath));
        var branchRef = JsonSerializer.Deserialize<BranchRef>(File.ReadAllBytes(branchPath), _jsonOptions);
        Assert.NotNull(branchRef);
        Assert.Equal(commitHash, branchRef.Commit);
    }

    [Fact]
    public void LoadHeadTreeSnapshot_ReturnsEmpty_WhenHeadAndBranchMissing()
    {
        // Act
        var snapshot = HeadStore.LoadHeadTreeSnapshot(_testRoot, _jsonOptions);

        // Assert
        Assert.Empty(snapshot);
    }

    [Fact]
    public void LoadHeadTreeSnapshot_ReturnsEmpty_WhenBranchMissing()
    {
        //Arrange
        var head = new HeadReference { Ref = "refs/heads/main" };
        File.WriteAllText(Path.Combine(_testRoot, ".git", "HEAD"), JsonSerializer.Serialize(head, _jsonOptions));

        // Act
        var snapshot = HeadStore.LoadHeadTreeSnapshot(_testRoot, _jsonOptions);

        // Assert
        Assert.Empty(snapshot);
    }

    [Fact]
    public void LoadHeadTreeSnapshot_ReturnsTreeEntries_WhenAllFilesExist()
    {
        // Arrange
        // Setup HEAD
        var head = new HeadReference { Ref = "refs/heads/main" };
        string headPath = Path.Combine(_testRoot, ".git", "HEAD");
        File.WriteAllText(headPath, JsonSerializer.Serialize(head, _jsonOptions));


        // Setup tree object with entries
        TreeGitObject tree = new([
                new TreeEntry(GitFileModes.RegularFile , "file1.txt", "hash1" ),
                new TreeEntry(GitFileModes.RegularFile, "file2.txt", "hash2")
            ]);

        ObjectStore.Save(tree, _testRoot);

        // Setup commit object
        CommitGitObject commit = new(tree.GetHash(), null, "Author", "Author", "First Commit");
        ObjectStore.Save(commit, _testRoot);

        // Setup branch ref
        string branchPath = Path.Combine(_testRoot, ".git", "refs", "heads", "main");
        HeadStore.UpdateBranchReference(branchPath, commit.GetHash(), _jsonOptions);

        // Act
        Dictionary<string, string> snapshot = HeadStore.LoadHeadTreeSnapshot(_testRoot, _jsonOptions);

        // Assert
        Assert.NotEmpty(snapshot);
        Assert.Equal(2, snapshot.Count);
        Assert.Equal("hash1", snapshot["file1.txt"]);
        Assert.Equal("hash2", snapshot["file2.txt"]);
    }
}
