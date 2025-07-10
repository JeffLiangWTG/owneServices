using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public partial class HRHiringRequestForm : ZTemplateForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.System;
		protected override bool SupportsEDocs => false;

		public HRHiringRequestForm(HRHiringRequest hiringRequest)
			: base(hiringRequest)
		{
			InitializeComponent();

			WorkflowTabPage.Initialize(hiringRequest);

			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.HRHiringRequest;
			}
		}

		void WorkflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicantGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.WorkingBasisTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EndDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TeamCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WorkLocCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReportingMgrCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WorkLocCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProbDurationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabPage.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ApplicantGuidFindBox.SuspendLayout();
			this.TeamCodeFindBox.SuspendLayout();
			this.WorkLocCountryCodeFindBox.SuspendLayout();
			this.ReportingMgrCodeFindBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("787a21bd-7449-4f5e-bdcd-18472e8037a9", "Details");
			this.DetailsGroupBox.Controls.Add(this.ProbDurationTextBox);
			this.DetailsGroupBox.Controls.Add(this.WorkLocCityTextBox);
			this.DetailsGroupBox.Controls.Add(this.ReportingMgrCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.WorkLocCountryCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.TeamCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.EndDateTextBox);
			this.DetailsGroupBox.Controls.Add(this.StartDateTextBox);
			this.DetailsGroupBox.Controls.Add(this.WorkingBasisTextBox);
			this.DetailsGroupBox.Controls.Add(this.ApplicantGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.JobTitleTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 365, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// JobTitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobTitleTextBox, "HRR_JobTitle");
			this.JobTitleTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("7ecd757f-06d2-4ba3-af80-fc5feae090d9", "Job Title");
			this.JobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 72, true);
			this.JobTitleTextBox.Name = "JobTitleTextBox";
			this.JobTitleTextBox.ReadOnly = true;
			this.JobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
			this.JobTitleTextBox.TabIndex = 8;
			// 
			// ApplicantGuidFindBox
			// 
			this.ApplicantGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicantGuidFindBox, "HRR_HA_JobApplicant");
			this.ApplicantGuidFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("8accbe4a-1c4e-4dea-b469-65f973a6d491", "Job Applicant");
			this.ApplicantGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 33, true);
			this.ApplicantGuidFindBox.Name = "ApplicantGuidFindBox";
			this.ApplicantGuidFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.ApplicantGuidFindBox.ParentType = null;
			this.ApplicantGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 15, true);
			this.ApplicantGuidFindBox.TabIndex = 0;
			// 
			// WorkingBasisTextBox
			// 
			this.BindingSource.SetBindingMember(this.WorkingBasisTextBox, "HRR_WorkingBasis");
			this.WorkingBasisTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("22c42e4e-07e9-437e-897d-227e6f8da1a8", "Working Basis");
			this.WorkingBasisTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 150, true);
			this.WorkingBasisTextBox.Name = "WorkingBasisTextBox";
			this.WorkingBasisTextBox.ReadOnly = true;
			this.WorkingBasisTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 15, true);
			this.WorkingBasisTextBox.TabIndex = 3;
			// 
			// StartDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartDateTextBox, "HRR_StartDate");
			this.StartDateTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("0708c826-8b65-475e-bebd-cfa003a77f36", "Start Date");
			this.StartDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 232, true);
			this.StartDateTextBox.Name = "StartDateTextBox";
			this.StartDateTextBox.ReadOnly = true;
			this.StartDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 15, true);
			this.StartDateTextBox.TabIndex = 5;
			// 
			// EndDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndDateTextBox, "HRR_EndDate");
			this.EndDateTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("3a289022-3628-44a7-9048-a82d7149d59f", "End Date");
			this.EndDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 276, true);
			this.EndDateTextBox.Name = "EndDateTextBox";
			this.EndDateTextBox.ReadOnly = true;
			this.EndDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 15, true);
			this.EndDateTextBox.TabIndex = 6;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "HRR_Status");
			this.StatusTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("4a0784c1-e374-4e3d-b7df-a35ceb3cde30", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 33, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ReadOnly = true;
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
			this.StatusTextBox.TabIndex = 7;
			// 
			// TeamCodeFindBox
			// 
			this.TeamCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TeamCodeFindBox, "HRR_GST_NKTeam");
			this.TeamCodeFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("18b7b075-bb89-496c-9328-6f58b1d16e35", "Team");
			this.TeamCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 72, true);
			this.TeamCodeFindBox.Name = "TeamCodeFindBox";
			this.TeamCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.TeamCodeFindBox.ParentType = null;
			this.TeamCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 15, true);
			this.TeamCodeFindBox.TabIndex = 1;
			// 
			// WorkLocCountryCodeFindBox
			// 
			this.WorkLocCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkLocCountryCodeFindBox, "HRR_RN_NKWorkLocationCountry");
			this.WorkLocCountryCodeFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("3ca23de7-3054-4190-9ec1-ca7ce8a6a4f8", "Work Location Ctry./Rgn.", "Work Location Country/Region");
			this.WorkLocCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 194, true);
			this.WorkLocCountryCodeFindBox.Name = "WorkLocCountryCodeFindBox";
			this.WorkLocCountryCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.WorkLocCountryCodeFindBox.ParentType = null;
			this.WorkLocCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 15, true);
			this.WorkLocCountryCodeFindBox.TabIndex = 4;
			// 
			// ReportingMgrCodeFindBox
			// 
			this.ReportingMgrCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportingMgrCodeFindBox, "HRR_GS_NKReportingManager");
			this.ReportingMgrCodeFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("fa2d1a8e-ec86-48c0-9139-c77de1ce0ef1", "Reporting Manager");
			this.ReportingMgrCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 111, true);
			this.ReportingMgrCodeFindBox.Name = "ReportingMgrCodeFindBox";
			this.ReportingMgrCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.ReportingMgrCodeFindBox.ParentType = null;
			this.ReportingMgrCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
			this.ReportingMgrCodeFindBox.TabIndex = 9;
			// 
			// WorkLocCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.WorkLocCityTextBox, "HRR_WorkLocationCity");
			this.WorkLocCityTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("808ef0ae-9750-492b-9d48-4982a9de7376", "Work Location City");
			this.WorkLocCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 194, true);
			this.WorkLocCityTextBox.Name = "WorkLocCityTextBox";
			this.WorkLocCityTextBox.ReadOnly = true;
			this.WorkLocCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
			this.WorkLocCityTextBox.TabIndex = 11;
			// 
			// ProbDurationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProbDurationTextBox, "HRR_ProbationDurationOverride");
			this.ProbDurationTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("4ed23639-5394-423b-9ed7-52c595738c06", "Probation End Date");
			this.ProbDurationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 232, true);
			this.ProbDurationTextBox.Name = "ProbDurationTextBox";
			this.ProbDurationTextBox.ReadOnly = true;
			this.ProbDurationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
			this.ProbDurationTextBox.TabIndex = 12;
			this.MainTabPage.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ApplicantGuidFindBox.ResumeLayout(true);
			this.ApplicantGuidFindBox.PerformLayout();
			this.TeamCodeFindBox.ResumeLayout(true);
			this.TeamCodeFindBox.PerformLayout();
			this.WorkLocCountryCodeFindBox.ResumeLayout(true);
			this.WorkLocCountryCodeFindBox.PerformLayout();
			this.ReportingMgrCodeFindBox.ResumeLayout(true);
			this.ReportingMgrCodeFindBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}
	}
}
