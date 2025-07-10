using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestGetGenPivotFetchHintQuery()
		{
			var order = Factory.New<Order>();
			var orderFetchStrategy = new OrderFetchStrategy(order);
			var methodInfo = orderFetchStrategy.GetType().GetMethod("GetGenPivotFetchHintQuery", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var result = methodInfo.Invoke(orderFetchStrategy, System.Array.Empty<object>()) as ZQuery;
			CombineAssertions("Please make sure the sql script of fetch hint on GenPiovt hint the index 'NR_RX__XX_RelationType_XX_Relation2ID' ([XX_RelationType] ASC, [XX_Relation2ID] ASC)", () =>
			{
				AssertNotNull(result);
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_Relation2ID));
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_RelationType));
			});
		}
	}
}
