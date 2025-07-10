using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbAgentBookingInvoicingSupporter : DtbBookingInvoicingSupporter
	{
		public DtbAgentBookingInvoicingSupporter(DtbAgentBooking parent)
			: base(parent.Booking)
		{
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgentBooking; }
		}
	}
}
