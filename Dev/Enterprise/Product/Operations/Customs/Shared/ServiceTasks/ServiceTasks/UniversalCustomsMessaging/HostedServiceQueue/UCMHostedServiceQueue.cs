using System;
using CargoWise.Common;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCMHostedServiceQueue : IHostedServiceQueue
	{
		public UCMHostedServiceQueue(string serviceTaskCode, string name, string applicationCode, Func<string, QueueResult> getQueueResult)
		{
			this.ServiceTaskCode = Argument.NotNullOrEmpty(serviceTaskCode, nameof(serviceTaskCode));
			this.Name = Argument.NotNullOrEmpty(name, nameof(name));
			this.applicationCode = Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode));
			this.getQueueResult = Argument.NotNull(getQueueResult, nameof(getQueueResult));
		}

		QueueResult IHostedServiceQueue.QueueResult => getQueueResult(applicationCode);

		public string Name { get; }

		public string ServiceTaskCode { get; }

		readonly string applicationCode;
		readonly Func<string, QueueResult> getQueueResult;
	}
}
