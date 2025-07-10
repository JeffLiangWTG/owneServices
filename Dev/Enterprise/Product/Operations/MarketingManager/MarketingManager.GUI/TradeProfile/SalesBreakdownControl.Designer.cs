namespace Enterprise.MarketingManager.GUI
{
	partial class SalesBreakdownControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TopPanelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.topSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FilterOptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RevenueReportOptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShowFinancialYearToDateRevenueRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ShowPerAnnumRevenueRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.CompanyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.salesHeaderControl = new Enterprise.MarketingManager.GUI.SalesHeaderControl();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.viewProspectValuesButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ChartViewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ChartOptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ChartShowSelectedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ChartShowAllRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.tradeDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TradedSalesAnalysisTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.dynamicTradedSalesAnalysisControl = new Enterprise.MarketingManager.GUI.DynamicTradedSalesAnalysisControl();
			this.ProspectiveAnalysisTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.estimateSalesAnalysisControl = new Enterprise.MarketingManager.GUI.EstimateSalesAnalysisControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.TopPanelGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).BeginInit();
			this.topSplitContainer.Panel1.SuspendLayout();
			this.topSplitContainer.Panel2.SuspendLayout();
			this.topSplitContainer.SuspendLayout();
			this.FilterOptionsPanel.SuspendLayout();
			this.CompanyFindBox.SuspendLayout();
			this.salesHeaderControl.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.ChartOptionsPanel.SuspendLayout();
			this.tradeDetailsTabControl.SuspendLayout();
			this.TradedSalesAnalysisTabPage.SuspendLayout();
			this.ProspectiveAnalysisTabPage.SuspendLayout();
			this.estimateSalesAnalysisControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesBreakdown);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.TopPanelGroupBox);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.tradeDetailsTabControl);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 591, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(221);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// TopPanelGroupBox
			// 
			this.TopPanelGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("38d6136d-9274-4b3b-aedb-8d458d73642e", "Summary");
			this.TopPanelGroupBox.Controls.Add(this.topSplitContainer);
			this.TopPanelGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanelGroupBox.Name = "TopPanelGroupBox";
			this.TopPanelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 221, true);
			this.TopPanelGroupBox.TabIndex = 3;
			this.TopPanelGroupBox.TabStop = false;
			// 
			// topSplitContainer
			// 
			this.topSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.topSplitContainer.Name = "topSplitContainer";
			// 
			// topSplitContainer.Panel1
			// 
			this.topSplitContainer.Panel1.Controls.Add(this.FilterOptionsPanel);
			this.topSplitContainer.Panel1.Controls.Add(this.salesHeaderControl);
			this.topSplitContainer.Panel1.Controls.Add(this.toolStrip);
			// 
			// topSplitContainer.Panel2
			// 
			this.topSplitContainer.Panel2.Controls.Add(this.ChartViewPanel);
			this.topSplitContainer.Panel2.Controls.Add(this.ChartOptionsPanel);
			this.topSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 204, true);
			this.topSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(617);
			this.topSplitContainer.TabIndex = 2;
			// 
			// FilterOptionsPanel
			// 
			this.FilterOptionsPanel.Controls.Add(this.CompanyFindBox);
			this.FilterOptionsPanel.Controls.Add(this.RevenueReportOptionLabel);
			this.FilterOptionsPanel.Controls.Add(this.ShowFinancialYearToDateRevenueRadioButton);
			this.FilterOptionsPanel.Controls.Add(this.ShowPerAnnumRevenueRadioButton);
			this.FilterOptionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterOptionsPanel.Name = "FilterOptionsPanel";
			this.FilterOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 20, true);
			this.FilterOptionsPanel.TabIndex = 0;
			// 
			// RevenueReportOptionLabel
			// 
			this.RevenueReportOptionLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("85f7ea62-5a13-4c3a-a744-34d48b6e8a57", "Report Summary by");
			this.RevenueReportOptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RevenueReportOptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.RevenueReportOptionLabel.Name = "RevenueReportOptionLabel";
			this.RevenueReportOptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 18, true);
			this.RevenueReportOptionLabel.TabIndex = 0;
			// 
			// ShowFinancialYearToDateRevenueRadioButton
			// 
			this.ShowFinancialYearToDateRevenueRadioButton.AutoCheck = false;
			this.ShowFinancialYearToDateRevenueRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowFinancialYearToDateRevenueRadioButton, "ShowFinancialYearRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).ShowFinancialYearRevenue)));
			this.ShowFinancialYearToDateRevenueRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowFinancialYearToDateRevenueRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 0, true);
			this.ShowFinancialYearToDateRevenueRadioButton.Name = "ShowFinancialYearToDateRevenueRadioButton";
			this.ShowFinancialYearToDateRevenueRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 16, true);
			this.ShowFinancialYearToDateRevenueRadioButton.TabIndex = 0;
			this.ShowFinancialYearToDateRevenueRadioButton.TabStop = true;
			this.ShowFinancialYearToDateRevenueRadioButton.UseVisualStyleBackColor = true;
			// 
			// ShowPerAnnumRevenueRadioButton
			// 
			this.ShowPerAnnumRevenueRadioButton.AutoCheck = false;
			this.ShowPerAnnumRevenueRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowPerAnnumRevenueRadioButton, "ShowPerAnnumRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).ShowPerAnnumRevenue)));
			this.ShowPerAnnumRevenueRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowPerAnnumRevenueRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 0, true);
			this.ShowPerAnnumRevenueRadioButton.Name = "ShowPerAnnumRevenueRadioButton";
			this.ShowPerAnnumRevenueRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.ShowPerAnnumRevenueRadioButton.TabIndex = 1;
			this.ShowPerAnnumRevenueRadioButton.TabStop = true;
			this.ShowPerAnnumRevenueRadioButton.UseVisualStyleBackColor = true;
			// 
			// CompanyFindBox
			// 
			this.CompanyFindBox.AllowDrop = true;
			this.CompanyFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "CompanyFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).CompanyFilter)));
			this.CompanyFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0e29af04-3733-4c51-b353-c0a74025a3b1", "Company Analysis");
			this.CompanyFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 0, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.PreBoundMaxLength = 3;
			this.CompanyFindBox.ShouldResize = true;
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CompanyFindBox.TabIndex = 2;
			// 
			// salesHeaderControl
			// 
			this.salesHeaderControl.AllowDrop = true;
			this.salesHeaderControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.salesHeaderControl, "SalesHeaderCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesHeaderCollection)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).SalesHeaderCollection)));
			this.salesHeaderControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.salesHeaderControl.Name = "salesHeaderControl";
			this.salesHeaderControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 150, true);
			this.salesHeaderControl.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewProspectValuesButton});
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 21, true);
			this.toolStrip.TabIndex = 3;
			this.toolStrip.Text = "zToolStrip1";
			// 
			// viewProspectValuesButton
			// 
			this.viewProspectValuesButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b3512aac-f842-42e1-9297-6e69b43c72ea", "View Estimate Values");
			this.viewProspectValuesButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.viewProspectValuesButton.Name = "viewProspectValuesButton";
			this.viewProspectValuesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29);
			this.viewProspectValuesButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.viewProspectValuesButton.Click += new System.EventHandler(this.ViewProspectValuesButton_Click);
			// 
			// ChartViewPanel
			// 
			this.ChartViewPanel.BackColor = System.Drawing.Color.White;
			this.ChartViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChartViewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChartViewPanel.Name = "ChartViewPanel";
			this.ChartViewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 183, true);
			this.ChartViewPanel.TabIndex = 1;
			// 
			// ChartOptionsPanel
			// 
			this.ChartOptionsPanel.BackColor = System.Drawing.Color.White;
			this.ChartOptionsPanel.Controls.Add(this.ChartShowSelectedRadioButton);
			this.ChartOptionsPanel.Controls.Add(this.ChartShowAllRadioButton);
			this.ChartOptionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ChartOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.ChartOptionsPanel.Name = "ChartOptionsPanel";
			this.ChartOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 21, true);
			this.ChartOptionsPanel.TabIndex = 0;
			// 
			// ChartShowSelectedRadioButton
			// 
			this.ChartShowSelectedRadioButton.AutoCheck = false;
			this.ChartShowSelectedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChartShowSelectedRadioButton, "ShowSelectedProductRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).ShowSelectedProductRevenue)));
			this.ChartShowSelectedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChartShowSelectedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 2, true);
			this.ChartShowSelectedRadioButton.Name = "ChartShowSelectedRadioButton";
			this.ChartShowSelectedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.ChartShowSelectedRadioButton.TabIndex = 1;
			this.ChartShowSelectedRadioButton.TabStop = true;
			this.ChartShowSelectedRadioButton.UseVisualStyleBackColor = true;
			// 
			// ChartShowAllRadioButton
			// 
			this.ChartShowAllRadioButton.AutoCheck = false;
			this.ChartShowAllRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChartShowAllRadioButton, "ShowAllRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).ShowAllRevenue)));
			this.ChartShowAllRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChartShowAllRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ChartShowAllRadioButton.Name = "ChartShowAllRadioButton";
			this.ChartShowAllRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.ChartShowAllRadioButton.TabIndex = 0;
			this.ChartShowAllRadioButton.TabStop = true;
			this.ChartShowAllRadioButton.UseVisualStyleBackColor = true;
			// 
			// tradeDetailsTabControl
			// 
			this.tradeDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tradeDetailsTabControl.Controls.Add(this.TradedSalesAnalysisTabPage);
			this.tradeDetailsTabControl.Controls.Add(this.ProspectiveAnalysisTabPage);
			this.tradeDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tradeDetailsTabControl.Name = "tradeDetailsTabControl";
			this.tradeDetailsTabControl.SelectedIndex = 0;
			this.tradeDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 367, true);
			this.tradeDetailsTabControl.TabIndex = 0;
			// 
			// TradedSalesAnalysisTabPage
			// 
			this.TradedSalesAnalysisTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3955652d-7f93-4ae4-9462-bae5abc9a164", "Trading Analysis");
			this.TradedSalesAnalysisTabPage.Controls.Add(this.dynamicTradedSalesAnalysisControl);
			this.TradedSalesAnalysisTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.TradedSalesAnalysisTabPage.Name = "TradedSalesAnalysisTabPage";
			this.TradedSalesAnalysisTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TradedSalesAnalysisTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 345, true);
			this.TradedSalesAnalysisTabPage.TabIndex = 0;
			this.TradedSalesAnalysisTabPage.UseVisualStyleBackColor = true;
			// 
			// dynamicTradedSalesAnalysisControl
			// 
			this.dynamicTradedSalesAnalysisControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dynamicTradedSalesAnalysisControl, "SalesHeaderCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesHeader)(((Enterprise.MarketingManager.Business.SalesHeader)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).SalesHeaderCollection)).SyncRoot)))));
			this.dynamicTradedSalesAnalysisControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicTradedSalesAnalysisControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.dynamicTradedSalesAnalysisControl.Name = "dynamicTradedSalesAnalysisControl";
			this.dynamicTradedSalesAnalysisControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 39, true);
			this.dynamicTradedSalesAnalysisControl.TabIndex = 0;
			// 
			// ProspectiveAnalysisTabPage
			// 
			this.ProspectiveAnalysisTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4f038141-d567-46a6-ac1a-fdfca359f36c", "Estimate Analysis");
			this.ProspectiveAnalysisTabPage.Controls.Add(this.estimateSalesAnalysisControl);
			this.ProspectiveAnalysisTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ProspectiveAnalysisTabPage.Name = "ProspectiveAnalysisTabPage";
			this.ProspectiveAnalysisTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProspectiveAnalysisTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 345, true);
			this.ProspectiveAnalysisTabPage.TabIndex = 1;
			this.ProspectiveAnalysisTabPage.UseVisualStyleBackColor = true;
			// 
			// estimateSalesAnalysisControl
			// 
			this.estimateSalesAnalysisControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.estimateSalesAnalysisControl, "SalesHeaderCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesHeader)(((Enterprise.MarketingManager.Business.SalesHeader)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesBreakdown)(null)).SalesHeaderCollection)).SyncRoot)))));
			this.estimateSalesAnalysisControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.estimateSalesAnalysisControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.estimateSalesAnalysisControl.Name = "estimateSalesAnalysisControl";
			this.estimateSalesAnalysisControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 340, true);
			this.estimateSalesAnalysisControl.TabIndex = 0;
			// 
			// SalesBreakdownControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "SalesBreakdownControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 591, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.TopPanelGroupBox.ResumeLayout(false);
			this.TopPanelGroupBox.PerformLayout();
			this.topSplitContainer.Panel1.ResumeLayout(false);
			this.topSplitContainer.Panel1.PerformLayout();
			this.topSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).EndInit();
			this.topSplitContainer.ResumeLayout(false);
			this.topSplitContainer.PerformLayout();
			this.FilterOptionsPanel.ResumeLayout(false);
			this.FilterOptionsPanel.PerformLayout();
			this.CompanyFindBox.ResumeLayout(true);
			this.CompanyFindBox.PerformLayout();
			this.salesHeaderControl.ResumeLayout(true);
			this.salesHeaderControl.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ChartOptionsPanel.ResumeLayout(false);
			this.ChartOptionsPanel.PerformLayout();
			this.tradeDetailsTabControl.ResumeLayout(false);
			this.tradeDetailsTabControl.PerformLayout();
			this.TradedSalesAnalysisTabPage.ResumeLayout(false);
			this.TradedSalesAnalysisTabPage.PerformLayout();
			this.ProspectiveAnalysisTabPage.ResumeLayout(false);
			this.ProspectiveAnalysisTabPage.PerformLayout();
			this.estimateSalesAnalysisControl.ResumeLayout(true);
			this.estimateSalesAnalysisControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		protected Enterprise.MarketingManager.GUI.SalesHeaderControl salesHeaderControl;
		private CargoWise.Windows.UI.KSplitContainer topSplitContainer;
		private ZArchitecture.GUI.ZTabControl tradeDetailsTabControl;
		protected ZArchitecture.GUI.ZTabPage TradedSalesAnalysisTabPage;
		protected ZArchitecture.GUI.ZTabPage ProspectiveAnalysisTabPage;
		protected DynamicTradedSalesAnalysisControl dynamicTradedSalesAnalysisControl;
		private ZArchitecture.GUI.ZPanel ChartViewPanel;
		private ZArchitecture.GUI.ZPanel ChartOptionsPanel;
		private ZArchitecture.GUI.ZPanel FilterOptionsPanel;
		private ZArchitecture.GUI.ZRadioButton ChartShowSelectedRadioButton;
		private ZArchitecture.GUI.ZRadioButton ChartShowAllRadioButton;
		private ZArchitecture.GUI.ZGroupBox TopPanelGroupBox;
		private ZArchitecture.GUI.ZToolStrip toolStrip;
		protected ZArchitecture.GUI.ZToolStripButton viewProspectValuesButton;
		protected ZArchitecture.GUI.ZRadioButton ShowPerAnnumRevenueRadioButton;
		protected ZArchitecture.GUI.ZRadioButton ShowFinancialYearToDateRevenueRadioButton;
		private EstimateSalesAnalysisControl estimateSalesAnalysisControl;
		private ZArchitecture.ZLabel RevenueReportOptionLabel;
		private ZArchitecture.GUI.ZGuidFindBox CompanyFindBox;
	}
}
