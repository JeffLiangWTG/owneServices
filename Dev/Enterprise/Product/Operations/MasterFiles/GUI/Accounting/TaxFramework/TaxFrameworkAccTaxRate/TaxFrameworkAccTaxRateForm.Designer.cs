namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class TaxFrameworkAccTaxRateForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.saveButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.saveButtonsControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.taxFrameworkAccTaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.saveButtonsPanel.SuspendLayout();
			this.saveButtonsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxFrameworkAccTaxRateGrid)).BeginInit();
			this.taxFrameworkAccTaxRateGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader);
			// 
			// saveButtonsPanel
			// 
			this.saveButtonsPanel.Controls.Add(this.saveButtonsControl);
			this.saveButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.saveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 255, true);
			this.saveButtonsPanel.Name = "saveButtonsPanel";
			this.saveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 35, true);
			this.saveButtonsPanel.TabIndex = 1;
			// 
			// saveButtonsControl
			// 
			this.saveButtonsControl.AllowDrop = true;
			this.saveButtonsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButtonsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 8, true);
			this.saveButtonsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.saveButtonsControl.Name = "saveButtonsControl";
			this.saveButtonsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.saveButtonsControl.TabIndex = 0;
			// 
			// taxFrameworkAccTaxRateGrid
			// 
			this.taxFrameworkAccTaxRateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.taxFrameworkAccTaxRateGrid, "TaxFrameworkAccTaxRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)).SyncRoot)).AT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)).SyncRoot)).AT_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)).SyncRoot)).AT_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)).SyncRoot)).AT_TaxSystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader)(null)).TaxFrameworkAccTaxRates)).SyncRoot)).AT_RateSource)));
			this.taxFrameworkAccTaxRateGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AT_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "AT_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AT_RN_NKCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.ColumnName = "AT_TaxSystemCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "AT_RateSource";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.taxFrameworkAccTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.taxFrameworkAccTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.taxFrameworkAccTaxRateGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.taxFrameworkAccTaxRateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.taxFrameworkAccTaxRateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.taxFrameworkAccTaxRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxFrameworkAccTaxRateGrid.GridId = "58972475-93e4-40c4-8e3d-00e00ad41062";
			this.taxFrameworkAccTaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.taxFrameworkAccTaxRateGrid.LayoutKey = "TaxFrameworkAccTaxRateGrid";
			this.taxFrameworkAccTaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxFrameworkAccTaxRateGrid.Name = "taxFrameworkAccTaxRateGrid";
			this.taxFrameworkAccTaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 255, true);
			this.taxFrameworkAccTaxRateGrid.TabIndex = 0;
			// 
			// TaxFrameworkAccTaxRateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxFrameworkAccTaxRateForm|16A094F7-320A-4EAF-A425-3ADC00DE332B", "Update Tax Framework Tax Rate");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 314, true);
			this.Controls.Add(this.taxFrameworkAccTaxRateGrid);
			this.Controls.Add(this.saveButtonsPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxFrameworkAccTaxRateLoader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 284, true);
			this.Name = "TaxFrameworkAccTaxRateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.saveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.taxFrameworkAccTaxRateGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.saveButtonsPanel.ResumeLayout(false);
			this.saveButtonsPanel.PerformLayout();
			this.saveButtonsControl.ResumeLayout(true);
			this.saveButtonsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxFrameworkAccTaxRateGrid)).EndInit();
			this.taxFrameworkAccTaxRateGrid.ResumeLayout(false);
			this.taxFrameworkAccTaxRateGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Core.Forms.ZPostingButtonsUserControl saveButtonsControl;
		ZArchitecture.ZGrid taxFrameworkAccTaxRateGrid;
		ZArchitecture.GUI.ZPanel saveButtonsPanel;
	}
}
