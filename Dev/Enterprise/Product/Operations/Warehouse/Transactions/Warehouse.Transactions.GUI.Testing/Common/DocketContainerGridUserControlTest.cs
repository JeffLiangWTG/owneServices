namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class DocketContainerGridUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestBindingAttributes()
		{
			using (DocketContainerGridUserControl userControl = new DocketContainerGridUserControl())
			{
				AssertEquals("Containers", userControl.BindTo);
				userControl.BindTo = "ContainersTest";
				AssertEquals("ContainersTest", userControl.BindTo);
			}
		}
	}
}
