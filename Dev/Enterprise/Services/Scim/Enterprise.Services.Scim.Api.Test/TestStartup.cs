using System;
using System.Linq;
using Enterprise.Services.Scim.Business;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Services.Scim.Api.Test
{
	public class TestStartup : Startup
	{
		public TestStartup() : base()
		{
			var services = GetServices();

			var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IIPSafelistHelper));
			if (descriptor != null)
			{
				services.Remove(descriptor);
			}

			services.AddSingleton<IIPSafelistHelper, TestIPSafelistHelper>();
		}

		protected override IServiceProvider CreateServiceProvider()
		{
			return Services.BuildServiceProvider();
		}
	}
}
