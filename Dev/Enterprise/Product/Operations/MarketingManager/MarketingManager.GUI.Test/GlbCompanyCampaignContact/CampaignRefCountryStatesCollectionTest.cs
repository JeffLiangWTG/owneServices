using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignRefCountryStatesCollection))]
	class CampaignRefCountryStatesCollectionTest : ActiveBusinessObjectCollectionTestCase<CampaignRefCountryStatesCollection>
	{
		public void TestCampaignRefCountryStatesCodeFromDescriptionEqualsFilter()
		{
			var campaignRefCountryStates = Factory.NewWithValidTestData<CampaignRefCountryStates>();
			campaignRefCountryStates.RW_Code = "TSE";
			campaignRefCountryStates.RW_Description = "Test State";
			Factory.Save();

			var findBoxListProvider = new CampaignRefCountryStatesCollection(Factory) as IFindBoxListProviderDescriptionEx;
			AssertEquals("TSE", findBoxListProvider.CodeFromDescription("Test State"));
		}

		public void TestCampaignRefCountryStatesCodeFromDescriptionStartsWithFilter()
		{
			var campaignRefCountryStates = Factory.NewWithValidTestData<CampaignRefCountryStates>();
			campaignRefCountryStates.RW_Code = "TSB";
			campaignRefCountryStates.RW_Description = "Test State B";

			var campaignRefCountryStates2 = Factory.NewWithValidTestData<CampaignRefCountryStates>();
			campaignRefCountryStates2.RW_Code = "TSA";
			campaignRefCountryStates2.RW_Description = "Test State A";

			var campaignRefCountryStates3 = Factory.NewWithValidTestData<CampaignRefCountryStates>();
			campaignRefCountryStates3.RW_Code = "TSC";
			campaignRefCountryStates3.RW_Description = "Test State C";
			Factory.Save();

			var findBoxListProvider = new CampaignRefCountryStatesCollection(Factory) as IFindBoxListProviderDescriptionEx;
			var descriptionMatch = findBoxListProvider.NearestDescriptionMatch("Test State", true);

			AssertEquals("Test State A", descriptionMatch);
			AssertEquals("TSA", findBoxListProvider.CodeFromDescription(descriptionMatch));
		}
	}
}
