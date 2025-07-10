namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	interface ITaskCreationJobStrategyFactory
	{
		ITaskCreationJobStrategy GetJobStrategy(string jobType);
	}
}
