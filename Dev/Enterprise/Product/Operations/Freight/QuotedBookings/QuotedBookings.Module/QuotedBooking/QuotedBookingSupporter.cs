using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class QuotedBookingSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext => CargoWise.Definitions.BusinessContext.QuotedBooking; // keep full name to avoid confusion

		public override BusinessContext DocumentBusinessContext => CargoWise.Definitions.BusinessContext.QuotedBooking;

		public override Type RootType => typeof(QuotedBooking);

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint => Env.Security.QuickBookingJobInvoicing;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.QuickBooking;

		public override string SingularElementNoun => Res.GetString("QuoteBooking|Supporter|SingularElementNoun", "Booking");

		public override string PluralElementNoun => Res.GetString("QuoteBooking|Supporter|PluralElementNoun", "Bookings");

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}
	}
}
