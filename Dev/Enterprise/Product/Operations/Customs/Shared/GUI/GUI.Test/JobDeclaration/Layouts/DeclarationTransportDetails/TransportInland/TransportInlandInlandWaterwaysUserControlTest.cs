using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandInlandWaterwaysUserControlTest : TestCase
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

		public void TestVesselIDTextBox()
		{
			var vesselIDTextBox = control.VesselIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), vesselIDTextBox.Location);
				AssertEquals("Tab", 0, vesselIDTextBox.TabIndex);
				AssertEquals("Character Casing", System.Windows.Forms.CharacterCasing.Normal, vesselIDTextBox.CharacterCasing);
				AssertEquals("Caption", "Vessel ID", vesselIDTextBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "ID", vesselIDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", vesselIDTextBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandInlandWaterwaysUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandInlandWaterwaysUserControl control;
	}
}
