using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ScheduleItemDataLoaderTest : TestCaseWithFactory
	{
		public void TestScheduledGlbCompanyCampaignItems()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.UtcNow;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_ScheduleTimeUtc = time;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_ScheduleTimeUtc = time;

			Factory.Save();

			var scheduler = campaign.CampaignItemSchedule;
			scheduler.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2, campaignItem3 });
			var scheduleCampaignItem = new ScheduleItemDataLoader(scheduler).LoadScheduleItems(ZDateTime.Now);

			AssertEquals("2 item match the condition", 2, scheduleCampaignItem.Count);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					campaignItem1,
					campaignItem3
				},
				campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(i => i.G8_TrackingStatus == "QUE"));
			AssertEquals("Schedule Status should be queued", TrackingStatusCodes.Codes.QUE, scheduleCampaignItem.First().Status);
		}

		public void TestScheduledGlbCompanyCampaignItems_ContactDeleted()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.UtcNow;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_ScheduleTimeUtc = time;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_ScheduleTimeUtc = time;

			Factory.Save();

			GlbCompanyCampaignItemSchedule scheduler = campaign.CampaignItemSchedule;
			scheduler.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2, campaignItem3 });

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(newFactory))
			{
				var contact22 = newFactory.Load<OrgContact>(contact2.PK);
				contact22.Delete();
				newFactory.Save();
			}

			AssertNoExceptionThrown(() =>
			{
				var scheduleCampaignItem = new ScheduleItemDataLoader(scheduler).LoadScheduleItems(ZDateTime.Now);
			});
		}

		public void TestScheduledGlbCompanyCampaignItems_ContactInactive()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.UtcNow;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_ScheduleTimeUtc = time;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = time;
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_ScheduleTimeUtc = time;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(newFactory))
			{
				var contact22 = newFactory.Load<OrgContact>(contact2.PK);
				contact22.OC_IsActive = false;
				newFactory.Save();
			}

			campaign = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbCompanyCampaign>(campaign.PK);
			var scheduler = campaign.CampaignItemSchedule;
			campaignItem1 = Factory.Load<GlbCompanyCampaignItem>(campaignItem1.PK);
			campaignItem2 = Factory.Load<GlbCompanyCampaignItem>(campaignItem2.PK);
			campaignItem3 = Factory.Load<GlbCompanyCampaignItem>(campaignItem3.PK);
			scheduler.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2, campaignItem3 });

			AssertNoExceptionThrown(() =>
			{
				var scheduleCampaignItem = new ScheduleItemDataLoader(scheduler).LoadScheduleItems(ZDateTime.Now);
			});
		}

		[TestDate(2016, 4, 7)]
		[TestUtcOffset(11, 0, 0)]
		public void TestScheduledCampaignContacts()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			RefTimeZone timeZoneDst = Factory.NewWithValidTestData<RefTimeZone>();
			timeZoneDst.R2_CivilianTimeZoneCode = "ESTD";
			timeZoneDst.R2_OffsetMinutesFromUTC = 660;

			timeZoneSet.R3_R2_DaylightSavingZone = timeZoneDst.PK;
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.Now;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			GlbCampaignContactCollection collection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			GlbCompanyCampaignItemSchedule scheduler = campaign.CampaignItemSchedule;
			scheduler.SelectedScheduleItems.UnionWith(collection.Cast<CampaignContact>());
			ScheduleItemDataLoader loader = new ScheduleItemDataLoader(scheduler);

			isInDayLightSavings = false;
			loader.TimeZoneInfo = new TestTimeZone("TestZoneName", ((decimal)timeZone.R2_OffsetMinutesFromUTC) / 60m, ((decimal)timeZoneDst.R2_OffsetMinutesFromUTC) / 60m);

			var scheduleCampaignItem = loader.LoadScheduleItems(time);

			AssertEquals("1 item match the condition", 1, scheduleCampaignItem.Count);

			var item = scheduleCampaignItem.Single();
			AssertEquals("Schedule Status should be queued", TrackingStatusCodes.Codes.QUE, item.Status);
			AssertEquals("UtcOffset", new ZShort(600), item.UtcOffset);

			isInDayLightSavings = true;
			scheduleCampaignItem = loader.LoadScheduleItems(time);

			AssertEquals("1 item match the condition", 1, scheduleCampaignItem.Count);
			AssertEquals("UtcOffset", new ZShort(660), scheduleCampaignItem.Single().UtcOffset);
		}

		[TestDate(2016, 3, 1)]
		public void TestOffsetFromUtcTimeZone()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			RefTimeZone timeZoneDst = Factory.NewWithValidTestData<RefTimeZone>();
			timeZoneDst.R2_CivilianTimeZoneCode = "ESTD";
			timeZoneDst.R2_OffsetMinutesFromUTC = 660;

			timeZoneSet.R3_R2_DaylightSavingZone = timeZoneDst.PK;
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			ScheduleItemDataLoader loader = new ScheduleItemDataLoader(scheduler);

			isInDayLightSavings = false;
			loader.TimeZoneInfo = new TestTimeZone("TestZoneName", ((decimal)timeZone.R2_OffsetMinutesFromUTC) / 60m, ((decimal)timeZoneDst.R2_OffsetMinutesFromUTC) / 60m);

			var result = loader.OffsetFromUtcTimeZone("LOCO2", ZDateTime.UtcNow.ToDateTime());
			AssertEquals(new ZShort(600), result);

			isInDayLightSavings = true;
			result = loader.OffsetFromUtcTimeZone("LOCO2", ZDateTime.UtcNow.ToDateTime());
			AssertEquals(new ZShort(660), result);
		}

		[TestDate(2016, 3, 1)]
		public void TestCivilianTimeZoneCodeTimeZone()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			RefTimeZone timeZoneDst = Factory.NewWithValidTestData<RefTimeZone>();
			timeZoneDst.R2_CivilianTimeZoneCode = "ESTD";
			timeZoneDst.R2_OffsetMinutesFromUTC = 660;

			timeZoneSet.R3_R2_DaylightSavingZone = timeZoneDst.PK;
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			ScheduleItemDataLoader loader = new ScheduleItemDataLoader(scheduler);

			isInDayLightSavings = false;
			loader.TimeZoneInfo = new TestTimeZone("TestZoneName", ((decimal)timeZone.R2_OffsetMinutesFromUTC / 60m), ((decimal)timeZoneDst.R2_OffsetMinutesFromUTC) / 60m);

			var result = loader.CivilianTimeZoneCodeTimeZone("LOCO2", ZDateTime.UtcNow.ToDateTime());
			AssertEquals("EST", result);

			isInDayLightSavings = true;
			result = loader.CivilianTimeZoneCodeTimeZone("LOCO2", ZDateTime.UtcNow.ToDateTime());
			AssertEquals("ESTD", result);
		}

		internal class TestTimeZone : TimeZoneBase
		{
			public TestTimeZone(string zoneName, decimal standardTimeZone, decimal dstTimeZone)
				: base(zoneName, standardTimeZone, dstTimeZone)
			{
			}

			protected override bool HasDaylightSaving
			{
				get { return isInDayLightSavings; }
			}

			protected override RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year)
			{
				DayLightSavingsInterval(year);

				RefTimeZoneRuleInfo startDstInfo = new RefTimeZoneRuleInfo(
					year, TimeZoneConstants.DstTransitionTypeStart,
					TimeZoneConstants.DstRuleDayOfMonth, TimeZoneConstants.DstTimeBaseLocal, DstStartDate,
					0, "", "");

				RefTimeZoneRuleInfo endDstInfo = new RefTimeZoneRuleInfo(
					year, TimeZoneConstants.DstTransitionTypeEnd,
					TimeZoneConstants.DstRuleDayOfMonth, TimeZoneConstants.DstTimeBaseLocal, DstEndDate,
					0, "", "");

				RefTimeZoneRuleInfoStartAndEndPair result = new RefTimeZoneRuleInfoStartAndEndPair(startDstInfo, endDstInfo);
				return result;
			}
		}

		RefTimeZoneSet timeZoneSet;
		RefTimeZone timeZone;
		RefCountry country;
		internal static bool isInDayLightSavings;
		static DateTime DstStartDate;
		static DateTime DstEndDate;

		static void DayLightSavingsInterval(int year)
		{
			DstStartDate = new DateTime(year, 2, 1);
			DstEndDate = DstStartDate.AddMonths(5);
		}

		protected override void SetUp()
		{
			base.SetUp();

			timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.R3_TimeZoneSetName = "Eastern";

			timeZone = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "EST";
			timeZone.R2_OffsetMinutesFromUTC = new ZShort(600);

			country = Factory.New<RefCountry>();
			country.RN_Code = "XX";
		}
	}
}
