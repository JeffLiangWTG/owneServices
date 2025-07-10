using System;
using System.Drawing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class OpportunityCreationTemplateControl : ZUserControl
	{
		public OpportunityCreationTemplateControl()
		{
			InitializeComponent();
		}

		#region Implementation

		public new OpportunityCreationTemplate CurrentDataItem => (OpportunityCreationTemplate)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (CurrentDataItem != null)
			{
				SetupSourceDetailsControl();
				CurrentDataItem.OpportunityStatusInfo.ValueChanged -= OpportunityStatusInfo_ValueChanged;
				CurrentDataItem.SourceInfo.ValueChanged -= SourceInfo_ValueChanged;
				CurrentDataItem.UseCampaignNameInfo.ValueChanged -= UseCampaignNameInfo_ValueChanged;
				CurrentDataItem.SourceDetailsInfo.ValueChanged -= SourceDetailsInfo_ValueChanged;
				CurrentDataItem.OpportunityAssignmentInfo.ValueChanged -= OpportunityAssignmentInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				SetupSourceDetailsControl();
				SetOpportunityAssignmentVisibility();
				CurrentDataItem.OpportunityStatusInfo.ValueChanged += OpportunityStatusInfo_ValueChanged;
				CurrentDataItem.SourceInfo.ValueChanged += SourceInfo_ValueChanged;
				CurrentDataItem.UseCampaignNameInfo.ValueChanged += UseCampaignNameInfo_ValueChanged;
				CurrentDataItem.SourceDetailsInfo.ValueChanged += SourceDetailsInfo_ValueChanged;
				CurrentDataItem.OpportunityAssignmentInfo.ValueChanged += OpportunityAssignmentInfo_ValueChanged;
			}
		}

		void SetOpportunityAssignmentVisibility()
		{
			var oppAssignment = CurrentDataItem.OpportunityAssignment;

			SalesPersonCodeFindBox.Visible = oppAssignment == OpportunityAssignmentList.Codes.IndividualSalesPerson;
			OrgStaffAssignmentDropEdit.Visible = oppAssignment == OpportunityAssignmentList.Codes.StaffAssignment;
			StaffPoolAssignmentsButton.Visible = oppAssignment == OpportunityAssignmentList.Codes.StaffPoolAssignments;
			OpportunityAssignmentDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(oppAssignment == OpportunityAssignmentList.Codes.StaffPoolAssignments ? 276 : 302, 18, true);
		}

		void StaffPoolSenderAssignmentButton_Click(object sender, EventArgs e)
		{
			var glbCompanyCampaignSenderPool = CurrentDataItem?.Campaign?.SenderPool;
			if (glbCompanyCampaignSenderPool != null)
			{
				var staffPoolAssignmentForm = new SelectStaffPoolOpportunityAssignmentForm(glbCompanyCampaignSenderPool);
				staffPoolAssignmentForm.Closed += (o, args) => CurrentDataItem.Validation.ValidateOpportunityAssignment();

				ZFormModaliser.Show(staffPoolAssignmentForm, ParentForm);
			}
		}

		void OpportunityStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOverallDispositionLabelColor();
		}

		void SourceInfo_ValueChanged(object sender, EventArgs e)
		{
			SwitchSourceDetailsControl(CurrentDataItem.Source != string.Empty && CurrentDataItem.Lookups.SourceDetailsList.Count > 0);
		}

		void UseCampaignNameInfo_ValueChanged(object sender, EventArgs e)
		{
			CurrentDataItem.ActiveSourceDetails = CurrentDataItem.UseCampaignName ? CurrentDataItem.Campaign.G0_CampaignNameMultilingual : ZString.Empty;
		}

		void SourceDetailsInfo_ValueChanged(object sender, EventArgs e)
		{
			CurrentDataItem.UseCampaignName = false;
			CurrentDataItem.ActiveSourceDetails = CurrentDataItem.SourceDetails;
		}

		void OpportunityAssignmentInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOpportunityAssignmentVisibility();
		}

		void SetOverallDispositionLabelColor()
		{
			OverallDispositionLabel.BackColor = CurrentDataItem.IsClosed ? Color.Red : Color.LimeGreen;
		}

		void SwitchSourceDetailsControl(bool dropDownEnabled)
		{
			SourceDetailsDropEdit.Visible = dropDownEnabled;
			SourceDetailsTextBox.Visible = !dropDownEnabled;
		}

		void SetupSourceDetailsControl()
		{
			if (CurrentDataItem.Lookups.SourceDetailsList.Count > 0)
			{
				SwitchSourceDetailsControl(true);
			}
		}

		#endregion
	}
}
