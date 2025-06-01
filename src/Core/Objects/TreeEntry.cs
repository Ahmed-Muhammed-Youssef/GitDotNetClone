namespace Core.Objects
{
    /// <summary>
    /// Represents a single entry in a Git tree object.
    /// </summary>
    /// <param name="Mode">The file mode (e.g., "100644" for blobs, "40000" for trees).</param>
    /// <param name="Name">The file or directory name.</param>
    /// <param name="Hash">The SHA-1 or SHA-256 hash of the blob or subtree object.</param>
    public record TreeEntry(string Mode, string Name, string Hash);
}
