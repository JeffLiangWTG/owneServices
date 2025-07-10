using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeOperationalActionsSupporter))]
	class StocktakeOperationalActionsSupporterTest : OperationalActionSupporterTest<StocktakeOperationalActionsSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsStocktake;

		#endregion
	}
}
