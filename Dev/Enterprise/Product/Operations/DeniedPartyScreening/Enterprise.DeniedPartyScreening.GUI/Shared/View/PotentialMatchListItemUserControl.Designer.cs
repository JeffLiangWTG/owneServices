namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class PotentialMatchListItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProfileName = new Enterprise.ZArchitecture.ZLabel();
			this.ProfileIconPicture = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.WarningIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ScoreGradeText = new Enterprise.ZArchitecture.ZLabel();
			this.ContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfileIconPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WarningIcon)).BeginInit();
			this.ContentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DeniedPartyScreening.GUI.PotentialMatchWinModel);
			// 
			// ProfileName
			// 
			this.ProfileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileName.AutoEllipsis = true;
			this.ProfileName.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ProfileName, "ProfileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DeniedPartyScreening.GUI.PotentialMatchWinModel)(null)).ProfileName)));
			this.ProfileName.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.ProfileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
			this.ProfileName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 9, true);
			this.ProfileName.Name = "ProfileName";
			this.ProfileName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 18, true);
			this.ProfileName.TabIndex = 1;
			this.ProfileName.UseMnemonic = false;
			// 
			// ProfileIconPicture
			// 
			this.ProfileIconPicture.BackColor = System.Drawing.Color.Transparent;
			this.ProfileIconPicture.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.ProfileIconPicture.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ProfileIconPicture.Name = "ProfileIconPicture";
			this.ProfileIconPicture.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ProfileIconPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.ProfileIconPicture.TabIndex = 2;
			this.ProfileIconPicture.TabStop = false;
			// 
			// WarningIcon
			// 
			this.WarningIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WarningIcon.BackColor = System.Drawing.Color.Transparent;
			this.WarningIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 9, true);
			this.WarningIcon.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WarningIcon.Name = "WarningIcon";
			this.WarningIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.WarningIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.WarningIcon.TabIndex = 3;
			this.WarningIcon.TabStop = false;
			// 
			// ScoreGradeText
			// 
			this.ScoreGradeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ScoreGradeText.AutoEllipsis = true;
			this.ScoreGradeText.BackColor = System.Drawing.Color.Transparent;
			this.ScoreGradeText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ScoreGradeText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 9, true);
			this.ScoreGradeText.Name = "ScoreGradeText";
			this.ScoreGradeText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 18, true);
			this.ScoreGradeText.TabIndex = 4;
			this.ScoreGradeText.UseMnemonic = false;
			// 
			// ContentPanel
			// 
			this.ContentPanel.Controls.Add(this.ScoreGradeText);
			this.ContentPanel.Controls.Add(this.WarningIcon);
			this.ContentPanel.Controls.Add(this.ProfileIconPicture);
			this.ContentPanel.Controls.Add(this.ProfileName);
			this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 35, true);
			this.ContentPanel.TabIndex = 5;
			// 
			// PotentialMatchListItemUserControl
			// 
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContentPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 2, 0, true);
			this.Name = "PotentialMatchListItemUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 37, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfileIconPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WarningIcon)).EndInit();
			this.ContentPanel.ResumeLayout(false);
			this.ContentPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel ProfileName;
		private ZArchitecture.GUI.ZPictureBox ProfileIconPicture;
		private ZArchitecture.GUI.ZPictureBox WarningIcon;
		private ZArchitecture.ZLabel ScoreGradeText;
		public ZArchitecture.GUI.ZPanel ContentPanel;
	}
}
