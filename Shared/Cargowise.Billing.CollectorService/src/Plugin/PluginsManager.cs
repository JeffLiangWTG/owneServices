using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public class PluginsManager : IPluginsManager
	{
		public event EventHandler PluginsLoadFailed = delegate { };
		public event EventHandler PluginsLoadSettingsFileFailed = delegate { };
		public event EventHandler PluginsLoadStateFileFailed = delegate { };

		public PluginsManager(
			Assembly pluginsAssembly,
			string settingsFilePath,
			string stateFilePath,
			Dictionary<string, string> globalSettings,
			ILoggerFactory loggerFactory,
			Func<IBillingServiceClient> serviceClientFactory,
			Func<IBillingKafkaClient> kafkaClientFactory,
			IErrorReportingClient? errorReportingClient,
			Type[]? additionalSchedulerTypes = null,
			Dictionary<string, string[]?>? knownExceptions = null)
		{
			this.pluginsAssembly = pluginsAssembly ?? throw new ArgumentNullException("pluginsAssembly");
			this.settingsFilePath = settingsFilePath ?? throw new ArgumentNullException("settingsFilePath");
			this.stateFilePath = stateFilePath ?? throw new ArgumentNullException("stateFilePath");
			this.globalSettings = globalSettings ?? throw new ArgumentNullException("globalSettings");
			this.loggerFactory = loggerFactory;
			this.kafkaClientFactory = kafkaClientFactory;
			this.errorReportingClient = errorReportingClient;
			this.knownExceptions = knownExceptions;
			this.serviceClientFactory = serviceClientFactory;
			this.additionalSchedulerTypes = additionalSchedulerTypes;

			serviceState = new ServiceState();
			fileWatcherSettings = CreateWatcher(settingsFilePath, OnSettingsFileChanged);
			fileWatcherState = CreateWatcher(stateFilePath, OnStateFileChanged);
		}

		FileSystemWatcher CreateWatcher(string filePath, FileSystemEventHandler handler)
		{
			var directoryName = Path.GetDirectoryName(filePath);
			if (directoryName == null)
			{
				throw new ArgumentException("Could not find a folder containing file", filePath);
			}
			var fileWatcher = new FileSystemWatcher(directoryName, Path.GetFileName(filePath));
			fileWatcher.Changed += handler;
			return fileWatcher;
		}

		public virtual void Start()
		{
			try
			{
				serviceState = ServiceState.ReadFromFile(stateFilePath);
			}
			catch (FormatException ex)
			{
				errorReportingClient?.ReportToIssueManager("No plugins found in the state file.", ex, Logger);
				serviceState = new ServiceState();
			}

			ServiceSettings serviceSettings;
			try
			{
				serviceSettings = ServiceSettings.ReadFromFile(settingsFilePath);
			}
			catch (FormatException ex)
			{
				errorReportingClient?.ReportToIssueManager("No plugins found in the settings file.", ex, Logger);
				serviceSettings = new ServiceSettings { Plugins = new PluginControllerSettings[0] };
			}

			foreach (PluginControllerSettings settings in serviceSettings.Plugins)
			{
				// find matching state
				PluginControllerState? matchingState = null;
				foreach (PluginControllerState state in serviceState.Plugins)
				{
					if (state.Key == settings.Key)
					{
						matchingState = state;
					}
				}

				// create a new state if it wasn't in the file
				if (matchingState == null)
				{
					matchingState = new PluginControllerState(CreateClock());
					matchingState.Key = settings.Key;
					serviceState.Plugins.Add(matchingState);
				}
			}

			LoadPlugins(serviceSettings.Plugins);

			fileWatcherSettings.EnableRaisingEvents = true;
			fileWatcherState.EnableRaisingEvents = true;
		}

		public virtual void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				UnloadPlugins();
				fileWatcherSettings.Dispose();
				fileWatcherState.Dispose();
				errorReportingClient?.Dispose();
			}
		}

		IEnumerable<string> CurrentPluginsKeys
		{
			get { return pluginControllers.Keys.ToArray(); }
		}

		void LoadPlugins(PluginControllerSettings[] serviceSettings)
		{
			Logger.LogInformation("Loading plugins.");

			// Disable any plugins that have been removed from the settings file.
			foreach (var pluginKey in CurrentPluginsKeys.Where(key => serviceSettings.All(p => p.Key != key)))
			{
				DisablePlugin(pluginKey);
			}
			foreach (PluginControllerSettings pluginSettings in serviceSettings)
			{
				PluginController pluginController;
				// Update any plugins that continue to exist in the settings file.
				if (pluginControllers.TryGetValue(pluginSettings.Key, out pluginController))
				{
					pluginController.UpdateSettings(pluginSettings);
				}
				// Create any plugins that have been added to the settings file.
				else
				{
					foreach (var pluginState in serviceState.Plugins)
					{
						if (pluginState.Key == pluginSettings.Key)
						{
							pluginController = new PluginController(pluginsAssembly, pluginSettings, pluginState, globalSettings, serviceClientFactory(), kafkaClientFactory(), CreateClock(), stateLock, loggerFactory, errorReportingClient, knownExceptions, additionalSchedulerTypes);
							pluginController.LastSuccessfulRunChanged += OnPluginLastSuccessfulRunChanged;
							pluginController.Start();
							pluginControllers.Add(pluginSettings.Key, pluginController);
							break;
						}
					}
				}
			}

			Logger.LogInformation($"Loading plugins finished. {serviceSettings.Length} plugins running.");
		}

		void UnloadPlugins()
		{
			foreach (var key in CurrentPluginsKeys)
			{
				DisablePlugin(key);
			}
		}

		void DisablePlugin(string key)
		{
			PluginController pluginController;
			if (pluginControllers.TryGetValue(key, out pluginController))
			{
				pluginController.Dispose();
				pluginController.LastSuccessfulRunChanged -= OnPluginLastSuccessfulRunChanged;
				pluginControllers.Remove(key);
			}
		}

		void OnPluginLastSuccessfulRunChanged(object sender, EventArgs eventArgs)
		{
			for (int tries = 0; tries <= MaxRetries; tries++)
			{
				fileWatcherState.EnableRaisingEvents = false;

				try
				{
					serviceState.WriteToFile(stateFilePath);
				}
				catch (IOException ex)
				{
					if (tries < MaxRetries)
					{
						Logger.LogError(ex, "Failed to write to state file. Retrying...");
						Thread.Sleep(DelayOnRetry);
					}
					else
					{
						errorReportingClient?.ReportToIssueManager($"Failed to write to state file after retrying for {MaxRetries} times", ex, Logger);
					}
				}
				catch (Exception ex)
				{
					errorReportingClient?.ReportToIssueManager("Failed to write to state file", ex, Logger);
					break;
				}
				finally
				{
					fileWatcherState.EnableRaisingEvents = true;
				}
			}
		}

		void OnSettingsFileChanged(object sender, EventArgs eventArgs)
		{
			int tries = 0;
			ServiceSettings? serviceSettings = null;
			while (serviceSettings == null)
			{
				try
				{
					serviceSettings = ServiceSettings.ReadFromFile(settingsFilePath);
				}
				catch (FormatException exception)
				{
					errorReportingClient?.ReportToIssueManager("No plugins found in the settings file.", exception, Logger);
					return;
				}
				catch (Exception e)
				{
					if (tries < 3)
					{
						Logger.LogError(e, "Failed to load settings xml. Retrying...");
						tries++;
						Thread.Sleep(500);
					}
					else
					{
						errorReportingClient?.ReportToIssueManager("Failed to load settings xml after retrying for 3 times", e, Logger);
						PluginsLoadSettingsFileFailed(this, EventArgs.Empty);
						return;
					}
				}
			}

			try
			{
				LoadPlugins(serviceSettings.Plugins);
			}
			catch (Exception e)
			{
				errorReportingClient?.ReportToIssueManager("Failed to load plugins", e, Logger);
				PluginsLoadFailed(this, EventArgs.Empty);
			}
		}

		void OnStateFileChanged(object sender, EventArgs eventArgs)
		{
			var tries = 0;
			var continueToRetry = true;
			while (continueToRetry)
			{
				try
				{
					serviceState.Reload(stateFilePath, stateLock);

					foreach (var pluginController in pluginControllers.Values)
					{
						pluginController.ScheduleNextRun();
					}
					continueToRetry = false;
				}
				catch (FormatException exception)
				{
					continueToRetry = false;
					serviceState = new ServiceState();
					errorReportingClient?.ReportToIssueManager("No plugins found in the state file.", exception, Logger);
				}
				catch (Exception exception)
				{
					if (tries < 3)
					{
						Logger.LogError(exception, "Failed to load state xml. Retrying...");
						tries++;
						Thread.Sleep(500);
					}
					else
					{
						continueToRetry = false;
						errorReportingClient?.ReportToIssueManager("Failed to load state xml after retrying for 3 times", exception, Logger);
						PluginsLoadStateFileFailed(this, EventArgs.Empty);
					}
				}
			}
		}

		protected internal virtual IClock CreateClock()
		{
			return new Clock(PluginsRunDelay);
		}

		static readonly TimeSpan PluginsRunDelay = TimeSpan.FromSeconds(59);
		bool disposed;
		public ServiceState serviceState;
		readonly Assembly pluginsAssembly;
		readonly string settingsFilePath;
		readonly string stateFilePath;
		readonly FileSystemWatcher fileWatcherSettings;
		readonly FileSystemWatcher fileWatcherState;
		readonly Dictionary<string, PluginController> pluginControllers = new ();
		readonly ILoggerFactory loggerFactory;
		readonly Func<IBillingServiceClient> serviceClientFactory;
		readonly Func<IBillingKafkaClient> kafkaClientFactory;
		readonly Dictionary<string, string> globalSettings;
		readonly IErrorReportingClient? errorReportingClient;
		readonly Type[]? additionalSchedulerTypes;
		readonly Dictionary<string, string[]?>? knownExceptions;

		public ILogger<PluginsManager> Logger
		{
			get { return logger ??= CreateLogger(); }
		}
		ILogger<PluginsManager>? logger;

		public virtual ILogger<PluginsManager> CreateLogger() => loggerFactory.CreateLogger<PluginsManager>();

		private const int MaxRetries = 3;
		private const int DelayOnRetry = 500;

		// To avoid race conditions this lock must be taken while the serviceState is read, written to, read from disk or written to disk.
		//
		// However there remains a case impossible to handle with locks.
		// The user could open the state file, then billing writes to the state file, then the user saves the state file.
		// We are relying on notepad++ to alert the user about the changes to the file.
		readonly object stateLock = new ();
	}
}
