namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsDocketPalletLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		public void TestPalletTypes()
		{
			WhsDocketPallet pallet = Factory.New<WhsDocketPallet>();
			AssertNotNull(pallet.Lookups.PalletTypes);
		}
	}
}
