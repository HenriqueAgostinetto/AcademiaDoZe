using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    private const string BaseSelectQuery = "SELECT m.id_matricula, m.aluno_id, m.plano, m.data_inicio, m.data_fim, m.objetivo, m.restricao_medica, m.obs_restricao, m.laudo_medico, a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_matricula m INNER JOIN tb_aluno a ON m.aluno_id = a.id_aluno INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public MatriculaRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public static Matricula Map(DbDataReader reader)
    {
        var aluno = AlunoRepository.Map(reader, "aluno_nome");
        var laudo = reader.GetNullableBytes("laudo_medico");
        var result = Matricula.Criar(reader.GetInt32Value("id_matricula"), aluno, (MatriculaPlano)reader.GetInt32Value("plano"), reader.GetDateOnlyValue("data_inicio"), reader.GetStringValue("objetivo"), (MatriculaRestricoes)reader.GetInt32Value("restricao_medica"), laudo == null ? null : Arquivo.Criar(laudo).Value!, reader.GetNullableString("obs_restricao"));
        if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(notification => notification.Mensagem)));
        return result.Value!;
    }

    public Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE m.id_matricula = @Id", "@Id", id, cancellationToken);
    public Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} ORDER BY m.data_inicio DESC", cancellationToken);
    public Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC", cancellationToken, "@AlunoId", alunoId);
    public Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default) => ObterUm($"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId AND m.data_fim >= {GetCurrentDateFunction()} ORDER BY m.data_fim DESC", "@AlunoId", alunoId, cancellationToken);
    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default) => await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken) != null;
    public Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default) => alunoId == 0
        ? ObterLista($"{BaseSelectQuery} WHERE m.data_fim >= {GetCurrentDateFunction()} ORDER BY m.data_fim", cancellationToken)
        : ObterLista($"{BaseSelectQuery} WHERE m.data_fim >= {GetCurrentDateFunction()} AND m.aluno_id = @AlunoId ORDER BY m.data_fim", cancellationToken, "@AlunoId", alunoId);
    public Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE m.data_fim >= {GetCurrentDateFunction()} AND m.data_fim <= {GetDateAddDaysExpression(GetCurrentDateFunction(), "@Dias")} ORDER BY m.data_fim", cancellationToken, "@Dias", dias);
    public Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default) => ObterLista($"{BaseSelectQuery} WHERE m.plano = @Plano ORDER BY a.nome", cancellationToken, "@Plano", (int)plano);

    public Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default) => ExecuteAsync(FormatInsertQuery("INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico) VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico)"), async command =>
    {
        AddParameters(command, entity);
        return CriarComId(entity, await command.ExecuteScalarIdAsync(cancellationToken));
    }, cancellationToken);

    public Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default) => ExecuteAsync("UPDATE tb_matricula SET aluno_id = @AlunoId, plano = @Plano, data_inicio = @DataInicio, data_fim = @DataFim, objetivo = @Objetivo, restricao_medica = @RestricaoMedica, obs_restricao = @ObsRestricao, laudo_medico = @LaudoMedico WHERE id_matricula = @Id", async command =>
    {
        AddParameters(command, entity);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Matricula nao encontrada");
        return entity;
    }, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default) => ExecuteAsync("DELETE FROM tb_matricula WHERE id_matricula = @Id", async command =>
    {
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }, cancellationToken);

    private Task<Matricula?> ObterUm(string query, string name, int value, CancellationToken cancellationToken) => ExecuteAsync(query, async command =>
    {
        command.AddParameter(name, value, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }, cancellationToken);

    private Task<IEnumerable<Matricula>> ObterLista(string query, CancellationToken cancellationToken, string? name = null, int? value = null) => ExecuteAsync(query, async command =>
    {
        if (name != null) command.AddParameter(name, value, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<Matricula>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return (IEnumerable<Matricula>)result;
    }, cancellationToken);

    private static void AddParameters(DbCommand command, Matricula entity)
    {
        command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
        command.AddParameter("@DataInicio", entity.DataInicio.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@DataFim", entity.DataFim.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
        command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
        command.AddParameter("@ObsRestricao", entity.ObservacoesRestricoes, DbType.String);
        command.AddParameter("@LaudoMedico", entity.LaudoMedico?.Conteudo, DbType.Binary);
    }

    private static Matricula CriarComId(Matricula entity, int id)
    {
        var aluno = Aluno.Criar(entity.AlunoId, "henrique", "12345678909", new DateOnly(1995, 5, 15), "11912345678", "henrique@academia.com", Logradouro.Criar(1, "13083850", "rua", "bairro", "cidade", "SP", "Brasil").Value!, "100", "agostinetto", "Senha123", Arquivo.Criar([1]).Value!).Value!;
        return Matricula.Criar(id, aluno, entity.Plano, entity.DataInicio, entity.Objetivo, entity.RestricoesMedicas, entity.LaudoMedico, entity.ObservacoesRestricoes).Value!;
    }
}
