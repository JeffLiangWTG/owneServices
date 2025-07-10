using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDocumentInventoryEventArgsTest : TestCaseWithFactory
	{
		#region Properties

		public void TestProperties()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var options = new WhsDocumentInventoryOptions(receive.Inventory);
			var e = new WhsDocumentInventoryEventArgs(options);
			AssertEquals(true, e.ContinueToPrint);
			AssertEquals(options, e.Options);

			e.ContinueToPrint = false;
			AssertEquals(false, e.ContinueToPrint);
		}

		#endregion
	}
}
