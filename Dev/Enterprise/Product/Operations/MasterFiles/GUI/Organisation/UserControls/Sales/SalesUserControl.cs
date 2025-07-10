using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesUserControl : OrganisationSecurityContainerControl
	{
		public SalesUserControl()
		{
			InitializeComponent();

			AddOpportunityManagementControl();
			SalesTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.TradeProfileForOrgPlugin, 2);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var org = CurrentDataItem as OrgHeader;
			if (org != null && org.OH_IsSalesLead)
			{
				SetupTabPageSecurity();
			}

			AddSalesProspectControl();
		}

		#region Opportunity Management Control

		protected OpportunityManagementControl OpportunityControl;

		void AddOpportunityManagementControl()
		{
			OpportunityControl = OpportunityManagementControl.New();
			OpportunityTabPage.Controls.Add(OpportunityControl);
			// 
			// OpportunityControl
			// 
			OpportunityControl.AllowDrop = true;
			BindingSource.SetBindingMember(OpportunityControl, ".");
			OpportunityControl.Dock = System.Windows.Forms.DockStyle.Fill;
			OpportunityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			OpportunityControl.Name = "OpportunityControl";
			OpportunityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			OpportunityControl.TabIndex = 0;
		}

		#endregion

		void AddSalesProspectControl()
		{
			SalesClientSummaryUserControl salesProspectControl1;
			salesProspectControl1 = SalesClientSummaryUserControl.New();
			SalesProspectTabPage.Controls.Add(salesProspectControl1);
			// 
			// salesProspectControl1
			// 
			salesProspectControl1.AllowDrop = true;
			salesProspectControl1.AutoScroll = true;
			salesProspectControl1.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1375, 0, false);
			BindingSource.SetBindingMember(salesProspectControl1, ".");
			salesProspectControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			salesProspectControl1.Name = "salesProspectControl1";
			salesProspectControl1.TabIndex = 0;
			salesProspectControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			salesProspectControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 495, true);
		}

		#region Security

		internal void SetupTabPageSecurity()
		{
			SalesProspectTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewClientSummary }, Env.Licence.RelationshipClientIntelligence);
			SalesClientRelTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewClientRelationship }, Env.Licence.RelationshipClientIntelligence);
			SalesActivityTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewSalesActivity }, Env.Licence.RelationshipClientIntelligence);
			OpportunityTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewOpportunityManagement }, Env.Licence.RelationshipOpportunityManager);
			CommunicationsTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewCommunicationManager }, Env.Licence.CommunicationManager);

			var tradeProfileTabPage = SalesTabControl.PlugIns.GetPlugIn(ControllerIDs.TradeProfileForOrgPlugin).TabPage;
			tradeProfileTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgSalesView, Env.Security.ClientIntelligenceViewTradeProfile }, Env.Licence.RelationshipClientIntelligence);
		}

		internal void DetachLicenceCheckpoints()
		{
			SalesProspectTabPage.LicenceCheckpoint = null;
			SalesClientRelTabPage.LicenceCheckpoint = null;
			SalesActivityTabPage.LicenceCheckpoint = null;
			OpportunityTabPage.LicenceCheckpoint = null;
			CommunicationsTabPage.LicenceCheckpoint = null;

			var tradeProfileTabPage = SalesTabControl.PlugIns.GetPlugIn(ControllerIDs.TradeProfileForOrgPlugin).TabPage;
			tradeProfileTabPage.LicenceCheckpoint = null;
		}

		#endregion

		#region Navigate to WorkflowItem

		public void NavigateToWorkflowItem(ProcessTask task)
		{
			SalesTabControl.SelectedTab = OpportunityTabPage;
			OpportunityControl.NavigateToWorkflowItem(task);
		}

		public void NavigateToWorkflowItem(IProcessHeader workflow)
		{
			SalesTabControl.SelectedTab = OpportunityTabPage;
			OpportunityControl.NavigateToWorkflowItem(workflow);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region ReadOnly / Visible

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				SetOpportunityManagementReadOnly();
				SetSalesActivityTabVisible();
			}
		}

		void SetSalesActivityTabVisible()
		{
			SalesActivityTabPage.TabVisible = OrganisationsDataRegistry.Instance.ShowSalesActivities.Value;
		}

		void SetOpportunityManagementReadOnly()
		{
			OrgHeader header = (OrgHeader)CurrentDataItem;
			OpportunityControl.SetControlReadOnly(!header.SecurityProvider.HasModifySalesOpportunityManagementSecurity);
		}

		#endregion

	}
}
