namespace Core.Objects
{
    public class TreeGitObject(byte[] content) : GitObject
    {
        public override string Type => "tree";

        public override byte[] Content { get; } = content;
    }
}
