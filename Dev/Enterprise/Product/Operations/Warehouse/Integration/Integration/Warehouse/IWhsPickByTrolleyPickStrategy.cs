namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickByTrolleyPickStrategy : IWhsPickStrategy
	{
		bool HasAnyPackageAssignedToATrolleyJob(IWhsOrder order);
	}
}
