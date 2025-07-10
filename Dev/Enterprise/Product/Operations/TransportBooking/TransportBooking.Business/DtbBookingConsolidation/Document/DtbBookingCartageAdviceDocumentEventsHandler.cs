using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingCartageAdviceDocumentEventsHandler : CartageAdviceDocumentEventsHandler
	{
		public override bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return ContainsCartageAdvice(menuItem.Documents);
		}

		protected override IDocumentSupportable[] Bookings => new DtbBooking[] { ((DtbBookingDocumentSupporter)DocumentSupporter).Booking };
	}
}
