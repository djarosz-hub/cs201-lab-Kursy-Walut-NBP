using System;
using System.Collections.Generic;
using System.Linq;
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
        enum Currency { USD, EUR, CHF, GBP };

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            var fd = FromDateDP.SelectedDate.ToString().Substring(0, 10).Split('.').Reverse();
            var td = ToDateDP.SelectedDate.ToString().Substring(0, 10).Split('.').Reverse();
            string fromDate = string.Join("-", fd.ToArray());
            string toDate = string.Join("-", td.ToArray());
            string currency = CurrencyCB.SelectedItem.ToString();
        }
    }
}
