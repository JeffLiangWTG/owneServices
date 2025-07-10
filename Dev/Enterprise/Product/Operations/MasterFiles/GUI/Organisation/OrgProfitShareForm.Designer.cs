namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgProfitShareForm
	{

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.GUI.ZDropEdit ProfitShareTypeDropEdit;
		ZOrganisationControl HeadOfficeOrgControl;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl ProfitShareTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage GenericProfitShareTabPage;
		ProfitShareAgreementControl ProfitShareControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ClientSpecificProfitShareTabPage;
		ProfitShareAgreementControl ClientSpecificProfitShareControl;
		Enterprise.ZArchitecture.ZLabel ProfitShareDetailsLabel;
		ZOrganisationControl RcvAgentOrgControl;
		ZOrganisationControl SendAgentOrgControl;
		System.ComponentModel.IContainer components;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ProfitShareTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HeadOfficeOrgControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ProfitShareTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.GenericProfitShareTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProfitShareControl = new Enterprise.MasterFiles.GUI.ProfitShareAgreementControl();
			this.ClientSpecificProfitShareTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClientSpecificProfitShareControl = new Enterprise.MasterFiles.GUI.ProfitShareAgreementControl();
			this.ProfitShareDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RcvAgentOrgControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.SendAgentOrgControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfitShareTabControl.SuspendLayout();
			this.GenericProfitShareTabPage.SuspendLayout();
			this.ClientSpecificProfitShareTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 573, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ProfitShareTypeDropEdit);
			this.MainTabPage.Controls.Add(this.HeadOfficeOrgControl);
			this.MainTabPage.Controls.Add(this.zLabel1);
			this.MainTabPage.Controls.Add(this.ProfitShareTabControl);
			this.MainTabPage.Controls.Add(this.ProfitShareDetailsLabel);
			this.MainTabPage.Controls.Add(this.RcvAgentOrgControl);
			this.MainTabPage.Controls.Add(this.SendAgentOrgControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 546, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(689);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgAgentRelationship);
			// 
			// ProfitShareTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ProfitShareTypeDropEdit, "O3_ProfitShareType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).O3_ProfitShareType)));
			this.ProfitShareTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 9, true);
			this.ProfitShareTypeDropEdit.Name = "ProfitShareTypeDropEdit";
			this.ProfitShareTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ProfitShareTypeDropEdit.TabIndex = 7;
			// 
			// HeadOfficeOrgControl
			// 
			this.BindingSource.SetBindingMember(this.HeadOfficeOrgControl, "O3_OH_GroupNetworkOrFranchise");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).O3_OH_GroupNetworkOrFranchise)));
			this.HeadOfficeOrgControl.BindToOrganisations = "Lookups+GroupNetworkOrFranchises";
			this.HeadOfficeOrgControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|59b91051-7cff-4d27-b390-298854d1763a", "Head Office");
			this.HeadOfficeOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 38, true);
			this.HeadOfficeOrgControl.Name = "HeadOfficeOrgControl";
			this.HeadOfficeOrgControl.PopupCaption = "";
			this.HeadOfficeOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.HeadOfficeOrgControl.TabIndex = 10;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel1, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).Description)));
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|8e8ca578-9f23-4be9-a08b-e7121e433725", "XXXXX - YYYYY");
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 197, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.zLabel1.TabIndex = 12;
			// 
			// ProfitShareTabControl
			// 
			this.ProfitShareTabControl.Controls.Add(this.GenericProfitShareTabPage);
			this.ProfitShareTabControl.Controls.Add(this.ClientSpecificProfitShareTabPage);
			this.ProfitShareTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 224, true);
			this.ProfitShareTabControl.Name = "ProfitShareTabControl";
			this.ProfitShareTabControl.SelectedIndex = 0;
			this.ProfitShareTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 319, true);
			this.ProfitShareTabControl.TabIndex = 13;
			// 
			// GenericProfitShareTabPage
			// 
			this.GenericProfitShareTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|9e8707b8-1df9-465e-9990-1b107d0167b7", "Generic");
			this.GenericProfitShareTabPage.Controls.Add(this.ProfitShareControl);
			this.GenericProfitShareTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GenericProfitShareTabPage.Name = "GenericProfitShareTabPage";
			this.GenericProfitShareTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 292, true);
			this.GenericProfitShareTabPage.TabIndex = 0;
			// 
			// ProfitShareControl
			// 
			this.BindingSource.SetBindingMember(this.ProfitShareControl, "GenericProfitShareDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).GenericProfitShareDetails)).SyncRoot)))));
			this.ProfitShareControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfitShareControl.IncludeOrgOverrideColumn = false;
			this.ProfitShareControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProfitShareControl.Name = "ProfitShareControl";
			this.ProfitShareControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 292, true);
			this.ProfitShareControl.TabIndex = 0;
			// 
			// ClientSpecificProfitShareTabPage
			// 
			this.ClientSpecificProfitShareTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|ce606ef9-ff26-4347-8d06-c31a7267046c", "Client Specific");
			this.ClientSpecificProfitShareTabPage.Controls.Add(this.ClientSpecificProfitShareControl);
			this.ClientSpecificProfitShareTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ClientSpecificProfitShareTabPage.Name = "ClientSpecificProfitShareTabPage";
			this.ClientSpecificProfitShareTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 292, true);
			this.ClientSpecificProfitShareTabPage.TabIndex = 1;
			// 
			// profitShareAgreementControl1
			// 
			this.BindingSource.SetBindingMember(this.ClientSpecificProfitShareControl, "ClientSpecificProfitShareDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).ClientSpecificProfitShareDetails)).SyncRoot)))));
			this.ClientSpecificProfitShareControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientSpecificProfitShareControl.IncludeOrgOverrideColumn = true;
			this.ClientSpecificProfitShareControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientSpecificProfitShareControl.Name = "ClientSpecificProfitShareControl";
			this.ClientSpecificProfitShareControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 292, true);
			this.ClientSpecificProfitShareControl.TabIndex = 1;
			// 
			// ProfitShareDetailsLabel
			// 
			this.ProfitShareDetailsLabel.AutoSize = true;
			this.ProfitShareDetailsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|ec879279-4c2a-42a1-a5a9-1a45591a9482", "Profit Share Setup");
			this.ProfitShareDetailsLabel.IsFontBold = true;
			this.ProfitShareDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 197, true);
			this.ProfitShareDetailsLabel.Name = "ProfitShareDetailsLabel";
			this.ProfitShareDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 13, true);
			this.ProfitShareDetailsLabel.TabIndex = 11;
			// 
			// RcvAgentOrgControl
			// 
			this.BindingSource.SetBindingMember(this.RcvAgentOrgControl, "O3_OH_ReceivingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).O3_OH_ReceivingAgent)));
			this.RcvAgentOrgControl.BindToOrganisations = "Lookups.ReceivingAgents";
			this.RcvAgentOrgControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|ccf1a3f1-2b46-42d0-b131-d5342fbfaa74", "Receiving Agent");
			this.RcvAgentOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 38, true);
			this.RcvAgentOrgControl.Name = "RcvAgentOrgControl";
			this.RcvAgentOrgControl.PopupCaption = "";
			this.RcvAgentOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.RcvAgentOrgControl.TabIndex = 9;
			// 
			// SendAgentOrgControl
			// 
			this.BindingSource.SetBindingMember(this.SendAgentOrgControl, "O3_OH_SendingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgAgentRelationship)(null)).O3_OH_SendingAgent)));
			this.SendAgentOrgControl.BindToOrganisations = "Lookups.SendingAgents";
			this.SendAgentOrgControl.Text = "Sending Agent";
			this.SendAgentOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 38, true);
			this.SendAgentOrgControl.Name = "SendAgentOrgControl";
			this.SendAgentOrgControl.PopupCaption = "";
			this.SendAgentOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.SendAgentOrgControl.TabIndex = 8;
			// 
			// OrgProfitShareForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 629, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgProfitShareForm|56de39c6-e0bf-47e2-8634-c305348e32d3", "Profit Share Agreement");
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgAgentRelationship);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 648, true);
			this.Name = "OrgProfitShareForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfitShareTabControl.ResumeLayout(false);
			this.GenericProfitShareTabPage.ResumeLayout(false);
			this.ClientSpecificProfitShareTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
