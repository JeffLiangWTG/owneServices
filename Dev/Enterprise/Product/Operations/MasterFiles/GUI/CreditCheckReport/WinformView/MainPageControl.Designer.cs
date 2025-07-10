namespace Enterprise.MasterFiles.GUI
{
	partial class MainPageControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.kTableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.topBannerControl = new Enterprise.MasterFiles.GUI.TopBannerControl();
			this.zLabelTopLine = new Enterprise.ZArchitecture.ZLabel();
			this.reportsControl = new Enterprise.MasterFiles.GUI.ReportsControl();
			this.zLabelBottomLine = new Enterprise.ZArchitecture.ZLabel();
			this.eventsBannerControl = new Enterprise.MasterFiles.GUI.EventsBannerControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.kTableLayoutPanel1.SuspendLayout();
			this.topBannerControl.SuspendLayout();
			this.reportsControl.SuspendLayout();
			this.eventsBannerControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.MainPageModel);
			// 
			// kTableLayoutPanel1
			// 
			this.kTableLayoutPanel1.ColumnCount = 1;
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.kTableLayoutPanel1.Controls.Add(this.topBannerControl, 0, 0);
			this.kTableLayoutPanel1.Controls.Add(this.zLabelTopLine, 0, 1);
			this.kTableLayoutPanel1.Controls.Add(this.reportsControl, 0, 2);
			this.kTableLayoutPanel1.Controls.Add(this.zLabelBottomLine, 0, 3);
			this.kTableLayoutPanel1.Controls.Add(this.eventsBannerControl, 0, 4);
			this.kTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kTableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kTableLayoutPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.kTableLayoutPanel1.Name = "kTableLayoutPanel1";
			this.kTableLayoutPanel1.RowCount = 5;
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140)));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.kTableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 525, true);
			this.kTableLayoutPanel1.TabIndex = 0;
			// 
			// topBannerControl
			// 
			this.topBannerControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topBannerControl, "TopBannerModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.GUI.TopBannerModel)(((Enterprise.MasterFiles.GUI.MainPageModel)(null)).TopBannerModel)));
			this.topBannerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topBannerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topBannerControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.topBannerControl.Name = "topBannerControl";
			this.topBannerControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 7, 20, 0, true);
			this.topBannerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 93, true);
			this.topBannerControl.TabIndex = 1;
			// 
			// zLabelTopLine
			// 
			this.zLabelTopLine.BackColor = System.Drawing.SystemColors.Control;
			this.zLabelTopLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.zLabelTopLine.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabelTopLine, false);
			this.zLabelTopLine.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 95, true);
			this.zLabelTopLine.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 3, 10, 7, true);
			this.zLabelTopLine.Name = "zLabelTopLine";
			this.zLabelTopLine.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 3, true);
			this.zLabelTopLine.TabIndex = 2;
			// 
			// reportsControl
			// 
			this.reportsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reportsControl, "ReportsModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.GUI.ReportsModel)(((Enterprise.MasterFiles.GUI.MainPageModel)(null)).ReportsModel)));
			this.reportsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 107, true);
			this.reportsControl.Name = "reportsControl";
			this.reportsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 263, true);
			this.reportsControl.TabIndex = 3;
			// 
			// zLabelBottomLine
			// 
			this.zLabelBottomLine.BackColor = System.Drawing.SystemColors.Control;
			this.zLabelBottomLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.zLabelBottomLine.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabelBottomLine, false);
			this.zLabelBottomLine.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 375, true);
			this.zLabelBottomLine.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 3, 10, 7, true);
			this.zLabelBottomLine.Name = "zLabelBottomLine";
			this.zLabelBottomLine.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 3, true);
			this.zLabelBottomLine.TabIndex = 4;
			// 
			// eventsBannerControl
			// 
			this.eventsBannerControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eventsBannerControl, "EventsBannerModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.GUI.EventsBannerModel)(((Enterprise.MasterFiles.GUI.MainPageModel)(null)).EventsBannerModel)));
			this.eventsBannerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventsBannerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 387, true);
			this.eventsBannerControl.Name = "eventsBannerControl";
			this.eventsBannerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 150, true);
			this.eventsBannerControl.TabIndex = 5;
			// 
			// MainPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.kTableLayoutPanel1);
			this.Name = "MainPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 825, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kTableLayoutPanel1.ResumeLayout(false);
			this.kTableLayoutPanel1.PerformLayout();
			this.topBannerControl.ResumeLayout(true);
			this.topBannerControl.PerformLayout();
			this.reportsControl.ResumeLayout(true);
			this.reportsControl.PerformLayout();
			this.eventsBannerControl.ResumeLayout(true);
			this.eventsBannerControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel kTableLayoutPanel1;
		private ZArchitecture.ZLabel zLabelTopLine;
		private ZArchitecture.ZLabel zLabelBottomLine;
		private Enterprise.MasterFiles.GUI.TopBannerControl topBannerControl;
		private Enterprise.MasterFiles.GUI.ReportsControl reportsControl;
		private Enterprise.MasterFiles.GUI.EventsBannerControl eventsBannerControl;
	}
}
