namespace Enterprise.MasterFiles.GUI
{
	partial class EventsBannerControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.latestCreditEventsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.latestCreditEventsContainerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.eventListPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.eventListUnavailableMessageControl = new Enterprise.MasterFiles.GUI.EventsUnavailableMessageControl();
			this.latestCreditEventsTitlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EventsTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.latestCreditEventsContainerPanel.SuspendLayout();
			this.eventListPanel.SuspendLayout();
			this.eventListUnavailableMessageControl.SuspendLayout();
			this.latestCreditEventsTitlePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.EventsBannerModel);
			// 
			// latestCreditEventsLabel
			// 
			this.BindingSource.SetBindingMember(this.latestCreditEventsLabel, "EventsBannerCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.EventsBannerModel)(null)).EventsBannerCaption)));
			this.latestCreditEventsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.latestCreditEventsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.latestCreditEventsLabel.Name = "latestCreditEventsLabel";
			this.latestCreditEventsLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 0, 0, true);
			this.latestCreditEventsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 40, true);
			this.latestCreditEventsLabel.TabIndex = 2;
			this.latestCreditEventsLabel.UseMnemonic = false;
			// 
			// latestCreditEventsContainerPanel
			// 
			this.latestCreditEventsContainerPanel.AutoSize = true;
			this.latestCreditEventsContainerPanel.Controls.Add(this.eventListPanel);
			this.latestCreditEventsContainerPanel.Controls.Add(this.latestCreditEventsTitlePanel);
			this.latestCreditEventsContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.latestCreditEventsContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.latestCreditEventsContainerPanel.Name = "latestCreditEventsContainerPanel";
			this.latestCreditEventsContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 670, true);
			this.latestCreditEventsContainerPanel.TabIndex = 0;
			// 
			// eventListPanel
			// 
			this.eventListPanel.AutoScroll = true;
			this.eventListPanel.Controls.Add(this.eventListUnavailableMessageControl);
			this.eventListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.eventListPanel.Name = "eventListPanel";
			this.eventListPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 630, true);
			this.eventListPanel.TabIndex = 0;
			// 
			// eventListUnavailableMessageControl
			// 
			this.eventListUnavailableMessageControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eventListUnavailableMessageControl, ".");
			this.eventListUnavailableMessageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventListUnavailableMessageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eventListUnavailableMessageControl.Name = "eventListUnavailableMessageControl";
			this.eventListUnavailableMessageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 630, true);
			this.eventListUnavailableMessageControl.TabIndex = 0;
			// 
			// latestCreditEventsTitlePanel
			// 
			this.latestCreditEventsTitlePanel.Controls.Add(this.latestCreditEventsLabel);
			this.latestCreditEventsTitlePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.latestCreditEventsTitlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.latestCreditEventsTitlePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.latestCreditEventsTitlePanel.Name = "latestCreditEventsTitlePanel";
			this.latestCreditEventsTitlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 40, true);
			this.latestCreditEventsTitlePanel.TabIndex = 0;
			// 
			// EventsTableLayout
			// 
			this.EventsTableLayout.AutoSize = true;
			this.EventsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.EventsTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.EventsTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EventsTableLayout.Name = "EventsTableLayout";
			this.EventsTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 0, true);
			this.EventsTableLayout.TabIndex = 1;
			// 
			// EventsBannerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.latestCreditEventsContainerPanel);
			this.Name = "EventsBannerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 670, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.latestCreditEventsContainerPanel.ResumeLayout(false);
			this.latestCreditEventsContainerPanel.PerformLayout();
			this.eventListPanel.ResumeLayout(false);
			this.eventListPanel.PerformLayout();
			this.eventListUnavailableMessageControl.ResumeLayout(true);
			this.eventListUnavailableMessageControl.PerformLayout();
			this.latestCreditEventsTitlePanel.ResumeLayout(false);
			this.latestCreditEventsTitlePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel latestCreditEventsLabel;
		private ZArchitecture.GUI.ZPanel latestCreditEventsContainerPanel;
		private ZArchitecture.GUI.ZPanel latestCreditEventsTitlePanel;
		private ZArchitecture.GUI.ZPanel eventListPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel EventsTableLayout;
		private EventsUnavailableMessageControl eventListUnavailableMessageControl;
	}
}
