using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(OrderItemCollection))]
	sealed class OrderItemCollectionBOTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDocsAndCartage master = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			return new OrderItemCollection(master, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderItem>();
		}
	}
}
