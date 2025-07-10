using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.HRM.GUI
{
	partial class ReviewProcessNodeForm
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
		private new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 511, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 511, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 0, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 32, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.HRM.Common.ReviewProcessNode);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 544, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.HRM.Common.ReviewProcessNode)(null)).RRN_RPR_ReviewProcess)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.HRM.Common.ReviewProcessNode)(null)).RRN_GS_Reviewer)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.HRM.Common.ReviewProcessNode)(null)).RRN_RRN_Parent)));
			// 
			// ReviewProcessNodeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("2aa4e222-6650-43ec-b62b-9ab2a2f23599", "Manager Review");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 567, true);
			this.DataSourceType = typeof(Enterprise.HRM.Common.ReviewProcessNode);
			this.Name = "ReviewProcessNodeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.ReviewProcessFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.reviewerFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ParentReviewNodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.MainTabPage.SuspendLayout();
            this.ReviewProcessFindBox.SuspendLayout();
            this.reviewerFindBox.SuspendLayout();
            this.ParentReviewNodeFindBox.SuspendLayout();
            this.MainTabPage.Controls.Add(this.ParentReviewNodeFindBox);
            this.MainTabPage.Controls.Add(this.reviewerFindBox);
            this.MainTabPage.Controls.Add(this.ReviewProcessFindBox);
            // 
            // ReviewProcessFindBox
            // 
            this.ReviewProcessFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReviewProcessFindBox, "RRN_RPR_ReviewProcess");
            this.ReviewProcessFindBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("a4235aa5-74f4-4cd1-9103-35ef17cf01ee", "Review");
            this.ReviewProcessFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 23, true);
            this.ReviewProcessFindBox.Name = "ReviewProcessFindBox";
            this.ReviewProcessFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 26, true);
            this.ReviewProcessFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ReviewProcess;
            this.ReviewProcessFindBox.TabIndex = 0;
            // 
            // reviewerFindBox
            // 
            this.reviewerFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.reviewerFindBox, "RRN_GS_Reviewer");
            this.reviewerFindBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("6ba0e2f2-fe68-4d12-9aea-0292f2ea27af", "Reviewer");
            this.reviewerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 87, true);
            this.reviewerFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.reviewerFindBox.Name = "ReviewerFindBox";
            this.reviewerFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.reviewerFindBox.ParentType = null;
            this.reviewerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 26, true);
            this.reviewerFindBox.TabIndex = 2;
            // 
            // ParentReviewNodeFindBox
            // 
            this.ParentReviewNodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ParentReviewNodeFindBox, "RRN_RRN_Parent");
            this.ParentReviewNodeFindBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("91456bc4-c3d6-4137-86d7-f42806f538c2", "Parent Review");
            this.ParentReviewNodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 55, true);
            this.ParentReviewNodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ReviewProcessNode;
            this.ParentReviewNodeFindBox.Name = "ParentReviewNodeFindBox";
            this.ParentReviewNodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ReviewProcessNode;
            this.ParentReviewNodeFindBox.ParentType = null;
            this.ParentReviewNodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 26, true);
            this.ParentReviewNodeFindBox.TabIndex = 1;
            this.MainTabPage.PerformLayout();
            this.ReviewProcessFindBox.ResumeLayout(true);
            this.ReviewProcessFindBox.PerformLayout();
            this.reviewerFindBox.ResumeLayout(true);
            this.reviewerFindBox.PerformLayout();
            this.ParentReviewNodeFindBox.ResumeLayout(true);
            this.ParentReviewNodeFindBox.PerformLayout();
            this.MainTabPage.ResumeLayout(true);

        }

        private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.NotesTabPage.SuspendLayout();
            this.NotesTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(true);

        }

        void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.WorkflowTabPage.SuspendLayout();
            this.WorkflowTabPage.PerformLayout();
            this.WorkflowTabPage.ResumeLayout(true);

        }

        #endregion

        private ZWorkflowTabPage WorkflowTabPage;
		internal ZGuidFindBox ReviewProcessFindBox;
		internal ZGuidFindBox ParentReviewNodeFindBox;
		private ZGuidFindBox reviewerFindBox;
	}
}
