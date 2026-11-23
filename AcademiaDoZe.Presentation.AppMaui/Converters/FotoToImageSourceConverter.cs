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

        if (conteudo == null || !TemFormatoImagem(conteudo)) return null;
        return ImageSource.FromStream(() => new MemoryStream(conteudo));
    }

    private static bool TemFormatoImagem(byte[] conteudo) =>
        conteudo.Length >= 3 &&
        ((conteudo[0] == 0xFF && conteudo[1] == 0xD8 && conteudo[2] == 0xFF) ||
         (conteudo.Length >= 8 && conteudo[0] == 0x89 && conteudo[1] == 0x50 && conteudo[2] == 0x4E && conteudo[3] == 0x47));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
