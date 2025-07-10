using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPostCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void Test_When_CountrySetOnPostCode_Then_CityTownListIsFilteredByThatCountry()
		{
			var cityTown1 = Factory.New<RefCityTown>();
			cityTown1.R9_RN_NKCountry = "PN";
			var cityTown2 = Factory.New<RefCityTown>();
			cityTown2.R9_RN_NKCountry = "NZ";
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = "PN";

			AssertCollectionContains("CityTownCollection should contain CityTown1", cityTown1, postCode.Lookups.CityTowns);
			AssertCollectionNotContains("CityTownCollection should NOT contain CityTown2", cityTown2, postCode.Lookups.CityTowns);
		}

		public void Test_When_CountrySetOnPostCode_Then_CollectionContainsCityTowns()
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = "PN";
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_RN_NKCountry = "PN";
			var postCode2 = Factory.New<RefPostCode>();

			AssertNotEquals("There should be city/towns in CityTownCollection if the post code's country is defined.", 0, postCode1.Lookups.CityTowns.Count);
			AssertEquals("There should be NO city/towns in CityTownCollection if post code's country is null.", 0, postCode2.Lookups.CityTowns.Count);
		}

		public void Test_When_CountrySetOnPostCode_Then_DefaultFilterShouldBeApplied()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = "AU";

			AssertEquals("Default filter should be applied correctly.", "AU", postCode.Lookups.CityTowns.FilterBusinessObjectDefaults["CountryState:Property1"].Value);
		}
	}
}
