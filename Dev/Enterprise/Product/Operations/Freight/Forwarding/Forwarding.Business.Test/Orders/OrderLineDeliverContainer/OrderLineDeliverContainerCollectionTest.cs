using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineDeliverContainerCollection))]
	sealed class OrderLineDeliverContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<OrderLineDeliverContainerCollection>
	{
		protected override OrderLineDeliverContainerCollection GetCollectionToTest()
		{
			OrderLineDelivery delivery = Factory.New<OrderLineDelivery>();
			return new OrderLineDeliverContainerCollection(delivery);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderLineDeliverContainer>();
		}
	}
}
