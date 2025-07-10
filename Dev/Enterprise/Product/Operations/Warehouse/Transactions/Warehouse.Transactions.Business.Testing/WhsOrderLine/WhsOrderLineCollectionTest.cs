using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLineCollection))]
	sealed class WhsOrderLineCollectionTest : WhsOrderLineCollectionTest<WhsOrderLineCollection>
	{
		protected override WhsOrderLineCollection GetCollectionToTest()
		{
			return new WhsOrderLineCollection(Factory.NewWithValidTestData<WhsOrder>());
		}
	}
}
