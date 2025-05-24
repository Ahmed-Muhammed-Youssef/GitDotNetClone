using System.Security.Cryptography;
using System.Text;

namespace Core.Objects
{
    public abstract class GitObject
    {
        public abstract string Type { get; }
        public abstract byte[] Content { get; }
        public byte[] Serialize()
        {
            var header = $"{Type} {Content.Length}\0";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            return headerBytes.Concat(Content).ToArray();
        }

        public string GetHash()
        {
            var serialized = Serialize();
            return Convert.ToHexStringLower(SHA256.HashData(serialized));
        }
    }
}
