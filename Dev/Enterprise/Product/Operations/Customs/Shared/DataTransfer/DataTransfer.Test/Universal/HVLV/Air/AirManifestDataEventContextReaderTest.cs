using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class AirManifestDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MAWB = "0815555111";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "USCHI";
			mawb.CM_FlightNo = "QF001";
			mawb.CM_MasterHouseBill = "MHB";

			var manager = mawb.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-5555111
MAWBOriginIATAAirportCode - SYD
MAWBDestinationIATAAirportCode - MDW
MBOLOriginUNLOCO - AUSYD
MBOLDestinationUNLOCO - USCHI
MasterHouseBill - MHB
".Trim(), eventContextValues);
		}
	}
}
