// henrique agostinetto piva
using AcademiaDoZe.Presentation.AppMaui.Message;
using AcademiaDoZe.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
		AplicarTema();
		WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (_, _) => AplicarTema());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new LoginPage());
	}

	private void AplicarTema() => UserAppTheme = Preferences.Get("Tema", "system") switch
	{
		"light" => AppTheme.Light,
		"dark" => AppTheme.Dark,
		_ => AppTheme.Unspecified
	};
}
