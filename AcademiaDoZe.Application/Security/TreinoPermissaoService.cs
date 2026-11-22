// henrique agostinetto piva
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.Security;

public static class TreinoPermissaoService
{
    public static bool PodeVisualizar(AppPerfil perfil, string loginAtual, string alunoLogin, string personalLogin) => perfil switch
    {
        AppPerfil.Administrador => true,
        AppPerfil.Colaborador => string.Equals(loginAtual, personalLogin, StringComparison.OrdinalIgnoreCase),
        _ => string.Equals(loginAtual, alunoLogin, StringComparison.OrdinalIgnoreCase)
    };
}
