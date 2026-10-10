// henrique agostinetto piva
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
	public static IServiceProvider Services { get; private set; } = null!;

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		var config = ConfigurationHelper.ObterConfiguracaoAtual();
		if (config.DatabaseType == AppDatabaseType.Sqlite)
			DbInitializer.InitializeAsync(config.ConnectionString, config.DatabaseType.ToDatabaseType()).GetAwaiter().GetResult();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
			});

		ConfigurationHelper.ConfigureServices(builder.Services);
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<LogradouroListPage>();
		builder.Services.AddTransient<LogradouroListViewModel>();
		builder.Services.AddTransient<LogradouroPage>();
		builder.Services.AddTransient<LogradouroFormViewModel>();
		builder.Services.AddTransient<ColaboradorListPage>();
		builder.Services.AddTransient<ColaboradorListViewModel>();
		builder.Services.AddTransient<ColaboradorPage>();
		builder.Services.AddTransient<ColaboradorViewModel>();
		builder.Services.AddTransient<AlunoListPage>();
		builder.Services.AddTransient<AlunoListViewModel>();
		builder.Services.AddTransient<AlunoPage>();
		builder.Services.AddTransient<AlunoViewModel>();
		builder.Services.AddTransient<ConfigPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		Services = app.Services;
		return app;
	}
}
