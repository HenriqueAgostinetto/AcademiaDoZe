// henrique agostinetto piva
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const string BaseSelectQuery = "SELECT aa.id_acesso_aluno, aa.aluno_id, aa.data_hora_entrada, aa.data_hora_saida, a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_acesso_aluno aa INNER JOIN tb_aluno a ON aa.aluno_id = a.id_aluno INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static AcessoAluno Map(DbDataReader reader)
    {
        var aluno = AlunoRepository.Map(reader, "aluno_nome");
        var result = AcessoAluno.Criar(reader.GetInt32Value("id_acesso_aluno"), aluno, reader.GetDateTimeValue("data_hora_entrada"));
        if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(notification => notification.Mensagem)));

        var acesso = result.Value!;
        var saida = reader.GetNullableDateTimeValue("data_hora_saida");
        if (saida.HasValue && acesso.RegistrarSaida(saida.Value).IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", "Saida invalida");
        return acesso;
    }

    public Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE aa.id_acesso_aluno = @Id", "@Id", id, cancellationToken);
    public Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} ORDER BY aa.data_hora_entrada DESC", cancellationToken);
    public Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE aa.aluno_id = @AlunoId ORDER BY aa.data_hora_entrada DESC, aa.id_acesso_aluno DESC", "@AlunoId", alunoId, cancellationToken);

    public Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default) => ExecuteAsync(BaseSelectQuery + " WHERE (@AlunoId IS NULL OR aa.aluno_id = @AlunoId) AND (@Inicio IS NULL OR aa.data_hora_entrada >= @Inicio) AND (@Fim IS NULL OR aa.data_hora_entrada < @Fim) ORDER BY aa.data_hora_entrada DESC", async command =>
    {
        command.AddParameter("@AlunoId", alunoId, DbType.Int32);
        command.AddParameter("@Inicio", inicio?.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        command.AddParameter("@Fim", fim?.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        return await LerListaAsync(command, cancellationToken);
    }, cancellationToken);

    public Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default) => ExecuteAsync("SELECT COUNT(1) FROM tb_acesso_aluno WHERE aluno_id = @AlunoId AND data_hora_saida IS NULL", async command =>
    {
        command.AddParameter("@AlunoId", alunoId, DbType.Int32);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }, cancellationToken);

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterTodos(cancellationToken);
        return acessos.Where(item => item.DataHora.Month == mes).GroupBy(item => TimeOnly.FromDateTime(item.DataHora)).ToDictionary(item => item.Key, item => item.Count());
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterTodos(cancellationToken);
        return acessos.Where(item => item.DataHora.Month == mes && item.DataHoraSaida.HasValue).GroupBy(item => item.DataHora.Day).ToDictionary(item => item.Key, item => TimeSpan.FromTicks((long)item.Average(acesso => acesso.Permanencia.Ticks)));
    }

    public Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default) => ExecuteAsync("SELECT a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_aluno a INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro WHERE NOT EXISTS (SELECT 1 FROM tb_acesso_aluno aa WHERE aa.aluno_id = a.id_aluno AND aa.data_hora_entrada >= @Inicio)", async command =>
    {
        command.AddParameter("@Inicio", DateTime.Today.AddDays(-dias), DbType.DateTime);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var alunos = new List<Aluno>();
        while (await reader.ReadAsync(cancellationToken)) alunos.Add(AlunoRepository.Map(reader));
        return (IEnumerable<Aluno>)alunos;
    }, cancellationToken);

    public Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_acesso_aluno (aluno_id, data_hora_entrada, data_hora_saida) VALUES (@AlunoId, @Entrada, @Saida)"), async command =>
    {
        AddParameters(command, entity);
        return CriarComId(entity, await command.ExecuteScalarIdAsync(cancellationToken));
    }, cancellationToken);

    public Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_acesso_aluno SET data_hora_saida = @Saida WHERE id_acesso_aluno = @Id", async command =>
    {
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Acesso do aluno nao encontrado");
        return entity;
    }, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_acesso_aluno WHERE id_acesso_aluno = @Id", async command =>
    {
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }, cancellationToken);

    private Task<AcessoAluno?> ObterUm(string query, string name, int value, CancellationToken cancellationToken) => ExecuteAsync(query, async command =>
    {
        command.AddParameter(name, value, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }, cancellationToken);

    private Task<IEnumerable<AcessoAluno>> ObterLista(string query, CancellationToken cancellationToken) => ExecuteAsync(query, async command => await LerListaAsync(command, cancellationToken), cancellationToken);

    private static async Task<IEnumerable<AcessoAluno>> LerListaAsync(DbCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var acessos = new List<AcessoAluno>();
        while (await reader.ReadAsync(cancellationToken)) acessos.Add(Map(reader));
        return acessos;
    }

    private static void AddParameters(DbCommand command, AcessoAluno entity)
    {
        command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@Entrada", entity.DataHora, DbType.DateTime);
        command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);
    }

    private static AcessoAluno CriarComId(AcessoAluno entity, int id)
    {
        var aluno = Aluno.Criar(entity.AlunoId, "henrique", "12345678909", new DateOnly(1995, 5, 15), "11912345678", "henrique@academia.com", Logradouro.Criar(1, "13083850", "rua", "bairro", "cidade", "SP", "Brasil").Value!, "100", "agostinetto", "Senha123", Domain.ValueObjects.Arquivo.Criar([1]).Value!).Value!;
        var acesso = AcessoAluno.Criar(id, aluno, entity.DataHora).Value!;
        if (entity.DataHoraSaida.HasValue) acesso.RegistrarSaida(entity.DataHoraSaida.Value);
        return acesso;
    }
}
