using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class UpdateContactsForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NDRContactReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeactivateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumCampaignsSentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).BeginInit();
			this.ContactsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendersGrid)).BeginInit();
			this.SendersGrid.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.DeliveryDetailsGroupBox.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 674, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater);
			// 
			// NDRContactReportButton
			// 
			this.NDRContactReportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NDRContactReportButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4d6f727c-a143-44f9-8129-2ed9f79769a6", "Delivery Failure Report");
			this.NDRContactReportButton.FlatAppearance.BorderSize = 0;
			this.NDRContactReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 1, true);
			this.NDRContactReportButton.Name = "NDRContactReportButton";
			this.NDRContactReportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NDRContactReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 24, true);
			this.NDRContactReportButton.TabIndex = 3;
			this.NDRContactReportButton.UseVisualStyleBackColor = true;
			// 
			// DeactivateButton
			// 
			this.DeactivateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeactivateButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("947c61b3-f299-4360-954f-66383fc2a4d4", "Deactivate Contacts");
			this.DeactivateButton.FlatAppearance.BorderSize = 0;
			this.DeactivateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 1, true);
			this.DeactivateButton.Name = "DeactivateButton";
			this.DeactivateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeactivateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 24, true);
			this.DeactivateButton.TabIndex = 4;
			this.DeactivateButton.UseVisualStyleBackColor = true;
			// 
			// NumCampaignsSentLabel
			// 
			this.NumCampaignsSentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NumCampaignsSentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 10, true);
			this.NumCampaignsSentLabel.Name = "NumCampaignsSentLabel";
			this.NumCampaignsSentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 97, true);
			this.NumCampaignsSentLabel.TabIndex = 5;
			// 
			// SendersLabel
			// 
			this.SendersLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SendersLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CF13027A-F267-460F-B436-E0C2B8383F68", "A time zone could not be determined for the following email sender(s). Please set a Home Branch in attempt to resend. Double click to edit.");
			this.SendersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 450, true);
			this.SendersLabel.Name = "SendersLabel";
			this.SendersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 20, true);
			this.SendersLabel.TabIndex = 5;
			// 
			// DeliveryDetailsTextBox
			// 
			this.DeliveryDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeliveryDetailsTextBox, "BounceBackEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).BounceBackEmail)));
			this.DeliveryDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryDetailsTextBox, false);
			this.DeliveryDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.DeliveryDetailsTextBox.Multiline = true;
			this.DeliveryDetailsTextBox.Name = "DeliveryDetailsTextBox";
			this.DeliveryDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DeliveryDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 321, true);
			this.DeliveryDetailsTextBox.TabIndex = 1;
			// 
			// ContactsGrid
			// 
			this.ContactsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContactsGrid, "ContactsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignContact)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)).SyncRoot)).CurrentActionsAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignContact)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)).SyncRoot)).VCC_OrgFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignContact)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)).SyncRoot)).VCC_ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.CampaignContact)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)).SyncRoot)).IsNDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignContact)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).ContactsCollection)).SyncRoot)).VCC_Email)));
			this.ContactsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("95c8cf55-a490-43d3-9d51-bce79bfbf4d5", "Action");
			zTextBoxColumnStyleInfo1.ColumnName = "CurrentActionsAsText";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6ad531a4-3060-4d38-b1ae-4f604cc8f246", "Client Name");
			zTextBoxColumnStyleInfo2.ColumnName = "VCC_OrgFullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("09e08aa9-72d8-4a1c-bc14-667739e2035f", "Primary Contact");
			zTextBoxColumnStyleInfo3.ColumnName = "VCC_ContactName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c2a01413-61dd-4d6b-a61c-8253a30fc7a5", "NDR");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsNDR";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b64e5bdf-f01a-417a-9b3f-5aebed9cb1b0", "Email");
			zTextBoxColumnStyleInfo4.ColumnName = "VCC_Email";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContactsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContactsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsGrid.GridId = "30bf9fba-6002-4f45-99f1-a1f9a599c58b";
			this.ContactsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactsGrid.LayoutKey = "ContactsGrid";
			this.ContactsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ContactsGrid.Name = "ContactsGrid";
			this.ContactsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 329, true);
			this.ContactsGrid.TabIndex = 0;
			// 
			// SendersGrid
			// 
			this.SendersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SendersGrid, "SendersCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)).SyncRoot)).GS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)).SyncRoot)).GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)).SyncRoot)).BranchCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)).SyncRoot)).GS_GB_HomeBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater)(null)).SendersCollection)).SyncRoot)).BranchDescription)));
			this.SendersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "GS_Code";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "GS_FullName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2499b507-4ac6-4940-8bc7-a152109e2eb2", "Country/Region");
			zTextBoxColumnStyleInfo7.ColumnName = "BranchCountry";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "GS_GB_HomeBranch";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b8c249b7-4eb4-4544-9617-0bbf49957217", "Branch Name");
			zTextBoxColumnStyleInfo8.ColumnName = "BranchDescription";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SendersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SendersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SendersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SendersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.SendersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SendersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SendersGrid.GridId = "04D6032A-8824-436B-91C9-60FBA0098397";
			this.SendersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SendersGrid.IsWholeRowSelectedOnClick = true;
			this.SendersGrid.LayoutKey = "SendersGrid";
			this.SendersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.SendersGrid.Name = "SendersGrid";
			this.SendersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 170, true);
			this.SendersGrid.TabIndex = 2;
			this.SendersGrid.DoubleClick += new System.EventHandler(this.SendersGrid_DoubleClick);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 1, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Controls.Add(this.NDRContactReportButton);
			this.bottomPanel.Controls.Add(this.DeactivateButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 698, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 28, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// DeliveryDetailsGroupBox
			// 
			this.DeliveryDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeliveryDetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("cd22d03c-cfd9-43a9-8a4e-2d8bd026f1ea", "Delivery Details");
			this.DeliveryDetailsGroupBox.Controls.Add(this.DeliveryDetailsTextBox);
			this.DeliveryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 100, true);
			this.DeliveryDetailsGroupBox.Name = "DeliveryDetailsGroupBox";
			this.DeliveryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 341, true);
			this.DeliveryDetailsGroupBox.TabIndex = 4;
			this.DeliveryDetailsGroupBox.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("44338c38-586f-40e3-8c99-0702b909140d", "Non Delivery Receipts");
			this.groupBox1.Controls.Add(this.ContactsGrid);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 110, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 331, true);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("75c4467d-a5af-4251-b8f1-5ad33a008ec7", "Missing Home Branches");
			this.groupBox2.Controls.Add(this.SendersGrid);
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 470, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 186, true);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			// 
			// UpdateContactsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 726, true);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.DeliveryDetailsGroupBox);
			this.Controls.Add(this.SendersLabel);
			this.Controls.Add(this.NumCampaignsSentLabel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.ContactsWithNonDeliveryReportsUpdater);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 725, true);
			this.Name = "UpdateContactsForm";
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NumCampaignsSentLabel, 0);
			this.Controls.SetChildIndex(this.SendersLabel, 0);
			this.Controls.SetChildIndex(this.DeliveryDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.groupBox1, 0);
			this.Controls.SetChildIndex(this.groupBox2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).EndInit();
			this.ContactsGrid.ResumeLayout(false);
			this.ContactsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendersGrid)).EndInit();
			this.SendersGrid.ResumeLayout(false);
			this.SendersGrid.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.DeliveryDetailsGroupBox.ResumeLayout(false);
			this.DeliveryDetailsGroupBox.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		protected ZArchitecture.ZGrid ContactsGrid;
		protected ZArchitecture.ZGrid SendersGrid;
		private ZArchitecture.GUI.ZButton NDRContactReportButton;
		private ZArchitecture.GUI.ZButton DeactivateButton;
		protected Enterprise.ZArchitecture.ZLabel NumCampaignsSentLabel;
		protected Enterprise.ZArchitecture.ZLabel SendersLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DeliveryDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox DeliveryDetailsTextBox;
		private ZGroupBox groupBox1;
		private ZGroupBox groupBox2;
	}
}
