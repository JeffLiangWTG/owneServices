namespace Enterprise.MarketingManager.GUI
{
	partial class TouchSummaryGrid
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
			this.touchPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.touchGrid = new Enterprise.ZArchitecture.ZGrid();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.addHorizontalButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.addVerticalButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.removeCampaignButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.openCampaignButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.launchButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.touchPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.touchGrid)).BeginInit();
			this.touchGrid.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TouchSummaryViewModel);
			// 
			// touchPanel
			// 
			this.touchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.touchPanel.Controls.Add(this.touchGrid);
			this.touchPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.touchPanel.Name = "touchPanel";
			this.touchPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 176, true);
			this.touchPanel.TabIndex = 0;
			// 
			// touchGrid
			// 
			this.BindingSource.SetBindingMember(this.touchGrid, "MasterCampaign.AllTouches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).AllTouches)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|cef4c809-4e1d-40a7-bde4-ee3229d2a475", "Touch ID");
			zTextBoxColumnStyleInfo1.ColumnName = "TouchId";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|5035d2c7-a4b7-41bb-be7a-b850899533e9", "Campaign Name");
			zTextBoxColumnStyleInfo2.ColumnName = "G0_CampaignNameMultilingual";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|16c613a3-8003-4602-8b91-a32c8f59e418", "Total");
			zTextBoxColumnStyleInfo3.ColumnName = "SummaryStats+TotalCount";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|0dde4669-a3ad-4e46-beee-7136c7a59753", "Verified");
			zTextBoxColumnStyleInfo4.ColumnName = "SummaryStats+TouchSummaryVerifiedCount";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|469970da-1efc-49f3-a2cd-7e2f73cf0069", "Unverified");
			zTextBoxColumnStyleInfo5.ColumnName = "SummaryStats+TouchSummaryUnverifiedCount";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|65eef34f-fb5d-4280-b6ef-35229f44712f", "Non-Delivery Receipt");
			zTextBoxColumnStyleInfo6.ColumnName = "SummaryStats+NonDeliveredCount";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|0fd141eb-72c3-4029-aa6f-edeea63e586c", "Queued");
			zTextBoxColumnStyleInfo7.ColumnName = "SummaryStats+QueuedCount";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|12e5025f-91af-450c-bced-7947f2f87a53", "Scheduled");
			zTextBoxColumnStyleInfo8.ColumnName = "SummaryStats+ScheduledCount";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|481c10b2-a224-4e79-b80f-1b59bbb05349", "Unscheduled");
			zTextBoxColumnStyleInfo9.ColumnName = "SummaryStats+UnScheduledCount";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|06ead0e9-0639-43ba-928b-24560b93fc65", "Sent");
			zTextBoxColumnStyleInfo10.ColumnName = "SummaryStats+SentCount";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|ae5131e2-55d7-4bea-8a52-5ffc7dbfbe7f", "Unsubscribed");
			zTextBoxColumnStyleInfo11.ColumnName = "SummaryStats+UnsubscribedCount";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|911d74d7-d1da-4f9f-a9c9-0528b73e3c69", "Failed");
			zTextBoxColumnStyleInfo12.ColumnName = "SummaryStats+FailedToTransitionCount";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|519fe92a-9b10-47ff-87ac-b0cef387e6ee", "Transitioned");
			zTextBoxColumnStyleInfo13.ColumnName = "SummaryStats+TransitionedToNextTouchCount";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.touchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.touchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.touchGrid.AllowNavigation = false;
			this.touchGrid.CaptionVisible = false;
			this.touchGrid.GridId = "1d3cbacc-c68f-4fe1-8bd4-f0ac11b1fd87";
			this.touchGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.touchGrid.LayoutKey = "touchGrid";
			this.touchGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.touchGrid.Name = "touchGrid";
			this.touchGrid.ShouldSetErrorsOnTabPage = false;
			this.touchGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 146, true);
			this.touchGrid.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.addHorizontalButton,
			this.addVerticalButton,
			this.removeCampaignButton,
			this.openCampaignButton,
			this.launchButton,
			this.refreshButton});
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 50, true);
			this.toolStrip.TabIndex = 1;
			// 
			// addHorizontalButton
			//
			this.addHorizontalButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|42cd470c-8ee7-4ec5-9399-f51051f9089e", "New Horizontal Touch");
			this.addHorizontalButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.AddHorizontalImage;
			this.addHorizontalButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.addHorizontalButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.addHorizontalButton.Name = "addHorizontalButton";
			this.addHorizontalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.addHorizontalButton.Click += new System.EventHandler(this.AddHorizontalButton_Click);
			// 
			// addVerticalButton
			// 
			this.addVerticalButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|05f9812a-0377-47b0-aaa0-f26d414f0737", "New Vertical Touch");
			this.addVerticalButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.AddVerticalImage;
			this.addVerticalButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.addVerticalButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.addVerticalButton.Name = "addVerticalButton";
			this.addVerticalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.addVerticalButton.Click += new System.EventHandler(this.AddVerticalButton_Click);
			// 
			// removeCampaignButton
			// 
			this.removeCampaignButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|dd922504-8531-4f7a-8601-a47409688c6f", "Delete");
			this.removeCampaignButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.DeleteImage;
			this.removeCampaignButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.removeCampaignButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.removeCampaignButton.Name = "removeCampaignButton";
			this.removeCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.removeCampaignButton.Click += new System.EventHandler(this.RemoveCampaignButton_Click);
			// 
			// openCampaignButton
			// 
			this.openCampaignButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|44b7ad67-5aa4-4da2-87e2-4bad972e9bd5", "Edit");
			this.openCampaignButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.EditImage;
			this.openCampaignButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.openCampaignButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.openCampaignButton.Name = "openCampaignButton";
			this.openCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.openCampaignButton.Click += new System.EventHandler(this.OpenCampaignButton_Click);
			// 
			// launchButton
			// 
			this.launchButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|00e534a8-9d96-4e21-be71-f7c60b695598", "Transition Master List");
			this.launchButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.TransitionImage;
			this.launchButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.launchButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.launchButton.Name = "launchButton";
			this.launchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.launchButton.Click += new System.EventHandler(this.LaunchButton_Click);
			// 
			// refreshButton
			// 
			this.refreshButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchSummaryGrid|28cb2c92-f6ee-4b33-853c-6ab42cfe0fb5", "Refresh");
			this.refreshButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.RefreshImage;
			this.refreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.refreshButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 25, true);
			this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			//
			// TouchSummaryGrid
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.touchPanel);
			this.Controls.Add(this.toolStrip);
			this.Name = "TouchSummaryGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.touchPanel.ResumeLayout(false);
			this.touchPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.touchGrid)).EndInit();
			this.touchGrid.ResumeLayout(false);
			this.touchGrid.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private Enterprise.ZArchitecture.GUI.ZPanel touchPanel;
		private Enterprise.ZArchitecture.ZGrid touchGrid;
		private Enterprise.ZArchitecture.GUI.ZToolStrip toolStrip;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton addHorizontalButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton addVerticalButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton removeCampaignButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton openCampaignButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton launchButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton refreshButton;

		#endregion
	}
}
