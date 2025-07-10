using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DummyWithDtbBookingJobInvoicingSupporterWithIServiceDirection : DummyWithDtbBookingJobInvoicingSupporter, IServiceDirection
	{
		public ZString ServiceDirection => "Test ServiceDirection";
	}
}
