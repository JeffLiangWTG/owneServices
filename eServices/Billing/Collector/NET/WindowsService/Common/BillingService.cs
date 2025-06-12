using System.Reflection;
using CargoWise.Billing.Client;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.Kafka.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common
{
	public class BillingService : Microsoft.Extensions.Hosting.BackgroundService
	{

		public BillingService(IServiceProvider serviceProvider, Assembly pluginsAssembly, string settingsFileName, string stateFileName)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException("serviceProvider");
			this.pluginsAssembly = pluginsAssembly ?? throw new ArgumentNullException("pluginsAssembly");
			this.settingsFileName = settingsFileName ?? throw new ArgumentNullException("settingsFileName");
			this.stateFileName = stateFileName ?? throw new ArgumentNullException("stateFileName");
			configuration = serviceProvider.GetService<IConfiguration>() ?? throw new ArgumentNullException("configuration");
			errorReportingClient = serviceProvider.GetService<IErrorReportingClient>() ?? throw new ArgumentNullException("errorReportingClient");
			loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? throw new ArgumentNullException("loggerFactory");
			globalSettings = configuration.AsEnumerable().Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
		}

		internal virtual IPluginsManager CreatePluginsManager(string settingsFilePath, string stateFilePath)
		{
			return new PluginsManager(pluginsAssembly, settingsFilePath, stateFilePath, globalSettings, loggerFactory, CreateBillingServiceClient, CreateBillingKafkaClient, errorReportingClient, new []{ typeof(PluginCronScheduler) });
		}

		IPluginsManager pluginsManager;
		readonly Assembly pluginsAssembly;
		readonly string settingsFileName;
		readonly string stateFileName;

		public ILogger<BillingService> Logger
		{
			get { return logger ??= CreateLogger(); }
		}
		ILogger<BillingService> logger;

		internal virtual ILogger<BillingService> CreateLogger() => loggerFactory.CreateLogger<BillingService>();

		readonly IServiceProvider serviceProvider;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var startFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (startFolder != null)
			{
				try
				{
					pluginsManager = CreatePluginsManager(Path.Combine(startFolder, settingsFileName), Path.Combine(startFolder, stateFileName));
					pluginsManager.PluginsLoadFailed += async delegate
					{
						Logger.LogInformation("PluginsLoadFailed. Stopping host");
						await StopAsync(stoppingToken);
					};
					pluginsManager.PluginsLoadSettingsFileFailed += async delegate
					{
						Logger.LogInformation("PluginsLoadSettingsFileFailed. Stopping host");
						await StopAsync(stoppingToken);
					};
					pluginsManager.PluginsLoadStateFileFailed += async delegate
					{
						Logger.LogInformation("PluginsLoadStateFileFailed. Stopping host");
						await StopAsync(stoppingToken);
					};
					pluginsManager.Start();
					Logger.LogInformation("Plugin manager started successfully.");
				}
				catch (Exception e)
				{
					errorReportingClient.ReportToIssueManager("Failed to start billing service.", e, Logger);
					await StopAsync(stoppingToken);
				}
			}
			else
			{
				errorReportingClient.ReportToIssueManager("Failed to start billing service: check assembly location.", new ArgumentException("Assembly location is root directory or null"), Logger);
				await StopAsync(stoppingToken);
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation("Disposing plugin manager");
			if (pluginsManager != null)
			{
				pluginsManager.Dispose();
				pluginsManager = null;
			}
			return base.StopAsync(cancellationToken);
		}

		internal virtual IBillingServiceClient CreateBillingServiceClient() => serviceProvider.GetService<IBillingServiceClient>();
		internal virtual IBillingKafkaClient CreateBillingKafkaClient() => serviceProvider.GetService<IBillingKafkaClient>();

		readonly IConfiguration configuration;
		readonly IErrorReportingClient errorReportingClient;
		readonly ILoggerFactory loggerFactory;
		readonly Dictionary<string, string> globalSettings;
	}
}
