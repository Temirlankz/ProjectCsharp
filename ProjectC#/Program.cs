using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectCSharp
{
    class Project
    {
        static ConcurrentDictionary<string, Dictionary<string, int>> mergedData = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Master started");

            //  Set CPU Affinity to Core 0
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << 0);
                Console.WriteLine("Master running on CPU Core 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Affinity] Error: {ex.Message}");
            }

            var agent1 = Task.Run(() => ListenToAgentAsync("agent007"));
            var agent2 = Task.Run(() => ListenToAgentAsync("agent008"));

            await Task.WhenAll(agent1, agent2);

            Console.WriteLine("\n Final Merged Result \n");
            foreach (var fileEntry in mergedData)
            {
                foreach (var wordEntry in fileEntry.Value)
                {
                    Console.WriteLine($"{fileEntry.Key}:{wordEntry.Key}:{wordEntry.Value}");
                }
            }

            Console.WriteLine("\n Finished");
        }

        static async Task ListenToAgentAsync(string pipeName)
        {
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Console.WriteLine($"Connecting {pipeName}");
            await pipe.WaitForConnectionAsync();
            Console.WriteLine($"{pipeName} connected");

            using var reader = new StreamReader(pipe);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
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
