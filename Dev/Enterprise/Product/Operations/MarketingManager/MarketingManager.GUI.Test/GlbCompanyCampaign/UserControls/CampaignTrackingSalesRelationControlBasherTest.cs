using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignTrackingSalesRelationControlFormForTest))]
	sealed class CampaignTrackingSalesRelationControlBasherTest : ZFormBasherTest
	{
		public void TestSetupNewActivityButton_AnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = new CampaignTrackingSalesRelationControlFormForTest(campaign))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Communication", "Inquiry", "Campaign", "Opportunity", "Quotation", "One Off Quote", "Project" },
					form.CampaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestSetupAttachActivityButton_AnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = new CampaignTrackingSalesRelationControlFormForTest(campaign))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Communication", "Inquiry", "Campaign", "Opportunity", "Quotation", "One Off Quote", "Project" },
					form.CampaignSalesRelationControl.AttachToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestNewActivityButton_WhenCreatingChildCommunicationWithParentCampaignItem()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = inquiry.PK;

			Factory.Save();
			using (var form = new CampaignTrackingSalesRelationControlFormForTest(campaign, true))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.LastFormShownForTest = null;

				Env.Security.SalesRelationsNew.IsAllowed = true;
				form.CampaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems[0].PerformClick();
				AssertType(typeof(CommunicationForm), ZFormModaliser.LastFormShownForTest);
				var formBizObj = ((CommunicationForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				AssertType(typeof(OrgSalesCall), formBizObj);
				AssertEquals("Should have imported summary from parent campaign Item", ((ISalesRelationActivity)campaignItem).Summary, ((OrgSalesCall)formBizObj).OQ_CallSummary);
			}
		}

		public void TestNewActivityButton_WhenCreatingChildSpotQuoteWithParentCampaignItem()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = inquiry.PK;

			Factory.Save();

			using (var form = new CampaignTrackingSalesRelationControlFormForTest(campaign, true))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.LastFormShownForTest = null;

				Env.Security.SalesRelationsNew.IsAllowed = true;
				form.CampaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems[5].PerformClick();   // Spot Quote

				var formBizObj = ((IZForm)ZFormModaliser.LastFormShownForTest).BusinessEntityForPersistingForm;
				var relatableActivity = ((IRelatableActivity)formBizObj);
				Assert(relatableActivity.ShouldIgnoreSuperAndSubActivityRelationships);
				AssertNotNull(relatableActivity.RelatedParentActivityPivotCollection.Activities.FirstOrDefault(s => s.GetType() == typeof(GlbCompanyCampaignItem)));
			}
		}

		public void TestNewActivityButton_WhenCreatingChildQuotationWithParentCampaignItem()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = inquiry.PK;

			Factory.Save();

			using (var form = new CampaignTrackingSalesRelationControlFormForTest(campaign, true))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.LastFormShownForTest = null;

				Env.Security.SalesRelationsNew.IsAllowed = true;
				form.CampaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems[4].PerformClick();   // Quotation

				var formBizObj = ((IZForm)ZFormModaliser.LastFormShownForTest).BusinessEntityForPersistingForm;
				var relatableActivity = ((IRelatableActivity)formBizObj);
				Assert(relatableActivity.ShouldIgnoreSuperAndSubActivityRelationships);
				AssertNotNull(relatableActivity.RelatedParentActivityPivotCollection.Activities.FirstOrDefault(s => s.GetType() == typeof(GlbCompanyCampaignItem)));
			}
		}

		public void TestNewOpportunityCreatedManually_BroadcastTouchCampaignItem()
		{
			AssertNewOpportunityCreatedManuallyCampaignItem(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Broadcast);
		}

		public void TestNewOpportunityCreatedManually_SurveyTouchCampaignItem()
		{
			AssertNewOpportunityCreatedManuallyCampaignItem(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Survey);
		}

		public void TestNewOpportunityCreatedManually_VotingTouchCampaignItem()
		{
			AssertNewOpportunityCreatedManuallyCampaignItem(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Voting);
		}

		public void TestNewOpportunityCreatedManually_PreApproachEmailTouchCampaignItem()
		{
			AssertNewOpportunityCreatedManuallyCampaignItem(CampaignTypeList.Codes.InsideSales, InsideSalesTouchTypeList.Codes.PreApproachEmail);
		}

		public void TestNewOpportunityCreatedManually_OpportunityCreationTouchCampaignItem()
		{
			AssertNewOpportunityCreatedManuallyCampaignItem(CampaignTypeList.Codes.InsideSales, InsideSalesTouchTypeList.Codes.OpportunityCreation);
		}

		public void AssertNewOpportunityCreatedManuallyCampaignItem(string masterCampaignType, string touchType)
		{
			var contact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = masterCampaignType;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.G0_BroadcastVoteSurveyExam = touchType;

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			using (var form = new CampaignTrackingSalesRelationControlFormForTest(touch, true))
			{
				form.Show();
				Application.DoEvents();

				var campaignSalesRelationControl = form.CampaignSalesRelationControl;
				var opportunityButton = campaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Opportunity");

				opportunityButton.PerformClick();
				campaignSalesRelationControl.InvokeNewFormSavedForTest();
				AssertEquals(touch.PK, campaignSalesRelationControl.NewOpportunity.P8_G0);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new CampaignTrackingSalesRelationControlFormForTest(campaign);
		}

		#region Classes

		public class CampaignTrackingSalesRelationControlForTest : CampaignTrackingSalesRelationControl
		{
			internal new ZToolStripDropDownButton AttachToolStripDropDownButton => base.AttachToolStripDropDownButton;

			internal new ZToolStripDropDownButton NewToolStripDropDownButton => base.NewToolStripDropDownButton;

			internal new void InvokeNewFormSavedForTest() => base.InvokeNewFormSavedForTest();

			internal OrgOpportunity NewOpportunity => (OrgOpportunity)NewFormEntityBusinessEntity;
		}

		public class CampaignTrackingSalesRelationControlFormForTest : ZForm
		{
			public CampaignTrackingSalesRelationControlFormForTest(GlbCompanyCampaign campaign, bool setBindingForCampaignItems = false)
				: base(campaign)
			{
				BindingSource.SetBindingMember(CampaignSalesRelationControl, (setBindingForCampaignItems ? "CampaignsItemsSent.SalesRelationModel" : "SalesRelationModel"));
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 768, true);

				Controls.Add(CampaignSalesRelationControl);
				CaptionRenderingEnabled = true;
			}

			internal CampaignTrackingSalesRelationControlForTest CampaignSalesRelationControl = new CampaignTrackingSalesRelationControlForTest();
		}

		#endregion

		#endregion
	}
}
