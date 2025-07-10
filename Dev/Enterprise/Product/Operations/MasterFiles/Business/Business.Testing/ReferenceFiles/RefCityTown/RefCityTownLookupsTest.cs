using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCityTownLookupsTest : BusinessObjectLookupsTestCase
	{
		public void Test_When_CountrySetOnParentCityTown_Then_AdditionalFilterIsAdded()
		{
			var cityTown = Factory.New<RefCityTown>();

			Assert("Precondition - AdditionFilter should be set to return no states", cityTown.Lookups.States.AdditionalFilter.IsNoResultQuery);

			cityTown.R9_RN_NKCountry = "US";

			Assert(!cityTown.Lookups.States.AdditionalFilter.IsEmpty);
			Assert(!cityTown.Lookups.States.AdditionalFilter.IsNoResultQuery);
			AssertEquals(cityTown.Lookups.States.AdditionalFilter.LiteralTextADO, "RW_RN_NKCountryCode = 'US'");
		}

		public void Test_When_CountrySetOnCityTown_Then_PostCodeListIsFilteredByThatCountry()
		{
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_RN_NKCountry = "PN";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_RN_NKCountry = "NZ";
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = "PN";

			AssertCollectionContains("PostCodeCollection should contain postCode1", postCode1, cityTown.Lookups.PostCodes);
			AssertCollectionNotContains("PostCodeCollection should NOT contain postCode2", postCode2, cityTown.Lookups.PostCodes);
		}

		public void Test_When_CountrySetOnCityTown_Then_CollectionContainsPostCodes()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = "PN";
			var cityTown1 = Factory.New<RefCityTown>();
			cityTown1.R9_RN_NKCountry = "PN";
			var cityTown2 = Factory.New<RefCityTown>();

			AssertNotEquals("There should be post codes in PostCodeCollection if the city/town's country is defined.", 0, cityTown1.Lookups.PostCodes.Count);
			AssertEquals("There should be NO post codes in PostCodeCollectionn if city/town's country is null.", 0, cityTown2.Lookups.PostCodes.Count);
		}

		public void Test_When_CountrySetOnCityTown_Then_DefaultFilterShouldBeApplied()
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = "AU";

			AssertEquals("Default filter should be applied correctly.", "AU", cityTown.Lookups.PostCodes.FilterBusinessObjectDefaults["Country:Property"].Value);
		}
	}
}
