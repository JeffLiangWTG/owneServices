using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Test.DocumentScanning
{
	internal class AgencyBillContainerEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadAgencyBillContainer()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_UniqueConsignRef = "V00002008";
			agencyShipment.JS_RL_NKOrigin = "AUBNE";
			agencyShipment.JS_RL_NKDestination = "AUSYD";
			agencyShipment.JS_IsForwardRegistered = false;
			agencyShipment.JS_IsShipping = true;
			Factory.Save();

			var loader = new AgencyBillContainerEDocsViaUniversalXmlSupport();
			AssertEquals(agencyShipment.PK, loader.LoadBusinessObjectFromCode(Factory, agencyShipment.JS_UniqueConsignRef)?.PK);
		}

		public void TestTryAgencyBillContainerNotInDb()
		{
			var loader = new AgencyBillContainerEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "V99999999"));
		}
	}
}
