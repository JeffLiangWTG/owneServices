using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class OrgContactSelectionForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContactGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PromptLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShowInactiveContactsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactGrid)).BeginInit();
			this.ContactGrid.SuspendLayout();
			this.ContactGroupBox.SuspendLayout();
			this.ContactPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 326, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper);
			// 
			// ContactGrid
			// 
			this.ContactGrid.AllowNavigation = false;
			this.ContactGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).OC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).OC_ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).OC_Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).OC_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).ParentOrg.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).Collection)).SyncRoot)).OrganisationCode)));
			this.ContactGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|6AA03139-C38B-44EF-8810-62FFFBC62CA7", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "OC_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|17B24EB3-375D-4698-BD40-B3148B938531", "Contact Name");
			zTextBoxColumnStyleInfo1.ColumnName = "OC_ContactName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|9302f14d-4ced-41bd-abc8-f1276e02fec8", "Title");
			zTextBoxColumnStyleInfo2.ColumnName = "OC_Title";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|8453ed22-561d-4888-836d-bebc97ebb415", "Email");
			zTextBoxColumnStyleInfo3.ColumnName = "OC_Email";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|B48F4F07-B674-4440-972C-F2E58D968051", "Organization Full Name");
			zTextBoxColumnStyleInfo4.ColumnName = "ParentOrg+OH_FullName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgContactSelectionForm|9263a2db-f4ce-402f-94c0-e1a4ffea356a", "Organization Code");
			zTextBoxColumnStyleInfo5.ColumnName = "OrganisationCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContactGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ContactGrid.GridId = "4F75F737-4C03-4120-9F3D-2F5850C741E8";
			this.ContactGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactGrid.IsWholeRowSelectedOnClick = true;
			this.ContactGrid.LayoutKey = "zGrid1";
			this.ContactGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 40, true);
			this.ContactGrid.Name = "ContactGrid";
			this.ContactGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 240, true);
			this.ContactGrid.TabIndex = 1;
			this.ContactGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.ContactGrid_ColourDeciding);
			this.ContactGrid.DoubleClick += new System.EventHandler(this.ContactGrid_DoubleClick);
			this.ContactGrid.ReadOnly = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8c36f3a7-a31b-4295-a17b-36e77cde1fc3", "OK");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(826, 302, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.SaveButton.TabIndex = 6;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("60a47bf4-7c01-4c24-b81d-675f4094173a", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(909, 302, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// PromptLabel
			// 
			this.PromptLabel.AutoSize = true;
			this.PromptLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4D5E1907-171F-4997-9470-2A51824A68DA", "Multiple contacts exist sharing the same email address. Please select the preferred contact");
			this.PromptLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PromptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 16, true);
			this.PromptLabel.Name = "PromptLabel";
			this.PromptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 13, true);
			this.PromptLabel.TabIndex = 19;
			// 
			// ContactGroupBox
			// 
			this.ContactGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ContactGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3E71CC87-A230-46DF-AAE1-6D0FE6265733", "Contacts");
			this.ContactGroupBox.Controls.Add(this.ContactGrid);
			this.ContactGroupBox.Controls.Add(this.PromptLabel);
			this.ContactGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 5, true);
			this.ContactGroupBox.Name = "ContactGroupBox";
			this.ContactGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 290, true);
			this.ContactGroupBox.TabIndex = 3;
			this.ContactGroupBox.TabStop = false;
			// 
			// ContactPanel
			// 
			this.ContactPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ContactPanel.Controls.Add(this.ContactGroupBox);
			this.ContactPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContactPanel.Name = "ContactPanel";
			this.ContactPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 300, true);
			this.ContactPanel.TabIndex = 2;
			// 
			// ShowInactiveContactsCheckBox
			// 
			this.ShowInactiveContactsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ShowInactiveContactsCheckBox, "IncludeInactiveContacts");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper)(null)).IncludeInactiveContacts)));
			this.ShowInactiveContactsCheckBox.AutoSize = true;
			this.ShowInactiveContactsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A01D38B0-E965-49A9-99E3-D45CC09963F7", "Show Inactive");
			this.ShowInactiveContactsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowInactiveContactsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 301, true);
			this.ShowInactiveContactsCheckBox.Name = "ShowInactiveContactsCheckBox";
			this.ShowInactiveContactsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.ShowInactiveContactsCheckBox.TabIndex = 5;
			this.ShowInactiveContactsCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgContactSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("07efc3f4-2f12-4e05-a0ff-6be8523e3089", "Multiple Contacts Found");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 350, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.ContactPanel);
			this.Controls.Add(this.ShowInactiveContactsCheckBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.FilteredContactsCollectionWrapper);
			this.Name = "OrgContactSelectionForm";
			this.Controls.SetChildIndex(this.ShowInactiveContactsCheckBox, 0);
			this.Controls.SetChildIndex(this.ContactPanel, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactGrid)).EndInit();
			this.ContactGrid.ResumeLayout(false);
			this.ContactGrid.PerformLayout();
			this.ContactGroupBox.ResumeLayout(false);
			this.ContactGroupBox.PerformLayout();
			this.ContactPanel.ResumeLayout(false);
			this.ContactPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZGrid ContactGrid;
		private ZButton SaveButton;
		private ZButton CloseButton;
		private ZLabel PromptLabel;
		private ZGroupBox ContactGroupBox;
		private ZPanel ContactPanel;
		protected ZCheckBox ShowInactiveContactsCheckBox;
	}
}
