using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCityTownCollection))]
	sealed class RefCityTownCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCityTownCollection>
	{
		public void TestICityTownPostcodeUserInteraction()
		{
			var collection = new RefCityTownCollection(Factory, new ZQuery(), "US", "");
			Assert(collection is ICityTownPostcodeUserInteraction);
		}

		public void TestFindBoxListProviderType()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			var collection = new RefCityTownCollectionForTest(Factory, "US");
			AssertType<RefCityTownListProvider>(collection.Provider);

			collection = new RefCityTownCollectionForTest(Factory, "");
			AssertType<FindBoxListProvider>(collection.Provider);

			Env.Registry.EnableAddressValidationWebService = false;
			collection = new RefCityTownCollectionForTest(Factory, "US");
			AssertType<RefCityTownListProvider>(collection.Provider);
		}

		public void Test_When_CountrySetOnPostCode_Then_DefaultCountryOnNewCityTownMatches()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = "AU";
			var cityTown = postCode.CityTowns.AddNew();
			AssertEquals("Default country for the new city/town should match the parent's.", "AU", cityTown.R9_RN_NKCountry);
		}

		public void Test_When_CountryIsNotSetOnPostCode_Then_NoCountryDefaultOnNewCityTown()
		{
			var collection = new RefCityTownCollection(Factory);
			var cityTown = collection.AddNew();
			AssertEquals("Default country for the new city/town should be an empty string if there is no parent post code.", ZString.Empty, cityTown.R9_RN_NKCountry);
		}
	}
}
