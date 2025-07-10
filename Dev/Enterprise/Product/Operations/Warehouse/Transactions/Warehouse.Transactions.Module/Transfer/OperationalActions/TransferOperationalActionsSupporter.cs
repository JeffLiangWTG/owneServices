using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferOperationalActionsSupporter : WhsOperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.WhsTransfer;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsTransfer;

		public override Type RootType => typeof(WhsTransfer);
	}
}