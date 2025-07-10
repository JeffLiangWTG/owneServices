using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyOriginDependentCollectionTest : BaseAgencyTest
	{
		public void TestSync()
		{
			SetUpVoyage();
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(fSeaVoyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			VoyageCountry currentCountry = fSeaVoyage.Countries.GetCountry("AU", true);
			int initialOriginCount = currentCountry.Origins.Count;
			AssertEquals("Origins should be in sync", initialOriginCount, principal.Origins.Count);
			VoyageOrigin addedOrigin = fSeaVoyage.Origins.AddNew();
			addedOrigin.JA_RL_NKPortOfLoading = GetPortInCountryExcluding(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, HomePort, AlternateHomePort);
			AssertEquals("Added an Origin, Current Country Origins should have added one record", initialOriginCount + 1, currentCountry.Origins.Count);
			AssertEquals("Origins should be in sync", currentCountry.Origins.Count, principal.Origins.Count);
			VoyageOrigin homePortOrigin = fSeaVoyage.Origins.GetOriginFromLoading(HomePort);
			homePortOrigin.JA_RL_NKPortOfLoading = OverseasPort4;
			AssertEquals("Moved one Origin to overseas Current Country Origins should have removed one record", initialOriginCount, currentCountry.Origins.Count);
			VoyageOrigin altHomePortOrigin = fSeaVoyage.Origins.GetOriginFromLoading(AlternateHomePort);
			altHomePortOrigin.Delete();
			AssertEquals("Deleted one Origin from the current country, Origins should have removed one record", initialOriginCount - 1, currentCountry.Origins.Count);
			VoyageOrigin newOrigin = Factory.New<VoyageOrigin>();
			newOrigin.JA_RL_NKPortOfLoading = AlternateHomePort;
			fSeaVoyage.Origins.Add(newOrigin);
			AssertEquals("Added an origin like a grid would have added, Origins should have added one record", initialOriginCount, currentCountry.Origins.Count);
			VoyageOrigin usedToBeOverseasOrigin = fSeaVoyage.Origins.GetOriginFromLoading(OverseasPort);
			usedToBeOverseasOrigin.JA_RL_NKPortOfLoading = HomePort;
			AssertEquals("Changed and existing overseas origin to be a local origin, Origins should have added one record", initialOriginCount + 1, currentCountry.Origins.Count);
		}

		public void TestObjectWrapping()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			AssertSame(voyage.Origins[0], principal.Origins[0].Origin);
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			AssertSame(voyage.Origins[1], principal.Origins[1].Origin);
		}

		public void TestAllowNew()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AssertEquals("AllowNew", false, schedule.GenericPrincipal.Origins.AllowNew);
		}
	}
}
