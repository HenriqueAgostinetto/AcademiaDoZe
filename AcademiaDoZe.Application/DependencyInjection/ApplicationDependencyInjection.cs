// henrique agostinetto piva
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ILogradouroService, LogradouroService>();
        services.AddTransient<IAlunoService, AlunoService>();
        services.AddTransient<IColaboradorService, ColaboradorService>();
        services.AddTransient<IMatriculaService, MatriculaService>();
        services.AddTransient(provider => (Func<ILogradouroRepository>)(() => { var config = provider.GetRequiredService<RepositoryConfig>(); return new LogradouroRepository(config.ConnectionString, config.DatabaseType); }));
        services.AddTransient(provider => (Func<IAlunoRepository>)(() => { var config = provider.GetRequiredService<RepositoryConfig>(); return new AlunoRepository(config.ConnectionString, config.DatabaseType); }));
        services.AddTransient(provider => (Func<IColaboradorRepository>)(() => { var config = provider.GetRequiredService<RepositoryConfig>(); return new ColaboradorRepository(config.ConnectionString, config.DatabaseType); }));
        services.AddTransient(provider => (Func<IMatriculaRepository>)(() => { var config = provider.GetRequiredService<RepositoryConfig>(); return new MatriculaRepository(config.ConnectionString, config.DatabaseType); }));
        return services;
    }
}
