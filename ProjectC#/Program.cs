using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace ProjectCSharp
{
    class Project
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Master process started");

            Thread agent1Thread = new Thread(() => ListenToAgent("pipe_agent007"));
            Thread agent2Thread = new Thread(() => ListenToAgent("pipe_agent008"));

            agent1Thread.Start();
            agent2Thread.Start();

            agent1Thread.Join();
            agent2Thread.Join();

            Console.WriteLine("Master process finished");
        }

        static void ListenToAgent(string pipeName)
        {
            using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                Console.WriteLine($"Connecting {pipeName}...");
                pipe.WaitForConnection();
                Console.WriteLine($"{pipeName} connected");

                using (StreamReader reader = new StreamReader(pipe))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine($"[{pipeName}] {line}");
                    }
                }
            }
        }
    }
}
