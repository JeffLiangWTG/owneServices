using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	public class CompanyCampaignItemDataTest : TestCaseWithFactory
	{
		public void TestAssemblyDataProperties()
		{
			var campaignItemData = new CompanyCampaignItemData();
			CombineAssertions(() =>
			{
				AssertEquals("BusinessObjectType", typeof(GlbCompanyCampaignItem), campaignItemData.BusinessObjectType);
				AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.ClientSupplierRelationship, campaignItemData.ReferenceType);
				AssertEquals("HumanReadableName", "Sent Campaign", campaignItemData.HumanReadableName);
			});
		}

		public void TestGetBusinessObjectCollection()
		{
			var campaignItemData = new CompanyCampaignItemData();
			var collection = campaignItemData.GetBusinessObjectCollection(Factory);
			AssertNull(collection);
		}
	}
}
