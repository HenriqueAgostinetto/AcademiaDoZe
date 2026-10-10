// henrique agostinetto piva
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateOnly data) return data.ToDateTime(TimeOnly.MinValue);
        return DateTime.Today;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime data) return DateOnly.FromDateTime(data);
        return DateOnly.FromDateTime(DateTime.Today);
    }
}
