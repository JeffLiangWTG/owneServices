using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondHeaderSailingSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronise()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2006, 4, 20);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var header = Factory.New<CusInBondHeader>();
			header.ChangeSailing(sailing.PK);
			header.BH_OverrideFreightDefaults = true;
			header.BH_OverrideFreightDefaults = false;
			AssertEquals("NZAKL", header.BH_RL_NKPortUnlading);
			AssertEquals(new ZDateTime(2006, 4, 20), header.BH_ETA);
			AssertEquals("AALSMEERGRACHT", header.BH_ImportConveyanceName);
			AssertEquals("123SD", header.BH_VoyageNumber);

			voyage.JV_VoyageFlight = "123456";
			sailing = voyage.Sailings[0];
			header.ChangeSailing(sailing.PK);
			AssertEquals("12345", header.BH_VoyageNumber);

			voyage.JV_VoyageFlight = "1234";
			sailing = voyage.Sailings[0];
			header.ChangeSailing(sailing.PK);
			AssertEquals("1234", header.BH_VoyageNumber);

			voyage.JV_VoyageFlight = ZString.Empty;
			sailing = voyage.Sailings[0];
			header.ChangeSailing(sailing.PK);
			AssertEquals(ZString.Empty, header.BH_VoyageNumber);
		}
	}
}
