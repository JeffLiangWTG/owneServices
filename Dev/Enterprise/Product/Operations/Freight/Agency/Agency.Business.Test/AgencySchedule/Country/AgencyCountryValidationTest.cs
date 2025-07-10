using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyCountryValidationTest : BaseAgencyTest
	{
		public void TestAllocationMethodValidation()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			AgencyCountry country = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			country.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AssertNoNotifications(country.J0_AllocationMethodInfo);
			country.J0_AllocationMethod = "!NV";
			AssertHasErrors(country.J0_AllocationMethodInfo);
			country.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			AssertNoNotifications(country.J0_AllocationMethodInfo);
			country.J0_AllocationMethod = "";
			AssertHasErrors(country.J0_AllocationMethodInfo);
			country.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(10, 10, 10, 10, 10, 10));
			country.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AssertHasError("Should have Error.", country.J0_AllocationMethodInfo, "You cannot change allocations to \"Not Set\" because there is cargo booked against this sailing. Either revert to the original allocation method or choose \"Ignore\" to disregard and ignore allocations for this sailing.");
			country.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			AssertNoNotifications("Should NOT have any errors or warnings.", country.J0_AllocationMethodInfo);
		}

		public void TestJ0_AllocationsByPrincipal()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.J0_AllocationsByPrincipal = false;
			AssertNoNotifications("Should NOT have any errors or warnings.", schedule.J0_AllocationsByPrincipalInfo);
			schedule.Principals.Add(principal1);
			AssertHasError("Should have Error.", schedule.J0_AllocationsByPrincipalInfo, "There are principals in the principals list but per-principal allocations has not been enabled.");
			schedule.J0_AllocationsByPrincipal = true;
			AssertHasWarning("Should have Warning.", schedule.J0_AllocationsByPrincipalInfo, "You have enabled per-principal allocations but only added one principal.");
			schedule.Principals.Add(principal2);
			AssertNoNotifications("Should NOT have any errors or warnings.", schedule.J0_AllocationsByPrincipalInfo);
			schedule.Principals.RemoveAll();
			AssertHasError("Should have Error.", schedule.J0_AllocationsByPrincipalInfo, "You have enabled per-principal allocations but have not added any principals to the principals list.");
			schedule.J0_AllocationsByPrincipal = false;
			AssertNoNotifications("Should NOT have any errors or warnings.", schedule.J0_AllocationsByPrincipalInfo);
		}
	}
}
