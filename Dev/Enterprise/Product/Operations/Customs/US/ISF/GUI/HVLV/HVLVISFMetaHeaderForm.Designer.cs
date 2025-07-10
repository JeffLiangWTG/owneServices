namespace Enterprise.Customs.US.ISF.GUI
{
	partial class HVLVISFMetaHeaderForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MetaHeaderSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CommonDataSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RelatedJobsUserControl = new Enterprise.ZArchitecture.GUI.RelatedJobsUserControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SaveButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.CusISFDetailsUserControl = new Enterprise.Customs.US.ISF.GUI.CusISFDetailsUserControl();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MetaHeaderSplitContainer)).BeginInit();
			this.MetaHeaderSplitContainer.Panel1.SuspendLayout();
			this.MetaHeaderSplitContainer.Panel2.SuspendLayout();
			this.MetaHeaderSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommonDataSplitContainer)).BeginInit();
			this.CommonDataSplitContainer.Panel1.SuspendLayout();
			this.CommonDataSplitContainer.Panel2.SuspendLayout();
			this.CommonDataSplitContainer.SuspendLayout();
			this.CusISFDetailsUserControl.SuspendLayout();
			this.RelatedJobsUserControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 576, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.HVLVISFMetaHeader);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Controls.Add(this.BottomPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 600, true);
			this.MainPanel.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.BackColor = System.Drawing.SystemColors.Control;
			this.TopPanel.Controls.Add(this.MetaHeaderSplitContainer);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 571, true);
			this.TopPanel.TabIndex = 1;
			// 
			// metaHeaderSplitContainer
			// 
			this.MetaHeaderSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MetaHeaderSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MetaHeaderSplitContainer.Name = "metaHeaderSplitContainer";
			this.MetaHeaderSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.MetaHeaderSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			// 
			// metaHeaderSplitContainer.Panel1
			// 
			this.MetaHeaderSplitContainer.Panel1.Controls.Add(this.CusISFDetailsUserControl);
			// 
			// metaHeaderSplitContainer.Panel2
			// 
			this.MetaHeaderSplitContainer.Panel2.Controls.Add(this.RelatedJobsUserControl);
			this.MetaHeaderSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 800, true);
			this.MetaHeaderSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(585);
			this.MetaHeaderSplitContainer.TabIndex = 2;
			// 
			// CusISFDetailsUserControl
			//
			this.CusISFDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusISFDetailsUserControl, "FirstImporterSecurityFilingJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(((Enterprise.Customs.US.ISF.Business.HVLVISFMetaHeader)(null)).FirstImporterSecurityFilingJob)));
			this.CusISFDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusISFDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.CusISFDetailsUserControl.Name = "CusISFDetailsUserControl";
			this.CusISFDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 416, true);
			this.CusISFDetailsUserControl.TabIndex = 3;
			// 
			// RelatedJobsUserControl
			// 
			this.RelatedJobsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedJobsUserControl, "RelatedJobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.RelatedJobCollection)(((Enterprise.Customs.US.ISF.Business.HVLVISFMetaHeader)(null)).RelatedJobs)));
			this.RelatedJobsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedJobsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedJobsUserControl.Name = "RelatedJobsUserControl";
			this.RelatedJobsUserControl.ShouldShowFormAsDialog = true;
			this.RelatedJobsUserControl.TabIndex = 4;
			// 
			// BottomPanel
			// 
			this.BottomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.BottomPanel.Controls.Add(this.SaveButtonUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 571, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 29, true);
			this.BottomPanel.TabIndex = 5;
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.AllowDrop = true;
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 0, true);
			this.SaveButtonUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.SaveButtonUserControl.Name = "SaveButtonUserControl";
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 29, true);
			this.SaveButtonUserControl.TabIndex = 6;
			// 
			// HVLVISFMetaHeaderForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = GUI.Res.GetData("0b746425-7ee6-4053-8249-89ec14b89895", "HVLV ISF Headers");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 800, true);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.HVLVISFMetaHeader);
			this.Name = "HVLVISFMetaHeaderForm";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.MetaHeaderSplitContainer.Panel1.ResumeLayout(false);
			this.MetaHeaderSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MetaHeaderSplitContainer)).EndInit();
			this.MetaHeaderSplitContainer.ResumeLayout(false);
			this.MetaHeaderSplitContainer.PerformLayout();
			this.CommonDataSplitContainer.Panel1.ResumeLayout(false);
			this.CommonDataSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CommonDataSplitContainer)).EndInit();
			this.CommonDataSplitContainer.ResumeLayout(false);
			this.CommonDataSplitContainer.PerformLayout();
			this.CusISFDetailsUserControl.ResumeLayout(true);
			this.CusISFDetailsUserControl.PerformLayout();
			this.RelatedJobsUserControl.ResumeLayout(true);
			this.RelatedJobsUserControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private CargoWise.Windows.UI.KSplitContainer MetaHeaderSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer CommonDataSplitContainer;
		private Enterprise.ZArchitecture.GUI.RelatedJobsUserControl RelatedJobsUserControl;
		private Enterprise.Customs.US.ISF.GUI.CusISFDetailsUserControl CusISFDetailsUserControl;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl SaveButtonUserControl;
	}
}
