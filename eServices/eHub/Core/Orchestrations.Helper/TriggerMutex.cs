using System;
using System.Linq;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Operations;
using Microsoft.XLANGs.Core;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public class TriggerMutex
	{
		public const string MutexPropertyName = "TriggerMutex";
		public const string MutexPropertyNamespace = "http://cargowise.com/ehub/core/mutex";

		public static bool CheckNoParallelInstanceExist(ILog logger = null)
		{
			return CheckNoParallelInstanceExist(CurrentOrchestrationInstanceId, CurrentOrchestrationName, logger);
		}


		// TODO: https://docs.telerik.com/devtools/justmock/advanced-usage/sealed-mocking and publish for the whole team this.
		public static bool CheckNoParallelInstanceExist(Guid currentOrchestrationInstanceId, string orchestrationName, ILog logger, int retries = 3)
		{
			if (retries == 0) throw new FatalMessageProcessingException("TriggerMutex check having problem, expected BizTalkMessage should be init.");
			if (logger == null) logger = new NoOpLogger();

			try
			{
				using (var bizTalkOperations = new BizTalkOperations())
				{
					var rawInstances = bizTalkOperations.GetServiceInstances();
					if (rawInstances == null)
					{
						logger.Error("GetServiceInstances returned null.");
						throw new Exception("Failed to retrieve service instances.");
					}

					var instances = rawInstances.Cast<Instance>().Where(x =>
					{
						var orchestrationInstance = x as OrchestrationInstance;
						return orchestrationInstance != null
						       && HasOption(orchestrationInstance.InstanceStatus, InstanceStatus.Active | InstanceStatus.Dehydrated)
						       && orchestrationInstance.ServiceType.Contains(orchestrationName);
					}).Cast<OrchestrationInstance>().ToList();

					logger.Debug($"Found {instances.Count} active or dehydrated instances for orchestration: {orchestrationName}");

					var currentInstance = instances.SingleOrDefault(x => x.ID == currentOrchestrationInstanceId);
					if (currentInstance == null)
					{
						logger.Error($"Current orchestration instance with ID {currentOrchestrationInstanceId} not found.");
						throw new Exception("Current orchestration instance not found.");
					}
					logger.Debug($"Current instance ID: {currentOrchestrationInstanceId}");

					var mutexPropertyValues = currentInstance.Messages.Cast<BizTalkMessage>()
																.Where(x => !string.IsNullOrEmpty((string)x.Context.Read(MutexPropertyName, MutexPropertyNamespace)))
																.Select(x => (string)x.Context.Read(MutexPropertyName, MutexPropertyNamespace)).ToList();

					logger.Debug($"Mutex property values found: {string.Join(", ", mutexPropertyValues)}");

					if (mutexPropertyValues.Count == 0)
					{
						logger.Error($"Expected {MutexPropertyName}, {MutexPropertyNamespace} context property but could not find it.");
						throw new FatalMessageProcessingException($"Expected {MutexPropertyName}, {MutexPropertyNamespace} context property but could not find it.");
					}

					OrchestrationInstance previousOrchestration = null;
					string mutexValue = "";

					foreach (var instance in instances)
					{
						if (instance.ID == currentOrchestrationInstanceId) continue;

						foreach (var message in instance.Messages)
						{
							var bizTalkMessage = message as BizTalkMessage;
							if (bizTalkMessage == null)
								continue;

							mutexValue = (string)bizTalkMessage.Context.Read(MutexPropertyName, MutexPropertyNamespace);
							if (!string.IsNullOrEmpty(mutexValue) && mutexPropertyValues.Contains(mutexValue))
							{
								previousOrchestration = instance;
								logger.Debug($"Found mutex conflict with instance ID: {instance.ID}, Mutex Value: {mutexValue}");
								break;
							}
						}
						if (previousOrchestration != null)
							break;
					}

					if (previousOrchestration != null)
					{
						logger.DebugFormat(
							"Found TriggerMutex! Found previous orchestration with {0} ID is running. State:{1}, Try to acquire mutex on: {2}, Mutex found: {4}, CreationTime:{3}",
							previousOrchestration.ID, previousOrchestration.InstanceStatus,
							string.Join(", ", mutexPropertyValues), previousOrchestration.CreationTime, mutexValue);
					}
					else
					{
						logger.DebugFormat("No TriggerMutex! Current orchestration ID: {0}, State: {1}, Mutex Acquired: {2}, Created On: {3}",
							currentInstance.ID, currentInstance.InstanceStatus, string.Join(", ", mutexPropertyValues), currentInstance.CreationTime);
					}

					return previousOrchestration == null;
				}
			}
			catch (Exception ex)
			{
				logger.Error("An error occurred while checking parallel instances.", ex);
				throw;
			}
		}


		static bool HasOption(InstanceStatus actual, InstanceStatus expect)
		{
			return (expect & actual) != 0;
		}

		public static Guid CurrentOrchestrationInstanceId
		{
			get
			{
				return Service.RootService.InstanceId;
			}
		}

		public static string CurrentOrchestrationName
		{
			get
			{
				return Service.RootService.FullName;
			}
		}
	}
}
