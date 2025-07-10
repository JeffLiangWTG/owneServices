using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineCollection))]
	sealed class OrderLineCollectionBOTest : ActiveBusinessObjectCollectionTestCase<OrderLineCollection>
	{
		#region Implementation

		protected override OrderLineCollection GetCollectionToTest()
		{
			Order parentOrder = Factory.New<Order>();
			return new OrderLineCollection(parentOrder);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderLine>();
		}

		#endregion
	}
}
