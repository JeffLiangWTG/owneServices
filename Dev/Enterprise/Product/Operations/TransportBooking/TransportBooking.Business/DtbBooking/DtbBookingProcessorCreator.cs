using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingProcessorCreator : IDtbBookingProcessorCreator
	{
		public IProcessor CreateDtbBookingProcessor(IWorkflowProvider provider, ProcessTaskNotification action)
		{
			return new DtbBookingProcessor((IDtbBookingParent)provider, action);
		}
	}
}
