using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefPostCodeCollection))]
	sealed class RefPostCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefPostCodeCollection>
	{
		public void TestICityTownPostcodeUserInteraction()
		{
			var collection = new RefPostCodeCollection(Factory, new ZQuery(), "US", "", "");
			Assert(collection is ICityTownPostcodeUserInteraction);
		}

		public void TestFindBoxListProviderType()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			var collection = new RefPostCodeCollectionForTest(Factory, "US");
			AssertType<RefPostcodeListProvider>(collection.Provider);

			collection = new RefPostCodeCollectionForTest(Factory, "");
			AssertType<FindBoxListProvider>(collection.Provider);

			Env.Registry.EnableAddressValidationWebService = false;
			collection = new RefPostCodeCollectionForTest(Factory, "US");
			AssertType<RefPostcodeListProvider>(collection.Provider);
		}

		public void Test_When_CountrySetOnCityTown_Then_DefaultCountryOnNewPostCodeMatches()
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = "AU";
			var postCode = cityTown.PostCodes.AddNew();
			AssertEquals("Default country for the new post code should match the parent's.", "AU", postCode.RK_RN_NKCountry);
		}

		public void Test_When_CountryIsNotSetOnCityTown_Then_NoCountryDefaultOnNewPostCode()
		{
			var collection = new RefPostCodeCollection(Factory);
			var postCode = collection.AddNew();
			AssertEquals("Default country for the new post code should be an empty string if there is no parent city/town.", ZString.Empty, postCode.RK_RN_NKCountry);
		}
	}
}
