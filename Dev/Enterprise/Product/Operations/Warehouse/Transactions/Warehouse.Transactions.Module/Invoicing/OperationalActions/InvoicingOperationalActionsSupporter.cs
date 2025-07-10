using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InvoicingOperationalActionsSupporter : WhsOperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.WhsPeriodicBilling;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsInvoicing;

		public override Type RootType => typeof(WhsInvoice);
	}
}
