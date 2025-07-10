using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyBookingInvoicingSupporter : AgencyShipmentInvoicingSupporter
	{
		public AgencyBookingInvoicingSupporter(AgencyBooking parent)
			: base(parent) { }

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.AgencyBookingAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.AgencyBookingJobInvoicing;
		}
	}
}


