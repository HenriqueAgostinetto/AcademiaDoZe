using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(string connectionString, DatabaseType databaseType, CancellationToken cancellationToken = default)
    {
        if (databaseType == DatabaseType.SqlServer) await CreateSqlServerDatabaseAsync(connectionString, cancellationToken);
        await using var connection = DbProvider.CreateConnection(connectionString, databaseType);
        await connection.OpenAsync(cancellationToken);
        foreach (var query in GetQueries(databaseType))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task CreateSqlServerDatabaseAsync(string connectionString, CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var database = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"IF DB_ID('{database}') IS NULL CREATE DATABASE [{database}]";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IEnumerable<string> GetQueries(DatabaseType databaseType)
    {
        var id = databaseType switch
        {
            DatabaseType.SqlServer => "INT IDENTITY(1,1) PRIMARY KEY",
            DatabaseType.MySql => "INT AUTO_INCREMENT PRIMARY KEY",
            _ => "INTEGER PRIMARY KEY AUTOINCREMENT"
        };
        var text = databaseType == DatabaseType.SqlServer ? "NVARCHAR" : "VARCHAR";
        var blob = databaseType == DatabaseType.SqlServer ? "VARBINARY(MAX)" : "BLOB";
        var createLogradouro = $"CREATE TABLE IF NOT EXISTS tb_logradouro (id_logradouro {id}, cep {text}(8) NOT NULL UNIQUE, nome {text}(150) NOT NULL, bairro {text}(100) NOT NULL, cidade {text}(100) NOT NULL, estado {text}(2) NOT NULL, pais {text}(100) NOT NULL)";
        var createAluno = $"CREATE TABLE IF NOT EXISTS tb_aluno (id_aluno {id}, cpf {text}(11) NOT NULL UNIQUE, nome {text}(150) NOT NULL, nascimento DATE NOT NULL, telefone {text}(11) NOT NULL, email {text}(150) NOT NULL UNIQUE, logradouro_id INT NOT NULL, numero {text}(20) NOT NULL, complemento {text}(100) NULL, senha {text}(200) NOT NULL, foto {blob} NOT NULL)";
        var createColaborador = $"CREATE TABLE IF NOT EXISTS tb_colaborador (id_colaborador {id}, cpf {text}(11) NOT NULL UNIQUE, nome {text}(150) NOT NULL, nascimento DATE NOT NULL, telefone {text}(11) NOT NULL, email {text}(150) NOT NULL UNIQUE, logradouro_id INT NOT NULL, numero {text}(20) NOT NULL, complemento {text}(100) NULL, senha {text}(200) NOT NULL, foto {blob} NOT NULL, admissao DATE NOT NULL, tipo INT NOT NULL, vinculo INT NOT NULL)";
        var createMatricula = $"CREATE TABLE IF NOT EXISTS tb_matricula (id_matricula {id}, aluno_id INT NOT NULL, plano INT NOT NULL, data_inicio DATE NOT NULL, data_fim DATE NOT NULL, objetivo {text}(150) NOT NULL, restricao_medica INT NOT NULL, obs_restricao {text}(500) NULL, laudo_medico {blob} NULL)";
        return databaseType == DatabaseType.SqlServer
            ? [WrapSqlServer(createLogradouro, "tb_logradouro"), WrapSqlServer(createAluno, "tb_aluno"), WrapSqlServer(createColaborador, "tb_colaborador"), WrapSqlServer(createMatricula, "tb_matricula")]
            : [createLogradouro, createAluno, createColaborador, createMatricula];
    }

    private static string WrapSqlServer(string query, string table) => $"IF OBJECT_ID('{table}', 'U') IS NULL BEGIN {query.Replace("CREATE TABLE IF NOT EXISTS", "CREATE TABLE")} END";
}
