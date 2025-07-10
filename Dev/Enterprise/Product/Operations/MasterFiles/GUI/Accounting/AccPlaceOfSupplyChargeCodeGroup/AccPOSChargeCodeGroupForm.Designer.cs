
namespace Enterprise.MasterFiles.GUI
{
	partial class AccPOSChargeCodeGroupForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ChargeGroupTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.placeOfSupplyConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PlaceOfSupplyConfiguration = new Enterprise.MasterFiles.GUI.AccPlaceOfSupplyConfigurationControl();
			this.zStmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.ChargeGroupTabPage.SuspendLayout();
			this.ChargeCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).BeginInit();
			this.ChargeCodesGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.placeOfSupplyConfigurationTabPage.SuspendLayout();
			this.PlaceOfSupplyConfiguration.SuspendLayout();
			this.zStmNoteTabPage.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.ChargeGroupTabPage);
			this.MainTabControl.Controls.Add(this.placeOfSupplyConfigurationTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage);
			this.MainTabControl.Controls.Add(this.zLogsTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 405, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// ChargeGroupTabPage
			// 
			this.ChargeGroupTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccPOSChargeCodeGroupForm|a23bba04-c6c9-45d7-83d7-28d47265cde8", "POS Charge Code Group");
			this.ChargeGroupTabPage.Controls.Add(this.ChargeCodesGroupBox);
			this.ChargeGroupTabPage.Controls.Add(this.TopPanel);
			this.ChargeGroupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ChargeGroupTabPage.Name = "ChargeGroupTabPage";
			this.ChargeGroupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 383, true);
			this.ChargeGroupTabPage.TabIndex = 0;
			this.ChargeGroupTabPage.UseVisualStyleBackColor = true;
			// 
			// ChargeCodesGroupBox
			// 
			this.ChargeCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("353419df-500a-425b-a9d9-3f625d9e9ec6", "Charge Codes");
			this.ChargeCodesGroupBox.Controls.Add(this.ChargeCodesGrid);
			this.ChargeCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.ChargeCodesGroupBox.Name = "ChargeCodesGroupBox";
			this.ChargeCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 318, true);
			this.ChargeCodesGroupBox.TabIndex = 1;
			this.ChargeCodesGroupBox.TabStop = false;
			// 
			// ChargeCodesGrid
			// 
			this.ChargeCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeCodesGrid, "ChargeCodePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).ChargeCodePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).ChargeCodePivots)).SyncRoot)).GRP_MemberID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).ChargeCodePivots)).SyncRoot)).ChargeCode.AC_DescMultilingual)));
			this.ChargeCodesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b1188f48-71e3-40a0-b376-dbcfc5d346a7", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GRP_MemberID";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7cfc5028-3485-4f73-a866-1d99e46d555e", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCode+AC_DescMultilingual";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeCodesGrid.GridId = "5fe6d094-365c-4767-8a13-e9aa432d0af1";
			this.ChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodesGrid.LayoutKey = "ChargeCodePivotsGrid";
			this.ChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ChargeCodesGrid.Name = "ChargeCodesGrid";
			this.ChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 301, true);
			this.ChargeCodesGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.DescriptionTextBox);
			this.TopPanel.Controls.Add(this.CodeTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 65, true);
			this.TopPanel.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "GRO_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).GRO_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 36, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 17, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "GRO_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).GRO_Code)));
			this.CodeTextBox.CaptionResourceString = null;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 10, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 17, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// placeOfSupplyConfigurationTabPage
			// 
			this.placeOfSupplyConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1b6f4f32-f31f-4284-8862-da54ec091577", "Place of Supply Configuration");
			this.placeOfSupplyConfigurationTabPage.Controls.Add(this.PlaceOfSupplyConfiguration);
			this.placeOfSupplyConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.placeOfSupplyConfigurationTabPage.Name = "placeOfSupplyConfigurationTabPage";
			this.placeOfSupplyConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 383, true);
			this.placeOfSupplyConfigurationTabPage.TabIndex = 3;
			this.placeOfSupplyConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// PlaceOfSupplyConfiguration
			// 
			this.PlaceOfSupplyConfiguration.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyConfiguration, "AccPlaceOfSupplyConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccPOSConfigurationCollection)(((Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup)(null)).AccPlaceOfSupplyConfigurations)));
			this.PlaceOfSupplyConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlaceOfSupplyConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfSupplyConfiguration.Name = "PlaceOfSupplyConfiguration";
			this.PlaceOfSupplyConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 383, true);
			this.PlaceOfSupplyConfiguration.TabIndex = 2;
			// 
			// zStmNoteTabPage
			// 
			this.zStmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage.Name = "zStmNoteTabPage";
			this.zStmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 383, true);
			this.zStmNoteTabPage.TabIndex = 1;
			// 
			// zLogsTabPage
			// 
			this.zLogsTabPage.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage.Name = "zLogsTabPage";
			this.zLogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 383, true);
			this.zLogsTabPage.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 405, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 34, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 4, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// AccPOSChargeCodeGroupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccPOSChargeCodeGroupForm|39fb83f2-73fa-4e4a-aa63-278f55e95ecb", "Place of Supply Charge Code Group");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 463, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccPOSChargeCodeGroup);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "AccPOSChargeCodeGroupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AccPOSChargeCodeGroupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ChargeGroupTabPage.ResumeLayout(false);
			this.ChargeGroupTabPage.PerformLayout();
			this.ChargeCodesGroupBox.ResumeLayout(false);
			this.ChargeCodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).EndInit();
			this.ChargeCodesGrid.ResumeLayout(false);
			this.ChargeCodesGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.placeOfSupplyConfigurationTabPage.ResumeLayout(false);
			this.placeOfSupplyConfigurationTabPage.PerformLayout();
			this.PlaceOfSupplyConfiguration.ResumeLayout(true);
			this.PlaceOfSupplyConfiguration.PerformLayout();
			this.zStmNoteTabPage.ResumeLayout(false);
			this.zStmNoteTabPage.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ChargeGroupTabPage;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ChargeCodesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage;
		private Enterprise.ZArchitecture.ZGrid ChargeCodesGrid;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.GUI.ZTabPage placeOfSupplyConfigurationTabPage;
		private AccPlaceOfSupplyConfigurationControl PlaceOfSupplyConfiguration;
	}
}
