// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService(Func<IAcessoAlunoRepository> repository, Func<IAlunoRepository> alunos, Func<IMatriculaRepository> matriculas) : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repository = repository;
    private readonly Func<IAlunoRepository> _alunos = alunos;
    private readonly Func<IMatriculaRepository> _matriculas = matriculas;

    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repository().ObterTodos(cancellationToken);
        var resultado = new List<AcessoAlunoDto>();
        foreach (var acesso in acessos)
        {
            var aluno = await ObterAlunoAsync(acesso.AlunoId, cancellationToken);
            resultado.Add(acesso.ToDto(aluno.ToDto()));
        }
        return resultado;
    }

    public async Task<AcessoAlunoDto> RegistrarEntradaAsync(int alunoId, DateTime? dataHora = null, CancellationToken cancellationToken = default)
    {
        var aluno = await ObterAlunoAsync(alunoId, cancellationToken);
        var matricula = await _matriculas().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken) ?? throw new InvalidOperationException("Aluno sem matricula ativa.");
        if (await _repository().EstaNaAcademia(alunoId, cancellationToken)) throw new InvalidOperationException("Aluno ja possui uma entrada em aberto.");

        var entrada = dataHora ?? DateTime.Now;
        var acesso = CriarAcesso(aluno, entrada);
        var salvo = await _repository().Adicionar(acesso, cancellationToken);
        var diasRestantes = Math.Max(0, (matricula.DataFim.DayNumber - DateOnly.FromDateTime(entrada).DayNumber));
        return salvo.ToDto(aluno.ToDto(), diasRestantes);
    }

    public async Task<AcessoAlunoDto> RegistrarSaidaAsync(int alunoId, DateTime? dataHora = null, CancellationToken cancellationToken = default)
    {
        var aluno = await ObterAlunoAsync(alunoId, cancellationToken);
        var acesso = await _repository().ObterUltimoAcesso(alunoId, cancellationToken) ?? throw new InvalidOperationException("Aluno nao possui entrada em aberto.");
        if (acesso.DataHoraSaida.HasValue) throw new InvalidOperationException("Aluno nao possui entrada em aberto.");

        var resultado = acesso.RegistrarSaida(dataHora ?? DateTime.Now);
        if (resultado.IsFailure) throw new InvalidOperationException(resultado.Notifications.First().Mensagem);
        var salvo = await _repository().Atualizar(acesso, cancellationToken);
        return salvo.ToDto(aluno.ToDto());
    }

    private async Task<Aluno> ObterAlunoAsync(int alunoId, CancellationToken cancellationToken) => await _alunos().ObterPorId(alunoId, cancellationToken) ?? throw new KeyNotFoundException("Aluno nao encontrado.");
    private static AcessoAluno CriarAcesso(Aluno aluno, DateTime entrada)
    {
        var resultado = AcessoAluno.Criar(0, aluno, entrada);
        return resultado.IsSuccess ? resultado.Value! : throw new InvalidOperationException(resultado.Notifications.First().Mensagem);
    }
}

public class AcessoColaboradorService(Func<IAcessoColaboradorRepository> repository, Func<IColaboradorRepository> colaboradores) : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _repository = repository;
    private readonly Func<IColaboradorRepository> _colaboradores = colaboradores;

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repository().ObterTodos(cancellationToken);
        var resultado = new List<AcessoColaboradorDto>();
        foreach (var acesso in acessos)
        {
            var colaborador = await ObterColaboradorAsync(acesso.ColaboradorId, cancellationToken);
            var horas = await _repository().ObterHorasTrabalhadasNoDia(colaborador.Id, DateOnly.FromDateTime(acesso.DataHora), cancellationToken);
            resultado.Add(acesso.ToDto(colaborador.ToDto(), horas, LimiteDiario(colaborador)));
        }
        return resultado;
    }

    public async Task<AcessoColaboradorDto> RegistrarEntradaAsync(int colaboradorId, DateTime? dataHora = null, CancellationToken cancellationToken = default)
    {
        var colaborador = await ObterColaboradorAsync(colaboradorId, cancellationToken);
        var ultimoAcesso = await _repository().ObterUltimoAcesso(colaboradorId, cancellationToken);
        if (ultimoAcesso?.DataHoraSaida is null && ultimoAcesso is not null) throw new InvalidOperationException("Colaborador ja possui uma entrada em aberto.");

        var entrada = dataHora ?? DateTime.Now;
        var horas = await _repository().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(entrada), cancellationToken);
        var limite = LimiteDiario(colaborador);
        if (horas >= limite) throw new InvalidOperationException($"Limite diario de {limite.TotalHours:0} horas atingido.");

        var acesso = CriarAcesso(colaborador, entrada);
        var salvo = await _repository().Adicionar(acesso, cancellationToken);
        return salvo.ToDto(colaborador.ToDto(), horas, limite);
    }

    public async Task<AcessoColaboradorDto> RegistrarSaidaAsync(int colaboradorId, DateTime? dataHora = null, CancellationToken cancellationToken = default)
    {
        var colaborador = await ObterColaboradorAsync(colaboradorId, cancellationToken);
        var acesso = await _repository().ObterUltimoAcesso(colaboradorId, cancellationToken) ?? throw new InvalidOperationException("Colaborador nao possui entrada em aberto.");
        if (acesso.DataHoraSaida.HasValue) throw new InvalidOperationException("Colaborador nao possui entrada em aberto.");

        var resultado = acesso.RegistrarSaida(dataHora ?? DateTime.Now);
        if (resultado.IsFailure) throw new InvalidOperationException(resultado.Notifications.First().Mensagem);
        var salvo = await _repository().Atualizar(acesso, cancellationToken);
        var horas = await _repository().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(salvo.DataHora), cancellationToken);
        return salvo.ToDto(colaborador.ToDto(), horas, LimiteDiario(colaborador));
    }

    private async Task<Colaborador> ObterColaboradorAsync(int colaboradorId, CancellationToken cancellationToken) => await _colaboradores().ObterPorId(colaboradorId, cancellationToken) ?? throw new KeyNotFoundException("Colaborador nao encontrado.");
    private static TimeSpan LimiteDiario(Colaborador colaborador) => colaborador.Vinculo == ColaboradorVinculo.Estagio ? TimeSpan.FromHours(6) : TimeSpan.FromHours(8);
    private static AcessoColaborador CriarAcesso(Colaborador colaborador, DateTime entrada)
    {
        var resultado = AcessoColaborador.Criar(0, colaborador, entrada);
        return resultado.IsSuccess ? resultado.Value! : throw new InvalidOperationException(resultado.Notifications.First().Mensagem);
    }
}
