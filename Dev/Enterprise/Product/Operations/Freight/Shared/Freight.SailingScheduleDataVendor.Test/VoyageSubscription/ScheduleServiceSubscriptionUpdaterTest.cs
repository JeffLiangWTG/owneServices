using System;
using System.Linq;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.SailingScheduleDataVendor.Testing
{
	abstract class ScheduleServiceSubscriptionUpdaterTest<T> : LogSubscriberTest<T> where T : LogSubscriber, new()
	{
		public void TestProcessLogs_ScheduleFeedServiceIsDisabled_DoNotProcessLogs()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var businessObject = GetBusinessObjectInstance();
				businessObject.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as Schedule Feed Tracking is disabled.", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_eHubIDIsNotSpecified_DoNotProcessLogs()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
				{
					var businessObject = GetBusinessObjectInstance();
					businessObject.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));

					Factory.Save();
					RunLogWalkerCycleForTest();

					AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as eHub ID is not set.", GetLogsAsString(Notifier));
				}
			}
		}

		protected abstract EnterpriseBusinessObject GetBusinessObjectInstance();

		protected string GetLogsAsString(ILogger logger)
		{
			var logPrefix = string.Format("[{0}]", LogSubscriber.FriendlyName);
			var logs = ((LoggerForTesting)logger).NotifiedEventList.Where(l => l.StartsWith(logPrefix)).Select(l => l.Substring(logPrefix.Length + 1));

			return string.Join(System.Environment.NewLine, logs);
		}
	}
}
