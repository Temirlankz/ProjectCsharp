using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace ProjectCSharp
{
    class Project
    {
        static ConcurrentDictionary<string, Dictionary<string, int>> mergedData = new();

        static void Main(string[] args)
        {
            Console.WriteLine("Master started");

            Thread agent1Thread = new Thread(() => ListenToAgent("agent007"));
            Thread agent2Thread = new Thread(() => ListenToAgent("agent008"));

            agent1Thread.Start();
            agent2Thread.Start();

            agent1Thread.Join();
            agent2Thread.Join();

            Console.WriteLine("\n Final count result \n");
            foreach (var fileEntry in mergedData)
            {
                foreach (var wordEntry in fileEntry.Value)
                {
                    Console.WriteLine($"{fileEntry.Key}:{wordEntry.Key}:{wordEntry.Value}");
                }
            }

            Console.WriteLine("\nFinished.");
        }

        static void ListenToAgent(string pipeName)
        {
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Console.WriteLine($"Connecting {pipeName}");
            pipe.WaitForConnection();
            Console.WriteLine($"{pipeName} connected");

            using var reader = new StreamReader(pipe);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"[{pipeName}] {line}");
                ProcessLine(line);
            }
        }

        static void ProcessLine(string line)
        {
            var parts = line.Split(':');
            if (parts.Length != 3) return;

            string file = parts[0];
            string word = parts[1];
            if (!int.TryParse(parts[2], out int count)) return;

            mergedData.AddOrUpdate(file,
                _ => new Dictionary<string, int> { [word] = count },
                (_, dict) =>
                {
                    if (dict.ContainsKey(word))
                        dict[word] += count;
                    else
                        dict[word] = count;
                    return dict;
                });
        }
    }
}
