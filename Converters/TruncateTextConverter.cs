using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ApiTester.Converters
{
    public class TruncateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (parameter != null && int.TryParse(parameter.ToString(), out int maxLength) && text.Length > maxLength)
                {
                    return text.Substring(0, maxLength) + "...";
                }
                else if (text.Length > 100) // Default to 100 if no parameter is provided
                {
                    return text.Substring(0, 100) + "...";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}