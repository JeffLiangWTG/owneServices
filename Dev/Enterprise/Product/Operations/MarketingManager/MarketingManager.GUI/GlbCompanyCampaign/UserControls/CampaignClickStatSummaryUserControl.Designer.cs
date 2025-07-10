namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignClickStatSummaryUserControl
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
            this.trackingStatToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.trackingStatToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.LinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReportByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReportRangeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.ClickStatModel);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).URL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).Clicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).LinkClicks)).SyncRoot)).ClicksRate)));
			this.LinksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f20ecb13-df43-4837-a249-89e914f66052", "Context Display Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Context";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7bcf6f84-00af-464d-a240-4273cea15a5d", "Destination URL");
			zTextBoxColumnStyleInfo2.ColumnName = "URL";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5f79eff4-8c5d-4871-9ce0-7ceef3491eb0", "Clicks");
			zCalcEditColumnStyleInfo1.ColumnName = "Clicks";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7caff293-23aa-40f8-bf38-ac4442ade31e", "Unique CTR");
			zTextBoxColumnStyleInfo3.ColumnName = "ClicksRatePercentage";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("569482cf-ee1e-4415-9b79-2f6b00a5a0b6", "Unique Clicks");
			zCalcEditColumnStyleInfo3.ColumnName = "UniqueClicks";
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinksGrid.CopySelectedRowsAllowed = true;
			this.LinksGrid.GridId = "dc389c96-f7d0-4d24-803b-fb3406cef9a7";
			this.LinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinksGrid.LayoutKey = "LinksGrid";
			this.LinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.LinksGrid.Name = "LinksGrid";
			this.LinksGrid.ReadOnly = true;
			this.LinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 139, true);
			this.LinksGrid.TabIndex = 5;
			this.LinksGrid.DoubleClick += new System.EventHandler(LinksGrid_DoubleClick);
			// 
			// ReportByDropEdit
			// 
			this.ReportByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportByDropEdit, "ReportBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ReportBy)));
			this.ReportByDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("848da765-5cf4-4197-bfc4-c63034b492ed", "Report By");
			this.ReportByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 7, true);
			this.ReportByDropEdit.Name = "ReportByDropEdit";
			this.ReportByDropEdit.PreBoundMaxLength = 3;
			this.ReportByDropEdit.ShowDescriptionBox = true;
			this.ReportByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ReportByDropEdit.TabIndex = 1;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "FromDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).FromDateTime)));
			this.FromDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b30bfdb5-e61d-461b-a764-a8bf2e810499", "From");
			this.FromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 7, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 3;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "ToDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ToDateTime)));
			this.ToDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9debff04-8f6c-4562-9e6b-defe03eb87e3", "To");
			this.ToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(727, 7, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 4;
            // 
            // buttonsToolStrip
            // 
            this.trackingStatToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.trackingStatToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.trackingStatToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.trackingStatToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.trackingStatToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackingStatToolStripButton});
            this.trackingStatToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(713, 180, true);
            this.trackingStatToolStrip.Name = "buttonsToolStrip";
            this.trackingStatToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 26, true);
            this.trackingStatToolStrip.TabIndex = 6;
            this.trackingStatToolStrip.Text = "zToolStrip1";
            // 
            // dropDownButtonsToolStrip
            // 
            this.trackingStatToolStripButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6ac04b6a-1500-44bc-b5c3-b40d06eec8e9", "Timeline View");
            this.trackingStatToolStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.trackingStatToolStripButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.ChartsImage;
            this.trackingStatToolStripButton.Name = "trackingStatToolStripButton";
            this.trackingStatToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 24, true);
            this.trackingStatToolStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.trackingStatToolStripButton.Click += new System.EventHandler(this.TrackingStatButton_Click);
			// 
			// ReportRangeDropEdit
			// 
			this.ReportRangeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportRangeDropEdit, "ReportTimeRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.ClickStatModel)(null)).ReportTimeRange)));
			this.ReportRangeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f031ace0-03ac-4d14-86e1-0d179f531182", "Date Range");
			this.ReportRangeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 7, true);
			this.ReportRangeDropEdit.Name = "ReportRangeDropEdit";
			this.ReportRangeDropEdit.PreBoundMaxLength = 3;
			this.ReportRangeDropEdit.ShowDescriptionBox = true;
			this.ReportRangeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ReportRangeDropEdit.TabIndex = 2;
			// 
			// CampaignClickStatUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportRangeDropEdit);
			this.Controls.Add(this.trackingStatToolStrip);
			this.Controls.Add(this.ToDateEdit);
			this.Controls.Add(this.FromDateEdit);
			this.Controls.Add(this.ReportByDropEdit);
			this.Controls.Add(this.LinksGrid);
			this.Name = "CampaignClickStatUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 213, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZGrid LinksGrid;
		private ZArchitecture.GUI.ZDropEdit ReportByDropEdit;
		private ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth ReportRangeDropEdit;
        private Enterprise.ZArchitecture.GUI.ZToolStrip trackingStatToolStrip;
	    private Enterprise.ZArchitecture.GUI.ZToolStripButton trackingStatToolStripButton;
	}
}
