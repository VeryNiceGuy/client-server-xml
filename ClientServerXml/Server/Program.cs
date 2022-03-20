using System.Threading.Tasks;
using Grpc.Core;

namespace Server
{
    class Program
    {
        private static int _port = 8081;

        private static Grpc.Core.Server _grpcServer = new Grpc.Core.Server
        {
            Services = { GrpcProcessor.Processor.BindService(new ProcessorService()) },
            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
        };

        private static FileTransferServer _fileTransferServer = new();

        public static async Task Main(string[] args)
        {
            _grpcServer.Start();
            await _fileTransferServer.Start();
            await _grpcServer.ShutdownAsync();
        }
    }
}
