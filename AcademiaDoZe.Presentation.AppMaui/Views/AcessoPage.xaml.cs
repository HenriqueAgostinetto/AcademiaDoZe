// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Visual;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    private readonly AcessoViewModel _viewModel;

    public AcessoPage() : this(MauiProgram.Services.GetRequiredService<AcessoViewModel>()) { }

    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.CarregarAsync();
        _ = PageTransition.AparecerAsync(Conteudo);
    }
}
