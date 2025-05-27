namespace Core.Objects
{
    public class BlobGitObject(byte[] content) : GitObject
    {
        public override string Type => "blob";

        public override byte[] Content { get; } = content;
    }
}
