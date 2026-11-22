// henrique agostinetto piva
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class AcessoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _alunoRepository;
    private readonly ColaboradorRepository _colaboradorRepository;
    private readonly AcessoAlunoRepository _acessoAlunoRepository;
    private readonly AcessoColaboradorRepository _acessoColaboradorRepository;

    public AcessoInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
        _colaboradorRepository = new ColaboradorRepository(ConnectionString, DatabaseType);
        _acessoAlunoRepository = new AcessoAlunoRepository(ConnectionString, DatabaseType);
        _acessoColaboradorRepository = new AcessoColaboradorRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task AcessoAluno_EntradaESaida_DeveAtualizarPermanencia()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        var entrada = AcessoAluno.Criar(0, aluno, DateTime.Today.AddHours(10)).Value!;

        var acesso = await _acessoAlunoRepository.Adicionar(entrada);

        Assert.True(await _acessoAlunoRepository.EstaNaAcademia(aluno.Id));

        acesso.RegistrarSaida(DateTime.Today.AddHours(11));
        await _acessoAlunoRepository.Atualizar(acesso);

        var salvo = await _acessoAlunoRepository.ObterPorId(acesso.Id);
        Assert.False(await _acessoAlunoRepository.EstaNaAcademia(aluno.Id));
        Assert.Equal(TimeSpan.FromHours(1), salvo!.Permanencia);
    }

    [Fact]
    public async Task AcessoColaborador_EntradaESaida_DeveSomarHorasDoDia()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        var entrada = AcessoColaborador.Criar(0, colaborador, DateTime.Today.AddHours(10)).Value!;

        var acesso = await _acessoColaboradorRepository.Adicionar(entrada);
        acesso.RegistrarSaida(DateTime.Today.AddHours(12));
        await _acessoColaboradorRepository.Atualizar(acesso);

        Assert.Equal(TimeSpan.FromHours(2), await _acessoColaboradorRepository.ObterHorasTrabalhadasNoDia(colaborador.Id, DateOnly.FromDateTime(DateTime.Today)));
    }
}
