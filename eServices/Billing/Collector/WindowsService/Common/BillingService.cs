using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using CargoWise.Billing.CollectorService.Plugin;
using Microsoft.Extensions.Logging;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Common.Logging;
using Confluent.Kafka;
using WTG.ErrorReporting;
using TimeoutException = System.TimeoutException;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common
{
	public partial class BillingService : ServiceBase
	{
		public BillingService(Assembly pluginsAssembly, string settingsFileName, string stateFileName, ILoggerFactory loggerFactory)
		{
			this.pluginsAssembly = pluginsAssembly ?? throw new ArgumentNullException("pluginsAssembly");
			this.settingsFileName = settingsFileName ?? throw new ArgumentNullException("settingsFileName");
			this.stateFileName = stateFileName ?? throw new ArgumentNullException("stateFileName");
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException("loggerFactory");

			InitializeComponent();
		}

		protected sealed override void OnStart(string[] args)
		{
			var startFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (startFolder != null)
			{
				try
				{
					pluginsManager = CreatePluginsManager(Path.Combine(startFolder, settingsFileName), Path.Combine(startFolder, stateFileName));
					pluginsManager.PluginsLoadFailed += delegate { Stop(); };
					pluginsManager.PluginsLoadSettingsFileFailed += delegate { Stop(); };
					pluginsManager.PluginsLoadStateFileFailed += delegate { Stop(); };
					pluginsManager.Start();
				}
				catch (Exception e)
				{
					GetErrorReportingClient().ReportToIssueManager("Failed to start billing service.", e, Logger);
					Stop();
				}
			}
			else
			{
				GetErrorReportingClient().ReportToIssueManager("Failed to start billing service: check assembly location.", new ArgumentException("Assembly location is root directory or null"), Logger);
				Stop();
			}
		}

		protected sealed override void OnStop()
		{
			if (pluginsManager != null)
			{
				pluginsManager.Dispose();
				pluginsManager = null;
			}
		}

		internal virtual IPluginsManager CreatePluginsManager(string settingsFilePath, string stateFilePath)
		{
			return new PluginsManager(pluginsAssembly, settingsFilePath, stateFilePath, globalSettings, loggerFactory, CreateBillingServiceClient, CreateBillingKafkaClient, GetErrorReportingClient(), null, KnownExceptions);
		}

		internal virtual IBillingServiceClient CreateBillingServiceClient() => new BillingServiceClient();
		internal virtual IBillingKafkaClient CreateBillingKafkaClient() => new BillingKafkaClient(CreateBillingProducerConfig());
		internal virtual IErrorReportingClient GetErrorReportingClient() => issueErrorReportingClient ?? (issueErrorReportingClient = new ErrorReportingClient(IssueManagerUri));
		IErrorReportingClient issueErrorReportingClient;

		Uri IssueManagerUri =>
			globalSettings.TryGetValue("IssueManagerUri", out var url) && !string.IsNullOrEmpty(url)
				? new Uri(url)
				: new Uri(string.Empty, UriKind.Relative);

		IPluginsManager pluginsManager;
		readonly Assembly pluginsAssembly;
		readonly string settingsFileName;
		readonly string stateFileName;

		public ILogger<BillingService> Logger
		{
			get { return logger ?? (logger = CreateLogger()); }
		}
		ILogger<BillingService> logger;

		public virtual ILogger<BillingService> CreateLogger() => loggerFactory.CreateLogger<BillingService>();

		readonly Dictionary<string, string> globalSettings = ConfigurationManager.AppSettings.AllKeys.ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

		internal static readonly Dictionary<string, string[]> KnownExceptions = new Dictionary<string, string[]>()
		{
			{ typeof(TimeoutException).FullName, null },
			{ typeof(EndpointNotFoundException).FullName, null },
			{ typeof(SqlException).FullName, new [] { "Could not open a connection to SQL Server", "Could not use view or function 'eHubArchiveMessage' because of binding errors" } },
			{ typeof(FaultException).FullName, new [] { "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible.", "Cannot open database \"billing\" requested by the login. The login failed.\r\nLogin failed for user 'billing_admin'" } }
		};

		internal static ProducerConfig CreateBillingProducerConfig()
		{
			var baseClientConfig = GetSettings("billingKafkaClientSettings");
			var producerSettings = GetSettings("billingKafkaProducerSettings");
			return BillingKafkaClient.GetKafkaConfig<ProducerConfig>(baseClientConfig, producerSettings);
		}
		private static Dictionary<string, string> GetSettings(string sectionName)
		{
			var settings = (Hashtable)ConfigurationManager.GetSection(sectionName);
			var settingsDic = settings.Cast<DictionaryEntry>().ToDictionary(x => x.Key.ToString(), y => y.Value.ToString());
			return settingsDic;
		}
		readonly ILoggerFactory loggerFactory;
	}
}
