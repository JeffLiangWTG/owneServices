using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.Module
{
	internal class DtbBookingConsignmentOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		#region OperationalActionSupporter Members

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsignment; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.DtbBookingConsignment;

		public override Type RootType
		{
			get { return typeof(DtbBookingConsignment); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("DtbConsignmentOperationalActionSupporter|SingularElementNoun", "Consignment"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("DtbConsignmentOperationalActionSupporter|PluralElementNoun", "Consignments"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#endregion

		#region IInvoicingSecurityCheckpointProvider Members

		public SecurityCheckpoint InvoicingCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
