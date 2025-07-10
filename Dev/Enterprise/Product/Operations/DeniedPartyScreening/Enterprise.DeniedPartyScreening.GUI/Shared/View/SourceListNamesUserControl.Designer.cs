namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class SourceListNamesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SourceListTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SourceListHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SourceListNamesExpander = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SourceListNamesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SourceListContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShowMoreUserControl = new Enterprise.DeniedPartyScreening.GUI.GenericMatchShowMoreUserControl();
			this.SourceListItemsPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SourceListTableLayoutPanel.SuspendLayout();
			this.HeaderBorderPanel.SuspendLayout();
			this.SourceListHeaderPanel.SuspendLayout();
			this.ContentBorderPanel.SuspendLayout();
			this.SourceListContentPanel.SuspendLayout();
			this.ShowMoreUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SourceListTableLayoutPanel
			// 
			this.SourceListTableLayoutPanel.AutoSize = true;
			this.SourceListTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SourceListTableLayoutPanel.ColumnCount = 1;
			this.SourceListTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.SourceListTableLayoutPanel.Controls.Add(this.HeaderBorderPanel, 0, 0);
			this.SourceListTableLayoutPanel.Controls.Add(this.ContentBorderPanel, 0, 1);
			this.SourceListTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SourceListTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.SourceListTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SourceListTableLayoutPanel.Name = "SourceListTableLayoutPanel";
			this.SourceListTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 10, 16, 10, true);
			this.SourceListTableLayoutPanel.RowCount = 2;
			this.SourceListTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(38)));
			this.SourceListTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.SourceListTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 168, true);
			this.SourceListTableLayoutPanel.TabIndex = 0;
			// 
			// HeaderBorderPanel
			// 
			this.HeaderBorderPanel.AutoSize = true;
			this.HeaderBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.HeaderBorderPanel.Controls.Add(this.SourceListHeaderPanel);
			this.HeaderBorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 10, true);
			this.HeaderBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderBorderPanel.Name = "HeaderBorderPanel";
			this.HeaderBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.HeaderBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 38, true);
			this.HeaderBorderPanel.TabIndex = 4;
			// 
			// SourceListHeaderPanel
			// 
			this.SourceListHeaderPanel.AutoSize = true;
			this.SourceListHeaderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SourceListHeaderPanel.BackColor = System.Drawing.Color.White;
			this.SourceListHeaderPanel.Controls.Add(this.SourceListNamesExpander);
			this.SourceListHeaderPanel.Controls.Add(this.SourceListNamesLabel);
			this.SourceListHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SourceListHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.SourceListHeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(80, 0, 0, 0, true);
			this.SourceListHeaderPanel.Name = "SourceListHeaderPanel";
			this.SourceListHeaderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2, true);
			this.SourceListHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 36, true);
			this.SourceListHeaderPanel.TabIndex = 0;
			// 
			// SourceListNamesExpander
			// 
			this.SourceListNamesExpander.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceListNamesExpander.AutoSize = true;
			this.SourceListNamesExpander.IsFontBold = false;
			this.SourceListNamesExpander.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 10, true);
			this.SourceListNamesExpander.Name = "SourceListNamesExpander";
			this.SourceListNamesExpander.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.SourceListNamesExpander.TabIndex = 2;
			this.SourceListNamesExpander.Text = ">";
			// 
			// SourceListNamesLabel
			// 
			this.SourceListNamesLabel.AutoSize = true;
			this.SourceListNamesLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("539C44D9-817A-461A-8E40-51F5CD4EC193", "Source List Names");
			this.SourceListNamesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SourceListNamesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.SourceListNamesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 8, true);
			this.SourceListNamesLabel.Name = "SourceListNamesLabel";
			this.SourceListNamesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.SourceListNamesLabel.TabIndex = 0;
			this.SourceListNamesLabel.UseMnemonic = false;
			// 
			// ContentBorderPanel
			// 
			this.ContentBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentBorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.ContentBorderPanel.Controls.Add(this.SourceListContentPanel);
			this.ContentBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.ContentBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContentBorderPanel.Name = "ContentBorderPanel";
			this.ContentBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 1, true);
			this.ContentBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 110, true);
			this.ContentBorderPanel.TabIndex = 3;
			// 
			// SourceListContentPanel
			// 
			this.SourceListContentPanel.AutoSize = true;
			this.SourceListContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SourceListContentPanel.BackColor = System.Drawing.Color.White;
			this.SourceListContentPanel.Controls.Add(this.ShowMoreUserControl);
			this.SourceListContentPanel.Controls.Add(this.SourceListItemsPanel);
			this.SourceListContentPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourceListContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.SourceListContentPanel.Name = "SourceListContentPanel";
			this.SourceListContentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.SourceListContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 42, true);
			this.SourceListContentPanel.TabIndex = 1;
			// 
			// ShowMoreUserControl
			// 
			this.ShowMoreUserControl.AllowDrop = true;
			this.ShowMoreUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ShowMoreUserControl.BackColor = System.Drawing.Color.White;
			this.ShowMoreUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShowMoreUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ShowMoreUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ShowMoreUserControl.Name = "ShowMoreUserControl";
			this.ShowMoreUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 26, true);
			this.ShowMoreUserControl.TabIndex = 3;
			// 
			// SourceListItemsPanel
			// 
			this.SourceListItemsPanel.AutoSize = true;
			this.SourceListItemsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SourceListItemsPanel.ColumnCount = 1;
			this.SourceListItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.SourceListItemsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourceListItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.SourceListItemsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SourceListItemsPanel.Name = "SourceListItemsPanel";
			this.SourceListItemsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 0, true);
			this.SourceListItemsPanel.RowCount = 1;
			this.SourceListItemsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.SourceListItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 0, true);
			this.SourceListItemsPanel.TabIndex = 2;
			// 
			// SourceListNamesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SourceListTableLayoutPanel);
			this.Name = "SourceListNamesUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SourceListTableLayoutPanel.ResumeLayout(false);
			this.SourceListTableLayoutPanel.PerformLayout();
			this.HeaderBorderPanel.ResumeLayout(false);
			this.HeaderBorderPanel.PerformLayout();
			this.SourceListHeaderPanel.ResumeLayout(false);
			this.SourceListHeaderPanel.PerformLayout();
			this.ContentBorderPanel.ResumeLayout(false);
			this.ContentBorderPanel.PerformLayout();
			this.SourceListContentPanel.ResumeLayout(false);
			this.SourceListContentPanel.PerformLayout();
			this.ShowMoreUserControl.ResumeLayout(true);
			this.ShowMoreUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel SourceListTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel SourceListHeaderPanel;
		private ZArchitecture.ZLabel SourceListNamesLabel;
		private ZArchitecture.GUI.ZPanel SourceListContentPanel;
		private ZArchitecture.GUI.ZLinkLabel SourceListNamesExpander;
		private CargoWise.Windows.UI.KTableLayoutPanel SourceListItemsPanel;
		private GenericMatchShowMoreUserControl ShowMoreUserControl;
		private ZArchitecture.GUI.ZPanel ContentBorderPanel;
		private ZArchitecture.GUI.ZPanel HeaderBorderPanel;
	}
}
