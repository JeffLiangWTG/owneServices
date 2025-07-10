using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesDashboardActivity))]
	sealed class SalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		[TestUtcOffset(10, 0, 0)]
		public void TestVSA_ActivityDateLocal()
		{
			var activity = Factory.New<SalesDashboardActivity>();
			activity.VSA_ActivityDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), activity.VSA_ActivityDateLocal);

			activity.VSA_ActivityDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), activity.VSA_ActivityDate);
		}

		public void TestAssignedGroupCode()
		{
			var org = Helper.NewOrgHeader();
			var group = Helper.NewGroup("DDD");
			group.GG_Desc = "Admin Group";

			var activity = Factory.New<SalesDashboardActivity>();
			AssertEquals("", activity.AssignedGroupCode);
			activity.VSA_GG_AssignedGroup = group.PK;
			AssertEquals("DDD", activity.AssignedGroupCode);
		}

		public void TestStaffAssignmentFullName()
		{
			var org = Helper.NewOrgHeader();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS0";
			staff.GS_FullName = "Test Full Name";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.P8_GS_NKPrimarySalesPerson = staff.GS_Code;

			var inquiry = Factory.NewWithValidTestData<OrgColdCallRegister>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_GS_NKRepAssigned = staff.GS_Code;

			var salesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			salesCall.OQ_OH = org.PK;
			salesCall.OQ_GS_NKSalesRep = staff.GS_Code;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			Factory.Save();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@example.com";

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_G0 = campaign.PK;
			item.G8_RecipientID = contact.PK;
			item.G8_RecipientTableCode = "OC";

			var project = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_OC_Contact, contact.PK);
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_GS_NKProjectManager, staff.GS_Code);

			Factory.Save();

			var activities = Factory.Load<SalesDashboardActivity>(new ZQuery(ViewSalesDashboardActivitySchema.PK, new ZGuid[] { opportunity.PK, inquiry.PK, item.PK, salesCall.PK, project.PK }));
			AssertEquals("number of Activities", 5, activities.Length);

			foreach (var activity in activities)
			{
				AssertEquals($"Full Name in {activity.VSA_ActivityType}", "Test Full Name", activity.VSA_ActivityStaffAssignmentFullName);
			}
		}

		public void TestSalesClientSize()
		{
			var org = Helper.NewOrgHeader();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;

			var inquiry = Factory.NewWithValidTestData<OrgColdCallRegister>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;

			var salesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			salesCall.OQ_OH = org.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			Factory.Save();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@example.com";

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_G0 = campaign.PK;
			item.G8_RecipientID = contact.PK;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_TrackingStatus = "UNV";

			var project = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_OC_Contact, contact.PK);

			Factory.Save();

			var activities = Factory.Load<SalesDashboardActivity>(new ZQuery(ViewSalesDashboardActivitySchema.PK, new ZGuid[] { opportunity.PK, inquiry.PK, item.PK, salesCall.PK, project.PK }));
			AssertEquals("number of Activities", 5, activities.Length);

			foreach (var activity in activities)
			{
				AssertEquals($"Organisation in {activity.VSA_ActivityType}", org.PK, activity.VSA_OH);
				AssertEquals($"Client Size in {activity.VSA_ActivityType}", "SML", activity.VSA_OrgSalesClientSize);
			}
		}

		public override void TestHumanReadableName()
		{
			var activity = Factory.New<SalesDashboardActivity>();
			AssertEquals("Sales Activity", activity.HumanReadableName);
		}

		SalesDashboardActivityTestHelper helper;
		SalesDashboardActivityTestHelper Helper
		{
			get { return helper ?? (helper = new SalesDashboardActivityTestHelper(Factory)); }
		}

		sealed class SalesDashboardActivityTestHelper
		{
			public SalesDashboardActivityTestHelper(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			public GlbGroup NewGroup(string code)
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = code;
				return group;
			}

			public OrgHeader NewOrgHeader()
			{
				OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
				result.OH_FullName = String.Format("Test Client #{0}", ++orgIndex);
				result.MainAddress.OA_Address1 = String.Format("{0} Fake Street", 100 + orgIndex);
				result.MainAddress.OA_City = "Sydney";
				result.MainAddress.OA_State = "NSW";
				result.MainAddress.OA_PostCode = "2000";
				result.OH_RL_NKClosestPort = "AUSYD";
				result.OH_Code = String.Format("TESTORG{0}", orgIndex);
				result.MiscServ.OM_CMClientSize = "SML";

				return result;
			}

			int orgIndex;
		}
	}
}

