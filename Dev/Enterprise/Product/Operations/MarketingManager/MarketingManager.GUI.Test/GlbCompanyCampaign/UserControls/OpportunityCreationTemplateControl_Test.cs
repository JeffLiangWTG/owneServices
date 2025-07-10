using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI.Test.GlbCompanyCampaign.UserControls;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI
{
	[TestedType(typeof(OpportunityCreationTemplateControlForm))]
	public class OpportunityCreationTemplateControl_Test : ZFormBasherTest
	{
		public void TestOpportunityStatusInfo_ValueChanged()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>());
			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.OpportunityStatus = "LOS";
				AssertEquals(form.OpportunityCreationTemplateControl.OverallDispositionLabel.BackColor, Color.Red);

				opportunityCreationTemplate.OpportunityStatus = "CRT";
				AssertEquals(form.OpportunityCreationTemplateControl.OverallDispositionLabel.BackColor, Color.LimeGreen);

				opportunityCreationTemplate.OpportunityStatus = "WON";
				AssertEquals(form.OpportunityCreationTemplateControl.OverallDispositionLabel.BackColor, Color.Red);

				opportunityCreationTemplate.OpportunityStatus = "SUS";
				AssertEquals(form.OpportunityCreationTemplateControl.OverallDispositionLabel.BackColor, Color.LimeGreen);

				opportunityCreationTemplate.OpportunityStatus = "ABA";
				AssertEquals(form.OpportunityCreationTemplateControl.OverallDispositionLabel.BackColor, Color.Red);
			}
		}

		public void TestSourceInfo_ValueChanged()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "WEB", (NoResString)"Web Campaign", true, "CL2" },
					{ "MKT", (NoResString)"Marketing Campaign", false, "CL2" }
				};
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opportunityCreationTemplate = new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>());
			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.Source = "TEST";
				Assert(!form.OpportunityCreationTemplateControl.SourceDetailsDropEdit.Visible);
				Assert(form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Visible);

				opportunityCreationTemplate.Source = "WEB";
				Assert(form.OpportunityCreationTemplateControl.SourceDetailsDropEdit.Visible);
				Assert(!form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Visible);

				opportunityCreationTemplate.Source = ZString.Empty;
				Assert(!form.OpportunityCreationTemplateControl.SourceDetailsDropEdit.Visible);
				Assert(form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Visible);

				opportunityCreationTemplate.Source = "MKT";
				Assert(form.OpportunityCreationTemplateControl.SourceDetailsDropEdit.Visible);
				Assert(!form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Visible);
			}
		}

		public void TestUseCampaignNameInfo_ValueChanged()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var opportunityCreationTemplate = new OpportunityCreationTemplate(campaign);

			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.UseCampaignName = true;
				AssertEquals(opportunityCreationTemplate.SourceDetails, campaign.G0_CampaignNameMultilingual);
				AssertEquals(form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Text, campaign.G0_CampaignNameMultilingual);

				opportunityCreationTemplate.UseCampaignName = false;
				AssertEquals(opportunityCreationTemplate.SourceDetails, ZString.Empty);
				AssertEquals(form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Text, ZString.Empty);
			}
		}

		public void TestSourceDetailsInfo_ValueChanged()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>());
			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.SourceDetails = "TEST";
				AssertEquals(opportunityCreationTemplate.SourceDetails, opportunityCreationTemplate.ActiveSourceDetails);
				AssertEquals(form.OpportunityCreationTemplateControl.SourceDetailsTextBox.Text, opportunityCreationTemplate.ActiveSourceDetails);
			}
		}

		public void TestOpportunityAssignmentInfo_ValueChanged()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>());
			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;
				Assert(form.OpportunityCreationTemplateControl.SalesPersonCodeFindBox.Visible);
				Assert(!form.OpportunityCreationTemplateControl.OrgStaffAssignmentDropEdit.Visible);
				Assert(!form.OpportunityCreationTemplateControl.StaffPoolAssignmentsButton.Visible);

				opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.StaffAssignment;
				Assert(!form.OpportunityCreationTemplateControl.SalesPersonCodeFindBox.Visible);
				Assert(form.OpportunityCreationTemplateControl.OrgStaffAssignmentDropEdit.Visible);
				Assert(!form.OpportunityCreationTemplateControl.StaffPoolAssignmentsButton.Visible);

				opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;
				Assert(!form.OpportunityCreationTemplateControl.SalesPersonCodeFindBox.Visible);
				Assert(!form.OpportunityCreationTemplateControl.OrgStaffAssignmentDropEdit.Visible);
				Assert(!form.OpportunityCreationTemplateControl.StaffPoolAssignmentsButton.Visible);

				opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.StaffPoolAssignments;
				Assert(!form.OpportunityCreationTemplateControl.SalesPersonCodeFindBox.Visible);
				Assert(!form.OpportunityCreationTemplateControl.OrgStaffAssignmentDropEdit.Visible);
				Assert(form.OpportunityCreationTemplateControl.StaffPoolAssignmentsButton.Visible);
			}
		}

		public void TestInvalidCharacterEntered()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);
			using (var form = new OpportunityCreationTemplateControlForm(opportunityCreationTemplate))
			{
				form.Show();

				opportunityCreationTemplate.OpportunityDescription = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.PackageType = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.OpportunityType = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.OpportunityStatus = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.OpportunityStage = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.Source = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.ActiveSourceDetails = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.OpportunityAssignment = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.StaffAssignment = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.OpportunityNotes = ZBlob.FromUTF8("Test " + opportunityCreationTemplate.Separator + " Test");
				opportunityCreationTemplate.Validation.ValidateAll();

				var errorMessage = "Invalid character entered: '" + opportunityCreationTemplate.Separator + "'";
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityDescriptionInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.PackageTypeInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityTypeInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityStatusInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityStageInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.SourceInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.ActiveSourceDetailsInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityAssignmentInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.StaffAssignmentInfo, errorMessage);
				AssertHasErrorContaining(opportunityCreationTemplate.OpportunityNotesInfo, errorMessage);

				opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;
				opportunityCreationTemplate.SalesPerson = "Test " + opportunityCreationTemplate.Separator + " Test";
				opportunityCreationTemplate.Validation.ValidateAll();

				AssertHasErrorContaining(opportunityCreationTemplate.SalesPersonInfo, errorMessage);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
		}

		protected override Form GetFormToBashCore()
		{
			return new OpportunityCreationTemplateControlForm(new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>()));
		}

		GlbCompanyCampaign Campaign => campaign ?? (campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>());
		GlbCompanyCampaign campaign;

		#endregion
	}
}
