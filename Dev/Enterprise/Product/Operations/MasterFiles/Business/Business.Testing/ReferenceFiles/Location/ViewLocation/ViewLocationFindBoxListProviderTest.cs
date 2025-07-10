using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ViewLocationFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestGetBizObjFromCode()
		{
			var state1 = Factory.New<RefCountryStates>();
			state1.RW_Code = "D=";
			state1.RW_RN_NKCountryCode = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN").RN_Code;

			var state2 = Factory.New<RefCountryStates>();
			state2.RW_Code = "D=";
			state2.RW_RN_NKCountryCode = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").RN_Code;

			Factory.Save();

			var collection = new ViewLocationCollection(Factory);
			var listProvider = new ViewLocationFindBoxListProvider(collection);

			var location = listProvider.GetBusinessObjectFromCode("D=");
			AssertEquals(state2.PK, location.PK);

			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = "D=";
			Factory.Save();

			collection = new ViewLocationCollection(Factory, ViewLocationType.Country);
			listProvider = new ViewLocationFindBoxListProvider(collection);

			location = listProvider.GetBusinessObjectFromCode("D=");
			AssertEquals(country1.PK, location.PK);
		}

		public void TestNearestMatch()
		{
			var unlocoXXX = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoXXX.RL_Code = "11XXX";
			unlocoXXX.RL_IATA = "XXX";

			var unlocoYYY = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoYYY.RL_Code = "11YYY";
			unlocoYYY.RL_IATA = "YYY";

			Factory.Save();

			IFindBoxListProvider collection = new ViewLocationCollection(Factory);
			AssertEquals("11XXX", collection.NearestMatch("XXX", false, -1).Item1);
			AssertEquals("XXX", collection.NearestMatch("XXX", true, -1).Item1);

			AssertEquals("11X", collection.NearestMatch("11X", false, -1).Item1);
			AssertEquals("11XXX", collection.NearestMatch("11X", true, -1).Item1);
		}

		public void TestNearestMatch_WithLocationTypesSpecified()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "11";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "11XXX";

			Factory.Save();

			IFindBoxListProvider collection = new ViewLocationCollection(Factory, ViewLocationType.UNLOCO);
			AssertEquals("Should not match against country because it is not allowed in collection", "11XXX", collection.NearestMatch("11", true, -1).Item1);
		}
	}
}
