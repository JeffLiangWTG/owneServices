using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ScheduleCampaignItems))]
	sealed class ScheduleCampaignItemsTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2015, 12, 20, 12, 12, 4)]
		[TestUtcOffset(10, 0, 0)]
		public void TestProperties()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ScheduleCampaignItems schedule = new ScheduleCampaignItems(campaign);
			schedule.UtcOffset = 600;
			schedule.TimeZone = "Australia/Brisbane";
			schedule.StandardTimeZoneCode = "EST";
			schedule.ContactsCount = 20;
			schedule.ScheduleSendTimeUTC = ZDateTime.UtcNow;
			schedule.Status = "QUE";

			AssertEquals((ZShort)600, schedule.UtcOffset);
			AssertEquals("UTC+10:00", schedule.UtcOffsetText);
			AssertEquals("Australia/Brisbane", schedule.TimeZone);
			AssertEquals("EST", schedule.StandardTimeZoneCode);
			AssertEquals(20, schedule.ContactsCount);
			AssertEquals(new ZDateTime(2015, 12, 20, 12, 12, 4), schedule.ScheduleSendTimeUTC);
			AssertEquals(new ZDateTime(2015, 12, 20, 22, 12, 4), schedule.ScheduleSendTimeLocal);
			AssertEquals("QUE", schedule.Status);

			schedule.ScheduleSendTimeLocal = ZDateTime.Now;
			AssertEquals(new ZDateTime(2015, 12, 20, 22, 12, 4), schedule.ScheduleSendTimeLocal);
			AssertEquals(new ZDateTime(2015, 12, 20, 12, 12, 4), schedule.ScheduleSendTimeUTC);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ScheduleCampaignItems(Factory.NewWithValidTestData<GlbCompanyCampaign>());
		}

		#endregion
	}
}
