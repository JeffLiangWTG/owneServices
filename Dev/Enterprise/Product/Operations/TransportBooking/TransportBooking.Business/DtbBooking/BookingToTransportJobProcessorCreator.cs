using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class BookingToTransportJobProcessorCreator : IBookingToTransportJobProcessorCreator
	{
		public IProcessor CreateBookingToTransportJobProcessor(IWorkflowProvider provider, ProcessTaskNotification action)
		{
			return new BookingToTransportJobProcessor((DtbBooking)provider, action);
		}
	}
}
