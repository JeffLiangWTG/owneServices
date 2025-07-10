using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class SADCCountriesTest : TestCaseWithFactory
	{
		public void TestNumberOfCountries()
		{
			AssertEquals(10, new SADCCountries().Count);
		}
	}
}
