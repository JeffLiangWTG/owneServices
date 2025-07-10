
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.NetworkVisualisation.GUI.JSInterop;

public static class Services
{
	/// <summary>
	/// Adds JSInterop Services for pages to the specified <see cref="IServiceCollection"/>.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
	public static void AddJSInteropServices(this IServiceCollection services)
	{
		services.AddScoped<IGlobalEventJSInterop, GlobalEventJSInterop>();
	}
}
