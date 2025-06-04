namespace Core.Stores.Interfaces
{
    public interface ITreeStore
    {
        /// <summary>
        /// Builds the tree structure from the current index entries and saves all Git tree objects.
        /// Returns the root tree's hash.
        /// </summary>
        public string BuildTreeFromIndex();
    }
}
