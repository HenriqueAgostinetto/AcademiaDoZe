using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    private const string BaseSelectQuery = "SELECT c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_colaborador c INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static Colaborador Map(DbDataReader reader, string nomeColumn = "nome")
    {
        var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
        var result = Colaborador.Criar(reader.GetInt32Value("id_colaborador"), reader.GetStringValue(nomeColumn), reader.GetStringValue("cpf"), reader.GetDateOnlyValue("nascimento"), reader.GetStringValue("telefone"), reader.GetStringValue("email"), logradouro, reader.GetStringValue("numero"), reader.GetNullableString("complemento"), reader.GetStringValue("senha"), Arquivo.Criar(reader.GetBytesValue("foto")).Value!, reader.GetDateOnlyValue("admissao"), (ColaboradorTipo)reader.GetInt32Value("tipo"), (ColaboradorVinculo)reader.GetInt32Value("vinculo"));
        if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(notification => notification.Mensagem)));
        return result.Value!;
    }

    public Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE c.id_colaborador = @Id", "@Id", id, DbType.Int32, cancellationToken);
    public Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE c.cpf = @Cpf", "@Cpf", cpf.Valor, DbType.String, cancellationToken);
    public Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE c.email = @Email", "@Email", email.Valor, DbType.String, cancellationToken);
    public Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} ORDER BY c.nome", cancellationToken);
    public Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE c.tipo = @Valor ORDER BY c.nome", cancellationToken, (int)tipo);
    public Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE c.vinculo = @Valor ORDER BY c.nome", cancellationToken, (int)vinculo);
    public Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default) => Existe("SELECT COUNT(1) FROM tb_colaborador WHERE cpf = @Valor AND (@Id IS NULL OR id_colaborador <> @Id)", cpf.Valor, id, cancellationToken);
    public Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default) => Existe("SELECT COUNT(1) FROM tb_colaborador WHERE email = @Valor AND (@Id IS NULL OR id_colaborador <> @Id)", email.Valor, id, cancellationToken);

    public Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo)"), async command => { AddParameters(command, entity); return CriarComId(entity, await command.ExecuteScalarIdAsync(cancellationToken)); }, cancellationToken);
    public Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_colaborador SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto, admissao = @Admissao, tipo = @Tipo, vinculo = @Vinculo WHERE id_colaborador = @Id", async command => { AddParameters(command, entity); command.AddParameter("@Id", entity.Id, DbType.Int32); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Colaborador nao encontrado"); return entity; }, cancellationToken);
    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_colaborador WHERE id_colaborador = @Id", async command => { command.AddParameter("@Id", id, DbType.Int32); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }, cancellationToken);
    public Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_colaborador SET senha = @Senha WHERE id_colaborador = @Id", async command => { command.AddParameter("@Id", id, DbType.Int32); command.AddParameter("@Senha", novaSenha.Valor, DbType.String); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }, cancellationToken);

    private Task<Colaborador?> ObterUm(string query, string name, object value, DbType type, CancellationToken cancellationToken) => ExecuteAsync(query, async command => { command.AddParameter(name, value, type); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }, cancellationToken);
    private Task<IEnumerable<Colaborador>> ObterLista(string query, CancellationToken cancellationToken, int? value = null) => ExecuteAsync(query, async command => { if (value.HasValue) command.AddParameter("@Valor", value.Value, DbType.Int32); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Colaborador>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return (IEnumerable<Colaborador>)result; }, cancellationToken);
    private Task<bool> Existe(string query, string value, int? id, CancellationToken cancellationToken) => ExecuteAsync(query, async command => { command.AddParameter("@Valor", value, DbType.String); command.AddParameter("@Id", id, DbType.Int32); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0; }, cancellationToken);

    private static void AddParameters(DbCommand command, Colaborador entity)
    {
        command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String); command.AddParameter("@Nome", entity.Nome, DbType.String); command.AddParameter("@Nascimento", entity.DataNascimento.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String); command.AddParameter("@Email", entity.Email.Valor, DbType.String); command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32); command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
        command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String); command.AddParameter("@Senha", entity.Senha.Valor, DbType.String); command.AddParameter("@Foto", entity.Foto.Conteudo, DbType.Binary); command.AddParameter("@Admissao", entity.DataAdmissao.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32); command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
    }

    private static Colaborador CriarComId(Colaborador entity, int id) => Colaborador.Criar(id, entity.Nome, entity.Cpf.Valor, entity.DataNascimento, entity.Telefone.Valor, entity.Email.Valor, Logradouro.Criar(entity.Endereco.LogradouroId, "13083850", "rua", "bairro", "cidade", "SP", "Brasil").Value!, entity.Endereco.Numero, entity.Endereco.Complemento, entity.Senha.Valor, entity.Foto, entity.DataAdmissao, entity.Tipo, entity.Vinculo).Value!;
}
