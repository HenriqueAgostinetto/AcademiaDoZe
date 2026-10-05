// henrique agostinetto piva
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static RepositoryConfig ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ObterConfiguracaoAtual();
        var config = new RepositoryConfig { ConnectionString = connectionString, DatabaseType = databaseType.ToDatabaseType() };
        services.AddSingleton(config);
        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(config, (item, _) =>
        {
            var (novaConnectionString, novoDatabaseType) = ObterConfiguracaoAtual();
            item.ConnectionString = novaConnectionString;
            item.DatabaseType = novoDatabaseType.ToDatabaseType();
        });
        services.AddApplicationServices();
        return config;
    }

    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        if (!Enum.TryParse(Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString()), out AppDatabaseType databaseType))
            databaseType = AppDatabaseType.Sqlite;

        if (databaseType == AppDatabaseType.Sqlite)
        {
            var caminhoPadrao = Path.Combine(FileSystem.AppDataDirectory, "academia_do_ze.db");
            var caminho = Preferences.Get("Sqlite_Caminho", caminhoPadrao);
            var complementoSqlite = Preferences.Get("Sqlite_Complemento", "Default Timeout=5;");
            return ($"Data Source={caminho};{complementoSqlite}", databaseType);
        }

        var prefixo = databaseType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
        var servidorPadrao = databaseType == AppDatabaseType.SqlServer ? "127.0.0.1,1433" : "127.0.0.1";
        var usuarioPadrao = databaseType == AppDatabaseType.SqlServer ? "sa" : "root";
        var complementoPadrao = databaseType == AppDatabaseType.SqlServer
            ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;"
            : "Port=3306;Connection Timeout=5;";
        var servidor = Preferences.Get($"{prefixo}_Servidor", servidorPadrao);
        var banco = Preferences.Get($"{prefixo}_Banco", "db_academia_do_ze");
        var usuario = Preferences.Get($"{prefixo}_Usuario", usuarioPadrao);
        var senha = Preferences.Get($"{prefixo}_Senha", string.Empty);
        var complementoBanco = Preferences.Get($"{prefixo}_Complemento", complementoPadrao);
        return ($"Server={servidor};Database={banco};User Id={usuario};Password={senha};{complementoBanco}", databaseType);
    }
}
