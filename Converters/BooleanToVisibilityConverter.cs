using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia;

// copilot way of changing the visibility of the element

namespace ApiTester.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return value;
            }
            return false;
        }
    }
}