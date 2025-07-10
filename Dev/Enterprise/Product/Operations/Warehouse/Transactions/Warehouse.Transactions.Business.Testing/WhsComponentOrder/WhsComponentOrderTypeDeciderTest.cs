namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsComponentOrderTypeDeciderTest : WhsDocketTypeDeciderTestCase
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(WhsComponentOrder), new WhsComponentOrderTypeDecider().GetTypeForBinding());
		}

		public void GetTypeForNew()
		{
			AssertEquals(typeof(WhsWorkOrder), new WhsComponentOrderTypeDecider().GetTypeForNew());
		}
	}
}
