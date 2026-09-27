// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
	private readonly LogradouroListViewModel _viewModel;

	public LogradouroListPage() : this(MauiProgram.Services.GetRequiredService<LogradouroListViewModel>()) { }

    public LogradouroListPage(LogradouroListViewModel viewModel)
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
