using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSubTypeDependencyConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList()
		{
			configuration.Country = "TW";
			AssertEquals("TaxRegistrationTypeList.Count", 17, Lookups.SubTypeList.Count);
			configuration.Country = "PE";
			AssertEquals("TaxRegistrationTypeList.Count", 15, Lookups.SubTypeList.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ComplianceSubTypeDependencyConfiguration();
			Lookups = new ComplianceSubTypeDependencyConfigurationLookups(configuration);
		}
		ComplianceSubTypeDependencyConfigurationLookups Lookups;
		ComplianceSubTypeDependencyConfiguration configuration;

		#endregion
	}
}
