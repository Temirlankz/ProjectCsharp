using System;
using System.IO;

namespace Agent008
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Asd");

            Console.Write("Enter the path to the directory with .txt files: ");
            string directoryPath = Console.ReadLine();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            string[] files = Directory.GetFiles(directoryPath, "*.txt");

            if (files.Length == 0)
            {
                Console.WriteLine("No .txt files found.");
                return;
            }

            Console.WriteLine("\nText files found:");
            foreach (string file in files)
            {
                Console.WriteLine(Path.GetFileName(file));
            }
        }
    }
}