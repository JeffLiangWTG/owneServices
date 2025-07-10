using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.ConfigurationManager;
using CargoWiseNext.Infrastructure.Instance.Management;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace CargoWise.Blazor.SessionBroker
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			AssemblyResolver.Setup();

			await StartServerAsync(args);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static async Task StartServerAsync(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
							.Enrich.FromLogContext()
							.WriteTo.Console()
							.CreateLogger();

			Log.Information("Starting up");

			// this adapts the environment variable provided by DAT to what is expected by the ASP.NET Core config system
			// the end result is that on DAT, ASP.NET Core should automatically load appsettings.DAT.json to override appsettings.json
			if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
				Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DAT", EnvironmentVariableTarget.Process);
			}

			try
			{
				// Register the current process to a job
				// When the processes exits child process will also exit
				using var job = new Job();
				job.AddProcess(Process.GetCurrentProcess());

				using var host = CreateHostBuilder(args).Build();
				host.Start();

				await host.Services.GetRequiredService<VersionBrokerRegistration>().RegisterAsync();
				host.WaitForShutdown();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unhandled exception");
				return;
			}
			finally
			{
				Log.Information("Shut down complete");
				await Log.CloseAndFlushAsync();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.AddScopedEnvironmentVariablesInDevelopment()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddConfigurationFromAppSettings();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.UseSerilog(SetupSerilog,
					preserveStaticLogger: true);

		static void SetupSerilog(HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration)
		{
			var cwOptions = serviceProvider.GetRequiredService<IOptions<CargoWiseOptions>>();
			var fileVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();

			configuration
				.ReadFrom.Configuration(context.Configuration)
				.AddEnrichers(cwOptions.Value.VersionBrokerProcessCorrelationId,
					cwOptions.Value.SessionBrokerProcessCorrelationId,
					fileVersionAttribute?.Version,
					cwOptions.Value.Hostname,
					context.HostingEnvironment.IsDevelopmentOrIntegrationTestOrDAT());
		}
	}
}
