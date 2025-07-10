using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderToBulkUpdateCollection))]
	sealed class OrderToBulkUpdateCollectionBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrderToBulkUpdateCollection>
	{
		OrderDetailsBulkUpdateBusinessObject Parent;
		protected override OrderToBulkUpdateCollection GetCollectionToTest()
		{
			Parent = new OrderDetailsBulkUpdateBusinessObject(Factory);
			return new OrderToBulkUpdateCollection(Factory, Parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrderToBulkUpdate(Factory, Parent);
		}
	}
}
