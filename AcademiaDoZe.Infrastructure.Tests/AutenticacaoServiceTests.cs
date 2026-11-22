// henrique agostinetto piva
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Application.Enums;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class AutenticacaoServiceTests
{
    [Theory]
    [InlineData("Henrique", "Administrador")]
    [InlineData("Henrique 1", "Colaborador")]
    [InlineData("Henrique 2", "Aluno")]
    public void Autenticar_ComCredenciaisConfiguradas_RetornaPerfil(string login, string perfil)
    {
        var sessao = AutenticacaoService.Autenticar(login, "123456");

        Assert.NotNull(sessao);
        Assert.Equal(perfil, sessao.Perfil.ToString());
    }

    [Fact]
    public void Autenticar_ComSenhaInvalida_NaoIniciaSessao()
    {
        Assert.Null(AutenticacaoService.Autenticar("Henrique", "errada"));
    }

    [Theory]
    [InlineData(AppPerfil.Administrador, "Henrique", "Henrique 2", "Henrique 1", true)]
    [InlineData(AppPerfil.Colaborador, "Henrique 1", "Henrique 2", "Henrique 1", true)]
    [InlineData(AppPerfil.Colaborador, "Henrique 1", "Henrique 2", "Henrique", false)]
    [InlineData(AppPerfil.Aluno, "Henrique 2", "Henrique 2", "Henrique 1", true)]
    [InlineData(AppPerfil.Aluno, "Henrique 2", "Henrique", "Henrique 1", false)]
    public void PodeVisualizarTreino_AplicaRegraDoPerfil(AppPerfil perfil, string loginAtual, string alunoLogin, string personalLogin, bool esperado)
    {
        var resultado = TreinoPermissaoService.PodeVisualizar(perfil, loginAtual, alunoLogin, personalLogin);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ObterAgendaDoAluno_RetornaTreinosDaSemanaParaOAlunoLogado()
    {
        var treinos = TreinoAgendaService.ObterAgendaDoAluno("Henrique 2");

        Assert.Equal(6, treinos.Count);
        Assert.Contains(treinos, item => item.DiaSemana == DayOfWeek.Monday && item.Nome == "Adaptacao e mobilidade");
        Assert.All(treinos, item => Assert.Equal("Henrique 2", item.AlunoLogin));
    }

    [Fact]
    public void ObterTreinoDoDia_RetornaOTreinoCorrespondenteAoDia()
    {
        var treino = TreinoAgendaService.ObterTreinoDoDia("Henrique 2", DayOfWeek.Tuesday);

        Assert.NotNull(treino);
        Assert.Equal("Cardio e condicionamento", treino.Nome);
    }
}
