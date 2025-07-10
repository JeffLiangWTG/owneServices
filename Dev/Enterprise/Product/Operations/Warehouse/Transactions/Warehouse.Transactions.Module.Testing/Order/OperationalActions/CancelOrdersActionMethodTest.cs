using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CancelOrdersActionMethod))]
	class CancelOrdersActionMethodTest : CancelDocketActionMethodTest<CancelOrdersActionMethod, WhsOrder>
	{
		protected override string GetExpectedNameAndDescription() => "Cancel Orders";

		protected override CancelOrdersActionMethod NewMethod()
		{
			return new CancelOrdersActionMethod();
		}
	}
}
