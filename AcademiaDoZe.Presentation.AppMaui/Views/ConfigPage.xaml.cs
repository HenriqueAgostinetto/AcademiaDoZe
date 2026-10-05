// henrique agostinetto piva
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }

    private void CarregarTema() => TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch
    {
        "light" => 0,
        "dark" => 1,
        _ => 2
    };

    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();
        foreach (var tipo in Enum.GetValues<AppDatabaseType>()) DatabaseTypePicker.Items.Add(tipo.ToString());
        DatabaseTypePicker.SelectedItem = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(object? sender, EventArgs eventArgs) => AtualizarInterfacePorTipoBanco();

    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string texto || !Enum.TryParse(texto, out AppDatabaseType tipo)) return;

        var sqlite = tipo == AppDatabaseType.Sqlite;
        SqliteInfoCard.IsVisible = sqlite;
        SqliteContainer.IsVisible = sqlite;
        ServidorBancoGrid.IsVisible = !sqlite;
        CredenciaisGrid.IsVisible = !sqlite;

        if (sqlite)
        {
            SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Path.Combine(FileSystem.AppDataDirectory, "academia_do_ze.db"));
            ComplementoLabel.Text = "Complemento";
            ComplementoEntry.Text = Preferences.Get("Sqlite_Complemento", "Default Timeout=5;");
            ComplementoEntry.Placeholder = "Default Timeout=5;";
            return;
        }

        var prefixo = tipo == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
        var servidor = tipo == AppDatabaseType.SqlServer ? "127.0.0.1,1433" : "127.0.0.1";
        var usuario = tipo == AppDatabaseType.SqlServer ? "sa" : "root";
        var complemento = tipo == AppDatabaseType.SqlServer ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;" : "Port=3306;Connection Timeout=5;";
        ServidorEntry.Text = Preferences.Get($"{prefixo}_Servidor", servidor);
        BancoEntry.Text = Preferences.Get($"{prefixo}_Banco", "db_academia_do_ze");
        UsuarioEntry.Text = Preferences.Get($"{prefixo}_Usuario", usuario);
        SenhaEntry.Text = Preferences.Get($"{prefixo}_Senha", string.Empty);
        ComplementoEntry.Text = Preferences.Get($"{prefixo}_Complemento", complemento);
        ComplementoLabel.Text = "Complemento";
        ComplementoEntry.Placeholder = complemento;
    }

    private async void OnSalvarTemaClicked(object? sender, EventArgs eventArgs)
    {
        var tema = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };
        Preferences.Set("Tema", tema);
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage(tema));
        await DisplayAlertAsync("Sucesso", "Tema salvo com sucesso.", "OK");
    }

    private async void OnSalvarBdClicked(object? sender, EventArgs eventArgs)
    {
        if (DatabaseTypePicker.SelectedItem is not string texto || !Enum.TryParse(texto, out AppDatabaseType tipo))
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco valido.", "OK");
            return;
        }

        if (tipo == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync("Aviso", "Informe o caminho do banco SQLite.", "OK");
                return;
            }
            Preferences.Set("Sqlite_Caminho", SqliteCaminhoEntry.Text.Trim());
            Preferences.Set("Sqlite_Complemento", ComplementoEntry.Text?.Trim() ?? string.Empty);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ServidorEntry.Text) || string.IsNullOrWhiteSpace(BancoEntry.Text) || string.IsNullOrWhiteSpace(UsuarioEntry.Text))
            {
                await DisplayAlertAsync("Aviso", "Preencha servidor, banco e usuario.", "OK");
                return;
            }
            var prefixo = tipo == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            Preferences.Set($"{prefixo}_Servidor", ServidorEntry.Text.Trim());
            Preferences.Set($"{prefixo}_Banco", BancoEntry.Text.Trim());
            Preferences.Set($"{prefixo}_Usuario", UsuarioEntry.Text.Trim());
            Preferences.Set($"{prefixo}_Senha", SenhaEntry.Text ?? string.Empty);
            Preferences.Set($"{prefixo}_Complemento", ComplementoEntry.Text?.Trim() ?? string.Empty);
        }

        Preferences.Set("DatabaseType", tipo.ToString());
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage(tipo.ToString()));
        await DisplayAlertAsync("Sucesso", "Configuracoes do banco salvas com sucesso.", "OK");
    }

    private async void OnCancelarClicked(object? sender, EventArgs eventArgs) => await Shell.Current.GoToAsync("//dashboard");

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
