namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class SourceListNameItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BottomBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OpenSourceListLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BottomBorderPanel
			// 
			this.BottomBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BottomBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.BottomBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.BottomBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BottomBorderPanel.Name = "BottomBorderPanel";
			this.BottomBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 1, true);
			this.BottomBorderPanel.TabIndex = 1;
			// 
			// OpenSourceListLinkLabel
			// 
			this.OpenSourceListLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OpenSourceListLinkLabel.AutoEllipsis = true;
			this.OpenSourceListLinkLabel.IsFontBold = false;
			this.OpenSourceListLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.OpenSourceListLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.OpenSourceListLinkLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OpenSourceListLinkLabel.Name = "OpenSourceListLinkLabel";
			this.OpenSourceListLinkLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.OpenSourceListLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 14, true);
			this.OpenSourceListLinkLabel.TabIndex = 3;
			this.OpenSourceListLinkLabel.TabStop = false;
			this.OpenSourceListLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenSourceListLinkLabel_LinkClicked);
			// 
			// SourceListNameItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.OpenSourceListLinkLabel);
			this.Controls.Add(this.BottomBorderPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SourceListNameItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 30, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomBorderPanel;
		protected ZArchitecture.GUI.ZLinkLabel OpenSourceListLinkLabel;
	}
}
