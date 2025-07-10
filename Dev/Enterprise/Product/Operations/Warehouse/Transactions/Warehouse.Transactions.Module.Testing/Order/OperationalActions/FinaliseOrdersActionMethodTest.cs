using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseOrdersActionMethod))]
	public class FinaliseOrdersActionMethodTest : FinalizeDocketActionMethodTest<FinaliseOrdersActionMethod, WhsOrder>
	{
		protected override string ExpectedOperationalActionName => "Finalize Orders";
	}
}
