using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderCollection))]
	class ImportedOrderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportedOrderCollection>
	{
		protected override ImportedOrderCollection GetCollectionToTest()
		{
			return new ImportedOrderCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportedOrder(Factory.New<Order>());
		}
	}
}
