using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemController))]
	public class TestGlbCompanyCampaignItemController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaignItem;
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItem);
			controller.ShowNewForm();
			AssertEquals(ErrorReporter.LastMessageReported, "The operator tried to create a new Company Campaign Item, which can only happen when a Campaign is sent");
			ErrorReporter.Clear();
		}

		#region TestViewForm

		public override void TestViewForm()
		{
			Assert("TestViewForm needs to be tested for each CampaignContact type instead of just one", true);
		}

		[SnailTest]
		public void TestViewForm_OrgContact()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var form = Controller.ShowViewForm(GetOrgContactInDatabase());
					AssertNotNull(form);
					AssertEquals(ControllerIDs.Organisation, form.ControllerID);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestViewForm_InquiryContact()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var form = Controller.ShowViewForm(GetInquiryContactInDatabase());
					AssertNotNull(form);
					AssertEquals(ControllerIDs.SalesEnquiry, form.ControllerID);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestViewForm_HRJobApplicant()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var form = Controller.ShowViewForm(GetHRJobApplicantInDatabase());
					AssertNotNull(form);
					AssertEquals(ControllerIDs.HRJobApplicant, form.ControllerID);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestViewForm_GlbStaff()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var form = Controller.ShowViewForm(GetGlbStaffInDatabase());
					AssertNotNull(form);
					AssertEquals(ControllerIDs.GlbStaff, form.ControllerID);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestViewForm_Campaign()
		{
			AssertControllerNotNull();
			using (var parent = new OrganisationModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var form = Controller.ShowViewForm(GetCampaignInDatabase());
					AssertNotNull(form);
					AssertEquals(ControllerIDs.GlbCompanyCampaign, form.ControllerID);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		#endregion

		#region TestEditForm

		public override void TestEditForm()
		{
			Assert("TestEditForm needs to be tested for each CampaignContact type instead of just one", true);
		}

		[SnailTest]
		public void TestEditForm_OrgContact()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					testForm = Controller.ShowEditForm(GetOrgContactInDatabase());
					AssertEquals(ControllerIDs.Organisation, testForm.ControllerID);
					AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
				finally
				{
					if (testForm != null)
					{ ((ZForm)testForm).Close(); }
				}
			}
		}

		[SnailTest]
		public void TestEditForm_InquiryContact()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					testForm = Controller.ShowEditForm(GetInquiryContactInDatabase());
					AssertEquals(ControllerIDs.SalesEnquiry, testForm.ControllerID);
					AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
				finally
				{
					if (testForm != null)
					{ ((ZForm)testForm).Close(); }
				}
			}
		}

		[SnailTest]
		public void TestEditForm_HRJobApplicant()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var testForm = Controller.ShowEditForm(GetHRJobApplicantInDatabase());
					AssertNotNull(testForm);
					AssertEquals(ControllerIDs.HRJobApplicant, testForm.ControllerID);
					AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestEditForm_GlbStaff()
		{
			AssertControllerNotNull();
			using (var parent = new GlbCompanyCampaignItemModule())
			{
				Controller.ParentModule = parent;
				try
				{
					var testForm = Controller.ShowEditForm(GetGlbStaffInDatabase());
					AssertNotNull(testForm);
					AssertEquals(ControllerIDs.GlbStaff, testForm.ControllerID);
					AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		[SnailTest]
		public void TestEditForm_Campaign()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			using (var parent = new OrganisationModule())
			{
				Controller.ParentModule = parent;
				try
				{
					testForm = Controller.ShowEditForm(GetCampaignInDatabase());
					AssertEquals(ControllerIDs.GlbCompanyCampaign, testForm.ControllerID);
					AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
				finally
				{
					if (testForm != null)
					{ ((ZForm)testForm).Close(); }
				}
			}
		}

		public void TestEditForm_Campaign_WhenSecurityRightDenied()
		{
			Env.Security.CampaignManagementCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			using (var parentModule = new OrganisationModule())
			using (var form = Controller.ShowEditForm(GetCampaignInDatabase()))
			{
				AssertEquals("Access Denied: Search and View Records Assigned to Other Login Staff", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNull(form);
			}
		}

		#endregion

		GlbCompanyCampaignItem GetOrgContactInDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			return campaignItem;
		}

		GlbCompanyCampaignItem GetInquiryContactInDatabase()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = inquiry.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			return campaignItem;
		}

		GlbCompanyCampaignItem GetHRJobApplicantInDatabase()
		{
			var applicant = Factory.New<IHRJobApplicant>();
			applicant.HA_FullName = "name";
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			return campaignItem;
		}

		GlbCompanyCampaignItem GetGlbStaffInDatabase()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = staff.PK;
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			return campaignItem;
		}

		GlbCompanyCampaignItem GetCampaignInDatabase()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			return campaignItem;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return GetCampaignInDatabase();
		}
	}
}
