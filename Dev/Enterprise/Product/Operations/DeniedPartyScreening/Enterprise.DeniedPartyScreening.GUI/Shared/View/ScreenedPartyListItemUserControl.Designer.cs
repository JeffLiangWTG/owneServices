namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ScreenedPartyListItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PartyName = new Enterprise.ZArchitecture.ZLabel();
			this.EntityIconPicture = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.PotentialMatchCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntityIconPicture)).BeginInit();
			this.ContentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DeniedPartyScreening.GUI.ScreenedPartyWinModel);
			// 
			// PartyName
			// 
			this.PartyName.AutoEllipsis = true;
			this.PartyName.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.PartyName, "PartyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DeniedPartyScreening.GUI.ScreenedPartyWinModel)(null)).PartyName)));
			this.PartyName.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.PartyName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
			this.PartyName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 6, true);
			this.PartyName.Name = "PartyName";
			this.PartyName.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 32, 0, true);
			this.PartyName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 18, true);
			this.PartyName.TabIndex = 0;
			this.PartyName.UseMnemonic = false;
			// 
			// EntityIconPicture
			// 
			this.EntityIconPicture.BackColor = System.Drawing.Color.Transparent;
			this.EntityIconPicture.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 5, true);
			this.EntityIconPicture.Name = "EntityIconPicture";
			this.EntityIconPicture.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.EntityIconPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.EntityIconPicture.TabIndex = 3;
			this.EntityIconPicture.TabStop = false;
			// 
			// PotentialMatchCountLabel
			// 
			this.PotentialMatchCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PotentialMatchCountLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.PotentialMatchCountLabel, "PotentialMatchWinModelsCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.DeniedPartyScreening.GUI.ScreenedPartyWinModel)(null)).PotentialMatchWinModelsCount)));
			this.PotentialMatchCountLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.PotentialMatchCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 6, true);
			this.PotentialMatchCountLabel.Name = "PotentialMatchCountLabel";
			this.PotentialMatchCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 18, true);
			this.PotentialMatchCountLabel.TabIndex = 4;
			this.PotentialMatchCountLabel.UseMnemonic = false;
			// 
			// ContentPanel
			// 
			this.ContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentPanel.BackColor = System.Drawing.Color.White;
			this.ContentPanel.Controls.Add(this.PotentialMatchCountLabel);
			this.ContentPanel.Controls.Add(this.EntityIconPicture);
			this.ContentPanel.Controls.Add(this.PartyName);
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ContentPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 32, true);
			this.ContentPanel.TabIndex = 5;
			// 
			// ScreenedPartyListItemUserControl
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(222)))), ((int)(((byte)(222)))));
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContentPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 2, 0, true);
			this.Name = "ScreenedPartyListItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 34, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntityIconPicture)).EndInit();
			this.ContentPanel.ResumeLayout(false);
			this.ContentPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel PartyName;
		private ZArchitecture.GUI.ZPictureBox EntityIconPicture;
		private ZArchitecture.ZLabel PotentialMatchCountLabel;
		public ZArchitecture.GUI.ZPanel ContentPanel;
	}
}
