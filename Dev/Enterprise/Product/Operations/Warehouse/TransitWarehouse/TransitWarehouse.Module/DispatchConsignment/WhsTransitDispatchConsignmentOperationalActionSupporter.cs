using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Module
{
	public sealed class WhsTransitDispatchConsignmentOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitDspConsignmnt; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsItemDispatchConsignment;

		public override Type RootType
		{
			get { return typeof(WhsItemDispatchConsignment); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("d0475755-bf58-4794-b323-c649da1abffd", "Dispatch Consignment"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("c7fcda12-1ce8-4781-85bb-eaacffe8d2b0", "Dispatch Consignments"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.WhsItemDispatchConsignmentJobInvoicing; }
		}

		#endregion
	}
}
