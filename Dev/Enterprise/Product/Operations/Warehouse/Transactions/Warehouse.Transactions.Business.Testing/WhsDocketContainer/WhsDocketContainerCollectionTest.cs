using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketContainerCollection))]
	class WhsDocketContainerCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsDocketContainerCollection>
	{
		protected override WhsDocketContainerCollection GetCollectionToTest()
		{
			var master = Factory.New<WhsReceive>();
			return new WhsDocketContainerCollection(master, Factory);
		}
	}
}
