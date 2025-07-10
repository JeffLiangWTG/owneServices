namespace Enterprise.Rating.GUI
{
	partial class RateChooserForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.containerTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.summaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.summaryCardsControl = new Enterprise.Rating.GUI.RateChooser.UIControls.RateChooserSummaryCardsControl();
            this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.NotificationLabel = new Enterprise.ZArchitecture.ZLabel();
            this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.tabControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.filterAndResultsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.resultsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.jobValuesLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.containerTabControl.SuspendLayout();
            this.summaryTabPage.SuspendLayout();
            this.summaryCardsControl.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.tabControlPanel.SuspendLayout();
            this.filterAndResultsPanel.SuspendLayout();
            this.resultsPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 739, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 24, true);
            this.MainStatusBar.Visible = false;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooserViewModel);
            // 
            // containerTabControl
            // 
            this.containerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.containerTabControl.Controls.Add(this.summaryTabPage);
            this.containerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.containerTabControl.Name = "containerTabControl";
            this.containerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 659, true);
            this.containerTabControl.TabIndex = 1;
            // 
            // summaryTabPage
            // 
            this.summaryTabPage.AutoScroll = true;
            this.summaryTabPage.Controls.Add(this.summaryCardsControl);
            this.summaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.summaryTabPage.Name = "summaryTabPage";
            this.summaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1177, 632, true);
            this.summaryTabPage.TabIndex = 0;
            // 
            // summaryCardsControl
            // 
            this.summaryCardsControl.AllowDrop = true;
            this.summaryCardsControl.AutoSize = true;
            this.summaryCardsControl.BackColor = System.Drawing.SystemColors.Control;
            this.BindingSource.SetBindingMember(this.summaryCardsControl, ".");
            this.summaryCardsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryCardsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.summaryCardsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.summaryCardsControl.Name = "summaryCardsControl";
            this.summaryCardsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1177, 632, true);
            this.summaryCardsControl.TabIndex = 0;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.NotificationLabel);
            this.buttonPanel.Controls.Add(this.toolStrip);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 711, true);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 28, true);
            this.buttonPanel.TabIndex = 2;
            // 
            // NotificationLabel
            // 
            this.NotificationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.NotificationLabel, "SelectionNotificationText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.GUI.RateChooserViewModel)(null)).SelectionNotificationText)));
            this.NotificationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.NotificationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 4, true);
            this.NotificationLabel.Name = "NotificationLabel";
            this.NotificationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
            this.NotificationLabel.TabIndex = 0;
            this.NotificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStrip
            // 
            this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1077, 3, true);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 25, true);
            this.toolStrip.TabIndex = 1;
            // 
            // tabControlPanel
            // 
            this.tabControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlPanel.BackColor = System.Drawing.SystemColors.Control;
            this.tabControlPanel.Controls.Add(this.containerTabControl);
            this.tabControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.tabControlPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.tabControlPanel.Name = "tabControlPanel";
            this.tabControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 659, true);
            this.tabControlPanel.TabIndex = 3;
            // 
            // filterAndResultsPanel
            // 
            this.filterAndResultsPanel.Controls.Add(this.resultsPanel);
            this.filterAndResultsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterAndResultsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
            this.filterAndResultsPanel.Name = "filterAndResultsPanel";
            this.filterAndResultsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 685, true);
            this.filterAndResultsPanel.TabIndex = 1;
            // 
            // resultsPanel
            // 
            this.resultsPanel.Controls.Add(this.tabControlPanel);
            this.resultsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.resultsPanel.Name = "resultsPanel";
            this.resultsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 685, true);
            this.resultsPanel.TabIndex = 6;
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.jobValuesLabel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 26, true);
            this.topPanel.TabIndex = 0;
            // 
            // jobValuesLabel
            // 
            this.jobValuesLabel.AutoSize = true;
            this.jobValuesLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("b795a629-de66-4993-9fca-3605705610fb", "Origin:");
            this.jobValuesLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.jobValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 6, true);
            this.jobValuesLabel.Name = "jobValuesLabel";
            this.jobValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
            this.jobValuesLabel.TabIndex = 5;
            // 
            // RateChooserForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("f9357384-8f19-46ee-9409-dca4d9f2d5eb", "Rate Selection");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 763, true);
            this.Controls.Add(this.filterAndResultsPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.buttonPanel);
            this.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooserViewModel);
            this.Name = "RateChooserForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.buttonPanel, 0);
            this.Controls.SetChildIndex(this.topPanel, 0);
            this.Controls.SetChildIndex(this.filterAndResultsPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.containerTabControl.ResumeLayout(false);
            this.containerTabControl.PerformLayout();
            this.summaryTabPage.ResumeLayout(false);
            this.summaryTabPage.PerformLayout();
            this.summaryCardsControl.ResumeLayout(true);
            this.summaryCardsControl.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.tabControlPanel.ResumeLayout(false);
            this.tabControlPanel.PerformLayout();
            this.filterAndResultsPanel.ResumeLayout(false);
            this.filterAndResultsPanel.PerformLayout();
            this.resultsPanel.ResumeLayout(false);
            this.resultsPanel.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl containerTabControl;
		private ZArchitecture.GUI.ZPanel buttonPanel;
		private Enterprise.ZArchitecture.GUI.ZToolStrip toolStrip;
		public ZArchitecture.ZLabel NotificationLabel;
		private Enterprise.ZArchitecture.GUI.ZTabPage summaryTabPage;
		private Enterprise.Rating.GUI.RateChooser.UIControls.RateChooserSummaryCardsControl summaryCardsControl;
		private ZArchitecture.GUI.ZPanel tabControlPanel;
		private ZArchitecture.GUI.ZPanel filterAndResultsPanel;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZPanel resultsPanel;
		private ZArchitecture.ZLabel jobValuesLabel;
	}
}
