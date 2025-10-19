namespace ApiTester.Converters
{
    using System;
    using Avalonia.Data.Converters;
    using System.Globalization;

    public class EqualityToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null)
            {
                return parameter.ToString() ?? string.Empty;
            }
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
