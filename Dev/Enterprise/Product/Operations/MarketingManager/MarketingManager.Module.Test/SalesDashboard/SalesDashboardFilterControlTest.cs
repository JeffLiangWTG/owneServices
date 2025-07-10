using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class SalesDashboardFilterControlTest : TestCaseWithFactory
	{
		public void TestCurrentUserLabelText()
		{
			GlbStaff.CurrentUser.GS_FriendlyName = "Andrew";
			GlbStaff.CurrentUser.GS_FullName = "Andrew Luong";
			using (var control = new SalesDashboardFilterControlForTesting(new SalesDashboardActivityCollection(Factory), new SalesDashboardFilterBusinessObject()))
			{
				AssertEquals("Andrew", control.CurrentUserLabel_Exposed.Text);
			}

			GlbStaff.CurrentUser.GS_FriendlyName = ZString.Empty;
			GlbStaff.CurrentUser.GS_FullName = "Andrew Luong";
			using (var control = new SalesDashboardFilterControlForTesting(new SalesDashboardActivityCollection(Factory), new SalesDashboardFilterBusinessObject()))
			{
				AssertEquals("Andrew Luong", control.CurrentUserLabel_Exposed.Text);
			}
		}

		public void TestShowPreviewFor()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var salesCall = organisation.SalesCalls.AddNew();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var activityCollection = new SalesDashboardActivityCollection(Factory);

			using (var control = new SalesDashboardFilterControlForTesting(activityCollection, new SalesDashboardFilterBusinessObject()))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				activityCollection.Load();
				AssertEquals("Precondition: activityCollection.Count", 3, activityCollection.Count);
				AssertEquals("Precondition: control.Grid.ListManager.Count", 3, control.Grid.ListManager.Count);
				var opportunityActivity = activityCollection.First(x => x.PK == opportunity.PK);
				var inquiryActivity = activityCollection.First(x => x.PK == inquiry.PK);
				var communicationActivity = activityCollection.First(x => x.PK == salesCall.PK);

				control.Grid.ListManager.Position = control.Grid.ListManager.List.IndexOf(opportunityActivity);
				AssertEquals("Precondition: selected opportunity task", opportunity.PK, ((SalesDashboardActivity)control.Grid.ListManager.GetCurrent()).PK);
				AssertEquals("Preview pane should be visible when opportunity activity is selected", true, control.PreviewPanel_Exposed.Visible);

				control.Grid.ListManager.Position = control.Grid.ListManager.List.IndexOf(inquiryActivity);
				AssertEquals("Precondition: selected inquiry task", inquiry.PK, ((SalesDashboardActivity)control.Grid.ListManager.GetCurrent()).PK);
				AssertEquals("Preview pane should be visible when inquiry activity is selected", true, control.PreviewPanel_Exposed.Visible);

				control.Grid.ListManager.Position = control.Grid.ListManager.List.IndexOf(communicationActivity);
				AssertEquals("Precondition: selected workflow item", salesCall.PK, ((SalesDashboardActivity)control.Grid.ListManager.GetCurrent()).PK);
				AssertEquals("Preview pane should be visible when sales call activity is selected", true, control.PreviewPanel_Exposed.Visible);
			}
		}

		public void TestCampaignTrackingMenuItemApplicability()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunityActivity = Factory.NewWithValidTestData<OpportunitySalesDashboardActivity>();
			opportunityActivity.VSA_OH = organisation.PK;
			opportunityActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Opportunity;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var parent = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			parent.G8_G0 = campaign.PK;
			parent.G8_RecipientID = contact.PK;
			parent.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignActivity = Factory.NewWithValidTestData<CampaignSalesDashboardActivity>();
			campaignActivity.VSA_OH = organisation.PK;
			campaignActivity.VSA_ParentId = parent.PK;
			campaignActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Campaign;

			var exam = (IGlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			exam.G0_CampaignName = "TestExam";
			var parent2 = (IGlbCompanyCampaignItem)Factory.New<ILearningCentreCampaignItem>();
			parent2.G8_G0 = exam.PK;
			parent2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			parent2.G8_RecipientID = contact.PK;

			var campaignActivity2 = Factory.NewWithValidTestData<CampaignSalesDashboardActivity>();
			campaignActivity2.VSA_OH = organisation.PK;
			campaignActivity2.VSA_ParentId = parent2.PK;
			campaignActivity2.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Campaign;

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.CampaignsItemsSent.RemoveAndDeleteAll();
			campaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;

			var parent3 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			parent3.G8_G0 = campaign2.PK;
			parent3.G8_RecipientID = contact.PK;
			parent3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignActivity3 = Factory.NewWithValidTestData<CampaignSalesDashboardActivity>();
			campaignActivity3.VSA_OH = organisation.PK;
			campaignActivity3.VSA_ParentId = parent3.PK;
			campaignActivity3.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Campaign;

			Factory.Save();

			using (var module = new OrgSalesDashboardModuleTesting(Factory))
			using (var form = new ZForm())
			{
				module.Org = organisation;

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				var opportunityActivityItem = module.GridCollection.FindByPK(opportunityActivity.PK);
				var campaignActivityItem = module.GridCollection.FindByPK(campaignActivity.PK);
				var campaignActivityItem2 = module.GridCollection.FindByPK(campaignActivity2.PK);
				var campaignActivityItem3 = module.GridCollection.FindByPK(campaignActivity3.PK);

				module.DisplayGrid.ListManager.Position = module.DisplayGrid.ListManager.List.IndexOf(opportunityActivityItem);
				module.DisplayGrid.OnPopup_CallForTesting();
				var campaignTrackingMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("CampaignTracking");
				Assert(!campaignTrackingMenuItem.Visible);

				module.DisplayGrid.ListManager.Position = module.DisplayGrid.ListManager.List.IndexOf(campaignActivityItem);
				module.DisplayGrid.OnPopup_CallForTesting();
				campaignTrackingMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("CampaignTracking");
				Assert(campaignTrackingMenuItem.Visible);

				module.DisplayGrid.ListManager.Position = module.DisplayGrid.ListManager.List.IndexOf(campaignActivityItem2);
				module.DisplayGrid.OnPopup_CallForTesting();
				campaignTrackingMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("CampaignTracking");
				Assert(!campaignTrackingMenuItem.Visible);

				module.DisplayGrid.ListManager.Position = module.DisplayGrid.ListManager.List.IndexOf(campaignActivityItem3);
				module.DisplayGrid.OnPopup_CallForTesting();
				campaignTrackingMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("CampaignTracking");
				Assert(!campaignTrackingMenuItem.Visible);
			}
		}

		#region Implementation

		class SalesDashboardFilterControlForTesting : SalesDashboardFilterControl
		{
			public SalesDashboardFilterControlForTesting(SalesDashboardActivityCollection gridCollection, SalesDashboardFilterBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject, true)
			{
			}

			public KSplitContainer PreviewPanel_Exposed
			{
				get { return previewSplitContainer; }
			}

			public ZLabel CurrentUserLabel_Exposed
			{
				get { return currentUserLabel; }
			}
		}

		class OrgSalesDashboardModuleTesting : OrgSalesDashboardModule
		{
			public OrgSalesDashboardModuleTesting(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			protected override BusinessObjectFactory GetNewFactory()
			{
				return factory;
			}

			readonly BusinessObjectFactory factory;
		}

		#endregion
	}
}
