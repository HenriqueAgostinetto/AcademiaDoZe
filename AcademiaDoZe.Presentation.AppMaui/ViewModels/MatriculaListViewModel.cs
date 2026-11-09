using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel(IMatriculaService service) : ObservableObject
{
    private readonly IMatriculaService _service = service;

    public ObservableCollection<string> TiposFiltro { get; } = ["Id", "Id Aluno", "Ativas", "Vencendo em dias"];
    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];

    [ObservableProperty]
    private string pesquisa = string.Empty;

    [ObservableProperty]
    private string filtroSelecionado = "Ativas";

    [ObservableProperty]
    private bool isBusy;

    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IEnumerable<MatriculaDto> itens;
            if (FiltroSelecionado == "Ativas") itens = await _service.ObterAtivasAsync();
            else itens = await _service.ObterTodasAsync();

            await AplicarPesquisaAsync(itens);
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
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
    private Task NovaAsync() => Shell.Current.GoToAsync("matricula");

    [RelayCommand]
    private Task EditarAsync(MatriculaDto item) => Shell.Current.GoToAsync($"matricula?Id={item.Id}");

    [RelayCommand]
    private async Task RemoverAsync(MatriculaDto item)
    {
        if (!await Shell.Current.DisplayAlertAsync("Remover", $"Deseja remover a matricula {item.Id}?", "Sim", "Nao")) return;

        try
        {
            if (await _service.RemoverAsync(item.Id)) await CarregarAsync();
            else await Shell.Current.DisplayAlertAsync("Atencao", "Matricula nao encontrada.", "Ok");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    private async Task AplicarPesquisaAsync(IEnumerable<MatriculaDto> itens)
    {
        var filtro = Pesquisa.Trim();
        if (string.IsNullOrWhiteSpace(filtro))
        {
            Matriculas.Clear();
            foreach (var item in itens) Matriculas.Add(item);
            return;
        }

        IEnumerable<MatriculaDto> resultado = [];
        if (FiltroSelecionado == "Id" && int.TryParse(filtro, out var id))
        {
            var item = await _service.ObterPorIdAsync(id);
            if (item != null) resultado = [item];
        }
        else if (FiltroSelecionado == "Id Aluno" && int.TryParse(filtro, out var alunoId))
            resultado = await _service.ObterPorAlunoIdAsync(alunoId);
        else if (FiltroSelecionado == "Ativas" && int.TryParse(filtro, out var alunoAtivoId))
            resultado = await _service.ObterAtivasAsync(alunoAtivoId);
        else if (FiltroSelecionado == "Vencendo em dias" && int.TryParse(filtro, out var dias))
            resultado = await _service.ObterVencendoEmDiasAsync(dias);
        else if (!int.TryParse(filtro, out _))
            await Shell.Current.DisplayAlertAsync("Atencao", "Informe um numero valido para este filtro.", "Ok");

        Matriculas.Clear();
        foreach (var item in resultado) Matriculas.Add(item);
    }
}
