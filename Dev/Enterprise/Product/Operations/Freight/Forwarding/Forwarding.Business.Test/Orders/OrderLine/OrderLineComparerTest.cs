using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			Order order = Factory.New<Order>();

			OrderLine orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 3;
			orderLine1.JO_LineSplitNumber = 2;

			OrderLine orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineNo = 3;
			orderLine2.JO_LineSplitNumber = 1;
			orderLine2.JO_SubLineNo = 2;

			OrderLine orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_LineNo = 2;

			OrderLine orderLine4 = order.OrderLines.AddNew();
			orderLine4.JO_LineNo = 1;

			OrderLine orderLine5 = order.OrderLines.AddNew();
			orderLine5.JO_LineNo = 3;
			orderLine5.JO_LineSplitNumber = 1;
			orderLine5.JO_SubLineNo = 1;

			List<OrderLine> orderLines = new List<OrderLine>(order.OrderLines);
			orderLines.Sort(new OrderLineComparer());

			AssertEquals(orderLine4, orderLines[0]);
			AssertEquals(orderLine3, orderLines[1]);
			AssertEquals(orderLine5, orderLines[2]);
			AssertEquals(orderLine2, orderLines[3]);
			AssertEquals(orderLine1, orderLines[4]);
		}
	}
}
