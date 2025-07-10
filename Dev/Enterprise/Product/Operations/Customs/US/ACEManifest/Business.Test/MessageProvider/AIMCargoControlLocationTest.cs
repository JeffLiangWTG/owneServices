using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMCargoControlLocationTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CarrierCode = "SHA2";
			header.AMA_RL_NKPortOfFirstArrival = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var aimCargoControlLocation = new AIMCargoControlLocation(header);
			AssertEquals("SYD", aimCargoControlLocation.AirportOfArrival);
			AssertEquals("SHA", aimCargoControlLocation.CargoTerminalOperator);
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			var aimCargoControlLocation2 = new AIMCargoControlLocation(header);
			AssertEquals("LAX", aimCargoControlLocation2.AirportOfArrival);
			AssertEquals("SHA", aimCargoControlLocation2.CargoTerminalOperator);
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			header.AMA_CarrierCode = ZString.Empty;
			var aimCargoControlLocation3 = new AIMCargoControlLocation(header);
			AssertEquals(ZString.Empty, aimCargoControlLocation3.AirportOfArrival);
			AssertEquals(ZString.Empty, aimCargoControlLocation3.CargoTerminalOperator);
		}
	}
}
