using GraphSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GraphSimulator.Converter
{
    public class RunModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RunningMode runMode && parameter is string enumValue)
            {
                return Enum.Parse(value.GetType(), enumValue).Equals(runMode);
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string enumValue && value is bool isChecked && isChecked)
            {
                try
                {
                    return Enum.Parse(targetType, enumValue);
                }
                catch (Exception)
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return Binding.DoNothing;
        }
    }
}
