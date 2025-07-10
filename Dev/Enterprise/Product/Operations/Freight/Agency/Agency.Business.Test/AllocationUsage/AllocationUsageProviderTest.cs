using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AllocationUsageProviderTest : BaseAgencyTest
	{
		public void TestGetSailingUsage()
		{
			GenericGetSailingUsageTest(false);
		}

		public void TestGetSailingUsage_ByPrincipal()
		{
			GenericGetSailingUsageTest(true);
		}

		public void GenericGetSailingUsageTest(bool byPrincipal)
		{
			SetUpShipments();
			AllocationUsageProvider provider = new AllocationUsageProvider();
			AssertEquals("Load() not called yet, should return an empty usage", true, provider.GetSailingUsage(sailing1).IsEmpty);
			if (byPrincipal)
			{
				provider.Load(voyage, principal1.PK);
				AssertAllocationUsage("Sailing1 ", provider.GetSailingUsage(sailing1), 0m, 0m, 0m, 0m, 0m, 0m, 0);
			}
			else
			{
				provider.Load(voyage, ZGuid.Empty);
				AssertAllocationUsage("Sailing1 ", provider.GetSailingUsage(sailing1), 9.8m, 0m, 0m, 4m, 2m, 2m, 1);
				AssertSame("Should return the same usage for the same sailing", provider.GetSailingUsage(sailing1), provider.GetSailingUsage(sailing1));
			}

			AssertEquals("Should return an empty usage for sailings that were not loaded", true, provider.GetSailingUsage(voyage.Sailings.AddNew()).IsEmpty);
		}

		public void TestGetOriginUsage()
		{
			GenericGetOriginUsageTest(false);
		}

		public void TestGetOriginUsage_ByPrincipal()
		{
			GenericGetOriginUsageTest(true);
		}

		public void GenericGetOriginUsageTest(bool byPrincipal)
		{
			SetUpShipments();
			AllocationUsageProvider provider = new AllocationUsageProvider();
			AssertEquals("Load() not called yet, should return an empty usage", true, provider.GetOriginUsage(origin1).IsEmpty);
			if (byPrincipal)
			{
				provider.Load(voyage, principal1.PK);
				AssertAllocationUsage("Origin1 ", provider.GetOriginUsage(origin1), 17.2m, 0m, 0m, 7m, 3m, 4m, 2);
			}
			else
			{
				provider.Load(voyage, ZGuid.Empty);
				AssertAllocationUsage("Origin1 ", provider.GetOriginUsage(origin1), 27m, 0m, 0m, 11m, 5m, 6m, 3);
			}

			AssertSame("Should return the same usage for the same origin", provider.GetOriginUsage(origin1), provider.GetOriginUsage(origin1));
			AssertEquals("Should return an empty usage for origins that were not loaded", true, provider.GetOriginUsage(voyage.Origins.AddNew()).IsEmpty);
		}

		public void TestGetCountryUsage()
		{
			GenericGetCountryUsageTest(false);
		}

		public void TestGetCountryUsage_ByPrincipal()
		{
			GenericGetCountryUsageTest(true);
		}

		public void GenericGetCountryUsageTest(bool byPrincipal)
		{
			SetUpShipments();
			AllocationUsageProvider provider = new AllocationUsageProvider();
			AssertEquals("Load() not called yet, should return an empty usage", true, provider.GetCountryUsage(origin1.VoyageCountry).IsEmpty);
			if (byPrincipal)
			{
				provider.Load(voyage, principal1.PK);
				AssertAllocationUsage("Origin1 ", provider.GetCountryUsage(origin1.VoyageCountry), 17.2m, 0m, 0m, 7m, 3m, 4m, 2);
			}
			else
			{
				provider.Load(voyage, ZGuid.Empty);
				AssertAllocationUsage("Origin1 ", provider.GetCountryUsage(origin1.VoyageCountry), 39.6m, 0m, 0m, 16m, 5m, 12m, 6);
			}

			AssertEquals("Should return an empty usage for countries that were not loaded", true, provider.GetCountryUsage(voyage.Countries.GetCountry("US", true)).IsEmpty);
		}

		#region Implementation
		JobVoyage voyage;
		OrgHeader principal1;
		OrgHeader principal2;
		VoyageOrigin origin0;
		VoyageOrigin origin1;
		VoyageOrigin origin2;
		VoyageDestination destination1;
		VoyageDestination destination2;
		JobSailing sailing0;
		JobSailing sailing1;
		JobSailing sailing2;
		JobSailing sailing3;
		public void SetUpShipments()
		{
			voyage = Factory.New<JobVoyage>();
			origin0 = voyage.Origins.AddNew();
			origin0.JA_RL_NKPortOfLoading = "NZAKL";
			origin0.JA_E_DEP = ZDateTime.Now;
			origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Now.AddDays(5);
			origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_E_DEP = ZDateTime.Now.AddDays(11);
			destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(9);
			destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(15);
			sailing0 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin0.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin2.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			principal1 = NewPrincipal();
			principal2 = NewPrincipal();
			Factory.Save();
			NewFCLShipment(sailing0, principal1, false, true, 9, 9);
			NewFCLShipment(sailing1, principal2, false, true, 2, 1);
			NewFCLShipment(sailing2, principal1, false, true, 3, 2);
			NewFCLShipment(sailing3, principal2, false, true, 1, 4);
			Factory.Save();
		}
		#endregion
	}
}
