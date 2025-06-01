using Core.Objects;

namespace Core.Stores
{
    /// <summary>
    /// Provides functionality to store and retrieve Git objects
    /// from the .git/objects directory in a content-addressed format.
    /// </summary>
    public static class ObjectStore
    {
        /// <summary>
        /// Loads a Git object from the object store using its hash.
        /// </summary>
        /// <param name="hash">The SHA-1 or SHA-256 hash of the Git object.</param>
        /// <returns>The deserialized <see cref="GitObject"/> instance.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the object file does not exist.</exception>
        /// <exception cref="FormatException">Thrown if the object file format is invalid.</exception>
        public static GitObject Load(string hash)
        {
            string suffix = hash[2..];

            string filePath = Path.Combine(".git", "objects", hash[..2], hash[2..]);

            byte[] data = File.ReadAllBytes(filePath);

            return GitObject.RemoveHeader(data);
        }

        /// <summary>
        /// Saves a Git object to the object store if it does not already exist.
        /// Git objects are stored under .git/objects/xx/yyyy... where "xx" is the first
        /// two characters of the object's hash and "yyyy..." is the remaining part.
        /// </summary>
        /// <param name="obj">The <see cref="GitObject"/> to save.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a hash collision occurs (i.e., an object with the same hash exists but with different content).
        /// </exception>
        public static void Save(GitObject obj)
        {
            string hash = obj.GetHash();
            byte[] content = obj.AddHeader();
            string dir = Path.Combine(".git", "objects", hash[..2]);
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
