namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class GenericMatchHeaderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ScreenedPartyHeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeniedPartyHeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PotentialMatchPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PotentialMatchPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ScreenedPartyHeaderLabel
			// 
			this.ScreenedPartyHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ScreenedPartyHeaderLabel.AutoEllipsis = true;
			this.ScreenedPartyHeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ScreenedPartyHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.ScreenedPartyHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.ScreenedPartyHeaderLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 5, 2, 2, true);
			this.ScreenedPartyHeaderLabel.Name = "ScreenedPartyHeaderLabel";
			this.ScreenedPartyHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 14, true);
			this.ScreenedPartyHeaderLabel.TabIndex = 1;
			this.ScreenedPartyHeaderLabel.UseMnemonic = false;
			// 
			// DeniedPartyHeaderLabel
			// 
			this.DeniedPartyHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeniedPartyHeaderLabel.AutoEllipsis = true;
			this.DeniedPartyHeaderLabel.BackColor = System.Drawing.Color.Transparent;
			this.DeniedPartyHeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeniedPartyHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.DeniedPartyHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 5, true);
			this.DeniedPartyHeaderLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 5, 2, 2, true);
			this.DeniedPartyHeaderLabel.Name = "DeniedPartyHeaderLabel";
			this.DeniedPartyHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 14, true);
			this.DeniedPartyHeaderLabel.TabIndex = 2;
			this.DeniedPartyHeaderLabel.UseMnemonic = false;
			// 
			// PotentialMatchPanel
			// 
			this.PotentialMatchPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PotentialMatchPanel.ColumnCount = 2;
			this.PotentialMatchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
			this.PotentialMatchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
			this.PotentialMatchPanel.Controls.Add(this.ScreenedPartyHeaderLabel, 0, 0);
			this.PotentialMatchPanel.Controls.Add(this.DeniedPartyHeaderLabel, 1, 0);
			this.PotentialMatchPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialMatchPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchPanel.Name = "PotentialMatchPanel";
			this.PotentialMatchPanel.RowCount = 1;
			this.PotentialMatchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PotentialMatchPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 24, true);
			this.PotentialMatchPanel.TabIndex = 3;
			// 
			// GenericMatchHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PotentialMatchPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "GenericMatchHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PotentialMatchPanel.ResumeLayout(false);
			this.PotentialMatchPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZLabel ScreenedPartyHeaderLabel;
		public ZArchitecture.ZLabel DeniedPartyHeaderLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel PotentialMatchPanel;
	}
}
