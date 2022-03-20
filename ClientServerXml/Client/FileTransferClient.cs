using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Client
{
    class FileTransferClient
    {
        private const int _bufferSize = 1024;

        private async Task FromFileToNetwork(FileStream fileStream, NetworkStream networkStream)
        {
            byte[] buffer = new byte[_bufferSize];

            while (true)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                await networkStream.WriteAsync(buffer, 0, bytesRead);
            }
        }

        public async Task UploadFiles(Guid clientId, string documentFilePath, string listFilePath)
        {
            using FileStream document = File.OpenRead(documentFilePath);
            using FileStream list = File.OpenRead(listFilePath);

            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 8080);
            using var networkStream = client.GetStream();

            Header header = new Header { Id = clientId, DocumentLength = document.Length, ListLength = list.Length };

            await networkStream.WriteAsync(header.Id.ToByteArray(), 0, 16);
            await networkStream.WriteAsync(BitConverter.GetBytes(header.DocumentLength), 0, 4);
            await networkStream.WriteAsync(BitConverter.GetBytes(header.ListLength), 0, 4);

            await FromFileToNetwork(document, networkStream);
            await FromFileToNetwork(list, networkStream);
        }
    }
}
