using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

class ShipmentDetailsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestShipmentDetailsIncoTermsUserControl()
	{
		AssertType<ShipmentDetailsIncoTermsUserControl>(control.ShipmentDetailsIncoTermsUserControl);
	}

	public void TestAgreedPlaceCodeFindBox()
	{
		AssertType<ZCodeFindBox>(control.AgreedPlaceCodeFindBox);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ShipmentDetailsUserControl control;
}
