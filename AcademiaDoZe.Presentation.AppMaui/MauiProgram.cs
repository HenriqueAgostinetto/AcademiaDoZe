// henrique agostinetto piva
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Infrastructure.Data;
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
		var config = new RepositoryConfig
		{
			ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "academia_do_ze.db")}",
			DatabaseType = DatabaseType.Sqlite
		};
		DbInitializer.InitializeAsync(config.ConnectionString, config.DatabaseType).GetAwaiter().GetResult();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(config);
		builder.Services.AddApplicationServices();
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<LogradouroListPage>();
		builder.Services.AddTransient<LogradouroListViewModel>();
		builder.Services.AddTransient<LogradouroPage>();
		builder.Services.AddTransient<LogradouroFormViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		Services = app.Services;
		return app;
	}
}
