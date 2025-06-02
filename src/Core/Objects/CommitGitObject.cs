using System.Text;

namespace Core.Objects
{
    public class CommitGitObject : GitObject
    {
        public string TreeHash { get; }
        public string? ParentHash { get; }
        public string Author { get; }
        public string Committer { get; }
        public string Message { get; }

        public override string Type => "commit";

        /// <summary>
        /// The serialized content of the commit, as per Git object format.
        /// </summary>
        public override byte[] Content => Encoding.UTF8.GetBytes(BuildCommitContent());

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
        }

        /// <summary>
        /// Serializes the commit object fields to Git's internal content format.
        /// </summary>
        private string BuildCommitContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"tree {TreeHash}");
            if (ParentHash is not null)
                sb.AppendLine($"parent {ParentHash}");
            sb.AppendLine($"author {Author}");
            sb.AppendLine($"committer {Committer}");
            sb.AppendLine();
            sb.AppendLine(Message);
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes a CommitGitObject from its raw content (as stored in .git/objects).
        /// </summary>
        /// <param name="content">Raw byte content of the commit.</param>
        public static CommitGitObject FromContent(byte[] content)
        {
            var contentStr = Encoding.UTF8.GetString(content);
            using var reader = new StringReader(contentStr);

            string? treeHash = null;
            string? parentHash = null;
            string? author = null;
            string? committer = null;
            string? line;

            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                if (line.StartsWith("tree "))
                    treeHash = line[5..].Trim();
                else if (line.StartsWith("parent "))
                    parentHash = line[7..].Trim();
                else if (line.StartsWith("author "))
                    author = line[7..].Trim();
                else if (line.StartsWith("committer "))
                    committer = line[10..].Trim();
            }

            var message = reader.ReadToEnd().Trim();

            if (treeHash is null || author is null || committer is null)
                throw new InvalidDataException("Commit object is missing required fields.");

            return new CommitGitObject(treeHash, parentHash, author, committer, message);
        }
    }
}
