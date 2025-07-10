namespace Enterprise.Recruitment.Module
{
	partial class CandidateManagementControl
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
			if (disposing)
			{
				components?.Dispose();
				searchManager?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.grid = new Enterprise.ZArchitecture.GUI.ZFilterGrid();
			this.candidateDetailsControl = new Enterprise.Recruitment.Module.CandidateManagement.CandidateDetailsControl();
			this.pageSplitControl = new CargoWise.Windows.UI.KSplitContainer();
			this.searchGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.moduleHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.toolbarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.moduleActionsToolbar = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.moduleLabelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.moduleLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.candidateDetailsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pageSplitControl)).BeginInit();
			this.pageSplitControl.Panel1.SuspendLayout();
			this.pageSplitControl.Panel2.SuspendLayout();
			this.pageSplitControl.SuspendLayout();
			this.searchGridPanel.SuspendLayout();
			this.moduleHeaderPanel.SuspendLayout();
			this.toolbarPanel.SuspendLayout();
			this.moduleActionsToolbar.SuspendLayout();
			this.moduleLabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruitment.Module.CandidateModuleBusinessObject);
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Applicant.HA_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).PreviousExperienceDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.JobOpening.JobRole.HJ_JobTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.HP_CurrentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Applicant.HA_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.SubmissionTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Rating_Suitable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Rating_Potential)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Rating_Unsuitable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.JobOpening.CampaignLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.JobOpening.HV_CampaignStartDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.JobOpening.HV_CampaignEndDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Stage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(((System.Collections.IList)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).Collection)).SyncRoot)).Application.HP_SourceType)));
			this.grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("25e8585f-07ec-4bf1-8d17-60b2673b0bba", "Candidate Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Applicant+HA_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("dd0e5172-293e-4704-9e08-93273cda0143", "Past Roles");
			zTextBoxColumnStyleInfo2.ColumnName = "PreviousExperienceDetails";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("60bf5913-34fe-43f7-a723-59e839136e97", "Job Title");
			zTextBoxColumnStyleInfo3.ColumnName = "Application+JobOpening+JobRole+HJ_JobTitle";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("e90f5fe8-6136-44e0-af97-8c1285bf4071", "Status");
			zDropEditColumnStyleInfo1.ColumnName = "Status";
			zDropEditColumnStyleInfo1.BindToList = "Application+Lookups+ApplicationStatuses";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("82e68f03-a4a0-4ebc-bb4d-5461bc1411d8", "Country/Region");
			zDropEditColumnStyleInfo2.ColumnName = "Applicant+HA_RN_NKCountry";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("4fd63a79-e63c-47a5-99e1-435781c4f8ad", "Submission Date");
			zDateEditColumnStyleInfo1.ColumnName = "Application+SubmissionTimeLocal";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("fd98d104-5f4b-47b6-b1ed-9e8dcc526013", "Suitable");
			zCheckBoxColumnStyleInfo1.ColumnName = "Rating_Suitable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("fd98d104-5f4b-47b6-b1ed-9e8dcc526014", "Potential");
			zCheckBoxColumnStyleInfo2.ColumnName = "Rating_Potential";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("fd98d104-5f4b-47b6-b1ed-9e8dcc526015", "Unsuitable");
			zCheckBoxColumnStyleInfo3.ColumnName = "Rating_Unsuitable";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("5818D82E-C976-4AAF-91A7-F6D2D9608024", "Location");
			zTextBoxColumnStyleInfo4.ColumnName = "Application+JobOpening+CampaignLocation";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("A0E46D53-0B78-4E9E-8F5E-7B689D2B327A", "Ad Start Date");
			zDateEditColumnStyleInfo2.ColumnName = "Application+JobOpening+HV_CampaignStartDateLocal";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("335B6563-9E4B-4F10-883C-7A48FD1B0708", "Ad End Date");
			zDateEditColumnStyleInfo3.ColumnName = "Application+JobOpening+HV_CampaignEndDateLocal";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("5F7CC802-0CB3-41C4-8413-E8595DF3E3F5", "Stage");
			zTextBoxColumnStyleInfo5.ColumnName = "Stage";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("CF095D45-6E55-4685-AF23-D28E05F93B79", "Source");
			zDropEditColumnStyleInfo3.ColumnName = "Source";
            zDropEditColumnStyleInfo3.BindToList = "Application+Lookups+SourceTypes";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "d05a817b-f021-4dc3-8a22-16c7eee0a30c";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 820, true);
			this.grid.TabIndex = 0;
			// 
			// candidateDetailsControl
			// 
			this.candidateDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.candidateDetailsControl, "DummyForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Recruitment.Common.Candidate)(((Enterprise.Recruitment.Module.CandidateModuleBusinessObject)(null)).DummyForBinding)));
			this.candidateDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.candidateDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.candidateDetailsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 599, true);
			this.candidateDetailsControl.Name = "candidateDetailsControl";
			this.candidateDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 856, true);
			this.candidateDetailsControl.TabIndex = 0;
			// 
			// pageSplitControl
			// 
			this.pageSplitControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageSplitControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pageSplitControl.Name = "pageSplitControl";
			// 
			// pageSplitControl.Panel1
			// 
			this.pageSplitControl.Panel1.Controls.Add(this.searchGridPanel);
			this.pageSplitControl.Panel1.Controls.Add(this.moduleHeaderPanel);
			this.pageSplitControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1367, 856, true);
			this.pageSplitControl.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(580);
			// 
			// pageSplitControl.Panel2
			// 
			this.pageSplitControl.Panel2.Controls.Add(this.candidateDetailsControl);
			this.pageSplitControl.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(420);
			this.pageSplitControl.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(580);
			this.pageSplitControl.SplitterWidth = 8;
			this.pageSplitControl.TabIndex = 0;
			// 
			// searchGridPanel
			// 
			this.searchGridPanel.Controls.Add(this.grid);
			this.searchGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.searchGridPanel.Name = "searchGridPanel";
			this.searchGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 820, true);
			this.searchGridPanel.TabIndex = 2;
			// 
			// moduleHeaderPanel
			// 
			this.moduleHeaderPanel.Controls.Add(this.toolbarPanel);
			this.moduleHeaderPanel.Controls.Add(this.moduleLabelPanel);
			this.moduleHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.moduleHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moduleHeaderPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 36, true);
			this.moduleHeaderPanel.Name = "moduleHeaderPanel";
			this.moduleHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 36, true);
			this.moduleHeaderPanel.TabIndex = 1;
			// 
			// toolbarPanel
			// 
			this.toolbarPanel.Controls.Add(this.moduleActionsToolbar);
			this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolbarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 0, true);
			this.toolbarPanel.Name = "toolbarPanel";
			this.toolbarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 36, true);
			this.toolbarPanel.TabIndex = 1;
			// 
			// moduleActionsToolbar
			// 
			this.moduleActionsToolbar.BackColor = System.Drawing.Color.Transparent;
			this.moduleActionsToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleActionsToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.moduleActionsToolbar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moduleActionsToolbar.Name = "moduleActionsToolbar";
			this.moduleActionsToolbar.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.moduleActionsToolbar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 36, true);
			this.moduleActionsToolbar.TabIndex = 0;
			// 
			// moduleLabelPanel
			// 
			this.moduleLabelPanel.Controls.Add(this.moduleLabel);
			this.moduleLabelPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.moduleLabelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moduleLabelPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 0, true);
			this.moduleLabelPanel.Name = "moduleLabelPanel";
			this.moduleLabelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 36, true);
			this.moduleLabelPanel.TabIndex = 0;
			// 
			// moduleLabel
			// 
			this.moduleLabel.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("064ba27c-ca55-47d4-8e6c-fdcf6d102ae4", "Candidate Management");
			this.moduleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.moduleLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.moduleLabel.IsFontBold = true;
			this.moduleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moduleLabel.Name = "moduleLabel";
			this.moduleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.moduleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 36, true);
			this.moduleLabel.TabIndex = 0;
			// 
			// CandidateManagementControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.pageSplitControl);
			this.Name = "CandidateManagementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1367, 856, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.candidateDetailsControl.ResumeLayout(true);
			this.candidateDetailsControl.PerformLayout();
			this.pageSplitControl.Panel1.ResumeLayout(false);
			this.pageSplitControl.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pageSplitControl)).EndInit();
			this.pageSplitControl.ResumeLayout(false);
			this.pageSplitControl.PerformLayout();
			this.searchGridPanel.ResumeLayout(false);
			this.searchGridPanel.PerformLayout();
			this.moduleHeaderPanel.ResumeLayout(false);
			this.moduleHeaderPanel.PerformLayout();
			this.toolbarPanel.ResumeLayout(false);
			this.toolbarPanel.PerformLayout();
			this.moduleActionsToolbar.ResumeLayout(false);
			this.moduleActionsToolbar.PerformLayout();
			this.moduleLabelPanel.ResumeLayout(false);
			this.moduleLabelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZFilterGrid grid;
		private CandidateManagement.CandidateDetailsControl candidateDetailsControl;
		private CargoWise.Windows.UI.KSplitContainer pageSplitControl;
		private ZArchitecture.GUI.ZPanel moduleHeaderPanel;
		private ZArchitecture.GUI.ZPanel searchGridPanel;
		private ZArchitecture.GUI.ZPanel toolbarPanel;
		private ZArchitecture.GUI.ZPanel moduleLabelPanel;
		private ZArchitecture.ZLabel moduleLabel;
		private ZArchitecture.GUI.ZToolStrip moduleActionsToolbar;
	}
}
