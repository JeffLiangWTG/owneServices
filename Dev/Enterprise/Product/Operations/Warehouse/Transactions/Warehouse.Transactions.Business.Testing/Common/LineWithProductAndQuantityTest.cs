using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class LineWithProductAndQuantityTest : WhsTestCaseWithFactory
	{
		public void TestLineWithProductAndQuantity()
		{
			var lineWithProductAndQuantity = new LineWithProductAndQuantity(ZGuid.BrettsGuid, 10m);

			AssertEquals("Value is correct.", ZGuid.BrettsGuid, lineWithProductAndQuantity.ProductPK);
			AssertEquals("Value is correct.", 10m, lineWithProductAndQuantity.Quantity);
		}
	}
}
