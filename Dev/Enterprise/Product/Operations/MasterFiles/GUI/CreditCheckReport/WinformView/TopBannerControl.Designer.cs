namespace Enterprise.MasterFiles.GUI
{
	partial class TopBannerControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comingSoonPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.statusDetail = new Enterprise.ZArchitecture.ZLabel();
			this.statusDetailInfo = new Enterprise.ZArchitecture.ZLabel();
			this.statusHeader = new Enterprise.ZArchitecture.ZHeaderLabel();
			this.titlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.statusImage = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.contextPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comingSoonPictureBox)).BeginInit();
			this.titlePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusImage)).BeginInit();
			this.contextPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.TopBannerModel);
			// 
			// comingSoonPictureBox
			// 
			this.comingSoonPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.ComingSoonBackground;
			this.comingSoonPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 0, true);
			this.comingSoonPictureBox.Name = "comingSoonPictureBox";
			this.comingSoonPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 120, true);
			this.comingSoonPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.comingSoonPictureBox.TabIndex = 6;
			this.comingSoonPictureBox.TabStop = false;
			// 
			// statusDetail
			// 
			this.BindingSource.SetBindingMember(this.statusDetail, "StatusDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.TopBannerModel)(null)).StatusDetails)));
			this.statusDetail.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.statusDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.statusDetail.Name = "statusDetail";
			this.statusDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 35, true);
			this.statusDetail.TabIndex = 4;
			// 
			// statusDetailInfo
			// 
			this.BindingSource.SetBindingMember(this.statusDetailInfo, "StatusDetailsEventsInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.TopBannerModel)(null)).StatusDetailsEventsInfo)));
			this.statusDetailInfo.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.statusDetailInfo.IsFontBold = true;
			this.statusDetailInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 35, true);
			this.statusDetailInfo.Name = "statusDetailInfo";
			this.statusDetailInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 25, true);
			this.statusDetailInfo.TabIndex = 5;
			this.statusDetailInfo.Visible = false;
			// 
			// statusHeader
			// 
			this.BindingSource.SetBindingMember(this.statusHeader, "StatusHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.TopBannerModel)(null)).StatusHeader)));
			this.statusHeader.Dock = System.Windows.Forms.DockStyle.Left;
			this.statusHeader.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.statusHeader.ForeColor = System.Drawing.Color.Black;
			this.statusHeader.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 0, true);
			this.statusHeader.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.statusHeader.Name = "statusHeader";
			this.statusHeader.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 40, true);
			this.statusHeader.TabIndex = 2;
			// 
			// titlePanel
			// 
			this.titlePanel.Controls.Add(this.statusHeader);
			this.titlePanel.Controls.Add(this.statusImage);
			this.titlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 10, true);
			this.titlePanel.Name = "titlePanel";
			this.titlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 40, true);
			this.titlePanel.TabIndex = 0;
			// 
			// statusImage
			// 
			this.statusImage.Dock = System.Windows.Forms.DockStyle.Left;
			this.statusImage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statusImage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.statusImage.Name = "statusImage";
			this.statusImage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.statusImage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.statusImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.statusImage.TabIndex = 1;
			this.statusImage.TabStop = false;
			this.statusImage.Visible = false;
			// 
			// contextPanel
			// 
			this.contextPanel.Controls.Add(this.statusDetail);
			this.contextPanel.Controls.Add(this.statusDetailInfo);
			this.contextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 60, true);
			this.contextPanel.Name = "contextPanel";
			this.contextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 60, true);
			this.contextPanel.TabIndex = 3;
			// 
			// TopBannerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.titlePanel);
			this.Controls.Add(this.contextPanel);
			this.Controls.Add(this.comingSoonPictureBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "TopBannerControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 7, 20, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comingSoonPictureBox)).EndInit();
			this.titlePanel.ResumeLayout(false);
			this.titlePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusImage)).EndInit();
			this.contextPanel.ResumeLayout(false);
			this.contextPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPictureBox comingSoonPictureBox;
		private ZArchitecture.ZLabel statusDetail;
		private ZArchitecture.ZLabel statusDetailInfo;
		private ZArchitecture.ZHeaderLabel statusHeader;
		private ZArchitecture.GUI.ZPanel titlePanel;
		private ZArchitecture.GUI.ZPictureBox statusImage;
		private ZArchitecture.GUI.ZPanel contextPanel;
	}
}
