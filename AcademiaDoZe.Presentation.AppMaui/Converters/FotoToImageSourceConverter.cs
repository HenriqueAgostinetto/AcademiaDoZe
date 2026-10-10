// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var conteudo = value switch
        {
            ArquivoDto arquivo => arquivo.Conteudo,
            byte[] bytes => bytes,
            _ => null
        };

        if (conteudo == null || conteudo.Length == 0) return null;
        return ImageSource.FromStream(() => new MemoryStream(conteudo));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
