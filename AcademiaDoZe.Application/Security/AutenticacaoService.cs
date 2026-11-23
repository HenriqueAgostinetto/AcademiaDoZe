// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.Security;

public static class AutenticacaoService
{
    public static SessaoDto? Autenticar(string? login, string? senha)
    {
        if (senha != "123456") return null;
        return Normalizar(login) switch
        {
            "henrique" => new SessaoDto { Nome = "Henrique", Perfil = AppPerfil.Administrador },
            "henrique 1" or "henrique1" => new SessaoDto { Nome = "Henrique Agostinetto Piva", Perfil = AppPerfil.Colaborador },
            "henrique 2" or "henrique2" => new SessaoDto { Nome = "Henrique Agostinetto Piva", Perfil = AppPerfil.Aluno },
            _ => null
        };
    }

    private static string Normalizar(string? login) => string.Join(' ', (login ?? string.Empty).Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
