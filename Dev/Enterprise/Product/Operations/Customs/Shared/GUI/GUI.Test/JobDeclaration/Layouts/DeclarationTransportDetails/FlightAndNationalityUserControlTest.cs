using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class FlightAndNationalityUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestFlightNumberTextBox()
		{
			var flightNumberTextBox = control.FlightNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), flightNumberTextBox.Location);
				AssertEquals("Before the Transport Nationality", 0, flightNumberTextBox.TabIndex);
				AssertEquals("Caption", "Flight", flightNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Flt.", flightNumberTextBox.CaptionResourceString.ShortCaption);
			});
		}

		public void TestTransportNationalityFindBox()
		{
			var transportNationalityFindBox = control.TransportNationalityFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityFindBox.Location);
				AssertEquals("After the VoyageFlightNumber", 1, transportNationalityFindBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new FlightAndNationalityUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		FlightAndNationalityUserControl control;
	}
}
