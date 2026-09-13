using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly ColaboradorRepository _colaboradorRepository;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _colaboradorRepository = new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        var obtido = await _colaboradorRepository.ObterPorId(colaborador.Id);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal("henrique", obtido.Nome);
        Assert.Equal("agostinetto", obtido.Endereco.Complemento);
    }

    [Fact]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente() => Assert.Null(await _colaboradorRepository.ObterPorId(999999));

    [Fact]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.NotEmpty(await _colaboradorRepository.ObterTodos());
    }

    [Fact]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        var logradouro = await InfrastructureData.CriarLogradouroAsync(_logradouroRepository);
        var atualizado = Colaborador.Criar(colaborador.Id, "henrique atualizado", colaborador.Cpf.Valor, colaborador.DataNascimento, colaborador.Telefone.Valor, colaborador.Email.Valor, logradouro, "201", "agostinetto", colaborador.Senha.Valor, colaborador.Foto, colaborador.DataAdmissao, ColaboradorTipo.Administrador, ColaboradorVinculo.CLT).Value!;
        await _colaboradorRepository.Atualizar(atualizado);
        Assert.Equal(ColaboradorTipo.Administrador, (await _colaboradorRepository.ObterPorId(colaborador.Id))!.Tipo);
    }

    [Fact]
    public async Task Colaborador_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await InfrastructureData.CriarLogradouroAsync(_logradouroRepository);
        var colaborador = Colaborador.Criar(999999, "henrique", InfrastructureData.GerarCpf(), new DateOnly(1995, 5, 15), InfrastructureData.GerarTelefone(), InfrastructureData.GerarEmail(), logradouro, "200", "agostinetto", SenhaTeste, Arquivo.Criar([1]).Value!, new DateOnly(2023, 1, 1), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;
        await Assert.ThrowsAsync<Infrastructure.Exceptions.InfrastructureException>(() => _colaboradorRepository.Atualizar(colaborador));
    }

    [Fact]
    public async Task Colaborador_Remover_SucessoEFalha()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.True(await _colaboradorRepository.Remover(colaborador.Id));
        Assert.False(await _colaboradorRepository.Remover(999999));
    }

    [Fact]
    public async Task Colaborador_ObterPorCpf_SucessoENulo()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.Equal(colaborador.Id, (await _colaboradorRepository.ObterPorCpf(colaborador.Cpf))!.Id);
        Assert.Null(await _colaboradorRepository.ObterPorCpf(Cpf.Criar(InfrastructureData.GerarCpf()).Value!));
    }

    [Fact]
    public async Task Colaborador_ObterPorEmail_SucessoENulo()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.Equal(colaborador.Id, (await _colaboradorRepository.ObterPorEmail(colaborador.Email))!.Id);
        Assert.Null(await _colaboradorRepository.ObterPorEmail(Email.Criar(InfrastructureData.GerarEmail()).Value!));
    }

    [Fact]
    public async Task Colaborador_CpfJaExiste_ValidacaoCorreta()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.True(await _colaboradorRepository.CpfJaExiste(colaborador.Cpf));
        Assert.False(await _colaboradorRepository.CpfJaExiste(colaborador.Cpf, colaborador.Id));
    }

    [Fact]
    public async Task Colaborador_EmailJaExiste_ValidacaoCorreta()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.True(await _colaboradorRepository.EmailJaExiste(colaborador.Email));
        Assert.False(await _colaboradorRepository.EmailJaExiste(colaborador.Email, colaborador.Id));
    }

    [Fact]
    public async Task Colaborador_ObterPorTipo_FiltragemCorreta()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.Contains(await _colaboradorRepository.ObterPorTipo(colaborador.Tipo), item => item.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_ObterPorVinculo_FiltragemCorreta()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        Assert.Contains(await _colaboradorRepository.ObterPorVinculo(colaborador.Vinculo), item => item.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_TrocarSenha_SucessoEFalha()
    {
        var colaborador = await InfrastructureData.CriarColaboradorAsync(_colaboradorRepository, _logradouroRepository);
        var senha = Senha.Criar($"Nova{SenhaTeste}").Value!;
        Assert.True(await _colaboradorRepository.TrocarSenha(colaborador.Id, senha));
        Assert.Equal(senha.Valor, (await _colaboradorRepository.ObterPorId(colaborador.Id))!.Senha.Valor);
        Assert.False(await _colaboradorRepository.TrocarSenha(999999, senha));
    }
}
