// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage, IQueryAttributable
{
    private readonly ColaboradorViewModel _viewModel;

    public ColaboradorPage() : this(MauiProgram.Services.GetRequiredService<ColaboradorViewModel>()) { }

    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.Definir(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.Definir(query.TryGetValue("colaborador", out var valor) ? valor as ColaboradorDto : null);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarLogradourosAsync();
    }
}
