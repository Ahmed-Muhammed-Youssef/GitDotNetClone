using System.Text;

namespace CrossCuttingConcerns.Helpers
{
    public static class GitObjectSerializer
    {
        public static byte[] AddHeader(string type, byte[] content)
        {
            var header = $"{type} {content.Length}\0";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            return headerBytes.Concat(content).ToArray();
        }

        public static (string Type, byte[] Content) RemoveHeader(byte[] raw)
        {
            int nullIndex = Array.IndexOf(raw, (byte)0);
            if (nullIndex == -1)
                throw new FormatException("Invalid Git object format.");

            string header = Encoding.UTF8.GetString(raw, 0, nullIndex);
            byte[] content = raw[(nullIndex + 1)..];

            string[] parts = header.Split(' ');
            if (parts.Length != 2)
                throw new FormatException("Invalid header format.");

            return (parts[0], content);
        }
    }
}
