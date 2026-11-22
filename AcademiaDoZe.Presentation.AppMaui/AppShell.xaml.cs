// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
	public static readonly BindableProperty FotoContaProperty = BindableProperty.Create(nameof(FotoConta), typeof(ImageSource), typeof(AppShell));

	public ImageSource? FotoConta
	{
		get => (ImageSource?)GetValue(FotoContaProperty);
		set => SetValue(FotoContaProperty, value);
	}

	public AppShell()
	{
		InitializeComponent();
		AplicarPermissoes();
		Navigating += OnNavigating;
		Routing.RegisterRoute(nameof(Views.LogradouroPage), typeof(Views.LogradouroPage));
		Routing.RegisterRoute(nameof(Views.ColaboradorPage), typeof(Views.ColaboradorPage));
		Routing.RegisterRoute(nameof(Views.AlunoPage), typeof(Views.AlunoPage));
		Routing.RegisterRoute("matricula", typeof(Views.MatriculaPage));
		Routing.RegisterRoute("minha-conta", typeof(Views.MinhaContaPage));
		Appearing += async (_, _) => await AtualizarFotoContaAsync();
	}

	private async Task AtualizarFotoContaAsync()
	{
		var perfil = Enum.TryParse(Preferences.Get("PerfilSessao", "Aluno"), out AppPerfil valor) ? valor : AppPerfil.Aluno;
		var nomeSessao = Preferences.Get("NomeSessao", string.Empty);
		PessoaDto? pessoa;
		if (perfil == AppPerfil.Aluno)
		{
			var alunos = MauiProgram.Services.GetRequiredService<IAlunoService>();
			pessoa = (await alunos.ObterPorNomeAsync(nomeSessao)).FirstOrDefault() ?? (await alunos.ObterTodosAsync()).FirstOrDefault();
		}
		else
		{
			var colaboradores = MauiProgram.Services.GetRequiredService<IColaboradorService>();
			pessoa = (await colaboradores.ObterTodosAsync()).FirstOrDefault(item => item.Nome.Equals(nomeSessao, StringComparison.OrdinalIgnoreCase)) ?? (await colaboradores.ObterTodosAsync()).FirstOrDefault();
		}

		var foto = new FotoToImageSourceConverter().Convert(pessoa?.Foto, typeof(ImageSource), null, CultureInfo.CurrentCulture) as ImageSource;
		if (foto != null) FotoConta = foto;
	}

	private void AplicarPermissoes()
	{
		var perfil = Preferences.Get("PerfilSessao", "Aluno");
		var administrador = perfil == "Administrador";
		var colaborador = perfil == "Colaborador";
		LogradourosItem.IsVisible = administrador;
		ColaboradoresItem.IsVisible = administrador;
		AlunosItem.IsVisible = administrador || colaborador;
		MatriculasItem.IsVisible = administrador || colaborador;
		AcessosItem.IsVisible = administrador || colaborador;
		TreinosItem.IsVisible = true;
		ConfiguracoesItem.IsVisible = administrador;
	}

	private void OnSairClicked(object? sender, EventArgs eventArgs)
	{
		Preferences.Remove("NomeSessao");
		Preferences.Remove("PerfilSessao");
		Microsoft.Maui.Controls.Application.Current?.Windows[0].Page = new Views.LoginPage();
	}

	private async void OnNavigating(object? sender, ShellNavigatingEventArgs eventArgs)
	{
		var destino = eventArgs.Target.Location.OriginalString.ToLowerInvariant();
		var perfil = Preferences.Get("PerfilSessao", "Aluno");
		var administrador = perfil == "Administrador";
		var colaborador = perfil == "Colaborador";
		var permitido = destino.Contains("dashboard") || destino.Contains("treinos") || destino.Contains("minha-conta") ||
			(administrador) ||
			(colaborador && (destino.Contains("alunos") || destino.Contains("matriculas") || destino.Contains("acessos")));
		if (permitido) return;

		eventArgs.Cancel();
		await DisplayAlertAsync("Acesso restrito", "Seu perfil nao possui permissao para esta tela.", "Ok");
	}
}
