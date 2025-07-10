using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocumentInventoryOptions))]
	public class WhsDocumentInventoryOptionsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestDocumentInventories

		public void TestDocumentInventories()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();

			var options = new WhsDocumentInventoryOptions(receive.Inventory);
			AssertEquals("No Inventory", 0, options.DocumentInventories.Count);

			var inv1 = receive.Lines.AddNew().Inventory[0];
			var inv2 = receive.Lines.AddNew().Inventory[0];
			options = new WhsDocumentInventoryOptions(receive.Inventory);
			AssertEquals("2 Inventory", 2, options.DocumentInventories.Count);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			return new WhsDocumentInventoryOptions(receive.Inventory);
		}

		#endregion
	}
}
