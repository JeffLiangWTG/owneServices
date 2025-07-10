using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageTestHelperTest : TestCaseWithFactory
	{
		public void TestCreateSeaVoyage()
		{
			var carrier = Factory.New<OrgHeader>();

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "123", carrier.PK);
			AssertEquals(Constants.TransportModes.Sea, voyage.JV_AirSeaRoad);
			AssertEquals("Visund", voyage.JV_RV_NKVessel);
			AssertEquals("123", voyage.JV_VoyageFlight);
			AssertEquals(carrier.PK, voyage.JV_OH_Line);

			AssertEquals("AUSYD", voyage.Sailings[0].JX_JA_RL_NKPortOfLoading);
			AssertEquals("NZAKL", voyage.Sailings[0].JX_JB_RL_NKPortOfDischarge);

			AssertNotNull(RefVessel.LookupVesselByCode("Visund", Factory));

			var voyage1 = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "456", carrier.PK, "AUMEL", "USLAX");
			AssertEquals(Constants.TransportModes.Sea, voyage1.JV_AirSeaRoad);
			AssertEquals("Visund", voyage1.JV_RV_NKVessel);
			AssertEquals("456", voyage1.JV_VoyageFlight);
			AssertEquals(carrier.PK, voyage1.JV_OH_Line);

			AssertEquals("AUMEL", voyage1.Sailings[0].JX_JA_RL_NKPortOfLoading);
			AssertEquals("USLAX", voyage1.Sailings[0].JX_JB_RL_NKPortOfDischarge);

			AssertEquals("Existing vessel was used", 1, RefVessel.LookupVesselByName("Visund", Factory).Length);
		}

		public void TestCreateVoyage()
		{
			var carrier = Factory.New<OrgHeader>();

			var voyage = new VoyageTestHelper(Factory).CreateVoyage(Constants.TransportModes.Road, "Visund", "123", carrier.PK, "AUMEL", "USLAX");
			AssertEquals(Constants.TransportModes.Road, voyage.JV_AirSeaRoad);
			AssertEquals("Visund", voyage.JV_RV_NKVessel);
			AssertEquals("123", voyage.JV_VoyageFlight);
			AssertEquals(carrier.PK, voyage.JV_OH_Line);

			AssertEquals("AUMEL", voyage.Sailings[0].JX_JA_RL_NKPortOfLoading);
			AssertEquals("USLAX", voyage.Sailings[0].JX_JB_RL_NKPortOfDischarge);

			AssertNotNull(RefVessel.LookupVesselByCode("Visund", Factory));
		}
	}
}
