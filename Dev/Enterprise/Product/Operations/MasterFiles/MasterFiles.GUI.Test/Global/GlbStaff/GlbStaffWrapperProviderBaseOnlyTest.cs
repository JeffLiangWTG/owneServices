using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffWrapperProviderForTesting))]
	sealed class GlbStaffWrapperProviderBaseOnlyTest : GlbStaffWrapperProviderTest<GlbStaffWrapperProviderForTesting>
	{
		public void TestGetProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var sgProvider = GlbStaffWrapperProvider.GetProvider();
				AssertEquals(true, typeof(Integration.Customs.SG.ISGGlbStaffWrapperProvider).IsAssignableFrom(sgProvider.GetType()));
				AssertEquals(sgProvider.GetType(), GlbStaffWrapperProvider.GetProvider(Core.Constants.CountryCodes.Singapore).GetType());
				AssertNull(GlbStaffWrapperProvider.GetProvider("!@"));
				AssertEquals(true, typeof(Integration.CustomsIntegration.IT.IGlbStaffWrapperProvider).IsAssignableFrom(GlbStaffWrapperProvider.GetProvider(Core.Constants.CountryCodes.Italy).GetType()));
			}
		}

		public void TestShowPreSaveDialogs()
		{
			var provider = new GlbStaffWrapperProviderForTesting();
			AssertEquals("Default value is Yes.", ContinueWithSave.Yes, provider.ShowPreSaveDialogs(null));
		}
	}
}
