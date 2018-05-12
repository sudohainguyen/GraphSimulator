using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphSimulator.SubWindows
{
    /// <summary>
    /// Interaction logic for CostInputWindow.xaml
    /// </summary>
    public partial class CostInputWindow : Window
    {
        private int _value = -1;

        public Action<int> Ok { get; set; }
        public CostInputWindow()
        {
            InitializeComponent();
        }

        private void txbCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex(@"\d+(.\d)?"); //regex that matches disallowed text
            return regex.IsMatch(text);
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txbCost.Text, out _value))
            {
                Ok(_value);
                this.Close();
            }
            else
            {
                MessageBox.Show("Something wrong happens, please try again");
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_value == -1)
                Ok(-1);
        }
    }
}
