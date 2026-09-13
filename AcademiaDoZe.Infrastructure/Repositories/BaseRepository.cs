using System.Data.Common;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository : IAsyncDisposable
{
    private readonly string _connectionString;
    protected DatabaseType DatabaseType { get; }

    protected BaseRepository(string connectionString, DatabaseType databaseType)
    {
        _connectionString = connectionString;
        DatabaseType = databaseType;
    }

    protected async Task<DbCommand> CreateCommandAsync(string query, CancellationToken cancellationToken)
    {
        var connection = DbProvider.CreateConnection(_connectionString, DatabaseType);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = query;
        return command;
    }

    protected async Task<T> ExecuteAsync<T>(string query, Func<DbCommand, Task<T>> operation, CancellationToken cancellationToken)
    {
        await using var connection = DbProvider.CreateConnection(_connectionString, DatabaseType);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        return await operation(command);
    }

    protected string FormatInsertQuery(string query) => DatabaseType switch
    {
        DatabaseType.SqlServer => query.Replace(") VALUES", $") OUTPUT INSERTED.{GetIdColumn(query)} VALUES"),
        DatabaseType.MySql => query + "; SELECT LAST_INSERT_ID();",
        _ => query + "; SELECT last_insert_rowid();"
    };

    private static string GetIdColumn(string query)
    {
        if (query.Contains("tb_matricula")) return "id_matricula";
        if (query.Contains("tb_colaborador")) return "id_colaborador";
        if (query.Contains("tb_aluno")) return "id_aluno";
        return "id_logradouro";
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected string GetCurrentDateFunction() => DatabaseType switch
    {
        DatabaseType.SqlServer => "CAST(GETDATE() AS DATE)",
        DatabaseType.MySql => "CURDATE()",
        _ => "DATE('now')"
    };

    protected string GetDateAddDaysExpression(string date, string days) => DatabaseType switch
    {
        DatabaseType.SqlServer => $"DATEADD(day, {days}, {date})",
        DatabaseType.MySql => $"DATE_ADD({date}, INTERVAL {days} DAY)",
        _ => $"DATE({date}, '+' || {days} || ' days')"
    };
}
