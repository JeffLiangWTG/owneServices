using System;

namespace Enterprise.Customs.US.GUI
{
	partial class USCTariffRuleForm
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
			if (tariffRule != null)
			{
                tariffRule.U1_RuleCodeInfo.ValueChanged -= new EventHandler(U1_RuleCodeInfo_ValueChanged);
                tariffRule.U1_TariffInfo.ValueChanged -= new EventHandler(U1_TariffInfo_ValueChanged);
                tariffRule.U1_TariffToInfo.ValueChanged -= new EventHandler(U1_TariffToInfo_ValueChanged);
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo23 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo24 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo21 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo22 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.AssociatedTarrifsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecondaryTariffsMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.SecondaryTariffRuleExceptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SecondaryTariffRuleExceptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecondaryTariffRuleExceptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SecondaryTariffRulesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SecondaryTariffsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AssociatedTarrifsTabPage.SuspendLayout();
			this.SecondaryTariffsMainPanel.SuspendLayout();
			this.SecondaryTariffRuleExceptionPanel.SuspendLayout();
			this.SecondaryTariffRuleExceptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffRuleExceptionGrid)).BeginInit();
			this.SecondaryTariffRulesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.AssociatedTarrifsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 445, true);
			this.MainTabControl.Controls.SetChildIndex(this.AssociatedTarrifsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 418, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USCTariffRule);
			// 
			// AssociatedTarrifsTabPage
			// 
			this.AssociatedTarrifsTabPage.Controls.Add(this.SecondaryTariffsMainPanel);
			this.AssociatedTarrifsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AssociatedTarrifsTabPage.Name = "AssociatedTarrifsTabPage";
			this.AssociatedTarrifsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AssociatedTarrifsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 418, true);
			this.AssociatedTarrifsTabPage.TabIndex = 3;
			this.AssociatedTarrifsTabPage.Text = "Associated Secondary Tariffs";
			this.AssociatedTarrifsTabPage.UseVisualStyleBackColor = true;
			// 
			// SecondaryTariffsMainPanel
			// 
			this.SecondaryTariffsMainPanel.Controls.Add(this.splitter1);
			this.SecondaryTariffsMainPanel.Controls.Add(this.SecondaryTariffRuleExceptionPanel);
			this.SecondaryTariffsMainPanel.Controls.Add(this.SecondaryTariffRulesPanel);
			this.SecondaryTariffsMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffsMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SecondaryTariffsMainPanel.Name = "SecondaryTariffsMainPanel";
			this.SecondaryTariffsMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 412, true);
			this.SecondaryTariffsMainPanel.TabIndex = 2;
			// 
			// splitter1
			// 
			this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 306, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 3, true);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// SecondaryTariffRuleExceptionPanel
			// 
			this.SecondaryTariffRuleExceptionPanel.Controls.Add(this.SecondaryTariffRuleExceptionGroupBox);
			this.SecondaryTariffRuleExceptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffRuleExceptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 306, true);
			this.SecondaryTariffRuleExceptionPanel.Name = "SecondaryTariffRuleExceptionPanel";
			this.SecondaryTariffRuleExceptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 106, true);
			this.SecondaryTariffRuleExceptionPanel.TabIndex = 1;
			// 
			// SecondaryTariffRuleExceptionGroupBox
			// 
			this.SecondaryTariffRuleExceptionGroupBox.Controls.Add(this.SecondaryTariffRuleExceptionGrid);
			this.SecondaryTariffRuleExceptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffRuleExceptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecondaryTariffRuleExceptionGroupBox.Name = "SecondaryTariffRuleExceptionGroupBox";
			this.SecondaryTariffRuleExceptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 106, true);
			this.SecondaryTariffRuleExceptionGroupBox.TabIndex = 0;
			this.SecondaryTariffRuleExceptionGroupBox.TabStop = false;
			this.SecondaryTariffRuleExceptionGroupBox.Text = "Exceptions";
			// 
			// SecondaryTariffRuleExceptionGrid
			// 
			this.SecondaryTariffRuleExceptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SecondaryTariffRuleExceptionGrid, "SecondaryTariffs.Exceptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).Exceptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariffException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).Exceptions)).SyncRoot)).FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariffException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).Exceptions)).SyncRoot)).U4_DateFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariffException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).Exceptions)).SyncRoot)).U4_DateTo)));
			this.SecondaryTariffRuleExceptionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Tariff";
			zTextBoxColumnStyleInfo6.ColumnName = "FormattedTariff";
			zDateEditColumnStyleInfo23.Caption = "Date From";
			zDateEditColumnStyleInfo23.ColumnName = "U4_DateFrom";
			zDateEditColumnStyleInfo23.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo24.Caption = "Date To";
			zDateEditColumnStyleInfo24.ColumnName = "U4_DateTo";
			zDateEditColumnStyleInfo24.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.SecondaryTariffRuleExceptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SecondaryTariffRuleExceptionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo23);
			this.SecondaryTariffRuleExceptionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo24);
			this.SecondaryTariffRuleExceptionGrid.GridId = "0b9b9da5-45bc-4617-b67c-9ec66727dcdf";
			this.SecondaryTariffRuleExceptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffRuleExceptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecondaryTariffRuleExceptionGrid.LayoutKey = "zGrid1";
			this.SecondaryTariffRuleExceptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SecondaryTariffRuleExceptionGrid.Name = "SecondaryTariffRuleExceptionGrid";
			this.SecondaryTariffRuleExceptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 87, true);
			this.SecondaryTariffRuleExceptionGrid.TabIndex = 2;
			// 
			// SecondaryTariffRulesPanel
			// 
			this.SecondaryTariffRulesPanel.Controls.Add(this.SecondaryTariffsGrid);
			this.SecondaryTariffRulesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SecondaryTariffRulesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecondaryTariffRulesPanel.Name = "SecondaryTariffRulesPanel";
			this.SecondaryTariffRulesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 306, true);
			this.SecondaryTariffRulesPanel.TabIndex = 0;
			// 
			// SecondaryTariffsGrid
			// 
			this.SecondaryTariffsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SecondaryTariffsGrid, "SecondaryTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).FormattedTariffFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).FormattedTariffTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).FormattedTariff2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).FormattedTariff3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).U3_DateFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCRuleSecondaryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).SecondaryTariffs)).SyncRoot)).U3_DateTo)));
			this.SecondaryTariffsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo21.Caption = "Tariff (From)";
			zCodeFindBoxColumnStyleInfo21.ColumnName = "FormattedTariffFrom";
			zCodeFindBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo22.Caption = "Tariff To";
			zCodeFindBoxColumnStyleInfo22.ColumnName = "FormattedTariffTo";
			zCodeFindBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo23.Caption = "Tariff 2";
			zCodeFindBoxColumnStyleInfo23.ColumnName = "FormattedTariff2";
			zCodeFindBoxColumnStyleInfo24.Caption = "Tariff 3";
			zCodeFindBoxColumnStyleInfo24.ColumnName = "FormattedTariff3";
			zDateEditColumnStyleInfo21.Caption = "Date From";
			zDateEditColumnStyleInfo21.ColumnName = "U3_DateFrom";
			zDateEditColumnStyleInfo21.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo22.Caption = "Date To";
			zDateEditColumnStyleInfo22.ColumnName = "U3_DateTo";
			zDateEditColumnStyleInfo22.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo21);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo22);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo23);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo24);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo21);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo22);
			this.SecondaryTariffsGrid.GridId = "58791208-c0ed-4090-9707-4d8879ef3095";
			this.SecondaryTariffsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecondaryTariffsGrid.LayoutKey = "zGrid1";
			this.SecondaryTariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecondaryTariffsGrid.Name = "SecondaryTariffsGrid";
			this.SecondaryTariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 306, true);
			this.SecondaryTariffsGrid.TabIndex = 1;
			// 
			// USCTariffRuleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 501, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USCTariffRule);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 528, true);
			this.Name = "USCTariffRuleForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "USCTariffRuleForm";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AssociatedTarrifsTabPage.ResumeLayout(false);
			this.SecondaryTariffsMainPanel.ResumeLayout(false);
			this.SecondaryTariffRuleExceptionPanel.ResumeLayout(false);
			this.SecondaryTariffRuleExceptionGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffRuleExceptionGrid)).EndInit();
			this.SecondaryTariffRulesPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage AssociatedTarrifsTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel SecondaryTariffsMainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel SecondaryTariffRuleExceptionPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SecondaryTariffRuleExceptionGroupBox;
		private Enterprise.ZArchitecture.ZGrid SecondaryTariffRuleExceptionGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel SecondaryTariffRulesPanel;
		private Enterprise.ZArchitecture.ZGrid SecondaryTariffsGrid;
		private CargoWise.Windows.UI.KSplitter splitter1;
	}
}
