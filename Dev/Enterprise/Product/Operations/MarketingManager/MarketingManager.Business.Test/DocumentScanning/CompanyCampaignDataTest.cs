using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	public class CompanyCampaignDataTest : TestCaseWithFactory
	{
		public void TestAssemblyDataProperties()
		{
			var campaignData = new CompanyCampaignData();
			CombineAssertions(() =>
			{
				AssertEquals("BusinessObjectType", typeof(GlbCompanyCampaign), campaignData.BusinessObjectType);
				AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.ClientSupplierRelationship, campaignData.ReferenceType);
				AssertEquals("HumanReadableName", "Sales Campaign", campaignData.HumanReadableName);
			});
		}

		public void TestGetBusinessObjectCollection()
		{
			var campaignData = new CompanyCampaignData();
			var collection = campaignData.GetBusinessObjectCollection(Factory);
			AssertType<GlbCompanyCampaignCollection>(collection);
		}
	}
}
