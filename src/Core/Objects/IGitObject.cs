using CrossCuttingConcerns.Helpers;
using System.Security.Cryptography;

namespace Core.Objects
{
    public interface IGitObject
    {
        /// <summary>
        /// Git object type (e.g., "blob", "tree", "commit").
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Raw content without header.
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// Computes the full hash (based on type + content).
        /// </summary>
        string GetHash()
        {
            byte[] serialized = GitObjectSerializer.AddHeader(Type, Content);
            return Convert.ToHexStringLower(SHA256.HashData(serialized));
        }
    }
}
