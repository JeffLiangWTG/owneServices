using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportIDAndNationalityUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportIDTextBox()
		{
			var transportIDTextBox = control.TransportIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), transportIDTextBox.Location);
				AssertEquals("Before the Transport Nationality", 0, transportIDTextBox.TabIndex);
				AssertEquals("Caption used in the layout", "Transport ID", transportIDTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestTransportNationalityFindBox()
		{
			var transportNationalityFindBox = control.TransportNationalityFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityFindBox.Location);
				AssertEquals("After the Transport ID", 1, transportNationalityFindBox.TabIndex);
				AssertEquals("PreBoundMaxLength", 2, transportNationalityFindBox.PreBoundMaxLength);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportIDAndNationalityUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportIDAndNationalityUserControl control;
	}
}
