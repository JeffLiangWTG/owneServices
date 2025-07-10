using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class EntrySGCPlaceInfoTest : TestCaseWithFactory
	{
		public void TestSGCPlace()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			EntrySGCPlaceInfo validPlace = new EntrySGCPlaceInfo("AT1B");
			AssertEquals("SGCPlace Code", "AT1B", validPlace.Code);
			AssertEquals("SGCPlace Type", "AT1B", validPlace.Type);
			AssertEquals("SGCPlace Name and Address", "AIRPORT TERMINAL 1 BOND", validPlace.NameAndAddress);
			AssertEquals("SGCPlace SCSYO", false, validPlace.AddressRequired);
		}

		public void TestSGCPlaceWhenEmpty()
		{
			EntrySGCPlaceInfo validPlace = new EntrySGCPlaceInfo("");
			AssertEquals("SGCPlace Type", "", validPlace.Type);
			AssertEquals("SGCPlace Name and Address", "", validPlace.NameAndAddress);
			AssertEquals("SGCPlace SCSYO", false, validPlace.AddressRequired);
		}

		public void TestUserCreatedSGCPlace()
		{
			var testLocation = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "SGPLACE", "O", "Other User Location 1792 Raffles Ave Central");
			Factory.Save();
			EntrySGCPlaceInfo validPlace = new EntrySGCPlaceInfo("SGPLACE");
			AssertEquals("SGCPlace Code", "SGPLACE", validPlace.Code);
			AssertEquals("SGCPlace Type", "O", validPlace.Type);
			AssertEquals("SGCPlace Name and Address", "Other User Location 1792 Raffles Ave Central", validPlace.NameAndAddress);
			AssertEquals("SGCPlace SCSYO", true, validPlace.AddressRequired);
		}

		public void TestSGCPlaceWithNewCodeNotInSystemYet()
		{
			EntrySGCPlaceInfo validNewPlace = new EntrySGCPlaceInfo("NEWCODE");
			AssertEquals("SGCPlace Code", "NEWCODE", validNewPlace.Code);
		}
	}
}
