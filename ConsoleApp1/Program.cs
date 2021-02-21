using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            //EUR 2018-09-01 2018-09-20
            if (args.Length == 0)
                return;
            Console.WriteLine(args.Length);
            if (args.Length != 3)
            {
                Console.WriteLine("Unsupported input format");
                return;
            }
            bool wrongCur = true;
            var currencies = Enum.GetValues(typeof(Currency));
            string currency = args[0];
            foreach (var c in currencies)
            {
                if (c.ToString() == currency)
                    wrongCur = false;
            }
            if (wrongCur)
            {
                Console.WriteLine("Not supported currency");
                return;
            }
            try
            {
                DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
                DateTime.TryParseExact(args[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2);
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
            string beginFileDate = string.Join("", args[1].Substring(2).Split('-'));
            string endFileDate = string.Join("", args[2].Substring(2).Split('-'));
            string beginYear = args[1].Substring(0, 4);
            string endYear = args[2].Substring(0, 4);
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
                        if (string.Compare(docDate, endFileDate) <= 0 && string.Compare(docDate, beginFileDate) >= 0)
                            docDirs.Add(line);
                    }
                    File.Delete("tempfile.txt");
                }
            }
            List<CurrencyInfo> currList = new List<CurrencyInfo>();
            int processedEntries = 1;
            foreach (var year in yearsToDownload)
            {
                string yearSignature = year.Substring(2);
                var yearDocs = docDirs.Where(x => x.Substring(5, 2) == yearSignature);
                int cnt = docDirs.Count();
                foreach (var y in yearDocs)
                {
                    Console.WriteLine($"processing {processedEntries} out of {cnt} entries.");
                    string path = $"http://www.nbp.pl/kursy/xml/{y}.xml";
                    try
                    {
                        XDocument doc = XDocument.Load(path);
                        var d = doc.Root.Elements("pozycja").Single(x => x.Element("kod_waluty").Value == currency);
                        string date = doc.Root.Element("data_publikacji").Value;
                        decimal buyPrice = decimal.Parse(d.Element("kurs_kupna").Value);
                        decimal sellPrice = decimal.Parse(d.Element("kurs_sprzedazy").Value);
                        currList.Add(new CurrencyInfo { date = date, buyPrice = buyPrice, sellPrice = sellPrice });
                        processedEntries++;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to load data.");
                        return;
                    }
                }
            }
            if (currList.Count == 0)
            {
                Console.WriteLine("No data avaiable for this timespan.");
                return;
            }
            Console.Clear();
            Console.WriteLine($"Data about {currency} from {args[1]} to {args[2]}:\n");
            //sell
            var sortedSell = currList.OrderByDescending(x => x.sellPrice);
            var highestSell = sortedSell.First();
            var lowestSell = sortedSell.Last();
            var allHighestSell = sortedSell.Where(x => x.sellPrice == highestSell.sellPrice);
            var allLowestSell = sortedSell.Where(x => x.sellPrice == lowestSell.sellPrice);
            decimal avgSell = sortedSell.Aggregate(0m, (sum,val)=> sum + val.sellPrice)/sortedSell.Count();
            double varianceSell = 0;
            foreach(var cur in sortedSell)
            {
                varianceSell += Math.Pow((double)(cur.sellPrice - avgSell), 2);
            }
            double standardDeviationSell = Math.Sqrt(varianceSell/sortedSell.Count());
            Console.WriteLine($"Average sell exchange rate: {avgSell:C4}");
            Console.WriteLine($"Standard deviation for sell exchange rate: {standardDeviationSell:C4}");
            Console.WriteLine("Highest sell exchange rate:");
            foreach (var a in allHighestSell)
                Console.WriteLine($"{a.date}: {a.sellPrice:C4}");
            Console.WriteLine("Lowest sell exchange rate:");
            foreach (var a in allLowestSell)
                Console.WriteLine($"{a.date}: {a.sellPrice:C4}");
            //buy
            Console.WriteLine();
            var sortedBuy = currList.OrderByDescending(x => x.buyPrice);
            var highestBuy = sortedBuy.First();
            var lowestBuy = sortedBuy.Last();
            var allHighestBuy = sortedBuy.Where(x => x.buyPrice == highestBuy.buyPrice);
            var allLowestBuy = sortedBuy.Where(x => x.buyPrice == lowestBuy.buyPrice);
            decimal avgBuy = sortedBuy.Aggregate(0m, (sum, val) => sum + val.buyPrice) / sortedBuy.Count();
            double varianceBuy = 0;
            foreach (var cur in sortedBuy)
            {
                varianceBuy += Math.Pow((double)(cur.buyPrice - avgBuy), 2);
            }
            double standardDeviationBuy = Math.Sqrt(varianceBuy / sortedBuy.Count());
            Console.WriteLine($"Average buy exchange rate: {avgBuy:C4}");
            Console.WriteLine($"Standard deviation for buy exchange rate: {standardDeviationBuy:C4}");
            Console.WriteLine("Highest buy exchange rate:");
            foreach (var a in allHighestBuy)
                Console.WriteLine($"{a.date}: {a.buyPrice:C4}");
            Console.WriteLine("Lowest buy exchange rate:");
            foreach (var a in allLowestBuy)
                Console.WriteLine($"{a.date}: {a.buyPrice:C4}");

            Console.ReadKey();
        }
        enum Currency { USD, EUR, CHF, GBP };
        public struct CurrencyInfo
        {
            public decimal sellPrice { get; set; }
            public decimal buyPrice { get; set; }
            public string date { get; set; }
        }
    }
}
