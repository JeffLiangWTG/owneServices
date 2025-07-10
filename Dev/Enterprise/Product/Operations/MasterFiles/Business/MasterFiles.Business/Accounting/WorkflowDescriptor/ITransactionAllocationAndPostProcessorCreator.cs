using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransactionAllocationAndPostProcessorCreator
	{
		IProcessor Create(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog);
	}
}
