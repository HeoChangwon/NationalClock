using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NationalClock;

/// <summary>
/// Converter that converts null values to Visibility.Collapsed and non-null values to Visibility.Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || (value is string str && string.IsNullOrEmpty(str)))
        {
            return Visibility.Collapsed;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}