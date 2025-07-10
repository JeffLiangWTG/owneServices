using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewService.Test
{
	public static class WebApplicationFactoryHelper
	{
		public static WebApplicationFactory<Program> WebAppFactory
		{
			get
			{
				using (var factory = new WebApplicationFactory<Program>())
				{
					return factory.WithWebHostBuilder(builder =>
					{
						builder.UseContentRoot(Directory.GetCurrentDirectory());
						builder.ConfigureTestServices(services =>
						{
							var partManager = GetApplicationPartManager(services);
							partManager.FeatureProviders.Add(new ExternalControllersFeatureProvider(typeof(DummyDataSetController)));
						});
					});
				}
			}
		}

		static ApplicationPartManager GetApplicationPartManager(IServiceCollection services)
		{
			var partManager = (ApplicationPartManager)services
				.Last(descriptor => descriptor.ServiceType == typeof(ApplicationPartManager))
				.ImplementationInstance;
			return partManager;
		}
	}
}
