// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoViewModel(IAlunoService service, ILogradouroService logradouroService) : ObservableObject
{
    private readonly IAlunoService _service = service;
    private readonly ILogradouroService _logradouroService = logradouroService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(HasEnderecoVinculado))]
    [NotifyPropertyChangedFor(nameof(HasNoEnderecoVinculado))]
    private AlunoDto aluno = NovoAluno();

    [ObservableProperty]
    private string confirmarSenha = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private LogradouroDto? enderecoSelecionado;

    public bool IsEditMode => Aluno.Id > 0;
    public bool HasEnderecoVinculado => Aluno.Endereco?.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;
    public ObservableCollection<LogradouroDto> Logradouros { get; } = [];

    public void Definir(AlunoDto? item)
    {
        Aluno = item ?? NovoAluno();
        ConfirmarSenha = string.Empty;
        EnderecoSelecionado = Logradouros.FirstOrDefault(logradouro => logradouro.Id == Aluno.Endereco?.Id);
    }

    public async Task CarregarLogradourosAsync()
    {
        var selecionado = Aluno.Endereco?.Id;
        var itens = await _logradouroService.ObterTodosAsync();

        Logradouros.Clear();
        foreach (var item in itens.OrderBy(item => item.Nome)) Logradouros.Add(item);

        EnderecoSelecionado = Logradouros.FirstOrDefault(logradouro => logradouro.Id == selecionado);
    }

    partial void OnEnderecoSelecionadoChanged(LogradouroDto? value)
    {
        if (value == null) return;

        Aluno.Endereco = value;
        OnPropertyChanged(nameof(Aluno));
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    [RelayCommand]
    private async Task BuscarCepAsync()
    {
        try
        {
            var cep = Aluno.Endereco?.Cep ?? string.Empty;
            var endereco = await _logradouroService.ObterPorCepAsync(cep);

            if (endereco == null)
            {
                await Shell.Current.DisplayAlertAsync("Atencao", "Logradouro nao encontrado.", "Ok");
                return;
            }

            Aluno.Endereco = endereco;
            EnderecoSelecionado = Logradouros.FirstOrDefault(logradouro => logradouro.Id == endereco.Id);
            OnPropertyChanged(nameof(Aluno));
            OnPropertyChanged(nameof(HasEnderecoVinculado));
            OnPropertyChanged(nameof(HasNoEnderecoVinculado));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        var arquivos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Selecione uma foto" });
        await DefinirFotoAsync(arquivos?.FirstOrDefault());
    }

    [RelayCommand]
    private async Task TirarFotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Camera indisponivel neste dispositivo.", "Ok");
            return;
        }

        var arquivo = await MediaPicker.Default.CapturePhotoAsync();
        await DefinirFotoAsync(arquivo);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Aluno.Nome) || string.IsNullOrWhiteSpace(Aluno.Cpf) || string.IsNullOrWhiteSpace(Aluno.Telefone) || string.IsNullOrWhiteSpace(Aluno.Email) || string.IsNullOrWhiteSpace(Aluno.Numero))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Preencha os campos obrigatorios.", "Ok");
            return;
        }

        if (Aluno.Endereco?.Id <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Busque um CEP valido.", "Ok");
            return;
        }

        if (!IsEditMode && string.IsNullOrWhiteSpace(Aluno.Senha))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Informe uma senha.", "Ok");
            return;
        }

        if (!string.Equals(Aluno.Senha ?? string.Empty, ConfirmarSenha, StringComparison.Ordinal))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "As senhas nao conferem.", "Ok");
            return;
        }

        if (Aluno.Foto == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione ou tire uma foto.", "Ok");
            return;
        }

        try
        {
            IsBusy = true;
            if (IsEditMode) await _service.AtualizarAsync(Aluno); else await _service.AdicionarAsync(Aluno);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task CancelarAsync() => Shell.Current.GoToAsync("..");

    private async Task DefinirFotoAsync(FileResult? arquivo)
    {
        if (arquivo == null) return;

        await using var stream = await arquivo.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        Aluno.Foto = new ArquivoDto { Conteudo = memory.ToArray() };
        OnPropertyChanged(nameof(Aluno));
    }

    private static AlunoDto NovoAluno() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Endereco = new LogradouroDto { Id = 0, Cep = string.Empty, Nome = string.Empty, Bairro = string.Empty, Cidade = string.Empty, Estado = string.Empty, Pais = "Brasil" },
        Numero = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty
    };
}
