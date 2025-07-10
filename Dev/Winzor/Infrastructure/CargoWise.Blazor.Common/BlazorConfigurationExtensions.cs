using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CargoWise.Blazor.Common
{
	public static class BlazorConfigurationExtensions
	{
		/// <summary>
		/// Augments Microsoft.Extensions.Configuration.IConfiguration as similar as possible to what is created for CargoWise.Winzor.AppServer.exe
		/// </summary>
		public static IConfigurationBuilder AddBlazorCommon(this IConfigurationBuilder builder)
		{
			builder.AddJsonFile("appsettings.json", optional: true);

			if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
				//optional - DAT config overrides only apply if defined and execution environment is DAT
				builder.AddJsonFile("appsettings.DAT.json", optional: true);
			}

			return builder
				.AddEnvironmentVariables()
				// Some developers, particularly when working from home, prefer to have the database on a different
				// machine to their IDE. Those developers can `set CARGOWISEDEV_CARGOWISEOPTIONS:SERVERNAME sydco-myworkstation-99`
				.AddEnvironmentVariables("CARGOWISEDEV_");
		}

		/// <summary>
		/// Consumes environment variables with the prefix CARGOWISEDEV_
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IHostBuilder AddScopedEnvironmentVariablesInDevelopment(this IHostBuilder builder)
		{
			return builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
			{
				if (hostingContext.HostingEnvironment.IsDevelopmentOrIntegrationTest())
				{
					configBuilder.AddEnvironmentVariables("CARGOWISEDEV_");
				}
			});
		}

		/// <summary>
		/// Reads configuration variables from standard input until the stream is closed
		/// Variables must be in standard ASPNET.Core configuration JSON format
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IHostBuilder UseStandardInputConfig(this IHostBuilder builder)
		{
			return builder.ConfigureAppConfiguration((_, configBuilder) =>
			{
				var cargoWiseOptions = new CargoWiseOptions();
				var config = configBuilder.Build();
				config.Bind(nameof(CargoWiseOptions), cargoWiseOptions);
				if (cargoWiseOptions.ReadConfigFromStdIn)
				{
					configBuilder.AddJsonStream(Console.OpenStandardInput());
				}
			});
		}

		/// <summary>
		/// Setup for blazor console hosting scenarios
		/// </summary>
		/// <param name="args">Command line arguments</param>
		public static IConfigurationBuilder AddBlazorCommandLine(this IConfigurationBuilder builder, string[] args)
		{
			return builder
				.AddBlazorCommon()
				.AddCommandLine(args);
		}
	}
}
