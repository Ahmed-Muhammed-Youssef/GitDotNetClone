using Core.Services;
using Infrastructure.FileSystem;

namespace CLI.Services
{
    public class GitContextProvider : IGitContextProvider
    {
        /// <summary>
        /// Attempts to find the Git repository root starting from the current directory.
        /// </summary>
        /// <param name="root">The discovered Git root if found.</param>
        /// <returns>True if the root was found, otherwise false.</returns>
        public bool TryGetRepositoryRoot(out string root)
        {
            root = PathHelper.GetRepositoryRoot(Directory.GetCurrentDirectory()) ?? string.Empty;
            return !string.IsNullOrEmpty(root);
        }
    }
}
