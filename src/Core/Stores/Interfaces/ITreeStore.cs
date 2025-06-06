namespace Core.Stores.Interfaces
{
    public interface ITreeStore
    {
        /// <summary>
        /// Builds the tree structure from the current index entries and saves all Git tree objects.
        /// Returns the root tree's hash.
        /// If the index is empty, returns an empty string and no tree is created.
        /// </summary>
        public string BuildTreeFromIndex();
    }
}
