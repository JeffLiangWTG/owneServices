using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Scheduler.GraphEngine;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class UCMPHostedServiceQueuesSubProvider : IHostedServiceQueuesSubProvider
	{
		IEnumerable<IHostedServiceQueue> IHostedServiceQueuesSubProvider.Queues
			=> queues ?? (queues = CreateQueues().ToArray());

		IEnumerable<IHostedServiceQueue> queues;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task names")]
		IEnumerable<IHostedServiceQueue> CreateQueues()
		{
			foreach (var applicationCode in UniversalCustomsMessagingSubscribers.GetApplicationCodes())
			{
				yield return new UCKHostedServiceQueue(applicationCode);
				var masterServiceTask = $"{UniversalCustomsMessagingConstants.ServiceTaskCodes.Master}-{applicationCode}";
				yield return new EDIMessageQueueStateQueueProvider(UniversalCustomsMessagingConstants.ServiceTaskCodes.Master, masterServiceTask + " PreKey Service Task", applicationCode, QueueStatusCodes.Codes.PreKey);
				yield return new EDIMessageQueueStateQueueProvider(UniversalCustomsMessagingConstants.ServiceTaskCodes.Master, masterServiceTask + " Blocked Service Task", applicationCode, QueueStatusCodes.Codes.Blocked);
				var workerServiceTask = $"{UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker}-{applicationCode}";
				yield return new EDIMessageQueueStateQueueProvider(UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker, workerServiceTask + " Queued Service Task", applicationCode, QueueStatusCodes.Codes.Queued);
			}
		}
	}
}
