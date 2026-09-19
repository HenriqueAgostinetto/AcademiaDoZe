// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro item) => new() { Id = item.Id, Cep = item.Cep.Valor, Nome = item.Nome, Bairro = item.Bairro, Cidade = item.Cidade, Estado = item.Estado, Pais = item.Pais };
    public static Logradouro ToEntity(this LogradouroDto item) => Result(Logradouro.Criar(item.Id, item.Cep, item.Nome, item.Bairro, item.Cidade, item.Estado, item.Pais));
    public static AlunoDto ToDto(this Aluno item, Logradouro? logradouro = null) => new() { Id = item.Id, Nome = item.Nome, Cpf = item.Cpf.Valor, DataNascimento = item.DataNascimento, Telefone = item.Telefone.Valor, Email = item.Email.Valor, Endereco = logradouro?.ToDto(), Numero = item.Endereco.Numero, Complemento = item.Endereco.Complemento, Foto = new ArquivoDto { Conteudo = item.Foto.Conteudo } };
    public static ColaboradorDto ToDto(this Colaborador item, Logradouro? logradouro = null) => new() { Id = item.Id, Nome = item.Nome, Cpf = item.Cpf.Valor, DataNascimento = item.DataNascimento, Telefone = item.Telefone.Valor, Email = item.Email.Valor, Endereco = logradouro?.ToDto(), Numero = item.Endereco.Numero, Complemento = item.Endereco.Complemento, Foto = new ArquivoDto { Conteudo = item.Foto.Conteudo }, DataAdmissao = item.DataAdmissao, Tipo = (AppColaboradorTipo)item.Tipo, Vinculo = (AppColaboradorVinculo)item.Vinculo };
    public static MatriculaDto ToDto(this Matricula item, AlunoDto aluno) => new() { Id = item.Id, AlunoMatricula = aluno, Plano = (AppMatriculaPlano)item.Plano, DataInicio = item.DataInicio, DataFim = item.DataFim, Objetivo = item.Objetivo, RestricoesMedicas = (AppMatriculaRestricoes)item.RestricoesMedicas, ObservacoesRestricoes = item.ObservacoesRestricoes, LaudoMedico = item.LaudoMedico == null ? null : new ArquivoDto { Conteudo = item.LaudoMedico.Conteudo } };
    public static Aluno ToEntity(this AlunoDto item, Logradouro endereco, string? senha = null) => Result(Aluno.Criar(item.Id, item.Nome, item.Cpf, item.DataNascimento, item.Telefone, item.Email ?? string.Empty, endereco, item.Numero, item.Complemento, senha ?? item.Senha ?? string.Empty, Arquivo(item.Foto)));
    public static Colaborador ToEntity(this ColaboradorDto item, Logradouro endereco, string? senha = null) => Result(Colaborador.Criar(item.Id, item.Nome, item.Cpf, item.DataNascimento, item.Telefone, item.Email ?? string.Empty, endereco, item.Numero, item.Complemento, senha ?? item.Senha ?? string.Empty, Arquivo(item.Foto), item.DataAdmissao, (ColaboradorTipo)item.Tipo, (ColaboradorVinculo)item.Vinculo));
    public static Matricula ToEntity(this MatriculaDto item, Aluno aluno, Arquivo? laudo = null) => Result(Matricula.Criar(item.Id, aluno, (MatriculaPlano)item.Plano, item.DataInicio, item.Objetivo, (MatriculaRestricoes)item.RestricoesMedicas, laudo ?? Arquivo(item.LaudoMedico), item.ObservacoesRestricoes ?? string.Empty));
    private static Arquivo? Arquivo(ArquivoDto? item) => item == null ? null : Result(Domain.ValueObjects.Arquivo.Criar(item.Conteudo));
    private static T Result<T>(Domain.Common.Result<T> result) => result.IsSuccess ? result.Value! : throw new InvalidOperationException(string.Join(", ", result.Notifications.Select(item => item.Mensagem)));
}
