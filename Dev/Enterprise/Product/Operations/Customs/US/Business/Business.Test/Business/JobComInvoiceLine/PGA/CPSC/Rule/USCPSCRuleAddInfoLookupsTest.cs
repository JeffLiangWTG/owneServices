using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCPSCRuleAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizations()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.Organizations);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var ruleDetail = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().CPSCHeaders.AddNew().RuleAndLabs.AddNew();
			var addInfo = new CPSCRuleAddInfo(ruleDetail.B7_AddInfoDataInfo);
			lookups = new USCPSCRuleAddInfoLookups(addInfo);
		}
		USCPSCRuleAddInfoLookups lookups;
	}
}
