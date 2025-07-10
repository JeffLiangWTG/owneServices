using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyCountryCollectionTest : BaseAgencyTest
	{
		[UseGlobalAllocations(true)]
		public void Refresh_Global()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			AgencyCountryCollection collection = new AgencyCountryCollection(voyage);
			AssertEquals("use global allocations", true, collection.UseGlobalAllocations);
			AssertLoadedCountries("not loaded", collection);
			collection.Refresh();
			AssertLoadedCountries("loaded", collection, "AU");
			origin1.JA_RL_NKPortOfLoading = "SGSIN";
			AssertLoadedCountries("changed load port - not loaded yet", collection, "AU");
			collection.Refresh();
			AssertLoadedCountries("changed load port", collection, "SG");
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			AssertLoadedCountries("added origin - not loaded yet", collection, "SG");
			collection.Refresh();
			AssertLoadedCountries("added origin", collection, "AU", "SG");
			origin1.Delete();
			AssertLoadedCountries("deleted load origin - not loaded yet", collection, "SG");
			collection.Refresh();
			AssertLoadedCountries("deleted load origin", collection, "SG");
		}

		[UseGlobalAllocations(false)]
		public void Refresh_Local()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			AgencyCountryCollection collection = new AgencyCountryCollection(voyage);
			AssertEquals("use local allocations", false, collection.UseGlobalAllocations);
			AssertLoadedCountries("not loaded", collection);
			collection.Refresh();
			AssertLoadedCountries("loaded", collection, "AU");
		}

		public void TestMutex()
		{
			AgencyCountryCollection agencySchedule = new AgencyCountryCollection(Factory.New<JobVoyage>());
			AssertNotNull("Failed to create Mutex", agencySchedule.Mutex);
		}

		public void TestPerformPreSave()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			AgencyCountry countryWrapper = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AgencyPrincipal principalWrapper = countryWrapper.GenericPrincipal;
			AgencyCountryCollection collection = new AgencyCountryCollection(voyage);
			collection.Add(countryWrapper);
			NewBulkShipment(sailing, null, false, true, 10, 0);
			Factory.Save();
			AssertEquals("Usage should not have been loaded yet", 0m, principalWrapper.UsageProvider.GetSailingUsage(sailing).Tonnes);
			collection.PerformPreSave();
			try
			{
				AssertEquals("Should be holding the lock", true, collection.Mutex.HasLock);
				AssertEquals("Should have loaded the usages from the DB.", 10m, principalWrapper.UsageProvider.GetSailingUsage(sailing).Tonnes);
			}
			finally
			{
				collection.PerformPostSave();
			}
		}

		public void TestPerformPostSave()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			NewBulkShipment(sailing, null, false, true, 10, 0);
			Factory.Save();
			AgencyCountryCollection collection = new AgencyCountryCollection(voyage);
			collection.PerformPreSave();
			Factory.Save();
			collection.PerformPostSave();
			AssertEquals("Should no longer be holding the lock", false, collection.Mutex.HasLock);
		}

		#region Implementation
		void AssertLoadedCountries(string message, AgencyCountryCollection collection, params string[] expectedCountryCodes)
		{
			AssertContainsExactElementsInAnyOrder(message, expectedCountryCodes, Array.ConvertAll(collection.ToArray<AgencyCountry>(), (c) => c.CountryCode));
		}
		#endregion
	}
}
