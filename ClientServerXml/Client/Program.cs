using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        public async static Task Main(string[] args)
        {
            Console.WriteLine("Please provide a path to xml:");
            var documentFilePath = Console.ReadLine();

            Console.WriteLine("Please provide a path to list:");
            var listFilePath = Console.ReadLine();

            var clientId = Guid.NewGuid();
            var fileTransferClient = new FileTransferClient();
            await fileTransferClient.UploadFiles(clientId, documentFilePath, listFilePath);

            var processorClient = new ProcessorClient();
            await processorClient.Start(clientId);

            Console.WriteLine("Program terminated");
            Console.ReadLine();
        }
    }
}
