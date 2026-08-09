// henrique agostinetto piva
namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoAluno(int id, Aluno aluno, DateTime dataHora) : base(id)
    {
        Aluno = aluno;
        DataHora = dataHora;
    }
}
