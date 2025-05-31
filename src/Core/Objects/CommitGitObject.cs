namespace Core.Objects
{
    public class CommitGitObject(byte[] content) : GitObject
    {
        public override string Type => "commit";

        public override byte[] Content => content;
    }
}
