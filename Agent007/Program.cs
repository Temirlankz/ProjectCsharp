using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Agent007
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Agent007 started.");

            Console.Write("Path to txt files: ");
            string directoryPath = Console.ReadLine();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Path do not exist.");
                return;
            }

            string[] files = Directory.GetFiles(directoryPath, "*.txt");

            if (files.Length == 0)
            {
                Console.WriteLine("No txt files");
                return;
            }

            foreach (string file in files)
            {
                Console.WriteLine($"\nReading file: {Path.GetFileName(file)}");
                string text = File.ReadAllText(file);

                Dictionary<string, int> wordCounts = CountWords(text);

                foreach (var pair in wordCounts)
                {
                    Console.WriteLine($"{Path.GetFileName(file)}: {pair.Key} = {pair.Value}");
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
