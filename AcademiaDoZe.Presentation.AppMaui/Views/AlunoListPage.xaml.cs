// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    private readonly AlunoListViewModel _viewModel;

    public AlunoListPage() : this(MauiProgram.Services.GetRequiredService<AlunoListViewModel>()) { }

    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.CarregarAsync();
    }
}
