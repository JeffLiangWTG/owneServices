using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsPortOfDischargeUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestPortOfDischargeFindBox()
		{
			var portOfDischargeFindBox = control.PortOfDischargeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), portOfDischargeFindBox.Location);
				AssertEquals("Before the Date Of Arrival", 0, portOfDischargeFindBox.TabIndex);
				AssertEquals("Short Caption", "Discharge", portOfDischargeFindBox.CaptionResourceString.ShortCaption);
				AssertEquals("Caption", "Port Of Discharge", portOfDischargeFindBox.CaptionResourceString.Caption);
			});
		}

		public void TestDateOfArrivalDateEdit()
		{
			var dateOfArrivalDateEdit = control.DateOfArrivalDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true), dateOfArrivalDateEdit.Location);
				AssertEquals("After the Port Of Discharge", 1, dateOfArrivalDateEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsPortOfDischargeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsPortOfDischargeUserControl control;
	}
}
