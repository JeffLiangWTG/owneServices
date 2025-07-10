using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryPackageDetailCollection))]
	public class WhsInventoryPackageDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsInventoryPackageDetailCollection>
	{
		#region TestCollection

		public void TestCollection()
		{
			var collection = GetCollectionToTest();
			AssertEquals("No Inventory", 0, collection.Count);
			for (int i = 0; i < 4; i++)
			{
				collection.Add(new WhsInventoryPackageDetail(Factory.NewWithValidTestData<WhsOrder>(), "Packageid00" + i.ToString(), false));
			}
			AssertEquals("4 PackageInfo", 4, collection.Count);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Empty collection", 0, collection.Count);
			AssertEquals("Cannot add lines", false, collection.AllowNew);
			AssertEquals("Cannot delete lines", false, collection.AllowRemove);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsInventoryPackageDetail();
		}

		protected override WhsInventoryPackageDetailCollection GetCollectionToTest()
		{
			return new WhsInventoryPackageDetailCollection(Factory);
		}

		#endregion
	}
}
