using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessQueueLogCollectionBase : BaseStmALogDependentCollection<ProcessQueueLog>
	{
		public ProcessQueueLogCollectionBase(ProcessQueue processQueue)
			: base(processQueue)
		{
		}
	}
}
