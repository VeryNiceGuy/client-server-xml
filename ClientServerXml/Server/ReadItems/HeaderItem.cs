using System;
using System.Threading.Tasks;

namespace Server
{
    class HeaderItem : ReadItem
    {
        private readonly Header _header;
        private readonly byte[] _bytes;
        private readonly FileItem _documentItem;
        private readonly FileItem _listItem;

        public HeaderItem(FileItem documentItem, FileItem listItem) : base((sizeof(int) * 2) + 16)
        {
            _header = new Header();
            _bytes = new byte[Size];
            _documentItem = documentItem;
            _listItem = listItem;
        }

        private void InitHeader()
        {
            byte[] guidBytes = new byte[16];
            Array.Copy(_bytes, 0, guidBytes, 0, 16);
            _header.Id = new Guid(guidBytes);

            byte[] intBytes = new byte[4];
            Array.Copy(_bytes, 16, intBytes, 0, 4);
            _header.DocumentLength = BitConverter.ToInt32(intBytes, 0);

            Array.Copy(_bytes, 20, intBytes, 0, 4);
            _header.ListLength = BitConverter.ToInt32(intBytes, 0);

            _documentItem.Size = _header.DocumentLength;
            _documentItem.FilePath = $"{_header.Id}.xml";

            _listItem.Size = _header.ListLength;
            _listItem.FilePath = $"{_header.Id}.txt";
        }

        public override async Task<(bool complete, int offset)> Read(int offset, int bytesRead, byte[] buffer)
        {
            try
            {
                var leftToRead = Size - BytesRead;
                var available = bytesRead - offset;
                var toBeRead = (int)(available < leftToRead ? available : leftToRead);

                Array.Copy(buffer, offset, _bytes, BytesRead, toBeRead);
                BytesRead += toBeRead;

                if (BytesRead < Size)
                {
                    return (false, offset + toBeRead);
                }
                else
                {
                    InitHeader();
                    return (true, offset + toBeRead);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return (false, offset + 0);
            }
        }
    }
}
