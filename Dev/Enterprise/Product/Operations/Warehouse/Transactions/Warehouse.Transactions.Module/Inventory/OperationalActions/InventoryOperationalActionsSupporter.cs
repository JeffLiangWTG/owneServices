using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InventoryOperationalActionSupporter : WhsOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsInventory;

		public override BusinessContext BusinessContext => BusinessContext.WhsInventory;

		public override Type RootType => typeof(WhsModuleInventory);
	}
}