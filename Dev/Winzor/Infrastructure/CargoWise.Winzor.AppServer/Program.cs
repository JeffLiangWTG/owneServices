using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.HostedAppRuntime;
using CargoWise.Winzor.AppServer.Helpers;
using CargoWiseNext.Infrastructure.ConfigurationManager;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using WinzorFramework;

namespace CargoWise.Winzor.AppServer
{
	public sealed class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static void Main(string[] args)
		{
			if ("true".Equals(Environment.GetEnvironmentVariable("CARGOWISE_APPSERVER_LAUNCHDEBUGGERONSTART"), StringComparison.OrdinalIgnoreCase))
			{
				Debugger.Launch();
			}
			AssemblyResolver.Setup();
			MainImpl(args);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void MainImpl(string[] args)
		{
			// this adapts the environment variable provided by DAT to what is expected by the ASP.NET Core config system
			// the end result is that on DAT, ASP.NET Core should automatically load appsettings.DAT.json to override appsettings.json
			if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
				Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DAT", EnvironmentVariableTarget.Process);
			}

			// The WinzorDispatcher must be disposed after the Host stops
			// See: WI00618394 - Cannot access a disposed object.Object name: 'System.Collections.Concurrent.Bloc
			// We instantiate this service in <see cref="Startup.cs/>
			// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0#services-not-created-by-the-service-container
			using (var host = CreateHostBuilder(args).Build())
			{
				var winzorDispatcher = host.Services.GetService<WinzorDispatcher>();
				var logger = host.Services.GetRequiredService<ILogger<Program>>();
				try
				{
					// initialise cargowise statics here because it's after the host is built (configuration + DI setup)
					// and before the host is started (we want the runtime to be ready before IHostedServices begin to start up)
					// an alternative would be to internally call initialise when the runtime is first used, but we don't really have anywhere to hook that at the moment
					logger.LogDebug((NoResString)"Initialising runtime...");

					//Construct the tracer before initialization
					host.Services.GetService<TracerProvider>();
					host.Services.GetService<MeterProvider>();
					host.InitialiseCargoWiseRuntime();

					logger.LogDebug((NoResString)"Starting host...");
					host.Start();
					logger.LogDebug((NoResString)"Host started, running post-start setup...");
					var notifier = host.Services.GetRequiredService<IProcessStartNotifier>();
					// The CircuitHandler implementation is responsible for the "disconnected" idle shutdown.  It's a singleton, and ASP.NET/Blazor
					// doesn't instantiate it until the first circuit is connected.  The "user idle" timer is controlled from the client side as there is no
					// method of detecting it from the server side (that I know of).  Therefore, if no circuit ever connects, the CargoWise.Winzor.AppServer instance will live forever.
					// To work arond this, we need to manually instantiate the CircuitHandler which will initiate the "disconnected" timer.
					_ = host.Services.GetRequiredService<IShutDownCircuitHandler>();
					notifier.ProcessStarted();
					logger.LogDebug((NoResString)"Host post-start setup complete");
					logger.LogInformation((NoResString)"AppServer is currently using : {GCSettings}", System.Runtime.GCSettings.IsServerGC ? (NoResString)"Server GC" : (NoResString)"Workstation GC");
					host.WaitForShutdown();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, (NoResString)"Host terminated unexpectedly");
				}
				finally
				{
					winzorDispatcher?.Dispose();
					logger.LogInformation((NoResString)"Shut down complete");
					Log.CloseAndFlush();
				}
			}

			Environment.Exit(0); // Force any foreground threads that may still be running to terminate
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string ApplicationName = "Winzor";

		/// <summary>
		/// Configures the host
		/// </summary>
		/// <param name="args"></param>
		/// <param name="winzorDispatcher"></param>
		/// <returns></returns>
		public static IHostBuilder CreateHostBuilder(string[] args, WinzorDispatcher winzorDispatcher = null) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices(config =>
				{
					if (winzorDispatcher is not null)
					{
						config.AddSingleton(winzorDispatcher);
						config.AddSingleton(winzorDispatcher.FormOpener);
						config.AddSingleton(winzorDispatcher.FormInstanceRegister);
					}
				})
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddConfigurationFromAppSettings();
				})
				.AddScopedEnvironmentVariablesInDevelopment()
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
				.ConfigureLogging(SetupLogger)
				.UseStandardInputConfig()
				.UseSerilog(SetupSerilog, writeToProviders: true);

		static void SetupLogger(HostBuilderContext context, ILoggingBuilder logging)
		{
			if (!context.HostingEnvironment.IsDevelopmentOrIntegrationTestOrDAT())  // if we want to use the logger in DAT, we can use IsDevelopmentOrIntegrationTest()
			{
				logging.AddWiseTechErrorReporting(settings => settings.ApplicationName = ApplicationName);
			}
		}

		static void SetupSerilog(HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration)
		{
			var cwOptions = serviceProvider.GetRequiredService<IOptions<CargoWiseOptions>>();
			var fileVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();

			configuration
				.ReadFrom.Configuration(context.Configuration)
				.AddEnrichers(cwOptions.Value.VersionBrokerProcessCorrelationId,
					cwOptions.Value.SessionBrokerProcessCorrelationId,
					cwOptions.Value.AppServerProcessCorrelationId,
					fileVersionAttribute?.Version,
					cwOptions.Value.Hostname,
					context.HostingEnvironment.IsDevelopmentOrIntegrationTestOrDAT())
				.Filter.ByExcluding(logEvent =>
				{
					var ignoreNoBrowserRendererError = logEvent.Exception is AggregateException aggregateException &&
						aggregateException.InnerExceptions.Any(inner => inner.Message.Contains((NoResString)"There is no browser renderer with ID 1."));
					return ignoreNoBrowserRendererError;
				});
		}
	}
}
