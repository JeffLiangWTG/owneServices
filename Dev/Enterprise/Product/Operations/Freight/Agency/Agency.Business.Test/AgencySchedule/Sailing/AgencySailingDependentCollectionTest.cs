using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencySailingDependentCollectionTest : BaseAgencyTest
	{
		public void TestSync()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.GenerateSailings();
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AssertNotNull(principal.Sailings);
			AssertEquals("opened a voyage with a single sailing (Voyage)", 1, voyage.Sailings.Count);
			AssertEquals("opened a voyage with a single sailing (Schedule)", 1, principal.Sailings.Count);
			VoyageOrigin origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_RL_NKPortOfLoading = OverseasPort3;
			voyage.Origins.Add(origin2);
			VoyageDestination destination2 = Factory.New<VoyageDestination>();
			destination2.JB_RL_NKPortOfDischarge = OverseasPort2;
			voyage.Destinations.Add(destination2);
			voyage.GenerateSailings();
			AssertEquals("should have 2 sailing", 2, principal.Sailings.Count);
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			voyage.GenerateSailings();
			AssertEquals("should have 4 sailings", 4, principal.Sailings.Count);
			origin1.JA_RL_NKPortOfLoading = OverseasPort4;
			voyage.GenerateSailings();
			AssertEquals("Should have 2 sailings", 2, principal.Sailings.Count);
			origin2.Delete();
			voyage.GenerateSailings();
			AssertEquals("Should have 0 sailings", 0, principal.Sailings.Count);
		}

		public void TestAllowNew()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AssertEquals("AllowNew", false, schedule.GenericPrincipal.Sailings.AllowNew);
		}
	}
}
