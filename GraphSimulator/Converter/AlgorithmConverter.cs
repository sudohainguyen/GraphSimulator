using GraphSimulator.Helpers.AlgorithmHelpers;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GraphSimulator.Converter
{
    public class AlgorithmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                //try
                //{
                //    return Enum.Parse(targetType, index.ToString());
                //}
                //catch (Exception)
                //{
                //    return Binding.DoNothing;
                //}
                switch (index)
                {
                    case 0:
                        {
                            return new DijsktraAlgorithm();
                        }
                    case 1:
                        {
                            return new BellmanFordAlgorithm();
                        }
                    case 2:
                        {
                            return new PrimAlgorithm();
                        }
                    case 3:
                        {
                            return new KruskalAlgorithm();
                        }
                    default:
                        break;
                }
            }
            return Binding.DoNothing;
        }
    }
}
