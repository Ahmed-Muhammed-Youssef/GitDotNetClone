namespace Core.Objects
{
    public class BlobGitObject : GitObject
    {
        private readonly byte[] _content;

        public BlobGitObject(byte[] content)
        {
            _content = content;
        }

        /// <summary>
        /// for deserialization purposes only.
        /// </summary>
        public BlobGitObject()
        {
            _content = [];
        }
        public override string Type => "blob";
        public override byte[] GetContent() => _content;

    }
}
