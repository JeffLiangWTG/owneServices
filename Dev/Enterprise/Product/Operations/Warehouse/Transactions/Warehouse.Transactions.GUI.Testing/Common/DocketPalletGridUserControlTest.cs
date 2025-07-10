namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class DocketPalletGridUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestBindingAttributes()
		{
			using (DocketPalletGridUserControl userControl = new DocketPalletGridUserControl())
			{
				AssertEquals("Pallets", userControl.BindTo);
				userControl.BindTo = "PalletsTest";
				AssertEquals("PalletsTest", userControl.BindTo);
			}
		}
	}
}
