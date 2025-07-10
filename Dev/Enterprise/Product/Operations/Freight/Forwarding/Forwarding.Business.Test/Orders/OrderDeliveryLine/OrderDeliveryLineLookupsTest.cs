using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderDeliveryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNumberLists()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;
			OrderLine order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_LineNo = 12;

			Order order2 = preAdvice.Orders.AddNew();
			order2.JD_OrderNumber = "2";
			OrderLine order2Line1 = order2.OrderLines.AddNew();
			order2Line1.JO_LineNo = 21;
			OrderLine order2Line2 = order2.OrderLines.AddNew();
			order2Line2.JO_LineNo = 22;

			Order order3 = preAdvice.Orders.AddNew();
			order3.JD_OrderNumber = "3";
			order3.JD_OrderNumberSplit = 1;
			OrderLine order3Line1 = order3.OrderLines.AddNew();
			order3Line1.JO_LineNo = 31;

			OrderDeliveryLine line1 = preAdvice.OrderLines.AddNew();
			AssertEquals("1", line1.Lookups.OrderNumbers[0].Code);
			AssertEquals("2", line1.Lookups.OrderNumbers[1].Code);
			AssertEquals(0, line1.Lookups.OrderLineNumbers.Count);

			line1.OrderNumber = "1";
			AssertEquals(2, line1.Lookups.OrderLineNumbers.Count);
			AssertEquals("11", line1.Lookups.OrderLineNumbers[0].Code);
			AssertEquals("12", line1.Lookups.OrderLineNumbers[1].Code);

			line1.OrderNumber = "3";
			AssertEquals(0, line1.Lookups.OrderLineNumbers.Count);

			line1.OrderNumberSplit = 1;
			AssertEquals(1, line1.Lookups.OrderLineNumbers.Count);
			AssertEquals("31", line1.Lookups.OrderLineNumbers[0].Code);
		}
	}
}
