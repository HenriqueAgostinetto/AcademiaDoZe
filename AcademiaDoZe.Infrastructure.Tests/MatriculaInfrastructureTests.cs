using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _alunoRepository;
    private readonly MatriculaRepository _matriculaRepository;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
        _matriculaRepository = new MatriculaRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository, restricoes: MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta);
        var obtida = await _matriculaRepository.ObterPorId(matricula.Id);
        Assert.NotNull(obtida);
        Assert.Equal(matricula.Id, obtida.Id);
        Assert.Equal("henrique agostinetto piva", obtida.Objetivo);
        Assert.Equal(InfrastructureData.SgbdTeste, obtida.ObservacoesRestricoes);
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.NotNull(obtida.LaudoMedico);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente() => Assert.Null(await _matriculaRepository.ObterPorId(999999));

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        Assert.Contains(await _matriculaRepository.ObterTodos(), item => item.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        var aluno = await _alunoRepository.ObterPorId(matricula.AlunoId);
        var atualizada = Matricula.Criar(matricula.Id, aluno!, MatriculaPlano.Anual, matricula.DataInicio, "henrique agostinetto piva", MatriculaRestricoes.Alergias, Arquivo.Criar([9, 8, 7]).Value!, InfrastructureData.SgbdTeste).Value!;
        await _matriculaRepository.Atualizar(atualizada);
        var obtida = await _matriculaRepository.ObterPorId(matricula.Id);
        Assert.Equal(MatriculaPlano.Anual, obtida!.Plano);
        Assert.Equal(MatriculaRestricoes.Alergias, obtida.RestricoesMedicas);
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        var matricula = Matricula.Criar(999999, aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "henrique agostinetto piva", MatriculaRestricoes.None, null, InfrastructureData.SgbdTeste).Value!;
        var exception = await Assert.ThrowsAsync<InfrastructureException>(() => _matriculaRepository.Atualizar(matricula));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", exception.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Remover_SucessoEFalha()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        Assert.True(await _matriculaRepository.Remover(matricula.Id));
        Assert.False(await _matriculaRepository.Remover(999999));
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        var resultado = await _matriculaRepository.ObterPorAluno(matricula.AlunoId);
        Assert.Contains(resultado, item => item.Id == matricula.Id && item.AlunoId == matricula.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        Assert.True(await _matriculaRepository.PossuiMatriculaAtiva(matricula.AlunoId));
        Assert.Equal(matricula.Id, (await _matriculaRepository.ObterMatriculaAtivaPorAluno(matricula.AlunoId))!.Id);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository);
        Assert.Contains(await _matriculaRepository.ObterAtivas(), item => item.Id == matricula.Id);
        Assert.Contains(await _matriculaRepository.ObterAtivas(matricula.AlunoId), item => item.Id == matricula.Id && item.AlunoId == matricula.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculaProxima()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository, dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(-25)));
        Assert.Contains(await _matriculaRepository.ObterVencendoEmDias(30), item => item.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var matricula = await InfrastructureData.CriarMatriculaAsync(_matriculaRepository, _alunoRepository, _logradouroRepository, MatriculaPlano.Trimestral);
        Assert.Contains(await _matriculaRepository.ObterPorPlano(MatriculaPlano.Trimestral), item => item.Id == matricula.Id && item.Plano == MatriculaPlano.Trimestral);
    }
}
