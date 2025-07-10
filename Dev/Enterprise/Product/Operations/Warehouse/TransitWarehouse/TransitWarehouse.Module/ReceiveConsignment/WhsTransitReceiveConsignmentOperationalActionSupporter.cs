using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Module
{
	public sealed class WhsTransitReceiveConsignmentOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitRcvConsignmnt; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsItemReceiveConsignment;

		public override Type RootType
		{
			get { return typeof(WhsItemReceiveConsignment); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("7d08b462-bcf3-4074-bfb9-2848f731c7b7", "Receive Consignment"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("dfb41e8d-c35d-4e40-b555-0c03e5ccf75c", "Receive Consignments"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.WhsItemReceiveConsignmentJobInvoicing; }
		}

		#endregion
	}
}
