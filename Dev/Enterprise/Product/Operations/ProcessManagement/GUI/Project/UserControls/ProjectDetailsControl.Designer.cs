using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectDetailsControl : ZUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SplitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerTop = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.State = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.splitContainerRightTop = new CargoWise.Windows.UI.KSplitContainer();
			this.MiddleTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactInformation = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.RightTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Information = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.splitContainerBottom = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Log = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.RightBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Description = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).BeginInit();
			this.SplitContainerMain.Panel1.SuspendLayout();
			this.SplitContainerMain.Panel2.SuspendLayout();
			this.SplitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
			this.splitContainerTop.Panel1.SuspendLayout();
			this.splitContainerTop.Panel2.SuspendLayout();
			this.splitContainerTop.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.State.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerRightTop)).BeginInit();
			this.splitContainerRightTop.Panel1.SuspendLayout();
			this.splitContainerRightTop.Panel2.SuspendLayout();
			this.splitContainerRightTop.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.ContactInformation.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.Information.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).BeginInit();
			this.splitContainerBottom.Panel1.SuspendLayout();
			this.splitContainerBottom.Panel2.SuspendLayout();
			this.splitContainerBottom.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.Log.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.Description.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerMain
			// 
			this.SplitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainerMain.Name = "SplitContainerMain";
			this.SplitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainerMain.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(305);
			// 
			// splitContainerMain.Panel1
			// 
			this.SplitContainerMain.Panel1.Controls.Add(this.splitContainerTop);
			// 
			// splitContainerMain.Panel2
			// 
			this.SplitContainerMain.Panel2.Controls.Add(this.splitContainerBottom);
			this.SplitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 537, true);
			this.SplitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(305);
			this.SplitContainerMain.TabIndex = 0;
			this.SplitContainerMain.TabStop = false;
			// 
			// splitContainerTop
			// 
			this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerTop.Name = "splitContainerTop";
			// 
			// splitContainerTop.Panel1
			// 
			this.splitContainerTop.Panel1.Controls.Add(this.LeftTopPanel);
			// 
			// splitContainerTop.Panel2
			// 
			this.splitContainerTop.Panel2.Controls.Add(this.splitContainerRightTop);
			this.splitContainerTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 305, true);
			this.splitContainerTop.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(397);
			this.splitContainerTop.TabIndex = 0;
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.State);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 305, true);
			this.LeftTopPanel.TabIndex = 0;
			// 
			// State
			// 
			this.State.AllowDrop = true;
			this.State.Dock = System.Windows.Forms.DockStyle.Fill;
			this.State.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.State.Name = "State";
			this.State.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 305, true);
			this.State.TabIndex = 1;
			this.State.UserControlType = typeof(Enterprise.ProcessManagement.GUI.ProjectStatusControl);
			// 
			// splitContainerRightTop
			// 
			this.splitContainerRightTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerRightTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerRightTop.Name = "splitContainerRightTop";
			// 
			// splitContainerRightTop.Panel1
			// 
			this.splitContainerRightTop.Panel1.Controls.Add(this.MiddleTopPanel);
			// 
			// splitContainerRightTop.Panel2
			// 
			this.splitContainerRightTop.Panel2.Controls.Add(this.RightTopPanel);
			this.splitContainerRightTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 305, true);
			this.splitContainerRightTop.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(447);
			this.splitContainerRightTop.TabIndex = 0;
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.ContactInformation);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 305, true);
			this.MiddleTopPanel.TabIndex = 0;
			// 
			// ContactInformation
			// 
			this.ContactInformation.AllowDrop = true;
			this.ContactInformation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactInformation.Name = "ContactInformation";
			this.ContactInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 305, true);
			this.ContactInformation.TabIndex = 2;
			this.ContactInformation.UserControlType = typeof(Enterprise.ProcessManagement.GUI.ProjectContactControl);
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Controls.Add(this.Information);
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 305, true);
			this.RightTopPanel.TabIndex = 0;
			// 
			// Information
			// 
			this.Information.AllowDrop = true;
			this.Information.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Information.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Information.Name = "Information";
			this.Information.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 305, true);
			this.Information.TabIndex = 1;
			this.Information.UserControlType = typeof(Enterprise.ProcessManagement.GUI.ProjectInformationControl);
			// 
			// splitContainerBottom
			// 
			this.splitContainerBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerBottom.Name = "splitContainerBottom";
			// 
			// splitContainerBottom.Panel1
			// 
			this.splitContainerBottom.Panel1.Controls.Add(this.LeftBottomPanel);
			// 
			// splitContainerBottom.Panel2
			// 
			this.splitContainerBottom.Panel2.Controls.Add(this.RightBottomPanel);
			this.splitContainerBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 228, true);
			this.splitContainerBottom.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(700);
			this.splitContainerBottom.TabIndex = 0;
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.Log);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 228, true);
			this.LeftBottomPanel.TabIndex = 0;
			// 
			// Log
			// 
			this.Log.AllowDrop = true;
			this.Log.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Log.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Log.Name = "Log";
			this.Log.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 228, true);
			this.Log.TabIndex = 2;
			this.Log.UserControlType = typeof(Enterprise.ProcessManagement.GUI.ProjectLogControl);
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.Description);
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 228, true);
			this.RightBottomPanel.TabIndex = 0;
			// 
			// Description
			// 
			this.Description.AllowDrop = true;
			this.Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Description.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Description.Name = "Description";
			this.Description.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 228, true);
			this.Description.TabIndex = 2;
			this.Description.UserControlType = typeof(Enterprise.ProcessManagement.GUI.ProjectDescriptionControl);
			// 
			// ProjectDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainerMain);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 537, true);
			this.Name = "ProjectDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainerMain.Panel1.ResumeLayout(false);
			this.SplitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).EndInit();
			this.SplitContainerMain.ResumeLayout(false);
			this.SplitContainerMain.PerformLayout();
			this.splitContainerTop.Panel1.ResumeLayout(false);
			this.splitContainerTop.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
			this.splitContainerTop.ResumeLayout(false);
			this.splitContainerTop.PerformLayout();
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.State.ResumeLayout(true);
			this.State.PerformLayout();
			this.splitContainerRightTop.Panel1.ResumeLayout(false);
			this.splitContainerRightTop.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerRightTop)).EndInit();
			this.splitContainerRightTop.ResumeLayout(false);
			this.splitContainerRightTop.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.ContactInformation.ResumeLayout(true);
			this.ContactInformation.PerformLayout();
			this.RightTopPanel.ResumeLayout(false);
			this.RightTopPanel.PerformLayout();
			this.Information.ResumeLayout(true);
			this.Information.PerformLayout();
			this.splitContainerBottom.Panel1.ResumeLayout(false);
			this.splitContainerBottom.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).EndInit();
			this.splitContainerBottom.ResumeLayout(false);
			this.splitContainerBottom.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.Log.ResumeLayout(true);
			this.Log.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.Description.ResumeLayout(true);
			this.Description.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public CargoWise.Windows.UI.KSplitContainer SplitContainerMain;
		private CargoWise.Windows.UI.KSplitContainer splitContainerTop;
		private CargoWise.Windows.UI.KSplitContainer splitContainerBottom;
		private ZPanel LeftBottomPanel;
		private ZDynamicControlCreationUserControl Description;
		private ZPanel RightBottomPanel;
		internal ZDynamicControlCreationUserControl ContactInformation;
		private ZPanel LeftTopPanel;
		private ZDynamicControlCreationUserControl Information;
		private ZPanel RightTopPanel;
		internal ZDynamicControlCreationUserControl State;
		private ZDynamicControlCreationUserControl Log;
		private CargoWise.Windows.UI.KSplitContainer splitContainerRightTop;
		private ZPanel MiddleTopPanel;

	}
}
