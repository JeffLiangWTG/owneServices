using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ShipmentDetailsFinalDestinationUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestFinalDestinationFindBox()
		{
			var finalDestinationFindBox = control.FinalDestinationFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), finalDestinationFindBox.Location);
				AssertEquals("Before the Date of arrival", 0, finalDestinationFindBox.TabIndex);
			});
		}

		public void TestEstimatedArrivalDateEdit()
		{
			var estimatedArrivalDateEdit = control.EstimatedArrivalDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true), estimatedArrivalDateEdit.Location);
				AssertEquals("After the Final Destination", 1, estimatedArrivalDateEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsFinalDestinationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsFinalDestinationUserControl control;
	}
}
