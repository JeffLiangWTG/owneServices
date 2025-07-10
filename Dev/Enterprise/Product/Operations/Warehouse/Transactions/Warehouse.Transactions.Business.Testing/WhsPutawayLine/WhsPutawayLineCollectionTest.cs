using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayLineCollection))]
	public class WhsPutawayLineCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsPutawayLineCollection>
	{
		protected override WhsPutawayLineCollection GetCollectionToTest()
		{
			return new WhsPutawayLineCollection(Factory.New<WhsPutawayJob>());
		}
	}
}
