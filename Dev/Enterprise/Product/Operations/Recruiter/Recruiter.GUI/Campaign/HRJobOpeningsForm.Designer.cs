using System.ComponentModel;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruiter.GUI.Applicant;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobOpeningsForm
	{
		ZTabPage AdPlacementsTabPage;
		ZGrid AdPlacementGrid;
		ZCalcEdit PositionsFilledCalcEdit;
		ZCalcEdit PositionsAvailableCalcEdit;
		ZTextBox AdTitleTextBox;
		ZCalcEdit SalaryHighCalcEdit;
		ZCalcEdit SalaryLowCalcEdit;
		ZCodeFindBox RecruitmentCoordinatorCodeFindBox;
		ZCodeFindBox SalaryCodeFindBox;
		ZDateEdit EndDateEdit;
		ZDateEdit StartDateEdit;
		ZGuidFindBox JobRoleGuidFindBox;
		internal ZToolStripDropDownButton EmailApplicantsButton;
		ZToolStripButton EditApplicationButton;
		ZToolStrip buttonsToolStripLeft;
		internal ZToolStrip buttonsToolStripRight;
		ZGroupBox JobOpeningGroupBox;
		ZGuidDropEdit LocationContactGuidDropEdit;
		ZGuidDropEdit LocationAddressGuidDropEdit;
		ZGuidFindBox OrgGuidFindBox;
		protected internal ZTemplateTabControl BottomTabControl;
		internal ZTabPage ApplicantsTabPage;
		protected internal ZGrid ApplicationsGrid;
		HRContactPhoneDialUserControl ContactPhoneDiallerUserControl;
		ZToolStripButton NewApplicantButton;
		ZToolStripButton EditApplicantButton;
		ZToolStripButton AttachApplicantButton;
		ZToolStripButton DetachApplicantButton;
		public ZLabel StartTimeZoneLabel;
		public ZLabel EndTimeZoneLabel;
		ProcessTemplateCustomFieldsControl CustomFieldsControl;
		IContainer components = null;
		ZGroupBox CustomFieldsGroupBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HRJobOpeningsForm));
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ContactPhoneDiallerUserControl = new Enterprise.Recruiter.GUI.Applicant.HRContactPhoneDialUserControl();
			this.ApplicantsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ApplicationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.buttonsToolStripLeft = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.EditApplicationButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.EmailApplicantsButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.buttonsToolStripRight = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.NewApplicantButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.EditApplicantButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.AttachApplicantButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.DetachApplicantButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.AdPlacementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdPlacementGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EndTimeZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StartTimeZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PositionsFilledCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PositionsAvailableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AdTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SalaryHighCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SalaryLowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RecruitmentCoordinatorCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SalaryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JobRoleGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JobOpeningGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LocationAddressGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.LocationContactGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.BottomTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.CustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContactPhoneDiallerUserControl.SuspendLayout();
			this.ApplicantsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationsGrid)).BeginInit();
			this.ApplicationsGrid.SuspendLayout();
			this.AdPlacementsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdPlacementGrid)).BeginInit();
			this.AdPlacementGrid.SuspendLayout();
			this.RecruitmentCoordinatorCodeFindBox.SuspendLayout();
			this.SalaryCodeFindBox.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.JobRoleGuidFindBox.SuspendLayout();
			this.JobOpeningGroupBox.SuspendLayout();
			this.OrgGuidFindBox.SuspendLayout();
			this.LocationAddressGuidDropEdit.SuspendLayout();
			this.LocationContactGuidDropEdit.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.CustomFieldsControl.SuspendLayout();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.AdPlacementsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 417, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AdPlacementsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			//
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.CustomFieldsGroupBox);
			this.MainTabPage.Controls.Add(this.JobOpeningGroupBox);
			this.MainTabPage.Controls.Add(this.BottomTabControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 390, true);
			//
			// NotesTabPage
			//
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 390, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 390, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 417, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 24, true);
			this.MainStatusBar.SizingGrip = false;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(317);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(317);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRRecruitmentJobCampaign);
			//
			// ContactPhoneDiallerUserControl
			//
			this.ContactPhoneDiallerUserControl.AllowDrop = true;
			this.ContactPhoneDiallerUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.ContactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1021, 184, true);
			this.ContactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.ContactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.ContactPhoneDiallerUserControl.Name = "ContactPhoneDiallerUserControl";
			this.ContactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.ContactPhoneDiallerUserControl.TabIndex = 5;
			//
			// ApplicantsTabPage
			//
			this.ApplicantsTabPage.CaptionResourceString = Res.GetData("HRJobCampaignForm|6f16a52b-c04a-4e7e-a9e0-a2af681eab23", "Applicants");
			this.ApplicantsTabPage.Controls.Add(this.ApplicationsGrid);
			this.ApplicantsTabPage.Controls.Add(this.buttonsToolStripLeft);
			this.ApplicantsTabPage.Controls.Add(this.buttonsToolStripRight);
			this.ApplicantsTabPage.Controls.Add(this.ContactPhoneDiallerUserControl);
			this.ApplicantsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ApplicantsTabPage.Name = "ApplicantsTabPage";
			this.ApplicantsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1334, 207, true);
			this.ApplicantsTabPage.TabIndex = 4;
			//
			// ApplicationsGrid
			//
			this.ApplicationsGrid.AllowNavigation = false;
			this.ApplicationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right));
			this.BindingSource.SetBindingMember(this.ApplicationsGrid, "Applications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).SubmissionTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.HA_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.HA_EmailAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.HA_MobilePhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.HA_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.HA_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_GS_NKAssignedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Lookups.AssignedTos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_JobExperienceYears)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).ApplicationOverallRatingDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Applicant.CompulsorySkillRatingTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_CurrentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).Lookups.ApplicationStatuses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_SourceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_SourceDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_OH_ReferringOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).ReferringOrganisation.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_PER_ReferringPerson)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).ReferringStaffCode)));
			this.ApplicationsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobCampaignForm|93afebc1-d8e8-46d2-9d42-79de5f3139c4", "Submission Time");
			zDateEditColumnStyleInfo4.ColumnName = "SubmissionTimeLocal";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobCampaignForm|380bc8d0-ed7e-4d35-a88c-59d9292b7212", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Applicant+HA_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobCampaignForm|ed693b3e-77ac-4bf3-8cf0-a5e858e84fe2", "Email");
			zTextBoxColumnStyleInfo2.ColumnName = "Applicant+HA_EmailAddress";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("HRJobCampaignForm|5de36502-82b6-4f47-af62-f40d4e7807f9", "Mobile");
			zTextBoxColumnStyleInfo3.ColumnName = "Applicant+HA_MobilePhone";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobCampaignForm|3448da92-b87b-4d39-984d-751026bc1c64", "City");
			zTextBoxColumnStyleInfo4.ColumnName = "Applicant+HA_City";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Res.GetData("HRJobCampaignForm|e90121bb-b5d8-473f-899e-222e8729140c", "Country/Region");
			zTextBoxColumnStyleInfo5.ColumnName = "Applicant+HA_RN_NKCountry";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups+AssignedTos";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobCampaignForm|164209e1-793a-4129-b98a-d74c72b24f49", "Assigned To");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "HP_GS_NKAssignedTo";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.ModuleID = ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobCampaignForm|c61f48fe-c4e0-41eb-8782-c01434978ebb", "Yrs Experience");
			zCalcEditColumnStyleInfo2.ColumnName = "HP_JobExperienceYears";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobCampaignForm|fde68de4-9c9c-4393-a338-14190c41c5c9", "Overall Rating");
			zDropEditColumnStyleInfo2.ColumnName = "ApplicationOverallRatingDescription";
			zDropEditColumnStyleInfo2.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobCampaignForm|29e81a03-86eb-47cb-b49e-f384a2ffaeba", "Comp. Rating (%)", "Compulsory Skill Rating (%)", "");
			zCalcEditColumnStyleInfo4.ColumnName = "Applicant+CompulsorySkillRatingTotal";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.BindToList = "Lookups+ApplicationStatuses";
			zDropEditColumnStyleInfo3.CaptionResourceString = Res.GetData("HRJobCampaignForm|fb1699e9-dfe1-431a-a99d-e7513b8258bc", "Status");
			zDropEditColumnStyleInfo3.ColumnName = "HP_CurrentStatus";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobCampaignForm|3703a3ca-747b-48a7-aa0d-127b315df176", "Source");
			zDropEditColumnStyleInfo4.ColumnName = "HP_SourceType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Res.GetData("HRJobCampaignForm|01ec8bac-38c5-4a5d-943f-20cced3a7004", "Source Details");
			zTextBoxColumnStyleInfo6.ColumnName = "HP_SourceDetails";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobCampaignForm|ab32bcf8-9f91-4614-a401-ae468a092dcc", "Referring Org. Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "HP_OH_ReferringOrganisation";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Res.GetData("HRJobCampaignForm|5cc1b065-adfa-4f66-bb8e-0fc1af25b088", "Referring Org. Name");
			zTextBoxColumnStyleInfo7.ColumnName = "ReferringOrganisation+OH_FullName";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobCampaignForm|3fff8d89-b724-4366-a1a0-9bc16d475c8a", "Referring Person");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "HP_PER_ReferringPerson";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ModuleIDs.GlbPerson;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("HRJobCampaignForm|456b39e6-3561-4a8b-8568-f9047ba8c3e2", "Referring Staff");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ReferringStaffCode";
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.ModuleID = ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ApplicationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ApplicationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ApplicationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ApplicationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ApplicationsGrid.GridId = "219f2076-01d5-476f-91d1-b0adb8beddb2";
			this.ApplicationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicationsGrid.LayoutKey = "ApplicationsGrid";
			this.ApplicationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicationsGrid.Name = "ApplicationsGrid";
			this.ApplicationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1334, 178, true);
			this.ApplicationsGrid.TabIndex = 4;
			this.ApplicationsGrid.DoubleClick += new System.EventHandler(this.ApplicationGrid_DoubleClick);
			//
			// buttonsToolStripLeft
			//
			this.buttonsToolStripLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			this.buttonsToolStripLeft.BackColor = System.Drawing.Color.Transparent;
			this.buttonsToolStripLeft.Dock = DockStyle.None;
			this.buttonsToolStripLeft.GripStyle = ToolStripGripStyle.Hidden;
			this.buttonsToolStripLeft.Items.AddRange(
				new System.Windows.Forms.ToolStripItem[]
				{
					this.EditApplicationButton,
					this.EmailApplicantsButton
				});
			this.buttonsToolStripLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 178, true);
			this.buttonsToolStripLeft.Name = "buttonsToolStripLeft";
			this.buttonsToolStripLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 25, true);
			this.buttonsToolStripLeft.TabIndex = 5;
			this.buttonsToolStripLeft.Text = "zToolStrip1";
			//
			// EditApplicationButton
			//
			this.EditApplicationButton.CaptionResourceString = Res.GetData("1b7a21c6-24dc-4acd-9400-9a342656967c", "Edit Application");
			this.EditApplicationButton.Image = ((System.Drawing.Image)(resources.GetObject("EditApplicationButton.Image")));
			this.EditApplicationButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.EditApplicationButton.Name = "EditApplicationButton";
			this.EditApplicationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 22, true);
			this.EditApplicationButton.Click += new System.EventHandler(this.ApplicationEditButton_Click);
			//
			// EmailApplicantsButton
			//
			this.EmailApplicantsButton.CaptionResourceString = Res.GetData("HRJobCampaignForm|17bcbe0e-2ae3-425a-b0ff-be7bd3afc01c", "Email Applicants");
			this.EmailApplicantsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.EmailApplicantsButton.Name = "EmailApplicantsButton";
			this.EmailApplicantsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 22, true);
			//
			// buttonsToolStripRight
			//
			this.buttonsToolStripRight.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.buttonsToolStripRight.BackColor = System.Drawing.Color.Transparent;
			this.buttonsToolStripRight.Dock = DockStyle.None;
			this.buttonsToolStripRight.GripStyle = ToolStripGripStyle.Hidden;
			this.buttonsToolStripRight.Items.AddRange(
				new System.Windows.Forms.ToolStripItem[]
				{
					this.NewApplicantButton,
					this.EditApplicantButton,
					this.AttachApplicantButton,
					this.DetachApplicantButton
				});
			this.buttonsToolStripRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1107, 178, true);
			this.buttonsToolStripRight.Name = "buttonsToolStripRight";
			this.buttonsToolStripRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 25, true);
			this.buttonsToolStripRight.TabIndex = 5;
			this.buttonsToolStripRight.Text = "zToolStrip1";
			//
			// NewApplicantButton
			//
			this.NewApplicantButton.CaptionResourceString = Res.GetData("5570e7e0-990e-40f2-bb22-45998dba0624", "New");
			this.NewApplicantButton.Image = ((System.Drawing.Image)(resources.GetObject("NewApplicantButton.Image")));
			this.NewApplicantButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NewApplicantButton.Name = "NewApplicantButton";
			this.NewApplicantButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 22, true);
			this.NewApplicantButton.Click += new System.EventHandler(this.NewApplicantButton_Click);
			//
			// EditApplicantButton
			//
			this.EditApplicantButton.CaptionResourceString = Res.GetData("cacdb9a7-ceec-4236-902a-3150ec4305c8", "Edit");
			this.EditApplicantButton.Image = ((System.Drawing.Image)(resources.GetObject("EditApplicantButton.Image")));
			this.EditApplicantButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.EditApplicantButton.Name = "EditApplicantButton";
			this.EditApplicantButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 22, true);
			this.EditApplicantButton.Click += new System.EventHandler(this.EditApplicantButton_Click);
			//
			// AttachApplicantButton
			//
			this.AttachApplicantButton.CaptionResourceString = Res.GetData("f0834226-992e-4529-bf7b-7d5098279611", "Attach");
			this.AttachApplicantButton.Image = ((System.Drawing.Image)(resources.GetObject("AttachApplicantButton.Image")));
			this.AttachApplicantButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AttachApplicantButton.Name = "AttachApplicantButton";
			this.AttachApplicantButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 22, true);
			this.AttachApplicantButton.Click += new System.EventHandler(this.AttachApplicantButton_Click);
			//
			// DetachApplicantButton
			//
			this.DetachApplicantButton.CaptionResourceString = Res.GetData("3015f267-d486-43bb-ab35-e87fbd6d774b", "Detach");
			this.DetachApplicantButton.Image = ((System.Drawing.Image)(resources.GetObject("DetachApplicantButton.Image")));
			this.DetachApplicantButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DetachApplicantButton.Name = "DetachApplicantButton";
			this.DetachApplicantButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.DetachApplicantButton.Click += new System.EventHandler(this.DetachApplicantButton_Click);
			//
			// AdPlacementsTabPage
			//
			this.AdPlacementsTabPage.CaptionResourceString = Res.GetData("HRJobCampaignForm|973c80ef-83ec-4124-aeb8-9722a2e38c5f", "Ad Placements");
			this.AdPlacementsTabPage.Controls.Add(this.AdPlacementGrid);
			this.AdPlacementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdPlacementsTabPage.Name = "AdPlacementsTabPage";
			this.AdPlacementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 390, true);
			this.AdPlacementsTabPage.TabIndex = 5;
			//
			// AdPlacementGrid
			//
			this.AdPlacementGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdPlacementGrid, "AdPlacements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_AdBookedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_EffectiveStartDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_EffectiveEndDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_AdBookedIn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).Lookups.AdPlacementPublicationsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_AdCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_RX_NKAdCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).Lookups.AdCostCurrencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobAdPlacement)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).AdPlacements)).SyncRoot)).HQ_AdComments)));
			this.AdPlacementGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "HQ_AdBookedDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.CaptionResourceString = Res.GetData("AdPlacementGrid|d5b85546-c947-4813-878e-ceeC8dc26c30", "Start Date");
			zDateEditColumnStyleInfo2.ColumnName = "HQ_EffectiveStartDateLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo3.CaptionResourceString = Res.GetData("AdPlacementGrid|eb2447e1-ec57-41ca-a3d9-57ea8703ebe5", "End Date");
			zDateEditColumnStyleInfo3.ColumnName = "HQ_EffectiveEndDateLocal";
			zDateEditColumnStyleInfo3.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AdPlacementPublicationsList";
			zDropEditColumnStyleInfo1.ColumnName = "HQ_AdBookedIn";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "HQ_AdCost";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+AdCostCurrencies";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HQ_RX_NKAdCostCurrency";
			zCodeFindBoxColumnStyleInfo1.ModuleID = ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "HQ_AdComments";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AdPlacementGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AdPlacementGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AdPlacementGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.AdPlacementGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdPlacementGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AdPlacementGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdPlacementGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AdPlacementGrid.Dock = DockStyle.Fill;
			this.AdPlacementGrid.GridId = "42e092fe-7ced-46d3-8d8b-7ac402f25627";
			this.AdPlacementGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdPlacementGrid.LayoutKey = "AdPlacementGrid";
			this.AdPlacementGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdPlacementGrid.Name = "AdPlacementGrid";
			this.AdPlacementGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 390, true);
			this.AdPlacementGrid.TabIndex = 0;
			//
			// EndTimeZoneLabel
			//
			this.BindingSource.SetBindingMember(this.EndTimeZoneLabel, "CampaignEndDateTimeZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).CampaignEndDateTimeZone)));
			this.EndTimeZoneLabel.Cursor = Cursors.Default;
			this.EndTimeZoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((ZArchitecture.Core.OFontTypes.Normal | ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EndTimeZoneLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EndTimeZoneLabel, false);
			this.EndTimeZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 72, true);
			this.EndTimeZoneLabel.Name = "EndTimeZoneLabel";
			this.EndTimeZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.EndTimeZoneLabel.TabIndex = 37;
			//
			// StartTimeZoneLabel
			//
			this.BindingSource.SetBindingMember(this.StartTimeZoneLabel, "CampaignStartDateTimeZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).CampaignStartDateTimeZone)));
			this.StartTimeZoneLabel.Cursor = Cursors.Default;
			this.StartTimeZoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((ZArchitecture.Core.OFontTypes.Normal | ZArchitecture.Core.OFontTypes.SansSerif)));
			this.StartTimeZoneLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StartTimeZoneLabel, false);
			this.StartTimeZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 72, true);
			this.StartTimeZoneLabel.Name = "StartTimeZoneLabel";
			this.StartTimeZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.StartTimeZoneLabel.TabIndex = 36;
			//
			// PositionsFilledCalcEdit
			//
			this.BindingSource.SetBindingMember(this.PositionsFilledCalcEdit, "HV_NumberOfPositionsFilled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_NumberOfPositionsFilled)));
			this.PositionsFilledCalcEdit.CaptionResourceString = null;
			this.PositionsFilledCalcEdit.DecimalPlaces = 0;
			this.PositionsFilledCalcEdit.Decimals = 0;
			this.PositionsFilledCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 94, true);
			this.PositionsFilledCalcEdit.Name = "PositionsFilledCalcEdit";
			this.PositionsFilledCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PositionsFilledCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PositionsFilledCalcEdit.TabIndex = 5;
			this.PositionsFilledCalcEdit.Text = "0";
			this.PositionsFilledCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// PositionsAvailableCalcEdit
			//
			this.BindingSource.SetBindingMember(this.PositionsAvailableCalcEdit, "HV_NumberOfPositionsAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_NumberOfPositionsAvailable)));
			this.PositionsAvailableCalcEdit.CaptionResourceString = Res.GetData("HRJobCampaignForm|ebb92a1b-57e8-45a8-87a5-b7dce85bb892", "Positions Available");
			this.PositionsAvailableCalcEdit.DecimalPlaces = 0;
			this.PositionsAvailableCalcEdit.Decimals = 0;
			this.PositionsAvailableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 94, true);
			this.PositionsAvailableCalcEdit.Name = "PositionsAvailableCalcEdit";
			this.PositionsAvailableCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PositionsAvailableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PositionsAvailableCalcEdit.TabIndex = 4;
			this.PositionsAvailableCalcEdit.Text = "0";
			this.PositionsAvailableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// AdTitleTextBox
			//
			this.BindingSource.SetBindingMember(this.AdTitleTextBox, "HV_AdTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_AdTitle)));
			this.AdTitleTextBox.CaptionResourceString = null;
			this.AdTitleTextBox.CharacterCasing = CharacterCasing.Normal;
			this.AdTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.AdTitleTextBox.Name = "AdTitleTextBox";
			this.AdTitleTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AdTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 20, true);
			this.AdTitleTextBox.TabIndex = 0;
			//
			// SalaryHighCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SalaryHighCalcEdit, "HV_WageHigh");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_WageHigh)));
			this.SalaryHighCalcEdit.CaptionResourceString = Res.GetData("HRJobCampaignForm|95c8daab-e8be-4512-9791-e4f8e381bb6d", "To");
			this.SalaryHighCalcEdit.DecimalPlaces = 0;
			this.SalaryHighCalcEdit.Decimals = 0;
			this.SalaryHighCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 120, true);
			this.SalaryHighCalcEdit.Name = "SalaryHighCalcEdit";
			this.SalaryHighCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.SalaryHighCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.SalaryHighCalcEdit.TabIndex = 8;
			this.SalaryHighCalcEdit.Text = "0";
			this.SalaryHighCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// SalaryLowCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SalaryLowCalcEdit, "HV_WageLow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_WageLow)));
			this.SalaryLowCalcEdit.CaptionResourceString = Res.GetData("HRJobCampaignForm|e3deab11-fdd3-4bc2-afb1-b48c50affb77", "Salary Range");
			this.SalaryLowCalcEdit.DecimalPlaces = 0;
			this.SalaryLowCalcEdit.Decimals = 0;
			this.SalaryLowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 120, true);
			this.SalaryLowCalcEdit.Name = "SalaryLowCalcEdit";
			this.SalaryLowCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.SalaryLowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.SalaryLowCalcEdit.TabIndex = 7;
			this.SalaryLowCalcEdit.Text = "0";
			this.SalaryLowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// RecruitmentCoordinatorCodeFindBox
			//
			this.RecruitmentCoordinatorCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RecruitmentCoordinatorCodeFindBox, "HV_GS_NKControlledBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_GS_NKControlledBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.ControlledBys)));
			this.RecruitmentCoordinatorCodeFindBox.BindToList = "Lookups+ControlledBys";
			this.RecruitmentCoordinatorCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 16, true);
			this.RecruitmentCoordinatorCodeFindBox.ModuleID = ModuleIDs.GlbStaff;
			this.RecruitmentCoordinatorCodeFindBox.Name = "RecruitmentCoordinatorCodeFindBox";
			this.RecruitmentCoordinatorCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.RecruitmentCoordinatorCodeFindBox.ParentType = null;
			this.RecruitmentCoordinatorCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.RecruitmentCoordinatorCodeFindBox.TabIndex = 6;
			//
			// SalaryCodeFindBox
			//
			this.SalaryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalaryCodeFindBox, "HV_RX_NKWageRangeCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_RX_NKWageRangeCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.WageRangeCurrencies)));
			this.SalaryCodeFindBox.BindToList = "Lookups+WageRangeCurrencies";
			this.SalaryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 120, true);
			this.SalaryCodeFindBox.ModuleID = ModuleIDs.RefCurrency;
			this.SalaryCodeFindBox.Name = "SalaryCodeFindBox";
			this.SalaryCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.SalaryCodeFindBox.ParentType = null;
			this.SalaryCodeFindBox.PreBoundMaxLength = 3;
			this.SalaryCodeFindBox.ShowDescriptionBox = false;
			this.SalaryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.SalaryCodeFindBox.TabIndex = 9;
			//
			// EndDateEdit
			//
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "HV_CampaignEndDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_CampaignEndDateLocal)));
			this.EndDateEdit.CaptionResourceString = Res.GetData("HRJobCampaignForm|91d7419f-7e14-4017-9a2c-d39aeedf8f08", "End Date");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 68, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 3;
			//
			// StartDateEdit
			//
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "HV_CampaignStartDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_CampaignStartDateLocal)));
			this.StartDateEdit.CaptionResourceString = Res.GetData("HRJobCampaignForm|46efe49a-bfd8-4da6-bc8e-1ca415c78e82", "Start Date");
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 2;
			//
			// JobRoleGuidFindBox
			//
			this.JobRoleGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobRoleGuidFindBox, "HV_HJ_JobRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_HJ_JobRole)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.JobRoles)));
			this.JobRoleGuidFindBox.BindToList = "Lookups+JobRoles";
			this.JobRoleGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 42, true);
			this.JobRoleGuidFindBox.ModuleID = ModuleIDs.HRJobRole;
			this.JobRoleGuidFindBox.Name = "JobRoleGuidFindBox";
			this.JobRoleGuidFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.JobRoleGuidFindBox.ParentType = null;
			this.JobRoleGuidFindBox.PreBoundMaxLength = 15;
			this.JobRoleGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 20, true);
			this.JobRoleGuidFindBox.TabIndex = 1;
			//
			// JobOpeningGroupBox
			//
			this.JobOpeningGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)));
			this.JobOpeningGroupBox.CaptionResourceString = Res.GetData("HRJobCampaignForm|b36c5d1d-c9bb-43f2-8973-40a158e5eea8", "Job Opening");
			this.JobOpeningGroupBox.Controls.Add(this.AdTitleTextBox);
			this.JobOpeningGroupBox.Controls.Add(this.JobRoleGuidFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.StartDateEdit);
			this.JobOpeningGroupBox.Controls.Add(this.StartTimeZoneLabel);
			this.JobOpeningGroupBox.Controls.Add(this.EndDateEdit);
			this.JobOpeningGroupBox.Controls.Add(this.EndTimeZoneLabel);
			this.JobOpeningGroupBox.Controls.Add(this.PositionsAvailableCalcEdit);
			this.JobOpeningGroupBox.Controls.Add(this.PositionsFilledCalcEdit);
			this.JobOpeningGroupBox.Controls.Add(this.SalaryLowCalcEdit);
			this.JobOpeningGroupBox.Controls.Add(this.SalaryHighCalcEdit);
			this.JobOpeningGroupBox.Controls.Add(this.SalaryCodeFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.RecruitmentCoordinatorCodeFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.OrgGuidFindBox);
			this.JobOpeningGroupBox.Controls.Add(this.LocationAddressGuidDropEdit);
			this.JobOpeningGroupBox.Controls.Add(this.LocationContactGuidDropEdit);
			this.JobOpeningGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobOpeningGroupBox.Name = "JobOpeningGroupBox";
			this.JobOpeningGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 150, true);
			this.JobOpeningGroupBox.TabIndex = 30;
			this.JobOpeningGroupBox.TabStop = false;
			//
			// OrgGuidFindBox
			//
			this.OrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgGuidFindBox, "HV_OH_ClientAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_OH_ClientAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.ClientAccounts)));
			this.OrgGuidFindBox.BindToList = "Lookups+ClientAccounts";
			this.OrgGuidFindBox.CaptionResourceString = Res.GetData("HRJobCampaignForm|6d70398a-ef2d-4c98-862b-3d131dac452c", "Office Location");
			this.OrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 42, true);
			this.OrgGuidFindBox.Name = "OrgGuidFindBox";
			this.OrgGuidFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			this.OrgGuidFindBox.ParentType = null;
			this.OrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OrgGuidFindBox.TabIndex = 41;
			//
			// LocationAddressGuidDropEdit
			//
			this.LocationAddressGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationAddressGuidDropEdit, "HV_OA_ClientAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_OA_ClientAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.ClientAddresses)));
			this.LocationAddressGuidDropEdit.BindToList = "Lookups+ClientAddresses";
			this.LocationAddressGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 68, true);
			this.LocationAddressGuidDropEdit.Name = "LocationAddressGuidDropEdit";
			this.LocationAddressGuidDropEdit.PreBoundMaxLength = 35;
			this.LocationAddressGuidDropEdit.ShouldResizeByMaxLength = true;
			this.LocationAddressGuidDropEdit.ShowDescriptionBox = false;
			this.LocationAddressGuidDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LocationAddressGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.LocationAddressGuidDropEdit.TabIndex = 44;
			//
			// LocationContactGuidDropEdit
			//
			this.LocationContactGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationContactGuidDropEdit, "HV_OC_ClientContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).HV_OC_ClientContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Lookups.ClientContacts)));
			this.LocationContactGuidDropEdit.BindToList = "Lookups+ClientContacts";
			this.LocationContactGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 94, true);
			this.LocationContactGuidDropEdit.Name = "LocationContactGuidDropEdit";
			this.LocationContactGuidDropEdit.PreBoundMaxLength = 35;
			this.LocationContactGuidDropEdit.ShouldResizeByMaxLength = true;
			this.LocationContactGuidDropEdit.ShowDescriptionBox = false;
			this.LocationContactGuidDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LocationContactGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.LocationContactGuidDropEdit.TabIndex = 45;
			//
			// BottomTabControl
			//
			this.BottomTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom)));
			this.BottomTabControl.Controls.Add(this.ApplicantsTabPage);
			this.BottomTabControl.Dock = DockStyle.None;
			this.BottomTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 19, true);
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 234, true);
			this.BottomTabControl.TabIndex = 5;
			this.BottomTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.BottomTabControl_Selecting);
			//
			// CustomFieldsControl
			//
			this.CustomFieldsControl.AllowDrop = true;
			this.CustomFieldsControl.Dock = DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("927e779d-e3c1-4062-9d01-1b65fa471e6a", "To make use of these fields, please set up Human Resources Job Opening (HRJ) custom fields in Workflow Templates");
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 128, true);
			this.CustomFieldsControl.TabIndex = 31;
			//
			// CustomFieldsGroupBox
			//
			this.CustomFieldsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.CustomFieldsGroupBox.CaptionResourceString = Res.GetData("bf409aa4-e12d-4c68-a83b-29009399be22", "Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.CustomFieldsControl);
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(978, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 147, true);
			this.CustomFieldsGroupBox.TabIndex = 32;
			this.CustomFieldsGroupBox.TabStop = false;
			//
			// HRJobOpeningsForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("HRJobCampaignForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Job Opening");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 473, true);
			this.DataSourceAssemblyName = "Enterprise.Recruiter.Business";
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.HRRecruitmentJobCampaign);
			this.DataSourceTypeName = "Enterprise.Recruiter.Business.HRRecruitmentJobCampaign";
			this.Menu = null;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 488, true);
			this.Name = "HRJobOpeningsForm";
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
			this.ContactPhoneDiallerUserControl.ResumeLayout(true);
			this.ContactPhoneDiallerUserControl.PerformLayout();
			this.ApplicantsTabPage.ResumeLayout(false);
			this.ApplicantsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationsGrid)).EndInit();
			this.ApplicationsGrid.ResumeLayout(false);
			this.ApplicationsGrid.PerformLayout();
			this.AdPlacementsTabPage.ResumeLayout(false);
			this.AdPlacementsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdPlacementGrid)).EndInit();
			this.AdPlacementGrid.ResumeLayout(false);
			this.AdPlacementGrid.PerformLayout();
			this.RecruitmentCoordinatorCodeFindBox.ResumeLayout(true);
			this.RecruitmentCoordinatorCodeFindBox.PerformLayout();
			this.SalaryCodeFindBox.ResumeLayout(true);
			this.SalaryCodeFindBox.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.JobRoleGuidFindBox.ResumeLayout(true);
			this.JobRoleGuidFindBox.PerformLayout();
			this.JobOpeningGroupBox.ResumeLayout(false);
			this.JobOpeningGroupBox.PerformLayout();
			this.OrgGuidFindBox.ResumeLayout(true);
			this.OrgGuidFindBox.PerformLayout();
			this.LocationAddressGuidDropEdit.ResumeLayout(true);
			this.LocationAddressGuidDropEdit.PerformLayout();
			this.LocationContactGuidDropEdit.ResumeLayout(true);
			this.LocationContactGuidDropEdit.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.CustomFieldsControl.ResumeLayout(true);
			this.CustomFieldsControl.PerformLayout();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.CustomFieldsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
