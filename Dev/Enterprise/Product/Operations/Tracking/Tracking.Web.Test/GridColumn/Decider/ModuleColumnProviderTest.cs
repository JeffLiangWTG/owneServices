using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ModuleColumnProviderTest : TestCaseWithFactory
	{
		public void TestGetColumnProviderForCountry_Base()
		{
			AssertEquals(DeclarationColumnProviderTypeDecider.GetColumnProviderForCountry(Constants.CountryCodes.Australia).GetType(), typeof(BaseDeclarationModuleColumnProvider));
		}

		public void TestGetColumnProviderForCountry_US()
		{
			AssertEquals(DeclarationColumnProviderTypeDecider.GetColumnProviderForCountry(Constants.CountryCodes.UnitedStates).GetType(), typeof(USDeclarationModuleColumnProvider));
		}

		public void TestGetColumnProviderForCountry_CA()
		{
			AssertEquals(DeclarationColumnProviderTypeDecider.GetColumnProviderForCountry(Constants.CountryCodes.Canada).GetType(), typeof(CADeclarationModuleColumnProvider));
		}

		public void TestGetColumnProviderForCountry_NZ()
		{
			AssertEquals(DeclarationColumnProviderTypeDecider.GetColumnProviderForCountry(Constants.CountryCodes.NewZealand).GetType(), typeof(NZDeclarationModuleColumnProvider));
		}
	}
}
