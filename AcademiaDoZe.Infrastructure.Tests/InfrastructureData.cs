using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

internal static class InfrastructureData
{
    public static async Task<Logradouro> CriarLogradouroAsync(LogradouroRepository repository)
    {
        var result = Logradouro.Criar(0, Random.Shared.Next(10000000, 99999999).ToString(), "rua henrique", "centro", "campinas", "SP", "brasil");
        return await repository.Adicionar(result.Value!);
    }

    public static async Task<Aluno> CriarAlunoAsync(AlunoRepository repository, LogradouroRepository logradouroRepository)
    {
        var logradouro = await CriarLogradouroAsync(logradouroRepository);
        var result = Aluno.Criar(0, "henrique", GerarCpf(), new DateOnly(1995, 5, 15), GerarTelefone(), GerarEmail(), logradouro, "100", "agostinetto", TestBase.SenhaTeste, Arquivo.Criar([1, 2, 3]).Value!);
        return await repository.Adicionar(result.Value!);
    }

    public static async Task<Colaborador> CriarColaboradorAsync(ColaboradorRepository repository, LogradouroRepository logradouroRepository)
    {
        var logradouro = await CriarLogradouroAsync(logradouroRepository);
        var result = Colaborador.Criar(0, "henrique", GerarCpf(), new DateOnly(1995, 5, 15), GerarTelefone(), GerarEmail(), logradouro, "200", "agostinetto", TestBase.SenhaTeste, Arquivo.Criar([4, 5, 6]).Value!, new DateOnly(2023, 1, 1), ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT);
        return await repository.Adicionar(result.Value!);
    }

    public static async Task<Matricula> CriarMatriculaAsync(MatriculaRepository repository, AlunoRepository alunoRepository, LogradouroRepository logradouroRepository, MatriculaPlano plano = MatriculaPlano.Mensal, DateOnly? dataInicio = null, MatriculaRestricoes restricoes = MatriculaRestricoes.None, Arquivo? laudo = null)
    {
        var aluno = await CriarAlunoAsync(alunoRepository, logradouroRepository);
        if (restricoes != MatriculaRestricoes.None && laudo == null) laudo = Arquivo.Criar([7, 8, 9]).Value!;
        var result = Matricula.Criar(0, aluno, plano, dataInicio ?? DateOnly.FromDateTime(DateTime.Today), "henrique agostinetto piva", restricoes, laudo, SgbdTeste);
        return await repository.Adicionar(result.Value!);
    }

    public static string SgbdTeste => TestBase.DatabaseType switch
    {
        AcademiaDoZe.Infrastructure.Data.DatabaseType.SqlServer => "SQLServer",
        AcademiaDoZe.Infrastructure.Data.DatabaseType.MySql => "MySQL",
        _ => "SQLite"
    };

    public static string GerarCpf() => Random.Shared.NextInt64(10000000000, 99999999999).ToString();
    public static string GerarTelefone() => "119" + Random.Shared.Next(10000000, 99999999);
    public static string GerarEmail() => $"henrique{Guid.NewGuid():N}@academia.com";
}
