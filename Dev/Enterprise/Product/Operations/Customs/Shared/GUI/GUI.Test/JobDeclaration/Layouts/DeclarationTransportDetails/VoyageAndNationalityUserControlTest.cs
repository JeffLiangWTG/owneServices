using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class VoyageAndNationalityUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestVoyageFlightNumberTextBox()
		{
			var voyageNumberTextBox = control.VoyageNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), voyageNumberTextBox.Location);
				AssertEquals("Before the Transport Nationality", 0, voyageNumberTextBox.TabIndex);
				AssertEquals("Caption", "Voyage", voyageNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Voy.", voyageNumberTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_VoyageFlightNo", voyageNumberTextBox.BindTo);
			});
		}

		public void TestTransportNationalityFindBox()
		{
			var transportNationalityFindBox = control.TransportNationalityFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityFindBox.Location);
				AssertEquals("After the VoyageFlightNumber", 1, transportNationalityFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationality", transportNationalityFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new VoyageAndNationalityUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		VoyageAndNationalityUserControl control;
	}
}
