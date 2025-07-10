namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkItemForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.splitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerTop = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerTopLeft = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerTopRight = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Details = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MiddleTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.State = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ReleaseSequence = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.RightTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WorkItemCustomFields = new Enterprise.ProcessManagement.GUI.DynamicControl();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WorkItemDescription = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.RelatedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
			this.splitContainerTop.Panel1.SuspendLayout();
			this.splitContainerTop.Panel2.SuspendLayout();
			this.splitContainerTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTopLeft)).BeginInit();
			this.splitContainerTopLeft.Panel1.SuspendLayout();
			this.splitContainerTopLeft.Panel2.SuspendLayout();
			this.splitContainerTopLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTopRight)).BeginInit();
			this.splitContainerTopRight.Panel1.SuspendLayout();
			this.splitContainerTopRight.Panel2.SuspendLayout();
			this.splitContainerTopRight.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.Details.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.State.SuspendLayout();
			this.ReleaseSequence.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.WorkItemCustomFields.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.WorkItemDescription.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.RelatedItemsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1310, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RelatedItemsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.splitContainerMain);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1302, 603, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerMain.Name = "splitContainerMain";
			this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.splitContainerTop);
			this.splitContainerMain.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(239);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.LeftBottomPanel);
			this.splitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 603, true);
			this.splitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(225);
			this.splitContainerMain.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.splitContainerMain.TabIndex = 0;
			this.splitContainerMain.TabStop = false;
			// 
			// splitContainerTop
			// 
			this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerTop.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 0, true);
			this.splitContainerTop.Name = "splitContainerTop";
			// 
			// splitContainerTop.Panel1
			// 
			this.splitContainerTop.Panel1.Controls.Add(this.splitContainerTopLeft);
			this.splitContainerTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 225, true);
			this.splitContainerTop.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(725);
			// 
			// splitContainerTop.Panel2
			// 
			this.splitContainerTop.Panel2.Controls.Add(this.RightTopPanel);
			this.splitContainerTop.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(555);
			this.splitContainerTop.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(746);
			this.splitContainerTop.TabIndex = 2;
			this.splitContainerTop.TabStop = false;
			// 
			// splitContainerTopLeft
			// 
			this.splitContainerTopLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTopLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerTopLeft.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 0, true);
			this.splitContainerTopLeft.Name = "splitContainerTopLeft";
			// 
			// splitContainerTopLeft.Panel1
			// 
			this.splitContainerTopLeft.Panel1.Controls.Add(this.LeftTopPanel);
			this.splitContainerTopLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 225, true);
			this.splitContainerTopLeft.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			// 
			// splitContainerTopLeft.Panel2
			// 
			this.splitContainerTopLeft.Panel2.Controls.Add(this.MiddleTopPanel);
			this.splitContainerTopLeft.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.splitContainerTopLeft.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.splitContainerTopLeft.TabIndex = 0;
			this.splitContainerTopLeft.TabStop = false;
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.Details);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 225, true);
			this.LeftTopPanel.TabIndex = 0;
			// 
			// Details
			// 
			this.Details.AllowDrop = true;
			this.Details.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Details.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Details.Name = "Details";
			this.Details.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 225, true);
			this.Details.TabIndex = 0;
			this.Details.UserControlType = typeof(Enterprise.ProcessManagement.GUI.WorkItemDetailsControl);
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.State);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 225, true);
			this.MiddleTopPanel.TabIndex = 1;
			// 
			// State
			// 
			this.State.AllowDrop = true;
			this.State.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.State.Dock = System.Windows.Forms.DockStyle.Fill;
			this.State.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.State.Name = "State";
			this.State.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 225, true);
			this.State.TabIndex = 1;
			this.State.UserControlType = typeof(Enterprise.ProcessManagement.GUI.WorkItemStatusControl);
			// 
			// ReleaseSequence
			// 
			this.ReleaseSequence.AllowDrop = true;
			this.ReleaseSequence.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ReleaseSequence.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReleaseSequence.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReleaseSequence.Name = "ReleaseSequence";
			this.ReleaseSequence.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 225, true);
			this.ReleaseSequence.TabIndex = 1;
			this.ReleaseSequence.UserControlType = typeof(Enterprise.ProcessManagement.GUI.WorkItemReleaseSequenceControl);
			// 
			// splitContainerTopRight
			// 
			this.splitContainerTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTopRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerTopRight.Name = "splitContainerTopRight";
			this.splitContainerTopRight.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			// 
			// splitContainerTopRight.Panel1
			// 
			this.splitContainerTopRight.Panel1.Controls.Add(this.WorkItemCustomFields);
			this.splitContainerTopRight.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			// 
			// splitContainerTopRight.Panel2
			// 
			this.splitContainerTopRight.Panel2.Controls.Add(this.ReleaseSequence);
			this.splitContainerTopRight.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(302);
			this.splitContainerTopRight.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(274);
			this.splitContainerTopRight.TabIndex = 0;
			this.splitContainerTopRight.TabStop = false;
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Controls.Add(this.splitContainerTopRight);
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 225, true);
			this.RightTopPanel.TabIndex = 0;
			// 
			// WorkItemCustomFields
			// 
			this.WorkItemCustomFields.AllowDrop = true;
			this.WorkItemCustomFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkItemCustomFields.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkItemCustomFields.Name = "WorkItemCustomFields";
			this.WorkItemCustomFields.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 225, true);
			this.WorkItemCustomFields.TabIndex = 0;
			this.WorkItemCustomFields.UserControlType = typeof(Enterprise.ProcessManagement.GUI.WorkItemCustomFieldsControl);
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.WorkItemDescription);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 374, true);
			this.LeftBottomPanel.TabIndex = 0;
			// 
			// WorkItemDescription
			// 
			this.WorkItemDescription.AllowDrop = true;
			this.WorkItemDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkItemDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkItemDescription.Name = "WorkItemDescription";
			this.WorkItemDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 374, true);
			this.WorkItemDescription.TabIndex = 0;
			this.WorkItemDescription.UserControlType = typeof(Enterprise.ProcessManagement.GUI.WorkItemDescriptionControl);
			// 
			// WorkItemForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 686, true);
			this.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "WorkItemForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
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
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.splitContainerMain.PerformLayout();
			this.splitContainerTop.Panel1.ResumeLayout(false);
			this.splitContainerTop.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
			this.splitContainerTop.ResumeLayout(false);
			this.splitContainerTop.PerformLayout();
			this.splitContainerTopLeft.Panel1.ResumeLayout(false);
			this.splitContainerTopLeft.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTopLeft)).EndInit();
			this.splitContainerTopLeft.ResumeLayout(false);
			this.splitContainerTopLeft.PerformLayout();
			this.splitContainerTopRight.Panel1.ResumeLayout(false);
			this.splitContainerTopRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTopRight)).EndInit();
			this.splitContainerTopRight.ResumeLayout(false);
			this.splitContainerTopRight.PerformLayout();
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.Details.ResumeLayout(true);
			this.Details.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.State.ResumeLayout(true);
			this.State.PerformLayout();
			this.ReleaseSequence.ResumeLayout(true);
			this.ReleaseSequence.PerformLayout();
			this.RightTopPanel.ResumeLayout(false);
			this.RightTopPanel.PerformLayout();
			this.WorkItemCustomFields.ResumeLayout(true);
			this.WorkItemCustomFields.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.WorkItemDescription.ResumeLayout(true);
			this.WorkItemDescription.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		protected CargoWise.Windows.UI.KSplitContainer splitContainerMain;
		private CargoWise.Windows.UI.KSplitContainer splitContainerTopLeft;
		private CargoWise.Windows.UI.KSplitContainer splitContainerTopRight;
		public ZArchitecture.GUI.ZPanel LeftTopPanel;
		private ZArchitecture.GUI.ZPanel RightTopPanel;
		private ZArchitecture.GUI.ZPanel LeftBottomPanel;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl Details;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl WorkItemDescription;
		private DynamicControl WorkItemCustomFields;
		private ZArchitecture.GUI.ZPanel MiddleTopPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainerTop;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl State;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl ReleaseSequence;
	}
}
