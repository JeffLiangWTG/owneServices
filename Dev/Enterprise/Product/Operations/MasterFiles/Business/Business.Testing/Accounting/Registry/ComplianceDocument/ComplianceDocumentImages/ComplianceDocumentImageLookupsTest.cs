using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceDocumentImageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComplianceSubTypeList()
		{
			configuration.Country = "TW";
			AssertEquals("TaxRegistrationTypeList.Count", 17, Lookups.ComplianceSubTypeList.Count);
			configuration.Country = "PE";
			AssertEquals("TaxRegistrationTypeList.Count", 15, Lookups.ComplianceSubTypeList.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ComplianceDocumentImage();
			Lookups = new ComplianceDocumentImageLookups(configuration);
		}
		ComplianceDocumentImageLookups Lookups;
		ComplianceDocumentImage configuration;

		#endregion
	}
}
