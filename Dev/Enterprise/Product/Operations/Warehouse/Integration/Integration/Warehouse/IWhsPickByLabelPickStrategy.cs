namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickByLabelPickStrategy : IWhsPickStrategy
	{
		bool IsOrderAssociatedWithPickByLabelJob(IWhsOrder order);
	}
}
