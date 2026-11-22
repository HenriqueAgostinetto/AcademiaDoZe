// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MinhaContaPage : ContentPage
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;

    public MinhaContaPage(IAlunoService alunoService, IColaboradorService colaboradorService)
    {
        InitializeComponent();
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;
        MensagemLabel.IsVisible = false;
        var perfil = Enum.TryParse(Preferences.Get("PerfilSessao", "Aluno"), out AppPerfil valor) ? valor : AppPerfil.Aluno;
        var nomeSessao = Preferences.Get("NomeSessao", string.Empty);
        if (perfil == AppPerfil.Aluno)
        {
            var aluno = (await _alunoService.ObterPorNomeAsync(nomeSessao)).FirstOrDefault() ?? (await _alunoService.ObterTodosAsync()).FirstOrDefault();
            Preencher(aluno, "Aluno", "Matricula ativa");
        }
        else
        {
            var colaborador = (await _colaboradorService.ObterTodosAsync()).FirstOrDefault(item => item.Nome.Equals(nomeSessao, StringComparison.OrdinalIgnoreCase)) ?? (await _colaboradorService.ObterTodosAsync()).FirstOrDefault();
            Preencher(colaborador, perfil == AppPerfil.Administrador ? "Administrador" : "Colaborador", perfil == AppPerfil.Administrador ? "Acesso administrativo" : "Colaborador ativo");
        }
        CarregandoIndicator.IsRunning = false;
        CarregandoIndicator.IsVisible = false;
    }

    private void Preencher(PessoaDto? pessoa, string perfil, string situacao)
    {
        if (pessoa == null)
        {
            MensagemLabel.Text = "Nenhum cadastro foi encontrado no banco selecionado.";
            MensagemLabel.IsVisible = true;
            return;
        }
        NomeLabel.Text = pessoa.Nome;
        PerfilLabel.Text = perfil;
        CpfLabel.Text = pessoa.Cpf;
        NascimentoLabel.Text = pessoa.DataNascimento.ToString("dd/MM/yyyy");
        TelefoneLabel.Text = pessoa.Telefone;
        EmailLabel.Text = pessoa.Email;
        EnderecoLabel.Text = pessoa.Endereco == null ? $"{pessoa.Numero} - {pessoa.Complemento}" : $"{pessoa.Endereco.Nome}, {pessoa.Numero} - {pessoa.Endereco.Bairro}, {pessoa.Endereco.Cidade}/{pessoa.Endereco.Estado}";
        SituacaoLabel.Text = situacao;
        FotoImage.Source = new FotoToImageSourceConverter().Convert(pessoa.Foto, typeof(ImageSource), null, CultureInfo.CurrentCulture) as ImageSource;
    }
}
