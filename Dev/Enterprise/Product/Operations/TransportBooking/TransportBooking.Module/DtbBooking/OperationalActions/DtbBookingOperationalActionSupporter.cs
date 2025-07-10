using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.DtbBooking);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbBooking; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.DtbBooking;

		public override Type RootType
		{
			get { return typeof(DtbBooking); }
		}

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.DtbTransportBookingsOperations; }
		}
	}
}
