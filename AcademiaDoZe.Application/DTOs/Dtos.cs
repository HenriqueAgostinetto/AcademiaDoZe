// henrique agostinetto piva
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class ArquivoDto { public required byte[] Conteudo { get; set; } }
public class LogradouroDto { public int Id { get; set; } public required string Cep { get; set; } public required string Nome { get; set; } public required string Bairro { get; set; } public required string Cidade { get; set; } public required string Estado { get; set; } public required string Pais { get; set; } }
public abstract class PessoaDto { public int Id { get; set; } public required string Nome { get; set; } public required string Cpf { get; set; } public required DateOnly DataNascimento { get; set; } public required string Telefone { get; set; } public string? Email { get; set; } public LogradouroDto? Endereco { get; set; } public required string Numero { get; set; } public string? Complemento { get; set; } public string? Senha { get; set; } public ArquivoDto? Foto { get; set; } }
public class AlunoDto : PessoaDto { }
public class ColaboradorDto : PessoaDto { public required DateOnly DataAdmissao { get; set; } public required AppColaboradorTipo Tipo { get; set; } public required AppColaboradorVinculo Vinculo { get; set; } }
public class MatriculaDto { public int Id { get; set; } public required AlunoDto AlunoMatricula { get; set; } public required AppMatriculaPlano Plano { get; set; } public required DateOnly DataInicio { get; set; } public DateOnly DataFim { get; set; } public required string Objetivo { get; set; } public required AppMatriculaRestricoes RestricoesMedicas { get; set; } public string? ObservacoesRestricoes { get; set; } public ArquivoDto? LaudoMedico { get; set; } }
