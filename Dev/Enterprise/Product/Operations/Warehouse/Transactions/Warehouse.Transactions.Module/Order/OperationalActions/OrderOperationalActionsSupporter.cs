using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderOperationalActionSupporter : WhsOperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsOrder;

		public override BusinessContext BusinessContext => BusinessContext.WhsOrder;

		public override Type RootType => typeof(WhsOrder);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#region SingularElementrNoun

		public override string SingularElementNoun => Res.GetString("Warehouse|OrderOperationalActionSupporter|SingularElementNoun", "Order");

		#endregion

		#region PluralElementNoun

		public override string PluralElementNoun => Res.GetString("Warehouse|OrderOperationalActionSupporter|PluralElementNoun", "Orders");

		#endregion

		#region IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint => Env.Security.WhsOrderJobInvoicing;

		#endregion
	}
}
