namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class DpsResultUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DpsResultTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ScreenedPartyLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScrollPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScreenedItemsPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TopBannerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HeaderTextPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ScreenedPartiesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MatchesNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SwitchBarPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.LinePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScreenedPartyControl = new Enterprise.DeniedPartyScreening.GUI.ScreenedPartyUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DpsResultTableLayoutPanel.SuspendLayout();
			this.ScreenedPartyLeftPanel.SuspendLayout();
			this.ScrollPanel.SuspendLayout();
			this.TopBannerPanel.SuspendLayout();
			this.HeaderTextPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SwitchBarPictureBox)).BeginInit();
			this.ScreenedPartyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// DpsResultTableLayoutPanel
			// 
			this.DpsResultTableLayoutPanel.ColumnCount = 2;
			this.DpsResultTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350)));
			this.DpsResultTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DpsResultTableLayoutPanel.Controls.Add(this.ScreenedPartyLeftPanel, 0, 0);
			this.DpsResultTableLayoutPanel.Controls.Add(this.ScreenedPartyControl, 1, 0);
			this.DpsResultTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DpsResultTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DpsResultTableLayoutPanel.Name = "DpsResultTableLayoutPanel";
			this.DpsResultTableLayoutPanel.RowCount = 1;
			this.DpsResultTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DpsResultTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1330, 910, true);
			this.DpsResultTableLayoutPanel.TabIndex = 0;
			// 
			// ScreenedPartyLeftPanel
			// 
			this.ScreenedPartyLeftPanel.Controls.Add(this.ScrollPanel);
			this.ScreenedPartyLeftPanel.Controls.Add(this.TopBannerPanel);
			this.ScreenedPartyLeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScreenedPartyLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ScreenedPartyLeftPanel.Name = "ScreenedPartyLeftPanel";
			this.ScreenedPartyLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 905, true);
			this.ScreenedPartyLeftPanel.TabIndex = 7;
			// 
			// ScrollPanel
			// 
			this.ScrollPanel.AutoScroll = true;
			this.ScrollPanel.AutoSize = true;
			this.ScrollPanel.BackColor = System.Drawing.Color.White;
			this.ScrollPanel.Controls.Add(this.ScreenedItemsPanel);
			this.ScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScrollPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.ScrollPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ScrollPanel.Name = "ScrollPanel";
			this.ScrollPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 860, true);
			this.ScrollPanel.TabIndex = 8;
			// 
			// ScreenedItemsPanel
			// 
			this.ScreenedItemsPanel.AutoSize = true;
			this.ScreenedItemsPanel.ColumnCount = 1;
			this.ScreenedItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ScreenedItemsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ScreenedItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreenedItemsPanel.Name = "ScreenedItemsPanel";
			this.ScreenedItemsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 0, true);
			this.ScreenedItemsPanel.RowCount = 1;
			this.ScreenedItemsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ScreenedItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 2, true);
			this.ScreenedItemsPanel.TabIndex = 0;
			// 
			// TopBannerPanel
			// 
			this.TopBannerPanel.BackColor = System.Drawing.Color.White;
			this.TopBannerPanel.Controls.Add(this.HeaderTextPanel);
			this.TopBannerPanel.Controls.Add(this.SwitchBarPictureBox);
			this.TopBannerPanel.Controls.Add(this.LinePanel);
			this.TopBannerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopBannerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopBannerPanel.Name = "TopBannerPanel";
			this.TopBannerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 45, true);
			this.TopBannerPanel.TabIndex = 7;
			// 
			// HeaderTextPanel
			// 
			this.HeaderTextPanel.Controls.Add(this.ScreenedPartiesLabel);
			this.HeaderTextPanel.Controls.Add(this.MatchesNumberLabel);
			this.HeaderTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 7, true);
			this.HeaderTextPanel.Name = "HeaderTextPanel";
			this.HeaderTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 26, true);
			this.HeaderTextPanel.TabIndex = 1;
			// 
			// ScreenedPartiesLabel
			// 
			this.ScreenedPartiesLabel.AutoSize = true;
			this.ScreenedPartiesLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
			this.ScreenedPartiesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.ScreenedPartiesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreenedPartiesLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ScreenedPartiesLabel.Name = "ScreenedPartiesLabel";
			this.ScreenedPartiesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 19, true);
			this.ScreenedPartiesLabel.TabIndex = 0;
			this.ScreenedPartiesLabel.UseMnemonic = false;
			// 
			// MatchesNumberLabel
			// 
			this.MatchesNumberLabel.AutoSize = true;
			this.MatchesNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MatchesNumberLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.MatchesNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchesNumberLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MatchesNumberLabel.Name = "MatchesNumberLabel";
			this.MatchesNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 16, true);
			this.MatchesNumberLabel.TabIndex = 1;
			this.MatchesNumberLabel.UseMnemonic = false;
			// 
			// SwitchBarPictureBox
			// 
			this.SwitchBarPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SwitchBarPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.SwitchBarPictureBox.Image = global::Enterprise.DeniedPartyScreening.GUI.Properties.Resources.collapseDrawingGroup;
			this.SwitchBarPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 6, true);
			this.SwitchBarPictureBox.Name = "SwitchBarPictureBox";
			this.SwitchBarPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 24, true);
			this.SwitchBarPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.SwitchBarPictureBox.TabIndex = 4;
			this.SwitchBarPictureBox.TabStop = false;
			this.SwitchBarPictureBox.Click += new System.EventHandler(this.SwitchBarPictureBox_Click);
			// 
			// LinePanel
			// 
			this.LinePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LinePanel.BackColor = System.Drawing.Color.LightGray;
			this.LinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
			this.LinePanel.Name = "LinePanel";
			this.LinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 1, true);
			this.LinePanel.TabIndex = 3;
			// 
			// ScreenedPartyControl
			// 
			this.ScreenedPartyControl.AllowDrop = true;
			this.ScreenedPartyControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScreenedPartyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 2, true);
			this.ScreenedPartyControl.Name = "ScreenedPartyControl";
			this.ScreenedPartyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 905, true);
			this.ScreenedPartyControl.TabIndex = 0;
			// 
			// DpsResultUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DpsResultTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "DpsResultUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1330, 910, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DpsResultTableLayoutPanel.ResumeLayout(false);
			this.DpsResultTableLayoutPanel.PerformLayout();
			this.ScreenedPartyLeftPanel.ResumeLayout(false);
			this.ScreenedPartyLeftPanel.PerformLayout();
			this.ScrollPanel.ResumeLayout(false);
			this.ScrollPanel.PerformLayout();
			this.TopBannerPanel.ResumeLayout(false);
			this.TopBannerPanel.PerformLayout();
			this.HeaderTextPanel.ResumeLayout(false);
			this.HeaderTextPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SwitchBarPictureBox)).EndInit();
			this.ScreenedPartyControl.ResumeLayout(true);
			this.ScreenedPartyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public CargoWise.Windows.UI.KTableLayoutPanel DpsResultTableLayoutPanel;
		public ScreenedPartyUserControl ScreenedPartyControl;
		public Enterprise.ZArchitecture.GUI.ZPanel ScreenedPartyLeftPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel TopBannerPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ScrollPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel ScreenedItemsPanel;
		private Enterprise.ZArchitecture.ZLabel ScreenedPartiesLabel;
		private ZArchitecture.GUI.ZPanel LinePanel;
		private ZArchitecture.GUI.ZPictureBox SwitchBarPictureBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel HeaderTextPanel;
		public Enterprise.ZArchitecture.ZLabel MatchesNumberLabel;
	}
}
