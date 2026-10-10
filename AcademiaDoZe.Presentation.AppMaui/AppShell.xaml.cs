// henrique agostinetto piva
namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Views.LogradouroPage), typeof(Views.LogradouroPage));
		Routing.RegisterRoute(nameof(Views.ColaboradorPage), typeof(Views.ColaboradorPage));
		Routing.RegisterRoute(nameof(Views.AlunoPage), typeof(Views.AlunoPage));
	}
}
