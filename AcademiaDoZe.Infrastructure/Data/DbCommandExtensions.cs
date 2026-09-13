using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbCommandExtensions
{
    public static void AddParameter(this DbCommand command, string name, object? value, DbType dbType)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        parameter.DbType = dbType;
        command.Parameters.Add(parameter);
    }

    public static async Task<int> ExecuteScalarIdAsync(this DbCommand command, CancellationToken cancellationToken = default)
    {
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value);
    }
}
