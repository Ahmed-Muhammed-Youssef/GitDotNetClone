namespace Core.Constants
{
    public static class GitFileModes
    {
        public const string RegularFile = "100644";
        public const string Executable = "100755";
        public const string Symlink = "120000";
        public const string Tree = "40000";
        public const string Submodule = "160000";
    }
}
