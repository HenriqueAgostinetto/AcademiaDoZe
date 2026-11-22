// henrique agostinetto piva
namespace AcademiaDoZe.Presentation.AppMaui.Templates;

public sealed class FlyoutItemTemplateSelector : DataTemplateSelector
{
    public required DataTemplate PadraoTemplate { get; init; }
    public required DataTemplate ContaTemplate { get; init; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        item is FlyoutItem { Title: "Minha conta" } ? ContaTemplate : PadraoTemplate;
}
