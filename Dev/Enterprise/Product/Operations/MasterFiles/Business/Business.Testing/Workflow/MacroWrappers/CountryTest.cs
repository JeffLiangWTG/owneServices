using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class CountryTest : TestCaseWithFactory
	{
		public void TestCountry()
		{
			var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "PL");

			var country = new Country(refCountry);

			CombineAssertions(() =>
			{
				AssertEquals("Code", "PL", country.Code);
				AssertEquals("Name", "Poland", country.Name);
				AssertEquals("IsInEU", true, country.IsInEU);
				AssertEquals("IsInEFTA", false, country.IsInEFTA);
			});
		}

		public void TestNullCountry()
		{
			var country = new Country(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", country.Code);
				AssertNullOrEmpty("Name", country.Name);
				AssertEquals("IsInEU", false, country.IsInEU);
				AssertEquals("IsInEFTA", false, country.IsInEFTA);
			});
		}
	}
}
