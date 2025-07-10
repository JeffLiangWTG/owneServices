using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IDtbBookingProcessorCreator
	{
		IProcessor CreateDtbBookingProcessor(IWorkflowProvider provider, ProcessTaskNotification action);
	}
}
