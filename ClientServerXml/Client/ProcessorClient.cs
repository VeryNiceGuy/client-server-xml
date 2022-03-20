using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcProcessor;

namespace Client
{
    class ProcessorClient
    {
        private static int _port = 8081;
        public async Task Start(Guid clientId)
        {
            using var channel = GrpcChannel.ForAddress($"http://localhost:{_port}");
            var client = new Processor.ProcessorClient(channel);

            while (true)
            {
                var reports = client.StartProcessing(new ProcessRequest { Id = ByteString.CopyFrom(clientId.ToByteArray()) });
                await foreach (var report in reports.ResponseStream.ReadAllAsync())
                {
                    if (report.Equals("CMD_RETRY"))
                    {
                        Console.WriteLine("Files are still being transfered, please try later. Use command 'retry'");
                    }
                    else
                    {
                        Console.WriteLine(report.Message);
                    }
                }

                var command = Console.ReadLine();

                if (command.Equals("retry"))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
