using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GraphSimulator.Converter
{
    public class PlayButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool ispause)
            {
                var myResourceDictionary = new ResourceDictionary
                {
                    Source =
                    new Uri("./Styles/Icons.xaml", UriKind.RelativeOrAbsolute)
                };
                return ispause ? myResourceDictionary["Play"] : myResourceDictionary["Pause"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
