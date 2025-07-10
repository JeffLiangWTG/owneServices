using System;
using System.Collections.Generic;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.Telemetry;
using Enterprise.Startup;
using Enterprise.Winzor.Architecture;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinzorFramework;

namespace CargoWise.Blazor.HostedAppRuntime
{
	/// <summary>
	/// Exposes lifetime management for the CargoWiseRuntime (ensures *global state* is initialised once without failing)
	/// This service should be consumed as a singleton via DI
	/// </summary>
	public class CargoWiseRuntime
	{
		bool isSuccessfullyInitialised;
		bool failedInitialisation;

		/// <summary>
		/// Exception captured if Initialise failed with an unhandled exception (other than multi-init guard)
		/// <see cref="InitialiserStatus.Failed"/>
		/// </summary>
		Exception statusFailedException;

		static readonly object initialiseSequenceLock = new();
		readonly ILogger<CargoWiseRuntime> logger;
		readonly CargoWiseOptions cargoWiseOptions;
		readonly IWinzorCargoWiseLoginHandler loginHandler;
		readonly WinzorDispatcher winzorDispatcher;
		readonly UserMonitorRegistry userMonitorRegistry;
		readonly IWinzorTelemetry telemetry;

		public CargoWiseRuntime(
			ILogger<CargoWiseRuntime> logger,
			IOptions<CargoWiseOptions> cargoWiseOptions,
			IWinzorCargoWiseLoginHandler loginHandler,
			WinzorDispatcher winzorDispatcher,
			UserMonitorRegistry userMonitorRegistry,
			IWinzorTelemetry telemetry)
		{
			this.logger = logger;
			this.cargoWiseOptions = cargoWiseOptions.Value;
			this.loginHandler = loginHandler;
			this.winzorDispatcher = winzorDispatcher;
			this.userMonitorRegistry = userMonitorRegistry;
			this.telemetry = telemetry;
		}

		/// <summary>
		/// Bootstraps CargoWise 'runtime' - static services required to run CW in the current process
		/// </summary>
		public void Initialise()
		{
			logger.LogInformation($"START: {nameof(Initialise)}");
			lock (initialiseSequenceLock)
			{
				logger.LogInformation($"Entered initialisation lock");
				if (isSuccessfullyInitialised)
				{
					// if runtime is already initialised then we should not re-initialise it on this path
					// in practice this should only happen when spinning up a test host however preventing or making safe
					// multiple "startups" of the CargoWise runtime in a single test assembly process is outstanding
					logger.LogInformation($"CargoWise runtime already initialised");
					return;
				}

				if (failedInitialisation)
				{
					throw new InvalidOperationException(
						message: "CargoWise runtime status 'initialisation failed' due to a previous Initialise call. " +
									"The runtime may be incorrectly configured and should not be used. See InnerException for the initial exception.",
						innerException: statusFailedException);
				}

				try
				{
					Environment.SetEnvironmentVariable("WTG_CW_BLAZOR", (NoResString)"true");

					logger.LogDebug((NoResString)"Initialize 'CargoWise' ");
					var arguments = new List<string> { cargoWiseOptions.DbServerName, cargoWiseOptions.DatabaseName };
					if (!string.IsNullOrEmpty(cargoWiseOptions.ClientName))
					{
						var clientArgument = ApplicationArguments.OptionClient + cargoWiseOptions.ClientName;
						arguments.Add(clientArgument);
					}
					if (!string.IsNullOrEmpty(cargoWiseOptions.Persist))
					{
						arguments.Add($"{ApplicationArguments.OptionPersist}{cargoWiseOptions.Persist}");
					}

					var applicationArguments = new ApplicationArguments(arguments.ToArray());
					CommandLineArguments.UsedToLaunchApplication = applicationArguments;
					Initialization.StartConfigureCargoWise(winzorDispatcher, applicationArguments, loginHandler, userMonitorRegistry, cargoWiseOptions.AppServerProcessCorrelationId, telemetry.LoadingActivity, logger);
					isSuccessfullyInitialised = true;
				}
				catch (Exception ex)
				{
					statusFailedException = ex;
					failedInitialisation = true;
					throw;
				}
			}
		}
	}
}
