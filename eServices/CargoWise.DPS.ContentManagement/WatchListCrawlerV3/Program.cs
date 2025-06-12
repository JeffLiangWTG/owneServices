using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WatchListCrawlerV3
{
    public class Program
    {
        public static string batchProcessPath = "C:/batch.csv";
        static void Main(string[] args)
        {
            List<string[]> parsedData = new List<string[]>();
            if (File.Exists(batchProcessPath))
            {
                parsedData = parseCSV(batchProcessPath);
                foreach (string[] item in parsedData)
                {
                    Console.WriteLine("=========================");
                    Console.WriteLine("Using parameters:");
                    Console.WriteLine(item[0]);
                    Console.WriteLine(item[1]);
                    Console.WriteLine(item[2]);
                    Console.WriteLine();

                    NameUrlGrabber temp = new NameUrlGrabber(item[0], item[1], item[2]);
                    temp.DoWork();
                }
            }
            else
            {
                NameUrlGrabber temp = new NameUrlGrabber(args[0], args[1], args[2]);
                temp.DoWork();
            }
        }

        public static List<string[]> parseCSV(string path)
        {
            List<string[]> parsedData = new List<string[]>();

            try
            {
                using (StreamReader readFile = new StreamReader(path))
                {
                    string line;
                    string[] row;

                    while ((line = readFile.ReadLine()) != null)
                    {
                        row = line.Split('|');
                        parsedData.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return parsedData;
        }

    }
}
