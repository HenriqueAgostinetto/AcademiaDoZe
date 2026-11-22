// henrique agostinetto piva
using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }
    public DateTime DataHora { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }
    public TimeSpan Permanencia => DataHoraSaida is null ? TimeSpan.Zero : DataHoraSaida.Value - DataHora;

    private AcessoAluno(int id, int alunoId, DateTime dataHora) : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno? aluno, DateTime dataHora)
    {
        if (aluno is null) return Result<AcessoAluno>.Failure("Aluno", "ALUNO_INVALIDO");
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            return Result<AcessoAluno>.Failure("DataHora", "DATA_HORA_INTERVALO_INVALIDO");

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno.Id, dataHora));
    }

    public Result<AcessoAluno> RegistrarSaida(DateTime dataHoraSaida)
    {
        if (dataHoraSaida < DataHora) return Result<AcessoAluno>.Failure("DataHoraSaida", "DATA_HORA_SAIDA_INVALIDA");
        if (dataHoraSaida.TimeOfDay < new TimeSpan(6, 0, 0) || dataHoraSaida.TimeOfDay > new TimeSpan(22, 0, 0))
            return Result<AcessoAluno>.Failure("DataHoraSaida", "DATA_HORA_INTERVALO_INVALIDO");

        DataHoraSaida = dataHoraSaida;
        return Result<AcessoAluno>.Success(this);
    }
}
