// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorViewModel(IColaboradorService service, ILogradouroService logradouroService) : ObservableObject
{
    private readonly IColaboradorService _service = service;
    private readonly ILogradouroService _logradouroService = logradouroService;

    public AppColaboradorTipo[] ColaboradorTipos { get; } = Enum.GetValues<AppColaboradorTipo>();
    public AppColaboradorVinculo[] ColaboradorVinculos { get; } = Enum.GetValues<AppColaboradorVinculo>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(HasEnderecoVinculado))]
    [NotifyPropertyChangedFor(nameof(HasNoEnderecoVinculado))]
    private ColaboradorDto colaborador = NovoColaborador();

    [ObservableProperty]
    private string confirmarSenha = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool IsEditMode => Colaborador.Id > 0;
    public bool HasEnderecoVinculado => Colaborador.Endereco?.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public void Definir(ColaboradorDto? item)
    {
        Colaborador = item ?? NovoColaborador();
        ConfirmarSenha = string.Empty;
    }

    [RelayCommand]
    private async Task BuscarCepAsync()
    {
        try
        {
            var cep = Colaborador.Endereco?.Cep ?? string.Empty;
            var endereco = await _logradouroService.ObterPorCepAsync(cep);

            if (endereco == null)
            {
                await Shell.Current.DisplayAlertAsync("Atencao", "Logradouro nao encontrado.", "Ok");
                return;
            }

            Colaborador.Endereco = endereco;
            OnPropertyChanged(nameof(Colaborador));
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

        if (string.IsNullOrWhiteSpace(Colaborador.Nome) || string.IsNullOrWhiteSpace(Colaborador.Cpf) || string.IsNullOrWhiteSpace(Colaborador.Telefone) || string.IsNullOrWhiteSpace(Colaborador.Email) || string.IsNullOrWhiteSpace(Colaborador.Numero))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Preencha os campos obrigatorios.", "Ok");
            return;
        }

        if (Colaborador.Endereco?.Id <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Busque um CEP valido.", "Ok");
            return;
        }

        if (!IsEditMode && string.IsNullOrWhiteSpace(Colaborador.Senha))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Informe uma senha.", "Ok");
            return;
        }

        if (!string.Equals(Colaborador.Senha ?? string.Empty, ConfirmarSenha, StringComparison.Ordinal))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "As senhas nao conferem.", "Ok");
            return;
        }

        if (Colaborador.Foto == null)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione ou tire uma foto.", "Ok");
            return;
        }

        try
        {
            IsBusy = true;
            if (IsEditMode) await _service.AtualizarAsync(Colaborador); else await _service.AdicionarAsync(Colaborador);
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
        Colaborador.Foto = new ArquivoDto { Conteudo = memory.ToArray() };
        OnPropertyChanged(nameof(Colaborador));
    }

    private static ColaboradorDto NovoColaborador() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Endereco = new LogradouroDto { Id = 0, Cep = string.Empty, Nome = string.Empty, Bairro = string.Empty, Cidade = string.Empty, Estado = string.Empty, Pais = "Brasil" },
        Numero = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Instrutor,
        Vinculo = AppColaboradorVinculo.CLT
    };
}
