namespace Enterprise.MasterFiles.GUI
{
	partial class SalesTeamCommissionControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();

			this.commissionRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.commissionRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.rightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.commissionRulePreviewPane = new Enterprise.MasterFiles.GUI.AccCommissionRulePreviewPane();
			this.commissionRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.commissionRuleRatesControl = new Enterprise.MasterFiles.GUI.AccCommissionRuleRatesControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.commissionRulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.commissionRulesGrid)).BeginInit();
			this.commissionRulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).BeginInit();
			this.rightSplitContainer.Panel1.SuspendLayout();
			this.rightSplitContainer.Panel2.SuspendLayout();
			this.rightSplitContainer.SuspendLayout();
			this.commissionRulePreviewPane.SuspendLayout();
			this.commissionRatesGroupBox.SuspendLayout();
			this.commissionRuleRatesControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesTeam);
			// 
			// commissionRulesGroupBox
			// 
			this.commissionRulesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9a7544ec-172f-4f9b-986a-c6348102c91e", "Entitlement Rules");
			this.commissionRulesGroupBox.Controls.Add(this.mainSplitContainer);
			this.commissionRulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRulesGroupBox.Name = "commissionRulesGroupBox";
			this.commissionRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.commissionRulesGroupBox.TabIndex = 0;
			this.commissionRulesGroupBox.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.mainSplitContainer.IsSplitterFixed = true;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.commissionRulesGrid);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.AutoScroll = true;
			this.mainSplitContainer.Panel2.Controls.Add(this.rightSplitContainer);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 481, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// commissionRulesGrid
			// 
			this.commissionRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.commissionRulesGrid, "CommissionRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_Service)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_SubModule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_CommissionBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_CommissionTriggerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).ACM_EndDate)));
			this.commissionRulesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ACM_Product";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.ColumnName = "ACM_Service";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.ColumnName = "ACM_SubModule";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("732E5002-8A56-4EDD-8B48-76CEE5595CD4", "Mode");
			zDropEditColumnStyleInfo6.ColumnName = "ACM_Mode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.ColumnName = "ACM_CommissionBasis";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.ColumnName = "ACM_CommissionTriggerType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "ACM_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo2.ColumnName = "ACM_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("933C6455-FCA8-4F91-80B2-F30C5CB97F03", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ACM_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2127BFD6-0A26-478C-9107-39BDFE39973D", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ACM_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.commissionRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.commissionRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.commissionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.commissionRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.commissionRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.commissionRulesGrid.CopySelectedRowsAllowed = true;
			this.commissionRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRulesGrid.GridId = "a3919b73-b8c9-4eaf-86c3-fa8a4211683a";
			this.commissionRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.commissionRulesGrid.LayoutKey = "commissionRulesGrid";
			this.commissionRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRulesGrid.Name = "commissionRulesGrid";
			this.commissionRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 481, true);
			this.commissionRulesGrid.TabIndex = 0;
			// 
			// rightSplitContainer
			// 
			this.rightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.rightSplitContainer.IsSplitterFixed = true;
			this.rightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rightSplitContainer.Name = "rightSplitContainer";
			this.rightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// rightSplitContainer.Panel1
			// 
			this.rightSplitContainer.Panel1.Controls.Add(this.commissionRulePreviewPane);
			// 
			// rightSplitContainer.Panel2
			// 
			this.rightSplitContainer.Panel2.Controls.Add(this.commissionRatesGroupBox);
			this.rightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 481, true);
			this.rightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(195);
			this.rightSplitContainer.TabIndex = 0;
			// 
			// commissionRulePreviewPane
			// 
			this.commissionRulePreviewPane.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.commissionRulePreviewPane, "CommissionRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCommissionRule)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)))));
			this.commissionRulePreviewPane.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRulePreviewPane.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRulePreviewPane.Name = "commissionRulePreviewPane";
			this.commissionRulePreviewPane.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 195, true);
			this.commissionRulePreviewPane.TabIndex = 0;
			// 
			// commissionRatesGroupBox
			// 
			this.commissionRatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f9eafdf-49bd-4af4-8416-6b0314a040f9", "Commission Rates");
			this.commissionRatesGroupBox.Controls.Add(this.commissionRuleRatesControl);
			this.commissionRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRatesGroupBox.Name = "commissionRatesGroupBox";
			this.commissionRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 282, true);
			this.commissionRatesGroupBox.TabIndex = 1;
			this.commissionRatesGroupBox.TabStop = false;
			// 
			// commissionRuleRatesControl
			// 
			this.commissionRuleRatesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.commissionRuleRatesControl, "CommissionRules.Rates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCommissionRuleRateCollection)(((Enterprise.MasterFiles.Business.AccGroupCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CommissionRules)).SyncRoot)).Rates)));
			this.commissionRuleRatesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRuleRatesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.commissionRuleRatesControl.Name = "commissionRuleRatesControl";
			this.commissionRuleRatesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.commissionRuleRatesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 263, true);
			this.commissionRuleRatesControl.TabIndex = 0;
			// 
			// SalesTeamCommissionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.commissionRulesGroupBox);
			this.Name = "SalesTeamCommissionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.commissionRulesGroupBox.ResumeLayout(false);
			this.commissionRulesGroupBox.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.commissionRulesGrid)).EndInit();
			this.commissionRulesGrid.ResumeLayout(false);
			this.commissionRulesGrid.PerformLayout();
			this.rightSplitContainer.Panel1.ResumeLayout(false);
			this.rightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).EndInit();
			this.rightSplitContainer.ResumeLayout(false);
			this.rightSplitContainer.PerformLayout();
			this.commissionRulePreviewPane.ResumeLayout(true);
			this.commissionRulePreviewPane.PerformLayout();
			this.commissionRatesGroupBox.ResumeLayout(false);
			this.commissionRatesGroupBox.PerformLayout();
			this.commissionRuleRatesControl.ResumeLayout(true);
			this.commissionRuleRatesControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox commissionRulesGroupBox;
		private ZArchitecture.ZGrid commissionRulesGrid;
		private CargoWise.Windows.UI.KSplitContainer rightSplitContainer;
		private Enterprise.MasterFiles.GUI.AccCommissionRulePreviewPane commissionRulePreviewPane;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private AccCommissionRuleRatesControl commissionRuleRatesControl;
		private ZArchitecture.GUI.ZGroupBox commissionRatesGroupBox;

	}
}
