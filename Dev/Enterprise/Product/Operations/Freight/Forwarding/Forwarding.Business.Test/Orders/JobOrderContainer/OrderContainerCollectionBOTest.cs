using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderContainerCollection))]
	sealed class OrderContainerCollectionBOTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Order parentOrder = Factory.New<Order>();
			return new OrderContainerCollection(parentOrder, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderContainer>();
		}
	}
}
