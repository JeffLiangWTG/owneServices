namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class GenericMatchItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BottomBorderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PotentialMatchPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DeniedPartyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScoreGradePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScoreLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeniedPartyInnerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScreenedPartyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScreenedPartyInnerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PotentialMatchPanel.SuspendLayout();
			this.DeniedPartyPanel.SuspendLayout();
			this.ScreenedPartyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BottomBorderPanel
			// 
			this.BottomBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BottomBorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(234)))), ((int)(((byte)(236)))));
			this.BottomBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.BottomBorderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BottomBorderPanel.Name = "BottomBorderPanel";
			this.BottomBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 1, true);
			this.BottomBorderPanel.TabIndex = 0;
			// 
			// PotentialMatchPanel
			// 
			this.PotentialMatchPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PotentialMatchPanel.AutoSize = true;
			this.PotentialMatchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.PotentialMatchPanel.ColumnCount = 2;
			this.PotentialMatchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
			this.PotentialMatchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
			this.PotentialMatchPanel.Controls.Add(this.DeniedPartyPanel, 1, 0);
			this.PotentialMatchPanel.Controls.Add(this.ScreenedPartyPanel, 0, 0);
			this.PotentialMatchPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialMatchPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PotentialMatchPanel.Name = "PotentialMatchPanel";
			this.PotentialMatchPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 2, true);
			this.PotentialMatchPanel.RowCount = 1;
			this.PotentialMatchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PotentialMatchPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 30, true);
			this.PotentialMatchPanel.TabIndex = 1;
			// 
			// DeniedPartyPanel
			// 
			this.DeniedPartyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeniedPartyPanel.AutoSize = true;
			this.DeniedPartyPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
			this.DeniedPartyPanel.Controls.Add(this.ScoreGradePanel);
			this.DeniedPartyPanel.Controls.Add(this.ScoreLabel);
			this.DeniedPartyPanel.Controls.Add(this.DeniedPartyInnerPanel);
			this.DeniedPartyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 1, true);
			this.DeniedPartyPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.DeniedPartyPanel.Name = "DeniedPartyPanel";
			this.DeniedPartyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 26, true);
			this.DeniedPartyPanel.TabIndex = 4;
			// 
			// ScoreGradePanel
			// 
			this.ScoreGradePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ScoreGradePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
			this.ScoreGradePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScoreGradePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ScoreGradePanel.Name = "ScoreGradePanel";
			this.ScoreGradePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 26, true);
			this.ScoreGradePanel.TabIndex = 2;
			// 
			// ScoreLabel
			// 
			this.ScoreLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ScoreLabel.BackColor = System.Drawing.Color.Transparent;
			this.ScoreLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ScoreLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 0, true);
			this.ScoreLabel.Name = "ScoreLabel";
			this.ScoreLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.ScoreLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 21, true);
			this.ScoreLabel.TabIndex = 1;
			this.ScoreLabel.UseMnemonic = false;
			// 
			// DeniedPartyInnerPanel
			// 
			this.DeniedPartyInnerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeniedPartyInnerPanel.AutoSize = true;
			this.DeniedPartyInnerPanel.BackColor = System.Drawing.Color.Transparent;
			this.DeniedPartyInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.DeniedPartyInnerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DeniedPartyInnerPanel.Name = "DeniedPartyInnerPanel";
			this.DeniedPartyInnerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 5, true);
			this.DeniedPartyInnerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 25, true);
			this.DeniedPartyInnerPanel.TabIndex = 0;
			// 
			// ScreenedPartyPanel
			// 
			this.ScreenedPartyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ScreenedPartyPanel.AutoSize = true;
			this.ScreenedPartyPanel.Controls.Add(this.ScreenedPartyInnerPanel);
			this.ScreenedPartyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.ScreenedPartyPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 2, true);
			this.ScreenedPartyPanel.Name = "ScreenedPartyPanel";
			this.ScreenedPartyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 26, true);
			this.ScreenedPartyPanel.TabIndex = 5;
			// 
			// ScreenedPartyInnerPanel
			// 
			this.ScreenedPartyInnerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ScreenedPartyInnerPanel.AutoSize = true;
			this.ScreenedPartyInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreenedPartyInnerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ScreenedPartyInnerPanel.Name = "ScreenedPartyInnerPanel";
			this.ScreenedPartyInnerPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 5, true);
			this.ScreenedPartyInnerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 26, true);
			this.ScreenedPartyInnerPanel.TabIndex = 0;
			// 
			// GenericMatchItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.BottomBorderPanel);
			this.Controls.Add(this.PotentialMatchPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "GenericMatchItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PotentialMatchPanel.ResumeLayout(false);
			this.PotentialMatchPanel.PerformLayout();
			this.DeniedPartyPanel.ResumeLayout(false);
			this.DeniedPartyPanel.PerformLayout();
			this.ScreenedPartyPanel.ResumeLayout(false);
			this.ScreenedPartyPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomBorderPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel PotentialMatchPanel;
		private ZArchitecture.GUI.ZPanel ScreenedPartyInnerPanel;
		private ZArchitecture.GUI.ZPanel DeniedPartyPanel;
		private ZArchitecture.GUI.ZPanel ScoreGradePanel;
		private ZArchitecture.ZLabel ScoreLabel;
		private ZArchitecture.GUI.ZPanel DeniedPartyInnerPanel;
		private ZArchitecture.GUI.ZPanel ScreenedPartyPanel;
	}
}
