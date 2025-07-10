namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignTrackingLinkActivityUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LastActivityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstActivityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TrackingStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ViewDetailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.uniqueOpensLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReportByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.infoPanel = new ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
			this.LinksGrid.SuspendLayout();
			this.ReportByDropEdit.SuspendLayout();
			this.infoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.CampaignItemClickStatModel);
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.SummaryGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("006e97ff-0515-4611-a0bb-97ebf6dd2eb3", "Summary");
			this.SummaryGroupBox.Controls.Add(this.LastActivityLabel);
			this.SummaryGroupBox.Controls.Add(this.FirstActivityLabel);
			this.SummaryGroupBox.Controls.Add(this.TrackingStatusLabel);
			this.SummaryGroupBox.Controls.Add(this.zLabel3);
			this.SummaryGroupBox.Controls.Add(this.zLabel2);
			this.SummaryGroupBox.Controls.Add(this.zLabel1);
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 224, true);
			this.SummaryGroupBox.TabIndex = 0;
			this.SummaryGroupBox.TabStop = false;
			// 
			// LastActivityLabel
			// 
			this.BindingSource.SetBindingMember(this.LastActivityLabel, "LastActivityDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LastActivityDays)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LastActivityLabel, false);
			this.LastActivityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 56, true);
			this.LastActivityLabel.Name = "LastActivityLabel";
			this.LastActivityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LastActivityLabel.TabIndex = 6;
			// 
			// FirstActivityLabel
			// 
			this.BindingSource.SetBindingMember(this.FirstActivityLabel, "FirstActivityDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).FirstActivityDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FirstActivityLabel, false);
			this.FirstActivityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 36, true);
			this.FirstActivityLabel.Name = "FirstActivityLabel";
			this.FirstActivityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.FirstActivityLabel.TabIndex = 5;
			// 
			// TrackingStatusLabel
			// 
			this.TrackingStatusLabel.BackColor = System.Drawing.Color.Silver;
			this.BindingSource.SetBindingMember(this.TrackingStatusLabel, "DeliveryStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).DeliveryStatusDescription)));
			this.TrackingStatusLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("82e8cb55-f0fb-488c-8517-8097e9dca2ef", "Unknown receipt");
			this.TrackingStatusLabel.ForeColor = System.Drawing.Color.White;
			this.TrackingStatusLabel.IsFontBold = true;
			this.TrackingStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 16, true);
			this.TrackingStatusLabel.Name = "TrackingStatusLabel";
			this.TrackingStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TrackingStatusLabel.TabIndex = 4;
			this.TrackingStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2a8a890e-460c-4cbe-a29d-6ce3819fd33c", "Last Activity:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel3.TabIndex = 2;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("015ae6a0-9b5b-4b9a-a374-98a31bd8ed7d", "Verified Date:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 36, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel2.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0ab6fe57-d2ca-4774-b41f-30c51b165f2b", "Tracking Status:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel1.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b74b7a5b-078f-44a6-89c5-105677907401", "Details");
			this.DetailsGroupBox.Controls.Add(this.ViewDetailsButton);
			this.DetailsGroupBox.Controls.Add(this.LinksGrid);
			this.DetailsGroupBox.Controls.Add(this.infoPanel);
			this.DetailsGroupBox.Controls.Add(this.ReportByDropEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 224, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ViewDetailsButton
			// 
			this.ViewDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewDetailsButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("cfec9789-0a15-4c46-98f0-0971ad318756", "View Details");
			this.ViewDetailsButton.Enabled = false;
			this.ViewDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 13, true);
			this.ViewDetailsButton.Name = "ViewDetailsButton";
			this.ViewDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.ViewDetailsButton.TabIndex = 3;
			this.ViewDetailsButton.UseVisualStyleBackColor = true;
			this.ViewDetailsButton.Click += new System.EventHandler(this.ViewDetailsButton_Click);
			// 
			// LinksGrid
			// 
			this.LinksGrid.AllowNavigation = false;
			this.LinksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LinksGrid, "LinkClicks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)).SyncRoot)).Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.CampaignItemClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)).SyncRoot)).Clicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.CampaignItemClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)).SyncRoot)).FirstClick)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)).SyncRoot)).LastClick)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).LinkClicks)).SyncRoot)).URL)));
			this.LinksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b1ba810e-88d9-436d-9d2c-bd5a56a5e019", "Context Display Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Context";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("060b2c30-a785-41ab-9ecb-837636e6e941", "Clicks");
			zCalcEditColumnStyleInfo1.ColumnName = "Clicks";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5bdd7fd0-69e9-4e55-abc2-201d7d2a1537", "First Click");
			zDateEditColumnStyleInfo1.ColumnName = "FirstClick";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a07b430a-496e-43e9-8ad9-46c431bc877b", "Last Click");
			zTextBoxColumnStyleInfo2.ColumnName = "LastClick";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a82e96da-6539-4899-9227-50f3abc9d41d", "Destination URL");
			zTextBoxColumnStyleInfo3.ColumnName = "URL";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinksGrid.CopySelectedRowsAllowed = true;
			this.LinksGrid.GridId = "aa094049-7a99-4605-a579-a22a7928f236";
			this.LinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinksGrid.LayoutKey = "LinksGrid";
			this.LinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 43, true);
			this.LinksGrid.Name = "LinksGrid";
			this.LinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 175, true);
			this.LinksGrid.TabIndex = 2;
			// 
			// infoPanel
			// 
			this.infoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)
			| System.Windows.Forms.AnchorStyles.Top)));
			this.infoPanel.Controls.Add(this.contactNameLabel);
			this.infoPanel.Controls.Add(this.uniqueOpensLabel);
			this.infoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 20, true);
			this.infoPanel.Name = "infoPanel";
			this.infoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 23, true);
			this.infoPanel.TabIndex = 1;
			// 
			// contactNameLabel
			// 
			this.contactNameLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.contactNameLabel, "ContactNameAssociatedWithUniqueOpensText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).ContactNameAssociatedWithUniqueOpensText)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.contactNameLabel, false);
			this.contactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contactNameLabel.Name = "contactNameLabel";
			this.contactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 23, true);
			this.contactNameLabel.TabIndex = 0;
			this.contactNameLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.contactNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
			// 
			// uniqueOpensLabel
			// 
			this.uniqueOpensLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.uniqueOpensLabel, "TotalUniqueOpensText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).TotalUniqueOpensText)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.uniqueOpensLabel, false);
			this.uniqueOpensLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 0, true);
			this.uniqueOpensLabel.Name = "uniqueOpensLabel";
			this.uniqueOpensLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.uniqueOpensLabel.TabIndex = 1;
			this.uniqueOpensLabel.IsFontBold = true;
			this.uniqueOpensLabel.Dock = System.Windows.Forms.DockStyle.Left;
			// 
			// ReportByDropEdit
			// 
			this.ReportByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportByDropEdit, "ReportBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.CampaignItemClickStatModel)(null)).ReportBy)));
			this.ReportByDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fc80bfb7-33ee-465d-aea8-be1660ba6b5a", "Report By");
			this.ReportByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 16, true);
			this.ReportByDropEdit.Name = "ReportByDropEdit";
			this.ReportByDropEdit.PreBoundMaxLength = 25;
			this.ReportByDropEdit.ShowDescriptionBox = false;
			this.ReportByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ReportByDropEdit.TabIndex = 0;
			// 
			// CampaignTrackingLinkActivityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.SummaryGroupBox);
			this.Name = "CampaignTrackingLinkActivityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.infoPanel.ResumeLayout(false);
			this.infoPanel.PerformLayout();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
			this.LinksGrid.ResumeLayout(false);
			this.LinksGrid.PerformLayout();
			this.ReportByDropEdit.ResumeLayout(true);
			this.ReportByDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		internal ZArchitecture.ZLabel TrackingStatusLabel;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel1;
		internal ZArchitecture.GUI.ZButton ViewDetailsButton;
		private ZArchitecture.ZGrid LinksGrid;
		private ZArchitecture.ZLabel uniqueOpensLabel;
		private ZArchitecture.ZLabel contactNameLabel;
		private ZArchitecture.GUI.ZDropEdit ReportByDropEdit;
		private ZArchitecture.ZLabel LastActivityLabel;
		private ZArchitecture.ZLabel FirstActivityLabel;
		private ZArchitecture.GUI.ZPanel infoPanel;
	}
}
