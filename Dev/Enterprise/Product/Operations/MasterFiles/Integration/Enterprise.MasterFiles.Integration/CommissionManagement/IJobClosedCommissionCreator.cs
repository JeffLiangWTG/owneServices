namespace Enterprise.MasterFiles.Integration
{
	public interface IJobClosedCommissionCreator : ICommissionCreator
	{
		void PostQueueItemOrCreateCommissionsOnJobClosed();
		void CreateCommissionsOnJobClosed();
	}
}
