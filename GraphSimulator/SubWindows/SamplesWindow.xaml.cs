using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphSimulator.SubWindows
{
    /// <summary>
    /// Interaction logic for SamplesWindow.xaml
    /// </summary>
    public partial class SamplesWindow : Window
    {
        private bool _madeAChoice = false;
        public List<dynamic> Data { get; set; }
        public Action<string> PassData { get; set; }

        public SamplesWindow()
        {
            InitializeComponent();
            Data = new List<dynamic>()
            {
                new
                {
                    Data = "dHJ1ZS0tW3siSWQiOiJBIiwiWCI6MTQ0LjI1LCJZIjoyODIuODMzMzMzMzMzMzMzMzd9LHsiSWQiOiJCIiwiWCI6MzMxLjI1LCJZIjoxMDIuODMzMzMzMzMzMzMzMzd9LHsiSWQiOiJDIiwiWCI6MzU3LjI1LCJZIjo0NDYuODMzMzMzMzMzMzMzMzd9LHsiSWQiOiJEIiwiWCI6Njg0LjI1LCJZIjo0NDQuODMzMzMzMzMzMzMzMzd9LHsiSWQiOiJFIiwiWCI6Njc1LjI1LCJZIjoxMjQuODMzMzMzMzMzMzMzMzd9LHsiSWQiOiJGIiwiWCI6OTA3LjI1LCJZIjoyNjQuODMzMzMzMzMzMzMzMzd9XS0tW3siU3RhcnQiOiJBIiwiRGVzdCI6IkIiLCJEaXIiOjEsIkNvc3QiOjI4fSx7IlN0YXJ0IjoiQSIsIkRlc3QiOiJDIiwiRGlyIjoxLCJDb3N0Ijo1fSx7IlN0YXJ0IjoiQiIsIkRlc3QiOiJFIiwiRGlyIjoxLCJDb3N0Ijo1fSx7IlN0YXJ0IjoiQiIsIkRlc3QiOiJEIiwiRGlyIjoxLCJDb3N0IjoxM30seyJTdGFydCI6IkMiLCJEZXN0IjoiRSIsIkRpciI6MSwiQ29zdCI6MTh9LHsiU3RhcnQiOiJDIiwiRGVzdCI6IkQiLCJEaXIiOjEsIkNvc3QiOjJ9LHsiU3RhcnQiOiJEIiwiRGVzdCI6IkUiLCJEaXIiOjEsIkNvc3QiOjZ9LHsiU3RhcnQiOiJFIiwiRGVzdCI6IkYiLCJEaXIiOjEsIkNvc3QiOjE5fSx7IlN0YXJ0IjoiRCIsIkRlc3QiOiJGIiwiRGlyIjoxLCJDb3N0Ijo0fV0=",
                    ImgSrc = "../Sample Graphs/sample1.png",
                    Title = "Sample 1"
                },
                new
                {
                    Data = "dHJ1ZS0tW3siSWQiOiJBIiwiWCI6NDA3LjI1LCJZIjo0Mi4zMDc2OTIzMDc2OTIyNjR9LHsiSWQiOiJCIiwiWCI6Mjc0LjI1LCJZIjo0NTIuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJDIiwiWCI6NTc0LjI1LCJZIjo0NDcuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJEIiwiWCI6MTYyLjI1LCJZIjoxODEuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJFIiwiWCI6Njc5LjI1LCJZIjoxNzQuMzA3NjkyMzA3NjkyMjZ9XS0tW3siU3RhcnQiOiJBIiwiRGVzdCI6IkQiLCJEaXIiOjEsIkNvc3QiOjIxfSx7IlN0YXJ0IjoiRCIsIkRlc3QiOiJCIiwiRGlyIjoxLCJDb3N0IjoxM30seyJTdGFydCI6IkIiLCJEZXN0IjoiRSIsIkRpciI6MSwiQ29zdCI6NH0seyJTdGFydCI6IkEiLCJEZXN0IjoiQyIsIkRpciI6MSwiQ29zdCI6MjF9LHsiU3RhcnQiOiJCIiwiRGVzdCI6IkEiLCJEaXIiOjEsIkNvc3QiOjIyfSx7IlN0YXJ0IjoiRCIsIkRlc3QiOiJFIiwiRGlyIjoxLCJDb3N0Ijo5fSx7IlN0YXJ0IjoiQSIsIkRlc3QiOiJFIiwiRGlyIjoxLCJDb3N0IjoxMH0seyJTdGFydCI6IkMiLCJEZXN0IjoiRSIsIkRpciI6MSwiQ29zdCI6MjB9LHsiU3RhcnQiOiJDIiwiRGVzdCI6IkIiLCJEaXIiOjEsIkNvc3QiOjN9LHsiU3RhcnQiOiJEIiwiRGVzdCI6IkMiLCJEaXIiOjEsIkNvc3QiOjI0fV0=",
                    ImgSrc = "../Sample Graphs/sample2.png",
                    Title = "Sample 2"
                },
                new
                {
                    Data = "dHJ1ZS0tW3siSWQiOiJBIiwiWCI6MjM4LjI1LCJZIjoxMTUuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJCIiwiWCI6Mjc1LjI1LCJZIjoyNTkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJDIiwiWCI6NDQyLjI1LCJZIjoxNjkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJEIiwiWCI6NDEwLjI1LCJZIjo0MDkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJFIiwiWCI6NjU1LjI1LCJZIjoyNTkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJGIiwiWCI6NjgxLjI1LCJZIjo0MTMuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJHIiwiWCI6ODYwLjI1LCJZIjozMzEuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJIIiwiWCI6ODc0LjI1LCJZIjoxODUuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJJIiwiWCI6NzE3LjI1LCJZIjoxMDMuMzA3NjkyMzA3NjkyMjZ9XS0tW3siU3RhcnQiOiJBIiwiRGVzdCI6IkIiLCJEaXIiOjEsIkNvc3QiOjE1fSx7IlN0YXJ0IjoiQyIsIkRlc3QiOiJCIiwiRGlyIjoxLCJDb3N0IjoxfSx7IlN0YXJ0IjoiQSIsIkRlc3QiOiJDIiwiRGlyIjoxLCJDb3N0IjoyOX0seyJTdGFydCI6IkMiLCJEZXN0IjoiSSIsIkRpciI6MSwiQ29zdCI6MTV9LHsiU3RhcnQiOiJJIiwiRGVzdCI6IkUiLCJEaXIiOjEsIkNvc3QiOjIxfSx7IlN0YXJ0IjoiRSIsIkRlc3QiOiJIIiwiRGlyIjoxLCJDb3N0IjoxOH0seyJTdGFydCI6IkMiLCJEZXN0IjoiRSIsIkRpciI6MSwiQ29zdCI6NX0seyJTdGFydCI6IkIiLCJEZXN0IjoiRCIsIkRpciI6MSwiQ29zdCI6MTh9LHsiU3RhcnQiOiJEIiwiRGVzdCI6IkUiLCJEaXIiOjEsIkNvc3QiOjd9LHsiU3RhcnQiOiJCIiwiRGVzdCI6IkUiLCJEaXIiOjEsIkNvc3QiOjF9LHsiU3RhcnQiOiJFIiwiRGVzdCI6IkYiLCJEaXIiOjEsIkNvc3QiOjE3fSx7IlN0YXJ0IjoiRiIsIkRlc3QiOiJHIiwiRGlyIjoxLCJDb3N0IjoyOH0seyJTdGFydCI6IkciLCJEZXN0IjoiSCIsIkRpciI6MSwiQ29zdCI6NH0seyJTdGFydCI6IkkiLCJEZXN0IjoiSCIsIkRpciI6MSwiQ29zdCI6MjN9LHsiU3RhcnQiOiJFIiwiRGVzdCI6IkciLCJEaXIiOjEsIkNvc3QiOjEwfSx7IlN0YXJ0IjoiRiIsIkRlc3QiOiJEIiwiRGlyIjoxLCJDb3N0IjoyNH1d",
                    ImgSrc = "../Sample Graphs/sample3.png",
                    Title = "Sample 3"
                },
                new
                {
                    Data = "dHJ1ZS0tW3siSWQiOiJBIiwiWCI6MTczLjI1LCJZIjoxNzIuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJCIiwiWCI6MzQyLjI1LCJZIjoxNzEuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJDIiwiWCI6NTE5LjI1LCJZIjoxNzEuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJEIiwiWCI6NjkzLjI1LCJZIjoxNjguMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJFIiwiWCI6ODU1LjI1LCJZIjoxNjUuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJGIiwiWCI6MzQ0LjI1LCJZIjozMTYuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJHIiwiWCI6NTI2LjI1LCJZIjozMTUuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJIIiwiWCI6NzAxLjI1LCJZIjozMDkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJJIiwiWCI6MTc4LjI1LCJZIjozMTkuMzA3NjkyMzA3NjkyMjZ9LHsiSWQiOiJKIiwiWCI6ODY2LjI1LCJZIjozMDguMzA3NjkyMzA3NjkyMjZ9XS0tW3siU3RhcnQiOiJBIiwiRGVzdCI6IkIiLCJEaXIiOjEsIkNvc3QiOjd9LHsiU3RhcnQiOiJCIiwiRGVzdCI6IkMiLCJEaXIiOjEsIkNvc3QiOjE2fSx7IlN0YXJ0IjoiQyIsIkRlc3QiOiJEIiwiRGlyIjoxLCJDb3N0IjoxMn0seyJTdGFydCI6IkQiLCJEZXN0IjoiRSIsIkRpciI6MSwiQ29zdCI6Mjd9LHsiU3RhcnQiOiJCIiwiRGVzdCI6IkYiLCJEaXIiOjEsIkNvc3QiOjd9LHsiU3RhcnQiOiJGIiwiRGVzdCI6IkkiLCJEaXIiOjEsIkNvc3QiOjI4fSx7IlN0YXJ0IjoiQiIsIkRlc3QiOiJJIiwiRGlyIjoxLCJDb3N0IjoxNn0seyJTdGFydCI6IkIiLCJEZXN0IjoiRyIsIkRpciI6MSwiQ29zdCI6MTZ9LHsiU3RhcnQiOiJGIiwiRGVzdCI6IkciLCJEaXIiOjEsIkNvc3QiOjd9LHsiU3RhcnQiOiJDIiwiRGVzdCI6IkciLCJEaXIiOjEsIkNvc3QiOjR9LHsiU3RhcnQiOiJDIiwiRGVzdCI6IkgiLCJEaXIiOjEsIkNvc3QiOjI3fSx7IlN0YXJ0IjoiRyIsIkRlc3QiOiJIIiwiRGlyIjoxLCJDb3N0IjoyfSx7IlN0YXJ0IjoiRCIsIkRlc3QiOiJIIiwiRGlyIjoxLCJDb3N0IjoxOH0seyJTdGFydCI6IkgiLCJEZXN0IjoiSiIsIkRpciI6MSwiQ29zdCI6MTB9LHsiU3RhcnQiOiJEIiwiRGVzdCI6IkoiLCJEaXIiOjEsIkNvc3QiOjEwfV0=",
                    ImgSrc = "../Sample Graphs/sample4.png",
                    Title = "Sample 4"
                }
            };
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            _madeAChoice = true;
            for (int i = 0; i < itemsContrl.Items.Count; i++)
            {
                var c = (ContentPresenter)itemsContrl.ItemContainerGenerator.ContainerFromItem(itemsContrl.Items[i]);
                var rBtn = c.ContentTemplate.FindName("rbtnSample", c) as RadioButton;
                if (rBtn.IsChecked.HasValue && rBtn.IsChecked.Value == true)
                {
                    PassData((string)rBtn.Tag);
                    break;
                }
            }
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!_madeAChoice)
                PassData(null);
        }
    }
}
