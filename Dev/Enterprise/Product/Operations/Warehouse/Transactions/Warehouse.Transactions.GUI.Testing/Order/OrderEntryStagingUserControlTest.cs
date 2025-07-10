using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrderEntryStagingUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestEntryStagingInstructionLabel()
		{
			using (var control = new OrderEntryStagingUserControl())
			{
				control.Show();
				var entryStagingInstructionLabel = GUITestHelper.FindControl<ZLabel>(control.Controls, "EntryStagingInstructionLabel");
				var expectedString = "Sometimes the Cross-Dock Area Volumes (to the left) and Allocations (below) will not be the most current. To see the most up to date values, Save, Close and Reopen this Order.";
				AssertEquals(expectedString, entryStagingInstructionLabel.CaptionResourceString.FullDescription);
			}
		}
	}
}
