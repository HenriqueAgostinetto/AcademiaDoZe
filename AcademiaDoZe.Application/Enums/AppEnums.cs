// henrique agostinetto piva
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

public enum AppColaboradorTipo { [Display(Name = "Administrador")] Administrador, [Display(Name = "Atendente")] Atendente, [Display(Name = "Instrutor")] Instrutor }
public enum AppColaboradorVinculo { [Display(Name = "CLT")] CLT, [Display(Name = "Estagiario")] Estagio }
public enum AppMatriculaPlano { [Display(Name = "Mensal")] Mensal, [Display(Name = "Trimestral")] Trimestral, [Display(Name = "Semestral")] Semestral, [Display(Name = "Anual")] Anual }
[Flags] public enum AppMatriculaRestricoes { [Display(Name = "Nenhuma restricao")] None, Diabetes = 1, PressaoAlta = 2, Labirintite = 4, Alergias = 8, ProblemasRespiratorios = 16, RemedioContinuo = 32 }
