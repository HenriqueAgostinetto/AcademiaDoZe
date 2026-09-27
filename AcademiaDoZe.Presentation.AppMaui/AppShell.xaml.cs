// henrique agostinetto piva
namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Views.LogradouroPage), typeof(Views.LogradouroPage));
	}
}
