using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseAdjustmentsActionMethod))]
	class FinaliseAdjustmentsActionMethodTest : FinalizeDocketActionMethodTest<FinaliseAdjustmentsActionMethod, WhsAdjustment>
	{
		protected override string ExpectedOperationalActionName => "Finalize Adjustments";
	}
}
