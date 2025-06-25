using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	public class PluginControllerTestHelper
	{
		public static PluginControllerSettings PluginControllerSettings(Type pluginType, bool isActive = true, string schedulerType = "")
		{
			return new PluginControllerSettings
			{
				Key = pluginType.FullName + ".Key",
				Active = isActive,
				TypeName = pluginType.FullName,
				IntervalMinutes = 10,
				RetryIntervalSeconds = 10,
				MaxRetryAttempts = 10,
				SchedulerType = schedulerType
			};
		}

		public static PluginControllerState PluginControllerState(Type pluginType, DateTime lastSuccessfulRun, DateTime lastTransactionTimestamp)
		{
			return new PluginControllerState
			{
				Key = pluginType.FullName + ".Key",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};
		}
	}
}
