using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository, ILogradouroRepository
{
    public LogradouroRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static Logradouro Map(DbDataReader reader, string nomeColumn = "nome") => Logradouro.Criar(
        reader.GetInt32Value("id_logradouro"), reader.GetStringValue("cep"), reader.GetStringValue(nomeColumn),
        reader.GetStringValue("bairro"), reader.GetStringValue("cidade"), reader.GetStringValue("estado"), reader.GetStringValue("pais")).Value!;

    public Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm("SELECT * FROM tb_logradouro WHERE id_logradouro = @Id", "@Id", id, cancellationToken);
    public Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default) => ObterUm("SELECT * FROM tb_logradouro WHERE cep = @Cep", "@Cep", cep.Valor, cancellationToken);

    public Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista("SELECT * FROM tb_logradouro ORDER BY nome", cancellationToken);
    public Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default) => ObterLista(DatabaseType == AcademiaDoZe.Infrastructure.Data.DatabaseType.Sqlite ? "SELECT * FROM tb_logradouro WHERE cidade = @Cidade COLLATE NOCASE ORDER BY nome" : "SELECT * FROM tb_logradouro WHERE cidade = @Cidade ORDER BY nome", cancellationToken, "@Cidade", cidade);
    public Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default) => ObterLista("SELECT * FROM tb_logradouro WHERE cidade = @Cidade AND bairro = @Bairro ORDER BY nome", cancellationToken, "@Cidade", cidade, "@Bairro", bairro);

    public Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default) => ExecuteAsync("SELECT COUNT(1) FROM tb_logradouro WHERE cep = @Cep AND (@Id IS NULL OR id_logradouro <> @Id)", async command =>
    {
        command.AddParameter("@Cep", cep.Valor, DbType.String);
        command.AddParameter("@Id", id, DbType.Int32);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }, cancellationToken);

    public Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais) VALUES (@Cep, @Nome, @Bairro, @Cidade, @Estado, @Pais)"), async command =>
    {
        AddParameters(command, entity);
        var id = await command.ExecuteScalarIdAsync(cancellationToken);
        return MapInserted(id, entity);
    }, cancellationToken);

    public Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_logradouro SET cep = @Cep, nome = @Nome, bairro = @Bairro, cidade = @Cidade, estado = @Estado, pais = @Pais WHERE id_logradouro = @Id", async command =>
    {
        AddParameters(command, entity);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Logradouro nao encontrado");
        return entity;
    }, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_logradouro WHERE id_logradouro = @Id", async command =>
    {
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }, cancellationToken);

    private Task<Logradouro?> ObterUm(string query, string parameter, object value, CancellationToken cancellationToken) => ExecuteAsync(query, async command =>
    {
        command.AddParameter(parameter, value, DbType.String);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }, cancellationToken);

    private Task<IEnumerable<Logradouro>> ObterLista(string query, CancellationToken cancellationToken, params string[] parameters) => ExecuteAsync(query, async command =>
    {
        for (var index = 0; index < parameters.Length; index += 2) command.AddParameter(parameters[index], parameters[index + 1], DbType.String);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<Logradouro>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return (IEnumerable<Logradouro>)result;
    }, cancellationToken);

    private static void AddParameters(DbCommand command, Logradouro entity)
    {
        command.AddParameter("@Cep", entity.Cep.Valor, DbType.String); command.AddParameter("@Nome", entity.Nome, DbType.String);
        command.AddParameter("@Bairro", entity.Bairro, DbType.String); command.AddParameter("@Cidade", entity.Cidade, DbType.String);
        command.AddParameter("@Estado", entity.Estado, DbType.String); command.AddParameter("@Pais", entity.Pais, DbType.String);
    }

    private static Logradouro MapInserted(int id, Logradouro entity) => Logradouro.Criar(id, entity.Cep.Valor, entity.Nome, entity.Bairro, entity.Cidade, entity.Estado, entity.Pais).Value!;
}
