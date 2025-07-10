
namespace Enterprise.MasterFiles.GUI
{
	partial class OrganisationTaxRateFileImportForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.RateSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ImportLinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ImportLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.SuspendLayout();
			this.RateSourceDropEdit.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.ImportLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportLinesGrid)).BeginInit();
			this.ImportLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.TaxConfigurationCodeAndDescriptionZGuidDropEdit);
			this.TopPanel.Controls.Add(this.RateSourceDropEdit);
			this.TopPanel.Controls.Add(this.FileNameTextBox);
			this.TopPanel.Controls.Add(this.ImportButton);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 86, true);
			this.TopPanel.TabIndex = 0;
			// 
			// TaxConfigurationCodeAndDescriptionZGuidDropEdit
			// 
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxConfigurationCodeAndDescriptionZGuidDropEdit, "TaxConfiguration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).TaxConfiguration)));
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 15, true);
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.Name = "TaxConfigurationCodeAndDescriptionZGuidDropEdit";
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.ShouldResizeByMaxLength = false;
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 13, true);
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.TabIndex = 0;
			// 
			// RateSourceDropEdit
			// 
			this.RateSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RateSourceDropEdit, "RateSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).RateSource)));
			this.RateSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 15, true);
			this.RateSourceDropEdit.Name = "RateSourceDropEdit";
			this.RateSourceDropEdit.ShouldResizeByMaxLength = false;
			this.RateSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.RateSourceDropEdit.TabIndex = 1;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FileNameTextBox, "FileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).FileName)));
			this.FileNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1D4D5257-C44A-4D78-9E58-09F2DFAA9A69", "File");
			this.FileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 50, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 20, true);
			this.FileNameTextBox.TabIndex = 0;
			this.FileNameTextBox.TabStop = false;
			// 
			// ImportButton
			// 
			this.ImportButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("80B7DA98-003F-4629-A497-04E05357236C", "Rate File Import");
			this.ImportButton.IsCaptionOverridden = false;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 48, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 23, true);
			this.ImportButton.TabIndex = 2;
			this.ImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 405, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 34, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 0, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.ImportLinesTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 319, true);
			this.TabControl.TabIndex = 2;
			// 
			// ImportLinesTabPage
			// 
			this.ImportLinesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("af9c2b9d-a4e8-4d24-b23f-feeb36581067", "Import Log");
			this.ImportLinesTabPage.Controls.Add(this.ImportLinesGrid);
			this.ImportLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ImportLinesTabPage.Name = "ImportLinesTabPage";
			this.ImportLinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ImportLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 292, true);
			this.ImportLinesTabPage.TabIndex = 0;
			// 
			// ImportLinesGrid
			// 
			this.ImportLinesGrid.AllowNavigation = false;
			this.ImportLinesGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.ImportLinesGrid, "ImportLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).OrganizationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).RegistrationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).RateSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).RateNumerator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImportLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport)(null)).ImportLines)).SyncRoot)).RateDenominator)));
			this.ImportLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "OrganizationName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.ColumnName = "RegistrationCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "RateSource";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo5.ColumnName = "RateNumerator";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.ColumnName = "RateDenominator";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ImportLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ImportLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ImportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ImportLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportLinesGrid.GridId = "eca8c83c-6d4a-422e-b4eb-c6f479a613b4";
			this.ImportLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportLinesGrid.LayoutKey = "ImportLinesGrid";
			this.ImportLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ImportLinesGrid.Name = "ImportLinesGrid";
			this.ImportLinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ImportLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 286, true);
			this.ImportLinesGrid.TabIndex = 0;
			// 
			// OpenFileDialog
			// 
			this.OpenFileDialog.AddExtension = true;
			this.OpenFileDialog.CheckFileExists = true;
			this.OpenFileDialog.CheckPathExists = true;
			this.OpenFileDialog.DefaultExt = "";
			this.OpenFileDialog.DereferenceLinks = true;
			this.OpenFileDialog.Filter = "";
			this.OpenFileDialog.FilterIndex = 1;
			this.OpenFileDialog.InitialDirectory = "";
			this.OpenFileDialog.Multiselect = false;
			this.OpenFileDialog.ReadOnlyChecked = false;
			this.OpenFileDialog.RestoreDirectory = false;
			this.OpenFileDialog.ShowHelp = false;
			this.OpenFileDialog.SupportMultiDottedExtensions = false;
			this.OpenFileDialog.Title = "";
			this.OpenFileDialog.ValidateNames = true;
			// 
			// OrganisationTaxRateFileImportForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrganisationTaxRateFileImportForm|69F91B6A-E7D1-4930-86ED-60F83DEB3622", "Tax Configuration Organization Rate Update File Import");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 463, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrganisationTaxRateFileImport);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 501, true);
			this.Name = "OrganisationTaxRateFileImportForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.ResumeLayout(true);
			this.TaxConfigurationCodeAndDescriptionZGuidDropEdit.PerformLayout();
			this.RateSourceDropEdit.ResumeLayout(true);
			this.RateSourceDropEdit.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.ImportLinesTabPage.ResumeLayout(false);
			this.ImportLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportLinesGrid)).EndInit();
			this.ImportLinesGrid.ResumeLayout(false);
			this.ImportLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZGuidDropEdit TaxConfigurationCodeAndDescriptionZGuidDropEdit;
		private ZArchitecture.GUI.ZDropEdit RateSourceDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		private Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		private ZArchitecture.GUI.ZTabControl TabControl;
		private ZArchitecture.GUI.ZTabPage ImportLinesTabPage;
		private ZArchitecture.ZGrid ImportLinesGrid;
	}
}
