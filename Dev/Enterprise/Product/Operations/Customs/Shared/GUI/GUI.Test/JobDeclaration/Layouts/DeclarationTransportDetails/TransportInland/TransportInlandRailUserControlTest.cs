using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandRailUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTrainNationalityCodeFindBox()
		{
			var trainNationalityCodeFindBox = control.TrainNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), trainNationalityCodeFindBox.Location);
				AssertEquals("Tab", 1, trainNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationalityInland", trainNationalityCodeFindBox.BindTo);
			});
		}

		public void TestTrainNumberTextBox()
		{
			var trainNumberTextBox = control.TrainNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), trainNumberTextBox.Location);
				AssertEquals("Tab", 0, trainNumberTextBox.TabIndex);
				AssertEquals("Caption", "Train Number", trainNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Train Num.", trainNumberTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", trainNumberTextBox.BindTo);
			});
		}

		public void TestWagonNumberTextBox()
		{
			var wagonNumberTextBox = control.WagonNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true), wagonNumberTextBox.Location);
				AssertEquals("Tab", 2, wagonNumberTextBox.TabIndex);
				AssertEquals("Caption", "Wagon Number", wagonNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Wagon Num.", wagonNumberTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_Trailer1RegNo", wagonNumberTextBox.BindTo);
			});
		}

		public void TestWagonNationalityCodeFindBox()
		{
			var wagonNationalityCodeFindBox = control.WagonNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 22, true), wagonNationalityCodeFindBox.Location);
				AssertEquals("Tab", 3, wagonNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTrailer1Nationality", wagonNationalityCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandRailUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandRailUserControl control;
	}
}
