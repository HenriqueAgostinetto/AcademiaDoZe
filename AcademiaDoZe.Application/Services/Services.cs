// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class LogradouroService(Func<ILogradouroRepository> repository) : ILogradouroService
{
    private readonly Func<ILogradouroRepository> _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken token = default) => (await _repository().ObterPorId(id, token))?.ToDto();
    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken token = default) => (await _repository().ObterTodos(token)).Select(item => item.ToDto());
    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken token = default) => (await _repository().ObterPorCep(ValidarCep(cep), token))?.ToDto();
    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken token = default) => (await _repository().ObterPorCidade(Obrigatorio(cidade, nameof(cidade)), token)).Select(item => item.ToDto());
    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken token = default) => (await _repository().ObterPorBairro(Obrigatorio(cidade, nameof(cidade)), Obrigatorio(bairro, nameof(bairro)), token)).Select(item => item.ToDto());
    public async Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken token = default) => !string.IsNullOrWhiteSpace(cep) && await _repository().CepJaExiste(ValidarCep(cep), id, token);
    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto dto, CancellationToken token = default) { ArgumentNullException.ThrowIfNull(dto); var cep = ValidarCep(dto.Cep); if (await _repository().CepJaExiste(cep, null, token)) throw new InvalidOperationException("CEP ja cadastrado"); return (await _repository().Adicionar(dto.ToEntity(), token)).ToDto(); }
    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto dto, CancellationToken token = default) { ArgumentNullException.ThrowIfNull(dto); var atual = await _repository().ObterPorId(dto.Id, token) ?? throw new KeyNotFoundException("Logradouro nao encontrado"); if (await _repository().CepJaExiste(ValidarCep(dto.Cep), dto.Id, token)) throw new InvalidOperationException("CEP ja cadastrado"); return (await _repository().Atualizar(dto.ToEntity(), token)).ToDto(); }
    public async Task<bool> RemoverAsync(int id, CancellationToken token = default) => await _repository().Remover(id, token);
    private static Cep ValidarCep(string value) { var result = Cep.Criar(value); return result.IsSuccess ? result.Value! : throw new ArgumentException("CEP invalido", nameof(value)); }
    private static string Obrigatorio(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Valor obrigatorio", name) : value.Trim();
}

public class AlunoService(Func<IAlunoRepository> repository, Func<ILogradouroRepository> logradouros) : IAlunoService
{
    private readonly Func<IAlunoRepository> _repository = repository ?? throw new ArgumentNullException(nameof(repository)); private readonly Func<ILogradouroRepository> _logradouros = logradouros ?? throw new ArgumentNullException(nameof(logradouros));
    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken token = default) { var item = await _repository().ObterPorId(id, token); return item == null ? null : item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token)); }
    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken token = default) { var items = await _repository().ObterTodos(token); return await Mapear(items, token); }
    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken token = default) { var item = await _repository().ObterPorCpf(ValidarCpf(cpf), token); return item?.ToDto(); }
    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken token = default) { var item = await _repository().ObterPorEmail(ValidarEmail(email), token); return item?.ToDto(); }
    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken token = default) => (await _repository().ObterPorNome(Obrigatorio(nome, nameof(nome)), token)).Select(item => item.ToDto());
    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken token = default) => !string.IsNullOrWhiteSpace(cpf) && await _repository().CpfJaExiste(ValidarCpf(cpf), id, token);
    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken token = default) => !string.IsNullOrWhiteSpace(email) && await _repository().EmailJaExiste(ValidarEmail(email), id, token);
    public async Task<AlunoDto> AdicionarAsync(AlunoDto dto, CancellationToken token = default) { var endereco = await Endereco(dto, token); var cpf = ValidarCpf(dto.Cpf); if (await _repository().CpfJaExiste(cpf, null, token)) throw new InvalidOperationException("CPF ja cadastrado"); if (await _repository().EmailJaExiste(ValidarEmail(dto.Email), null, token)) throw new InvalidOperationException("Email ja cadastrado"); var item = await _repository().Adicionar(dto.ToEntity(endereco, PasswordHasher.Hash(Obrigatorio(dto.Senha!, "senha"))), token); return item.ToDto(endereco); }
    public async Task<AlunoDto> AtualizarAsync(AlunoDto dto, CancellationToken token = default) { var atual = await _repository().ObterPorId(dto.Id, token) ?? throw new KeyNotFoundException("Aluno nao encontrado"); var endereco = await Endereco(dto, token, atual.Endereco.LogradouroId); if (await _repository().CpfJaExiste(ValidarCpf(dto.Cpf), dto.Id, token)) throw new InvalidOperationException("CPF ja cadastrado"); if (await _repository().EmailJaExiste(ValidarEmail(dto.Email), dto.Id, token)) throw new InvalidOperationException("Email ja cadastrado"); var senha = string.IsNullOrWhiteSpace(dto.Senha) ? atual.Senha.Valor : PasswordHasher.Hash(dto.Senha); return (await _repository().Atualizar(dto.ToEntity(endereco, senha), token)).ToDto(endereco); }
    public Task<bool> RemoverAsync(int id, CancellationToken token = default) => _repository().Remover(id, token);
    public Task<bool> TrocarSenhaAsync(int id, string senha, CancellationToken token = default) => _repository().TrocarSenha(id, Senha.Criar(PasswordHasher.Hash(Obrigatorio(senha, nameof(senha)))).Value!, token);
    private async Task<AcademiaDoZe.Domain.Entities.Logradouro> Endereco(AlunoDto dto, CancellationToken token, int fallback = 0) => await _logradouros().ObterPorId(dto.Endereco?.Id > 0 ? dto.Endereco.Id : fallback, token) ?? throw new KeyNotFoundException("Logradouro nao encontrado");
    private async Task<IEnumerable<AlunoDto>> Mapear(IEnumerable<AcademiaDoZe.Domain.Entities.Aluno> items, CancellationToken token) { var result = new List<AlunoDto>(); foreach (var item in items) result.Add(item.ToDto(await _logradouros().ObterPorId(item.Endereco.LogradouroId, token))); return result; }
    private static Cpf ValidarCpf(string value) { var result = Cpf.Criar(value); return result.IsSuccess ? result.Value! : throw new ArgumentException("CPF invalido"); } private static Email ValidarEmail(string? value) { var result = Email.Criar(value ?? string.Empty); return result.IsSuccess ? result.Value! : throw new ArgumentException("Email invalido"); } private static string Obrigatorio(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Valor obrigatorio", name) : value.Trim();
}
