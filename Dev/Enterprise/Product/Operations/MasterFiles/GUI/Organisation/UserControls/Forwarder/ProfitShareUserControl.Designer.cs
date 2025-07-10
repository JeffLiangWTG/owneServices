using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class ProfitShareUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ProfitShareTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.GenericProfitShareTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GenericProfitShareDetailsControl = new Enterprise.MasterFiles.GUI.ProfitShareAgreementControl();
			this.ClientSpecificProfitShareTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClientSpecificProfitShareDetailsControl = new Enterprise.MasterFiles.GUI.ProfitShareAgreementControl();
			this.AgentRelationshipsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProfitShareGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProfitShareDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfitShareTabControl.SuspendLayout();
			this.GenericProfitShareTabPage.SuspendLayout();
			this.ClientSpecificProfitShareTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgentRelationshipsGrid)).BeginInit();
			this.ProfitShareGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ProfitShareTabControl
			// 
			this.ProfitShareTabControl.Controls.Add(this.GenericProfitShareTabPage);
			this.ProfitShareTabControl.Controls.Add(this.ClientSpecificProfitShareTabPage);
			this.ProfitShareTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 38, true);
			this.ProfitShareTabControl.Name = "ProfitShareTabControl";
			this.ProfitShareTabControl.SelectedIndex = 0;
			this.ProfitShareTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 473, true);
			this.ProfitShareTabControl.TabIndex = 5;
			// 
			// GenericProfitShareTabPage
			// 
			this.GenericProfitShareTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareUserControl|1ff2c91f-ade3-432a-8ee8-a3e5560f2864", "Generic");
			this.GenericProfitShareTabPage.Controls.Add(this.GenericProfitShareDetailsControl);
			this.GenericProfitShareTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GenericProfitShareTabPage.Name = "GenericProfitShareTabPage";
			this.GenericProfitShareTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 446, true);
			this.GenericProfitShareTabPage.TabIndex = 0;
			// 
			// GenericProfitShareDetailsControl
			// 
			this.BindingSource.SetBindingMember(this.GenericProfitShareDetailsControl, "AgentRelationships.GenericProfitShareDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).GenericProfitShareDetails)).SyncRoot)))));
			this.GenericProfitShareDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenericProfitShareDetailsControl.IncludeOrgOverrideColumn = false;
			this.GenericProfitShareDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GenericProfitShareDetailsControl.Name = "GenericProfitShareDetailsControl";
			this.GenericProfitShareDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 446, true);
			this.GenericProfitShareDetailsControl.TabIndex = 1;
			// 
			// ClientSpecificProfitShareTabPage
			// 
			this.ClientSpecificProfitShareTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareUserControl|85b3825b-24be-4408-82ff-3a24433763fc", "Client Specific");
			this.ClientSpecificProfitShareTabPage.Controls.Add(this.ClientSpecificProfitShareDetailsControl);
			this.ClientSpecificProfitShareTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ClientSpecificProfitShareTabPage.Name = "ClientSpecificProfitShareTabPage";
			this.ClientSpecificProfitShareTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 446, true);
			this.ClientSpecificProfitShareTabPage.TabIndex = 1;
			// 
			// ClientSpecificProfitShareDetailsControl
			// 
			this.BindingSource.SetBindingMember(this.ClientSpecificProfitShareDetailsControl, "AgentRelationships.ClientSpecificProfitShareDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).ClientSpecificProfitShareDetails)).SyncRoot)))));
			this.ClientSpecificProfitShareDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientSpecificProfitShareDetailsControl.IncludeOrgOverrideColumn = true;
			this.ClientSpecificProfitShareDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientSpecificProfitShareDetailsControl.Name = "ClientSpecificProfitShareDetailsControl";
			this.ClientSpecificProfitShareDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 446, true);
			this.ClientSpecificProfitShareDetailsControl.TabIndex = 2;
			// 
			// AgentRelationshipsGrid
			// 
			this.AgentRelationshipsGrid.AllowNavigation = false;
			this.AgentRelationshipsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AgentRelationshipsGrid, "AgentRelationships");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).O3_ProfitShareType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).O3_OH_ReceivingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).O3_OH_SendingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AgentRelationships)).SyncRoot)).O3_OH_GroupNetworkOrFranchise)));
			this.AgentRelationshipsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "O3_ProfitShareType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "O3_OH_ReceivingAgent";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "O3_OH_SendingAgent";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "O3_OH_GroupNetworkOrFranchise";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.AgentRelationshipsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AgentRelationshipsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AgentRelationshipsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.AgentRelationshipsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.AgentRelationshipsGrid.GridId = "8d4ab8da-32c4-441b-83fb-74e5c05f67cf";
			this.AgentRelationshipsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AgentRelationshipsGrid.LayoutKey = "zGrid1";
			this.AgentRelationshipsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 38, true);
			this.AgentRelationshipsGrid.Name = "AgentRelationshipsGrid";
			this.AgentRelationshipsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 469, true);
			this.AgentRelationshipsGrid.TabIndex = 4;
			// 
			// ProfitShareGroupBox
			// 
			this.ProfitShareGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareUserControl|4ae1aa34-54f9-4cdf-a7c0-1a6d4012f2fe", "Profit Share Agreements");
			this.ProfitShareGroupBox.Controls.Add(this.ProfitShareDescriptionLabel);
			this.ProfitShareGroupBox.Controls.Add(this.ProfitShareTabControl);
			this.ProfitShareGroupBox.Controls.Add(this.AgentRelationshipsGrid);
			this.ProfitShareGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfitShareGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProfitShareGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ProfitShareGroupBox.Name = "ProfitShareGroupBox";
			this.ProfitShareGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 517, true);
			this.ProfitShareGroupBox.TabIndex = 6;
			this.ProfitShareGroupBox.TabStop = false;
			// 
			// ProfitShareDescriptionLabel
			// 
			this.ProfitShareDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProfitShareDescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareUserControl|f7bd4252-730e-4bb7-9db6-c6a442535857", "This section allows you to specify any profit share agreements you may have with your agents.");
			this.ProfitShareDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ProfitShareDescriptionLabel.Name = "ProfitShareDescriptionLabel";
			this.ProfitShareDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 19, true);
			this.ProfitShareDescriptionLabel.TabIndex = 6;
			// 
			// ProfitShareUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProfitShareGroupBox);
			this.Name = "ProfitShareUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 517, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfitShareTabControl.ResumeLayout(false);
			this.GenericProfitShareTabPage.ResumeLayout(false);
			this.ClientSpecificProfitShareTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AgentRelationshipsGrid)).EndInit();
			this.ProfitShareGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl ProfitShareTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage GenericProfitShareTabPage;
		private ProfitShareAgreementControl GenericProfitShareDetailsControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ClientSpecificProfitShareTabPage;
		private ProfitShareAgreementControl ClientSpecificProfitShareDetailsControl;
		private Enterprise.ZArchitecture.ZGrid AgentRelationshipsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProfitShareGroupBox;
		private Enterprise.ZArchitecture.ZLabel ProfitShareDescriptionLabel;
	}
}
