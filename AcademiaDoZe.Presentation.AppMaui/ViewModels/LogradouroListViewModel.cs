// henrique agostinetto piva
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel(ILogradouroService service) : ObservableObject
{
    private readonly ILogradouroService _service = service;

    public ObservableCollection<LogradouroDto> Logradouros { get; } = [];

    [ObservableProperty]
    private string pesquisa = string.Empty;

    public async Task CarregarAsync()
    {
        var itens = await _service.ObterTodosAsync();
        var filtro = Pesquisa.Trim();
        if (!string.IsNullOrWhiteSpace(filtro))
            itens = itens.Where(item => item.Cep.Contains(filtro, StringComparison.OrdinalIgnoreCase) || item.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase) || item.Bairro.Contains(filtro, StringComparison.OrdinalIgnoreCase) || item.Cidade.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        Logradouros.Clear();
        foreach (var item in itens.OrderBy(item => item.Nome)) Logradouros.Add(item);
    }

    [RelayCommand]
    private Task PesquisarAsync() => CarregarAsync();

    [RelayCommand]
    private async Task LimparAsync()
    {
        Pesquisa = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private Task NovoAsync() => Shell.Current.GoToAsync(nameof(LogradouroPage));

    [RelayCommand]
    private Task EditarAsync(LogradouroDto item) => Shell.Current.GoToAsync(nameof(LogradouroPage), new Dictionary<string, object> { ["logradouro"] = item });

    [RelayCommand]
    private async Task RemoverAsync(LogradouroDto item)
    {
		if (!await Shell.Current.DisplayAlertAsync("Remover", $"Deseja remover {item.Nome}?", "Sim", "Nao")) return;
        await _service.RemoverAsync(item.Id);
        await CarregarAsync();
    }
}
