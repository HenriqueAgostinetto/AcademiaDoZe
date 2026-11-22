// henrique agostinetto piva
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Presentation.AppMaui.Visual;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = PageTransition.AparecerAsync(this);
    }

    private async void OnLoginClicked(object? sender, EventArgs eventArgs)
    {
        var sessao = AutenticacaoService.Autenticar(LoginEntry.Text, SenhaEntry.Text);
        if (sessao == null)
        {
            MensagemLabel.Text = "Login ou senha invalidos.";
            MensagemLabel.IsVisible = true;
            await this.ScaleToAsync(0.99, 70);
            await this.ScaleToAsync(1, 110, Easing.CubicOut);
            return;
        }

        Preferences.Set("NomeSessao", sessao.Nome);
        Preferences.Set("PerfilSessao", sessao.Perfil.ToString());
        MensagemLabel.TextColor = Color.FromArgb("#237A4B");
        MensagemLabel.Text = $"Login bem-sucedido. Perfil: {sessao.Perfil}.";
        MensagemLabel.IsVisible = true;
        EntrarButton.IsEnabled = false;
        await Task.Delay(280);
        Microsoft.Maui.Controls.Application.Current?.Windows[0].Page = new AppShell();
    }
}
