// henrique agostinetto piva
namespace AcademiaDoZe.Domain.ValueObjects;

public record Endereco(
    Cep Cep,
    string Pais,
    string Estado,
    string Cidade,
    string Bairro,
    string NomeLogradouro,
    string Numero,
    string Complemento
);
