// henrique agostinetto piva
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardViewModel(ILogradouroService service) : ObservableObject
{
    private readonly ILogradouroService _service = service;

    [ObservableProperty]
    private int totalLogradouros;

    public async Task CarregarAsync()
    {
        TotalLogradouros = (await _service.ObterTodosAsync()).Count();
    }
}
