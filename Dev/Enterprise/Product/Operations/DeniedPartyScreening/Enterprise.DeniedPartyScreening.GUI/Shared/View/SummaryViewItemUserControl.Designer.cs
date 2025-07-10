namespace Enterprise.DeniedPartyScreening.GUI.Shared.View
{
	partial class SummaryViewItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.nameLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.statusLabel = new Enterprise.ZArchitecture.ZLabel();
            this.linePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.SuspendLayout();
            // 
            // nameLinkLabel
            // 
            this.nameLinkLabel.AutoSize = true;
            this.nameLinkLabel.IsFontBold = false;
            this.nameLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(112)))), ((int)(((byte)(154)))));
            this.nameLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 6, true);
            this.nameLinkLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 0, true);
            this.nameLinkLabel.Name = "nameLinkLabel";
            this.nameLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 16, true);
            this.nameLinkLabel.TabIndex = 0;
            this.nameLinkLabel.Text = "linkLabel1";
            this.nameLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.nameLinkLabel_LinkClicked);
            this.nameLinkLabel.SizeChanged += new System.EventHandler(this.nameLinkLabel_SizeChanged);
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.statusLabel.ForeColor = System.Drawing.Color.Green;
            this.statusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 6, true);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 16, true);
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "label1";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // linePanel
            // 
            this.linePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linePanel.BackColor = System.Drawing.Color.LightGray;
            this.linePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 31, true);
            this.linePanel.Name = "linePanel";
            this.linePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 1, true);
            this.linePanel.TabIndex = 2;
            // 
            // SummaryViewItemUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.linePanel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.nameLinkLabel);
            this.Name = "SummaryViewItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 32, true);
            this.SizeChanged += new System.EventHandler(this.SummaryViewItemUserControl_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZLinkLabel nameLinkLabel;
		private Enterprise.ZArchitecture.ZLabel statusLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel linePanel;
	}
}
