using GraphSimulator.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GraphSimulator.Converter
{
    public class BackNextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v && parameter is string paramStr)
            {
                switch (paramStr)
                {
                    case "Back":
                        {
                            if (v > 0)
                                return true;
                            return false;
                        }
                    case "Next":
                        {
                            if (v < RouteEngine.Instance.Nodes.Count - 1)
                                return true;
                            return false;
                        }
                    default:
                        break;
                }
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
