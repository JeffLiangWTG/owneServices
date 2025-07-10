using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(GenericLandedCostHistoryCollection))]
	sealed class GenericLandedCostHistoryCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadUsingForeignKey()
		{
			var collection = new GenericLandedCostHistoryCollection(Header);
			collection.Load();
			AssertEquals("No items yet", 0, collection.Count);

			var history1 = Factory.New<LandedCostHistory>();
			history1.LH_LT = Header.PK;
			collection.Load();
			AssertEquals("one item", 1, collection.Count);

			var history2 = Factory.New<LandedCostHistory>();
			history2.LH_LT = Header.PK;
			collection.Load();
			AssertEquals("Two items", 2, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new GenericLandedCostHistoryCollection(Header);

		LandedCostHeader header;
		LandedCostHeader Header => header ?? (header = Factory.New<LandedCostHeader>());
	}
}
