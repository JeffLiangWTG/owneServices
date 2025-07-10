namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsExceptionsTest : WhsTestCaseWithFactory
	{
		public void TestPickFailedException()
		{
			PickFailedException e = new PickFailedException();
			AssertEquals("Pick Failed", e.Message);
			e = new PickFailedException("Test");
			AssertEquals("Test", e.Message);
		}

		public void TestNotEnoughStockException()
		{
			NotEnoughStockException e = new NotEnoughStockException();
			AssertEquals("Not enough stock", e.Message);
			AssertEquals(0m, e.UnitsFound);

			e = new NotEnoughStockException(50.4m);
			AssertEquals("Not enough stock", e.Message);
			AssertEquals(50.4m, e.UnitsFound);
		}
	}
}
