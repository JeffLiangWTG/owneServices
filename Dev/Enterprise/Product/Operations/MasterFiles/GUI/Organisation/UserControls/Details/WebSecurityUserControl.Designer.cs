using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WebSecurityUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.WebSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientPortalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeveloperGuideLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.OM_CMClientPortalHomePageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WebSecurityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SyncWebSecurityToNeoGroupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SetWebSecurityButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SecurityRightsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SecurityInstructionsLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SecurityContactRightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SecurityContactRightGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WebSecurityPanel.SuspendLayout();
			this.ClientPortalGroupBox.SuspendLayout();
			this.WebSecurityGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecurityRightsGrid)).BeginInit();
			this.SecurityRightsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecurityContactRightGrid)).BeginInit();
			this.SecurityContactRightGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// WebSecurityPanel
			// 
			this.WebSecurityPanel.Controls.Add(this.ClientPortalGroupBox);
			this.WebSecurityPanel.Controls.Add(this.WebSecurityGroupBox);
			this.WebSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WebSecurityPanel.Name = "WebSecurityPanel";
			this.WebSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 474, true);
			this.WebSecurityPanel.TabIndex = 0;
			// 
			// ClientPortalGroupBox
			// 
			this.ClientPortalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ClientPortalGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebSecurityUserControl|cfdf4fec-2202-43b2-95f9-6fdbee0657f8", "Client Portal Home Page");
			this.ClientPortalGroupBox.Controls.Add(this.DeveloperGuideLinkLabel);
			this.ClientPortalGroupBox.Controls.Add(this.OM_CMClientPortalHomePageTextBox);
			this.ClientPortalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientPortalGroupBox.Name = "ClientPortalGroupBox";
			this.ClientPortalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 62, true);
			this.ClientPortalGroupBox.TabIndex = 10;
			this.ClientPortalGroupBox.TabStop = false;
			// 
			// DeveloperGuideLinkLabel
			// 
			this.DeveloperGuideLinkLabel.AutoSize = true;
			this.DeveloperGuideLinkLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebSecurityUserControl|035d28b5-e217-4dc6-8e93-23d51aabe839", "Show Developer Guide");
			this.DeveloperGuideLinkLabel.IsFontBold = false;
			this.DeveloperGuideLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 16, true);
			this.DeveloperGuideLinkLabel.Name = "DeveloperGuideLinkLabel";
			this.DeveloperGuideLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 13, true);
			this.DeveloperGuideLinkLabel.TabIndex = 10;
			this.DeveloperGuideLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.zLinkLabel1_LinkClicked);
			// 
			// OM_CMClientPortalHomePageTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_CMClientPortalHomePageTextBox, "MiscServ+OM_CMClientPortalHomePage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMClientPortalHomePage)));
			this.OM_CMClientPortalHomePageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OM_CMClientPortalHomePageTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OM_CMClientPortalHomePageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 34, true);
			this.OM_CMClientPortalHomePageTextBox.Name = "OM_CMClientPortalHomePageTextBox";
			this.OM_CMClientPortalHomePageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 20, true);
			this.OM_CMClientPortalHomePageTextBox.TabIndex = 9;
			// 
			// WebSecurityGroupBox
			// 
			this.WebSecurityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.WebSecurityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebSecurityUserControl|c8d468bf-0841-49d4-a5ec-204b1541e3cb", "Web Security Settings");
			this.WebSecurityGroupBox.Controls.Add(this.splitContainer);
			this.WebSecurityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.WebSecurityGroupBox.Name = "WebSecurityGroupBox";
			this.WebSecurityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 406, true);
			this.WebSecurityGroupBox.TabIndex = 9;
			this.WebSecurityGroupBox.TabStop = false;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			//
			this.splitContainer.Panel1.Controls.Add(this.SyncWebSecurityToNeoGroupButton);
			this.splitContainer.Panel1.Controls.Add(this.SetWebSecurityButton);
			this.splitContainer.Panel1.Controls.Add(this.SecurityRightsGrid);
			this.splitContainer.Panel1.Controls.Add(this.SecurityInstructionsLabel);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.SecurityContactRightLabel);
			this.splitContainer.Panel2.Controls.Add(this.SecurityContactRightGrid);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 387, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			this.splitContainer.TabIndex = 9;
			// 
			// SyncWebSecurityToNeoGroupButton
			// 
			this.SyncWebSecurityToNeoGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SyncWebSecurityToNeoGroupButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("91d30369-6873-4aa7-8376-c411409a8b45", "Sync WebTracker Security to Neo");
			this.SyncWebSecurityToNeoGroupButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.SyncWebSecurityToNeoGroupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 165, true);
			this.SyncWebSecurityToNeoGroupButton.Name = "SyncWebSecurityToNeoGroupButton";
			this.SyncWebSecurityToNeoGroupButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.SyncWebSecurityToNeoGroupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 21, true);
			this.SyncWebSecurityToNeoGroupButton.TabIndex = 6;
			this.SyncWebSecurityToNeoGroupButton.ToolTipCaption = null;
			this.SyncWebSecurityToNeoGroupButton.Click += new System.EventHandler(this.SyncWebSecurityToNeoGroupButton_Click);
			// 
			// SetWebSecurityButton
			// 
			this.SetWebSecurityButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SetWebSecurityButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ea3e796b-40d5-40fe-af9a-37f5371dc9ea", "Apply Security Profile");
			this.SetWebSecurityButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.SetWebSecurityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 165, true);
			this.SetWebSecurityButton.Name = "SetWebSecurityButton";
			this.SetWebSecurityButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.SetWebSecurityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 21, true);
			this.SetWebSecurityButton.TabIndex = 7;
			this.SetWebSecurityButton.ToolTipCaption = null;
			this.SetWebSecurityButton.Click += new System.EventHandler(this.SetWebSecurityButton_Click);
			// 
			// SecurityRightsGrid
			// 
			this.SecurityRightsGrid.AllowNavigation = false;
			this.SecurityRightsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityRightsGrid, "SecurityRightsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).OX_SecurityItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).OX_Granted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).OX_IsCustomerManaged)));
			this.SecurityRightsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Res.GetData("WebSecurityUserControl|SecurityItemNameForDisplay", "Security Item Name For Display");
			zDropEditColumnStyleInfo1.ColumnName = "SecurityItemNameForDisplay";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "OX_Granted";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("WebSecurityUserControl|CustomerManaged", "Customer Managed");
			zCheckBoxColumnStyleInfo3.ColumnName = "OX_IsCustomerManaged";
			zCheckBoxColumnStyleInfo3.IsMandatory = true;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SecurityRightsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SecurityRightsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SecurityRightsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.SecurityRightsGrid.GridId = "b9c5fd6b-794f-4e33-af5b-ac546a6e884e";
			this.SecurityRightsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecurityRightsGrid.LayoutKey = "SecurityRightsGrid";
			this.SecurityRightsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.SecurityRightsGrid.Name = "SecurityRightsGrid";
			this.SecurityRightsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SecurityRightsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 123, true);
			this.SecurityRightsGrid.TabIndex = 5;
			// 
			// SecurityInstructionsLabel
			// 
			this.SecurityInstructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityInstructionsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebSecurityUserControl|b66bf2b2-c741-4bf1-bf84-88796eed6995", "The following security rights are the default rights used for Web access to this system. Each contact will have these rights unless otherwise overridden.");
			this.SecurityInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.SecurityInstructionsLabel.Name = "SecurityInstructionsLabel";
			this.SecurityInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 34, true);
			this.SecurityInstructionsLabel.TabIndex = 7;
			this.SecurityInstructionsLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowPortalLabel_LinkClicked);
			// 
			// SecurityContactRightLabel
			// 
			this.SecurityContactRightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityContactRightLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebSecurityUserControl|f2358ec2-9815-4b6c-b058-29a27e49ebf7", "The security right for each contact is shown here. These can be overridden from the organization\'s default.");
			this.SecurityContactRightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.SecurityContactRightLabel.Name = "SecurityContactRightLabel";
			this.SecurityContactRightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 16, true);
			this.SecurityContactRightLabel.TabIndex = 8;
			// 
			// SecurityContactRightGrid
			// 
			this.SecurityContactRightGrid.AllowNavigation = false;
			this.SecurityContactRightGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityContactRightGrid, "SecurityRightsView.ContactSecurityRights");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).ContactSecurityRights)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSecurityContacts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).ContactSecurityRights)).SyncRoot)).OZ_OC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityContacts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SecurityRights)).SyncRoot)).ContactSecurityRights)).SyncRoot)).OZ_Granted)));
			this.SecurityContactRightGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OZ_OC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo2.ColumnName = "OZ_Granted";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			this.SecurityContactRightGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SecurityContactRightGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.SecurityContactRightGrid.GridId = "b7b6772f-5ec5-45d3-8e87-557f780b5083";
			this.SecurityContactRightGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecurityContactRightGrid.LayoutKey = "SecurityRightsGrid";
			this.SecurityContactRightGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
			this.SecurityContactRightGrid.Name = "SecurityContactRightGrid";
			this.SecurityContactRightGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SecurityContactRightGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 164, true);
			this.SecurityContactRightGrid.TabIndex = 7;
			// 
			// WebSecurityUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WebSecurityPanel);
			this.Name = "WebSecurityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 474, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WebSecurityPanel.ResumeLayout(false);
			this.WebSecurityPanel.PerformLayout();
			this.ClientPortalGroupBox.ResumeLayout(false);
			this.ClientPortalGroupBox.PerformLayout();
			this.WebSecurityGroupBox.ResumeLayout(false);
			this.WebSecurityGroupBox.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecurityRightsGrid)).EndInit();
			this.SecurityRightsGrid.ResumeLayout(false);
			this.SecurityRightsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecurityContactRightGrid)).EndInit();
			this.SecurityContactRightGrid.ResumeLayout(false);
			this.SecurityContactRightGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel WebSecurityPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox WebSecurityGroupBox;
		private Enterprise.ZArchitecture.ZLabel SecurityContactRightLabel;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel SecurityInstructionsLabel;
		private Enterprise.ZArchitecture.ZGrid SecurityContactRightGrid;
		protected Enterprise.ZArchitecture.ZGrid SecurityRightsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ClientPortalGroupBox;
		private Enterprise.ZArchitecture.ZTextBox OM_CMClientPortalHomePageTextBox;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel DeveloperGuideLinkLabel;
        private CargoWise.Windows.UI.KSplitContainer splitContainer;
		protected ZArchitecture.GUI.ZButton SyncWebSecurityToNeoGroupButton;
		protected ZArchitecture.GUI.ZButton SetWebSecurityButton;
	}
}
