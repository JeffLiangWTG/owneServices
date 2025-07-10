using CargoWise.Windows.UI;

namespace Enterprise.MarketingManager.GUI
{
	partial class LinkTrackCampaignClickStatUserControl
	{
#if !WINZOR
		Enterprise.ZArchitecture.GUI.OxyplotView summaryControlPlotView;
		Enterprise.ZArchitecture.GUI.OxyplotView timeControlPlotView;
#endif
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
#if !WINZOR
			if (disposing && summaryControlPlotView != null)
			{
				summaryControlPlotView.Dispose();
			}

			if (disposing && timeControlPlotView != null)
			{
				timeControlPlotView.Dispose();
			}
#endif
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReportTimeRangeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReportByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.chartContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SummaryControlHost = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TimeControlHost = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
			this.LinksGrid.SuspendLayout();
			this.ReportTimeRangeDropEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.ReportByDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chartContainer)).BeginInit();
			this.chartContainer.Panel1.SuspendLayout();
			this.chartContainer.Panel2.SuspendLayout();
			this.chartContainer.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.ClickStatModel);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.RefreshButton);
			this.splitContainer1.Panel1.Controls.Add(this.LinksGrid);
			this.splitContainer1.Panel1.Controls.Add(this.ReportTimeRangeDropEdit);
			this.splitContainer1.Panel1.Controls.Add(this.ToDateEdit);
			this.splitContainer1.Panel1.Controls.Add(this.FromDateEdit);
			this.splitContainer1.Panel1.Controls.Add(this.ReportByDropEdit);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.chartContainer);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 541, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.splitContainer1.TabIndex = 0;
			// 
			// RefreshButton
			// 
			this.RefreshButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.RefreshImage;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(786, 6, true);
			this.RefreshButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 22, true);
			this.RefreshButton.TabIndex = 5;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// LinksGrid
			// 
			this.LinksGrid.AllowNavigation = false;
			this.LinksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
						| System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LinksGrid, "LinkClicks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).ViewInChart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).URL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).TrackedUrl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Clicks)));
			this.LinksGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3130b896-ecc4-4f2f-9789-1cf71e9389fd", "Report");
			zCheckBoxColumnStyleInfo2.ColumnName = "ViewInChart";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2c974101-05cd-46cb-b2a1-1a84cbafbef9", "Context Display Name");
			zTextBoxColumnStyleInfo4.ColumnName = "Context";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e584b1e7-28dd-40e6-81cf-74a082528540", "Destination URL");
			zTextBoxColumnStyleInfo5.ColumnName = "URL";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("550bdced-6667-43bb-a861-b94b33e5058e", "Link URL");
			zTextBoxColumnStyleInfo6.ColumnName = "TrackedUrl";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7acf65c1-02f6-489c-9256-c6e5562cb374", "Clicks");
			zCalcEditColumnStyleInfo2.ColumnName = "Clicks";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.LinksGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinksGrid.GridId = "2f31e8f9-3ae3-46b8-ad41-74fc0b08e435";
			this.LinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinksGrid.LayoutKey = "zGrid1";
			this.LinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.LinksGrid.Name = "LinksGrid";
			this.LinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 110, true);
			this.LinksGrid.TabIndex = 6;
			// 
			// ReportTimeRangeDropEdit
			// 
			this.ReportTimeRangeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportTimeRangeDropEdit, "ReportTimeRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ReportTimeRange)));
			this.ReportTimeRangeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2060e5f9-0ff2-48d5-ab81-7e44e76374fc", "Date Range");
			this.ReportTimeRangeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 7, true);
			this.ReportTimeRangeDropEdit.Name = "ReportTimeRangeDropEdit";
			this.ReportTimeRangeDropEdit.PreBoundMaxLength = 25;
			this.ReportTimeRangeDropEdit.ShowDescriptionBox = false;
			this.ReportTimeRangeDropEdit.ShowHorizontalScrollBar = false;
			this.ReportTimeRangeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ReportTimeRangeDropEdit.TabIndex = 2;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "ToDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ToDateTime)));
			this.ToDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("80aeffae-dd11-4b53-90bc-dc3ef5cbe998", "To");
			this.ToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(666, 7, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 4;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "FromDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).FromDateTime)));
			this.FromDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("668b98dc-1660-4ac4-9f83-b2f808fc884a", "From");
			this.FromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 7, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 3;
			// 
			// ReportByDropEdit
			// 
			this.ReportByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportByDropEdit, "ReportBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ReportBy)));
			this.ReportByDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3dd2de0b-8379-4d8d-a254-03cbea49573f", "Report By");
			this.ReportByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 7, true);
			this.ReportByDropEdit.Name = "ReportByDropEdit";
			this.ReportByDropEdit.PreBoundMaxLength = 25;
			this.ReportByDropEdit.ShowDescriptionBox = false;
			this.ReportByDropEdit.ShowHorizontalScrollBar = false;
			this.ReportByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ReportByDropEdit.TabIndex = 1;
			// 
			// chartContainer
			// 
			this.chartContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.chartContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.chartContainer.Name = "chartContainer";
			this.chartContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// chartContainer.Panel1
			// 
			this.chartContainer.Panel1.Controls.Add(this.SummaryControlHost);
			this.chartContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.chartContainer.Panel1.Resize += ChartContainerPanel1_Resize;
			// 
			// chartContainer.Panel2
			// 
			this.chartContainer.Panel2.Controls.Add(this.TimeControlHost);
			this.chartContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.chartContainer.Panel2.Resize += ChartContainerPanel2_Resize;
			this.chartContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 387, true);
			this.chartContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(193);
			this.chartContainer.TabIndex = 6;
			// 
			// SummaryControlHost
			// 
			this.SummaryControlHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryControlHost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.SummaryControlHost.Name = "SummaryControlHost";
			this.SummaryControlHost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 190, true);
			this.SummaryControlHost.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8d76683e-01b0-4f3a-87c9-fb070791b0ee", "Link Activity");
			this.zGroupBox1.Controls.Add(this.splitContainer1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 560, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// TimeControlHost
			// 
			this.TimeControlHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TimeControlHost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.TimeControlHost.Name = "TimeControlHost";
			this.TimeControlHost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 187, true);
			this.TimeControlHost.TabIndex = 0;
			// 
			// LinkTrackCampaignClickStatUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "LinkTrackCampaignClickStatUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 560, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
			this.LinksGrid.ResumeLayout(false);
			this.LinksGrid.PerformLayout();
			this.ReportTimeRangeDropEdit.ResumeLayout(true);
			this.ReportTimeRangeDropEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ReportByDropEdit.ResumeLayout(true);
			this.ReportByDropEdit.PerformLayout();
			this.chartContainer.Panel1.ResumeLayout(false);
			this.chartContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.chartContainer)).EndInit();
			this.chartContainer.ResumeLayout(false);
			this.chartContainer.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		protected ZArchitecture.ZGrid LinksGrid;
		private ZArchitecture.GUI.ZDropEdit ReportByDropEdit;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private ZArchitecture.GUI.ZDropEdit ReportTimeRangeDropEdit;
		protected CargoWise.Windows.UI.KSplitContainer chartContainer;
		private ZArchitecture.GUI.ZButton RefreshButton;
		private Enterprise.ZArchitecture.GUI.ZPanel SummaryControlHost;
		private Enterprise.ZArchitecture.GUI.ZPanel TimeControlHost;
	}
}
