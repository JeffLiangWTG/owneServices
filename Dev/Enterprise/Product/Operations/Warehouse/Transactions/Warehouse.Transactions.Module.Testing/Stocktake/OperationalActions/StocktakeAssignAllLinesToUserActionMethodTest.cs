using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeAssignAllLinesToUserActionMethod))]
	class StocktakeAssignAllLinesToUserActionMethodTest : AssignAllLinesToUserActionMethodTest<StocktakeAssignAllLinesToUserActionMethod, WhsStocktake>
	{
		protected override string GetExpectedNameAndDescription() => "Assign Stocktake Lines to User";

		protected override StocktakeAssignAllLinesToUserActionMethod NewMethod()
		{
			return new StocktakeAssignAllLinesToUserActionMethod();
		}
	}
}
