using System.Text;
using System.Text.Json;

namespace Core.Objects
{
    public class TreeGitObject : GitObject
    {
        public List<TreeEntry> TreeEntries { get; } = [];
        public override string Type => "tree";
        private readonly byte[] _content;
        public override byte[] GetContent() => _content;
        
        /// <summary>
        /// Construct a new tree object from entries.
        /// </summary>
        public TreeGitObject(List<TreeEntry> entries)
        {
            TreeEntries = entries;
            _content = SerializeTreeEntries(entries);
        }

        /// <summary>
        /// Deserialize a tree object from its raw byte content.
        /// </summary>
        public TreeGitObject(byte[] content)
        {
            _content = content;
            TreeEntries = DeserializeTreeEntries(content);
        }

        private static byte[] SerializeTreeEntries(List<TreeEntry> entries)
        {
            string json = JsonSerializer.Serialize(entries);
            return Encoding.UTF8.GetBytes(json);
        }

        private static List<TreeEntry> DeserializeTreeEntries(byte[] content)
        {
            string json = Encoding.UTF8.GetString(content);
            return JsonSerializer.Deserialize<List<TreeEntry>>(json) ?? [];
        }
    }
}
