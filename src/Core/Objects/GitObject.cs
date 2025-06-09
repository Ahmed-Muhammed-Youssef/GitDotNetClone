using System.Security.Cryptography;
using System.Text.Json;

namespace Core.Objects
{
    public abstract class GitObject
    {
        /// <summary>
        /// Gets raw content without header.
        /// </summary>
        public abstract byte[] GetContent();
        public abstract string Type { get; }
        private readonly JsonSerializerOptions _serializerOptions;
         
        protected GitObject()
        {
            _serializerOptions = new()
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Serializes the current <see cref="GitObject"/> instance to a UTF-8 encoded JSON byte array.
        /// </summary>
        /// <returns>A <see cref="byte"/> array containing the UTF-8 encoded JSON representation of the object.</returns>
        public byte[] SerializeToUtf8<T>(T obj) where T : GitObject
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, _serializerOptions);
        }

        /// <summary>
        /// Deserializes the current Git object's content into the specified derived <see cref="GitObject"/> type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="GitObject"/> to deserialize into.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> representing the deserialized Git object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if deserialization returns <c>null</c>.</exception>
        public T Deserialize<T>() where T : GitObject
        {
            T? result = JsonSerializer.Deserialize<T>(GetContent(), _serializerOptions);
            return result ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        /// <summary>
        /// Computes the full hash (based on type + content).
        /// </summary>
        public string GetHash()
        {
            byte[] serialized = GetContent();
            return Convert.ToHexStringLower(SHA256.HashData(serialized));
        }
    }
}
