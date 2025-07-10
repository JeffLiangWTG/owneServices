using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	partial class IntegratedTouchSummary
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
#if WINZOR
			this.TouchSummaryContainer = new Enterprise.MarketingManager.GUI.TouchSummaryGrid();
#else
			this.TouchSummaryContainer = new Enterprise.ZArchitecture.GUI.ZElementHost();
#endif
			this.SummaryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliverySummaryPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ClickStatControl = new Enterprise.MarketingManager.GUI.CampaignClickStatDetailsUserControl();
			this.TrackingStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpportunitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransitionProgressControl = new Enterprise.MarketingManager.GUI.TransitionProgressChartUserControl();
			this.HorizontalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.VerticalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TransitionProgressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryPanel.SuspendLayout();
			this.DeliverySummaryPanel.SuspendLayout();
			this.ClickStatControl.SuspendLayout();
			this.TrackingStatusGroupBox.SuspendLayout();
			this.OpportunitiesGroupBox.SuspendLayout();
			this.TransitionProgressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HorizontalSplitContainer)).BeginInit();
			this.HorizontalSplitContainer.Panel1.SuspendLayout();
			this.HorizontalSplitContainer.Panel2.SuspendLayout();
			this.HorizontalSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VerticalSplitContainer)).BeginInit();
			this.VerticalSplitContainer.Panel1.SuspendLayout();
			this.VerticalSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// TouchSummaryContainer
			// 
			this.TouchSummaryContainer.BackColor = System.Drawing.Color.Transparent;
			this.TouchSummaryContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TouchSummaryContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TouchSummaryContainer.Name = "TouchSummaryContainer";
			this.TouchSummaryContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 267, true);
			this.TouchSummaryContainer.TabIndex = 0;
			// 
			// SummaryPanel
			//
			this.SummaryPanel.Controls.Add(this.ClickStatControl);
			this.SummaryPanel.Controls.Add(this.DeliverySummaryPanel);
			this.SummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.SummaryPanel.Name = "SummaryPanel";
			this.SummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 560, true);
			this.SummaryPanel.TabIndex = 1;
			// 
			// DeliverySummaryPanel
			//
			this.DeliverySummaryPanel.ColumnCount = 2;
			this.DeliverySummaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DeliverySummaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DeliverySummaryPanel.Controls.Add(this.TrackingStatusGroupBox, 0 ,0);
            this.DeliverySummaryPanel.Controls.Add(this.OpportunitiesGroupBox, 1, 0);
            this.DeliverySummaryPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DeliverySummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.DeliverySummaryPanel.Name = "DeliverySummaryPanel";
			this.DeliverySummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 160, true);
			this.DeliverySummaryPanel.TabIndex = 1;
			// 
			// ClickStatControl
			// 
			this.ClickStatControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClickStatControl, "StatModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.ClickStatModel)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).StatModel)));
			this.ClickStatControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClickStatControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.ClickStatControl.Name = "ClickStatControl";
			this.ClickStatControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.ClickStatControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 400, true);
			this.ClickStatControl.TabIndex = 1;
			// 
			// TrackingStatusGroupBox
			// 
			this.TrackingStatusGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.TrackingStatusGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9837A4EC-C2F7-4A9A-A015-1110F017ADA5", "Campaign Summary");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TrackingStatusGroupBox, false);
			this.TrackingStatusGroupBox.Name = "TrackingStatusGroupBox";
			this.TrackingStatusGroupBox.TabIndex = 0;
			this.TrackingStatusGroupBox.TabStop = false;
			// 
			// OpportunitiesGroupBox
			//
			this.OpportunitiesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.OpportunitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7B2FFEF2-BDC2-4154-A158-A2225DECFB6F", "Linked Opportunities");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OpportunitiesGroupBox, false);
			this.OpportunitiesGroupBox.Name = "OpportunitiesGroupBox";
			this.OpportunitiesGroupBox.TabIndex = 0;
			this.OpportunitiesGroupBox.TabStop = false;
			// 
			// HorizontalSplitContainer
			// 
			this.HorizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HorizontalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HorizontalSplitContainer.Name = "HorizontalSplitContainer";
			// 
			// HorizontalSplitContainer.Panel1
			// 
			this.HorizontalSplitContainer.Panel1.Controls.Add(this.VerticalSplitContainer);
			this.HorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 560, true);
			this.HorizontalSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(560);
			// 
			// HorizontalSplitContainer.Panel2
			// 
			this.HorizontalSplitContainer.Panel2.Controls.Add(this.SummaryPanel);
			this.HorizontalSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.HorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(413);
			this.HorizontalSplitContainer.TabIndex = 2;
			// 
			// VerticalSplitContainer
			// 
			this.VerticalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VerticalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VerticalSplitContainer.Name = "VerticalSplitContainer";
			this.VerticalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// VerticalSplitContainer.Panel2
			// 
			this.VerticalSplitContainer.Panel2.Controls.Add(this.TransitionProgressGroupBox);
			this.VerticalSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.VerticalSplitContainer.Panel2.AutoScroll = true;
			// 
			// 
			// VerticalSplitContainer.Panel1
			// 
			this.VerticalSplitContainer.Panel1.Controls.Add(this.TouchSummaryContainer);
			this.VerticalSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.VerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 600, true);
			this.VerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(350);
			this.VerticalSplitContainer.TabIndex = 0;
			// 
			// TransitionProgressGroupBox
			// 
			this.TransitionProgressGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9605C870-BDDB-4451-A7E0-9D250C5EA536", "Transition Progress");
			this.TransitionProgressGroupBox.Controls.Add(this.TransitionProgressControl);
			this.TransitionProgressGroupBox.Dock = DockStyle.Top | DockStyle.Bottom;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TransitionProgressGroupBox, false);
			this.TransitionProgressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransitionProgressGroupBox.Name = "TransitionProgressGroupBox";
			this.TransitionProgressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1440, 175, true);
			this.TransitionProgressGroupBox.TabIndex = 0;
			this.TransitionProgressGroupBox.TabStop = false;
