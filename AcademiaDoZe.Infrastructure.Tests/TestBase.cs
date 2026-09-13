using AcademiaDoZe.Infrastructure.Data;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase : IAsyncLifetime
{
    internal static DatabaseType DatabaseType => Environment.GetEnvironmentVariable("ACADEMIA_DATABASE")?.ToUpperInvariant() switch
    {
        "SQLSERVER" => AcademiaDoZe.Infrastructure.Data.DatabaseType.SqlServer,
        "MYSQL" => AcademiaDoZe.Infrastructure.Data.DatabaseType.MySql,
        _ => AcademiaDoZe.Infrastructure.Data.DatabaseType.Sqlite
    };

    protected static string ConnectionString => DatabaseType switch
    {
        AcademiaDoZe.Infrastructure.Data.DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",
        AcademiaDoZe.Infrastructure.Data.DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",
        _ => $"Data Source={Path.Combine(Path.GetTempPath(), "db_academia_do_ze.db")};Cache=Shared;"
    };

    protected string NomeRua => "Henrique";
    protected string NomeBairro => "Agostinetto Piva";
    protected string NomeCidade => DatabaseType switch
    {
        AcademiaDoZe.Infrastructure.Data.DatabaseType.SqlServer => "SQLServer",
        AcademiaDoZe.Infrastructure.Data.DatabaseType.MySql => "MySQL",
        _ => "SQLite"
    };

    public async Task InitializeAsync() => await DbInitializer.InitializeAsync(ConnectionString, DatabaseType);
    public Task DisposeAsync() => Task.CompletedTask;

    protected static string GerarCpf() => Random.Shared.NextInt64(10000000000, 99999999999).ToString();
    protected static string GerarCep() => Random.Shared.Next(10000000, 99999999).ToString();
    protected static string GerarTelefone() => "119" + Random.Shared.Next(10000000, 99999999);
    protected static string GerarEmail() => $"henrique{Guid.NewGuid():N}@academia.com";
    internal static string SenhaTeste => $"Senha{DatabaseType}123";
}
