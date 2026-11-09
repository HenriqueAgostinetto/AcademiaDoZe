using System.Collections.ObjectModel;
using System.ComponentModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaViewModel(IMatriculaService service, IAlunoService alunoService) : ObservableObject
{
    private readonly IMatriculaService _service = service;
    private readonly IAlunoService _alunoService = alunoService;
    private bool _suspendSelectionSync;

    public IEnumerable<AppMatriculaPlano> Planos { get; } = Enum.GetValues<AppMatriculaPlano>();
    public ObservableCollection<AlunoDto> Alunos { get; } = [];
    public ObservableCollection<RestricaoSelectable> Restricoes { get; } = [];

    [ObservableProperty]
    private MatriculaDto matricula = NovaMatricula();

    [ObservableProperty]
    private AlunoDto? alunoSelecionado;

    [ObservableProperty]
    private string cpfPesquisa = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private int matriculaId;

    public bool IsEditMode => Matricula.Id > 0;
    public bool PodeSelecionarAluno => !IsEditMode;
    public bool HasAlunoVinculado => Matricula.AlunoMatricula?.Id > 0;
    public bool IsMenor16 => Matricula.AlunoMatricula != null && Matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
    public bool HasRestricoes => Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;
    public bool ExigeLaudo => IsMenor16 || HasRestricoes;
    public bool HasLaudo => Matricula.LaudoMedico?.Conteudo.Length > 0;
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;
    public bool LaudoOpcional => !ExigeLaudo && !HasLaudo;

    partial void OnMatriculaChanged(MatriculaDto value)
    {
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PodeSelecionarAluno));
        NotificarPropriedadesVisuais();
        SincronizarSelecao();
    }

    partial void OnAlunoSelecionadoChanged(AlunoDto? value)
    {
        if (value == null) return;
        Matricula.AlunoMatricula = value;
        CpfPesquisa = value.Cpf;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    public async Task InicializarAsync()
    {
        try
        {
            IsBusy = true;
            Alunos.Clear();
            foreach (var item in (await _alunoService.ObterTodosAsync()).OrderBy(item => item.Nome)) Alunos.Add(item);

            Matricula = MatriculaId > 0
                ? await _service.ObterPorIdAsync(MatriculaId) ?? NovaMatricula()
                : NovaMatricula();
            AlunoSelecionado = Alunos.FirstOrDefault(item => item.Id == Matricula.AlunoMatricula.Id);
            CpfPesquisa = Matricula.AlunoMatricula.Cpf;
            MontarRestricoes();
            AtualizarDataFim();
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

    public void AtualizarDataFim()
    {
        var inicio = Matricula.DataInicio;
        Matricula.DataFim = Matricula.Plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };
        OnPropertyChanged(nameof(Matricula));
    }

    [RelayCommand]
    private async Task BuscarCpfAsync()
    {
        if (IsBusy || IsEditMode) return;

        if (string.IsNullOrWhiteSpace(CpfPesquisa))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Informe o CPF do aluno.", "Ok");
            return;
        }

        try
        {
            IsBusy = true;
            var cpf = new string(CpfPesquisa.Where(char.IsDigit).ToArray());
            var aluno = await _alunoService.ObterPorCpfAsync(cpf);
            if (aluno == null)
            {
                await Shell.Current.DisplayAlertAsync("Atencao", "Aluno nao encontrado.", "Ok");
                return;
            }

            AlunoSelecionado = Alunos.FirstOrDefault(item => item.Id == aluno.Id) ?? aluno;
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
    private async Task SelecionarLaudoAsync()
    {
        try
        {
            var arquivo = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Selecione a imagem do laudo", FileTypes = FilePickerFileType.Images });
            await DefinirLaudoAsync(arquivo);
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private async Task TirarFotoLaudoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Camera indisponivel neste dispositivo.", "Ok");
            return;
        }

        try
        {
            await DefinirLaudoAsync(await MediaPicker.Default.CapturePhotoAsync());
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    private async Task DefinirLaudoAsync(FileResult? arquivo)
    {
        if (arquivo == null) return;

        await using var stream = await arquivo.OpenReadAsync();
        using var memoria = new MemoryStream();
        await stream.CopyToAsync(memoria);
        Matricula.LaudoMedico = new ArquivoDto { Conteudo = memoria.ToArray() };
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (IsBusy) return;
        RecalcularRestricoes();

        if (!HasAlunoVinculado)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Selecione um aluno.", "Ok");
            return;
        }
        if (string.IsNullOrWhiteSpace(Matricula.Objetivo))
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Informe o objetivo da matricula.", "Ok");
            return;
        }
        if (ExigeLaudo && !HasLaudo)
        {
            await Shell.Current.DisplayAlertAsync("Atencao", "Anexe o laudo medico para salvar esta matricula.", "Ok");
            return;
        }

        try
        {
            IsBusy = true;
            if (IsEditMode) await _service.AtualizarAsync(Matricula);
            else await _service.AdicionarAsync(Matricula);
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

    private void MontarRestricoes()
    {
        Restricoes.Clear();
        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None) continue;
            var item = new RestricaoSelectable { Value = value, IsSelected = Matricula.RestricoesMedicas.HasFlag(value) };
            item.PropertyChanged += RestricaoAlterada;
            Restricoes.Add(item);
        }
    }

    private void RestricaoAlterada(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(RestricaoSelectable.IsSelected) && !_suspendSelectionSync) RecalcularRestricoes();
    }

    private void RecalcularRestricoes()
    {
        var flags = AppMatriculaRestricoes.None;
        foreach (var item in Restricoes)
            if (item.IsSelected) flags |= item.Value;

        Matricula.RestricoesMedicas = flags;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    private void SincronizarSelecao()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in Restricoes) item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
        }
        finally
        {
            _suspendSelectionSync = false;
        }
    }

    private void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }

    private static MatriculaDto NovaMatricula() => new()
    {
        AlunoMatricula = new AlunoDto { Nome = string.Empty, Cpf = string.Empty, DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)), Telefone = string.Empty, Numero = string.Empty },
        Plano = AppMatriculaPlano.Anual,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.None
    };
}
