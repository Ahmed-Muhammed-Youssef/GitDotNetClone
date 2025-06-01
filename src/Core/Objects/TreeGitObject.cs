using System.Text;
using System.Text.Json;

namespace Core.Objects
{
    public class TreeGitObject : GitObject
    {
        public List<TreeEntry> TreeEntries { get; } = [];
        public override string Type => "tree";

        public override byte[] Content { get; } = [];
        
        /// <summary>
        /// Construct a new tree object from entries.
        /// </summary>
        public TreeGitObject(List<TreeEntry> entries)
        {
            TreeEntries = entries;
            Content = SerializeTreeEntries(entries);
        }

        /// <summary>
        /// Deserialize a tree object from its raw byte content.
        /// </summary>
        public TreeGitObject(byte[] content)
        {
            Content = content;
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
