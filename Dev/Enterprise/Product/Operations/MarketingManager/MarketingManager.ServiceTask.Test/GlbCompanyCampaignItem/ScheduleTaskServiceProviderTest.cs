using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	[TestedType(typeof(CampaignItemScheduleProcessorServiceTask))]
	sealed class ScheduleTaskServiceProviderTest : ServiceTaskTestCase<CampaignItemScheduleProcessorServiceTask>
	{
		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestDoesNotThrow()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			AssertNoExceptionThrown(() => { RunTaskScheduleWithAnyBranchContext(serviceTask); });
		}

		public void TestDbHits()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTaskForTest();
			serviceTask.ServiceLogger = TestLogger;

			var categoryCollection = new CodeDescriptionBoolCollection();
			categoryCollection.Add("***", (NoResString)"Category");
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryCollection);

			var typeCollection = new CodeDescriptionBoolCollection();
			typeCollection.Add("***", (NoResString)"Type");
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeCollection);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.G0_Category = "***";
			touch1.G0_Type = "***";
			touch1.G0_EstimatedStartedDate = DateTime.Today;
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			touch2a.G0_Category = "***";
			touch2a.G0_Type = "***";
			touch2a.G0_EstimatedStartedDate = DateTime.Today;
			master.AllTouches.Add(touch2a);

			var touch3a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3a.G0_CampaignName = "Test Campaign3a";
			touch3a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "B";
			touch3a.G0_G0_Master = master.PK;
			touch3a.G0_Category = "***";
			touch3a.G0_Type = "***";
			touch3a.G0_EstimatedStartedDate = DateTime.Today;
			master.AllTouches.Add(touch3a);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_IsCheckTransitionRequired = true;

			var campaignItem2 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_IsCheckTransitionRequired = true;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ GlbCompanyCampaignItemSchema.Constants.TableName, 4 },
				{ GlbCompanyCampaignSchema.Constants.TableName, 6 },
				{ GlbCompanyCampaignDripMarketingSchema.Constants.TableName, 4 },
				{ GlbCompanyCampaignSendSettingsSchema.Constants.TableName, 1 },
				{ RefCurrencySchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 1 },
				{ StmModuleFilterUserDataSchema.Constants.TableName, 1 },
				{ ViewCampaignContactSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmScheduleTaskSchema.Constants.TableName, 4 },
				{ StmScheduleTaskRecipientSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedDbHits, serviceTask.LastFactory.Value);
		}

		[TestDate(2021, 9, 1)]
		public void TestRunTask()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";
			contact1.OC_Gender = "M";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign, Factory);
			campaign.G0_CampaignName = "Test Campaign";
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(TrackingStatusCodes.Codes.UNV, campaignItem1.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			contact1.OC_Email = "";
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);
			AssertEquals(TrackingStatusCodes.Codes.QUE, campaignItem1.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "Campaign: <210901_***_***_Test Campaign> failed to be sent to contact: AA", null),
				new LogForTest(LogType.Debug, "0 emails successfully sent", null),
				new LogForTest(LogType.Debug, "1 emails failed to be sent", null),
				new LogForTest(LogType.Warning, "Scheduled Campaign Item Email Processor task completed with warnings", null));
		}

		public void TestRunTaskOpportunityCreation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "CC@gmail.com";
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

			var campaignItem1 = touch.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem1.G8_RecipientTableCode = contact1.TablePrefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			campaignItem2.G8_RecipientTableCode = contact2.TablePrefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var campaignItem3 = touch.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			campaignItem3.G8_RecipientTableCode = contact3.TablePrefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_IsSuspended = true;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var newFactory = new BusinessObjectFactory();
			campaignItem1 = newFactory.Load<GlbCompanyCampaignItem>(campaignItem1.PK);
			campaignItem2 = newFactory.Load<GlbCompanyCampaignItem>(campaignItem2.PK);
			campaignItem3 = newFactory.Load<GlbCompanyCampaignItem>(campaignItem3.PK);
			masterCampaign = newFactory.Load<GlbCompanyCampaign>(masterCampaign.PK);
			touch = newFactory.Load<GlbCompanyCampaign>(touch.PK);

			AssertEquals("Master campaign sales relation count", 1, masterCampaign.RelatedChildActivityPivotCollection.Count());
			AssertEquals("Transition count", 3, touch.CampaignsItemsSent.Count);

			AssertEquals(campaignItem1.G8_TrackingStatus, TrackingStatusCodes.Codes.OPC);
			AssertEquals(campaignItem2.G8_TrackingStatus, TrackingStatusCodes.Codes.OPC);
			AssertEquals(campaignItem3.G8_TrackingStatus, TrackingStatusCodes.Codes.OPQ);

			var opportunity1 = newFactory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, campaignItem1.G8_RecipientID));
			var opportunity2 = newFactory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, campaignItem2.G8_RecipientID));
			var opportunity3 = newFactory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, campaignItem3.G8_RecipientID));

			AssertNotNull("Opportunity1 should have been created", opportunity1);
			AssertNotNull("Opportunity2 should have been created", opportunity2);
			AssertNull("Opportunity3 should not have been created", opportunity3);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "2 processing opportunities creation templates queued", null),
				new LogForTest(LogType.Debug, "2 opportunities creation templates were successful created", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));
		}

		public void TestRunTaskOpportunityCreation_ParentSalesRelation()
		{
			var contact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var opportunity = Factory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, campaignItem.G8_RecipientID));
			AssertEquals("Master Campaign Child Sales Relation count", 1, masterCampaign.RelatedChildActivityPivotCollection.Count());
			AssertEquals("Opportunity Parent Sales Relation count", 1, opportunity.RelatedParentActivityPivotCollection.Count());
		}

		public void TestRunTaskOpportunityCreation_IndividualSalesPerson()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "testindividualsalesperson@test.com";
			staff.GS_Code = "TIS";
			staff.GS_FullName = "Test Individual Sales Person";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

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

			var opportunityCreationTemplate = new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson,
				SalesPerson = staff.GS_Code
			};
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertOpportunityCreationTemplateData(campaignItem.PK, contact.PK, opportunityCreationTemplate, staff.GS_Code);
		}

		public void TestRunTaskOpportunityCreation_StaffAssignment()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "teststaffassignment@test.com";
			staff.GS_Code = "TAS";
			staff.GS_FullName = "Test Staff Assignment";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var orgStaffAssignments = org.StaffAssignments.AddNew();
			orgStaffAssignments.O8_Role = "SAL";
			orgStaffAssignments.O8_GS_NKPersonResponsible = staff.GS_Code;
			orgStaffAssignments.O8_GC = Env.CurrentCompanyPK;
			orgStaffAssignments.O8_OH = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

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

			var opportunityCreationTemplate = new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				OpportunityAssignment = OpportunityAssignmentList.Codes.StaffAssignment,
				StaffAssignment = "SAL"
			};
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertOpportunityCreationTemplateData(campaignItem.PK, contact.PK, opportunityCreationTemplate, staff.GS_Code);
		}

		public void TestRunTaskOpportunityCreation_MatchParentTouchSender()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "testcoordinator@test.com";
			staff.GS_Code = "TSC";
			staff.GS_FullName = "Test Coordinator";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = masterCampaign.PK;
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			touch1A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			touch1A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch1A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html");
			masterCampaign.AllTouches.Add(touch1A);

			var campaignItem1A = touch1A.CampaignsItemsSent.AddNew();
			campaignItem1A.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem1A.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem1A.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1A.G8_RecipientID = contact.PK;

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_G0_Master = masterCampaign.PK;
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplate = new OpportunityCreationTemplate(touch2A)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender
			};
			masterCampaign.AllTouches.Add(touch2A);

			var campaignItem2A = touch2A.CampaignsItemsSent.AddNew();
			campaignItem2A.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2A.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem2A.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem2A.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(campaignItem1A.PK, campaignItem2A.GetPreviousTransitionCampaignItem().PK);
			AssertOpportunityCreationTemplateData(campaignItem2A.PK, contact.PK, opportunityCreationTemplate, campaignItem1A.CompanyCampaign.G0_GS_NKCampaignCoordinator);
		}

		public void TestRunTaskOpportunityCreation_StaffPoolAssignments()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "teststaffpool1@test.com";
			staff1.GS_Code = "TS1";
			staff1.GS_FullName = "Test Staff Pool 1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "teststaffpool2@test.com";
			staff2.GS_Code = "TS2";
			staff2.GS_FullName = "Test Staff Pool 2";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "CC@gmail.com";
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

			var poolItem1 = touch.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = staff1.GS_Code;
			poolItem1.GCP_SendRatio = 2;

			var poolItem2 = touch.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = staff2.GS_Code;
			poolItem2.GCP_SendRatio = 1;

			_ = new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				StaffAssignment = "SAL",
				OpportunityAssignment = OpportunityAssignmentList.Codes.StaffPoolAssignments
			};
			masterCampaign.AllTouches.Add(touch);

			var campaignItem1 = touch.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem1.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem1.G8_RecipientID = contact.PK;

			var campaignItem2 = touch.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem2.G8_RecipientTableCode = contact2.TablePrefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var campaignItem3 = touch.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem3.G8_RecipientTableCode = contact3.TablePrefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var opportunitiesCreated = Factory.Load<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_G0, touch.PK)).ToList();
			AssertEquals(touch.CampaignsItemsSent.Count, opportunitiesCreated.Count);

			AssertEquals(poolItem1.GCP_SendRatio, opportunitiesCreated.Count(o => o.P8_GS_NKPrimarySalesPerson == poolItem1.GCP_GS_NKSender));
			AssertEquals(poolItem2.GCP_SendRatio, opportunitiesCreated.Count(o => o.P8_GS_NKPrimarySalesPerson == poolItem2.GCP_GS_NKSender));
		}

		void AssertOpportunityCreationTemplateData(ZGuid campaignItemPk, ZGuid contactPk, OpportunityCreationTemplate opportunityCreationTemplate, ZString primarySalesPerson)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignItem = newFactory.Load<GlbCompanyCampaignItem>(campaignItemPk);
			var orgOpportunity = newFactory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, contactPk));

			AssertEquals(campaignItem.OrgPK, orgOpportunity.P8_OH);
			AssertEquals(campaignItem.G8_RecipientID, orgOpportunity.P8_OC);
			AssertEquals(campaignItem.G8_G0, orgOpportunity.P8_G0);
			AssertEquals(campaignItem.CompanyCampaign.G0_GC, orgOpportunity.P8_GC);
			AssertEquals(opportunityCreationTemplate.PackageType, orgOpportunity.P8_PackageType);
			AssertEquals(opportunityCreationTemplate.OpportunityType, orgOpportunity.P8_OpportunityType);
			AssertEquals(opportunityCreationTemplate.OpportunityDescription, orgOpportunity.P8_OpportunityDescription);
			AssertEquals(opportunityCreationTemplate.OpportunityStatus, orgOpportunity.P8_Status);
			AssertEquals(opportunityCreationTemplate.OpportunityStage, orgOpportunity.P8_Stage);
			AssertEquals(opportunityCreationTemplate.Source, orgOpportunity.P8_Source);
			AssertEquals(opportunityCreationTemplate.ActiveSourceDetails, orgOpportunity.P8_SourceDetails);
			AssertEquals(opportunityCreationTemplate.OpportunityNotes, orgOpportunity.P8_OpportunityNotes);
			AssertEquals(primarySalesPerson, orgOpportunity.P8_GS_NKPrimarySalesPerson);
		}

		public void TestRunTaskOpportunityCreationCompany()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "teststaff@test.com";
			staff.GS_Code = "TSF";
			staff.GS_FullName = "Test Staff";

			var contact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact.OC_Email = "testcontact@test.com";
			contact.OC_ContactName = "Test Contact";

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

			_ = new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				SalesPerson = staff.GS_Code,
				OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson
			};
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = TestLogger };
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var newFactory = new BusinessObjectFactory();
			var orgOpportunity = newFactory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_OC, contact.PK));
			AssertEquals(touch.G0_GC, orgOpportunity.P8_GC);
		}

		[TestDate(2021, 9, 1)]
		public void TestProcessSendingQueue_TransitionAndSchedule()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "Test Master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1, Factory);
			touch1.G0_CampaignName = "Test Touch 1";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_G0_Master = master.PK;
			touch1.SendSettings.IsImmediate = true;
			touch1.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch1);

			var touch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2, Factory);
			touch2.G0_CampaignName = "Test Touch 2";
			touch2.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2.G0_HorizontalId = 2;
			touch2.G0_G0_Master = master.PK;
			touch2.SendSettings.IsImmediate = true;
			touch2.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch2);

			var campaignItem = touch1.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			Factory.Save();
			AssertEquals("Precondition: No campaign items scheduled for second touch", 0, touch2.CampaignsItemsSent.Count);

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("Campaign item should be scheduled for second touch after successfully sending first touch", 1, touch2.CampaignsItemsSent.Count);
			AssertEquals(TrackingStatusCodes.Codes.UNV, campaignItem.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null)
			);
		}

		[TestDate(2021, 9, 1)]
		public void TestRetransfer()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1, Factory);
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.SendSettings.IsImmediate = true;
			touch1.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2a, Factory);
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			touch2a.SendSettings.IsImmediate = true;
			touch2a.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch2a);

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2b, Factory);
			touch2b.G0_CampaignName = "Test Campaign2b";
			touch2b.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			touch2b.G0_G0_Master = master.PK;
			touch2b.SendSettings.IsImmediate = true;
			touch2b.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch2b);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(TrackingStatusCodes.Codes.UNV, campaignItem2.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 campaign items re-transferred to another touch", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null)
			);

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(TrackingStatusCodes.Codes.UNV, campaignItem2.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 campaign items re-transferred to another touch", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));
		}

		[TestDate(2021, 9, 1)]
		public void TestTransitionLastFailedForSentItemsQueueForTransition()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTaskForTestWithTestFactory(Factory);
			serviceTask.ServiceLogger = TestLogger;

			var campaignCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			campaignCoordinator.GS_Code = "AAA";
			campaignCoordinator.GS_EmailAddress = "aaa@123.net";

			var campaignManager = Factory.NewWithValidTestData<GlbStaff>();
			campaignManager.GS_Code = "BBB";
			campaignManager.GS_EmailAddress = "bbb@123.net";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "CC@gmail.com";
			contact3.OC_ContactName = "CC";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.SendSettings.IsImmediate = true;
			touch1.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.G0_Category = "PRINT";
			touch2a.G0_Type = "PREAP";
			touch2a.G0_GS_NKCampaignCoordinator = "AAA";
			touch2a.G0_GS_NKCampaignManager = "BBB";
			touch2a.G0_EstimatedStartedDate = new ZDateTime(2020, 9, 7);
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			touch2a.G0_G0_Master = master.PK;
			touch2a.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_ContactName, SQLComparisonOperator.StartsWith, "D");
			master.AllTouches.Add(touch2a);

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_CampaignName = "Test Campaign2b";
			touch2b.G0_Category = "PRINT";
			touch2b.G0_Type = "PREAP";
			touch2b.G0_GS_NKCampaignCoordinator = "AAA";
			touch2b.G0_GS_NKCampaignManager = "BBB";
			touch2b.G0_EstimatedStartedDate = new ZDateTime(2020, 9, 7);
			touch2b.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "B";
			touch2b.G0_G0_Master = master.PK;
			touch2b.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_ContactName, SQLComparisonOperator.StartsWith, "E");
			master.AllTouches.Add(touch2b);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem1.G8_IsCheckTransitionRequired = true;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch1.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem2.G8_IsCheckTransitionRequired = true;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var campaignItem3 = touch1.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem3.G8_IsCheckTransitionRequired = true;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing sent items queue for transition", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing sent items queue for transition", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			touch2a.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_ContactName, SQLComparisonOperator.StartsWith, "A");
			touch2a.FilterLayoutHasChanges = true;
			touch2b.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_ContactName, SQLComparisonOperator.StartsWith, "B");
			touch2b.FilterLayoutHasChanges = true;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing sent items queue for transition", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing sent items queue for transition", null),
				new LogForTest(LogType.Debug, "2 campaign items were successfuly transferred", null),
				new LogForTest(LogType.Debug, "2: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));
		}

		class CampaignItemScheduleProcessorServiceTaskForTestWithTestFactory : CampaignItemScheduleProcessorServiceTask
		{
			public CampaignItemScheduleProcessorServiceTaskForTestWithTestFactory(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;
			protected override Lazy<BusinessObjectFactory> GetTransitionsFactory()
			{
				return new Lazy<BusinessObjectFactory>(() => factory);
			}
		}

		[TestDate(2021, 9, 1)]
		public void TestTransitionLastFailedForRetransferQueue()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var campaignCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			campaignCoordinator.GS_Code = "AAA";
			campaignCoordinator.GS_EmailAddress = "aaa@123.net";

			var campaignManager = Factory.NewWithValidTestData<GlbStaff>();
			campaignManager.GS_Code = "BBB";
			campaignManager.GS_EmailAddress = "bbb@123.net";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "CC@gmail.com";
			contact3.OC_ContactName = "CC";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.G0_Category = "PRINT";
			touch1.G0_Type = "PREAP";
			touch1.G0_GS_NKCampaignCoordinator = "AAA";
			touch1.G0_GS_NKCampaignManager = "BBB";
			touch1.G0_EstimatedStartedDate = new ZDateTime(2020, 9, 7);
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.SendSettings.IsImmediate = true;
			touch1.SendSettings.ScheduleTask?.Delete();
			master.AllTouches.Add(touch1);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_IsCheckTransitionRequired = true;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(7);
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch1.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_IsCheckTransitionRequired = true;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(8);
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var campaignItem3 = touch1.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem3.G8_IsCheckTransitionRequired = true;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(8);
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			touch1.G0_CampaignName = "Test Campaign (changed)";

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(7);

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "3: processing re-transfer queue", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			touch1.CampaignsItemsSent.Reload(true);
			foreach (GlbCompanyCampaignItem item in touch1.CampaignsItemsSent)
			{
				if (item.ContactName == "AA")
				{
					AssertEquals(item.ContactName, TrackingStatusCodes.Codes.UNV, item.G8_TrackingStatus);
					AssertEquals(item.ContactName, ZDateTime.Empty, item.G8_LastFailedTransitionUtc);
				}
				else
				{
					AssertEquals(item.ContactName, TrackingStatusCodes.Codes.QUE, item.G8_TrackingStatus);
					AssertNotEquals(item.ContactName, ZDateTime.Empty, item.G8_LastFailedTransitionUtc);
				}
			}
		}

		public void TestUnsubscribed()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2a);

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_CampaignName = "Test Campaign2b";
			touch2b.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			touch2b.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2b);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;

			var unsubscribeFromThisCampaign = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribeFromThisCampaign.GCS_Email = contact1.OC_Email;
			unsubscribeFromThisCampaign.GCS_MediaCategory = touch1.G0_Category;
			unsubscribeFromThisCampaign.GCS_MediaType = touch1.G0_Type;
			unsubscribeFromThisCampaign.GCS_IsSubscribed = false;
			unsubscribeFromThisCampaign.GCS_G0 = touch1.PK;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			campaignItem2 = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem2.PK);

			AssertNull(campaignItem2);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing send queue", null),
				new LogForTest(LogType.Debug, "1 campaign items deleted", null),
				new LogForTest(LogType.Debug, "0 emails successfully sent", null),
				new LogForTest(LogType.Debug, "0 emails failed to be sent", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));
		}

		public void TestDeleteQueued()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2a);

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_CampaignName = "Test Campaign2b";
			touch2b.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			touch2b.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2b);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;

			var unsubscribeFromThisCampaign = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribeFromThisCampaign.GCS_Email = contact1.OC_Email;
			unsubscribeFromThisCampaign.GCS_MediaCategory = touch1.G0_Category;
			unsubscribeFromThisCampaign.GCS_MediaType = touch1.G0_Type;
			unsubscribeFromThisCampaign.GCS_IsSubscribed = false;
			unsubscribeFromThisCampaign.GCS_G0 = touch1.PK;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			campaignItem2 = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem2.PK);

			AssertNotNull(campaignItem2);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));

			campaignItem2.G8_IsCheckTransitionRequired = true;

			campaignItem2.Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			campaignItem2 = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem2.PK);

			AssertNull(campaignItem2);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing re-transfer queue", null),
				new LogForTest(LogType.Debug, "1 campaign items deleted", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null));
		}

		public void TestWebCampaignUrlFormatException()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "this@daveeast.com";
			staff.Groups.Add(Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "PMG"));
			Factory.Save();

			Env.ClearAllEmailsCreated();

			var serviceTask = new CampaignItemScheduleProcessorServiceTaskForTest();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.ExceptionExpected = new UriFormatException("WebCampaignUrlFormatException - 123 456");
			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var logs = string.Join("\r\n", TestLogger.Logs.Select(x => $"[{x.Type}]-[{x.Message}]"));
			AssertEquals(@"[Information]-[Scheduled Campaign Item Email Processor task started]
[Information]-[0 scheduled tasks to run]
[Error]-[Error occurred while processing batch: WebCampaignUrlFormatException - 123 456]", logs);

			var email = Env.AllEmailsCreated.Single();
			AssertEquals("Web Campaign URL Format Exception Notification", email.Subject);
			AssertEquals("WebCampaignUrlFormatException - 123 456", email.Body);
		}

		public void TestTouchCampaignErrorLogsWithMasterCampaignDetails()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var master1 = CreateMasterAndTouchesWithoutErrors();
			var touch1 = master1.AllTouches[0];
			var touch2 = master1.AllTouches[1];
			touch2.G0_Type = "AAAA";
			touch2.G0_Stage = "*";
			touch2.G0_Category = "BBB";
			touch2.G0_EmailSubject = "(*MalformedMacro";

			var master2 = CreateMasterAndTouchesWithoutErrors();
			var touch4 = master2.AllTouches[1];
			touch4.G0_EmailSubject = "(*MalformedMacro";

			master1.G0_CampaignID = "TST00001000";
			master2.G0_CampaignID = "TST00001001";

			Factory.Save();

			touch2.Validation.ValidateAll();
			AssertEquals(4, touch2.NotificationsIncludingChildren.Count());
			AssertHasErrorContaining(touch2.G0_EmailSubjectInfo, "The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.");
			AssertHasErrorContaining(touch2.G0_TypeInfo, "Enter a valid Media Type.");
			AssertHasErrorContaining(touch2.G0_StageInfo, "Enter a valid Stage.");
			AssertHasErrorContaining(touch2.G0_CategoryInfo, "Enter a valid Media Category");

			touch4.Validation.ValidateAll();
			AssertEquals(1, touch4.NotificationsIncludingChildren.Count());
			AssertHasErrorContaining(touch4.G0_EmailSubjectInfo, "The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.");

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			var expected = new List<LogForTest> {
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "2: processing sent items queue for transition", null),
				new LogForTest(LogType.Debug, $@"210901_BBB_AAAA_Test Campaign 2_2B: Cannot Send Campaigns All errors on this campaign must be corrected before campaigns can be sent.
- Enter a valid Media Category.
- The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.
- Enter a valid Stage.
- Enter a valid Media Type.
This touch campaign can be accessed through its master campaign with ID {master1.G0_CampaignID}.", null),
				new LogForTest(LogType.Debug, $@"210901_***_***_Test Campaign 2_2B: Cannot Send Campaigns All errors on this campaign must be corrected before campaigns can be sent.
- The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.
This touch campaign can be accessed through its master campaign with ID {master2.G0_CampaignID}.", null),
				new LogForTest(LogType.Debug, "2 campaign items were successfuly transferred", null),
				new LogForTest(LogType.Warning, "Scheduled Campaign Item Email Processor task completed with warnings", null)
			}.ConvertAll(l => l.ToString()).ToArray();

			var actual = TestLogger.Logs.ConvertAll(l => l.ToString()).ToArray();

			AssertEquals(expected.Length, actual.Length);
			Assert(expected.ContainsSameElementsInAnyOrder(actual));

			GlbCompanyCampaign CreateMasterAndTouchesWithoutErrors()
			{
				var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
				master.G0_CampaignName = "master";
				master.G0_EstimatedStartedDate = new ZDateTime(2021, 9, 1);

				var touchA = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				GlbCompanyCampaignTestHelper.PopulateCampaign(touchA, Factory);
				touchA.G0_CampaignName = "Test Campaign ";
				touchA.G0_HorizontalId = 1;
				touchA.G0_VerticalId = "A";
				touchA.G0_G0_Master = master.PK;
				touchA.SourceCampaignPK = master.PK;
				touchA.G0_EstimatedStartedDate = new ZDateTime(2021, 9, 1);

				master.AllTouches.Add(touchA);

				var touchB = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				GlbCompanyCampaignTestHelper.PopulateCampaign(touchB, Factory);
				touchB.G0_CampaignName = "Test Campaign 2";
				touchB.G0_HorizontalId = 2;
				touchB.G0_VerticalId = "B";
				touchB.G0_G0_Master = master.PK;
				touchB.G0_EstimatedStartedDate = new ZDateTime(2021, 9, 1);
				touchB.SourceCampaignPK = master.PK;
				master.AllTouches.Add(touchB);

				var campaignItem1 = touchA.CampaignsItemsSent.AddNew();
				campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
				campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				campaignItem1.G8_RecipientID = contact.PK;
				campaignItem1.G8_IsCheckTransitionRequired = true;

				return master;
			}
		}

		// No nudging: Campaigns that are sent on customer defined schedules cannot be nudged
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class CampaignItemScheduleProcessorServiceTaskForTest : CampaignItemScheduleProcessorServiceTask
		{
			protected override void ProcessQueue(CancellationToken token)
			{
				if (ExceptionExpected != null)
				{
					throw ExceptionExpected;
				}

				base.ProcessQueue(token);
			}

			public Exception ExceptionExpected { set; get; }

			internal Lazy<BusinessObjectFactory> LastFactory;
			protected override Lazy<BusinessObjectFactory> GetTransitionsFactory()
			{
				LastFactory = base.GetTransitionsFactory();
				return LastFactory;
			}
		}

		public void TestTransitionFailed()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";
			master.G0_CampaignID = "TST00001000";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2a);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(TrackingStatusCodes.Codes.VER, campaignItem1.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null)
			);

			campaignItem1.G8_IsCheckTransitionRequired = true;

			Factory.Save();

			RunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals(TrackingStatusCodes.Codes.VER, campaignItem1.G8_TrackingStatus);
			AssertLogs(
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task completed", null),
				new LogForTest(LogType.Information, "Scheduled Campaign Item Email Processor task started", null),
				new LogForTest(LogType.Information, "0 scheduled tasks to run", null),
				new LogForTest(LogType.Debug, "1: processing sent items queue for transition", null),
				new LogForTest(LogType.Debug, $@"___Test Campaign2a_2B: Cannot Send Campaigns All errors on this campaign must be corrected before campaigns can be sent.
- Invalid Coordinator Email address.
- Please enter a Media Category.
- Please enter an Estimated Start Date.
- Please enter a Campaign Coordinator.
- Please enter a Campaign Manager.
- Please enter a Media Type.
This touch campaign can be accessed through its master campaign with ID {master.G0_CampaignID}.", null),
				new LogForTest(LogType.Debug, "1 campaign items were successfuly transferred", null),
				new LogForTest(LogType.Warning, "Scheduled Campaign Item Email Processor task completed with warnings", null)
			);
		}

		public void TestLogerExceptionWithFormatString()
		{
			var serviceTask = new CampaignItemScheduleProcessorServiceTaskForTest();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.ExceptionExpected = new Exception("exception with format string . { jkhasdfjkh } ~~~ {0} - {1} - {2} ... ");

			try
			{
				RunTaskScheduleWithAnyBranchContext(serviceTask);
			}
			catch (Exception ex)
			{
				AssertEquals(serviceTask.ExceptionExpected, ex);
			}

			var logs = string.Join("\r\n", TestLogger.Logs.Select(x => $"[{x.Type}]-[{x.Message}]"));
			AssertEquals(@"[Information]-[Scheduled Campaign Item Email Processor task started]
[Information]-[0 scheduled tasks to run]
[Error]-[Error occurred while processing batch: exception with format string . { jkhasdfjkh } ~~~ {0} - {1} - {2} ... ]", logs);
		}

		void RunTaskScheduleWithAnyBranchContext(CampaignItemScheduleProcessorServiceTask serviceTask)
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SCH", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		#region Logger

		void AssertLogs(params LogForTest[] logs)
		{
			var expected = string.Join("\r\n", Array.ConvertAll(logs, l => l.ToString()));
			var actual = string.Join("\r\n", TestLogger.Logs.ConvertAll(l => l.ToString()).ToArray());
			var message = string.Format("\r\nEXPECTED:\r\n{0}\r\n\r\nACTUAL:\r\n{1}\r\n", expected, actual);

			AssertEquals(message, logs.Length, TestLogger.Logs.Count);
			for (var i = 0; i < logs.Length; i++)
			{
				var lineErrorMessage = string.Format("Line differs:{0}\r\n{1}", i + 1, message);
				AssertEquals(lineErrorMessage, logs[i], TestLogger.Logs[i]);
			}
		}

		LoggerForTest TestLogger
		{
			get { return testLogger ?? (testLogger = new LoggerForTest()); }
		}
		LoggerForTest testLogger;

		#endregion

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "SCH", hostedServiceAttribute.Code);
				AssertEquals("Description", "Scheduled Campaign Item Email Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "SAL", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}
	}
}
