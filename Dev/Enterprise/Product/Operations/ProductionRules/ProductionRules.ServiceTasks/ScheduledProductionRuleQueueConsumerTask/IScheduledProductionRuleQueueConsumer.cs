using System.Threading;
using CargoWise.ComponentModel;

namespace Enterprise.ProductionRules.ServiceTasks
{
	public interface IScheduledProductionRuleQueueConsumer
	{
		void ProcessQueue(INotifications notifications, CancellationToken token);
	}
}
