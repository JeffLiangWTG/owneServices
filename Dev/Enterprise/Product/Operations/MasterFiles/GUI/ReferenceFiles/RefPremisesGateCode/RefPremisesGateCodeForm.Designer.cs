namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPremisesGateCodeForm
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEditDataProvider = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEditOrgRegCode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBoxServices = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WharfCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CFSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CYardCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTextBoxPCode = new Enterprise.ZArchitecture.ZTextBox();
			this.R5_DescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.EquipmentGroupBox.SuspendLayout();
			this.zDropEditDataProvider.SuspendLayout();
			this.zDropEditOrgRegCode.SuspendLayout();
			this.zGroupBoxServices.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(842);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPremisesGateCode);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 300, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPremisesGateCodeForm|66d99372-76a2-41a4-84e1-392120d72ad1", "Premises Gate Code");
			this.MainTabPage.Controls.Add(this.EquipmentGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 277, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// EquipmentGroupBox
			// 
			this.EquipmentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.EquipmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPremisesGateCodeForm|2655b179-6b21-40fb-b51f-60a1b3e4901f", "Details");
			this.EquipmentGroupBox.Controls.Add(this.zDropEditDataProvider);
			this.EquipmentGroupBox.Controls.Add(this.zDropEditOrgRegCode);
			this.EquipmentGroupBox.Controls.Add(this.zGroupBoxServices);
			this.EquipmentGroupBox.Controls.Add(this.zTextBoxPCode);
			this.EquipmentGroupBox.Controls.Add(this.R5_DescriptionBoundTextBox);
			this.EquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.EquipmentGroupBox.Name = "EquipmentGroupBox";
			this.EquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 248, true);
			this.EquipmentGroupBox.TabIndex = 0;
			this.EquipmentGroupBox.TabStop = false;
			// 
			// zDropEditDataProvider
			// 
			this.BindingSource.SetBindingMember(this.zDropEditDataProvider, "R5_DataProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_DataProvider)));
			this.zDropEditDataProvider.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 49, true);
			this.zDropEditDataProvider.Name = "zDropEditDataProvider";
			this.zDropEditDataProvider.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 19, true);
			this.zDropEditDataProvider.TabIndex = 3;
			// 
			// zDropEditOrgRegCode
			// 
			this.BindingSource.SetBindingMember(this.zDropEditOrgRegCode, "R5_OrgRegCodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_OrgRegCodeType)));
			this.zDropEditOrgRegCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 23, true);
			this.zDropEditOrgRegCode.Name = "zDropEditOrgRegCode";
			this.zDropEditOrgRegCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 19, true);
			this.zDropEditOrgRegCode.TabIndex = 1;
			// 
			// zGroupBoxServices
			// 
			this.zGroupBoxServices.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPremisesGateCodeForm|d51bb105-ff7a-4fd8-8b37-ed82cf7ce62c", "Services");
			this.zGroupBoxServices.Controls.Add(this.WharfCheckBox);
			this.zGroupBoxServices.Controls.Add(this.CFSCheckBox);
			this.zGroupBoxServices.Controls.Add(this.CYardCheckBox);
			this.zGroupBoxServices.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 23, true);
			this.zGroupBoxServices.Name = "zGroupBoxServices";
			this.zGroupBoxServices.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 99, true);
			this.zGroupBoxServices.TabIndex = 8;
			this.zGroupBoxServices.TabStop = false;
			// 
			// WharfCheckBox
			// 
			this.WharfCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WharfCheckBox, "R5_IsWharf");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_IsWharf)));
			this.WharfCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WharfCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 23, true);
			this.WharfCheckBox.Name = "WharfCheckBox";
			this.WharfCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.WharfCheckBox.TabIndex = 0;
			this.WharfCheckBox.UseVisualStyleBackColor = true;
			// 
			// CFSCheckBox
			// 
			this.CFSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CFSCheckBox, "R5_IsCFS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_IsCFS)));
			this.CFSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CFSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 69, true);
			this.CFSCheckBox.Name = "CFSCheckBox";
			this.CFSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.CFSCheckBox.TabIndex = 2;
			this.CFSCheckBox.UseVisualStyleBackColor = true;
			// 
			// CYardCheckBox
			// 
			this.CYardCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CYardCheckBox, "R5_IsContainerYard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_IsContainerYard)));
			this.CYardCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CYardCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 46, true);
			this.CYardCheckBox.Name = "CYardCheckBox";
			this.CYardCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.CYardCheckBox.TabIndex = 1;
			this.CYardCheckBox.UseVisualStyleBackColor = true;
			// 
			// zTextBoxPCode
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxPCode, "R5_PremisesGateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_PremisesGateCode)));
			this.zTextBoxPCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 75, true);
			this.zTextBoxPCode.Name = "zTextBoxPCode";
			this.zTextBoxPCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 19, true);
			this.zTextBoxPCode.TabIndex = 5;
			// 
			// R5_DescriptionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.R5_DescriptionBoundTextBox, "R5_PremisesGateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPremisesGateCode)(null)).R5_PremisesGateDescription)));
			this.R5_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.R5_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 101, true);
			this.R5_DescriptionBoundTextBox.Name = "R5_DescriptionBoundTextBox";
			this.R5_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 19, true);
			this.R5_DescriptionBoundTextBox.TabIndex = 7;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 277, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 277, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 314, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// RefPremisesGateCodeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPremisesGateCodeForm|327836f9-ad91-4dab-a76d-99164d62f613", "Premises Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 370, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPremisesGateCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 396, true);
			this.Name = "RefPremisesGateCodeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.EquipmentGroupBox.ResumeLayout(false);
			this.EquipmentGroupBox.PerformLayout();
			this.zDropEditDataProvider.ResumeLayout(true);
			this.zDropEditDataProvider.PerformLayout();
			this.zDropEditOrgRegCode.ResumeLayout(true);
			this.zDropEditOrgRegCode.PerformLayout();
			this.zGroupBoxServices.ResumeLayout(false);
			this.zGroupBoxServices.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox EquipmentGroupBox;
		private Enterprise.ZArchitecture.ZTextBox R5_DescriptionBoundTextBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.ZTextBox zTextBoxPCode;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CFSCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CYardCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox WharfCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBoxServices;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditDataProvider;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditOrgRegCode;
	}
}
