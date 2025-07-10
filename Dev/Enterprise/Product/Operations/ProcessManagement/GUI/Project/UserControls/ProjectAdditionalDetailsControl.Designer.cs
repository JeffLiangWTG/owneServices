using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectAdditionalDetailsControl : ZUserControl
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
			this.splitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerLeft = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitContainerRight = new CargoWise.Windows.UI.KSplitContainer();
			this.RightTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NoPanelsMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			splitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
			this.splitContainerLeft.Panel1.SuspendLayout();
			this.splitContainerLeft.Panel2.SuspendLayout();
			splitContainerLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
			this.splitContainerRight.Panel1.SuspendLayout();
			this.splitContainerRight.Panel2.SuspendLayout();
			splitContainerRight.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerMain.Name = "splitContainerMain";
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.splitContainerRight);
			this.splitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 694, true);
			this.splitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(613);
			this.splitContainerMain.TabIndex = 0;
			// 
			// splitContainerLeft
			// 
			this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerLeft.Name = "splitContainerLeft";
			this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerLeft.Panel1
			// 
			this.splitContainerLeft.Panel1.Controls.Add(this.LeftTopPanel);
			// 
			// splitContainerLeft.Panel2
			// 
			this.splitContainerLeft.Panel2.Controls.Add(this.LeftBottomPanel);
			this.splitContainerLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 694, true);
			this.splitContainerLeft.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(331);
			this.splitContainerLeft.TabIndex = 0;
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 331, true);
			this.LeftTopPanel.TabIndex = 0;
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 360, true);
			this.LeftBottomPanel.TabIndex = 0;
			// 
			// splitContainerRight
			// 
			this.splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerRight.Name = "splitContainerRight";
			this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerRight.Panel1
			// 
			this.splitContainerRight.Panel1.Controls.Add(this.RightTopPanel);
			// 
			// splitContainerRight.Panel2
			// 
			this.splitContainerRight.Panel2.Controls.Add(this.RightBottomPanel);
			this.splitContainerRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 694, true);
			this.splitContainerRight.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(331);
			this.splitContainerRight.TabIndex = 0;
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 331, true);
			this.RightTopPanel.TabIndex = 0;
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 360, true);
			this.RightBottomPanel.TabIndex = 0;
			// 
			// NoPanelsMessageLabel
			// 
			this.NoPanelsMessageLabel.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("1e2e20c2-0211-48be-9876-1d79a6811020", "To make use of the Additional Details Tab, panels may be moved from the Main Details Tab. This can be done from the Workflow Template > Project > Screen Layout Tab.");
			this.NoPanelsMessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NoPanelsMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoPanelsMessageLabel.Name = "NoPanelsMessageLabel";
			this.NoPanelsMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 694, true);
			this.NoPanelsMessageLabel.TabIndex = 1;
			this.NoPanelsMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NoPanelsMessageLabel.Visible = false;
			// 
			// ProjectAdditionalDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainerMain);
			this.Controls.Add(this.NoPanelsMessageLabel);
			this.Name = "ProjectAdditionalDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 694, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			splitContainerMain.ResumeLayout(false);
			splitContainerMain.PerformLayout();
			this.splitContainerLeft.Panel1.ResumeLayout(false);
			this.splitContainerLeft.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
			splitContainerLeft.ResumeLayout(false);
			splitContainerLeft.PerformLayout();
			this.splitContainerRight.Panel1.ResumeLayout(false);
			this.splitContainerRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
			splitContainerRight.ResumeLayout(false);
			splitContainerRight.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainerMain;
		private CargoWise.Windows.UI.KSplitContainer splitContainerLeft;
		private CargoWise.Windows.UI.KSplitContainer splitContainerRight;
		private ZArchitecture.GUI.ZPanel LeftBottomPanel;
		private ZArchitecture.GUI.ZPanel RightBottomPanel;
		private ZArchitecture.GUI.ZPanel LeftTopPanel;
		private ZArchitecture.GUI.ZPanel RightTopPanel;
		private Enterprise.ZArchitecture.ZLabel NoPanelsMessageLabel;
	}
}
