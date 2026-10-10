// henrique agostinetto piva
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class EnumDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum item) return string.Empty;
        var member = item.GetType().GetMember(item.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
