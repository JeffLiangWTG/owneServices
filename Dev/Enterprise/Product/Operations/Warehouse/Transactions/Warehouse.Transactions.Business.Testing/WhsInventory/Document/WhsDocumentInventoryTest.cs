using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocumentInventory))]
	public class WhsDocumentInventoryTest : NonPersistentBusinessObjectTestCase
	{
		#region TestTotalLabelsToPrintFrom

		public void TestTotalLabelsToPrintFrom()
		{
			var docInventory = new WhsDocumentInventory();
			AssertEquals(0m, docInventory.TotalLabelsToPrintFrom);

			var inventory = Factory.New<WhsInventoryView>();
			docInventory = new WhsDocumentInventory(inventory);
			AssertEquals(0m, docInventory.TotalLabelsToPrintFrom);

			inventory.WI_TotalUnits = 10.1m;
			AssertEquals(11m, docInventory.TotalLabelsToPrintFrom);
		}

		#endregion

		#region TestInventory

		public void TestInventory()
		{
			var docInventory = new WhsDocumentInventory();
			AssertNull(docInventory.Inventory);

			var inventory = Factory.New<WhsInventoryView>();
			docInventory = new WhsDocumentInventory(inventory);
			AssertEquals(inventory, docInventory.Inventory);
		}

		#endregion

		#region TestValidationType

		public void TestValidationType()
		{
			var docInventory = new WhsDocumentInventory();
			AssertEquals(typeof(WhsDocumentInventoryValidation), docInventory.Validation.GetType());
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsDocumentInventory(Factory.New<WhsInventoryView>());
		}

		#endregion
	}
}
