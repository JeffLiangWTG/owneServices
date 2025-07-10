using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.Module
{
	class DtbConsignmentOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.LTConsignment; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.DtbConsignment;

		public override Type RootType
		{
			get { return typeof(DtbConsignment); }
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

		public SecurityCheckpoint InvoicingCheckpoint
		{
			get { return Env.Security.DtbConsignmentJobInvoicing; }
		}
	}
}
