using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator
	{
		IProcessor Create(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog);
	}
}
