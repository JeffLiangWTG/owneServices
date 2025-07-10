using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusTransportTest : TestCaseWithFactory
	{
		public void TestICusTransport_Properties()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_RadioCallSign = "CSN";
			vessel.RV_CarrierCode = "SEA";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC123SYD";
			orgHeader.MainAddress.OA_Code = "OFC: ADDRESS 1";
			var orgCusCode = carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			orgCusCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			ICusCarHeader carHeader = new CusCarHeader(header);
			header.TSS_VoyageFlight = "FL001";
			header.TSS_CargoCarrierPK = carrier.PK;
			header.AMA_E_ARV = new ZDateTime(2018, 3, 20);
			header.TSS_DateOfDeparture = new ZDateTime(2018, 3, 22);
			header.TSS_Vessel = vessel.RV_Code;
			AssertEquals(1, carHeader.Transports.Count());
			var firstTransport = carHeader.Transports.First();
			AssertEquals("FL001", firstTransport.VoyageFlightNumber);
			AssertEquals("AIR", firstTransport.TranportMode);
			AssertEquals("1234", firstTransport.CarrierCode);
			AssertEquals("CARRIE2WC (OFC: ADDRESS 1)", firstTransport.CarrierName);
			AssertEquals("CSN", firstTransport.CallSign);
			AssertEquals(new ZDateTime(2018, 3, 20), firstTransport.ETA);
			AssertEquals(new ZDateTime(2018, 3, 22), firstTransport.ETD);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("1234", firstTransport.CarrierCode);
		}
	}
}
