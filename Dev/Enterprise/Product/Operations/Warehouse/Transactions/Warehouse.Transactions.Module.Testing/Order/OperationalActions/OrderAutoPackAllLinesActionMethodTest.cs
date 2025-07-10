using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderAutoPackAllLinesActionMethod))]
	public class OrderAutoPackAllLinesActionMethodTest : OperationalActionMethodTest<OrderAutoPackAllLinesActionMethod>
	{
		protected override OrderAutoPackAllLinesActionMethod NewMethod()
		{
			return new OrderAutoPackAllLinesActionMethod();
		}
	}
}
