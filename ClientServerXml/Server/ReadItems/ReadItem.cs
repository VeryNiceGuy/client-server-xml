using System.Threading.Tasks;

namespace Server
{
    abstract class ReadItem
    {
        public ReadItem(long size = 0)
        {
            Size = size;
        }

        public long Size { get; set; }
        protected long BytesRead { get; set; }
        public abstract Task<(bool complete, int offset)> Read(int offset, int bytesRead, byte[] buffer);
    }
}
