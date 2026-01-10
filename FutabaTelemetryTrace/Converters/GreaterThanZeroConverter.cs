using System.Globalization;
using System.Windows.Data;

namespace FutabaTelemetryTrace.Converters;

/// <summary>
/// Converts value > 0 to true, 0 to false
/// </summary>
public class GreaterThanZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d > 0;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}