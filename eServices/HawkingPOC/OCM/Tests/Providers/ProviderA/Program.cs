using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace ProviderA
{
    class Program
    {
        static int counter;
        static string basePath = @"C:\Temp\OcmPoc-Tranceivers\ProviderA";
        static string receivePath = Path.Combine(basePath, "send");
        static string sendPath = Path.Combine(basePath, "receive");

        static void Main(string[] args)
        {
            using(var watcher = new FileSystemWatcher(receivePath))
            {
                watcher.Created += HandleNewMessage;
                watcher.EnableRaisingEvents = true;

                Console.WriteLine($"Watching file system ({receivePath}).  Press <Enter> to exit.");
                
                Console.ReadLine();
                
                watcher.Created -= HandleNewMessage;
            }
        }

        static void HandleNewMessage(object _, FileSystemEventArgs args)
        {
            var filename = args.Name;
            var sender = Regex.Match(filename, "^From_(?<sender>[^_]+)_").Groups["sender"].Value;
            
            var responseFilename = $"To_{sender}_{Interlocked.Increment(ref counter)}";
            var resposeText = $"Response to {filename}";

            File.WriteAllText(Path.Combine(sendPath, responseFilename), resposeText);

            Console.WriteLine($"Received {filename}; Sent {responseFilename}");
        }
    }
}
