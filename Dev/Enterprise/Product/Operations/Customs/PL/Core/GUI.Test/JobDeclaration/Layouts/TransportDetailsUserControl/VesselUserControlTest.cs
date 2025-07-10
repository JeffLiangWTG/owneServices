using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class VesselUserControlTest : TestCase
{
	public void TestVesselName()
	{
		var vesselCodeFindBox = control.VesselCodeFindBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), vesselCodeFindBox.Location);
			AssertEquals("Tab", 0, vesselCodeFindBox.TabIndex);
			AssertEquals("Binding", "JE_VesselName", vesselCodeFindBox.BindTo);
			AssertEquals("VesselCodeFindBox.Caption", "Vessel", vesselCodeFindBox.GetExtension<LabelCaptionRenderer>().Caption);
		});
	}

	public void TestLloydsIMO()
	{
		var lloydsIMOTextBox = control.LloydsIMOTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true), lloydsIMOTextBox.Location);
			AssertEquals("Tab", 1, lloydsIMOTextBox.TabIndex);
			AssertEquals("Binding", "JE_LloydsIMO", lloydsIMOTextBox.BindTo);
			AssertEquals("LloydsIMOTextBox.Caption", "IMO No.", lloydsIMOTextBox.GetExtension<LabelCaptionRenderer>().Caption);
		});
	}

	VesselUserControl control;

	protected override void SetUp()
	{
		base.SetUp();
		control = new VesselUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
