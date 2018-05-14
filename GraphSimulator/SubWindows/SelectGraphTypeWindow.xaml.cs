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
using System.Windows.Shapes;

namespace GraphSimulator.SubWindows
{
    /// <summary>
    /// Interaction logic for SelectGraphTypeWindow.xaml
    /// </summary>
    public partial class SelectGraphTypeWindow : Window
    {
        private bool _madeAChoice = false;
        public Action<bool?> Ok { get; set; }
   
        public SelectGraphTypeWindow()
        {
            InitializeComponent();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            _madeAChoice = true;
            Ok(undirected.IsChecked);
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!_madeAChoice)
                Ok(null);
        }
    }
}
