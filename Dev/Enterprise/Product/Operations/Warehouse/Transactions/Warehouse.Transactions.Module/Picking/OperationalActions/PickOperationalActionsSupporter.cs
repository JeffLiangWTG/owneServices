using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickOperationalActionsSupporter : WhsOperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.WhsDespatch;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsPicking;

		public override Type RootType => typeof(WhsPick);
	}
}
