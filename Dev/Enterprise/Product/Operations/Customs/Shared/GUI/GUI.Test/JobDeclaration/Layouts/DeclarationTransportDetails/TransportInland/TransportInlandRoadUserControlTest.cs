using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandRoadUserControlTest : TestCase
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
				AssertEquals("Tab", 0, transportIDTextBox.TabIndex);
				AssertEquals("Caption", "Transport ID", transportIDTextBox.CaptionResourceString.Caption);
				AssertEquals("Binding", "JE_TransportIDInland", transportIDTextBox.BindTo);
			});
		}

		public void TestTransportNationalityCodeFindBox()
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 0, true), transportNationalityCodeFindBox.Location);
				AssertEquals("Tab", 1, transportNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationalityInland", transportNationalityCodeFindBox.BindTo);
			});
		}

		public void TestTrailer1IDTextBox()
		{
			var trailer1IDTextBox = control.Trailer1IDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 0, true), trailer1IDTextBox.Location);
				AssertEquals("Tab", 2, trailer1IDTextBox.TabIndex);
				AssertEquals("Caption", "Trailer 1 ID", trailer1IDTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Trailer 1", trailer1IDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_Trailer1RegNo", trailer1IDTextBox.BindTo);
			});
		}

		public void TestTrailer1NationalityCodeFindBox()
		{
			var trailer1NationalityCodeFindBox = control.Trailer1NationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), trailer1NationalityCodeFindBox.Location);
				AssertEquals("Tab", 3, trailer1NationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTrailer1Nationality", trailer1NationalityCodeFindBox.BindTo);
			});
		}

		public void TestTrailer2IDTextBox()
		{
			var trailer2IDTextBox = control.Trailer2IDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 22, true), trailer2IDTextBox.Location);
				AssertEquals("Tab", 4, trailer2IDTextBox.TabIndex);
				AssertEquals("Caption", "Trailer 2 ID", trailer2IDTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Trailer 2", trailer2IDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_Trailer2RegNo", trailer2IDTextBox.BindTo);
			});
		}

		public void TestTrailer2NationalityCodeFindBox()
		{
			var trailer2NationalityCodeFindBox = control.Trailer2NationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 22, true), trailer2NationalityCodeFindBox.Location);
				AssertEquals("Tab", 5, trailer2NationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTrailer2Nationality", trailer2NationalityCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandRoadUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandRoadUserControl control;
	}
}
