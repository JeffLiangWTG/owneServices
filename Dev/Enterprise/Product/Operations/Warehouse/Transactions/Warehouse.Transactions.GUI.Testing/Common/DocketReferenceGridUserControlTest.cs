namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class DocketReferenceGridUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestBindingAttributes()
		{
			using (DocketReferenceGridUserControl userControl = new DocketReferenceGridUserControl())
			{
				AssertEquals("References", userControl.BindTo);
				userControl.BindTo = "ReferencesTest";
				AssertEquals("ReferencesTest", userControl.BindTo);
			}
		}
	}
}
