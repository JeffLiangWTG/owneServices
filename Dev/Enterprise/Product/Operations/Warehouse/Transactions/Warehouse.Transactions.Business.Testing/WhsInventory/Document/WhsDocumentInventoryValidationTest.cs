using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDocumentInventoryValidationTest : BusinessObjectValidationTestCase
	{
		#region TestLabelsToPrint

		public void TestLabelsToPrint()
		{
			var docInventory = new WhsDocumentInventory();
			AssertEquals(0m, docInventory.LabelsToPrint);
			AssertNoErrors(docInventory.LabelsToPrintInfo);

			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_TotalUnits = 10.1m;

			docInventory = new WhsDocumentInventory(inventory);
			AssertEquals(11m, docInventory.LabelsToPrint);
			AssertNoErrors(docInventory.LabelsToPrintInfo);

			docInventory.LabelsToPrint = 12m;
			AssertEquals(12m, docInventory.LabelsToPrint);
			AssertHasError(docInventory.LabelsToPrintInfo, "You cannot print more labels than available");

			docInventory.LabelsToPrint = -1m;
			AssertEquals(-1m, docInventory.LabelsToPrint);
			AssertHasError(docInventory.LabelsToPrintInfo, "Please enter the number of labels to print");

			docInventory.LabelsToPrint = 0m;
			AssertEquals(0m, docInventory.LabelsToPrint);
			AssertNoErrors(docInventory.LabelsToPrintInfo);
		}

		#endregion
	}
}
