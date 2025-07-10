using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class NameShipmentsEventArgsTest : TestCaseWithFactory
	{
		public void TestNameShipmentsEventArgs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			EnterShipmentNumbersEventArgs e = new EnterShipmentNumbersEventArgs(consol);
			AssertSame("Consol is same", consol, e.Consol);
		}
	}
}
