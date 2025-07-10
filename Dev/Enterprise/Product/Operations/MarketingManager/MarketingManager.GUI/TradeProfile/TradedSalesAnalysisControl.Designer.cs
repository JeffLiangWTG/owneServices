namespace Enterprise.MarketingManager.GUI
{
	partial class TradedSalesAnalysisControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.anaylsisPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.mainGroupingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.treeControl = new Enterprise.MarketingManager.GUI.TradedSalesAnalysisTreeControl();
			this.mainGroupingTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.locationGroupingTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.includeJobValueCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.revenueNoteLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.anaylsisPeriodDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainGroupingGrid)).BeginInit();
			this.mainGroupingGrid.SuspendLayout();
			this.treeControl.SuspendLayout();
			this.mainGroupingTypeDropEdit.SuspendLayout();
			this.locationGroupingTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.TradedSalesAnalysis);
			// 
			// anaylsisPeriodDropEdit
			// 
			this.anaylsisPeriodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.anaylsisPeriodDropEdit, "PeriodDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).PeriodDescription)));
			this.anaylsisPeriodDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.anaylsisPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 2, true);
			this.anaylsisPeriodDropEdit.Name = "anaylsisPeriodDropEdit";
			this.anaylsisPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.anaylsisPeriodDropEdit.TabIndex = 2;
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 58, true);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.mainGroupingGrid);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.treeControl);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 219, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.splitContainer.TabIndex = 3;
			// 
			// mainGroupingGrid
			// 
			this.mainGroupingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.mainGroupingGrid, "MainGroupings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Service)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Warehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).WarehouseCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).OriginCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).OriginState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).OriginUnloco)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).DestinationCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).DestinationState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).DestinationUnloco)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).SupplierPartNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).SupplierPartDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).GrossRevenue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).JobRevenue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).JobCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).JobProfit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)).CurrencyCode)));
			this.mainGroupingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Service";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "Warehouse";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo3.ColumnName = "WarehouseCountry";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.ColumnName = "Origin";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo5.ColumnName = "OriginCountry";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.ColumnName = "OriginState";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "OriginUnloco";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.ColumnName = "Destination";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo9.ColumnName = "DestinationCountry";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.ColumnName = "DestinationState";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.ColumnName = "DestinationUnloco";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo12.ColumnName = "Mode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo13.ColumnName = "Type";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo14.ColumnName = "SupplierPartNum";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo15.ColumnName = "SupplierPartDescription";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "GrossRevenue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JobRevenue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JobCost";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JobProfit";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo16.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.mainGroupingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.mainGroupingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.mainGroupingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.mainGroupingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.mainGroupingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.mainGroupingGrid.CopySelectedRowsAllowed = true;
			this.mainGroupingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainGroupingGrid.GridId = "c4720700-bf12-4e75-b1b1-a2bbeb93ee70";
			this.mainGroupingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.mainGroupingGrid.LayoutKey = "mainGroupingGrid";
			this.mainGroupingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainGroupingGrid.Name = "mainGroupingGrid";
			this.mainGroupingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 219, true);
			this.mainGroupingGrid.TabIndex = 0;
			// 
			// treeControl
			// 
			this.treeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.treeControl, "MainGroupings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((Enterprise.MarketingManager.Business.TradePeriodGrouping)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupings)).SyncRoot)))));
			this.treeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.treeControl.Name = "treeControl";
			this.treeControl.ProductCode = null;
			this.treeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 219, true);
			this.treeControl.TabIndex = 0;
			// 
			// mainGroupingTypeDropEdit
			// 
			this.mainGroupingTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mainGroupingTypeDropEdit, "MainGroupingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).MainGroupingType)));
			this.mainGroupingTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 21, true);
			this.mainGroupingTypeDropEdit.Name = "mainGroupingTypeDropEdit";
			this.mainGroupingTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.mainGroupingTypeDropEdit.TabIndex = 4;
			// 
			// locationGroupingTypeDropEdit
			// 
			this.locationGroupingTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.locationGroupingTypeDropEdit, "LocationGroupingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).LocationGroupingType)));
			this.locationGroupingTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 39, true);
			this.locationGroupingTypeDropEdit.Name = "locationGroupingTypeDropEdit";
			this.locationGroupingTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.locationGroupingTypeDropEdit.TabIndex = 5;
			// 
			// includeJobValueCheckBox
			// 
			this.includeJobValueCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.includeJobValueCheckBox, "IncludeJobValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.TradedSalesAnalysis)(null)).IncludeJobValue)));
			this.includeJobValueCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.includeJobValueCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 3, true);
			this.includeJobValueCheckBox.Name = "includeJobRevenueCheckBox";
			this.includeJobValueCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 16, true);
			this.includeJobValueCheckBox.TabIndex = 6;
			this.includeJobValueCheckBox.UseVisualStyleBackColor = true;
			// 
			// revenueNoteLabel
			// 
			this.revenueNoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.revenueNoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 21, true);
			this.revenueNoteLabel.Name = "revenueNoteLabel";
			this.revenueNoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 35, true);
			this.revenueNoteLabel.TabIndex = 7;
			this.revenueNoteLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// TradedSalesAnalysisControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.revenueNoteLabel);
			this.Controls.Add(this.includeJobValueCheckBox);
			this.Controls.Add(this.locationGroupingTypeDropEdit);
			this.Controls.Add(this.mainGroupingTypeDropEdit);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.anaylsisPeriodDropEdit);
			this.Name = "TradedSalesAnalysisControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 278, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.anaylsisPeriodDropEdit.ResumeLayout(true);
			this.anaylsisPeriodDropEdit.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainGroupingGrid)).EndInit();
			this.mainGroupingGrid.ResumeLayout(false);
			this.mainGroupingGrid.PerformLayout();
			this.treeControl.ResumeLayout(true);
			this.treeControl.PerformLayout();
			this.mainGroupingTypeDropEdit.ResumeLayout(true);
			this.mainGroupingTypeDropEdit.PerformLayout();
			this.locationGroupingTypeDropEdit.ResumeLayout(true);
			this.locationGroupingTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit anaylsisPeriodDropEdit;
		private CargoWise.Windows.UI.KSplitContainer splitContainer;
		private ZArchitecture.ZGrid mainGroupingGrid;
		private ZArchitecture.GUI.ZDropEdit mainGroupingTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit locationGroupingTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox includeJobValueCheckBox;
		private TradedSalesAnalysisTreeControl treeControl;
		private ZArchitecture.ZLabel revenueNoteLabel;
	}
}
