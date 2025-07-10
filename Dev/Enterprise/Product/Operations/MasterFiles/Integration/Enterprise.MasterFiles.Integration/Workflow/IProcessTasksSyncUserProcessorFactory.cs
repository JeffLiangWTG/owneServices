namespace Enterprise.Warehouse.Integration
{
	public interface IProcessTasksSyncUserProcessorFactory
	{
		IProcessTaskSyncUserProcessor GetUserSyncProcessor(string formFlowType);
	}
}
