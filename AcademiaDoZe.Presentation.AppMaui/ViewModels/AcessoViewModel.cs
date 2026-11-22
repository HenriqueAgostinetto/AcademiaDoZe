// henrique agostinetto piva
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel(IAlunoService alunoService, IColaboradorService colaboradorService, IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService) : ObservableObject
{
    private readonly IAlunoService _alunoService = alunoService;
    private readonly IColaboradorService _colaboradorService = colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService = acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService = acessoColaboradorService;

    public ObservableCollection<AlunoDto> Alunos { get; } = [];
    public ObservableCollection<ColaboradorDto> Colaboradores { get; } = [];
    public ObservableCollection<AcessoAlunoDto> AcessosAluno { get; } = [];
    public ObservableCollection<AcessoColaboradorDto> AcessosColaborador { get; } = [];

    [ObservableProperty]
    private AlunoDto? alunoSelecionado;

    [ObservableProperty]
    private ColaboradorDto? colaboradorSelecionado;

    [ObservableProperty]
    private bool isBusy;

    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var alunoId = AlunoSelecionado?.Id;
            var colaboradorId = ColaboradorSelecionado?.Id;
            var alunos = await _alunoService.ObterTodosAsync();
            var colaboradores = await _colaboradorService.ObterTodosAsync();
            var acessosAluno = await _acessoAlunoService.ObterTodosAsync();
            var acessosColaborador = await _acessoColaboradorService.ObterTodosAsync();

            AtualizarColecao(Alunos, alunos.OrderBy(item => item.Nome));
            AtualizarColecao(Colaboradores, colaboradores.OrderBy(item => item.Nome));
            AtualizarColecao(AcessosAluno, acessosAluno.Take(8));
            AtualizarColecao(AcessosColaborador, acessosColaborador.Take(8));
            AlunoSelecionado = Alunos.FirstOrDefault(item => item.Id == alunoId);
            ColaboradorSelecionado = Colaboradores.FirstOrDefault(item => item.Id == colaboradorId);
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
    private async Task RegistrarEntradaAlunoAsync()
    {
        if (AlunoSelecionado == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione um aluno.", "Ok");
            return;
        }

        try
        {
            var acesso = await _acessoAlunoService.RegistrarEntradaAsync(AlunoSelecionado.Id);
            await Shell.Current.DisplayAlertAsync("Entrada registrada", $"{acesso.Aluno.Nome} possui {acesso.DiasRestantesPlano} dia(s) restantes no plano.", "Ok");
            await CarregarAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private async Task RegistrarSaidaAlunoAsync()
    {
        if (AlunoSelecionado == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione um aluno.", "Ok");
            return;
        }

        try
        {
            var acesso = await _acessoAlunoService.RegistrarSaidaAsync(AlunoSelecionado.Id);
            await Shell.Current.DisplayAlertAsync("Saida registrada", $"Tempo na academia: {FormatarTempo(acesso.Permanencia)}.", "Ok");
            await CarregarAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private async Task RegistrarEntradaColaboradorAsync()
    {
        if (ColaboradorSelecionado == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione um colaborador.", "Ok");
            return;
        }

        try
        {
            var acesso = await _acessoColaboradorService.RegistrarEntradaAsync(ColaboradorSelecionado.Id);
            await Shell.Current.DisplayAlertAsync("Entrada registrada", $"Limite diario: {FormatarTempo(acesso.LimiteDiario)}.", "Ok");
            await CarregarAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private async Task RegistrarSaidaColaboradorAsync()
    {
        if (ColaboradorSelecionado == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione um colaborador.", "Ok");
            return;
        }

        try
        {
            var acesso = await _acessoColaboradorService.RegistrarSaidaAsync(ColaboradorSelecionado.Id);
            await Shell.Current.DisplayAlertAsync("Saida registrada", $"Tempo na academia: {FormatarTempo(acesso.Permanencia)}. Total do dia: {FormatarTempo(acesso.HorasTrabalhadasNoDia)}.", "Ok");
            await CarregarAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    private static void AtualizarColecao<T>(ObservableCollection<T> destino, IEnumerable<T> origem)
    {
        destino.Clear();
        foreach (var item in origem) destino.Add(item);
    }

    public static string FormatarTempo(TimeSpan tempo) => $"{(int)tempo.TotalHours:00}:{tempo.Minutes:00}";
}
