using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class ABLEntryNumProviderTest : TestCaseWithFactory
	{
		public void TestGetNewValidation()
		{
			var entryNumProvider = ASYCUDA.Business.ABLEntryNumProvider.GetByCountryCode(Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, entryNumProvider.CountryCode);
			AssertType<ABLEntryNumValidation>(entryNumProvider.GetNewValidation(Factory.New<ABLEntryNum>()));
		}
	}
}
