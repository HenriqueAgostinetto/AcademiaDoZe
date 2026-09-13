using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbProvider
{
    public static DbConnection CreateConnection(string connectionString, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => new SqlConnection(connectionString),
        DatabaseType.MySql => new MySqlConnection(connectionString),
        DatabaseType.Sqlite => new SqliteConnection(connectionString),
        _ => throw new ArgumentOutOfRangeException(nameof(databaseType))
    };
}
