using System.Reflection;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	public class TestPluginManager : PluginsManager
	{
		public TestPluginManager(Assembly pluginsAssembly, string settingsFilePath, string stateFilePath, Dictionary<string, string> globalSettings, ILoggerFactory loggerFactory, Func<IBillingServiceClient> serviceClientFactory, Func<IBillingKafkaClient> kafkaClientFactory, IErrorReportingClient errorReportingClient, IClock clock, Dictionary<string, string[]> knownExceptions = null) : base(pluginsAssembly, settingsFilePath, stateFilePath, globalSettings, loggerFactory, serviceClientFactory, kafkaClientFactory, errorReportingClient, null, knownExceptions)
		{
			Clock = clock;
		}

		protected internal override IClock CreateClock()
		{
			return Clock;
		}

		IClock Clock { get; }
	}
}
