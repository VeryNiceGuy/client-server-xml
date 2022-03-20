using System.IO;
using System.Threading.Tasks;

namespace Server
{
    class FileItem : ReadItem
    {
        public string FilePath { get; set; }
        private FileStream _fileStream;

        public override async Task<(bool complete, int offset)> Read(int offset, int bytesRead, byte[] buffer)
        {
            if(_fileStream == null)
            {
                _fileStream = File.OpenWrite(FilePath);
            }

            var leftToRead = Size - BytesRead;
            var available = bytesRead - offset;
            var toBeRead = (int)(available < leftToRead ? available : leftToRead);

            await _fileStream.WriteAsync(buffer, offset, toBeRead);
            BytesRead += toBeRead;

            if (BytesRead < Size)
            {
                return (false, offset + toBeRead);
            }
            else
            {
                _fileStream.Close();
                return (true, offset + toBeRead);
            }
        }
    }
}
