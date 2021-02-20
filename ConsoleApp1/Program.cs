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
            //EUR 2018-09-01 2019-09-20
            string[] input = Console.ReadLine().Split(' ');
            if (input.Length != 3)
            {
                Console.WriteLine("Unsupported input format");
                return;
            }
            bool wrongCur = true;
            var currency = Enum.GetValues(typeof(Currency));
            foreach (var c in currency)
            {
                if (c.ToString() == input[0])
                    wrongCur = false;
            }
            if (wrongCur)
            {
                Console.WriteLine("Not supported currency");
                return;
            }
            try
            {
                DateTime.TryParseExact(input[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
                DateTime.TryParseExact(input[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2);
                if (result.Year == 0001 || result2.Year == 0001)
                    throw new Exception();
                if (result2 < result)
                {
                    Console.WriteLine("Invalid timespan format");
                    return;
                }
                if (result.Year < 2002 || result2.Year < 2002)
                {
                    Console.WriteLine("No information before 2002.");
                    return;
                }
                if (result >= DateTime.Now || result2 >= DateTime.Now)
                {
                    Console.WriteLine("Date must be from past.");
                    return;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid date format");
                return;
            }
            string endFileDate = string.Join("", input[2].Substring(2).Split('-'));
            string beginYear = input[1].Substring(0, 4);
            string endYear = input[2].Substring(0, 4);
            List<string> yearsToDownload = new List<string>();
            if (beginYear == endYear)
                yearsToDownload.Add(beginYear);
            else if (int.Parse(endYear) - int.Parse(beginYear) == 1)
            {
                yearsToDownload.Add(beginYear);
                yearsToDownload.Add(endYear);
            }
            else
            {
                yearsToDownload.Add(beginYear);
                int begin = int.Parse(beginYear);
                int end = int.Parse(endYear);
                for (int i = begin + 1; i < end; i++)
                {
                    yearsToDownload.Add(i.ToString());
                }
                yearsToDownload.Add(endYear);
            }
            List<string> docDirs = new List<string>();
            using (var client = new WebClient())
            {
                if (File.Exists("tempfile.txt"))
                    File.Delete("tempfile.txt");
                Console.WriteLine("Downloading data...");
                foreach (var year in yearsToDownload)
                {
                    string inp = year;
                    if (year == DateTime.Now.Year.ToString())
                        inp = "";
                    try
                    {
                        client.DownloadFile($"https://www.nbp.pl/kursy/xml/dir{inp}.txt", "tempfile.txt");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to download data.");
                        return;
                    }
                    var linesRead = File.ReadLines("tempfile.txt");
                    foreach (var line in linesRead.Where(x => x[0] == 'c').ToList())
                    {
                        string docDate = line.Substring(5);
                        if(string.Compare(docDate,endFileDate) <= 0)
                        docDirs.Add(line);
                    }
                    File.Delete("tempfile.txt");
                }
            }
            foreach (var dir in docDirs)
                Console.WriteLine(dir);
        }
        enum Currency { USD, EUR, CHF, GBP };
    }
}
