namespace Core.Services
{
    /// <summary>
    /// Provides information about the Git repository context.
    /// </summary>
    public interface IGitContextProvider
    {
        /// <summary>
        /// Attempts to find the Git repository root starting from the current directory.
        /// </summary>
        /// <param name="root">The discovered Git root if found.</param>
        /// <returns>True if the root was found, otherwise false.</returns>
        bool TryGetRepositoryRoot(out string root);
    }
}
