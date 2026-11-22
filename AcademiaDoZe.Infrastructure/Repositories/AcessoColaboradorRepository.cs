// henrique agostinetto piva
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private const string BaseSelectQuery = "SELECT ac.id_acesso_colaborador, ac.colaborador_id, ac.data_hora_entrada, ac.data_hora_saida, c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_acesso_colaborador ac INNER JOIN tb_colaborador c ON ac.colaborador_id = c.id_colaborador INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static AcessoColaborador Map(DbDataReader reader)
    {
        var colaborador = ColaboradorRepository.Map(reader, "colaborador_nome");
        var result = AcessoColaborador.Criar(reader.GetInt32Value("id_acesso_colaborador"), colaborador, reader.GetDateTimeValue("data_hora_entrada"));
        if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(notification => notification.Mensagem)));

        var acesso = result.Value!;
        var saida = reader.GetNullableDateTimeValue("data_hora_saida");
        if (saida.HasValue && acesso.RegistrarSaida(saida.Value).IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", "Saida invalida");
        return acesso;
    }

    public Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE ac.id_acesso_colaborador = @Id", "@Id", id, cancellationToken);
    public Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} ORDER BY ac.data_hora_entrada DESC", cancellationToken);
    public Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE ac.colaborador_id = @ColaboradorId ORDER BY ac.data_hora_entrada DESC, ac.id_acesso_colaborador DESC", "@ColaboradorId", colaboradorId, cancellationToken);

    public Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default) => ExecuteAsync(BaseSelectQuery + " WHERE (@ColaboradorId IS NULL OR ac.colaborador_id = @ColaboradorId) AND (@Inicio IS NULL OR ac.data_hora_entrada >= @Inicio) AND (@Fim IS NULL OR ac.data_hora_entrada < @Fim) ORDER BY ac.data_hora_entrada DESC", async command =>
    {
        command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
        command.AddParameter("@Inicio", inicio?.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        command.AddParameter("@Fim", fim?.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        return await LerListaAsync(command, cancellationToken);
    }, cancellationToken);

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken);
        return TimeSpan.FromTicks(acessos.Where(item => item.DataHoraSaida.HasValue).Sum(item => item.Permanencia.Ticks));
    }

    public Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_acesso_colaborador (colaborador_id, data_hora_entrada, data_hora_saida) VALUES (@ColaboradorId, @Entrada, @Saida)"), async command =>
    {
        AddParameters(command, entity);
        return CriarComId(entity, await command.ExecuteScalarIdAsync(cancellationToken));
    }, cancellationToken);

    public Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_acesso_colaborador SET data_hora_saida = @Saida WHERE id_acesso_colaborador = @Id", async command =>
    {
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Acesso do colaborador nao encontrado");
        return entity;
    }, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_acesso_colaborador WHERE id_acesso_colaborador = @Id", async command =>
    {
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }, cancellationToken);

    private Task<AcessoColaborador?> ObterUm(string query, string name, int value, CancellationToken cancellationToken) => ExecuteAsync(query, async command =>
    {
        command.AddParameter(name, value, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }, cancellationToken);

    private Task<IEnumerable<AcessoColaborador>> ObterLista(string query, CancellationToken cancellationToken) => ExecuteAsync(query, async command => await LerListaAsync(command, cancellationToken), cancellationToken);

    private static async Task<IEnumerable<AcessoColaborador>> LerListaAsync(DbCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var acessos = new List<AcessoColaborador>();
        while (await reader.ReadAsync(cancellationToken)) acessos.Add(Map(reader));
        return acessos;
    }

    private static void AddParameters(DbCommand command, AcessoColaborador entity)
    {
        command.AddParameter("@ColaboradorId", entity.ColaboradorId, DbType.Int32);
        command.AddParameter("@Entrada", entity.DataHora, DbType.DateTime);
        command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);
    }

    private static AcessoColaborador CriarComId(AcessoColaborador entity, int id)
    {
        var colaborador = Colaborador.Criar(entity.ColaboradorId, "henrique", "12345678909", new DateOnly(1995, 5, 15), "11912345678", "henrique@academia.com", Logradouro.Criar(1, "13083850", "rua", "bairro", "cidade", "SP", "Brasil").Value!, "100", "agostinetto", "Senha123", Domain.ValueObjects.Arquivo.Criar([1]).Value!, new DateOnly(2023, 1, 1), Domain.Enums.ColaboradorTipo.Instrutor, Domain.Enums.ColaboradorVinculo.CLT).Value!;
        var acesso = AcessoColaborador.Criar(id, colaborador, entity.DataHora).Value!;
        if (entity.DataHoraSaida.HasValue) acesso.RegistrarSaida(entity.DataHoraSaida.Value);
        return acesso;
    }
}
