using Core.Services;
using CrossCuttingConcerns.Helpers;

namespace CLI.Services
{
    public class GitContextProvider : IGitContextProvider
    {
        /// <summary>
        /// Gets the current working directory from which Git commands are being executed.
        /// </summary>
        /// <returns>The absolute path of the current working directory.</returns>
        public string GetWorkingDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

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
