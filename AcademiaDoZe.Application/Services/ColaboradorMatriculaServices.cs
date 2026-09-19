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
    private readonly Func<IColaboradorRepository> _repository = repository; private readonly Func<ILogradouroRepository> _logradouros = logradouros;
    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken t = default) { var x = await _repository().ObterPorId(id,t); return x?.ToDto(await _logradouros().ObterPorId(x.Endereco.LogradouroId,t)); }
    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken t=default) => (await _repository().ObterTodos(t)).Select(x=>x.ToDto());
    public async Task<ColaboradorDto?> ObterPorCpfAsync(string v,CancellationToken t=default)=>(await _repository().ObterPorCpf(Cpf.Criar(v).Value!,t))?.ToDto(); public async Task<ColaboradorDto?> ObterPorEmailAsync(string v,CancellationToken t=default)=>(await _repository().ObterPorEmail(Email.Criar(v).Value!,t))?.ToDto();
    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo v,CancellationToken t=default)=>(await _repository().ObterPorTipo((Domain.Enums.ColaboradorTipo)v,t)).Select(x=>x.ToDto()); public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo v,CancellationToken t=default)=>(await _repository().ObterPorVinculo((Domain.Enums.ColaboradorVinculo)v,t)).Select(x=>x.ToDto());
    public async Task<bool> CpfJaExisteAsync(string v,int? id=null,CancellationToken t=default)=>await _repository().CpfJaExiste(Cpf.Criar(v).Value!,id,t); public async Task<bool> EmailJaExisteAsync(string v,int? id=null,CancellationToken t=default)=>await _repository().EmailJaExiste(Email.Criar(v).Value!,id,t);
    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto dto,CancellationToken t=default){var e=await _logradouros().ObterPorId(dto.Endereco!.Id,t)??throw new KeyNotFoundException(); if(await CpfJaExisteAsync(dto.Cpf,null,t))throw new InvalidOperationException("CPF ja cadastrado"); return (await _repository().Adicionar(dto.ToEntity(e,PasswordHasher.Hash(dto.Senha!)),t)).ToDto(e);} public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto dto,CancellationToken t=default){var e=await _logradouros().ObterPorId(dto.Endereco!.Id,t)??throw new KeyNotFoundException();return (await _repository().Atualizar(dto.ToEntity(e,string.IsNullOrWhiteSpace(dto.Senha)?(await _repository().ObterPorId(dto.Id,t))!.Senha.Valor:PasswordHasher.Hash(dto.Senha)),t)).ToDto(e);} public Task<bool> RemoverAsync(int id,CancellationToken t=default)=>_repository().Remover(id,t); public Task<bool> TrocarSenhaAsync(int id,string s,CancellationToken t=default)=>_repository().TrocarSenha(id,Senha.Criar(PasswordHasher.Hash(s)).Value!,t);
}

public class MatriculaService(Func<IMatriculaRepository> repository, Func<IAlunoRepository> alunos) : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _repository=repository; private readonly Func<IAlunoRepository> _alunos=alunos;
    private async Task<MatriculaDto> Dto(Domain.Entities.Matricula x,CancellationToken t){var a=await _alunos().ObterPorId(x.AlunoId,t)??throw new KeyNotFoundException();return x.ToDto(a.ToDto());}
    public async Task<MatriculaDto?> ObterPorIdAsync(int id,CancellationToken t=default){var x=await _repository().ObterPorId(id,t);return x==null?null:await Dto(x,t);} public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken t=default)=>(await _repository().ObterTodos(t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int id,CancellationToken t=default)=>(await _repository().ObterPorAluno(id,t)).Select(x=>Dto(x,t).Result); public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int id,CancellationToken t=default){var x=await _repository().ObterMatriculaAtivaPorAluno(id,t);return x==null?null:await Dto(x,t);} public Task<bool> PossuiMatriculaAtivaAsync(int id,CancellationToken t=default)=>_repository().PossuiMatriculaAtiva(id,t); public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int id=0,CancellationToken t=default)=>(await _repository().ObterAtivas(id,t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int d,CancellationToken t=default)=>(await _repository().ObterVencendoEmDias(d,t)).Select(x=>Dto(x,t).Result); public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano p,CancellationToken t=default)=>(await _repository().ObterPorPlano((Domain.Enums.MatriculaPlano)p,t)).Select(x=>Dto(x,t).Result);
    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto dto,CancellationToken t=default){var a=await _alunos().ObterPorId(dto.AlunoMatricula.Id,t)??throw new KeyNotFoundException();if(await _repository().PossuiMatriculaAtiva(a.Id,t))throw new InvalidOperationException("Matricula ativa existente");return await Dto(await _repository().Adicionar(dto.ToEntity(a),t),t);} public async Task<MatriculaDto> AtualizarAsync(MatriculaDto dto,CancellationToken t=default){var old=await _repository().ObterPorId(dto.Id,t)??throw new KeyNotFoundException();var a=await _alunos().ObterPorId(old.AlunoId,t)??throw new KeyNotFoundException();return await Dto(await _repository().Atualizar(dto.ToEntity(a),t),t);} public Task<bool> RemoverAsync(int id,CancellationToken t=default)=>_repository().Remover(id,t);
}
