namespace Core.Objects
{
    public class BlobGitObject(byte[] content) : GitObject
    {
        private readonly byte[] _content = content;
        public override string Type => "blob";
        public override byte[] GetContent() => _content;
    }
}
