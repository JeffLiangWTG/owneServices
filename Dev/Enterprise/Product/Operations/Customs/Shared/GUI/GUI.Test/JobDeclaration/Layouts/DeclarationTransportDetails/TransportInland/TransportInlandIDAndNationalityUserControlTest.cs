using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandIDAndNationalityUserControlTest : TestCase
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

		public void TestTransportIDTextBox()
		{
			var transportIDTextBox = control.TransportIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), transportIDTextBox.Location);
				AssertEquals("Tab", 0, transportIDTextBox.TabIndex);
				AssertEquals("Character Casing", System.Windows.Forms.CharacterCasing.Normal, transportIDTextBox.CharacterCasing);
				AssertEquals("Caption", "Transport ID", transportIDTextBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "ID", transportIDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", transportIDTextBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandIDAndNationalityUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandIDAndNationalityUserControl control;
	}
}
