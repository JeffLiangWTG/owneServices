namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ProfileNotesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProfileNotesTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderBoderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProfileNotesHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProfileNotesExpander = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ProfileNotesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProfileNotesContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProfileNoteContentRichTextBox = new Enterprise.DeniedPartyScreening.GUI.ProfileNotesUserControl.DpsRichTextBox();
			this.OpenProfileLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfileNotesTableLayoutPanel.SuspendLayout();
			this.HeaderBoderPanel.SuspendLayout();
			this.ProfileNotesHeaderPanel.SuspendLayout();
			this.ContentBorderPanel.SuspendLayout();
			this.ProfileNotesContentPanel.SuspendLayout();
			this.ProfileNoteContentRichTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ProfileNotesTableLayoutPanel
			// 
			this.ProfileNotesTableLayoutPanel.AutoSize = true;
			this.ProfileNotesTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ProfileNotesTableLayoutPanel.ColumnCount = 1;
			this.ProfileNotesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ProfileNotesTableLayoutPanel.Controls.Add(this.HeaderBoderPanel, 0, 0);
			this.ProfileNotesTableLayoutPanel.Controls.Add(this.ContentBorderPanel, 0, 1);
			this.ProfileNotesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfileNotesTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProfileNotesTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ProfileNotesTableLayoutPanel.Name = "ProfileNotesTableLayoutPanel";
			this.ProfileNotesTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 10, 16, 10, true);
			this.ProfileNotesTableLayoutPanel.RowCount = 2;
			this.ProfileNotesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(38)));
			this.ProfileNotesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ProfileNotesTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 287, true);
			this.ProfileNotesTableLayoutPanel.TabIndex = 0;
			// 
			// HeaderBoderPanel
			// 
			this.HeaderBoderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.HeaderBoderPanel.Controls.Add(this.ProfileNotesHeaderPanel);
			this.HeaderBoderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderBoderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 10, true);
			this.HeaderBoderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderBoderPanel.Name = "HeaderBoderPanel";
			this.HeaderBoderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.HeaderBoderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 38, true);
			this.HeaderBoderPanel.TabIndex = 4;
			// 
			// ProfileNotesHeaderPanel
			// 
			this.ProfileNotesHeaderPanel.BackColor = System.Drawing.Color.White;
			this.ProfileNotesHeaderPanel.Controls.Add(this.ProfileNotesExpander);
			this.ProfileNotesHeaderPanel.Controls.Add(this.ProfileNotesLabel);
			this.ProfileNotesHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfileNotesHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ProfileNotesHeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ProfileNotesHeaderPanel.Name = "ProfileNotesHeaderPanel";
			this.ProfileNotesHeaderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2, true);
			this.ProfileNotesHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 36, true);
			this.ProfileNotesHeaderPanel.TabIndex = 0;
			// 
			// ProfileNotesExpander
			// 
			this.ProfileNotesExpander.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileNotesExpander.AutoSize = true;
			this.ProfileNotesExpander.IsFontBold = false;
			this.ProfileNotesExpander.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 10, true);
			this.ProfileNotesExpander.Name = "ProfileNotesExpander";
			this.ProfileNotesExpander.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ProfileNotesExpander.TabIndex = 2;
			this.ProfileNotesExpander.Text = ">";
			// 
			// ProfileNotesLabel
			// 
			this.ProfileNotesLabel.AutoSize = true;
			this.ProfileNotesLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("d261041f-a8d2-402a-abc1-f572854ac55c", "Profile Notes");
			this.ProfileNotesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ProfileNotesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.ProfileNotesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 8, true);
			this.ProfileNotesLabel.Name = "ProfileNotesLabel";
			this.ProfileNotesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.ProfileNotesLabel.TabIndex = 0;
			this.ProfileNotesLabel.UseMnemonic = false;
			// 
			// ContentBorderPanel
			// 
			this.ContentBorderPanel.AutoSize = true;
			this.ContentBorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.ContentBorderPanel.Controls.Add(this.ProfileNotesContentPanel);
			this.ContentBorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.ContentBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContentBorderPanel.Name = "ContentBorderPanel";
			this.ContentBorderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 1, true);
			this.ContentBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 229, true);
			this.ContentBorderPanel.TabIndex = 3;
			// 
			// ProfileNotesContentPanel
			// 
			this.ProfileNotesContentPanel.AutoSize = true;
			this.ProfileNotesContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ProfileNotesContentPanel.BackColor = System.Drawing.Color.White;
			this.ProfileNotesContentPanel.Controls.Add(this.ProfileNoteContentRichTextBox);
			this.ProfileNotesContentPanel.Controls.Add(this.OpenProfileLinkLabel);
			this.ProfileNotesContentPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProfileNotesContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.ProfileNotesContentPanel.Name = "ProfileNotesContentPanel";
			this.ProfileNotesContentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(9, 1, 9, 24, true);
			this.ProfileNotesContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 228, true);
			this.ProfileNotesContentPanel.TabIndex = 1;
			// 
			// ProfileNoteContentRichTextBox
			// 
			this.ProfileNoteContentRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileNoteContentRichTextBox.BackColor = System.Drawing.Color.White;
			this.ProfileNoteContentRichTextBox.IsToolBarVisible = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProfileNoteContentRichTextBox, false);
			this.ProfileNoteContentRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 1, true);
			this.ProfileNoteContentRichTextBox.MaxLength = 10000000;
			this.ProfileNoteContentRichTextBox.Name = "ProfileNoteContentRichTextBox";
			this.ProfileNoteContentRichTextBox.ReadOnly = true;
			this.ProfileNoteContentRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 200, true);
			this.ProfileNoteContentRichTextBox.TabIndex = 3;
			// 
			// OpenProfileLinkLabel
			// 
			this.OpenProfileLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OpenProfileLinkLabel.IsFontBold = false;
			this.OpenProfileLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(112)))), ((int)(((byte)(154)))));
			this.OpenProfileLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 208, true);
			this.OpenProfileLinkLabel.Name = "OpenProfileLinkLabel";
			this.OpenProfileLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 14, true);
			this.OpenProfileLinkLabel.TabIndex = 2;
			this.OpenProfileLinkLabel.TabStop = false;
			this.OpenProfileLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenProfileLinkLabel_LinkClicked);
			// 
			// ProfileNotesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProfileNotesTableLayoutPanel);
			this.Name = "ProfileNotesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 287, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfileNotesTableLayoutPanel.ResumeLayout(false);
			this.ProfileNotesTableLayoutPanel.PerformLayout();
			this.HeaderBoderPanel.ResumeLayout(false);
			this.HeaderBoderPanel.PerformLayout();
			this.ProfileNotesHeaderPanel.ResumeLayout(false);
			this.ProfileNotesHeaderPanel.PerformLayout();
			this.ContentBorderPanel.ResumeLayout(false);
			this.ContentBorderPanel.PerformLayout();
			this.ProfileNotesContentPanel.ResumeLayout(false);
			this.ProfileNotesContentPanel.PerformLayout();
			this.ProfileNoteContentRichTextBox.ResumeLayout(true);
			this.ProfileNoteContentRichTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel ProfileNotesTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel ProfileNotesHeaderPanel;
		private ZArchitecture.ZLabel ProfileNotesLabel;
		private ZArchitecture.GUI.ZPanel ProfileNotesContentPanel;
		private ZArchitecture.GUI.ZLinkLabel ProfileNotesExpander;
		protected ZArchitecture.GUI.ZLinkLabel OpenProfileLinkLabel;
		internal DpsRichTextBox ProfileNoteContentRichTextBox;
		private ZArchitecture.GUI.ZPanel ContentBorderPanel;
		private ZArchitecture.GUI.ZPanel HeaderBoderPanel;
	}
}
