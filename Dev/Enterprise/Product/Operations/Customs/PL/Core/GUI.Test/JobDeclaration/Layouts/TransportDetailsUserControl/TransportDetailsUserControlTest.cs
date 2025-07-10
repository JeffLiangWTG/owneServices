using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.TransportDetails.Testing;

sealed class TransportDetailsUserControlTest : TestCase
{
	public void TestTransportInlandRailUserControl()
	{
		AssertType<TransportInlandRailUserControl>(control.TransportInlandRailUserControl);
	}

	public void TestVesselUserControl()
	{
		AssertType<VesselUserControl>(control.VesselUserControl);
	}

	TransportDetailsUserControl control;

	protected override void SetUp()
	{
		base.SetUp();
		control = new TransportDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
