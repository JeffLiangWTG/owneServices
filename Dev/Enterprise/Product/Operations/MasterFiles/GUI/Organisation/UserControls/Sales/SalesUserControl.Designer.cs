namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesUserControl
	{

		#region Component Designer generated code

		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl SalesTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage SalesProspectTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage SalesClientRelTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CommunicationsTabPage;
		private System.Windows.Forms.ImageList ImageList;
		private Enterprise.MasterFiles.GUI.SalesClientRelationshipControl ClientRelationshipControl;
		private Enterprise.MasterFiles.GUI.OrgCommunicationPreviewPaneControl CommunicationControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage OpportunityTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage SalesActivityTabPage;
		private Enterprise.MasterFiles.GUI.SalesActivityUserControl SalesActivityControl;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SalesTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.SalesProspectTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OpportunityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SalesClientRelTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClientRelationshipControl = new Enterprise.MasterFiles.GUI.SalesClientRelationshipControl();
			this.CommunicationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommunicationControl = new Enterprise.MasterFiles.GUI.OrgCommunicationPreviewPaneControl();
			this.SalesActivityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SalesActivityControl = new Enterprise.MasterFiles.GUI.SalesActivityUserControl();
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SalesTabControl.SuspendLayout();
			this.SalesProspectTabPage.SuspendLayout();
			this.OpportunityTabPage.SuspendLayout();
			this.SalesClientRelTabPage.SuspendLayout();
			this.CommunicationsTabPage.SuspendLayout();
			this.SalesActivityTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// SalesTabControl
			// 
			this.SalesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SalesTabControl.Controls.Add(this.SalesProspectTabPage);
			this.SalesTabControl.Controls.Add(this.OpportunityTabPage);
			this.SalesTabControl.Controls.Add(this.SalesClientRelTabPage);
			this.SalesTabControl.Controls.Add(this.CommunicationsTabPage);
			this.SalesTabControl.Controls.Add(this.SalesActivityTabPage);
			this.SalesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SalesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.SalesTabControl.Name = "SalesTabControl";
			this.SalesTabControl.SelectedIndex = 0;
			this.SalesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 528, true);
			this.SalesTabControl.TabIndex = 0;
			// 
			// SalesProspectTabPage
			// 
			this.SalesProspectTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesUserControl|af265c93-044f-4df4-80eb-277bde46380a", "Client Summary");
			this.SalesProspectTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesProspectTabPage.Name = "SalesProspectTabPage";
			this.SalesProspectTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.SalesProspectTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 501, true);
			this.SalesProspectTabPage.TabIndex = 0;
			// 
			// OpportunityTabPage
			// 
			this.OpportunityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesUserControl|44a071d2-b2d9-4250-9481-3ae5b9c3c6db", "Opportunity Management");
			this.OpportunityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OpportunityTabPage.Name = "OpportunityTabPage";
			this.OpportunityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 501, true);
			this.OpportunityTabPage.TabIndex = 4;
			// 
			// SalesClientRelTabPage
			// 
			this.SalesClientRelTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesUserControl|0eb0805b-beca-4e14-84fd-40b4699bb7ff", "Client Relationship");
			this.SalesClientRelTabPage.Controls.Add(this.ClientRelationshipControl);
			this.SalesClientRelTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesClientRelTabPage.Name = "SalesClientRelTabPage";
			this.SalesClientRelTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.SalesClientRelTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 501, true);
			this.SalesClientRelTabPage.TabIndex = 1;
			// 
			// ClientRelationshipControl
			// 
			this.ClientRelationshipControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientRelationshipControl, ".");
			this.ClientRelationshipControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientRelationshipControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ClientRelationshipControl.Name = "ClientRelationshipControl";
			this.ClientRelationshipControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 63, true);
			this.ClientRelationshipControl.TabIndex = 0;
			// 
			// CommunicationsTabPage
			// 
			this.CommunicationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesUserControl|9fe485d4-031a-4f2e-95d3-4af6f79b50c4", "Communications");
			this.CommunicationsTabPage.Controls.Add(this.CommunicationControl);
			this.CommunicationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommunicationsTabPage.Name = "CommunicationsTabPage";
			this.CommunicationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CommunicationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 501, true);
			this.CommunicationsTabPage.TabIndex = 2;
			// 
			// CommunicationControl
			// 
			this.CommunicationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommunicationControl, ".");
			this.CommunicationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.CommunicationControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 460, true);
			this.CommunicationControl.Name = "CommunicationControl";
			this.CommunicationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 491, true);
			this.CommunicationControl.TabIndex = 0;
			// 
			// SalesActivityTabPage
			// 
			this.SalesActivityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("66cd6560-ece7-4113-a508-a45cd8f37aa3", "Sales Activity");
			this.SalesActivityTabPage.Controls.Add(this.SalesActivityControl);
			this.SalesActivityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesActivityTabPage.Name = "SalesActivityTabPage";
			this.SalesActivityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SalesActivityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 501, true);
			this.SalesActivityTabPage.TabIndex = 5;
			this.SalesActivityTabPage.UseVisualStyleBackColor = true;
			// 
			// SalesActivityControl
			// 
			this.SalesActivityControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesActivityControl, ".");
			this.SalesActivityControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SalesActivityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SalesActivityControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 460, true);
			this.SalesActivityControl.Name = "SalesActivityControl";
			this.SalesActivityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 495, true);
			this.SalesActivityControl.TabIndex = 0;
			// 
			// ImageList
			// 
			this.ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.ImageList.ImageSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// SalesUserControl
			// 
			this.Controls.Add(this.SalesTabControl);
			this.IsModifySales = true;
			this.IsModifySalesClientRelationship = true;
			this.IsModifySalesClientSummary = true;
			this.IsModifySalesOpportunityManagement = true;
			this.IsModifySalesTradeProfile = true;
			this.Name = "SalesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 552, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.SalesTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SalesTabControl.ResumeLayout(false);
			this.SalesProspectTabPage.ResumeLayout(false);
			this.OpportunityTabPage.ResumeLayout(false);
			this.SalesClientRelTabPage.ResumeLayout(false);
			this.CommunicationsTabPage.ResumeLayout(false);
			this.SalesActivityTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
