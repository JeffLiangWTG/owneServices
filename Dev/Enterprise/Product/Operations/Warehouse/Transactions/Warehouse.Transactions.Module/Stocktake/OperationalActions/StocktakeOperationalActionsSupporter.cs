using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeOperationalActionsSupporter : WhsOperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.WhsStocktake;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsStocktake;

		public override Type RootType => typeof(WhsStocktake);
	}
}