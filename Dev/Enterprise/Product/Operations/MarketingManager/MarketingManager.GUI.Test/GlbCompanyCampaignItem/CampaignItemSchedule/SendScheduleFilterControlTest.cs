using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class SendScheduleFilterControlTest : TestCaseWithFactory
	{
		public void TestGetCampaignsDoesNotThrow()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var control = new SendScheduleFilterControlForTest(new GlbCompanyCampaignItemScheduleItemsCollection(campaign), new SendScheduleFilterBusinessObject(campaign)))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "ABC";
				var contact1 = org.Contacts.AddNew();
				contact1.OC_Email = "contact@test.com";
				contact1.OC_ContactName = "Contact 1";
				var contact2 = org.Contacts.AddNew();
				contact2.OC_Email = "contact2@test.com";
				contact2.OC_ContactName = "Contact 2";
				Factory.Save();

				ScheduleCampaignItems schedule = new ScheduleCampaignItems(campaign);
				schedule.UtcOffset = new ZShort(600);
				schedule.TimeZone = "Australia/Brisbane";
				schedule.StandardTimeZoneCode = "EST";
				schedule.ContactsCount = 20;
				schedule.ScheduleSendTimeUTC = ZDateTime.UtcNow;
				schedule.Status = "QUE";

				var item1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
				item1.G8_RecipientID = contact1.PK;
				item1.G8_RecipientTableCode = "OC";

				var item2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
				item2.G8_RecipientID = contact2.PK;
				item2.G8_RecipientTableCode = "OC";

				schedule.Campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { item1, item2 });

				contact2.OC_IsActive = false;
				Factory.Save();

				AssertNoExceptionThrown(delegate
				{ control.GetCampaignItems_Exposed(schedule); });
			}
		}

		class SendScheduleFilterControlForTest : SendScheduleFilterControl
		{
			public SendScheduleFilterControlForTest(IBusinessObjectCollection collection, SendScheduleFilterBusinessObject filterBusinessObject) : base(collection, filterBusinessObject)
			{
			}

			public IEnumerable<GlbCompanyCampaignItem> GetCampaignItems_Exposed(ScheduleCampaignItems scheduledCampaignItems)
			{
				return base.GetCampaignItems(scheduledCampaignItems);
			}
		}
	}
}
