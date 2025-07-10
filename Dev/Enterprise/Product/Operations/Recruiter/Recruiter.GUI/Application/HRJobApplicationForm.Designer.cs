using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using System.Windows.Forms;

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobApplicationForm
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox ApplicantGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox JobOpeningGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ApplicationDetailsGroupBox;

		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl BottomTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage InterviewTabPage;
		Enterprise.ZArchitecture.ZGrid InterviewsGrid;
		Enterprise.ZArchitecture.ZTextBox CityTextBox;
		Enterprise.ZArchitecture.ZTextBox EMailTextBox;
		protected ZGuidFindBox ApplicantGuidFindBox;
		protected PhoneNumberUserControl MobilePhoneNumberControl;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;

		ZGuidFindBox JobOpeningGuidFindBox;
		ZDateEdit SubmissionDateEdit;
		Enterprise.ZArchitecture.ZTextBox AdTitleTextBox;
		ZGuidFindBox JobRoleGuidFindBox;
		ZDateEdit EndDateEdit;
		ZDateEdit StartDateEdit;

		ZDropEdit CurrentStatusDropEdit;
		ZCodeFindBox AssignedToCodeFindBox;
		ZDropEdit OverallRatingDropEdit;
		ZArchitecture.ZCalcEdit JobExperienceCalcEdit;

		ZGroupBox ReferringSourceGroupBox;
		ZGuidFindBox ReferringOrgGuidFindBox;
		ZGuidDropEdit ReferringOrgGuidZGuidDropEdit;
		ZDropEditWithFixedWidth ReferringSourceDropEdit;
		ZArchitecture.ZTextBox SourceDetailsTextBox;
		ZGuidFindBox ReferringPersonGuidFindBox;
		CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel1;
		ZCodeFindBox ReferringStaffFindBox;

		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ApplicationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AssignedToCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OverallRatingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JobExperienceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.EMailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JobOpeningGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobOpeningGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SubmissionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AdTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JobRoleGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ApplicantGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplicantGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BottomTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.InterviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InterviewsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.ReferringSourceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FlowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ReferringOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ReferringOrgGuidZGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ReferringPersonGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ReferringStaffFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferringSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.SourceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApplicationDetailsGroupBox.SuspendLayout();
			this.CurrentStatusDropEdit.SuspendLayout();
			this.AssignedToCodeFindBox.SuspendLayout();
			this.OverallRatingDropEdit.SuspendLayout();
			this.MobilePhoneNumberControl.SuspendLayout();
			this.JobOpeningGroupBox.SuspendLayout();
			this.JobOpeningGuidFindBox.SuspendLayout();
			this.SubmissionDateEdit.SuspendLayout();
			this.JobRoleGuidFindBox.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.ApplicantGroupBox.SuspendLayout();
			this.ApplicantGuidFindBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.InterviewTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InterviewsGrid)).BeginInit();
			this.InterviewsGrid.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.ReferringSourceGroupBox.SuspendLayout();
			this.FlowLayoutPanel1.SuspendLayout();
			this.ReferringOrgGuidFindBox.SuspendLayout();
			this.ReferringOrgGuidZGuidDropEdit.SuspendLayout();
			this.ReferringPersonGuidFindBox.SuspendLayout();
			this.ReferringStaffFindBox.SuspendLayout();
			this.ReferringSourceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 546, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ReferringSourceGroupBox);
			this.MainTabPage.Controls.Add(this.ApplicantGroupBox);
			this.MainTabPage.Controls.Add(this.JobOpeningGroupBox);
			this.MainTabPage.Controls.Add(this.ApplicationDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.BottomTabControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1218, 519, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1218, 519, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1218, 519, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 546, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplication);
			// 
			// ApplicationDetailsGroupBox
			// 
			this.ApplicationDetailsGroupBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|2fbedf0b-6cc4-4dc0-8c84-237135338740", "Assessment");
			this.ApplicationDetailsGroupBox.Controls.Add(this.CurrentStatusDropEdit);
			this.ApplicationDetailsGroupBox.Controls.Add(this.AssignedToCodeFindBox);
			this.ApplicationDetailsGroupBox.Controls.Add(this.OverallRatingDropEdit);
			this.ApplicationDetailsGroupBox.Controls.Add(this.JobExperienceCalcEdit);
			this.ApplicationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 167, true);
			this.ApplicationDetailsGroupBox.Name = "ApplicationDetailsGroupBox";
			this.ApplicationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 155, true);
			this.ApplicationDetailsGroupBox.TabIndex = 3;
			this.ApplicationDetailsGroupBox.TabStop = false;
			// 
			// CurrentStatusDropEdit
			// 
			this.CurrentStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentStatusDropEdit, "HP_CurrentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_CurrentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Lookups.ApplicationStatuses)));
			this.CurrentStatusDropEdit.BindToList = "Lookups+ApplicationStatuses";
			this.CurrentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 17, true);
			this.CurrentStatusDropEdit.Name = "CurrentStatusDropEdit";
			this.CurrentStatusDropEdit.PreBoundMaxLength = 3;
			this.CurrentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CurrentStatusDropEdit.TabIndex = 11;
			// 
			// AssignedToCodeFindBox
			// 
			this.AssignedToCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedToCodeFindBox, "HP_GS_NKAssignedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_GS_NKAssignedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Lookups.AssignedTos)));
			this.AssignedToCodeFindBox.BindToList = "Lookups+AssignedTos";
			this.AssignedToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 42, true);
			this.AssignedToCodeFindBox.ModuleID = ModuleIDs.GlbStaff;
			this.AssignedToCodeFindBox.Name = "AssignedToCodeFindBox";
			this.AssignedToCodeFindBox.ShouldResize = true;
			this.AssignedToCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.AssignedToCodeFindBox.TabIndex = 12;
			// 
			// OverallRatingDropEdit
			// 
			this.OverallRatingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverallRatingDropEdit, "ApplicationOverallRatingDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ApplicationOverallRatingDescription)));
			this.OverallRatingDropEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|79151d16-bb67-47a1-8a77-593f85d16e5a", "Overall Rating");
			this.OverallRatingDropEdit.CharacterCasing = CharacterCasing.Normal;
			this.OverallRatingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 68, true);
			this.OverallRatingDropEdit.Name = "OverallRatingDropEdit";
			this.OverallRatingDropEdit.ShowDescriptionBox = false;
			this.OverallRatingDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.OverallRatingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.OverallRatingDropEdit.TabIndex = 13;
			this.OverallRatingDropEdit.UseFullWidthForCodeBox = true;
			// 
			// JobExperienceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JobExperienceCalcEdit, "HP_JobExperienceYears");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_JobExperienceYears)));
			this.JobExperienceCalcEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|17cb92b5-b855-4c22-9688-1fddecc47422", "Years Experience");
			this.JobExperienceCalcEdit.DecimalPlaces = 0;
			this.JobExperienceCalcEdit.Decimals = 0;
			this.JobExperienceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 94, true);
			this.JobExperienceCalcEdit.Name = "JobExperienceCalcEdit";
			this.JobExperienceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JobExperienceCalcEdit.TabIndex = 14;
			this.JobExperienceCalcEdit.Text = "0";
			this.JobExperienceCalcEdit.TextAlign = HorizontalAlignment.Right;
			// 
			// MobilePhoneNumberControl
			// 
			this.MobilePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "ApplicantCollection.MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ApplicantCollection)).SyncRoot)).MobilePhoneNumber)));
			this.MobilePhoneNumberControl.CaptionResourceString = Res.GetData("HRJobApplicationForm|367b31bb-b017-40ce-8891-e63ebf54db82", "Mobile");
			this.MobilePhoneNumberControl.EnableValidStateColor = true;
			this.MobilePhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 68, true);
			this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
			this.MobilePhoneNumberControl.ShowLocalNumberLabel = false;
			this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
			this.MobilePhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.MobilePhoneNumberControl.TabIndex = 2;
			// 
			// EMailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EMailTextBox, "ApplicantCollection.HA_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ApplicantCollection)).SyncRoot)).HA_EmailAddress)));
			this.EMailTextBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|5a5cf702-3d07-4997-9a9e-5c03ec4c42cf", "Email");
			this.EMailTextBox.CharacterCasing = CharacterCasing.Normal;
			this.EMailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 42, true);
			this.EMailTextBox.Name = "EMailTextBox";
			this.EMailTextBox.ReadOnly = true;
			this.EMailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.EMailTextBox.TabIndex = 1;
			// 
			// JobOpeningGroupBox
			// 
			this.JobOpeningGroupBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|c93ee27a-2782-45d9-9ef5-e0831bf2d70a", "Job Opening");
			this.JobOpeningGroupBox.Controls.Add(this.JobOpeningGuidFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.SubmissionDateEdit);
			this.JobOpeningGroupBox.Controls.Add(this.AdTitleTextBox);
			this.JobOpeningGroupBox.Controls.Add(this.JobRoleGuidFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.StartDateEdit);
			this.JobOpeningGroupBox.Controls.Add(this.EndDateEdit);
			this.JobOpeningGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 167, true);
			this.JobOpeningGroupBox.Name = "JobOpeningGroupBox";
			this.JobOpeningGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 155, true);
			this.JobOpeningGroupBox.TabIndex = 2;
			this.JobOpeningGroupBox.TabStop = false;
			// 
			// JobOpeningGuidFindBox
			// 
			this.JobOpeningGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobOpeningGuidFindBox, "HP_HV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_HV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Lookups.Campaigns)));
			this.JobOpeningGuidFindBox.BindToList = "Lookups+Campaigns";
			this.JobOpeningGuidFindBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|809f01fc-c863-4efa-b4f0-a997a5b5b8f7", "Job Opening");
			this.JobOpeningGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.JobOpeningGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.JobOpeningGuidFindBox.ModuleID = ModuleIDs.HRJobOpenings;
			this.JobOpeningGuidFindBox.Name = "JobOpeningGuidFindBox";
			this.JobOpeningGuidFindBox.PreBoundMaxLength = 35;
			this.JobOpeningGuidFindBox.ShouldResize = true;
			this.JobOpeningGuidFindBox.ShowDescriptionBox = false;
			this.JobOpeningGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JobOpeningGuidFindBox.TabIndex = 5;
			// 
			// SubmissionDateEdit
			// 
			this.SubmissionDateEdit.AllowDrop = true;
			this.SubmissionDateEdit.AutoCompleteMonthThreshold = 1;
			this.SubmissionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SubmissionDateEdit, "SubmissionTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).SubmissionTimeLocal)));
			this.SubmissionDateEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|b4eb203f-8c15-489d-a029-d2ecbf1e2996", "Submission Date");
			this.SubmissionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 42, true);
			this.SubmissionDateEdit.Name = "SubmissionDateEdit";
			this.SubmissionDateEdit.TabIndex = 6;
			// 
			// AdTitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdTitleTextBox, "JobOpening.HV_AdTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).JobOpening.HV_AdTitle)));
			this.AdTitleTextBox.CaptionResourceString = null;
			this.AdTitleTextBox.CharacterCasing = CharacterCasing.Normal;
			this.AdTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.AdTitleTextBox.Name = "AdTitleTextBox";
			this.AdTitleTextBox.ReadOnly = true;
			this.AdTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.AdTitleTextBox.TabIndex = 7;
			// 
			// JobRoleGuidFindBox
			// 
			this.JobRoleGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobRoleGuidFindBox, "JobOpening.HV_HJ_JobRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).JobOpening.HV_HJ_JobRole)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).JobOpening.Lookups.JobRoles)));
			this.JobRoleGuidFindBox.BindToList = "JobOpening+Lookups+JobRoles";
			this.JobRoleGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.JobRoleGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 94, true);
			this.JobRoleGuidFindBox.ModuleID = ModuleIDs.HRJobRole;
			this.JobRoleGuidFindBox.Name = "JobRoleGuidFindBox";
			this.JobRoleGuidFindBox.PreBoundMaxLength = 15;
			this.JobRoleGuidFindBox.ShouldResize = true;
			this.JobRoleGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.JobRoleGuidFindBox.TabIndex = 8;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "JobOpening.HV_CampaignStartDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).JobOpening.HV_CampaignStartDateLocal)));
			this.StartDateEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|6a257996-f779-4415-9117-2f268b155774", "Start Date");
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 120, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 9;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "JobOpening.HV_CampaignEndDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).JobOpening.HV_CampaignEndDateLocal)));
			this.EndDateEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|5e0e9274-7dff-4446-b579-4d503c0db67a", "End Date");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 120, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 10;
			// 
			// ApplicantGroupBox
			// 
			this.ApplicantGroupBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|e5044eae-5d29-410c-b042-7f21b7e14974", "Applicant");
			this.ApplicantGroupBox.Controls.Add(this.ApplicantGuidFindBox);
			this.ApplicantGroupBox.Controls.Add(this.EMailTextBox);
			this.ApplicantGroupBox.Controls.Add(this.MobilePhoneNumberControl);
			this.ApplicantGroupBox.Controls.Add(this.CityTextBox);
			this.ApplicantGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.ApplicantGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.ApplicantGroupBox.Name = "ApplicantGroupBox";
			this.ApplicantGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 160, true);
			this.ApplicantGroupBox.TabIndex = 0;
			this.ApplicantGroupBox.TabStop = false;
			// 
			// ApplicantGuidFindBox
			// 
			this.ApplicantGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicantGuidFindBox, "HP_HA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_HA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Lookups.Applicants)));
			this.ApplicantGuidFindBox.BindToList = "Lookups+Applicants";
			this.ApplicantGuidFindBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|e9ed9b92-69f9-49b4-81d8-2800b0eb42fd", "Full Name");
			this.ApplicantGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ApplicantGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.ApplicantGuidFindBox.ModuleID = ModuleIDs.HRJobApplicant;
			this.ApplicantGuidFindBox.Name = "ApplicantGuidFindBox";
			this.ApplicantGuidFindBox.PreBoundMaxLength = 35;
			this.ApplicantGuidFindBox.ShouldResize = true;
			this.ApplicantGuidFindBox.ShowDescriptionBox = false;
			this.ApplicantGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ApplicantGuidFindBox.TabIndex = 0;
			// 
			// CityTextBox
			// 
			this.CityTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CityTextBox, "ApplicantCollection.HA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ApplicantCollection)).SyncRoot)).HA_City)));
			this.CityTextBox.CaptionResourceString = null;
			this.CityTextBox.CharacterCasing = CharacterCasing.Normal;
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 94, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.ReadOnly = true;
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.CityTextBox.TabIndex = 3;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "ApplicantCollection.HA_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ApplicantCollection)).SyncRoot)).HA_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Applicant.Lookups.Countries)));
			this.CountryCodeFindBox.BindToList = "Applicant+Lookups+Countries";
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 120, true);
			this.CountryCodeFindBox.ModuleID = ModuleIDs.RefCountry;
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ReadOnly = true;
			this.CountryCodeFindBox.ShouldResize = true;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CountryCodeFindBox.TabIndex = 4;
			// 
			// BottomTabControl
			// 
			this.BottomTabControl.Controls.Add(this.InterviewTabPage);
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 326, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1210, 190, true);
			this.BottomTabControl.TabIndex = 4;
			// 
			// InterviewTabPage
			// 
			this.InterviewTabPage.CaptionResourceString = Res.GetData("HRJobApplicationForm|8c941f7c-c26a-4681-934f-6d6c7a21a753", "Interviews");
			this.InterviewTabPage.Controls.Add(this.InterviewsGrid);
			this.InterviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InterviewTabPage.Name = "InterviewTabPage";
			this.InterviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 167, true);
			this.InterviewTabPage.TabIndex = 0;
			// 
			// InterviewsGrid
			// 
			this.InterviewsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InterviewsGrid, "Interviews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).HI_GS_NKInterviewer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).Lookups.Interviewers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).InterviewerName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).HI_InterviewTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).HI_InterviewLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).HI_InterviewStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicationInterview)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).Interviews)).SyncRoot)).Lookups.InterviewStatuses)));
			this.InterviewsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+Interviewers";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HI_GS_NKInterviewer";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobApplicationForm|8d2ce2ca-cd9e-4e4a-8043-be6dc43e7e3f", "Interviewer Name");
			zTextBoxColumnStyleInfo1.ColumnName = "InterviewerName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo1.ColumnName = "HI_InterviewTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "HI_InterviewLocation";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+InterviewStatuses";
			zDropEditColumnStyleInfo1.ColumnName = "HI_InterviewStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.InterviewsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InterviewsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InterviewsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InterviewsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InterviewsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InterviewsGrid.Dock = DockStyle.Fill;
			this.InterviewsGrid.GridId = "0b5b87c8-e185-4812-b672-0990ce402d1c";
			this.InterviewsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InterviewsGrid.LayoutKey = "InterviewsGrid";
			this.InterviewsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InterviewsGrid.Name = "InterviewsGrid";
			this.InterviewsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 167, true);
			this.InterviewsGrid.TabIndex = 15;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// ReferringSourceGroupBox
			// 
			this.ReferringSourceGroupBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|85a8c502-ba66-45eb-a26c-f4b8dbac0605", "Referring Source");
			this.ReferringSourceGroupBox.Controls.Add(this.FlowLayoutPanel1);
			this.ReferringSourceGroupBox.Controls.Add(this.ReferringSourceDropEdit);
			this.ReferringSourceGroupBox.Controls.Add(this.SourceDetailsTextBox);
			this.ReferringSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 3, true);
			this.ReferringSourceGroupBox.Name = "ReferringSourceGroupBox";
			this.ReferringSourceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 160, true);
			this.ReferringSourceGroupBox.TabIndex = 1;
			this.ReferringSourceGroupBox.TabStop = false;
			// 
			// FlowLayoutPanel1
			// 
			this.FlowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.FlowLayoutPanel1.Controls.Add(this.ReferringOrgGuidFindBox);
			this.FlowLayoutPanel1.Controls.Add(this.ReferringOrgGuidZGuidDropEdit);
			this.FlowLayoutPanel1.Controls.Add(this.ReferringPersonGuidFindBox);
			this.FlowLayoutPanel1.Controls.Add(this.ReferringStaffFindBox);
			this.FlowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
			this.FlowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 68, true);
			this.FlowLayoutPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			this.FlowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 78, true);
			this.FlowLayoutPanel1.TabIndex = 6;
			// 
			// ReferringOrgGuidFindBox
			// 
			this.ReferringOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringOrgGuidFindBox, "HP_OH_ReferringOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_OH_ReferringOrganisation)));
			this.ReferringOrgGuidFindBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|8b22f421-8f89-4180-b1d9-60adc7605d2a", "Referring Organization");
			this.ReferringOrgGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ReferringOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 0, true);
			this.ReferringOrgGuidFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(123, 0, 0, 8, true);
			this.ReferringOrgGuidFindBox.Name = "ReferringOrgGuidFindBox";
			this.ReferringOrgGuidFindBox.ShouldResize = true;
			this.ReferringOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ReferringOrgGuidFindBox.TabIndex = 6;
			// 
			// ReferringOrgGuidZGuidDropEdit
			// 
			this.ReferringOrgGuidZGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringOrgGuidZGuidDropEdit, "HP_OH_ReferringOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_OH_ReferringOrganisation)));
			this.ReferringOrgGuidZGuidDropEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|8b22f421-8f89-4180-b1d9-60adc7605d2a", "Referring Organization");
			this.ReferringOrgGuidZGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 0, true);
			this.ReferringOrgGuidZGuidDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(123, 0, 0, 8, true);
			this.ReferringOrgGuidZGuidDropEdit.Name = "ReferringOrgGuidZGuidDropEdit";
			this.ReferringOrgGuidZGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ReferringOrgGuidZGuidDropEdit.TabIndex = 6;
			// 
			// ReferringPersonGuidFindBox
			// 
			this.ReferringPersonGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringPersonGuidFindBox, "HP_PER_ReferringPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_PER_ReferringPerson)));
			this.ReferringPersonGuidFindBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|6c614a70-1702-4813-9df1-644dc696b452", "Referring Person");
			this.ReferringPersonGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ReferringPersonGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 28, true);
			this.ReferringPersonGuidFindBox.ModuleID = ModuleIDs.GlbPerson;
			this.ReferringPersonGuidFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(123, 0, 0, 0, true);
			this.ReferringPersonGuidFindBox.Name = "ReferringPersonGuidFindBox";
			this.ReferringPersonGuidFindBox.ShouldResize = true;
			this.ReferringPersonGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ReferringPersonGuidFindBox.TabIndex = 7;
			// 
			// ReferringStaffFindBox
			// 
			this.ReferringStaffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringStaffFindBox, "ReferringStaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).ReferringStaffCode)));
			this.ReferringStaffFindBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|e5dadb5e-834e-4e7c-af94-cf6c1ae6a584", "Referring Staff");
			this.ReferringStaffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 48, true);
			this.ReferringStaffFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(123, 0, 0, 0, true);
			this.ReferringStaffFindBox.Name = "ReferringStaffFindBox";
			this.ReferringStaffFindBox.ShouldResize = true;
			this.ReferringStaffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ReferringStaffFindBox.TabIndex = 7;
			// 
			// ReferringSourceDropEdit
			// 
			this.ReferringSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringSourceDropEdit, "HP_SourceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_SourceType)));
			this.ReferringSourceDropEdit.CaptionResourceString = Res.GetData("HRJobApplicationForm|dcf0f85a-70e4-4448-ba19-83c86ad91dee", "Source");
			this.ReferringSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.ReferringSourceDropEdit.Name = "ReferringSourceDropEdit";
			this.ReferringSourceDropEdit.PreBoundMaxLength = 5;
			this.ReferringSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ReferringSourceDropEdit.TabIndex = 4;
			// 
			// SourceDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SourceDetailsTextBox, "HP_SourceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(null)).HP_SourceDetails)));
			this.SourceDetailsTextBox.CaptionResourceString = Res.GetData("HRJobApplicationForm|bc15c4f6-8f0c-4dd8-ac3f-e279d3a2ea72", "Source Details");
			this.SourceDetailsTextBox.CharacterCasing = CharacterCasing.Normal;
			this.SourceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 42, true);
			this.SourceDetailsTextBox.Name = "SourceDetailsTextBox";
			this.SourceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.SourceDetailsTextBox.TabIndex = 5;
			// 
			// HRJobApplicationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 602, true);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplication);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 626, true);
			this.Name = "HRJobApplicationForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.ApplicationDetailsGroupBox.ResumeLayout(false);
			this.ApplicationDetailsGroupBox.PerformLayout();
			this.CurrentStatusDropEdit.ResumeLayout(true);
			this.CurrentStatusDropEdit.PerformLayout();
			this.AssignedToCodeFindBox.ResumeLayout(true);
			this.AssignedToCodeFindBox.PerformLayout();
			this.OverallRatingDropEdit.ResumeLayout(true);
			this.OverallRatingDropEdit.PerformLayout();
			this.MobilePhoneNumberControl.ResumeLayout(true);
			this.MobilePhoneNumberControl.PerformLayout();
			this.JobOpeningGroupBox.ResumeLayout(false);
			this.JobOpeningGroupBox.PerformLayout();
			this.JobOpeningGuidFindBox.ResumeLayout(true);
			this.JobOpeningGuidFindBox.PerformLayout();
			this.SubmissionDateEdit.ResumeLayout(true);
			this.SubmissionDateEdit.PerformLayout();
			this.JobRoleGuidFindBox.ResumeLayout(true);
			this.JobRoleGuidFindBox.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.ApplicantGroupBox.ResumeLayout(false);
			this.ApplicantGroupBox.PerformLayout();
			this.ApplicantGuidFindBox.ResumeLayout(true);
			this.ApplicantGuidFindBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.InterviewTabPage.ResumeLayout(false);
			this.InterviewTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InterviewsGrid)).EndInit();
			this.InterviewsGrid.ResumeLayout(false);
			this.InterviewsGrid.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ReferringSourceGroupBox.ResumeLayout(false);
			this.ReferringSourceGroupBox.PerformLayout();
			this.FlowLayoutPanel1.ResumeLayout(false);
			this.FlowLayoutPanel1.PerformLayout();
			this.ReferringOrgGuidFindBox.ResumeLayout(true);
			this.ReferringOrgGuidFindBox.PerformLayout();
			this.ReferringOrgGuidZGuidDropEdit.ResumeLayout(true);
			this.ReferringOrgGuidZGuidDropEdit.PerformLayout();
			this.ReferringPersonGuidFindBox.ResumeLayout(true);
			this.ReferringPersonGuidFindBox.PerformLayout();
			this.ReferringStaffFindBox.ResumeLayout(true);
			this.ReferringStaffFindBox.PerformLayout();
			this.ReferringSourceDropEdit.ResumeLayout(true);
			this.ReferringSourceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
