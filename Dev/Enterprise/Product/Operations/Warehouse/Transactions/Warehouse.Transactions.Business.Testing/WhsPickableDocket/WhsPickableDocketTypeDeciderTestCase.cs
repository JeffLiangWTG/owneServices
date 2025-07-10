namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsPickableDocketTypeDeciderTestCase : WhsDocketTypeDeciderTestCase
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(WhsPickableDocket), new WhsPickableDocketTypeDecider().GetTypeForBinding());
		}
	}
}
