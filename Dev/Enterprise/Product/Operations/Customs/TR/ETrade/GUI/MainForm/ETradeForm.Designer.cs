using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class ETradeForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		
		new void InitializeComponent()
		{
			this.asycudaManifestUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaManifestUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.asycudaManifestUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 632, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("ETradeForm|F7FD0A96-8337-4923-A307-687B8F1D398D", "Main");
			this.MainTabPage.Controls.Add(this.asycudaManifestUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// MainPanel
			// 
			this.MainPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 632, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader);
			// 
			// asycudaManifestUserControl
			// 
			this.asycudaManifestUserControl.AllowDrop = true;
			this.asycudaManifestUserControl.AutoScroll = true;
			this.asycudaManifestUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.asycudaManifestUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)))));
			this.asycudaManifestUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaManifestUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asycudaManifestUserControl.Name = "asycudaManifestUserControl";
			this.asycudaManifestUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			this.asycudaManifestUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("681A7546-04DA-4A94-B9B2-F6C9B48B0C23", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// ETradeForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 688, true);
			this.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 725, true);
			this.Name = "ETradeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = Enterprise.Customs.TR.ETrade.GUI.Res.GetString("DAFB418B-66EA-4530-99E6-29567F5AE977", "E-Trade");
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.asycudaManifestUserControl.ResumeLayout(true);
			this.asycudaManifestUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal AsycudaManifestUserControl asycudaManifestUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
