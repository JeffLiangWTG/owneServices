using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandAirUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportNationalityCodeFindBox()
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityCodeFindBox.Location);
				AssertEquals("Tab", 1, transportNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationalityInland", transportNationalityCodeFindBox.BindTo);
			});
		}

		public void TestFlightTextBox()
		{
			var flightTextBox = control.FlightTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), flightTextBox.Location);
				AssertEquals("Tab", 0, flightTextBox.TabIndex);
				AssertEquals("Caption", "Flight Number", flightTextBox.CaptionResourceString.Caption);
				AssertEquals("Medium Caption", "Flight Num.", flightTextBox.CaptionResourceString.MediumCaption);
				AssertEquals("Short Caption", "Flight", flightTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", flightTextBox.BindTo);
			});
		}

		public void TestAircraftIDTextBox()
		{
			var aircraftIDTextBox = control.AircraftIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true), aircraftIDTextBox.Location);
				AssertEquals("Tab", 2, aircraftIDTextBox.TabIndex);
				AssertEquals("Binding", "JE_AircraftRegistrationInland", aircraftIDTextBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandAirUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandAirUserControl control;
	}
}
