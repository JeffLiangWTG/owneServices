using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulatePropagationTargets_FromOrderLineToOrder()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			var orderLine2 = order.OrderLines.AddNew();

			var handlineInfo = new OrderLineProcessHandlingInfo(orderLine1);
			var log = orderLine1.Logs.AddNew(Events.FreightUnloaded);

			var targets = handlineInfo.GetPropagationTargets(log);
			AssertContainsExactElementsInAnyOrder(
				PropagationLinkComparer.ByPKs,
				new[] { new PropagationLink(order, new[] { orderLine1, orderLine2 }, "Order OrderLines") },
				targets);
		}
	}
}
