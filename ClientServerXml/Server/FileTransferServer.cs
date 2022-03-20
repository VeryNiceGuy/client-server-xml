using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class FileTransferServer
    {
        private const int bufferSize = 1024;
        private const int backlogSize = 100;

        public async Task Start()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            var listener = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endpoint);
            listener.Listen(backlogSize);

            while (true)
            {
                await UploadFile(await listener.AcceptAsync());
            }
        }

        async Task UploadFile(Socket client)
        {
            try
            {
                using var networkStream = new NetworkStream(client);
                var buffer = new byte[bufferSize];
                var bytesRead = 0;
                var offset = 0;
                var readStack = new Stack<ReadItem>();
                var listItem = new FileItem();
                var documentItem = new FileItem();

                readStack.Push(listItem);
                readStack.Push(documentItem);
                readStack.Push(new HeaderItem(documentItem, listItem));

                while (true)
                {
                    var reminder = bytesRead - offset;
                    if (reminder == 0)
                    {
                        offset = 0;
                        bytesRead = await networkStream.ReadAsync(buffer, 0, bufferSize);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                    }

                    var readResult = await readStack.Peek().Read(offset, bytesRead, buffer);
                    offset = readResult.offset;

                    if (readResult.complete)
                    {
                        readStack.Pop();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
