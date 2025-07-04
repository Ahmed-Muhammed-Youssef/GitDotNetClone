﻿namespace Core.Objects
{
    public class TreeGitObject : GitObject
    {
        public List<TreeEntry> TreeEntries { get; init; } = [];
        public override string Type => "tree";
        private readonly byte[] _content;
        public override byte[] GetContent() => _content;
        
        /// <summary>
        /// Construct a new tree object from entries.
        /// </summary>
        public TreeGitObject(List<TreeEntry> entries)
        {
            TreeEntries = entries;
            _content = SerializeToUtf8(this);
        }

        /// <summary>
        /// Deserialize a tree object from its raw byte content.
        /// </summary>
        public TreeGitObject(byte[] content)
        {
            _content = content;
            TreeGitObject treeGitObject = Deserialize<TreeGitObject>();
            TreeEntries = treeGitObject.TreeEntries;
        }

        /// <summary>
        /// for deserialization purposes only.
        /// </summary>
        public TreeGitObject()
        {
            _content = [];
        }
    }
}
