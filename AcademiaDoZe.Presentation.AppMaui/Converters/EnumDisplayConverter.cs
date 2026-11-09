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
        if (item.GetType().IsDefined(typeof(FlagsAttribute), false))
        {
            var valores = Enum.GetValues(item.GetType()).Cast<Enum>().Where(flag => System.Convert.ToInt64(flag, culture) != 0 && item.HasFlag(flag)).ToList();
            if (valores.Count == 0) return Nome(item);
            return string.Join(", ", valores.Select(Nome));
        }

        return Nome(item);
    }

    private static string Nome(Enum item)
    {
        var member = item.GetType().GetMember(item.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
