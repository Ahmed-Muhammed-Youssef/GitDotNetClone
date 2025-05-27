namespace Core.Index
{
    public class IndexEntry(string filePath, string blobHash, long size, int mode = 0b110100100)
    {
        public string FilePath { get; init; } = filePath;
        public string BlobHash { get; init; } = blobHash;
        public long Size { get; init; } = size;
        public int Mode { get; init; } = mode;
        public DateTime LastTimeModified { get; set; } = DateTime.UtcNow;
    }
}
