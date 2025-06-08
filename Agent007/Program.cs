using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace Agent007
{
    class Program
    {
        static ConcurrentQueue<string> outputQueue = new ConcurrentQueue<string>();
        static bool readingDone = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Agent007 started");

            Console.Write("Path to txt files: ");
            string directoryPath = Console.ReadLine();

            Thread readerThread = new Thread(() => ReadAndProcessFiles(directoryPath));
            Thread senderThread = new Thread(SendToMaster);

            readerThread.Start();
            senderThread.Start();

            readerThread.Join();
            senderThread.Join();

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
            using (var pipe = new NamedPipeClientStream(".", "pipe_agent007", PipeDirection.Out))
            {
                pipe.Connect();
                using (StreamWriter writer = new StreamWriter(pipe))
                {
                    writer.AutoFlush = true;
                    while (!readingDone || !outputQueue.IsEmpty)
                    {
                        if (outputQueue.TryDequeue(out string data))
                        {
                            writer.WriteLine(data);
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
            }
        }

        static Dictionary<string, int> CountWords(string text)
        {
            Dictionary<string, int> wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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
