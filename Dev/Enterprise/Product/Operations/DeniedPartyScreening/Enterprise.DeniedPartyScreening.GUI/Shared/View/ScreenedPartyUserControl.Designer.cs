namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ScreenedPartyUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TopBannerTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ScreeningStatusControl = new Enterprise.DeniedPartyScreening.GUI.ScreeningStatusUserControl();
			this.TopBannerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntityIconAndNamePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntityIcon = new CargoWise.Windows.UI.KPictureBox();
			this.PartyName = new Enterprise.ZArchitecture.ZLabel();
			this.ParentsDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NavigationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.NavigationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MiddleTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.PotentialMatchControl = new Enterprise.DeniedPartyScreening.GUI.PotentialMatchUserControl();
			this.LeftListPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PotentialMatchItemsPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MiddleHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PotentialMatchesInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PotentialMatchesCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PotentialMatchesLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayoutPanel.SuspendLayout();
			this.TopBannerTableLayoutPanel.SuspendLayout();
			this.ScreeningStatusControl.SuspendLayout();
			this.TopBannerPanel.SuspendLayout();
			this.EntityIconAndNamePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntityIcon)).BeginInit();
			this.NavigationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NextButton)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousButton)).BeginInit();
			this.MiddleTableLayoutPanel.SuspendLayout();
			this.PotentialMatchControl.SuspendLayout();
			this.LeftListPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.MiddleHeaderPanel.SuspendLayout();
			this.PotentialMatchesInfoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTableLayoutPanel
			// 
			this.MainTableLayoutPanel.ColumnCount = 1;
			this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Controls.Add(this.TopBannerTableLayoutPanel, 0, 0);
			this.MainTableLayoutPanel.Controls.Add(this.MiddleTableLayoutPanel, 0, 2);
			this.MainTableLayoutPanel.Controls.Add(this.BottomPanel, 0, 3);
			this.MainTableLayoutPanel.Controls.Add(this.MiddleHeaderPanel, 0, 1);
			this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.RowCount = 4;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(109)));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(34)));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40)));
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1601, 984, true);
			this.MainTableLayoutPanel.TabIndex = 0;
			// 
			// TopBannerTableLayoutPanel
			// 
			this.TopBannerTableLayoutPanel.ColumnCount = 2;
			this.TopBannerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TopBannerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(558)));
			this.TopBannerTableLayoutPanel.Controls.Add(this.ScreeningStatusControl, 1, 0);
			this.TopBannerTableLayoutPanel.Controls.Add(this.TopBannerPanel, 0, 0);
			this.TopBannerTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopBannerTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.TopBannerTableLayoutPanel.Name = "TopBannerTableLayoutPanel";
			this.TopBannerTableLayoutPanel.RowCount = 1;
			this.TopBannerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TopBannerTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1596, 104, true);
			this.TopBannerTableLayoutPanel.TabIndex = 0;
			// 
			// ScreeningStatusControl
			// 
			this.ScreeningStatusControl.AllowDrop = true;
			this.ScreeningStatusControl.AutoSize = true;
			this.ScreeningStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScreeningStatusControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1041, 2, true);
			this.ScreeningStatusControl.Name = "ScreeningStatusControl";
			this.ScreeningStatusControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 99, true);
			this.ScreeningStatusControl.TabIndex = 0;
			// 
			// TopBannerPanel
			// 
			this.TopBannerPanel.Controls.Add(this.EntityIconAndNamePanel);
			this.TopBannerPanel.Controls.Add(this.NavigationPanel);
			this.TopBannerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopBannerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.TopBannerPanel.Name = "TopBannerPanel";
			this.TopBannerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.TopBannerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 99, true);
			this.TopBannerPanel.TabIndex = 1;
			// 
			// EntityIconAndNamePanel
			// 
			this.EntityIconAndNamePanel.Controls.Add(this.EntityIcon);
			this.EntityIconAndNamePanel.Controls.Add(this.PartyName);
			this.EntityIconAndNamePanel.Controls.Add(this.ParentsDescriptionLabel);
			this.EntityIconAndNamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntityIconAndNamePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.EntityIconAndNamePanel.Name = "EntityIconAndNamePanel";
			this.EntityIconAndNamePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 74, true);
			this.EntityIconAndNamePanel.TabIndex = 0;
			// 
			// EntityIcon
			// 
			this.EntityIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.EntityIcon.Name = "EntityIcon";
			this.EntityIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.EntityIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.EntityIcon.TabIndex = 0;
			this.EntityIcon.TabStop = false;
			// 
			// PartyName
			// 
			this.PartyName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PartyName.AutoEllipsis = true;
			this.PartyName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PartyName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.PartyName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 4, true);
			this.PartyName.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PartyName.Name = "PartyName";
			this.PartyName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 23, true);
			this.PartyName.TabIndex = 1;
			this.PartyName.UseMnemonic = false;
			// 
			// ParentsDescriptionLabel
			// 
			this.ParentsDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ParentsDescriptionLabel.AutoEllipsis = true;
			this.ParentsDescriptionLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.ParentsDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.ParentsDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 38, true);
			this.ParentsDescriptionLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ParentsDescriptionLabel.Name = "ParentsDescriptionLabel";
			this.ParentsDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 23, true);
			this.ParentsDescriptionLabel.TabIndex = 2;
			this.ParentsDescriptionLabel.UseMnemonic = false;
			// 
			// NavigationPanel
			// 
			this.NavigationPanel.Controls.Add(this.NextButton);
			this.NavigationPanel.Controls.Add(this.PreviousButton);
			this.NavigationPanel.Controls.Add(this.NavigationLabel);
			this.NavigationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.NavigationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.NavigationPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.NavigationPanel.Name = "NavigationPanel";
			this.NavigationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 22, true);
			this.NavigationPanel.TabIndex = 3;
			// 
			// NextButton
			// 
			this.NextButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.NextButton.Image = global::Enterprise.DeniedPartyScreening.GUI.Properties.Resources.arrowDrawingGroupRight;
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 3, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 16, true);
			this.NextButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.NextButton.TabIndex = 4;
			this.NextButton.TabStop = false;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// PreviousButton
			// 
			this.PreviousButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.PreviousButton.Image = global::Enterprise.DeniedPartyScreening.GUI.Properties.Resources.arrowDrawingGroupLeft;
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 3, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.PreviousButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.PreviousButton.TabIndex = 3;
			this.PreviousButton.TabStop = false;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// NavigationLabel
			// 
			this.NavigationLabel.AutoEllipsis = true;
			this.NavigationLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.NavigationLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.NavigationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true);
			this.NavigationLabel.Name = "NavigationLabel";
			this.NavigationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.NavigationLabel.TabIndex = 2;
			this.NavigationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NavigationLabel.UseMnemonic = false;
			// 
			// MiddleTableLayoutPanel
			// 
			this.MiddleTableLayoutPanel.ColumnCount = 2;
			this.MiddleTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(415)));
			this.MiddleTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MiddleTableLayoutPanel.Controls.Add(this.PotentialMatchControl, 1, 0);
			this.MiddleTableLayoutPanel.Controls.Add(this.LeftListPanel, 0, 0);
			this.MiddleTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 144, true);
			this.MiddleTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.MiddleTableLayoutPanel.Name = "MiddleTableLayoutPanel";
			this.MiddleTableLayoutPanel.RowCount = 1;
			this.MiddleTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MiddleTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1598, 798, true);
			this.MiddleTableLayoutPanel.TabIndex = 1;
			// 
			// PotentialMatchControl
			// 
			this.PotentialMatchControl.AllowDrop = true;
			this.PotentialMatchControl.BackColor = System.Drawing.Color.White;
			this.PotentialMatchControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PotentialMatchControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 0, true);
			this.PotentialMatchControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.PotentialMatchControl.Name = "PotentialMatchControl";
			this.PotentialMatchControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1181, 798, true);
			this.PotentialMatchControl.TabIndex = 0;
			// 
			// LeftListPanel
			// 
			this.LeftListPanel.AutoScroll = true;
			this.LeftListPanel.BackColor = System.Drawing.Color.White;
			this.LeftListPanel.Controls.Add(this.PotentialMatchItemsPanel);
			this.LeftListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftListPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 2, 0, true);
			this.LeftListPanel.Name = "LeftListPanel";
			this.LeftListPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 798, true);
			this.LeftListPanel.TabIndex = 1;
			// 
			// PotentialMatchItemsPanel
			// 
			this.PotentialMatchItemsPanel.AutoSize = true;
			this.PotentialMatchItemsPanel.ColumnCount = 1;
			this.PotentialMatchItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.PotentialMatchItemsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PotentialMatchItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialMatchItemsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchItemsPanel.Name = "PotentialMatchItemsPanel";
			this.PotentialMatchItemsPanel.RowCount = 1;
			this.PotentialMatchItemsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.PotentialMatchItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 0, true);
			this.PotentialMatchItemsPanel.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.SaveButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 946, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1596, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(170)))), ((int)(((byte)(169)))));
			this.SaveButton.IsCaptionOverridden = true;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1497, 1, true);
			this.SaveButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 32, true);
			this.SaveButton.TabIndex = 5;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = false;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// MiddleHeaderPanel
			// 
			this.MiddleHeaderPanel.BackColor = System.Drawing.Color.White;
			this.MiddleHeaderPanel.Controls.Add(this.PotentialMatchesInfoPanel);
			this.MiddleHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 110, true);
			this.MiddleHeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.MiddleHeaderPanel.Name = "MiddleHeaderPanel";
			this.MiddleHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1598, 30, true);
			this.MiddleHeaderPanel.TabIndex = 3;
			// 
			// PotentialMatchesInfoPanel
			// 
			this.PotentialMatchesInfoPanel.Controls.Add(this.PotentialMatchesCountLabel);
			this.PotentialMatchesInfoPanel.Controls.Add(this.PotentialMatchesLabel);
			this.PotentialMatchesInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PotentialMatchesInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialMatchesInfoPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchesInfoPanel.Name = "PotentialMatchesInfoPanel";
			this.PotentialMatchesInfoPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.PotentialMatchesInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1598, 30, true);
			this.PotentialMatchesInfoPanel.TabIndex = 4;
			// 
			// PotentialMatchesCountLabel
			// 
			this.PotentialMatchesCountLabel.AutoEllipsis = true;
			this.PotentialMatchesCountLabel.AutoSize = true;
			this.PotentialMatchesCountLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.PotentialMatchesCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PotentialMatchesCountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.PotentialMatchesCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.PotentialMatchesCountLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchesCountLabel.Name = "PotentialMatchesCountLabel";
			this.PotentialMatchesCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 17, true);
			this.PotentialMatchesCountLabel.TabIndex = 3;
			this.PotentialMatchesCountLabel.UseMnemonic = false;
			// 
			// PotentialMatchesLabel
			// 
			this.PotentialMatchesLabel.AutoSize = true;
			this.PotentialMatchesLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.PotentialMatchesLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.PotentialMatchesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.PotentialMatchesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.PotentialMatchesLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchesLabel.Name = "PotentialMatchesLabel";
			this.PotentialMatchesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 17, true);
			this.PotentialMatchesLabel.TabIndex = 2;
			this.PotentialMatchesLabel.UseMnemonic = false;
			// 
			// ScreenedPartyUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayoutPanel);
			this.Name = "ScreenedPartyUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1601, 984, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.TopBannerTableLayoutPanel.ResumeLayout(false);
			this.TopBannerTableLayoutPanel.PerformLayout();
			this.ScreeningStatusControl.ResumeLayout(true);
			this.ScreeningStatusControl.PerformLayout();
			this.TopBannerPanel.ResumeLayout(false);
			this.TopBannerPanel.PerformLayout();
			this.EntityIconAndNamePanel.ResumeLayout(false);
			this.EntityIconAndNamePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntityIcon)).EndInit();
			this.NavigationPanel.ResumeLayout(false);
			this.NavigationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NextButton)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousButton)).EndInit();
			this.MiddleTableLayoutPanel.ResumeLayout(false);
			this.MiddleTableLayoutPanel.PerformLayout();
			this.PotentialMatchControl.ResumeLayout(true);
			this.PotentialMatchControl.PerformLayout();
			this.LeftListPanel.ResumeLayout(false);
			this.LeftListPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MiddleHeaderPanel.ResumeLayout(false);
			this.MiddleHeaderPanel.PerformLayout();
			this.PotentialMatchesInfoPanel.ResumeLayout(false);
			this.PotentialMatchesInfoPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel TopBannerTableLayoutPanel;
		private ScreeningStatusUserControl ScreeningStatusControl;
		private CargoWise.Windows.UI.KTableLayoutPanel MiddleTableLayoutPanel;
		private PotentialMatchUserControl PotentialMatchControl;
		public ZArchitecture.GUI.ZButton SaveButton;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZPanel LeftListPanel;
		private ZArchitecture.GUI.ZPanel MiddleHeaderPanel;
		public CargoWise.Windows.UI.KTableLayoutPanel PotentialMatchItemsPanel;
		private ZArchitecture.GUI.ZPanel EntityIconAndNamePanel;
		public ZArchitecture.ZLabel PartyName;
		private CargoWise.Windows.UI.KPictureBox EntityIcon;
		public ZArchitecture.ZLabel ParentsDescriptionLabel;
		public ZArchitecture.GUI.ZPanel NavigationPanel;
		public ZArchitecture.ZLabel NavigationLabel;
		public ZArchitecture.GUI.ZPictureBox NextButton;
		public ZArchitecture.GUI.ZPictureBox PreviousButton;
		private ZArchitecture.GUI.ZPanel TopBannerPanel;
		public ZArchitecture.ZLabel PotentialMatchesLabel;
		public ZArchitecture.ZLabel PotentialMatchesCountLabel;
		private ZArchitecture.GUI.ZPanel PotentialMatchesInfoPanel;
	}
}
