// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService(Func<IColaboradorRepository> repository, Func<ILogradouroRepository> logradouros) : IColaboradorService
{
    private readonly Func<IColaboradorRepository> _repository = repository;
    private readonly Func<ILogradouroRepository> _logradouros = logradouros;

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken token = default)
    {
        var item = await _repository().ObterPorId(id, token);
        return item == null ? null : item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token));
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken token = default) => await Mapear(await _repository().ObterTodos(token), token);
    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken token = default) { var item = await _repository().ObterPorCpf(ValidarCpf(cpf), token); return item == null ? null : item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token)); }
    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken token = default) { var item = await _repository().ObterPorEmail(ValidarEmail(email), token); return item == null ? null : item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token)); }
    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken token = default) => await Mapear(await _repository().ObterPorTipo((Domain.Enums.ColaboradorTipo)tipo, token), token);
    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken token = default) => await Mapear(await _repository().ObterPorVinculo((Domain.Enums.ColaboradorVinculo)vinculo, token), token);
    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken token = default) => !string.IsNullOrWhiteSpace(cpf) && await _repository().CpfJaExiste(ValidarCpf(cpf), id, token);
    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken token = default) => !string.IsNullOrWhiteSpace(email) && await _repository().EmailJaExiste(ValidarEmail(email), id, token);

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto dto, CancellationToken token = default)
    {
        var endereco = await Endereco(dto, token);
        if (await CpfJaExisteAsync(dto.Cpf, null, token)) throw new InvalidOperationException("CPF ja cadastrado");
        if (await EmailJaExisteAsync(dto.Email ?? string.Empty, null, token)) throw new InvalidOperationException("Email ja cadastrado");
        return (await _repository().Adicionar(dto.ToEntity(endereco, PasswordHasher.Hash(Obrigatorio(dto.Senha!, "senha"))), token)).ToDto(endereco);
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto dto, CancellationToken token = default)
    {
        var atual = await _repository().ObterPorId(dto.Id, token) ?? throw new KeyNotFoundException("Colaborador nao encontrado");
        var endereco = await Endereco(dto, token, atual.Endereco.LogradouroId);
        if (await CpfJaExisteAsync(dto.Cpf, dto.Id, token)) throw new InvalidOperationException("CPF ja cadastrado");
        if (await EmailJaExisteAsync(dto.Email ?? string.Empty, dto.Id, token)) throw new InvalidOperationException("Email ja cadastrado");
        var senha = string.IsNullOrWhiteSpace(dto.Senha) ? atual.Senha.Valor : PasswordHasher.Hash(dto.Senha);
        return (await _repository().Atualizar(dto.ToEntity(endereco, senha), token)).ToDto(endereco);
    }

    public Task<bool> RemoverAsync(int id, CancellationToken token = default) => _repository().Remover(id, token);
    public Task<bool> TrocarSenhaAsync(int id, string senha, CancellationToken token = default) => _repository().TrocarSenha(id, Senha.Criar(PasswordHasher.Hash(Obrigatorio(senha, nameof(senha)))).Value!, token);

    private async Task<Domain.Entities.Logradouro> Endereco(ColaboradorDto dto, CancellationToken token, int fallback = 0) => await _logradouros().ObterPorId(dto.Endereco?.Id > 0 ? dto.Endereco.Id : fallback, token) ?? throw new KeyNotFoundException("Logradouro nao encontrado");
    private async Task<IEnumerable<ColaboradorDto>> Mapear(IEnumerable<Domain.Entities.Colaborador> items, CancellationToken token) { var result = new List<ColaboradorDto>(); foreach (var item in items) result.Add(item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token))); return result; }
    private static Cpf ValidarCpf(string value) { var result = Cpf.Criar(value); return result.IsSuccess ? result.Value! : throw new ArgumentException("CPF invalido"); }
    private static Email ValidarEmail(string value) { var result = Email.Criar(value); return result.IsSuccess ? result.Value! : throw new ArgumentException("Email invalido"); }
    private static string Obrigatorio(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Valor obrigatorio", name) : value.Trim();
}

public class MatriculaService(Func<IMatriculaRepository> repository, Func<IAlunoRepository> alunos) : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _repository=repository; private readonly Func<IAlunoRepository> _alunos=alunos;
    private async Task<MatriculaDto> Dto(Domain.Entities.Matricula x,CancellationToken t){var a=await _alunos().ObterPorId(x.AlunoId,t)??throw new KeyNotFoundException();return x.ToDto(a.ToDto());}
    public async Task<MatriculaDto?> ObterPorIdAsync(int id,CancellationToken t=default){var x=await _repository().ObterPorId(id,t);return x==null?null:await Dto(x,t);} public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken t=default)=>(await _repository().ObterTodos(t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int id,CancellationToken t=default)=>(await _repository().ObterPorAluno(id,t)).Select(x=>Dto(x,t).Result); public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int id,CancellationToken t=default){var x=await _repository().ObterMatriculaAtivaPorAluno(id,t);return x==null?null:await Dto(x,t);} public Task<bool> PossuiMatriculaAtivaAsync(int id,CancellationToken t=default)=>_repository().PossuiMatriculaAtiva(id,t); public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int id=0,CancellationToken t=default)=>(await _repository().ObterAtivas(id,t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int d,CancellationToken t=default)=>(await _repository().ObterVencendoEmDias(d,t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano p,CancellationToken t=default)=>(await _repository().ObterPorPlano((Domain.Enums.MatriculaPlano)p,t)).Select(x=>Dto(x,t).Result);
    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto dto,CancellationToken t=default){var a=await _alunos().ObterPorId(dto.AlunoMatricula.Id,t)??throw new KeyNotFoundException();if(await _repository().PossuiMatriculaAtiva(a.Id,t))throw new InvalidOperationException("Matricula ativa existente");return await Dto(await _repository().Adicionar(dto.ToEntity(a),t),t);} public async Task<MatriculaDto> AtualizarAsync(MatriculaDto dto,CancellationToken t=default){var old=await _repository().ObterPorId(dto.Id,t)??throw new KeyNotFoundException();var a=await _alunos().ObterPorId(old.AlunoId,t)??throw new KeyNotFoundException();return await Dto(await _repository().Atualizar(dto.ToEntity(a),t),t);} public Task<bool> RemoverAsync(int id,CancellationToken t=default)=>_repository().Remover(id,t);
}
