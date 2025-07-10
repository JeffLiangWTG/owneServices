using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CargoWise.Winzor.AppServer.Test
{
	class AppServerWebApplicationFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("IntegrationTest");
			builder.UseContentRoot(@"..\AppServer");
		}

		protected override IHostBuilder CreateHostBuilder()
		{
			// we don't want tests writing to disk or Kafka so this effectively suppresses logging
			return Program.CreateHostBuilder(Array.Empty<string>())
				.UseSerilog((_, _) => { });
		}
	}
}
