using System;
using CargoWise.Blazor.HostedAppRuntime.Backchannel;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CargoWise.Blazor.HostedAppRuntime
{
	public static class HostedAppRuntimeConfigurationExtensions
	{
		/// <summary>
		/// Registers Blazor wrappers for CargoWise services (and directly registers some CW services provided via DI)
		/// NOTE: See ObjectFactory xml configuration resources for registration of Blazor services into CargoWise runtime
		/// </summary>
		public static IServiceCollection AddCargoWiseRuntime(this IServiceCollection services)
		{
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);
			services.AddSingleton<CargoWiseRuntime>();
			services.AddSingleton<IHubConnector, HubConnector>();
			services.AddSingleton<IBackchannelProvider, BackchannelProvider>();
			services.AddTransient((_) => new Lazy<BusinessObjectFactory>());
			return services;
		}

		/// <summary>
		/// Exposes initialisation of the CargoWiseRuntime for invocation prior to host startup (before IHostedServices might require it)
		/// </summary>
		public static void InitialiseCargoWiseRuntime(this IHost host)
		{
			host.Services.GetRequiredService<CargoWiseRuntime>().Initialise();
		}
	}
}
