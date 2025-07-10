using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSendSettings))]
	sealed class GlbCompanyCampaignSendSettingsTest : EnterpriseBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			AssertEquals(true, settings.GSC_IsRecipientLocalTime);
			AssertEquals(false, settings.GSC_IsSenderLocalTime);
			AssertEquals(GlbCompanyCampaignSendSettingsLookups.Codes.BAT, settings.GSC_ScheduleType);
			AssertEquals(14, settings.GSC_ScheduleTime.Hour);
			AssertNotNull(settings.ScheduleTask);
		}

		#endregion

		#region Properties

		#region GSC_ContactLimitEachBatch

		public void TestGSC_ContactLimitEachBatch_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitEachBatchUsed = true;

			settings.IsContactLimitEachBatchUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitEachBatchInfo);
		}

		public void TestGSC_ContactLimitEachBatch_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitEachBatchUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitEachBatchInfo.ReadOnly);

			settings.IsContactLimitEachBatchUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitEachBatchInfo.ReadOnly);
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationInHorizontal

		public void TestGSC_ContactLimitPerOrganizationInHorizontal_ClearPeriodInDaysAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;
			settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 10;

			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			AssertEquals(false, settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed);
			AssertEquals((short)0, settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);
		}

		public void TestGSC_ContactLimitPerOrganizationInHorizontal_CopiedToOtherTouchesInSameHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = master.AllTouches.AddNew();
			touch1a.G0_HorizontalId = 1;
			touch1a.SendSettings.IsBatchSchedule = true;
			touch1a.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			touch1a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal = 50;
			touch1a.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;
			touch1a.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 5;

			var touch1b = master.AllTouches.AddNew();
			touch1b.G0_HorizontalId = 1;
			AssertEquals(true, touch1b.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed);
			AssertEquals((short)50, touch1b.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);
			AssertEquals(true, touch1b.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed);
			AssertEquals((short)5, touch1b.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);

			touch1b.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal = 80;
			AssertEquals((short)80, touch1a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);

			touch1b.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 10;
			AssertEquals((short)10, touch1a.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);

			var horizontal2 = master.AddHorizontal();
			var touch2a = master.AllTouches.AddNew();
			touch2a.G0_HorizontalId = 2;
			AssertEquals(false, touch2a.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed);
			AssertEquals((short)0, touch2a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);
			AssertEquals(false, touch2a.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed);
			AssertEquals((short)0, touch2a.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);

			touch2a.G0_HorizontalId = 1;
			AssertEquals(true, touch2a.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed);
			AssertEquals((short)80, touch2a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);
			AssertEquals(true, touch2a.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed);
			AssertEquals((short)10, touch2a.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);
		}

		public void TestGSC_ContactLimitPerOrganizationInHorizontal_WithGroup()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = master.AllTouches.AddNew();
			touch1a.G0_CampaignName = master.G0_CampaignName + "1A";
			touch1a.G0_HorizontalId = 1;
			touch1a.CurrentGroupColor = 14;

			var touch1b = master.AllTouches.AddNew();
			touch1b.G0_CampaignName = master.G0_CampaignName + "1B";
			touch1b.G0_HorizontalId = 1;
			touch1b.CurrentGroupColor = 14;

			Factory.Save();

			touch1a.SendSettings.IsBatchSchedule = true;
			touch1a.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			touch1a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal = 50;
			touch1a.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;
			touch1a.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 5;

			var touch1c = master.AllTouches.AddNew();
			touch1c.G0_HorizontalId = 1;
			AssertEquals(true, touch1c.SendSettings.IsContactLimitPerOrganizationInHorizontalUsed);
			AssertEquals((short)50, touch1c.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);
			AssertEquals(true, touch1c.SendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed);
			AssertEquals((short)5, touch1c.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);

			touch1a.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal = 80;
			AssertEquals((short)80, touch1c.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal);

			touch1b.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 10;
			AssertEquals((short)10, touch1c.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays);
		}

		public void TestGSC_ContactLimitPerOrganizationInHorizontal_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;

			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo);
		}

		public void TestGSC_ContactLimitPerOrganizationInHorizontal_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitPerOrganizationInHorizontalInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitPerOrganizationInHorizontalInfo.ReadOnly);
		}

		#endregion

		#region GSC_ContactLimitPerOrgInHorizontalPeriodInDays

		public void TestIsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			AssertEquals(true, settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			AssertEquals(false, settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedInfo.ReadOnly);
		}

		public void TestGSC_ContactLimitPerOrgInHorizontalPeriodInDays_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;

			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo);
		}

		public void TestGSC_ContactLimitPerOrgInHorizontalPeriodInDays_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo.ReadOnly);
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationInTouch

		public void TestGSC_ContactLimitPerOrganizationInTouch_ClearPeriodInDaysAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = true;
			settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays = 10;

			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			AssertEquals(false, settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed);
			AssertEquals((short)0, settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays);
		}

		public void TestGSC_ContactLimitPerOrganizationInTouch_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationInTouchUsed = true;

			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchInfo);
		}

		public void TestGSC_ContactLimitPerOrganizationInTouch_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitPerOrganizationInTouchInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitPerOrganizationInTouchInfo.ReadOnly);
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationInTouchPeriodInDays

		public void TestIsContactLimitPerOrganizationInTouchPeriodInDaysUsed_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			AssertEquals(true, settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			AssertEquals(false, settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedInfo.ReadOnly);
		}

		public void TestGSC_ContactLimitPerOrganizationInTouchPeriodInDays_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = true;

			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo);
		}

		public void TestGSC_ContactLimitPerOrganizationInTouchPeriodInDays_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo.ReadOnly);
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationEachBatch

		public void TestGSC_ContactLimitPerOrganizationEachBatch_NoValidationErrorAfterDisabling()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationEachBatchUsed = true;

			settings.IsContactLimitPerOrganizationEachBatchUsed = false;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationEachBatchInfo);
		}

		public void TestGSC_ContactLimitPerOrganizationEachBatch_ReadOnly()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationEachBatchUsed = false;
			AssertEquals(true, settings.GSC_ContactLimitPerOrganizationEachBatchInfo.ReadOnly);

			settings.IsContactLimitPerOrganizationEachBatchUsed = true;
			AssertEquals(false, settings.GSC_ContactLimitPerOrganizationEachBatchInfo.ReadOnly);
		}

		#endregion

		#region ScheduleTask

		public void TestFactorySaving_ScheduleTaskIsReloaded()
		{
			ZDateTime date1 = ZDateTime.BrettsBirthday;
			ZDateTime date2 = ZDateTime.Now;
			Factory.RefreshEnabled = false;
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var masterCampaign = factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			var scheduleTask = settings.ScheduleTask;
			scheduleTask.S5_StartDate = date1;
			AssertEquals(date1, settings.ScheduleTask.S5_StartDate);

			factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedTask = newFactory.Load<GlbCompanyCampaignSendScheduleTask>(scheduleTask.PK);
			reloadedTask.S5_StartDate = date2;
			newFactory.Save();

			factory.Save();
			Assert(settings.ScheduleTask.S5_StartDate >= date2);
		}

		#endregion

		#region GSC_HoursOffset and Days/Hours Offsets

		public void TestDaysHoursOffset()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			settings.IsUseCurrentTime = true;

			DaysAndHoursFromGSC_HoursOffset(settings);
			GSC_HoursOffsetFromDaysAndHours(settings);
		}

		static void GSC_HoursOffsetFromDaysAndHours(GlbCompanyCampaignSendSettings settings)
		{
			settings.DaysOffset = 0;
			settings.HoursOffset = 12;
			AssertEquals("Half a day", (short)12, settings.GSC_HoursOffset);

			settings.DaysOffset = 1;
			settings.HoursOffset = 0;
			AssertEquals("Whole day", (short)24, settings.GSC_HoursOffset);

			settings.DaysOffset = 1;
			settings.HoursOffset = 12;
			AssertEquals("A day and a half", (short)36, settings.GSC_HoursOffset);
		}

		static void DaysAndHoursFromGSC_HoursOffset(GlbCompanyCampaignSendSettings settings)
		{
			settings.GSC_HoursOffset = 12;
			AssertEquals("Half a day", (short)0, settings.DaysOffset);
			AssertEquals("Half a day", (short)12, settings.HoursOffset);

			settings.GSC_HoursOffset = 23;
			AssertEquals("Almost a day", (short)0, settings.DaysOffset);
			AssertEquals("Almost a day", (short)23, settings.HoursOffset);

			settings.GSC_HoursOffset = 24;
			AssertEquals("Whole day", (short)1, settings.DaysOffset);
			AssertEquals("Whole day", (short)0, settings.HoursOffset);

			settings.GSC_HoursOffset = 36;
			AssertEquals("A day and a half", (short)1, settings.DaysOffset);
			AssertEquals("A day and a half", (short)12, settings.HoursOffset);
		}

		public void TestHoursAreSetZeroWhen()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;

			settings.IsUseCurrentTime = true;
			settings.DaysOffset = 1;

			settings.HoursOffset = 8;
			AssertEquals("Hours offset'd be 8", settings.HoursOffset, (ZShort)8);

			settings.IsUseCurrentTime = false;

			AssertEquals("Hours offset'd be 0", settings.HoursOffset, (ZShort)0);
			settings.HoursOffset = 8;
			AssertEquals("Hours offset'd be 8", settings.HoursOffset, (ZShort)8);
		}

		public void TestHasChanges()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			settings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			settings.GSC_ScheduleTime = ZDateTime.Empty;
			Factory.Save();

			var settings2 = new BusinessObjectFactory().Load<GlbCompanyCampaignSendSettings>(settings.PK);
			AssertEquals(true, settings2.IsUseCurrentTime);
			AssertEquals(false, settings2.HasChanges);
		}

		#endregion

		#region HasScheduledItems

		public void TestHasScheduledItemsDripMarketing()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = master.AllTouches.AddNew();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			Assert(!campaign.SendSettings.HasScheduledItems);

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			Assert(!campaign.SendSettings.HasScheduledItems);

			item.G8_ScheduleTimeUtc = DateTime.Now;
			Assert(campaign.SendSettings.HasScheduledItems);
		}

		public void TestHasScheduledItemsInsideSales()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var campaign = master.AllTouches.AddNew();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			Assert(!campaign.SendSettings.HasScheduledItems);

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			Assert(!campaign.SendSettings.HasScheduledItems);

			item.G8_ScheduleTimeUtc = DateTime.Now;
			Assert(campaign.SendSettings.HasScheduledItems);
		}

		#endregion

		#region HasQueuedItems

		public void TestHasQueuedItemsDripMarketing()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = master.AllTouches.AddNew();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			Assert(!campaign.SendSettings.HasQueuedItems);

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			Assert(campaign.SendSettings.HasQueuedItems);

			item.G8_ScheduleTimeUtc = DateTime.Now;
			Assert(campaign.SendSettings.HasQueuedItems);
		}

		public void TestHasQueuedItemsInsideSales()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var campaign = master.AllTouches.AddNew();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			Assert(!campaign.SendSettings.HasQueuedItems);

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			Assert(campaign.SendSettings.HasQueuedItems);

			item.G8_ScheduleTimeUtc = DateTime.Now;
			Assert(campaign.SendSettings.HasQueuedItems);
		}

		#endregion

		#endregion

		#region Scheduling

		[TestDate(2016, 7, 10, 18, 30, 0)]
		public void TestGetNextTouchScheduleData_CanFitBatch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "thomas@test.com";
			contact1.OC_ContactName = "Thomas";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "gordon@test.com";
			contact2.OC_ContactName = "Gordon";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "viktor@test.com";
			contact3.OC_ContactName = "Viktor";

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch.G0_LastSentBatchNumber = 1;
			touch.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.IsContactLimitEachBatchUsed = true;
			sendSettings.GSC_ContactLimitEachBatch = 3;
			sendSettings.GSC_IsSenderLocalTime = ZBool.True;
			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 30, 0));
			var scheduleTask = sendSettings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var itemMaster1 = master.CampaignsItemsSent.AddNew();
			itemMaster1.G8_RecipientID = contact1.PK;
			itemMaster1.G8_TrackingStatus = "UNV";

			var itemMaster2 = master.CampaignsItemsSent.AddNew();
			itemMaster2.G8_RecipientID = contact1.PK;
			itemMaster2.G8_TrackingStatus = "UNV";

			var itemMaster3 = master.CampaignsItemsSent.AddNew();
			itemMaster3.G8_RecipientID = contact1.PK;
			itemMaster3.G8_TrackingStatus = "UNV";

			var item1 = touch.CampaignsItemsSent.AddNew();
			item1.G8_RecipientID = contact1.PK;
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item1.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item1.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item1.G8_BatchNumber = 1;

			var item2 = touch.CampaignsItemsSent.AddNew();
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item2.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item2.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item2.G8_BatchNumber = 1;

			var campaignContact = Factory.Load<CampaignContact>(contact3.PK);
			var scheduleData = sendSettings.GetNextTouchScheduleData(touch, campaignContact, new Dictionary<GlbCompanyCampaign, List<ScheduleData>>(), "AUSYD");

			AssertNotNull(scheduleData);
			AssertEquals(campaignContact, scheduleData.Contact);
			AssertEquals(ZDateTime.Empty, scheduleData.ScheduleTimeUtc);
			AssertEquals(0, scheduleData.BatchNumber);
		}

		[TestDate(2016, 7, 10, 18, 30, 0)]
		public void TestGetNextTouchScheduleData_CanFitBatch_InactiveContact()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_LastSentBatchNumber = 1;
			master.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "thomas@test.com";
			contact1.OC_ContactName = "Thomas";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "gordon@test.com";
			contact2.OC_ContactName = "Gordon";
			contact2.OC_IsActive = false;
			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "viktor@test.com";
			contact3.OC_ContactName = "Viktor";

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch.G0_LastSentBatchNumber = 1;
			touch.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.IsContactLimitEachBatchUsed = true;
			sendSettings.GSC_ContactLimitEachBatch = 3;
			sendSettings.GSC_IsSenderLocalTime = ZBool.True;
			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 30, 0));
			var scheduleTask = sendSettings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var itemMaster1 = master.CampaignsItemsSent.AddNew();
			itemMaster1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster1.G8_RecipientID = contact1.PK;
			itemMaster1.G8_TrackingStatus = "UNV";

			var itemMaster2 = master.CampaignsItemsSent.AddNew();
			itemMaster2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster2.G8_RecipientID = contact2.PK;
			itemMaster2.G8_TrackingStatus = "UNV";

			var itemMaster3 = master.CampaignsItemsSent.AddNew();
			itemMaster3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster3.G8_RecipientID = contact3.PK;
			itemMaster3.G8_TrackingStatus = "UNV";

			var item1 = touch.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item1.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item1.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item1.G8_BatchNumber = 1;

			var item2 = touch.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item2.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item2.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item2.G8_BatchNumber = 1;

			Factory.Save();

			var campaignContact = Factory.Load<CampaignContact>(contact3.PK);
			contact3.OC_IsActive = false;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			touch = newFactory.Load<GlbCompanyCampaign>(touch.PK);
			touch.G0_LastSentBatchNumber = 1;
			touch.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);
			sendSettings = newFactory.Load<GlbCompanyCampaignSendSettings>(sendSettings.PK);
			sendSettings.IsContactLimitPerOrganizationEachBatchUsed = true;
			var scheduleData = sendSettings
				.GetNextTouchScheduleData(touch, campaignContact, new Dictionary<GlbCompanyCampaign, List<ScheduleData>>(), "AUSYD");

			AssertNotNull(scheduleData);
			AssertEquals(campaignContact, scheduleData.Contact);
			AssertEquals(ZDateTime.Empty, scheduleData.ScheduleTimeUtc);
			AssertEquals(0, scheduleData.BatchNumber);
		}

		public void TestGetNextTouchScheduleData_CanFitBatch_NullContact()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_LastSentBatchNumber = 1;
			master.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "thomas@test.com";
			contact1.OC_ContactName = "Thomas";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "gordon@test.com";
			contact2.OC_ContactName = "Gordon";
			contact2.OC_IsActive = false;
			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "viktor@test.com";
			contact3.OC_ContactName = "Viktor";

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch.G0_LastSentBatchNumber = 1;
			touch.G0_LastSentBatchLocalDate = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.IsContactLimitEachBatchUsed = true;
			sendSettings.GSC_ContactLimitEachBatch = 3;
			sendSettings.GSC_IsSenderLocalTime = ZBool.True;
			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 30, 0));
			var scheduleTask = sendSettings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 7, 10, 18, 0, 0);

			var itemMaster1 = master.CampaignsItemsSent.AddNew();
			itemMaster1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster1.G8_RecipientID = contact1.PK;
			itemMaster1.G8_TrackingStatus = "UNV";

			var itemMaster2 = master.CampaignsItemsSent.AddNew();
			itemMaster2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster2.G8_RecipientID = contact2.PK;
			itemMaster2.G8_TrackingStatus = "UNV";

			var itemMaster3 = master.CampaignsItemsSent.AddNew();
			itemMaster3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster3.G8_RecipientID = contact3.PK;
			itemMaster3.G8_TrackingStatus = "UNV";

			var item1 = touch.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item1.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item1.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item1.G8_BatchNumber = 1;

			var item2 = touch.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item2.G8_ScheduleTimeUtc = new ZDateTime(2016, 7, 10, 10, 0, 0);
			item2.G8_LastSentTimeUtc = new ZDateTime(2016, 7, 10, 11, 0, 0);
			item2.G8_BatchNumber = 1;

			Factory.Save();

			AssertNoExceptionThrown(delegate
			{
				sendSettings.GetNextTouchScheduleData(touch, null, new Dictionary<GlbCompanyCampaign, List<ScheduleData>>(), "AUSYD");
			});
		}

		[TestDate(2002, 2, 2)]
		public void TestRecalculateAllScheduledTimes()
		{
			TestDateAttribute.UseUNLOCO = true;

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.DaysOffset = 1;
			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(10, 0, 0));
			sendSettings.GSC_IsRecipientLocalTime = false;
			sendSettings.GSC_IsSenderLocalTime = true;

			var scheduled1 = touch.CampaignsItemsSent.AddNew();
			scheduled1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			scheduled1.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			scheduled1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			scheduled1.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 1, 0, 0);
			scheduled1.G8_LastSentTimeUtc = ZDateTime.Empty;
			scheduled1.G8_BatchNumber = 1;

			var scheduled2 = touch.CampaignsItemsSent.AddNew();
			scheduled2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			scheduled2.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			scheduled2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			scheduled2.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 2, 0, 0);
			scheduled2.G8_LastSentTimeUtc = new ZDateTime(2002, 2, 2, 1, 0, 0);
			scheduled2.G8_BatchNumber = 2;

			var scheduled3 = touch.CampaignsItemsSent.AddNew();
			scheduled3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			scheduled3.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			scheduled3.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			scheduled3.G8_ScheduleTimeUtc = ZDateTime.Empty;
			scheduled3.G8_LastSentTimeUtc = ZDateTime.Empty;
			scheduled3.G8_BatchNumber = 2;

			var sent1 = touch.CampaignsItemsSent.AddNew();
			sent1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			sent1.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			sent1.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			sent1.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 1, 1, 0, 0);
			sent1.G8_LastSentTimeUtc = new ZDateTime(2002, 2, 1, 1, 0, 0);
			sent1.G8_BatchNumber = 1;

			Factory.Save();

			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(23, 0, 0));
			var newDate = sendSettings.CalculateScheduleTimeUtc(sent1.RecipientFromView,
				sendSettings.ScheduleTask.CalcNextRunTimeLocal.ToDateTime(),
				"AUSYD");

			sendSettings.RecalculateAllScheduledTimes();
			AssertEquals(newDate, scheduled1.G8_ScheduleTimeUtc);
			AssertEquals(newDate, scheduled2.G8_ScheduleTimeUtc);
			AssertEquals("Should not reschedule for batch", ZDateTime.Empty, scheduled3.G8_ScheduleTimeUtc);
			AssertEquals("Should not reschedule already sent", new ZDateTime(2002, 2, 1, 1, 0, 0), sent1.G8_ScheduleTimeUtc);
			AssertEquals(1, scheduled1.G8_BatchNumber);
			AssertEquals(2, scheduled2.G8_BatchNumber);

			sendSettings.IsImmediate = true;
			sendSettings.RecalculateAllScheduledTimes();
			AssertEquals(new ZDateTime(2002, 2, 2, 0, 0, 5), scheduled1.G8_ScheduleTimeUtc);
			AssertEquals(new ZDateTime(2002, 2, 2, 0, 0, 5), scheduled2.G8_ScheduleTimeUtc);
			AssertNotEquals("Should reschedule because schedule type is not batch", ZDateTime.Empty, scheduled3.G8_ScheduleTimeUtc);
			AssertEquals("Should not reschedule already sent", new ZDateTime(2002, 2, 1, 1, 0, 0), sent1.G8_ScheduleTimeUtc);
			AssertEquals(1, scheduled1.G8_BatchNumber);
			AssertEquals(2, scheduled2.G8_BatchNumber);
		}

		[TestDate(2002, 2, 2)]
		public void TestRecalculateAllScheduledTimes_SetCampaignItemsSender()
		{
			TestDateAttribute.UseUNLOCO = true;

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "Touch";
			touch.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			touch.SenderPool.AddNew().GCP_GS_NKSender = staff1.GS_Code;
			touch.SenderPool.AddNew().GCP_GS_NKSender = staff2.GS_Code;

			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.DaysOffset = 1;
			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(10, 0, 0));
			sendSettings.GSC_IsRecipientLocalTime = false;
			sendSettings.GSC_IsSenderLocalTime = true;

			var scheduled1 = touch.CampaignsItemsSent.AddNew();
			scheduled1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			scheduled1.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			scheduled1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			scheduled1.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 1, 0, 0);
			scheduled1.G8_LastSentTimeUtc = ZDateTime.Empty;
			scheduled1.G8_BatchNumber = 1;

			var scheduled2 = touch.CampaignsItemsSent.AddNew();
			scheduled2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			scheduled2.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			scheduled2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			scheduled2.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 2, 0, 0);
			scheduled2.G8_LastSentTimeUtc = new ZDateTime(2002, 2, 2, 1, 0, 0);
			scheduled2.G8_BatchNumber = 2;
			Factory.Save();

			sendSettings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(23, 0, 0));
			sendSettings.RecalculateAllScheduledTimes();

			var campaignsItemsSentList = touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();
			Assert("Staff1 should be one of the senders in CampaignsItemsSent.", campaignsItemsSentList.Any(i => i.G8_GS_NKSender == staff1.GS_Code));
			Assert("Staff2 should be one of the senders in CampaignsItemsSent.", campaignsItemsSentList.Any(i => i.G8_GS_NKSender == staff2.GS_Code));
		}

		[TestDate(2020, 8, 28, 14, 30, 0)]
		public void TestRecalculateAllScheduledTimes_OpportunityQueued()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = master.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_CampaignName = "Touch OPP";
			touch.G0_GS_NKCampaignCoordinator = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var campaignItem1 = touch.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.Now.AddDays(1);

			var campaignItem2 = touch.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.Now.AddDays(1);
			Factory.Save();

			var sendSettings = touch.SendSettings;
			sendSettings.IsFixedDate = true;
			sendSettings.GSC_ScheduleTime = ZDateTime.Now;
			sendSettings.RecalculateAllScheduledTimes();

			AssertEquals(sendSettings.GSC_ScheduleTime, campaignItem1.G8_ScheduleTimeUtc);
			AssertEquals(sendSettings.GSC_ScheduleTime, campaignItem2.G8_ScheduleTimeUtc);
			Assert(campaignItem1.G8_GS_NKSender.IsEmpty);
			Assert(campaignItem2.G8_GS_NKSender.IsEmpty);
		}

		public void TestRecalculateAllScheduledTimesOpportunityCreation_EmptySender()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = master.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_CampaignName = "Touch OPP";

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.Now;
			Factory.Save();

			var sendSettings = touch.SendSettings;
			sendSettings.IsDelayed = true;
			sendSettings.RecalculateAllScheduledTimes();

			Assert(campaignItem.G8_EmailSenderName.IsEmpty);
			Assert(campaignItem.G8_SenderEmailAddress.IsEmpty);
			Assert(campaignItem.G8_GS_NKSender.IsEmpty);
		}

		#endregion

		#region Related Business Objects

		public void TestScheduleTask()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			settings.IsBatchSchedule = true;
			AssertNotNull(settings.ScheduleTask);

			settings.IsImmediate = true;
			Factory.Save();

			AssertEquals(false, settings.ScheduleTask.S5_IsActive);
		}

		#endregion

		#region Save

		public void TestSave_ShouldClearG0_LastSentBatchLocalDate()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			var sendSettings = touch.SendSettings;
			sendSettings.IsBatchSchedule = true;
			sendSettings.GSC_IsRecipientLocalTime = true;
			Factory.Save();

			touch.G0_LastSentBatchLocalDate = new ZDateTime(2002, 2, 2);
			Factory.Save();

			sendSettings.GSC_IsRecipientLocalTime = false;
			sendSettings.GSC_IsSenderLocalTime = true;
			Factory.Save();
			AssertEquals("Clear this date so that it doesn't keep on trying to send to this legacy date when new contacts are being added", ZDateTime.Empty, touch.G0_LastSentBatchLocalDate);
		}

		public void TestScheduleTaskS5_IsActiveUpdatedOnSave()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_CampaignName = "touch 1";
			var item1 = touch.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = ZGuid.NewZGuid();
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			touch.SendSettings.IsBatchSchedule = true;
			touch.SendSettings.ScheduleTask.S5_StartDate = ZDate.Today;

			Factory.Save();
			AssertEquals("No contacts queued for scheduling - don't need to run schedule task", false, touch.SendSettings.ScheduleTask.S5_IsActive);

			var touch2 = masterCampaign.AllTouches.AddNew();
			touch2.G0_CampaignName = "touch 2";

			var item2 = touch2.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = ZGuid.NewZGuid();
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item2.G8_ScheduleTimeUtc = ZDateTime.Empty;
			touch2.SendSettings.IsBatchSchedule = true;
			touch2.SendSettings.ScheduleTask.S5_StartDate = ZDate.Today;

			Factory.Save();
			AssertEquals("Contains contacts queued for scheduling - need to run schedule task", true, touch2.SendSettings.ScheduleTask.S5_IsActive);

			touch2.SendSettings.IsImmediate = true;

			Factory.Save();
			AssertEquals("Batch Scheduling option not selected - do not run schedule task", false, touch2.SendSettings.ScheduleTask.S5_IsActive);
		}

		#endregion

		#region Delete

		public void TestDelete_DeletesRelatedBusinessObjects()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			settings.IsBatchSchedule = true;

			Factory.Save();
			var scheduleTask = settings.ScheduleTask;
			AssertNotNull("Precondition", scheduleTask);

			settings.Delete();

			AssertEquals(true, scheduleTask.IsDeleted);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			return settings;
		}
	}
}
