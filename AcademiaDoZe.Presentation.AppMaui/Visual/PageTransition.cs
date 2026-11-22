// henrique agostinetto piva
namespace AcademiaDoZe.Presentation.AppMaui.Visual;

public static class PageTransition
{
    public static async Task AparecerAsync(VisualElement element)
    {
        if (element.AnimationIsRunning("entrada")) return;
        element.Opacity = 0;
        element.TranslationY = 10;
        await Task.WhenAll(element.FadeToAsync(1, 180, Easing.CubicOut), element.TranslateToAsync(0, 0, 180, Easing.CubicOut));
    }
}
