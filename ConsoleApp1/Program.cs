using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //EUR 2018-09-01 2018-09-20
            //string[] input = Console.ReadLine().Split(' ');
            //if (input.Length != 3)
            //{
            //    Console.WriteLine("Unsupported input format");
            //    return;
            //}
            //bool wrongCur = true;
            //var currency = Enum.GetValues(typeof(Currency));
            //foreach (var c in currency)
            //{
            //    if (c.ToString() == input[0])
            //        wrongCur = false;
            //}
            //if (wrongCur)
            //{
            //    Console.WriteLine("Not supported currency");
            //    return;
            //}
            //try
            //{
            //    DateTime.TryParseExact(input[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
            //    DateTime.TryParseExact(input[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2);
            //    if (result.Year == 0001 || result2.Year == 0001)
            //        throw new Exception();
            //    if (result2 < result)
            //    {
            //        Console.WriteLine("Invalid timespan format");
            //        return;
            //    }
            //    if(result.Year < 2002 || result2.Year < 2002)
            //    {
            //        Console.WriteLine("No information before 2002.");
            //        return;
            //    }

            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("Invalid date format");
            //    return;
            //}
            //string beginDate = string.Join("", input[1].Substring(2).Split('-'));
            //string endDate = string.Join("", input[2].Substring(2).Split('-'));
            //Console.WriteLine(beginDate);
            //Console.WriteLine(endDate);
            List<string> docDirs = new List<string>();
            using(var client = new WebClient())
            {
                if (File.Exists("tempfile.txt"))
                    File.Delete("tempfile.txt");
                Console.WriteLine("Downloading data...");
                client.DownloadFile("https://www.nbp.pl/kursy/xml/dir.txt", "tempfile.txt");
                var linesRead = File.ReadLines("tempfile.txt");
                //foreach (var line in linesRead)
                //    docDirs.Add(line);
                linesRead = linesRead.Where(x => x[0] == 'c').ToList();
                foreach (var line in linesRead)
                    docDirs.Add(line);
                File.Delete("tempfile.txt");
            }
            foreach(var dir in docDirs)
                Console.WriteLine(dir);
        }
        enum Currency { USD, EUR, CHF, GBP };
    }
}
