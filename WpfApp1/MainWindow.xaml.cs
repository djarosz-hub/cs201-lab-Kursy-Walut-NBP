using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        List<string> AvailableCurrencies = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            FillCurrencies();
            ConstraintCalendars();
        }
        void FillCurrencies()
        {
            foreach (string name in Enum.GetNames(typeof(Currency)))
            {
                AvailableCurrencies.Add(name);
                CurrencyCB.Items.Add(name);
            }
        }
        void ConstraintCalendars()
        {
            FromDateDP.DisplayDateStart = new DateTime(2002, 1, 1);
            FromDateDP.DisplayDateEnd = DateTime.Today;
            ToDateDP.DisplayDateStart = new DateTime(2002, 1, 1);
            ToDateDP.DisplayDateEnd = DateTime.Today;
        }
        bool InvalidDates()
        {
            if (FromDateDP.SelectedDate == null)
            {
                MessageBox.Show("Choose date to start searing from.");
                return true;
            }
            if (ToDateDP.SelectedDate == null)
            {
                MessageBox.Show("Choose date to end searing at.");
                return true;
            }
            if (FromDateDP.SelectedDate >= ToDateDP.SelectedDate)
            {
                MessageBox.Show("Starting date must be earlier than ending date");
                return true;
            }
            return false;
        }
        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (InvalidDates())
                return;
            var fd = FromDateDP.SelectedDate.ToString().Substring(0, 10).Split('.').Reverse();
            var td = ToDateDP.SelectedDate.ToString().Substring(0, 10).Split('.').Reverse();

            string fromDate = string.Join("-", fd.ToArray());
            string toDate = string.Join("-", td.ToArray());
            string currency = CurrencyCB.SelectedItem.ToString();
            string beginFileDate = string.Join("", fromDate.Substring(2).Split('-'));
            string endFileDate = string.Join("", toDate.Substring(2).Split('-'));
            string beginYear = fromDate.Substring(0, 4);
            string endYear = toDate.Substring(0, 4);
            List<string> yearsToDownload = new List<string>();
            List<string> docDirs = new List<string>();
            List<CurrencyInfo> currList = new List<CurrencyInfo>();

            GetYearsToDownload(yearsToDownload, beginYear, endYear);
            if (!GetInfoFromFiles(yearsToDownload, docDirs, beginFileDate, endFileDate))
                return;
            if (!SuccessfullDataRetrieve(currList, yearsToDownload, docDirs, currency))
                return;
            if (currList.Count == 0)
            {
                MessageBox.Show("No data avaiable for this timespan.");
                return;
            }
            DisplayOutput(currency, fromDate, toDate, currList);
        }
        bool GetInfoFromFiles(List<string> yearsToDownload, List<string> docDirs, string beginFileDate, string endFileDate)
        {
            using (var client = new WebClient())
            {
                if (File.Exists("tempfile.txt"))
                    File.Delete("tempfile.txt");
                //Console.WriteLine("Downloading data...");
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
                        MessageBox.Show("Failed to download data.");
                        return false;
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
            return true;
        }
        void GetYearsToDownload(List<string> yearsToDownload, string beginYear, string endYear)
        {
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
        }
        bool SuccessfullDataRetrieve(List<CurrencyInfo> currList, List<string> yearsToDownload, List<string> docDirs, string currency)
        {
            foreach (var year in yearsToDownload)
            {
                string yearSignature = year.Substring(2);
                var yearDocs = docDirs.Where(x => x.Substring(5, 2) == yearSignature);
                foreach (var y in yearDocs)
                {
                    string path = $"http://www.nbp.pl/kursy/xml/{y}.xml";
                    try
                    {
                        XDocument doc = XDocument.Load(path);
                        var d = doc.Root.Elements("pozycja").Single(x => x.Element("kod_waluty").Value == currency);
                        string date = doc.Root.Element("data_publikacji").Value;
                        decimal buyPrice = decimal.Parse(d.Element("kurs_kupna").Value);
                        decimal sellPrice = decimal.Parse(d.Element("kurs_sprzedazy").Value);
                        currList.Add(new CurrencyInfo { date = date, buyPrice = buyPrice, sellPrice = sellPrice });
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to load data.");
                        return false;
                    }
                }
            }
            return true;
        }
        void DisplayOutput(string currency, string fromDate, string toDate, List<CurrencyInfo> currList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Data about {currency} from {fromDate} to {toDate}:\n");
            //sell
            var sortedSell = currList.OrderByDescending(x => x.sellPrice);
            var highestSell = sortedSell.First();
            var lowestSell = sortedSell.Last();
            var allHighestSell = sortedSell.Where(x => x.sellPrice == highestSell.sellPrice);
            var allLowestSell = sortedSell.Where(x => x.sellPrice == lowestSell.sellPrice);
            decimal avgSell = sortedSell.Aggregate(0m, (sum, val) => sum + val.sellPrice) / sortedSell.Count();
            double varianceSell = 0;
            foreach (var cur in sortedSell)
            {
                varianceSell += Math.Pow((double)(cur.sellPrice - avgSell), 2);
            }
            double standardDeviationSell = Math.Sqrt(varianceSell / sortedSell.Count());
            sb.Append($"Average sell exchange rate: {avgSell:C4}\n");
            sb.Append($"Standard deviation for sell exchange rate: {standardDeviationSell:C4}\n");
            sb.Append("Highest sell exchange rate:\n");
            foreach (var a in allHighestSell)
                sb.Append($"{a.date}: {a.sellPrice:C4}\n");
            sb.Append("Lowest sell exchange rate:\n");
            foreach (var a in allLowestSell)
                sb.Append($"{a.date}: {a.sellPrice:C4}\n");
            //buy
            sb.Append(Environment.NewLine);
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
            sb.Append($"Average buy exchange rate: {avgBuy:C4}\n");
            sb.Append($"Standard deviation for buy exchange rate: {standardDeviationBuy:C4}\n");
            sb.Append("Highest buy exchange rate:\n");
            foreach (var a in allHighestBuy)
                sb.Append($"{a.date}: {a.buyPrice:C4}\n");
            sb.Append("Lowest buy exchange rate:\n");
            foreach (var a in allLowestBuy)
                sb.Append($"{a.date}: {a.buyPrice:C4}\n");
            MessageBox.Show(sb.ToString());
        }
        struct CurrencyInfo
        {
            public decimal sellPrice { get; set; }
            public decimal buyPrice { get; set; }
            public string date { get; set; }
        }
        enum Currency { USD, EUR, CHF, GBP };
    }
}
