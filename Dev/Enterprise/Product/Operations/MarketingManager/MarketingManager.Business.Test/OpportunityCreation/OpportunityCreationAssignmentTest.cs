using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OpportunityCreationAssignmentTest : TestCaseWithFactory
	{
		public void TestSetStaffPoolAssignments()
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

			var poolItem1 = touch.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = staff1.GS_Code;
			poolItem1.GCP_SendRatio = 1;

			var poolItem2 = touch.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = staff2.GS_Code;
			poolItem2.GCP_SendRatio = 1;

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

			var orgOpportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity1.P8_OC = campaignItem1.G8_RecipientID;
			orgOpportunity1.P8_G0 = campaignItem1.G8_G0;

			var orgOpportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity2.P8_OC = campaignItem2.G8_RecipientID;
			orgOpportunity2.P8_G0 = campaignItem2.G8_G0;
			Factory.Save();

			var opportunityCreationAssignment = new OpportunityCreationAssignment(touch);
			opportunityCreationAssignment.SetStaffPoolAssignments(touch.CampaignsItemsSent.OfType<GlbCompanyCampaignItem>().ToList());

			var opportunitiesCreated = Factory.Load<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_G0, touch.PK)).ToList();
			AssertEquals(poolItem1.GCP_SendRatio, opportunitiesCreated.Count(o => o.P8_GS_NKPrimarySalesPerson == poolItem1.GCP_GS_NKSender));
			AssertEquals(poolItem2.GCP_SendRatio, opportunitiesCreated.Count(o => o.P8_GS_NKPrimarySalesPerson == poolItem2.GCP_GS_NKSender));
		}

		public void TestGetAssignmentPoolData()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "teststaffpool1@test.com";
			staff1.GS_Code = "TS1";
			staff1.GS_FullName = "Test Staff Pool 1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "teststaffpool2@test.com";
			staff2.GS_Code = "TS2";
			staff2.GS_FullName = "Test Staff Pool 2";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch);

			var poolItem1 = touch.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = staff1.GS_Code;
			poolItem1.GCP_SendRatio = 1;

			var poolItem2 = touch.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = staff2.GS_Code;
			poolItem2.GCP_SendRatio = 1;
			Factory.Save();

			var opportunityCreationAssignmentPoolData = new OpportunityCreationAssignment(touch).GetAssignmentPoolData();
			AssertEquals(0.5, opportunityCreationAssignmentPoolData.NormalizedRatio[staff1.GS_Code]);
			AssertEquals(0.5, opportunityCreationAssignmentPoolData.NormalizedRatio[staff2.GS_Code]);
		}
	}
}
