using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgCusAccountGUIProviderTest : TestCaseWithFactory
	{
		public void TestGUIProviderEritrea()
		{
			AssertType<OrgCusAccountGUIProvider>(OrgCusAccountGUIProvider.GetByCountryCode(Core.Constants.CountryCodes.Eritrea));
		}
	}
}
