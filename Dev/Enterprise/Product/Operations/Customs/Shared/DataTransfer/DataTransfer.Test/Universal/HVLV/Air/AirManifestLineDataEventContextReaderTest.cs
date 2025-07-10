using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class AirManifestLineDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MAWB = "0815555111";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "USCHI";
			mawb.CM_FlightNo = "QF001";
			mawb.CM_MasterHouseBill = "MHB";

			var hawb = Factory.NewWithValidTestData<CusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_HAWB = "HAWB001";
			hawb.CS_RL_NKOrigin = "AUMEL";
			hawb.CS_RL_NKDestination = "USLAX";
			hawb.CS_CustomsStatus = "WOF";

			var manager = hawb.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-5555111
MAWBOriginIATAAirportCode - SYD
MAWBDestinationIATAAirportCode - MDW
MBOLOriginUNLOCO - AUSYD
MBOLDestinationUNLOCO - USCHI
MasterHouseBill - MHB
HAWBNumber - HAWB001
HAWBOriginIATAAirportCode - MEL
HAWBDestinationIATAAirportCode - LAX
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - USLAX
ComplianceStatus - WOF
".Trim(), eventContextValues);
		}

		public void TestEventContextValues_NZSpecific()
		{
			var mawb = (CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
			mawb.FillWithValidTestData();
			mawb.CM_MAWB = "0815555111";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "USCHI";
			mawb.CM_FlightNo = "QF001";
			mawb.CM_MasterHouseBill = "MHB";

			var hawb = (CusHAWB)Factory.New<Integration.Customs.NZ.ICusHAWB>();
			hawb.FillWithValidTestData();
			hawb.CS_CM = mawb.PK;
			hawb.CS_HAWB = "HAWB001";
			hawb.CS_RL_NKOrigin = "AUMEL";
			hawb.CS_RL_NKDestination = "USLAX";
			hawb.CS_CustomsStatus = "WOF";
			hawb.CS_MasterHouseBill = "HB";

			var manager = hawb.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-5555111
MAWBOriginIATAAirportCode - SYD
MAWBDestinationIATAAirportCode - MDW
MBOLOriginUNLOCO - AUSYD
MBOLDestinationUNLOCO - USCHI
HAWBNumber - HAWB001
HAWBOriginIATAAirportCode - MEL
HAWBDestinationIATAAirportCode - LAX
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - USLAX
ComplianceStatus - WOF
MasterHouseBill - HB
".Trim(), eventContextValues);
		}
	}
}
