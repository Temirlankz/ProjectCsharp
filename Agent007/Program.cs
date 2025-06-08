using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Agent007
{
    class Program
    {
        static ConcurrentQueue<string> outputQueue = new ConcurrentQueue<string>();
        static bool readingDone = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Agent007 started");

            // Set CPU core
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << 1);
                Console.WriteLine("Agent007 running on CPU Core 2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Affinity] Error: {ex.Message}");
            }

            Console.Write("Path to txt files: ");
            string directoryPath = Console.ReadLine();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Path not exist");
                return;
            }

            // Run reading 
            await Task.WhenAll(
                Task.Run(() => ReadAndProcessFiles(directoryPath)),
                Task.Run(() => SendToMaster())
            );

            Console.WriteLine("Agent007 finished");
        }

        static void ReadAndProcessFiles(string directoryPath)
        {
            string[] files = Directory.GetFiles(directoryPath, "*.txt");

            foreach (string file in files)
            {
                string text = File.ReadAllText(file);
                var wordCounts = CountWords(text);

                foreach (var pair in wordCounts)
                {
                    string output = $"{Path.GetFileName(file)}:{pair.Key}:{pair.Value}";
                    outputQueue.Enqueue(output);
                }
            }

            readingDone = true;
        }

        static void SendToMaster()
        {
            using var pipe = new NamedPipeClientStream(".", "agent007", PipeDirection.Out);
            pipe.Connect();

            using var writer = new StreamWriter(pipe) { AutoFlush = true };

            while (!readingDone || !outputQueue.IsEmpty)
            {
                if (outputQueue.TryDequeue(out string data))
                {
                    writer.WriteLine(data);
                }
                else
                {
                    Task.Delay(50).Wait();
                }
            }
        }

        static Dictionary<string, int> CountWords(string text)
        {
            Dictionary<string, int> wordCounts = new(StringComparer.OrdinalIgnoreCase);
            string[] words = Regex.Split(text, @"\W+");

            foreach (string word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                if (wordCounts.ContainsKey(word))
                    wordCounts[word]++;
                else
                    wordCounts[word] = 1;
            }

            return wordCounts;
        }
    }
}
