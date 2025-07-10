using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentContainersInfoTest : TestCaseWithFactory
	{
		public void TestUseVerifiedGrossContainerWeight()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var booking = Factory.New<AgencyBooking>();
			var info = new AgencyContainersInfo(billOfLading, true);
			AssertEquals(false, info.IsVGM);
			info = new AgencyContainersInfo(booking, true);
			AssertEquals(true, info.IsVGM);
			info = new AgencyContainersInfo(booking, false);
			AssertEquals(false, info.IsVGM);
		}
	}
}
