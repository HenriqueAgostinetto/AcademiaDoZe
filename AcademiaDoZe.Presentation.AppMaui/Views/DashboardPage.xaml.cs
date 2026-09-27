// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardPage : ContentPage
{
	private readonly DashboardViewModel _viewModel;

	public DashboardPage() : this(MauiProgram.Services.GetRequiredService<DashboardViewModel>()) { }

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.CarregarAsync();
    }

	private async void GerenciarClicked(object? sender, EventArgs eventArgs) => await Shell.Current.GoToAsync("//logradouros");
}
