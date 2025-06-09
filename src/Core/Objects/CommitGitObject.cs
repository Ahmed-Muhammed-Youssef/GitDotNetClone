namespace Core.Objects
{
    public class CommitGitObject : GitObject
    {
    public string TreeHash { get; init; } = string.Empty;
    public string? ParentHash { get; init; }
    public string Author { get; init; } = string.Empty;
    public string Committer { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
        public override string Type => "commit";

        private readonly byte[] _content = [];
        public override byte[] GetContent() => _content;

        /// <summary>
        /// Initializes a new commit object.
        /// </summary>
        public CommitGitObject(string treeHash, string? parentHash, string author, string committer, string message)
        {
            TreeHash = treeHash;
            ParentHash = parentHash;
            Author = author;
            Committer = committer;
            Message = message;

            _content = SerializeToUtf8();
        }

        public CommitGitObject(byte[] content)
        {
            _content = content;

            CommitGitObject deserialized = Deserialize<CommitGitObject>();

            TreeHash = deserialized.TreeHash;
            ParentHash = deserialized.ParentHash;
            Author = deserialized.Author;
            Committer = deserialized.Committer;
            Message = deserialized.Message;
        }

    /// <summary>
    /// for deserialization purposes only.
    /// </summary>
    public CommitGitObject()
    {
        
    }
}
