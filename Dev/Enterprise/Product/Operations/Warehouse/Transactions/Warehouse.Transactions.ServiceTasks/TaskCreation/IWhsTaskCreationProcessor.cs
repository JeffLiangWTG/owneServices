using System.Threading;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	interface IWhsTaskCreationProcessor
	{
		void ProcessQueue(INotifications notifications, CancellationToken token);
	}
}
