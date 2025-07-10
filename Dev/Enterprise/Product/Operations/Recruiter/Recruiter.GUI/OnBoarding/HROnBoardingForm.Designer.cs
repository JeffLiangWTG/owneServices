
namespace Enterprise.Recruiter.GUI
{
	partial class HROnBoardingForm
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
			this.components = new System.ComponentModel.Container();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.WorkingBasisTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicantGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OnBoardingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.OnBoardingTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.ApplicantGuidFindBox.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.OnBoardingTabControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 966, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1354, 32, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HROnBoarding);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.BranchGuidFindBox);
			this.TopPanel.Controls.Add(this.WorkingBasisTextBox);
			this.TopPanel.Controls.Add(this.StartDateTextBox);
			this.TopPanel.Controls.Add(this.ApplicantGuidFindBox);
			this.TopPanel.Controls.Add(this.OnBoardingButton);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1354, 93, true);
			this.TopPanel.TabIndex = 0;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "HOB_GB_HomeBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HROnBoarding)(null)).HOB_GB_HomeBranch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("53475794-2b21-4f58-ab8b-edabfb821232", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 60, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 15, true);
			this.BranchGuidFindBox.TabIndex = 3;
			// 
			// WorkingBasisTextBox
			// 
			this.BindingSource.SetBindingMember(this.WorkingBasisTextBox, "HOB_WorkingBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HROnBoarding)(null)).HOB_WorkingBasis)));
			this.WorkingBasisTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("d91c9767-c73a-4c50-aa47-5b330a018574", "Working Basis");
			this.WorkingBasisTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 60, true);
			this.WorkingBasisTextBox.Name = "WorkingBasisTextBox";
			this.WorkingBasisTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 15, true);
			this.WorkingBasisTextBox.TabIndex = 2;
			// 
			// StartDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartDateTextBox, "HOB_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HROnBoarding)(null)).HOB_StartDate)));
			this.StartDateTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("efb4a756-faff-4593-960b-49c1d4354c04", "Start Date");
			this.StartDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 20, true);
			this.StartDateTextBox.Name = "StartDateTextBox";
			this.StartDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 15, true);
			this.StartDateTextBox.TabIndex = 1;
			// 
			// ApplicantGuidFindBox
			// 
			this.ApplicantGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicantGuidFindBox, "HOB_HA_JobApplicant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HROnBoarding)(null)).HOB_HA_JobApplicant)));
			this.ApplicantGuidFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("fcad203f-e7c6-4311-ae04-3348a280d97a", "Job Applicant");
			this.ApplicantGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 20, true);
			this.ApplicantGuidFindBox.Name = "ApplicantGuidFindBox";
			this.ApplicantGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ApplicantGuidFindBox.ParentType = null;
			this.ApplicantGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 15, true);
			this.ApplicantGuidFindBox.TabIndex = 0;
			// 
			// OnBoardingButton
			// 
			this.OnBoardingButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("28a611a8-b815-4f7a-9a24-0e4c782f0367", "View On boarding in GLOW");
			this.OnBoardingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(976, 20, true);
			this.OnBoardingButton.Name = "OnBoardingButton";
			this.OnBoardingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.OnBoardingButton.TabIndex = 4;
			this.OnBoardingButton.ToolTipCaption = null;
			this.OnBoardingButton.Click += new System.EventHandler(this.OnBoardingButton_Click);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1309, 770, true);
			this.LogsTabPage.TabIndex = 4;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1309, 751, true);
			this.NotesTabPage.TabIndex = 3;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("fba5f0ab-218a-480b-95c4-10f3ce0ea078", "Workflow & Tracking");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1309, 751, true);
			this.WorkflowTabPage.TabIndex = 2;
			// 
			// OnBoardingTabControl
			// 
			this.OnBoardingTabControl.Controls.Add(this.WorkflowTabPage);
			this.OnBoardingTabControl.Controls.Add(this.NotesTabPage);
			this.OnBoardingTabControl.Controls.Add(this.LogsTabPage);
			this.OnBoardingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 101, true);
			this.OnBoardingTabControl.Name = "OnBoardingTabControl";
			this.OnBoardingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1317, 794, true);
			this.OnBoardingTabControl.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 910, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1354, 56, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1009, 8, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 43, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// HROnBoardingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("65694c08-8d9f-4897-ace2-888e2ead086d", "On Boarding");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1354, 998, true);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.OnBoardingTabControl);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.HROnBoarding);
			this.Name = "HROnBoardingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "HROnBoardingForm";
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.OnBoardingTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ApplicantGuidFindBox.ResumeLayout(true);
			this.ApplicantGuidFindBox.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.OnBoardingTabControl.ResumeLayout(false);
			this.OnBoardingTabControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZPanel TopPanel;
		protected ZArchitecture.GUI.ZGuidFindBox ApplicantGuidFindBox;
		protected ZArchitecture.ZTextBox StartDateTextBox;
		private ZArchitecture.GUI.ZLogsTabPage LogsTabPage;
		private ZArchitecture.GUI.ZStmNoteTabPage NotesTabPage;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		protected ZArchitecture.GUI.ZTemplateTabControl OnBoardingTabControl;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		protected ZArchitecture.ZTextBox WorkingBasisTextBox;
		protected ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private ZArchitecture.GUI.ZButton OnBoardingButton;
	}
}
