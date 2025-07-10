using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSendScheduleTask))]
	sealed class GlbCompanyCampaignSendScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDbHits()
		{
			TestDateAttribute.UseUNLOCO = true;

			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";
			var ausydContact2 = ausydOrg.Contacts.AddNew();
			ausydContact2.OC_ContactName = "ausydContact2";

			var ussfoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ussfoOrg.OH_RL_NKClosestPort = "USSFO";
			var ussfoContact = ussfoOrg.Contacts.AddNew();
			ussfoContact.OC_ContactName = "ussfoContact";

			var gblonOrg = Factory.NewWithValidTestData<OrgHeader>();
			gblonOrg.OH_RL_NKClosestPort = "GBLON";
			var gblonContact = gblonOrg.Contacts.AddNew();
			gblonContact.OC_ContactName = "gblonContact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "JPTYO";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			touch.G0_LastSentBatchNumber = 2;
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			masterCampaign.AllTouches.Add(touch);

			var ausydItem1 = AddQueuedForBatchScehduleItem(touch);
			ausydItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem1.G8_RecipientID = ausydContact1.PK;
			ausydItem1.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 9, 0, 0);
			ausydItem1.G8_BatchNumber = 3;

			var ausydItem2 = AddQueuedForBatchScehduleItem(touch);
			ausydItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem2.G8_RecipientID = ausydContact2.PK;

			var ussfoItem = AddQueuedForBatchScehduleItem(touch);
			ussfoItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ussfoItem.G8_RecipientID = ussfoContact.PK;

			var gblonItem = AddQueuedForBatchScehduleItem(touch);
			gblonItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			gblonItem.G8_RecipientID = gblonContact.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 0, 0));
			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 18, 0, 0);

			Factory.Save();

			scheduleTask.Run();

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbCompanyCampaignItemSchema.Constants.TableName, 2);
			expectedDbHits.Add(GlbCompanyCampaignSchema.Constants.TableName, 1);
			expectedDbHits.Add(GlbCompanyCampaignDripMarketingSchema.Constants.TableName, 1);
			expectedDbHits.Add(GlbStaffSchema.Constants.TableName, 1);
			expectedDbHits.Add(ProcessTasksSchema.Constants.TableName, 1);
			expectedDbHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewCampaignContactSchema.Constants.TableName, 1);
			expectedDbHits.Add(GlbBranchSchema.Constants.TableName, 1);

			AssertDbHits(expectedDbHits, (scheduleTask as GlbCompanyCampaignSendScheduleTaskNotifyAndSaveOverride).LastFactory);
		}

		#region Test Classes

		class GlbCompanyCampaignForTest : GlbCompanyCampaign
		{
			public GlbCompanyCampaignForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override GlbCompanyCampaignSendSettings GetNewSettings()
			{
				return Factory.New<GlbCompanyCampaignSendSettingsForTest>();
			}
		}

		class GlbCompanyCampaignSendSettingsForTest : GlbCompanyCampaignSendSettings
		{
			public GlbCompanyCampaignSendSettingsForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override GlbCompanyCampaignSendScheduleTask LoadScheduleTask(ZQuery scheduleTaskQuery)
			{
				return Factory.LoadTop1<GlbCompanyCampaignSendScheduleTaskNotifyAndSaveOverride>(scheduleTaskQuery);
			}
		}

		class GlbCompanyCampaignSendScheduleTaskNotifyAndSaveOverride : GlbCompanyCampaignSendScheduleTask
		{
			public BusinessObjectFactory LastFactory;

			public GlbCompanyCampaignSendScheduleTaskNotifyAndSaveOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void NotifyAndSave(INotifications notifications, ZGuid touchPK, int countProcessed, BusinessObjectFactory factory)
			{
				LastFactory = factory;
				base.NotifyAndSave(notifications, touchPK, countProcessed, factory);
			}
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var scheduleTask = Factory.New<GlbCompanyCampaignSendScheduleTask>();
			AssertEquals("S5_ParentTableCode", GlbCompanyCampaignSendSettingsSchema.Constants.Prefix, scheduleTask.S5_ParentTableCode);
			AssertEquals("S5_GB", ZGuid.Empty, scheduleTask.S5_GB);
		}

		#endregion

		#region Run

		[TestDate(2001, 1, 1)]
		public void TestRun_SameTimeNextBatch()
		{
			TestDateAttribute.UseUNLOCO = true;

			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";
			var ausydContact2 = ausydOrg.Contacts.AddNew();
			ausydContact2.OC_ContactName = "ausydContact2";

			var ussfoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ussfoOrg.OH_RL_NKClosestPort = "USSFO";
			var ussfoContact = ussfoOrg.Contacts.AddNew();
			ussfoContact.OC_ContactName = "ussfoContact";

			var gblonOrg = Factory.NewWithValidTestData<OrgHeader>();
			gblonOrg.OH_RL_NKClosestPort = "GBLON";
			var gblonContact = gblonOrg.Contacts.AddNew();
			gblonContact.OC_ContactName = "gblonContact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "JPTYO";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_LastSentBatchNumber = 3;
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			masterCampaign.AllTouches.Add(touch);

			var ausydItem1 = AddQueuedForBatchScehduleItem(touch);
			ausydItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem1.G8_RecipientID = ausydContact1.PK;
			ausydItem1.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2, 9, 0, 0);
			ausydItem1.G8_BatchNumber = 3;

			var ausydItem2 = AddQueuedForBatchScehduleItem(touch);
			ausydItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem2.G8_RecipientID = ausydContact2.PK;

			var ussfoItem = AddQueuedForBatchScehduleItem(touch);
			ussfoItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ussfoItem.G8_RecipientID = ussfoContact.PK;

			var gblonItem = AddQueuedForBatchScehduleItem(touch);
			gblonItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			gblonItem.G8_RecipientID = gblonContact.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 0, 0));
			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 18, 0, 0);

			Factory.Save();

			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			ausydItem1 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem1.PK);
			ausydItem2 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem2.PK);
			ussfoItem = anotherFactory.Load<GlbCompanyCampaignItem>(ussfoItem.PK);
			gblonItem = anotherFactory.Load<GlbCompanyCampaignItem>(gblonItem.PK);
			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);

			CombineAssertions("G8_ScheduleTimeUtc", () =>
			{
				AssertEquals("ausydItem1", new ZDateTime(2002, 2, 2, 9, 0, 0), ausydItem1.G8_ScheduleTimeUtc);
				AssertEquals("ausydItem2", new ZDateTime(2002, 2, 2, 9, 0, 0), ausydItem2.G8_ScheduleTimeUtc);
				AssertEquals("ussfoItem", new ZDateTime(2002, 2, 2, 9, 0, 0), ussfoItem.G8_ScheduleTimeUtc);
				AssertEquals("gblonItem", new ZDateTime(2002, 2, 2, 9, 0, 0), gblonItem.G8_ScheduleTimeUtc);
			});

			CombineAssertions("G8_BatchNumber", () =>
			{
				AssertEquals("ausydItem1", 4, ausydItem1.G8_BatchNumber);
				AssertEquals("ausydItem2", 4, ausydItem2.G8_BatchNumber);
				AssertEquals("ussfoItem", 4, ussfoItem.G8_BatchNumber);
				AssertEquals("gblonItem", 4, gblonItem.G8_BatchNumber);
			});
		}

		[TestDate(2001, 1, 1, 3, 0, 0)]
		public void TestRun_RecipientLocalTime_LastTimeZone()
		{
			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";

			var nzaklOrg = Factory.NewWithValidTestData<OrgHeader>();
			nzaklOrg.OH_RL_NKClosestPort = "NZAKL";
			var nzaklContact1 = nzaklOrg.Contacts.AddNew();
			nzaklContact1.OC_ContactName = "nzaklContact1";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.AllTouches.Add(touch);
			touch.G0_EmailSenderOption = "EML";
			touch.G0_RL_NKEmailSenderUNLOCO = "AUSYD";

			var ausydItem = AddQueuedForBatchScehduleItem(touch);
			ausydItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem.G8_RecipientID = ausydContact1.PK;
			var nzaklItem = AddQueuedForBatchScehduleItem(touch);
			nzaklItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			nzaklItem.G8_RecipientID = nzaklContact1.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = false;
			settings.GSC_IsRecipientLocalTime = true;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(15, 0, 0));
			settings.GSC_ContactLimitEachBatch = 3;

			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2001, 1, 1, 15, 0, 0);

			Factory.Save();

			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			ausydItem = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem.PK);
			nzaklItem = anotherFactory.Load<GlbCompanyCampaignItem>(nzaklItem.PK);
			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);

			CombineAssertions("G8_ScheduleTimeUtc", () =>
			{
				AssertEquals("ausydItem1", new ZDateTime(2001, 1, 1), ausydItem.G8_ScheduleTimeUtc.Date);
				AssertEquals("nzaklItem", new ZDateTime(2001, 1, 1), nzaklItem.G8_ScheduleTimeUtc.Date);
			});

			CombineAssertions("G8_BatchNumber", () =>
			{
				AssertEquals("ausydItem1", 1, ausydItem.G8_BatchNumber);
				AssertEquals("nzaklItem", 1, nzaklItem.G8_BatchNumber);
			});

			AssertEquals(1, touch.G0_LastSentBatchNumber);
		}

		[TestDate(2001, 1, 1, 4, 0, 0)]
		public void TestRun_RecipientLocalTime_DoesNotSendPast()
		{
			TestDateAttribute.UseUNLOCO = true;
			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";

			var nzaklOrg = Factory.NewWithValidTestData<OrgHeader>();
			nzaklOrg.OH_RL_NKClosestPort = "NZAKL";
			var nzaklContact1 = nzaklOrg.Contacts.AddNew();
			nzaklContact1.OC_ContactName = "nzaklContact1";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.AllTouches.Add(touch);
			touch.G0_EmailSenderOption = "EML";
			touch.G0_RL_NKEmailSenderUNLOCO = "AUSYD";

			var ausydItem = AddQueuedForBatchScehduleItem(touch);
			ausydItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem.G8_RecipientID = ausydContact1.PK;
			var nzaklItem = AddQueuedForBatchScehduleItem(touch);
			nzaklItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			nzaklItem.G8_RecipientID = nzaklContact1.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = false;
			settings.GSC_IsRecipientLocalTime = true;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(15, 0, 0));
			settings.GSC_ContactLimitEachBatch = 3;

			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2001, 1, 1, 15, 0, 0);

			Factory.Save();

			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			ausydItem = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem.PK);
			nzaklItem = anotherFactory.Load<GlbCompanyCampaignItem>(nzaklItem.PK);
			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);

			CombineAssertions("G8_ScheduleTimeUtc", () =>
			{
				AssertEquals("ausydItem1", new ZDateTime(2001, 1, 1), ausydItem.G8_ScheduleTimeUtc.Date);
				AssertEquals("nzaklItem", new ZDateTime(2001, 1, 2), nzaklItem.G8_ScheduleTimeUtc.Date);
			});

			CombineAssertions("G8_BatchNumber", () =>
			{
				AssertEquals("ausydItem1", 1, ausydItem.G8_BatchNumber);
				AssertEquals("nzaklItem", 1, nzaklItem.G8_BatchNumber);
			});

			AssertEquals(1, touch.G0_LastSentBatchNumber);
		}

		[TestDate(2001, 1, 1)]
		public void TestRun_RecipientLocalTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";
			var ausydContact2 = ausydOrg.Contacts.AddNew();
			ausydContact2.OC_ContactName = "ausydContact2";

			var ussfoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ussfoOrg.OH_RL_NKClosestPort = "USSFO";
			var ussfoContact = ussfoOrg.Contacts.AddNew();
			ussfoContact.OC_ContactName = "ussfoContact";

			var gblonOrg = Factory.NewWithValidTestData<OrgHeader>();
			gblonOrg.OH_RL_NKClosestPort = "GBLON";
			var gblonContact = gblonOrg.Contacts.AddNew();
			gblonContact.OC_ContactName = "gblonContact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_LastSentBatchNumber = 2;
			masterCampaign.AllTouches.Add(touch);
			touch.G0_EmailSenderOption = "EML";
			touch.G0_RL_NKEmailSenderUNLOCO = "AUSYD";

			var ausydItem1 = AddQueuedForBatchScehduleItem(touch);
			ausydItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem1.G8_RecipientID = ausydContact1.PK;
			var ausydItem2 = AddQueuedForBatchScehduleItem(touch);
			ausydItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem2.G8_RecipientID = ausydContact2.PK;

			var ussfoItem = AddQueuedForBatchScehduleItem(touch);
			ussfoItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ussfoItem.G8_RecipientID = ussfoContact.PK;

			var gblonItem = AddQueuedForBatchScehduleItem(touch);
			gblonItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			gblonItem.G8_RecipientID = gblonContact.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = false;
			settings.GSC_IsRecipientLocalTime = true;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 0, 0));
			settings.GSC_ContactLimitEachBatch = 3;
			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 18, 0, 0);

			Factory.Save();

			SetSystemLastEditTimeUtc(ausydItem1, new DateTime(2000, 12, 1));
			SetSystemLastEditTimeUtc(ausydItem2, new DateTime(2000, 12, 10));
			SetSystemLastEditTimeUtc(ussfoItem, new DateTime(2000, 12, 2));
			SetSystemLastEditTimeUtc(gblonItem, new DateTime(2000, 12, 3));
			{
				scheduleTask.Run();

				var anotherFactory = new BusinessObjectFactory();
				ausydItem1 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem1.PK);
				ausydItem2 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem2.PK);
				ussfoItem = anotherFactory.Load<GlbCompanyCampaignItem>(ussfoItem.PK);
				gblonItem = anotherFactory.Load<GlbCompanyCampaignItem>(gblonItem.PK);
				touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);

				CombineAssertions("G8_ScheduleTimeUtc", () =>
				{
					AssertEquals("ausydItem1", new ZDateTime(2002, 2, 2, 7, 0, 0), ausydItem1.G8_ScheduleTimeUtc);
					AssertEquals("ausydItem2", ZDateTime.Empty, ausydItem2.G8_ScheduleTimeUtc);
					AssertEquals("ussfoItem", new ZDateTime(2002, 2, 3, 2, 0, 0), ussfoItem.G8_ScheduleTimeUtc);
					AssertEquals("gblonItem", new ZDateTime(2002, 2, 2, 18, 0, 0), gblonItem.G8_ScheduleTimeUtc);
				});

				CombineAssertions("G8_BatchNumber", () =>
				{
					AssertEquals("ausydItem1", 3, ausydItem1.G8_BatchNumber);
					AssertEquals("ausydItem2", 0, ausydItem2.G8_BatchNumber);
					AssertEquals("ussfoItem", 3, ussfoItem.G8_BatchNumber);
					AssertEquals("gblonItem", 3, gblonItem.G8_BatchNumber);
				});

				AssertEquals(3, touch.G0_LastSentBatchNumber);
				AssertEquals("Should remain active since there are still contacts remaining that require scheduling", true, scheduleTask.S5_IsActive);
			}

			{
				scheduleTask.Run();
				AssertEquals("Should not bump batch number since none were scheduled", 4, touch.G0_LastSentBatchNumber);
			}
		}

		[TestDate(2001, 1, 1)]
		public void TestRun_SenderLocalTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			var ausydOrg = Factory.NewWithValidTestData<OrgHeader>();
			ausydOrg.OH_RL_NKClosestPort = "AUSYD";
			var ausydContact1 = ausydOrg.Contacts.AddNew();
			ausydContact1.OC_ContactName = "ausydContact1";
			var ausydContact2 = ausydOrg.Contacts.AddNew();
			ausydContact2.OC_ContactName = "ausydContact2";

			var ussfoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ussfoOrg.OH_RL_NKClosestPort = "USSFO";
			var ussfoContact = ussfoOrg.Contacts.AddNew();
			ussfoContact.OC_ContactName = "ussfoContact";

			var gblonOrg = Factory.NewWithValidTestData<OrgHeader>();
			gblonOrg.OH_RL_NKClosestPort = "GBLON";
			var gblonContact = gblonOrg.Contacts.AddNew();
			gblonContact.OC_ContactName = "gblonContact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "JPTYO";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_LastSentBatchNumber = 2;
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			masterCampaign.AllTouches.Add(touch);

			var ausydItem1 = AddQueuedForBatchScehduleItem(touch);
			ausydItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem1.G8_RecipientID = ausydContact1.PK;
			var ausydItem2 = AddQueuedForBatchScehduleItem(touch);
			ausydItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ausydItem2.G8_RecipientID = ausydContact2.PK;

			var ussfoItem = AddQueuedForBatchScehduleItem(touch);
			ussfoItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ussfoItem.G8_RecipientID = ussfoContact.PK;

			var gblonItem = AddQueuedForBatchScehduleItem(touch);
			gblonItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			gblonItem.G8_RecipientID = gblonContact.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 0, 0));
			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 18, 0, 0);

			Factory.Save();

			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			ausydItem1 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem1.PK);
			ausydItem2 = anotherFactory.Load<GlbCompanyCampaignItem>(ausydItem2.PK);
			ussfoItem = anotherFactory.Load<GlbCompanyCampaignItem>(ussfoItem.PK);
			gblonItem = anotherFactory.Load<GlbCompanyCampaignItem>(gblonItem.PK);
			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);

			CombineAssertions("G8_ScheduleTimeUtc", () =>
			{
				AssertEquals("ausydItem1", new ZDateTime(2002, 2, 2, 9, 0, 0), ausydItem1.G8_ScheduleTimeUtc);
				AssertEquals("ausydItem2", new ZDateTime(2002, 2, 2, 9, 0, 0), ausydItem2.G8_ScheduleTimeUtc);
				AssertEquals("ussfoItem", new ZDateTime(2002, 2, 2, 9, 0, 0), ussfoItem.G8_ScheduleTimeUtc);
				AssertEquals("gblonItem", new ZDateTime(2002, 2, 2, 9, 0, 0), gblonItem.G8_ScheduleTimeUtc);
			});

			CombineAssertions("G8_BatchNumber", () =>
			{
				AssertEquals("ausydItem1", 3, ausydItem1.G8_BatchNumber);
				AssertEquals("ausydItem2", 3, ausydItem2.G8_BatchNumber);
				AssertEquals("ussfoItem", 3, ussfoItem.G8_BatchNumber);
				AssertEquals("gblonItem", 3, gblonItem.G8_BatchNumber);
			});

			AssertEquals(3, touch.G0_LastSentBatchNumber);
		}

		[TestDate(2001, 1, 1)]
		public void TestRun_WithGroup()
		{
			TestDateAttribute.UseUNLOCO = true;

			var ussfoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ussfoOrg.OH_RL_NKClosestPort = "USSFO";
			var ussfoContact = ussfoOrg.Contacts.AddNew();
			ussfoContact.OC_ContactName = "ussfoContact";

			var gblonOrg = Factory.NewWithValidTestData<OrgHeader>();
			gblonOrg.OH_RL_NKClosestPort = "GBLON";
			var gblonContact = gblonOrg.Contacts.AddNew();
			gblonContact.OC_ContactName = "gblonContact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "JPTYO";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_LastSentBatchNumber = 2;
			touch1.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch1.CurrentGroupColor = 10;

			var touch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2.G0_LastSentBatchNumber = 1;
			touch2.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2.CurrentGroupColor = 10;

			masterCampaign.AllTouches.Add(touch1);
			masterCampaign.AllTouches.Add(touch2);

			var ussfoItem = AddQueuedForBatchScehduleItem(touch1);
			ussfoItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ussfoItem.G8_RecipientID = ussfoContact.PK;
			var gblonItem = AddQueuedForBatchScehduleItem(touch2);
			gblonItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			gblonItem.G8_RecipientID = gblonContact.PK;

			Factory.Save();

			var settings = touch1.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.MinSmallDateTimeValue.Add(new TimeSpan(18, 0, 0));

			Factory.Save();

			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = new ZDateTime(2002, 2, 2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 18, 0, 0);

			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			ussfoItem = anotherFactory.Load<GlbCompanyCampaignItem>(ussfoItem.PK);
			gblonItem = anotherFactory.Load<GlbCompanyCampaignItem>(gblonItem.PK);

			CombineAssertions("G8_ScheduleTimeUtc", () =>
			{
				AssertEquals("ussfoItem", new ZDateTime(2002, 2, 2, 9, 0, 0), ussfoItem.G8_ScheduleTimeUtc);
				AssertEquals("gblonItem", new ZDateTime(2002, 2, 2, 9, 0, 0), gblonItem.G8_ScheduleTimeUtc);
			});

			CombineAssertions("G8_BatchNumber", () =>
			{
				AssertEquals("ussfoItem", 3, ussfoItem.G8_BatchNumber);
				AssertEquals("gblonItem", 2, gblonItem.G8_BatchNumber);
			});

			touch1 = anotherFactory.Load<GlbCompanyCampaign>(touch1.PK);
			touch2 = anotherFactory.Load<GlbCompanyCampaign>(touch2.PK);
			AssertEquals(3, touch1.G0_LastSentBatchNumber);
			AssertEquals(2, touch2.G0_LastSentBatchNumber);
		}

		public void TestRun_BatchCampaign_ContactLimitEachBatchChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "Contact3";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			touch.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			masterCampaign.AllTouches.Add(touch);

			var item1 = AddQueuedForBatchScehduleItem(touch);
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact.PK;
			var item2 = AddQueuedForBatchScehduleItem(touch);
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			var item3 = AddQueuedForBatchScehduleItem(touch);
			item3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3.G8_RecipientID = contact3.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.Now.AddDays(1);
			settings.GSC_ContactLimitEachBatch = 2;

			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = ZDateTime.Now.AddDays(-2);
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = ZDateTime.Now.AddDays(-1);

			Factory.Save();
			scheduleTask.Run();

			var anotherFactory = new BusinessObjectFactory();
			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);
			var touchItemsSent = touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(3, touchItemsSent.Count);
			AssertEquals(3, touchItemsSent.Count(i => i.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE));

			AssertEquals(2, touchItemsSent.Count(i => !i.G8_ScheduleTimeUtc.IsEmpty));
			AssertEquals(1, touchItemsSent.Count(i => i.G8_ScheduleTimeUtc.IsEmpty));

			AssertEquals(2, touchItemsSent.Count(i => i.G8_GS_NKSender == staff.GS_Code));
			AssertEquals(1, touchItemsSent.Count(i => i.G8_GS_NKSender.IsEmpty));

			settings.GSC_ScheduleTime = ZDateTime.Now.AddDays(2);
			settings.GSC_ContactLimitEachBatch = 1;

			Factory.Save();
			scheduleTask.Run();

			touch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);
			touchItemsSent = touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(3, touchItemsSent.Count);
			AssertEquals(3, touchItemsSent.Count(i => i.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE));

			AssertEquals(1, touchItemsSent.Count(i => !i.G8_ScheduleTimeUtc.IsEmpty));
			AssertEquals(2, touchItemsSent.Count(i => i.G8_ScheduleTimeUtc.IsEmpty));

			AssertEquals(1, touchItemsSent.Count(i => i.G8_GS_NKSender == staff.GS_Code));
			AssertEquals(2, touchItemsSent.Count(i => i.G8_GS_NKSender.IsEmpty));
		}

		public void TestRun_BatchCampaign_OpportunityCreation()
		{
			var contact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact.OC_ContactName = "AA";

			var contact2 = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact2.OC_ContactName = "BB";

			var contact3 = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact3.OC_ContactName = "CC";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.HtmlDocumentBlob = ZBlob.FromAscii("Test Html");
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.Empty;
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignItem2 = touch.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.Empty;
			campaignItem2.G8_RecipientTableCode = contact2.TablePrefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var campaignItem3 = touch.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.Empty;
			campaignItem3.G8_RecipientTableCode = contact3.TablePrefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			settings.GSC_IsSenderLocalTime = true;
			settings.GSC_IsRecipientLocalTime = false;
			settings.GSC_ScheduleTime = ZDateTime.Now;
			settings.GSC_ContactLimitEachBatch = 2;

			var scheduleTask = settings.ScheduleTask;
			scheduleTask.Recurrence.DailyDay = true;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.Recurrence.StartDateLocal = ZDateTime.Now;
			scheduleTask.Recurrence.NextScheduledPrintRunTimeLocal = ZDateTime.Now;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var opportunityCreationTouch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);
			var opportunityCreationTouchCampaignItems = opportunityCreationTouch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(3, opportunityCreationTouchCampaignItems.Count);
			AssertEquals(0, opportunityCreationTouchCampaignItems.Count(i => !i.G8_ScheduleTimeUtc.IsEmpty));

			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_EmailSenderName.IsEmpty));
			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_SenderEmailAddress.IsEmpty));
			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_GS_NKSender.IsEmpty));

			scheduleTask.Run();

			opportunityCreationTouch = anotherFactory.Load<GlbCompanyCampaign>(touch.PK);
			opportunityCreationTouchCampaignItems = opportunityCreationTouch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(3, opportunityCreationTouchCampaignItems.Count);
			AssertEquals(settings.GSC_ContactLimitEachBatch, opportunityCreationTouchCampaignItems.Count(i => !i.G8_ScheduleTimeUtc.IsEmpty));

			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_EmailSenderName.IsEmpty));
			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_SenderEmailAddress.IsEmpty));
			AssertEquals(3, opportunityCreationTouchCampaignItems.Count(i => i.G8_GS_NKSender.IsEmpty));
		}

		public void TestRunOpportunityCreation_EmptySender()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = masterCampaign.PK;
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;

			var settings = touch.SendSettings;
			settings.IsDelayed = true;
			settings.GSC_ScheduleTime = ZDateTime.Now;
			Factory.Save();

			settings.ScheduleTask.Run();

			var assertFactory = new BusinessObjectFactory();
			var opportunityCreationTouch = assertFactory.Load<GlbCompanyCampaign>(touch.PK);
			var opportunityCreationTouchCampaignItems = opportunityCreationTouch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(1, opportunityCreationTouchCampaignItems.Count(i => i.G8_EmailSenderName.IsEmpty));
			AssertEquals(1, opportunityCreationTouchCampaignItems.Count(i => i.G8_SenderEmailAddress.IsEmpty));
			AssertEquals(1, opportunityCreationTouchCampaignItems.Count(i => i.G8_GS_NKSender.IsEmpty));
		}

		#endregion

		#region Properties

		[TestDate(2016, 7, 10, 13, 34, 48)]
		public void TestShouldUpdateNextScheduleDate()
		{
			var task = Factory.New<GlbCompanyCampaignSendScheduleTaskForTest>();

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 9, 23, 0, 0);
			AssertEquals(true, task.ShouldUpdateNextScheduleDate_Exposed());

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 10, 1, 0, 0);
			AssertEquals(false, task.ShouldUpdateNextScheduleDate_Exposed());

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 10, 2, 0, 0);
			AssertEquals(false, task.ShouldUpdateNextScheduleDate_Exposed());

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 10, 3, 0, 0);
			AssertEquals(false, task.ShouldUpdateNextScheduleDate_Exposed());

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 10, 7, 0, 0);
			AssertEquals(false, task.ShouldUpdateNextScheduleDate_Exposed());

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2016, 7, 10, 14, 0, 0);
			AssertEquals(false, task.ShouldUpdateNextScheduleDate_Exposed());
		}

		[TestDate(2016, 2, 1)]
		public void TestShouldPreventNextRunTimeBounceBack()
		{
			var task = Factory.NewWithValidTestData<GlbCompanyCampaignSendScheduleTaskForTest>();
			task.Recurrence.DailyDay = true;
			task.Recurrence.StartDateLocal = new ZDateTime(2016, 1, 1);
			task.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 2, 2);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 2, 2, 0, 0, 0), task.Recurrence.NextScheduledPrintRunTimeLocal);

			task.ShouldPreventNextRunTimeBounceBack = false;
			task.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 2, 5, 13, 0, 0); //change date & time
			Factory.Save();
			AssertEquals("bouced back", new ZDateTime(2016, 2, 1, 13, 0, 0), task.Recurrence.NextScheduledPrintRunTimeLocal);

			task.ShouldPreventNextRunTimeBounceBack = true;
			task.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 2, 3, 14, 0, 0);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 2, 3, 14, 0, 0), task.Recurrence.NextScheduledPrintRunTimeLocal);
		}

		public void TestUtcOverride()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_G0_Master = master.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			var scheduleTask = settings.ScheduleTask;

			Assert(scheduleTask.UtcOffsetOverride.HasValue);
			AssertEquals(14, (int)scheduleTask.UtcOffsetOverride.Value.TotalHours);
		}

		public void TestNextScheduledPrintRunTimeLocal_ReadOnly()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_G0_Master = master.PK;

			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			var scheduleTask = settings.ScheduleTask;

			AssertEquals(false, scheduleTask.Recurrence.NextScheduledPrintRunTimeLocalInfo.ReadOnly);
		}

		#endregion

		#region Save

		public void TestSave_ShouldSetDailyStartTime()
		{
			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendScheduleTask>();
			settings.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 2, 2, 2);
			Factory.Save();

			AssertEquals(new TimeSpan(2, 2, 2), settings.S5_DailyStartTime.TimeOfDay);
		}

		#endregion

		static GlbCompanyCampaignItem AddQueuedForBatchScehduleItem(GlbCompanyCampaign campaign)
		{
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item.G8_ScheduleTimeUtc = ZDateTime.Empty;
			return item;
		}

		void SetSystemLastEditTimeUtc(GlbCompanyCampaignItem campaignItem, DateTime value)
		{
			using (var command = TestConnection.Command(@"
UPDATE dbo.GlbCompanyCampaignItem
SET G8_SystemLastEditTimeUtc = @Value, G8_SystemLastEditUser = 'E'
WHERE G8_PK = @G8_PK"))
			{
				command.AddParameter("@Value", SqlDbType.SmallDateTime, value);
				command.AddParameter("@G8_PK", SqlDbType.UniqueIdentifier, campaignItem.PK.ToGuid());

				command.ExecuteNonQuery();
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<GlbCompanyCampaignSendScheduleTask>();
		}

		class GlbCompanyCampaignSendScheduleTaskForTest : GlbCompanyCampaignSendScheduleTask
		{
			public GlbCompanyCampaignSendScheduleTaskForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ShouldUpdateNextScheduleDate_Exposed()
			{
				return ShouldUpdateNextScheduleDate;
			}
		}
	}
}
