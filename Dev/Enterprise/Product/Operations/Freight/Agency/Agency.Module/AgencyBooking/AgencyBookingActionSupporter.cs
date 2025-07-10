using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class AgencyBookingActionSupporter : AgencyShipmentActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyBooking; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyBooking;

		public override string PluralElementNoun
		{
			get { return Res.GetString("963dfd76-0c20-4c04-87e9-20af84f8a7eb", "bookings"); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("b108b878-b3dc-4afe-90a2-d7595bad9d19", "booking"); }
		}

		public override Type RootType
		{
			get { return typeof(AgencyBooking); }
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.AgencyBookingJobInvoicing; }
		}

		#endregion

		#region PopulateMethods

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipping);
		}

		#endregion
	}
}