#if WINZOR
			this.TransitionProgressGroupBox.Visible = false;
#endif
			// 
			// TransitionProgressControl
			// 
			this.TransitionProgressControl.AllowDrop = true;
			this.TransitionProgressControl.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.TransitionProgressControl, ".");
			this.TransitionProgressControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransitionProgressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransitionProgressControl.Name = "TransitionProgressControl";
			this.TransitionProgressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1440, 175, true);
			this.TransitionProgressControl.TabIndex = 0;
			// 
			// IntegratedTouchSummary
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HorizontalSplitContainer);
			this.Name = "IntegratedTouchSummary";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1440, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliverySummaryPanel.ResumeLayout(false);
			this.DeliverySummaryPanel.PerformLayout();
			this.SummaryPanel.ResumeLayout(false);
			this.SummaryPanel.PerformLayout();
			this.ClickStatControl.ResumeLayout(true);
			this.ClickStatControl.PerformLayout();
			this.TrackingStatusGroupBox.ResumeLayout(false);
			this.TrackingStatusGroupBox.PerformLayout();
			this.OpportunitiesGroupBox.ResumeLayout(false);
			this.OpportunitiesGroupBox.PerformLayout();
			this.TransitionProgressControl.ResumeLayout(false);
			this.TransitionProgressControl.PerformLayout();
			this.HorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.HorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HorizontalSplitContainer)).EndInit();
			this.HorizontalSplitContainer.ResumeLayout(false);
			this.HorizontalSplitContainer.PerformLayout();
			this.VerticalSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.VerticalSplitContainer)).EndInit();
			this.VerticalSplitContainer.ResumeLayout(false);
			this.VerticalSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

#if WINZOR
		protected Enterprise.MarketingManager.GUI.TouchSummaryGrid TouchSummaryContainer;
#else
		protected Enterprise.ZArchitecture.GUI.ZElementHost TouchSummaryContainer;
#endif
		private Enterprise.ZArchitecture.GUI.ZPanel SummaryPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel DeliverySummaryPanel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox TrackingStatusGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OpportunitiesGroupBox;
		private Enterprise.MarketingManager.GUI.TransitionProgressChartUserControl TransitionProgressControl;
		protected CargoWise.Windows.UI.KSplitContainer HorizontalSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer VerticalSplitContainer;
		protected Enterprise.MarketingManager.GUI.CampaignClickStatDetailsUserControl ClickStatControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TransitionProgressGroupBox;
	}
}
