using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ShipmentDetailsScreeningUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestScreeningStatusDropEdit()
		{
			var screeningStatusDropEdit = control.ScreeningStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), screeningStatusDropEdit.Location);
				AssertEquals("Before the Screening button", 0, screeningStatusDropEdit.TabIndex);
			});
		}

		public void TestScreenButton()
		{
			var screenButton = control.ScreenButton;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 0, true), screenButton.Location);
				AssertEquals("After the Screening Status", 1, screenButton.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsScreeningUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsScreeningUserControl control;
	}
}
