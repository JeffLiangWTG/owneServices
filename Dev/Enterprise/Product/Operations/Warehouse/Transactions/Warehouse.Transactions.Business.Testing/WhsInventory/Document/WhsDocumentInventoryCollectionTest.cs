using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocumentInventoryCollection))]
	public class WhsDocumentInventoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsDocumentInventoryCollection>
	{
		#region TestCollection

		public void TestCollection()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var documentInventories = WhsDocumentInventoryCollection.New(receive.Inventory);
			AssertEquals("No Inventory", 0, documentInventories.Count);

			var inv1 = receive.Lines.AddNew().Inventory[0];
			var inv2 = receive.Lines.AddNew().Inventory[0];
			documentInventories = WhsDocumentInventoryCollection.New(receive.Inventory);
			AssertEquals("2 Inventory", 2, documentInventories.Count);
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
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var inventory = receive.Inventory.AddNew();

			return new WhsDocumentInventory(inventory);
		}

		protected override WhsDocumentInventoryCollection GetCollectionToTest()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			return WhsDocumentInventoryCollection.New(receive.Inventory);
		}

		#endregion
	}
}
