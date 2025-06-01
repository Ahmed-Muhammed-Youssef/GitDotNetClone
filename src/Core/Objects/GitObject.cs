using System.Security.Cryptography;
using System.Text;

namespace Core.Objects
{
    public abstract class GitObject
    {
        public abstract string Type { get; }
        public abstract byte[] Content { get; }
        public byte[] AddHeader()
        {
            var header = $"{Type} {Content.Length}\0";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            return headerBytes.Concat(Content).ToArray();
        }

        public static GitObject RemoveHeader(byte[] source)
        {
            int nullIndex = Array.IndexOf(source, (byte)0);
            if (nullIndex == -1)
                throw new FormatException("Invalid Git object format.");

            string header = Encoding.UTF8.GetString(source, 0, nullIndex);
            byte[] content = source[(nullIndex + 1)..];

            string[] parts = header.Split(' ');
            if (parts.Length != 2)
                throw new FormatException("Invalid Git object header format.");

            string type = parts[0];

            return type switch
            {
                "blob" => new BlobGitObject(content),
                "tree" => new TreeGitObject(content),
                "commit" => new CommitGitObject(content),
                _ => throw new NotSupportedException($"Unsupported type: {type}")
            };
        }

        public string GetHash()
        {
            var serialized = AddHeader();
            return Convert.ToHexStringLower(SHA256.HashData(serialized));
        }
    }
}
