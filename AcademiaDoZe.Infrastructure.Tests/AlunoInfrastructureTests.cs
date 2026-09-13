using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _alunoRepository;

    public AlunoInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        var obtido = await _alunoRepository.ObterPorId(aluno.Id);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal("henrique", obtido.Nome);
        Assert.Equal("agostinetto", obtido.Endereco.Complemento);
    }

    [Fact]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente() => Assert.Null(await _alunoRepository.ObterPorId(999999));

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.NotEmpty(await _alunoRepository.ObterTodos());
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        var logradouro = await InfrastructureData.CriarLogradouroAsync(_logradouroRepository);
        var atualizado = Aluno.Criar(aluno.Id, "henrique atualizado", aluno.Cpf.Valor, aluno.DataNascimento, aluno.Telefone.Valor, aluno.Email.Valor, logradouro, "101", "agostinetto", aluno.Senha.Valor, aluno.Foto).Value!;
        await _alunoRepository.Atualizar(atualizado);
        Assert.Equal("henrique atualizado", (await _alunoRepository.ObterPorId(aluno.Id))!.Nome);
    }

    [Fact]
    public async Task Aluno_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await InfrastructureData.CriarLogradouroAsync(_logradouroRepository);
        var aluno = Aluno.Criar(999999, "henrique", InfrastructureData.GerarCpf(), new DateOnly(1995, 5, 15), InfrastructureData.GerarTelefone(), InfrastructureData.GerarEmail(), logradouro, "100", "agostinetto", SenhaTeste, Arquivo.Criar([1]).Value!).Value!;
        await Assert.ThrowsAsync<Infrastructure.Exceptions.InfrastructureException>(() => _alunoRepository.Atualizar(aluno));
    }

    [Fact]
    public async Task Aluno_Remover_SucessoEFalha()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.True(await _alunoRepository.Remover(aluno.Id));
        Assert.False(await _alunoRepository.Remover(999999));
    }

    [Fact]
    public async Task Aluno_ObterPorCpf_SucessoENulo()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.Equal(aluno.Id, (await _alunoRepository.ObterPorCpf(aluno.Cpf))!.Id);
        Assert.Null(await _alunoRepository.ObterPorCpf(Cpf.Criar(InfrastructureData.GerarCpf()).Value!));
    }

    [Fact]
    public async Task Aluno_ObterPorEmail_SucessoENulo()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.Equal(aluno.Id, (await _alunoRepository.ObterPorEmail(aluno.Email))!.Id);
        Assert.Null(await _alunoRepository.ObterPorEmail(Email.Criar(InfrastructureData.GerarEmail()).Value!));
    }

    [Fact]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.True(await _alunoRepository.CpfJaExiste(aluno.Cpf));
        Assert.False(await _alunoRepository.CpfJaExiste(aluno.Cpf, aluno.Id));
    }

    [Fact]
    public async Task Aluno_EmailJaExiste_ValidacaoCorreta()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.True(await _alunoRepository.EmailJaExiste(aluno.Email));
        Assert.False(await _alunoRepository.EmailJaExiste(aluno.Email, aluno.Id));
    }

    [Fact]
    public async Task Aluno_ObterPorNome_FiltragemCorreta()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        Assert.Contains(await _alunoRepository.ObterPorNome("henrique"), item => item.Id == aluno.Id);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_SucessoEFalha()
    {
        var aluno = await InfrastructureData.CriarAlunoAsync(_alunoRepository, _logradouroRepository);
        var senha = Senha.Criar($"Nova{SenhaTeste}").Value!;
        Assert.True(await _alunoRepository.TrocarSenha(aluno.Id, senha));
        Assert.Equal(senha.Valor, (await _alunoRepository.ObterPorId(aluno.Id))!.Senha.Valor);
        Assert.False(await _alunoRepository.TrocarSenha(999999, senha));
    }
}
