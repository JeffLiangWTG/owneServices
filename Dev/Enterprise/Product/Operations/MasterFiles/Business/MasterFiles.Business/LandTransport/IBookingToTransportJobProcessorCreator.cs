using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IBookingToTransportJobProcessorCreator
	{
		IProcessor CreateBookingToTransportJobProcessor(IWorkflowProvider provider, ProcessTaskNotification action);
	}
}
