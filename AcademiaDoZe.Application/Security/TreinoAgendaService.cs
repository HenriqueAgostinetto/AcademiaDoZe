// henrique agostinetto piva
namespace AcademiaDoZe.Application.Security;

public static class TreinoAgendaService
{
    private static readonly IReadOnlyList<TreinoAgendaItem> Agenda =
    [
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Monday, "Segunda-feira", "Adaptacao e mobilidade"),
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Tuesday, "Terca-feira", "Cardio e condicionamento"),
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Wednesday, "Quarta-feira", "Forca inicial - membros superiores"),
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Thursday, "Quinta-feira", "Forca inicial - membros inferiores"),
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Friday, "Sexta-feira", "Treino funcional"),
        new("Henrique 2", "Henrique 2", "Henrique 1", "Henrique 1", DayOfWeek.Saturday, "Sabado", "Alongamento e mobilidade")
    ];

    public static List<TreinoAgendaItem> ObterAgendaDoAluno(string alunoLogin) => Agenda
        .Where(item => string.Equals(item.AlunoLogin, alunoLogin, StringComparison.OrdinalIgnoreCase))
        .ToList();

    public static TreinoAgendaItem? ObterTreinoDoDia(string alunoLogin, DayOfWeek diaSemana) => ObterAgendaDoAluno(alunoLogin)
        .FirstOrDefault(item => item.DiaSemana == diaSemana);
}

public sealed record TreinoAgendaItem(string AlunoLogin, string AlunoNome, string PersonalLogin, string PersonalNome, DayOfWeek DiaSemana, string Dia, string Nome);
