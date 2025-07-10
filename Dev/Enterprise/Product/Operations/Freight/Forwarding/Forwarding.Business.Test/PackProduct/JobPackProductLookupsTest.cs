using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JobPackProductLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrderLinesListContainsOrderLinesFromAttachedOrders()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingPackLine packline = shipment.OuterPackLines.AddNew();
			PackProduct product = packline.Products.AddNew();

			AssertEquals("Precondition: list is empty", 0, product.Lookups.OrderLines_List.Count);

			Order order1 = Factory.New<Order>();
			order1.OrderLines.AddNew();
			order1.OrderLines.AddNew();

			shipment.AttachedOrders.Add(order1);
			AssertEquals("Orderlines from order1", 2, product.Lookups.OrderLines_List.Count);

			Order order2 = Factory.New<Order>();
			order2.OrderLines.AddNew();
			order2.OrderLines.AddNew();
			order2.OrderLines.AddNew();

			shipment.AttachedOrders.Add(order2);
			AssertEquals("Orderlines from order1 & order2", 5, product.Lookups.OrderLines_List.Count);

			shipment.AttachedOrders.RemoveFromRelationship(order1);
			AssertEquals("Orderlines from order2", 3, product.Lookups.OrderLines_List.Count);
		}
	}
}
