using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Grpc.Core;
using GrpcProcessor;
using Server.Models;

namespace Server
{
    class ProcessorService : Processor.ProcessorBase
    {
        private int CalculateNumRepetitions(string unique, List<string> words)
        {
            int count = 0;
            foreach(var word in words)
            {
                if(word.Equals(unique))
                {
                    ++count;
                }
            }
            return count;
        }

        private ICollection<Duplicate> ExtractDuplicates(string text)
        {
            List<Duplicate> duplicates = new List<Duplicate>();
            var matches = Regex.Matches(text, @"((\b[^\s]+\b)((?<=\.\w).)?)")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            var uniqueWords = matches.Distinct();

            foreach (var word in uniqueWords)
            {
                var numRepetitions = CalculateNumRepetitions(word, matches);
                if(numRepetitions > 1)
                {
                    duplicates.Add(new Duplicate { Word = word, NumRepetitions = numRepetitions });
                }
            }

            return duplicates;
        }

        protected virtual bool IsFileLocked(string filePath)
        {
            try
            {
                using FileStream stream = new FileStream(filePath,  FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        public override async Task StartProcessing(ProcessRequest request, IServerStreamWriter<ProcessReport> responseStream, ServerCallContext context)
        {
    
            var clientId = new Guid(request.Id.ToByteArray());
            var documentFilePath = $"{clientId}.xml";
            var listFilePath = $"{clientId}.txt";

            if(IsFileLocked(documentFilePath) || IsFileLocked(listFilePath))
            {
                await responseStream.WriteAsync(new ProcessReport { Message = "CMD_RETRY" });
                return;
            }

            using var db = new ServerContext();
            using var transaction = db.Database.BeginTransaction();

            var list = File.ReadAllText(listFilePath);
            var elementNames = list.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var elementCounters = new Dictionary<string, int>();

            foreach (var name in elementNames)
            {
                elementCounters.Add(name, 0);
            }

            await responseStream.WriteAsync(new ProcessReport { Message = "Processing started." });

            using (var documentReader = XmlReader.Create(documentFilePath, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Parse }))
            {
                try
                {
                    while (await documentReader.ReadAsync())
                    {
                        if (documentReader.NodeType == XmlNodeType.Element)
                        {
                            var elementName = documentReader.Name;

                            if (elementCounters.ContainsKey(elementName))
                            {
                                var tag = db.Tags.SingleOrDefault((t) => t.Name.Equals(elementName));
                                if(tag == null)
                                {
                                    db.Tags.Add(new Tag { Name = elementName, Elements = new List<Element>() });
                                    db.SaveChanges();
                                    tag = db.Tags.First((t)=>t.Name.Equals(elementName));
                                }

                                var content = documentReader.ReadInnerXml();
                                tag.Elements.Add(new Element { Content = content, Duplicates = ExtractDuplicates(content) });

                                elementCounters[elementName] = elementCounters[elementName] + 1;
                                await responseStream.WriteAsync(new ProcessReport { Message = $"{elementCounters[elementName]} {elementName} elements found." });
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await responseStream.WriteAsync(new ProcessReport { Message = "Processing complete." });

            File.Delete(documentFilePath);
            File.Delete(listFilePath);

            db.SaveChanges();
        }
    }
}
