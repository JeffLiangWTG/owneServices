using CargoWise.GUI.TileBar.JSInterop;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.GUI.TileBar.JsInterop;
public static class Services
{
	public static void AddJSInteropServices(this IServiceCollection services)
	{
		services.AddScoped<ITileBarJSInterop, TileBarJSInterop>();
	}
}
