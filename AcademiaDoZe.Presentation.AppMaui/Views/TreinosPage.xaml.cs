// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.Visual;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Security;
using System.Text.Json;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class TreinosPage : ContentPage
{
    private const string ChaveTreinos = "TreinosDemonstracao";

    public TreinosPage()
    {
        InitializeComponent();
        var perfil = PerfilAtual;
        GestaoTreinoCard.IsVisible = perfil != AppPerfil.Aluno;
        AlunoPicker.Items.Add("Henrique 2");
        AlunoPicker.SelectedIndex = 0;
        ListaTitulo.Text = perfil == AppPerfil.Aluno ? "Meu treino" : "Treinos disponiveis";
        CarregarTreinos();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = PageTransition.AparecerAsync(Conteudo);
    }

    private AppPerfil PerfilAtual => Enum.TryParse(Preferences.Get("PerfilSessao", "Aluno"), out AppPerfil perfil) ? perfil : AppPerfil.Aluno;

    private static List<TreinoItem> ObterTreinos()
    {
        var json = Preferences.Get(ChaveTreinos, string.Empty);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var itensSalvos = JsonSerializer.Deserialize<List<TreinoItem>>(json) ?? [];
            AtualizarAgendaSalva(itensSalvos);
            return itensSalvos;
        }
        var itens = TreinoAgendaService.ObterAgendaDoAluno("Henrique 2")
            .Select(item => new TreinoItem
            {
                AlunoLogin = item.AlunoLogin,
                AlunoNome = item.AlunoNome,
                PersonalLogin = item.PersonalLogin,
                PersonalNome = item.PersonalNome,
                Dia = item.Dia,
                Nome = item.Nome
            }).ToList();
        SalvarTreinos(itens);
        return itens;
    }

    private static void AtualizarAgendaSalva(List<TreinoItem> itens)
    {
        if (itens.Count == 0 || itens.Any(item => !string.IsNullOrWhiteSpace(item.Dia))) return;
        var agenda = TreinoAgendaService.ObterAgendaDoAluno("Henrique 2");
        for (var indice = 0; indice < itens.Count && indice < agenda.Count; indice++) itens[indice].Dia = agenda[indice].Dia;
        SalvarTreinos(itens);
    }

    private static void SalvarTreinos(List<TreinoItem> itens) => Preferences.Set(ChaveTreinos, JsonSerializer.Serialize(itens));

    private void CarregarTreinos()
    {
        var login = Preferences.Get("NomeSessao", string.Empty);
        TreinosCollection.ItemsSource = ObterTreinos().Where(item => TreinoPermissaoService.PodeVisualizar(PerfilAtual, login, item.AlunoLogin, item.PersonalLogin)).ToList();
    }

    private async void OnSalvarTreinoClicked(object? sender, EventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(TreinoEntry.Text) || AlunoPicker.SelectedItem is not string aluno)
        {
            await DisplayAlertAsync("Atencao", "Informe o treino e selecione o aluno.", "Ok");
            return;
        }
        var itens = ObterTreinos();
        var personal = Preferences.Get("NomeSessao", "Personal");
        itens.Add(new TreinoItem { AlunoLogin = aluno, AlunoNome = aluno, PersonalLogin = personal, PersonalNome = personal, Dia = "Treino adicional", Nome = TreinoEntry.Text.Trim() });
        SalvarTreinos(itens);
        TreinoEntry.Text = string.Empty;
        CarregarTreinos();
    }

    private async void OnImportarTreinoClicked(object? sender, EventArgs eventArgs)
    {
        var arquivo = await FilePicker.Default.PickAsync();
        if (arquivo == null) return;
        if (AlunoPicker.SelectedItem is not string aluno) return;
        var itens = ObterTreinos();
        var personal = Preferences.Get("NomeSessao", "Personal");
        itens.Add(new TreinoItem { AlunoLogin = aluno, AlunoNome = aluno, PersonalLogin = personal, PersonalNome = personal, Dia = "Treino importado", Nome = $"Arquivo importado: {arquivo.FileName}" });
        SalvarTreinos(itens);
        CarregarTreinos();
    }

    private sealed class TreinoItem
    {
        public required string AlunoLogin { get; set; }
        public required string AlunoNome { get; set; }
        public required string PersonalLogin { get; set; }
        public required string PersonalNome { get; set; }
        public string? Dia { get; set; }
        public required string Nome { get; set; }
    }
}
