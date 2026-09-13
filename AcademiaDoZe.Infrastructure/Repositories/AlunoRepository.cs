using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AlunoRepository : BaseRepository, IAlunoRepository
{
    private const string BaseSelectQuery = "SELECT a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_aluno a INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public AlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static Aluno Map(DbDataReader reader, string nomeColumn = "nome")
    {
        var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
        var result = Aluno.Criar(reader.GetInt32Value("id_aluno"), reader.GetStringValue(nomeColumn), reader.GetStringValue("cpf"), reader.GetDateOnlyValue("nascimento"), reader.GetStringValue("telefone"), reader.GetStringValue("email"), logradouro, reader.GetStringValue("numero"), reader.GetNullableString("complemento"), reader.GetStringValue("senha"), Arquivo.Criar(reader.GetBytesValue("foto")).Value!);
        if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(notification => notification.Mensagem)));
        return result.Value!;
    }

    public Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE a.id_aluno = @Id", "@Id", id, DbType.Int32, cancellationToken);
    public Task<Aluno?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE a.cpf = @Cpf", "@Cpf", cpf.Valor, DbType.String, cancellationToken);
    public Task<Aluno?> ObterPorEmail(Email email, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE a.email = @Email", "@Email", email.Valor, DbType.String, cancellationToken);
    public Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} ORDER BY a.nome", cancellationToken);
    public Task<IEnumerable<Aluno>> ObterPorNome(string nome, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE a.nome LIKE @Nome ORDER BY a.nome", cancellationToken, "@Nome", $"%{nome}%");

    public Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default) => Existe("SELECT COUNT(1) FROM tb_aluno WHERE cpf = @Valor AND (@Id IS NULL OR id_aluno <> @Id)", cpf.Valor, id, cancellationToken);
    public Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default) => Existe("SELECT COUNT(1) FROM tb_aluno WHERE email = @Valor AND (@Id IS NULL OR id_aluno <> @Id)", email.Valor, id, cancellationToken);

    public Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto)"), async command =>
    {
        AddParameters(command, entity);
        return CriarComId(entity, await command.ExecuteScalarIdAsync(cancellationToken));
    }, cancellationToken);

    public Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_aluno SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto WHERE id_aluno = @Id", async command =>
    {
        AddParameters(command, entity); command.AddParameter("@Id", entity.Id, DbType.Int32);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Aluno nao encontrado");
        return entity;
    }, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_aluno WHERE id_aluno = @Id", async command => { command.AddParameter("@Id", id, DbType.Int32); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }, cancellationToken);
    public Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_aluno SET senha = @Senha WHERE id_aluno = @Id", async command => { command.AddParameter("@Id", id, DbType.Int32); command.AddParameter("@Senha", novaSenha.Valor, DbType.String); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }, cancellationToken);

    private Task<Aluno?> ObterUm(string query, string name, object value, DbType type, CancellationToken cancellationToken) => ExecuteAsync(query, async command => { command.AddParameter(name, value, type); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }, cancellationToken);
    private Task<IEnumerable<Aluno>> ObterLista(string query, CancellationToken cancellationToken, string? name = null, string? value = null) => ExecuteAsync(query, async command => { if (name != null) command.AddParameter(name, value, DbType.String); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Aluno>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return (IEnumerable<Aluno>)result; }, cancellationToken);
    private Task<bool> Existe(string query, string value, int? id, CancellationToken cancellationToken) => ExecuteAsync(query, async command => { command.AddParameter("@Valor", value, DbType.String); command.AddParameter("@Id", id, DbType.Int32); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0; }, cancellationToken);

    private static void AddParameters(DbCommand command, Aluno entity)
    {
        command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String); command.AddParameter("@Nome", entity.Nome, DbType.String); command.AddParameter("@Nascimento", entity.DataNascimento.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String); command.AddParameter("@Email", entity.Email.Valor, DbType.String); command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32);
        command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String); command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String); command.AddParameter("@Senha", entity.Senha.Valor, DbType.String); command.AddParameter("@Foto", entity.Foto.Conteudo, DbType.Binary);
    }

    private static Aluno CriarComId(Aluno entity, int id) => Aluno.Criar(id, entity.Nome, entity.Cpf.Valor, entity.DataNascimento, entity.Telefone.Valor, entity.Email.Valor, Logradouro.Criar(entity.Endereco.LogradouroId, "13083850", "rua", "bairro", "cidade", "SP", "Brasil").Value!, entity.Endereco.Numero, entity.Endereco.Complemento, entity.Senha.Valor, entity.Foto).Value!;
}
