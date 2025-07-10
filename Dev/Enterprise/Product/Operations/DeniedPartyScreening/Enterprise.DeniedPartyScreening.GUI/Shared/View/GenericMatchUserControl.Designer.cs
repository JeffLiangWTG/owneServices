namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class GenericMatchUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GenericMatchTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GenericMatchStackPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProfileContentExpander = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ExpanderDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExpanderTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GenericMatchContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShowMoreUserControl = new Enterprise.DeniedPartyScreening.GUI.GenericMatchShowMoreUserControl();
			this.MatchedItemsListPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GenericMatchTableLayoutPanel.SuspendLayout();
			this.HeaderBorderPanel.SuspendLayout();
			this.GenericMatchStackPanel.SuspendLayout();
			this.ContentBorderPanel.SuspendLayout();
			this.GenericMatchContentPanel.SuspendLayout();
			this.ShowMoreUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// GenericMatchTableLayoutPanel
			// 
			this.GenericMatchTableLayoutPanel.AutoSize = true;
			this.GenericMatchTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GenericMatchTableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			this.GenericMatchTableLayoutPanel.ColumnCount = 1;
			this.GenericMatchTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.GenericMatchTableLayoutPanel.Controls.Add(this.HeaderBorderPanel, 0, 0);
			this.GenericMatchTableLayoutPanel.Controls.Add(this.ContentBorderPanel, 0, 1);
			this.GenericMatchTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenericMatchTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GenericMatchTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GenericMatchTableLayoutPanel.Name = "GenericMatchTableLayoutPanel";
			this.GenericMatchTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 10, 16, 10, true);
			this.GenericMatchTableLayoutPanel.RowCount = 2;
			this.GenericMatchTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(38)));
			this.GenericMatchTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.GenericMatchTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 142, true);
			this.GenericMatchTableLayoutPanel.TabIndex = 0;
			// 
			// HeaderBorderPanel
			// 
			this.HeaderBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.HeaderBorderPanel.Controls.Add(this.GenericMatchStackPanel);
			this.HeaderBorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 10, true);
			this.HeaderBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderBorderPanel.Name = "HeaderBorderPanel";
			this.HeaderBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.HeaderBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 38, true);
			this.HeaderBorderPanel.TabIndex = 3;
			// 
			// GenericMatchStackPanel
			// 
			this.GenericMatchStackPanel.BackColor = System.Drawing.Color.White;
			this.GenericMatchStackPanel.Controls.Add(this.ProfileContentExpander);
			this.GenericMatchStackPanel.Controls.Add(this.ExpanderDescriptionLabel);
			this.GenericMatchStackPanel.Controls.Add(this.ExpanderTitleLabel);
			this.GenericMatchStackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenericMatchStackPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.GenericMatchStackPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GenericMatchStackPanel.Name = "GenericMatchStackPanel";
			this.GenericMatchStackPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 36, true);
			this.GenericMatchStackPanel.TabIndex = 0;
			// 
			// ProfileContentExpander
			// 
			this.ProfileContentExpander.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileContentExpander.AutoSize = true;
			this.ProfileContentExpander.IsFontBold = false;
			this.ProfileContentExpander.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 10, true);
			this.ProfileContentExpander.Name = "ProfileContentExpander";
			this.ProfileContentExpander.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ProfileContentExpander.TabIndex = 2;
			this.ProfileContentExpander.Text = ">";
			// 
			// ExpanderDescriptionLabel
			// 
			this.ExpanderDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpanderDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExpanderDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 4, true);
			this.ExpanderDescriptionLabel.Name = "ExpanderDescriptionLabel";
			this.ExpanderDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 26, true);
			this.ExpanderDescriptionLabel.TabIndex = 1;
			this.ExpanderDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ExpanderDescriptionLabel.UseMnemonic = false;
			// 
			// ExpanderTitleLabel
			// 
			this.ExpanderTitleLabel.AutoSize = true;
			this.ExpanderTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ExpanderTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.ExpanderTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 8, true);
			this.ExpanderTitleLabel.Name = "ExpanderTitleLabel";
			this.ExpanderTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 17, true);
			this.ExpanderTitleLabel.TabIndex = 0;
			this.ExpanderTitleLabel.UseMnemonic = false;
			// 
			// ContentBorderPanel
			// 
			this.ContentBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentBorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.ContentBorderPanel.Controls.Add(this.GenericMatchContentPanel);
			this.ContentBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 47, true);
			this.ContentBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContentBorderPanel.Name = "ContentBorderPanel";
			this.ContentBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 1, true);
			this.ContentBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 86, true);
			this.ContentBorderPanel.TabIndex = 3;
			// 
			// GenericMatchContentPanel
			// 
			this.GenericMatchContentPanel.AutoSize = true;
			this.GenericMatchContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GenericMatchContentPanel.BackColor = System.Drawing.Color.White;
			this.GenericMatchContentPanel.Controls.Add(this.ShowMoreUserControl);
			this.GenericMatchContentPanel.Controls.Add(this.MatchedItemsListPanel);
			this.GenericMatchContentPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.GenericMatchContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.GenericMatchContentPanel.Name = "GenericMatchContentPanel";
			this.GenericMatchContentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.GenericMatchContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 45, true);
			this.GenericMatchContentPanel.TabIndex = 1;
			// 
			// ShowMoreUserControl
			// 
			this.ShowMoreUserControl.AllowDrop = true;
			this.ShowMoreUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ShowMoreUserControl.BackColor = System.Drawing.Color.White;
			this.ShowMoreUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShowMoreUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.ShowMoreUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ShowMoreUserControl.Name = "ShowMoreUserControl";
			this.ShowMoreUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 26, true);
			this.ShowMoreUserControl.TabIndex = 2;
			// 
			// MatchedItemsListPanel
			// 
			this.MatchedItemsListPanel.AutoSize = true;
			this.MatchedItemsListPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MatchedItemsListPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MatchedItemsListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.MatchedItemsListPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MatchedItemsListPanel.Name = "MatchedItemsListPanel";
			this.MatchedItemsListPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 0, true);
			this.MatchedItemsListPanel.RowCount = 1;
			this.MatchedItemsListPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MatchedItemsListPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 0, true);
			this.MatchedItemsListPanel.TabIndex = 1;
			// 
			// GenericMatchUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GenericMatchTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 2, 5, 2, true);
			this.Name = "GenericMatchUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 142, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GenericMatchTableLayoutPanel.ResumeLayout(false);
			this.GenericMatchTableLayoutPanel.PerformLayout();
			this.HeaderBorderPanel.ResumeLayout(false);
			this.HeaderBorderPanel.PerformLayout();
			this.GenericMatchStackPanel.ResumeLayout(false);
			this.GenericMatchStackPanel.PerformLayout();
			this.ContentBorderPanel.ResumeLayout(false);
			this.ContentBorderPanel.PerformLayout();
			this.GenericMatchContentPanel.ResumeLayout(false);
			this.GenericMatchContentPanel.PerformLayout();
			this.ShowMoreUserControl.ResumeLayout(true);
			this.ShowMoreUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel GenericMatchTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel GenericMatchStackPanel;
		private ZArchitecture.ZLabel ExpanderDescriptionLabel;
		private ZArchitecture.ZLabel ExpanderTitleLabel;
		private ZArchitecture.GUI.ZPanel GenericMatchContentPanel;
		private ZArchitecture.GUI.ZLinkLabel ProfileContentExpander;
		private CargoWise.Windows.UI.KTableLayoutPanel MatchedItemsListPanel;
		private GenericMatchShowMoreUserControl ShowMoreUserControl;
		private ZArchitecture.GUI.ZPanel HeaderBorderPanel;
		private ZArchitecture.GUI.ZPanel ContentBorderPanel;
	}
}
