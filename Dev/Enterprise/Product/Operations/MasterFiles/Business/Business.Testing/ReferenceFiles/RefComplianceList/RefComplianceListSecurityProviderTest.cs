using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefComplianceListSecurityProviderTest : TestCaseWithFactory
	{
		public void TestHasEditConfigurationSecurity()
		{
			var testSecurityProvider = new RefComplianceListSecurityProvider();

			Env.Security.RefComplianceListEdit.IsAllowed = false;
			AssertEquals("HasEditConfigurationSecurity should NOT be allowed", false, testSecurityProvider.HasEditConfigurationSecurity);

			Env.Security.RefComplianceListEdit.IsAllowed = true;
			AssertEquals("HasEditConfigurationSecurity should be allowed", true, testSecurityProvider.HasEditConfigurationSecurity);
		}
	}
}
