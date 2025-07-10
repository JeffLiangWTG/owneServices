using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSendSettingsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestScheduleTypes()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			var lookups = new GlbCompanyCampaignSendSettingsLookups(settings);
			AssertEquals(4, lookups.ScheduleTypes.Count);

			AssertEquals("Batch Schedule", lookups.ScheduleTypes.GetDescriptionFromCode(GlbCompanyCampaignSendSettingsLookups.Codes.BAT));
			AssertEquals("Immediate Delivery", lookups.ScheduleTypes.GetDescriptionFromCode(GlbCompanyCampaignSendSettingsLookups.Codes.IMM));
			AssertEquals("Delayed Delivery", lookups.ScheduleTypes.GetDescriptionFromCode(GlbCompanyCampaignSendSettingsLookups.Codes.DEL));
			AssertEquals("Fixed Date Delivery", lookups.ScheduleTypes.GetDescriptionFromCode(GlbCompanyCampaignSendSettingsLookups.Codes.FIX));
		}
	}
}
