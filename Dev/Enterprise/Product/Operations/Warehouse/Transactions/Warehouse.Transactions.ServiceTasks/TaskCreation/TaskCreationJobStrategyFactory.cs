using System.Collections;
using CargoWise.Application;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class TaskCreationJobStrategyFactory : ITaskCreationJobStrategyFactory
	{
		public ITaskCreationJobStrategy GetJobStrategy(string jobType)
		{
			Argument.NotNullOrEmpty(jobType, nameof(jobType));

			var providers = ObjectFactory.Get<Hashtable>(WarehouseTaskCreationStrategies);
			var providerHandle = (ObjectHandle)providers[jobType];
			return (ITaskCreationJobStrategy)providerHandle?.GetObject();
		}

		const string WarehouseTaskCreationStrategies = nameof(WarehouseTaskCreationStrategies);
	}
}
