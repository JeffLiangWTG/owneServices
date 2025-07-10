using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReceiveOperationalActionSupporter : WhsOperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsReceive;

		public override BusinessContext BusinessContext => BusinessContext.WhsInwards;

		public override Type RootType => typeof(WhsReceive);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#region SingularElementNoun

		public override string SingularElementNoun => Res.GetString("Warehouse|ReceiveOperationalActionSupporter|SingularElementNoun", "Receive");

		#endregion

		#region PluralElementNoun

		public override string PluralElementNoun => Res.GetString("Warehouse|ReceiveOperationalActionSupporter|PluralElementNoun", "Receives");

		#endregion

		#region IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint => Env.Security.WhsReceiveJobInvoicing;

		#endregion
	}
}
