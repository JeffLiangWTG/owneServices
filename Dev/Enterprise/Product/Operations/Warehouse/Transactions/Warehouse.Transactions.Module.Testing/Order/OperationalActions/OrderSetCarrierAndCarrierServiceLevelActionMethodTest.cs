using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderSetCarrierAndCarrierServiceLevelActionMethod))]
	public class OrderSetCarrierAndCarrierServiceLevelActionMethodTest : OperationalActionMethodTest<OrderSetCarrierAndCarrierServiceLevelActionMethod>
	{
		protected override OrderSetCarrierAndCarrierServiceLevelActionMethod NewMethod()
		{
			return new OrderSetCarrierAndCarrierServiceLevelActionMethod();
		}
	}
}
