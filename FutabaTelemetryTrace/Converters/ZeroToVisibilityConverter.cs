using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FutabaTelemetryTrace.Converters;

/// <summary>
/// Converts 0 to Visible, non-zero to Collapsed
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}