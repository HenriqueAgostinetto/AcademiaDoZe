namespace AcademiaDoZe.Infrastructure.Exceptions;

public sealed class InfrastructureException(string errorCode, string message, Exception? innerException = null) : Exception(message, innerException)
{
    public string ErrorCode { get; } = errorCode;
}
