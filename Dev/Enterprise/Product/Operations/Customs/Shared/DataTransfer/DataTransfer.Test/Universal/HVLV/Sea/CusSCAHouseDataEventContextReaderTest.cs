using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCAHouseDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var oceanBill = Factory.NewWithValidTestData<TestCusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OCEAN_BILL";
			oceanBill.CB_Voyage = "VOYAGE";
			oceanBill.CB_LloydsIMO = "1234567";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "AUMEL";
			oceanBill.CB_VesselName = "ADELAIDE EXPRESS";
			var houseBill = Factory.NewWithValidTestData<CusSCAHouseForTest>();
			houseBill.CA_CB = oceanBill.PK;
			houseBill.CA_RL_NKDischargePort = "AUSYD";
			houseBill.CA_RL_NKLoadPort = "AUMEL";
			houseBill.CA_ShipmentStatus = "HLD";

			var manager = houseBill.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLNumber - OCEAN_BILL
MBOLOriginUNLOCO - AUMEL
MBOLDestinationUNLOCO - AUSYD
LloydsNumber - 1234567
VoyageNumber - VOYAGE
ComplianceStatus - HLD
".Trim(), eventContextValues);
		}
	}
}
