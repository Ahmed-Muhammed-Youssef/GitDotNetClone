namespace Infrastructure.FileSystem
{
    /// <summary>
    /// Provides utilities for locating the root of a Git repository by searching upward
    /// from a given directory until a `.git` directory is found.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Converts all directory separators to '/' for cross-platform consistency.
        /// </summary>
        /// <param name="relativePath">The absolute file or directory path to normalize.</param>
        /// <returns>A normalized path using '/' as separator.</returns>
        public static string Normalize(string relativePath)
        {
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>
        /// Denormalizes a path back to a file system path.
        /// Converts '/' separators to OS-specific separators.
        /// </summary>
        /// <param name="normalizedPath">The normalized path using '/' as separator.</param>
        /// <returns>The path corresponding to the normalized path.</returns>
        public static string Denormalize(string normalizedPath)
        {
            return normalizedPath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Recursively searches parent directories from the given start path (or current directory) 
        /// to locate the Git repository root.
        /// </summary>
        /// <param name="startPath">Optional start path; defaults to the current working directory.</param>
        /// <returns>The absolute path to the directory that contains the `.git` folder.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no `.git` folder is found.</exception>
        public static string? GetRepositoryRoot(string? startDirectory)
        {
            DirectoryInfo? currentDirectory = new(startDirectory ?? Directory.GetCurrentDirectory());
            while (currentDirectory != null)
            {
                if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
                {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            return null;
        }
    }
}
