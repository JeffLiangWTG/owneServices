using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemSchedule))]
	sealed class GlbCompanyCampaignItemScheduleTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2015, 12, 20, 4, 4, 6)]
		public void TestScheduleSendTimeLocal()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaignContactCollection[0].VCC_OffsetMinutesFromUtc = 720;
			campaignContactCollection[1].VCC_OffsetMinutesFromUtc = 180;
			campaignContactCollection[0].VCC_RelatedPortCode = "NZABY";
			campaignContactCollection[1].VCC_RelatedPortCode = "QAALU";
			campaignContactCollection[0].VCC_CivilianZone = "NZST";
			campaignContactCollection[1].VCC_CivilianZone = "AST";

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.SelectedScheduleItems.UnionWith(campaignContactCollection.Cast<CampaignContact>());

			scheduler.ScheduleSendTimeLocal = ZDateTime.Now;
			AssertEquals(scheduler.ScheduleItemsCollection[0].ScheduleSendTimeLocal, scheduler.ScheduleSendTimeLocal);
			AssertEquals(scheduler.ScheduleItemsCollection[1].ScheduleSendTimeLocal, scheduler.ScheduleSendTimeLocal);
			AssertEquals(scheduler.ScheduleItemsCollection[0].ScheduleSendTimeUTC, new ZDateTime(2015, 12, 19, 16, 4, 6));
			AssertEquals(scheduler.ScheduleItemsCollection[1].ScheduleSendTimeUTC, new ZDateTime(2015, 12, 20, 1, 4, 6));

			scheduler.ScheduleSendTimeLocal = ZDateTime.Empty;
			AssertEquals(scheduler.ScheduleItemsCollection[0].ScheduleSendTimeLocal, ZDateTime.Now);
			AssertEquals(scheduler.ScheduleItemsCollection[1].ScheduleSendTimeLocal, ZDateTime.Now);
			AssertEquals(scheduler.ScheduleItemsCollection[0].ScheduleSendTimeUTC, new ZDateTime(2015, 12, 19, 16, 4, 6));
			AssertEquals(scheduler.ScheduleItemsCollection[1].ScheduleSendTimeUTC, new ZDateTime(2015, 12, 20, 1, 4, 6));
		}

		[TestDate(2015, 12, 20, 4, 4, 6)]
		public void TestIsSendersLocalTimeUsed()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaignContactCollection[0].VCC_OffsetMinutesFromUtc = 720;
			campaignContactCollection[1].VCC_OffsetMinutesFromUtc = -180;
			campaignContactCollection[0].VCC_RelatedPortCode = "NZABY";
			campaignContactCollection[1].VCC_RelatedPortCode = "BRADI";
			campaignContactCollection[0].VCC_CivilianZone = "NZST";
			campaignContactCollection[1].VCC_CivilianZone = "BRT";

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.SelectedScheduleItems.UnionWith(campaignContactCollection.Cast<CampaignContact>());

			scheduler.ScheduleSendTimeLocal = ZDateTime.Now;
			scheduler.SenderTimeZone = "USAAA"; // -6 Hours: lagging UTC
			scheduler.IsRecipientsLocalTimeUsed = false;
			scheduler.IsSendersLocalTimeUsed = true;
			Assert("Schedule Items Collections should be readonly", scheduler.ScheduleItemsCollection.ReadOnly);
			AssertEquals(new ZDateTime(2015, 12, 20, 16, 4, 6), scheduler.ScheduleItemsCollection[0].ScheduleSendTimeLocal);
			AssertEquals(new ZDateTime(2015, 12, 20, 1, 4, 6), scheduler.ScheduleItemsCollection[1].ScheduleSendTimeLocal);
			AssertEquals(new ZDateTime(2015, 12, 20, 4, 4, 6), scheduler.ScheduleItemsCollection[0].ScheduleSendTimeUTC);
			AssertEquals(new ZDateTime(2015, 12, 20, 4, 4, 6), scheduler.ScheduleItemsCollection[1].ScheduleSendTimeUTC);
		}

		[TestDate(2015, 12, 20, 4, 4, 6)]
		public void TestSenderTimeZone()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaignContactCollection[0].VCC_OffsetMinutesFromUtc = 720;
			campaignContactCollection[1].VCC_OffsetMinutesFromUtc = -180;
			campaignContactCollection[0].VCC_RelatedPortCode = "NZABY";
			campaignContactCollection[1].VCC_RelatedPortCode = "BRADI";
			campaignContactCollection[0].VCC_CivilianZone = "NZST";
			campaignContactCollection[1].VCC_CivilianZone = "BRT";

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.SelectedScheduleItems.UnionWith(campaignContactCollection.Cast<CampaignContact>());

			scheduler.ScheduleSendTimeLocal = ZDateTime.Now;
			scheduler.IsRecipientsLocalTimeUsed = false;
			scheduler.SenderTimeZone = "USAAA"; // -6 Hours: lagging UTC
			AssertEquals(new ZDateTime(2015, 12, 20, 16, 4, 6), scheduler.ScheduleItemsCollection[0].ScheduleSendTimeLocal);
			AssertEquals(new ZDateTime(2015, 12, 20, 1, 4, 6), scheduler.ScheduleItemsCollection[1].ScheduleSendTimeLocal);
			AssertEquals(new ZDateTime(2015, 12, 20, 4, 4, 6), scheduler.ScheduleItemsCollection[0].ScheduleSendTimeUTC);
			AssertEquals(new ZDateTime(2015, 12, 20, 4, 4, 6), scheduler.ScheduleItemsCollection[1].ScheduleSendTimeUTC);
		}

		public void TestNoTimeZoneDescription()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaignContactCollection[0].VCC_TimeZoneSetName = "";
			campaignContactCollection[1].VCC_TimeZoneSetName = "";
			campaignContactCollection[0].VCC_CivilianZone = "";
			campaignContactCollection[1].VCC_CivilianZone = "";
			scheduler.SelectedScheduleItems.UnionWith(campaignContactCollection.Cast<CampaignContact>());
			var scheduleCollection = scheduler.ScheduleItemsCollection;

			AssertEquals("No time zone exists for 2 contacts. These have been included to the sender's local time zone.", scheduler.NoTimeZoneDescription);
		}

		[TestDate(2016, 4, 7)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSenderTimeZoneDescription()
		{
			RefTimeZone timeZone = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "EST";
			timeZone.R2_OffsetMinutesFromUTC = 540;

			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.R3_TimeZoneSetName = "Eastern";

			var country = Factory.New<RefCountry>();
			country.RN_Code = "XX";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_R3 = timeZoneSet.PK;

			RefTimeZone timeZoneDst = Factory.NewWithValidTestData<RefTimeZone>();
			timeZoneDst.R2_CivilianTimeZoneCode = "ESTD";
			timeZoneDst.R2_OffsetMinutesFromUTC = 600;

			timeZoneSet.R3_R2_DaylightSavingZone = timeZoneDst.PK;
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			ScheduleItemDataLoaderTest.isInDayLightSavings = false;
			campaign.CampaignItemSchedule.DataLoader.TimeZoneInfo = new ScheduleItemDataLoaderTest.TestTimeZone("Eastern", timeZone.R2_OffsetMinutesFromUTC / 60m, timeZoneDst.R2_OffsetMinutesFromUTC / 60m);

			campaign.CampaignItemSchedule.SenderTimeZone = "LOCO2";
			AssertEquals(campaign.CampaignItemSchedule.SenderTimeZoneDescription, "EASTERN UTC+9:00");

			ScheduleItemDataLoaderTest.isInDayLightSavings = true;
			AssertEquals(campaign.CampaignItemSchedule.SenderTimeZoneDescription, "EASTERN UTC+10:00");
		}

		[TestDate(2016, 6, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSaveRelatedBusinessObjects_ForCampaignItem()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";

			ZDateTime currentDate = ZDateTime.UtcNow;
			var campaignItem1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = currentDate;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_G0 = campaign.PK;
			var campaignItem2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = currentDate;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_G0 = campaign.PK;
			Factory.Save();

			var scheduler = campaign.CampaignItemSchedule;
			BusinessObjectFactory itemFactory = new BusinessObjectFactory();
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem1.PK));
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem2.PK));

			var collection = scheduler.ScheduleItemsCollection;
			AssertEquals(currentDate, collection[0].ScheduleSendTimeUTC);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(campaign);
			var scheduler2 = campaignInNewFactory.CampaignItemSchedule;

			foreach (GlbCompanyCampaignItem campaignItem in campaign.CampaignItemSchedule.SelectedScheduleItems)
			{
				scheduler2.SelectedScheduleItems.Add(newFactory.ImportFromAnotherFactorySafe(campaignItem));
			}

			ZDateTime newLocalDate = currentDate.ToLocalBranchTime().AddDays(3);
			scheduler2.ScheduleSendTimeLocal = newLocalDate;
			scheduler2.SaveRelatedBusinessObjects();
			Factory.Save();
			collection = scheduler2.ScheduleItemsCollection;
			AssertEquals(newLocalDate.ToUniversalBranchTime(), collection[0].ScheduleSendTimeUTC);
		}

		[TestDate(2016, 6, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSaveRelatedBusinessObjects_ForCampaignItemWithInactiveContact()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";

			ZDateTime currentDate = ZDateTime.UtcNow;
			var campaignItem1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = currentDate;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_G0 = campaign.PK;
			var campaignItem2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = currentDate;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_G0 = campaign.PK;
			Factory.Save();

			var scheduler = campaign.CampaignItemSchedule;
			BusinessObjectFactory itemFactory = new BusinessObjectFactory();
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem1.PK));
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem2.PK));

			var collection = scheduler.ScheduleItemsCollection;
			AssertEquals(currentDate, collection[0].ScheduleSendTimeUTC);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.Load<GlbCompanyCampaign>(campaign.PK);
			var scheduler2 = campaignInNewFactory.CampaignItemSchedule;
			scheduler2.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem1.PK));
			scheduler2.SelectedScheduleItems.Add(itemFactory.Load<GlbCompanyCampaignItem>(campaignItem2.PK));

			contact1 = newFactory.Load<OrgContact>(contact1.PK);
			contact1.OC_IsActive = false;
			contact2 = newFactory.Load<OrgContact>(contact2.PK);
			contact2.OC_IsActive = false;
			newFactory.Save();

			AssertNoExceptionThrown(delegate
			{ scheduler2.SaveRelatedBusinessObjects(); });
		}

		[TestDate(2016, 6, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSaveRelatedBusinessObjects_ForCampaignContact()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact@test.com";
			contact1.OC_ContactName = "Contact 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_ContactName = "Contact 2";

			Factory.Save();

			ZDateTime currentDate = ZDateTime.UtcNow;

			var scheduler = campaign.CampaignItemSchedule;
			BusinessObjectFactory itemFactory = new BusinessObjectFactory();
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<CampaignContact>(contact1.PK));
			scheduler.SelectedScheduleItems.Add(itemFactory.Load<CampaignContact>(contact2.PK));

			var collection = scheduler.ScheduleItemsCollection;
			AssertEquals(currentDate, collection[0].ScheduleSendTimeUTC);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(campaign);
			var scheduler2 = campaignInNewFactory.CampaignItemSchedule;

			foreach (CampaignContact contact in campaign.CampaignItemSchedule.SelectedScheduleItems)
			{
				scheduler2.SelectedScheduleItems.Add(newFactory.ImportFromAnotherFactorySafe(contact));
			}

			scheduler2.SendScheduleEventHandler += (sender, e) =>
			{
				foreach (CampaignContact contact in e.Contacts)
				{
					GlbCompanyCampaignItem campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
					campaignItem.G8_ScheduleTimeUtc = contact.ScheduleData.ScheduleTimeUtc;
					campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
					campaignItem.G8_RecipientID = contact.PK;
					campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
				}
			};

			ZDateTime newLocalDate = currentDate.ToLocalBranchTime().AddDays(3);
			scheduler2.ScheduleSendTimeLocal = newLocalDate;
			scheduler2.SaveRelatedBusinessObjects();
			Factory.Save();

			GlbCompanyCampaignItem campaignItem1 = Factory.LoadTop1<GlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contact1.PK));
			GlbCompanyCampaignItem campaignItem2 = Factory.LoadTop1<GlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contact2.PK));
			AssertEquals(newLocalDate, campaignItem1.G8_ScheduleTimeUtc);
			AssertEquals(newLocalDate, campaignItem2.G8_ScheduleTimeUtc);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new GlbCompanyCampaignItemSchedule(campaign);
		}

		#endregion
	}
}
