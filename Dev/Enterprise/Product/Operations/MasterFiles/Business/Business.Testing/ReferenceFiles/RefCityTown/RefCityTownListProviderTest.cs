using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCityTownListProviderTest : TestCaseWithFactory
	{
		public void TestPrimaryKeyFromCode()
		{
			var collection = new RefCityTownCollection(Factory, new ZQuery(RefCityTownSchema.R9_InternationalName, "Kingsford"), "AU", "");
			var city = collection.First(c => !string.IsNullOrEmpty(c.R9_InternationalName));
			AssertNotNull("Precondition", city);

			var cityInZZ = collection.AddNew();
			cityInZZ.R9_InternationalName = city.R9_InternationalName;
			cityInZZ.R9_RN_NKCountry = "ZZ";

			var provider = new RefCityTownListProvider(collection, city.R9_RN_NKCountry, "");
			var resultPK = provider.PrimaryKeyFromCode(city.R9_InternationalName);
			AssertEquals(city.PK, resultPK);
			AssertNotEquals(cityInZZ.PK, resultPK);

			AssertEquals(false, provider.PrimaryKeyFromCode("NOTACITY$$$").IsValid);
			AssertEquals(false, provider.PrimaryKeyFromCode(" ").IsValid);
		}
	}
}
