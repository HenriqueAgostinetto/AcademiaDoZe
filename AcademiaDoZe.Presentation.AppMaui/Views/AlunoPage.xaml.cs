// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage, IQueryAttributable
{
    private readonly AlunoViewModel _viewModel;

    public AlunoPage() : this(MauiProgram.Services.GetRequiredService<AlunoViewModel>()) { }

    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.Definir(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.Definir(query.TryGetValue("aluno", out var valor) ? valor as AlunoDto : null);
    }
}
