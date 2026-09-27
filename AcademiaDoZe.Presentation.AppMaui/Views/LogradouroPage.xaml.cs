// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage, IQueryAttributable
{
	private readonly LogradouroFormViewModel _viewModel;

	public LogradouroPage() : this(MauiProgram.Services.GetRequiredService<LogradouroFormViewModel>()) { }

    public LogradouroPage(LogradouroFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.Definir(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.Definir(query.TryGetValue("logradouro", out var valor) ? valor as LogradouroDto : null);
    }
}
