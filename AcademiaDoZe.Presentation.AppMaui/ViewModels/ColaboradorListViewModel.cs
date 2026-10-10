// henrique agostinetto piva
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel(IColaboradorService service) : ObservableObject
{
    private readonly IColaboradorService _service = service;

    public ObservableCollection<ColaboradorDto> Colaboradores { get; } = [];

    [ObservableProperty]
    private string pesquisa = string.Empty;

    public async Task CarregarAsync()
    {
        var itens = await _service.ObterTodosAsync();
        var filtro = Pesquisa.Trim();

        if (!string.IsNullOrWhiteSpace(filtro))
            itens = itens.Where(item => item.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase) || item.Cpf.Contains(filtro, StringComparison.OrdinalIgnoreCase) || (item.Email?.Contains(filtro, StringComparison.OrdinalIgnoreCase) ?? false) || item.Tipo.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase) || item.Vinculo.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase));

        Colaboradores.Clear();
        foreach (var item in itens.OrderBy(item => item.Nome)) Colaboradores.Add(item);
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
    private Task NovoAsync() => Shell.Current.GoToAsync(nameof(ColaboradorPage));

    [RelayCommand]
    private async Task EditarAsync(ColaboradorDto item)
    {
        var colaborador = await _service.ObterPorIdAsync(item.Id);
        if (colaborador == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Colaborador nao encontrado.", "Ok");
            await CarregarAsync();
            return;
        }

        await Shell.Current.GoToAsync(nameof(ColaboradorPage), new Dictionary<string, object> { ["colaborador"] = colaborador });
    }

    [RelayCommand]
    private async Task RemoverAsync(ColaboradorDto item)
    {
        if (!await Shell.Current.DisplayAlertAsync("Remover", $"Deseja remover {item.Nome}?", "Sim", "Nao")) return;
        await _service.RemoverAsync(item.Id);
        await CarregarAsync();
    }
}
