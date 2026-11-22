// henrique agostinetto piva
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Security;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardViewModel(ILogradouroService logradouroService, IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService, IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService) : ObservableObject
{
    private readonly ILogradouroService _logradouroService = logradouroService;
    private readonly IAlunoService _alunoService = alunoService;
    private readonly IColaboradorService _colaboradorService = colaboradorService;
    private readonly IMatriculaService _matriculaService = matriculaService;
    private readonly IAcessoAlunoService _acessoAlunoService = acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService = acessoColaboradorService;

    public ObservableCollection<DashboardCard> Cards { get; } = [];

    [ObservableProperty] private string titulo = "Painel de controle";
    [ObservableProperty] private string subtitulo = "Acompanhe os principais dados da academia.";
    [ObservableProperty] private string destaque = string.Empty;
    [ObservableProperty] private double alturaCards;
    [ObservableProperty] private bool isBusy;

    public async Task CarregarAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var perfil = Enum.TryParse(Preferences.Get("PerfilSessao", "Aluno"), out AppPerfil valor) ? valor : AppPerfil.Aluno;
            var login = Preferences.Get("NomeSessao", string.Empty);
            Cards.Clear();
            AlturaCards = 0;
            if (perfil == AppPerfil.Aluno)
            {
                var acessosColaborador = await _acessoColaboradorService.ObterTodosAsync();
                var treinos = TreinoAgendaService.ObterAgendaDoAluno(login);
                Titulo = "Dashboard do aluno";
                Subtitulo = "Veja seu treino e quem esta disponivel na academia.";
                Destaque = TreinoDoDia(login, treinos);
                AdicionarCard("Treinos disponiveis", treinos.Count.ToString());
                AdicionarCard("Colaboradores ativos", acessosColaborador.Count(item => item.Saida == null).ToString());
                return;
            }

            var alunos = _alunoService.ObterTodosAsync();
            var colaboradores = _colaboradorService.ObterTodosAsync();
            var acessosAluno = _acessoAlunoService.ObterTodosAsync();
            var acessosColaboradorTodos = _acessoColaboradorService.ObterTodosAsync();
            var matriculas = _matriculaService.ObterAtivasAsync();
            await Task.WhenAll(alunos, colaboradores, acessosAluno, acessosColaboradorTodos, matriculas);
            if (perfil == AppPerfil.Colaborador)
            {
                Titulo = "Dashboard do colaborador";
                Subtitulo = "Acompanhe alunos e acessos da academia.";
                Destaque = $"Colaboradores ativos agora: {acessosColaboradorTodos.Result.Count(item => item.Saida == null)}";
                AdicionarCard("Alunos cadastrados", alunos.Result.Count().ToString());
                AdicionarCard("Colaboradores ativos", acessosColaboradorTodos.Result.Count(item => item.Saida == null).ToString());
                AdicionarCard("Acessos de alunos", acessosAluno.Result.Count().ToString());
                AdicionarCard("Acessos de colaboradores", acessosColaboradorTodos.Result.Count().ToString());
                AdicionarCard("Matriculas ativas", matriculas.Result.Count().ToString());
                return;
            }

            var logradouros = await _logradouroService.ObterTodosAsync();
            Titulo = "Dashboard administrativo";
            Subtitulo = "Visao completa da operacao da academia.";
            Destaque = $"Matriculas ativas: {matriculas.Result.Count()}";
            AdicionarCard("Alunos", alunos.Result.Count().ToString());
            AdicionarCard("Colaboradores", colaboradores.Result.Count().ToString());
            AdicionarCard("Logradouros", logradouros.Count().ToString());
            AdicionarCard("Acessos de alunos", acessosAluno.Result.Count().ToString());
            AdicionarCard("Acessos de colaboradores", acessosColaboradorTodos.Result.Count().ToString());
            AdicionarCard("Matriculas ativas", matriculas.Result.Count().ToString());
        }
        finally { IsBusy = false; }
    }

    private void AdicionarCard(string titulo, string valor)
    {
        Cards.Add(new DashboardCard(titulo, valor));
        var linhas = (Cards.Count + 1) / 2;
        AlturaCards = linhas * 120 - 12;
    }
    private static string TreinoDoDia(string login, List<TreinoAgendaItem> treinos)
    {
        if (treinos.Count == 0) return "Nenhum treino disponivel para hoje.";
        var treino = TreinoAgendaService.ObterTreinoDoDia(login, DateTime.Today.DayOfWeek);
        return treino == null ? "Hoje e dia de descanso. Retome seu treino na proxima sessao." : $"Treino de hoje: {treino.Nome}";
    }
}

public sealed record DashboardCard(string Titulo, string Valor);
