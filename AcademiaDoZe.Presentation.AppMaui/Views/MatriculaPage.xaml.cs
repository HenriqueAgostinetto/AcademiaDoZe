using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage, IQueryAttributable
{
    private readonly MatriculaViewModel _viewModel;

    public MatriculaPage() : this(MauiProgram.Services.GetRequiredService<MatriculaViewModel>()) { }

    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.MatriculaId = query.TryGetValue("Id", out var value) && int.TryParse(value?.ToString(), out var id) ? id : 0;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.InicializarAsync();
    }

    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e) => AtualizarDataFim();

    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e) => AtualizarDataFim();

    private void AtualizarDataFim()
    {
        _viewModel.AtualizarDataFim();
        FimDatePicker.Date = _viewModel.Matricula.DataFim.ToDateTime(TimeOnly.MinValue);
    }
}
