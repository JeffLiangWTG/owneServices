using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineDeliveryCollection))]
	sealed class OrderLineDeliveryCollectionTest : ActiveBusinessObjectCollectionTestCase<OrderLineDeliveryCollection>
	{
		protected override OrderLineDeliveryCollection GetCollectionToTest()
		{
			OrderLine orderLine = Factory.New<OrderLine>();
			return new OrderLineDeliveryCollection(Factory, orderLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderLineDelivery>();
		}

		public void TestAddToCopiedCollection()
		{
			Order order = Factory.New<Order>();

			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();

			OrderLineDelivery orderLineDelivery = line1.Deliveries.AddNew();
			orderLineDelivery.J4_RL_NKDestinationPort = "AUSYD";

			line1.Deliveries.AddToCopiedCollection(line2.Deliveries);

			AssertEquals(1, line2.Deliveries.Count);
			AssertEquals("AUSYD", line2.Deliveries[0].J4_RL_NKDestinationPort);
		}
	}
}
