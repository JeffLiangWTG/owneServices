namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignClickStatDetailsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.outerSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReportTimeRangeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReportByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.innerSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EmailClickThroughSummaryGroupBox = new ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outerSplitContainer)).BeginInit();
			this.outerSplitContainer.Panel1.SuspendLayout();
			this.outerSplitContainer.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).BeginInit();
			this.innerSplitContainer.Panel1.SuspendLayout();
			this.innerSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.ClickStatModel);
			// 
			// outerSplitContainer
			// 
			this.outerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outerSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.outerSplitContainer.Name = "outerSplitContainer";
			this.outerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// outerSplitContainer.Panel1
			// 
			this.outerSplitContainer.Panel1.Controls.Add(this.zGroupBox1);
			this.outerSplitContainer.Panel1.Controls.Add(this.innerSplitContainer);
			// 
			// outerSplitContainer.Panel2
			// 
			this.outerSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.outerSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 560, true);
			this.outerSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(247);
			this.outerSplitContainer.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.RefreshButton);
			this.zGroupBox1.Controls.Add(this.ReportTimeRangeDropEdit);
			this.zGroupBox1.Controls.Add(this.ToDateEdit);
			this.zGroupBox1.Controls.Add(this.FromDateEdit);
			this.zGroupBox1.Controls.Add(this.ReportByDropEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 37, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// RefreshButton
			// 
			this.RefreshButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.RefreshImage;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 10, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 22, true);
			this.RefreshButton.TabIndex = 5;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// ReportTimeRangeDropEdit
			// 
			this.ReportTimeRangeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportTimeRangeDropEdit, "ReportTimeRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ReportTimeRange)));
			this.ReportTimeRangeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2060e5f9-0ff2-48d5-ab81-7e44e76374fc", "Date Range");
			this.ReportTimeRangeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 11, true);
			this.ReportTimeRangeDropEdit.Name = "ReportTimeRangeDropEdit";
			this.ReportTimeRangeDropEdit.PreBoundMaxLength = 25;
			this.ReportTimeRangeDropEdit.ShowDescriptionBox = false;
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
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 11, true);
			this.ToDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
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
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 11, true);
			this.FromDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
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
			this.ReportByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 11, true);
			this.ReportByDropEdit.Name = "ReportByDropEdit";
			this.ReportByDropEdit.PreBoundMaxLength = 25;
			this.ReportByDropEdit.ShowDescriptionBox = false;
			this.ReportByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ReportByDropEdit.TabIndex = 1;
			// 
			// innerSplitContainer
			// 
			this.innerSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.innerSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
			this.innerSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.innerSplitContainer.Name = "innerSplitContainer";
			// 
			// innerSplitContainer.Panel1
			// 
			this.innerSplitContainer.Panel1.Controls.Add(this.LinksGrid);
			this.innerSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 201, true);
			this.innerSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(402);
			this.innerSplitContainer.TabIndex = 6;
			// 
			// LinksGrid
			// 
			this.LinksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinksGrid, "LinkClicks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).ViewInChart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Clicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).ClicksRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).URL)));
			this.LinksGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3130b896-ecc4-4f2f-9789-1cf71e9389fd", "Report");
			zCheckBoxColumnStyleInfo1.ColumnName = "ViewInChart";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2c974101-05cd-46cb-b2a1-1a84cbafbef9", "Context Display Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Context";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7acf65c1-02f6-489c-9256-c6e5562cb374", "Clicks");
			zCalcEditColumnStyleInfo1.ColumnName = "Clicks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d9c37783-42a0-4dc1-8a26-e96f7279adad", "Unique CTR");
			zTextBoxColumnStyleInfo3.ColumnName = "ClicksRatePercentage";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("710d20e6-8145-416d-a1a2-e20f52b76994", "Unique Clicks");
			zCalcEditColumnStyleInfo3.ColumnName = "UniqueClicks";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e584b1e7-28dd-40e6-81cf-74a082528540", "Destination URL");
			zTextBoxColumnStyleInfo2.ColumnName = "URL";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.LinksGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinksGrid.CopySelectedRowsAllowed = true;
			this.LinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinksGrid.GridId = "641e0d74-b554-4e01-8933-ece279e72cef";
			this.LinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinksGrid.LayoutKey = "zGrid1";
			this.LinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinksGrid.Name = "LinksGrid";
			this.LinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 201, true);
			this.LinksGrid.TabIndex = 6;
			this.LinksGrid.DoubleClick += new System.EventHandler(LinksGrid_DoubleClick);
			//
			// EmailClickThroughSummaryGroupBox
			// 
			this.EmailClickThroughSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EmailClickThroughSummaryGroupBox, false);
			this.EmailClickThroughSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.EmailClickThroughSummaryGroupBox.Name = "EmailClickThroughSummaryGroupBox";
			this.EmailClickThroughSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 560, true);
			this.EmailClickThroughSummaryGroupBox.TabIndex = 4;
			this.EmailClickThroughSummaryGroupBox.TabStop = false;
			this.EmailClickThroughSummaryGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("F1B5DD4E-ABCE-4AC3-B842-5D2B1F02B116", "Email Click Through Summary");
			this.EmailClickThroughSummaryGroupBox.Controls.Add(outerSplitContainer);
			// 
			// CampaignClickStatDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EmailClickThroughSummaryGroupBox);
			this.Name = "CampaignClickStatDetailsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 560, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.outerSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.outerSplitContainer)).EndInit();
			this.outerSplitContainer.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.innerSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).EndInit();
			this.innerSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer outerSplitContainer;
		private ZArchitecture.ZGrid LinksGrid;
		private ZArchitecture.GUI.ZDropEdit ReportByDropEdit;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private ZArchitecture.GUI.ZDropEdit ReportTimeRangeDropEdit;
		protected CargoWise.Windows.UI.KSplitContainer innerSplitContainer;
		private ZArchitecture.GUI.ZButton RefreshButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EmailClickThroughSummaryGroupBox;
	}
}
