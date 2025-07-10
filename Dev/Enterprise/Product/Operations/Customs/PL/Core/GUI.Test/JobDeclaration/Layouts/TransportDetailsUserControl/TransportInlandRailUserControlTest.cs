using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class TransportInlandRailUserControlTest : TestCase
{
	public void TestChildControlsCount()
	{
		AssertEquals("Only controls for Train Number and Nationality", 2, control.Controls.Count);
	}

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
