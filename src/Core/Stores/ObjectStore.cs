using Core.Objects;
using System.Text.Json;

namespace Core.Stores
{
    /// <summary>
    /// Provides functionality to store and retrieve Git objects
    /// from the .git/objects directory in a content-addressed format.
    /// </summary>
    public static class ObjectStore
    {
        /// <summary>
        /// Loads the byte array of a Git object from the object store using its hash.
        /// </summary>
        /// <param name="hash">The SHA-256 hash of the Git object.</param>
        /// <param name="rootPath">The root path of the repository (where the <c>.git</c> directory resides).</param>
        /// <returns>The deserialized <see cref="byte[]"/> instance.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the object file does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory is found.</exception>
        /// <exception cref="FormatException">Thrown if the object file format is invalid.</exception>
        public static byte[] Load(string hash, string rootPath)
        {
            string filePath = Path.Combine(rootPath, ".git", "objects", hash[..2], hash[2..]);

            byte[] data = File.ReadAllBytes(filePath);

            return data;
        }

        /// <summary>
        /// Loads a Git object of type <typeparamref name="T"/> from the object store using its hash.
        /// </summary>
        /// <typeparam name="T">The type of the Git object to load. Must derive from <see cref="GitObject"/>.</typeparam>
        /// <param name="hash">The SHA-256 hash of the Git object.</param>
        /// <param name="rootPath">The root path of the repository (where the <c>.git</c> directory resides).</param>
        /// <param name="options">The JSON serializer options used to deserialize the object.</param>
        /// <returns>
        /// The deserialized Git object of type <typeparamref name="T"/> if found; otherwise, <c>null</c> if deserialization fails.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown if the object file does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the expected object directory is missing.</exception>
        /// <exception cref="JsonException">Thrown if the object content cannot be deserialized into the specified type.</exception>
        public static T? Load<T>(string hash, string rootPath, JsonSerializerOptions options) where T : GitObject
        {
            string filePath = Path.Combine(rootPath, ".git", "objects", hash[..2], hash[2..]);

            byte[] data = File.ReadAllBytes(filePath);

            return JsonSerializer.Deserialize<T>(data, options);
        }

        /// <summary>
        /// Saves a Git object of type <typeparamref name="T"/> to the object store if it does not already exist.
        /// The object is stored under the path <c>.git/objects/xx/yyyy...</c> where:
        /// <list type="bullet">
        ///   <item><description><c>xx</c> is the first two characters of the object's SHA-256 hash</description></item>
        ///   <item><description><c>yyyy...</c> is the remaining part of the hash</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="T">The type of <see cref="GitObject"/> being saved.</typeparam>
        /// <param name="obj">The Git object instance to save.</param>
        /// <param name="rootPath">The root path of the repository (where the <c>.git</c> directory resides).</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a hash collision occurs (i.e., a file with the same hash already exists but its content differs from the current object).
        /// </exception>
        public static void Save<T>(T obj, string rootPath) where T : GitObject
        {
            string hash = obj.GetHash();
            byte[] content = obj.GetContent();
            string dir = Path.Combine(rootPath, ".git", "objects", hash[..2]);
            string file = Path.Combine(dir, hash[2..]);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(file))
            {
                File.WriteAllBytes(file, content);
            }
            else
            {
                var existing = File.ReadAllBytes(file);
                if (!existing.SequenceEqual(content))
                    throw new InvalidOperationException("Hash collision detected: object exists but differs.");
            }
        }
    }
}
