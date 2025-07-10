using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWGlbCompanyWrapperProvider))]
	sealed class TWGlbCompanyWrapperProviderTest : MasterFiles.GUI.Testing.GlbCompanyWrapperProviderTest<TWGlbCompanyWrapperProvider>
	{
		public void TestGetNewCompanyCredentialsLayout()
		{
			var provider = new TWGlbCompanyWrapperProvider();
			AssertType<CompanyCredentialsLayout>(provider.GetNewCompanyCredentialsLayout());
		}
	}
}
