using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

class ShipmentDetailsIncoTermsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestIncoTermDropEdit()
	{
		AssertType<ZDropEdit>(control.IncoTermDropEdit);
	}

	public void TestIncoTermExplainButton()
	{
		AssertType<ZButton>(control.IncoTermExplainButton);
	}

	public void TestShipmentIncoTermPlaceTextBox()
	{
		AssertType<ZTextBox>(control.ShipmentIncoTermPlaceTextBox);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsIncoTermsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ShipmentDetailsIncoTermsUserControl control;
}
