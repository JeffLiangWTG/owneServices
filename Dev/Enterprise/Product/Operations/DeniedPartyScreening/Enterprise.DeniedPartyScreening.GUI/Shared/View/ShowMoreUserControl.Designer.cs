namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ShowMoreUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SwitchBarPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.VerticalScreeningPartiesNumberLabel = new Enterprise.DeniedPartyScreening.GUI.VerticalLabel();
			this.LinePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VerticalScreenedPartiesLabel = new Enterprise.DeniedPartyScreening.GUI.VerticalLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SwitchBarPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// SwitchBarPictureBox
			// 
			this.SwitchBarPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.SwitchBarPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 6, true);
			this.SwitchBarPictureBox.Name = "SwitchBarPictureBox";
			this.SwitchBarPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 24, true);
			this.SwitchBarPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.SwitchBarPictureBox.TabIndex = 0;
			this.SwitchBarPictureBox.TabStop = false;
			// 
			// VerticalScreeningPartiesNumberLabel
			// 
			this.VerticalScreeningPartiesNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.VerticalScreeningPartiesNumberLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.VerticalScreeningPartiesNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 47, true);
			this.VerticalScreeningPartiesNumberLabel.Name = "VerticalScreeningPartiesNumberLabel";
			this.VerticalScreeningPartiesNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 30, true);
			this.VerticalScreeningPartiesNumberLabel.TabIndex = 1;
			this.VerticalScreeningPartiesNumberLabel.UseMnemonic = false;
			// 
			// LinePanel
			// 
			this.LinePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LinePanel.BackColor = System.Drawing.Color.LightGray;
			this.LinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.LinePanel.Name = "LinePanel";
			this.LinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 1, true);
			this.LinePanel.TabIndex = 4;
			// 
			// VerticalScreenedPartiesLabel
			// 
			this.VerticalScreenedPartiesLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
			this.VerticalScreenedPartiesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.VerticalScreenedPartiesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 78, true);
			this.VerticalScreenedPartiesLabel.Name = "VerticalScreenedPartiesLabel";
			this.VerticalScreenedPartiesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 202, true);
			this.VerticalScreenedPartiesLabel.TabIndex = 5;
			this.VerticalScreenedPartiesLabel.UseMnemonic = false;
			// 
			// ShowMoreUserControl
			// 
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VerticalScreenedPartiesLabel);
			this.Controls.Add(this.LinePanel);
			this.Controls.Add(this.VerticalScreeningPartiesNumberLabel);
			this.Controls.Add(this.SwitchBarPictureBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "ShowMoreUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 294, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SwitchBarPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZPictureBox SwitchBarPictureBox;
		public VerticalLabel VerticalScreeningPartiesNumberLabel;
		private ZArchitecture.GUI.ZPanel LinePanel;
		public VerticalLabel VerticalScreenedPartiesLabel;
	}
}
