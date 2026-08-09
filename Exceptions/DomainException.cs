// henrique agostinetto piva
namespace AcademiaDoZe.Domain.Exceptions;

// excecao de dominio para falhas irrecuperaveis
public sealed class DomainException(string message) : Exception(message);
