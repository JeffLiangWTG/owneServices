namespace Enterprise.MasterFiles.GUI
{
	partial class OrgSecurityProfileControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.glowSecurityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.manageGlowSecurityButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BulkUpdateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisclaimerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BulkUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProfileGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProfileGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SettingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.glowSecurityGroupBox.SuspendLayout();
			this.BulkUpdateGroupBox.SuspendLayout();
			this.ProfileGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfileGrid)).BeginInit();
			this.ProfileGrid.SuspendLayout();
			this.SettingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SettingsGrid)).BeginInit();
			this.SettingsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSecurityProfileCollection);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.glowSecurityGroupBox);
			this.mainSplitContainer.Panel1.Controls.Add(this.BulkUpdateGroupBox);
			this.mainSplitContainer.Panel1.Controls.Add(this.ProfileGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 506, true);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.SettingGroupBox);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(308);
			this.mainSplitContainer.TabIndex = 5;
			// 
			// glowSecurityGroupBox
			// 
			this.glowSecurityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glowSecurityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|GlowSecurity", "GLOW Security");
			this.glowSecurityGroupBox.Controls.Add(this.manageGlowSecurityButton);
			this.glowSecurityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.glowSecurityGroupBox.Name = "glowSecurityGroupBox";
			this.glowSecurityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 48, true);
			this.glowSecurityGroupBox.TabIndex = 4;
			this.glowSecurityGroupBox.TabStop = false;
			// 
			// manageGlowSecurityButton
			// 
			this.manageGlowSecurityButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.manageGlowSecurityButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|ManageGlowSecurity", "GLOW Portal User Admin");
			this.manageGlowSecurityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 15, true);
			this.manageGlowSecurityButton.Name = "manageGlowSecurityButton";
			this.manageGlowSecurityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 23, true);
			this.manageGlowSecurityButton.TabIndex = 3;
			this.manageGlowSecurityButton.ToolTipCaption = null;
			this.manageGlowSecurityButton.UseVisualStyleBackColor = true;
			this.manageGlowSecurityButton.Click += new System.EventHandler(this.ManageGlowSecurityButton_Click);
			// 
			// BulkUpdateGroupBox
			// 
			this.BulkUpdateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BulkUpdateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ec80facb-dde0-4c5b-af7d-189093e80220", "Action");
			this.BulkUpdateGroupBox.Controls.Add(this.DisclaimerLabel);
			this.BulkUpdateGroupBox.Controls.Add(this.BulkUpdateButton);
			this.BulkUpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 335, true);
			this.BulkUpdateGroupBox.Name = "BulkUpdateGroupBox";
			this.BulkUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 171, true);
			this.BulkUpdateGroupBox.TabIndex = 4;
			this.BulkUpdateGroupBox.TabStop = false;
			// 
			// DisclaimerLabel
			// 
			this.DisclaimerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DisclaimerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.DisclaimerLabel.Name = "DisclaimerLabel";
			this.DisclaimerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 126, true);
			this.DisclaimerLabel.TabIndex = 5;
			// 
			// BulkUpdateButton
			// 
			this.BulkUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BulkUpdateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("32622645-e628-408e-b3f7-6213f1a2f615", "Bulk Update");
			this.BulkUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 143, true);
			this.BulkUpdateButton.Name = "BulkUpdateButton";
			this.BulkUpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BulkUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BulkUpdateButton.TabIndex = 4;
			this.BulkUpdateButton.ToolTipCaption = null;
			this.BulkUpdateButton.UseVisualStyleBackColor = true;
			this.BulkUpdateButton.Click += new System.EventHandler(this.BulkUpdateButton_Click);
			// 
			// ProfileGroupBox
			// 
			this.ProfileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Profile", "Profile");
			this.ProfileGroupBox.Controls.Add(this.NewButton);
			this.ProfileGroupBox.Controls.Add(this.ProfileGrid);
			this.ProfileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 56, true);
			this.ProfileGroupBox.Name = "ProfileGroupBox";
			this.ProfileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 274, true);
			this.ProfileGroupBox.TabIndex = 3;
			this.ProfileGroupBox.TabStop = false;
			// 
			// NewButton
			// 
			this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|New", "New");
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 242, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewButton.TabIndex = 3;
			this.NewButton.ToolTipCaption = null;
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// ProfileGrid
			// 
			this.ProfileGrid.AllowNavigation = false;
			this.ProfileGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfileGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).Default)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).Published)));
			this.ProfileGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Name", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Default", "Default");
			zCheckBoxColumnStyleInfo1.ColumnName = "Default";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Published", "Published");
			zCheckBoxColumnStyleInfo2.ColumnName = "Published";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ProfileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProfileGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ProfileGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ProfileGrid.GridId = "e2305b8e-a886-472b-833b-95849fa8c4c8";
			this.ProfileGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProfileGrid.LayoutKey = "ProfileGrid";
			this.ProfileGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.ProfileGrid.Name = "ProfileGrid";
			this.ProfileGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 222, true);
			this.ProfileGrid.TabIndex = 1;
			// 
			// SettingGroupBox
			// 
			this.SettingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Defaults", "Defaults");
			this.SettingGroupBox.Controls.Add(this.SettingsGrid);
			this.SettingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SettingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettingGroupBox.Name = "SettingGroupBox";
			this.SettingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 506, true);
			this.SettingGroupBox.TabIndex = 4;
			this.SettingGroupBox.TabStop = false;
			// 
			// SettingsGrid
			// 
			this.SettingsGrid.AllowNavigation = false;
			this.SettingsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SettingsGrid, "OrgSecuritySettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).OrgSecuritySettings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecurityProfileSetting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).OrgSecuritySettings)).SyncRoot)).SecurityItemNameForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileSetting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).OrgSecuritySettings)).SyncRoot)).Granted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileSetting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecurityProfile)(null)).OrgSecuritySettings)).SyncRoot)).CustomerManaged)));
			this.SettingsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|SecurityItemName", "Security Item Name");
			zTextBoxColumnStyleInfo2.ColumnName = "SecurityItemNameForDisplay";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|Granted", "Granted");
			zCheckBoxColumnStyleInfo3.ColumnName = "Granted";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityProfileControl|CustomerManaged", "Customer Managed");
			zCheckBoxColumnStyleInfo4.ColumnName = "CustomerManaged";
			zCheckBoxColumnStyleInfo4.IsMandatory = true;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SettingsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.SettingsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.SettingsGrid.GridId = "b7add53a-7fa4-4607-9605-0b9bd75ee457";
			this.SettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SettingsGrid.LayoutKey = "SettingsGrid";
			this.SettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.SettingsGrid.Name = "SettingsGrid";
			this.SettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 485, true);
			this.SettingsGrid.TabIndex = 1;
			// 
			// OrgSecurityProfileControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "OrgSecurityProfileControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 506, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.glowSecurityGroupBox.ResumeLayout(false);
			this.glowSecurityGroupBox.PerformLayout();
			this.BulkUpdateGroupBox.ResumeLayout(false);
			this.BulkUpdateGroupBox.PerformLayout();
			this.ProfileGroupBox.ResumeLayout(false);
			this.ProfileGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfileGrid)).EndInit();
			this.ProfileGrid.ResumeLayout(false);
			this.ProfileGrid.PerformLayout();
			this.SettingGroupBox.ResumeLayout(false);
			this.SettingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SettingsGrid)).EndInit();
			this.SettingsGrid.ResumeLayout(false);
			this.SettingsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox ProfileGroupBox;
		protected ZArchitecture.ZGrid ProfileGrid;
		private ZArchitecture.GUI.ZGroupBox SettingGroupBox;
		protected ZArchitecture.ZGrid SettingsGrid;
		private ZArchitecture.GUI.ZButton NewButton;
		private ZArchitecture.GUI.ZGroupBox BulkUpdateGroupBox;
		private ZArchitecture.GUI.ZButton BulkUpdateButton;
		private ZArchitecture.ZLabel DisclaimerLabel;
		private ZArchitecture.GUI.ZGroupBox glowSecurityGroupBox;
		private ZArchitecture.GUI.ZButton manageGlowSecurityButton;
	}
}
