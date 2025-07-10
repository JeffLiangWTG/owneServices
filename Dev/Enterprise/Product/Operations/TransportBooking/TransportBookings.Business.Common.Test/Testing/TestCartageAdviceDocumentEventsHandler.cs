using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public class TestCartageAdviceDocumentEventsHandler : CartageAdviceDocumentEventsHandler
	{
		public TestCartageAdviceDocumentEventsHandler(IDtbBooking booking)
		{
			this.booking = booking;
		}

		protected override IDocumentSupportable[] Bookings
		{
			get
			{
				return new IDocumentSupportable[] { (IDocumentSupportable)booking };
			}
		}

		public override bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			// Most subclasses of CartageAdviceDocumentEventsHandler also check child menus, but DtbBookingCartageAdviceDocumentEventsHandler does not
			return ContainsCartageAdvice(menuItem.Documents);
		}

		readonly IDtbBooking booking;
	}
}
