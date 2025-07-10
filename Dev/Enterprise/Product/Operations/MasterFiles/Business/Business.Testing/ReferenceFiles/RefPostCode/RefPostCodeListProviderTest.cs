using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPostCodeListProviderTest : TestCaseWithFactory
	{
		public void TestPrimaryKeyFromCode()
		{
			var collection = new RefPostCodeCollection(Factory, new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"), "AU", "", "");
			var postcode = collection.First();
			AssertNotNull("Precondition", postcode);

			var postcodeInZZ = collection.AddNew();
			postcodeInZZ.RK_CityTownPostCode = "2000";
			postcodeInZZ.RK_RN_NKCountry = "ZZ";

			var provider = new RefPostcodeListProvider(collection, postcode.RK_RN_NKCountry, "", "");
			var resultPK = provider.PrimaryKeyFromCode("2000");
			AssertEquals(postcode.PK, resultPK);
			AssertNotEquals(postcodeInZZ.PK, resultPK);

			AssertEquals(false, provider.PrimaryKeyFromCode("0000").IsValid);
			AssertEquals(false, provider.PrimaryKeyFromCode(" ").IsValid);
		}
	}
}
