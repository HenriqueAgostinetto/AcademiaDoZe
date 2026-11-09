// henrique agostinetto piva
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

public enum AppColaboradorTipo { [Display(Name = "Administrador")] Administrador, [Display(Name = "Atendente")] Atendente, [Display(Name = "Instrutor")] Instrutor }
public enum AppColaboradorVinculo { [Display(Name = "CLT")] CLT, [Display(Name = "Estagiario")] Estagio }
public enum AppMatriculaPlano { [Display(Name = "Mensal")] Mensal, [Display(Name = "Trimestral")] Trimestral, [Display(Name = "Semestral")] Semestral, [Display(Name = "Anual")] Anual }
[Flags] public enum AppMatriculaRestricoes { [Display(Name = "Nenhuma restricao")] None = 0, [Display(Name = "Diabetes")] Diabetes = 1, [Display(Name = "Pressao alta")] PressaoAlta = 2, [Display(Name = "Labirintite")] Labirintite = 4, [Display(Name = "Alergias")] Alergias = 8, [Display(Name = "Problemas respiratorios")] ProblemasRespiratorios = 16, [Display(Name = "Uso de remedio continuo")] RemedioContinuo = 32 }
