using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	sealed class CurrentQueueUserControlTest : TestCase
	{
		[RequiresSTA]
		public void TestAlignControlsIntoSingleColumn()
		{
			using (CurrentQueueUserControl userControl = new CurrentQueueUserControl())
			{
				AssertEquals("Default should be false", false, userControl.AlignControlsIntoSingleColumn);

				userControl.AlignControlsIntoSingleColumn = true;
				AssertEquals(true, userControl.AlignControlsIntoSingleColumn);

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(8, 80), userControl.AssignToLabel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(8, 104), userControl.ReasonLabel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(80, 80), userControl.TaskAssignedToCodeFindBox.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(80, 104), userControl.ReasonTextBox.Location);

				userControl.AlignControlsIntoSingleColumn = false;
				AssertEquals(false, userControl.AlignControlsIntoSingleColumn);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(328, 8), userControl.AssignToLabel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(328, 32), userControl.ReasonLabel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(400, 8), userControl.TaskAssignedToCodeFindBox.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(400, 32), userControl.ReasonTextBox.Location);
			}
		}
	}
}
