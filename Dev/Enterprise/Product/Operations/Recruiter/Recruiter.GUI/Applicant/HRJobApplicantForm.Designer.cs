using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture;
using System.Windows.Forms;

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobApplicantForm
	{
		ZPanel zPanel2;
		ZGroupBox IdentificationGroupBox;
		Enterprise.ZArchitecture.ZTextBox DriversLicenseTextBox;
		Enterprise.ZArchitecture.ZTextBox PassportTextBox;
		ZCodeFindBox ISOCodeFindBox;
		ZTabPage CertificatesTabPage;
		CertificatesUserControl CertificatesUserControl;
		ZPanel zPanel1;
		ZToolStrip EditToolStrip;
		ZToolStripButton EditPersonButton;
		Enterprise.ZArchitecture.GUI.ZTabPage SkillsTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox RecruitmentInfoGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox PersonalInfoGroupBox;
		Enterprise.ZArchitecture.ZTextBox EMailTextBox;
		Enterprise.ZArchitecture.ZTextBox FullNameTextBox;
		Enterprise.ZArchitecture.ZTextBox TitleTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit WorkPermitStatusDropEdit;
		protected PhoneNumberUserControl HomePhoneNumberControl;
		protected PhoneNumberUserControl MobilePhoneNumberControl;
		Enterprise.ZArchitecture.ZTextBox NameSuffixTextBox;
		protected PhoneNumberUserControl FaxNumberControl;
		protected PhoneNumberUserControl WorkPhoneNumberControl;
		PhoneNumberUserControl WorkExtensionNumberControl;
		Enterprise.ZArchitecture.GUI.ZDropEdit AvailabilityDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox CurrentWageCalcFindBox;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox ExpectedWageCalcFindBox;
		protected Enterprise.ZArchitecture.ZGrid ApplicationsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ApplicationsTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel ApplicationsPanel;
		ZArchitecture.GUI.ZToolStrip buttonsToolStrip;
		ZArchitecture.GUI.ZToolStripButton EditButton;
		ZCodeFindBox NationalityCodeFindBox;
		ZDropEdit GenderDropEdit;
		ZDateEdit BirthDateDateEdit;
		ZCodeFindBox HA_CountryFindBox;
		ZTextBox HA_PostcodeBoundText;
		ZDropEditWithFixedWidth HA_StateBoundDropEdit;
		ZTextBox HA_CityBoundText;
		ZTextBox HA_UserAddress2BoundText;
		ZTextBox HA_UserAddress1BoundText;
		ZButton ValidateAddressButton;
		ZButton ClearFieldsButton;
		System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HRJobApplicantForm));
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HomePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.WorkPhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.EMailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.WorkExtensionNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.ApplicationsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApplicationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.buttonsToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.RecruitmentInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentWageCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.WorkPermitStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AvailabilityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExpectedWageCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.PersonalInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GenderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BirthDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameSuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SkillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IdentificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DriversLicenseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PassportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ISOCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CertificatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CertificatesUserControl = new Enterprise.MasterFiles.GUI.CertificatesUserControl();
			this.HA_CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HA_PostcodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.HA_StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.HA_CityBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.HA_UserAddress2BoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.HA_UserAddress1BoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EditToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.EditPersonButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.HomePhoneNumberControl.SuspendLayout();
			this.WorkPhoneNumberControl.SuspendLayout();
			this.MobilePhoneNumberControl.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.WorkExtensionNumberControl.SuspendLayout();
			this.ApplicationsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationsGrid)).BeginInit();
			this.ApplicationsGrid.SuspendLayout();
			this.RecruitmentInfoGroupBox.SuspendLayout();
			this.CurrentWageCalcFindBox.SuspendLayout();
			this.WorkPermitStatusDropEdit.SuspendLayout();
			this.AvailabilityDropEdit.SuspendLayout();
			this.ExpectedWageCalcFindBox.SuspendLayout();
			this.PersonalInfoGroupBox.SuspendLayout();
			this.GenderDropEdit.SuspendLayout();
			this.BirthDateDateEdit.SuspendLayout();
			this.ApplicationsTabPage.SuspendLayout();
			this.SkillsTabPage.SuspendLayout();
			this.EditToolStrip.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.IdentificationGroupBox.SuspendLayout();
			this.ISOCodeFindBox.SuspendLayout();
			this.CertificatesTabPage.SuspendLayout();
			this.CertificatesUserControl.SuspendLayout();
			this.NationalityCodeFindBox.SuspendLayout();
			this.HA_CountryFindBox.SuspendLayout();
			this.HA_StateBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.ApplicationsTabPage);
			this.MainTabControl.Controls.Add(this.SkillsTabPage);
			this.MainTabControl.Controls.Add(this.CertificatesTabPage);
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(861, 533, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CertificatesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.SkillsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ApplicationsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			//
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.zPanel2);
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			//
			// NotesTabPage
			//
			this.NotesTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = ControlDpiScalingHelper.NewScaledSize(861, 533, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(861, 24, true);
			this.MainStatusBar.SizingGrip = false;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplicant);
			//
			// ContactDetailsGroupBox
			//
			this.ContactDetailsGroupBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|278ae576-68b8-45c1-b1b6-5439c4f0338e", "Contact Details");
			this.ContactDetailsGroupBox.Controls.Add(this.HomePhoneNumberControl);
			this.ContactDetailsGroupBox.Controls.Add(this.WorkPhoneNumberControl);
			this.ContactDetailsGroupBox.Controls.Add(this.MobilePhoneNumberControl);
			this.ContactDetailsGroupBox.Controls.Add(this.EMailTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.FaxNumberControl);
			this.ContactDetailsGroupBox.Controls.Add(this.WorkExtensionNumberControl);
			this.ContactDetailsGroupBox.Dock = DockStyle.Top;
			this.ContactDetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(497, 194, true);
			this.ContactDetailsGroupBox.TabIndex = 1;
			this.ContactDetailsGroupBox.TabStop = false;
			//
			// HomePhoneNumberControl
			//
			this.HomePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomePhoneNumberControl, "HomePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HomePhoneNumber)));
			this.HomePhoneNumberControl.CaptionResourceString = Res.GetData("HRJobApplicantForm|999b9f5a-0cbf-448e-8b41-c97bf30b86a4", "Home");
			this.HomePhoneNumberControl.EnableValidStateColor = true;
			this.HomePhoneNumberControl.Location = ControlDpiScalingHelper.NewScaledPoint(32, 72, true);
			this.HomePhoneNumberControl.Name = "HomePhoneNumberControl";
			this.HomePhoneNumberControl.ShowPublishedCheckBox = false;
			this.HomePhoneNumberControl.Size = ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.HomePhoneNumberControl.TabIndex = 2;
			//
			// WorkPhoneNumberControl
			//
			this.WorkPhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkPhoneNumberControl, "WorkPhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).WorkPhoneNumber)));
			this.WorkPhoneNumberControl.CaptionResourceString = Res.GetData("HRJobApplicantForm|96a580d2-50ad-4772-b263-338667ceb0f6", "Work");
			this.WorkPhoneNumberControl.EnableValidStateColor = true;
			this.WorkPhoneNumberControl.Location = ControlDpiScalingHelper.NewScaledPoint(32, 120, true);
			this.WorkPhoneNumberControl.Name = "WorkPhoneNumberControl";
			this.WorkPhoneNumberControl.ShowPublishedCheckBox = false;
			this.WorkPhoneNumberControl.Size = ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.WorkPhoneNumberControl.TabIndex = 4;
			//
			// MobilePhoneNumberControl
			//
			this.MobilePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).MobilePhoneNumber)));
			this.MobilePhoneNumberControl.CaptionResourceString = Res.GetData("HRJobApplicantForm|5563d02e-9e51-4192-80b5-923c7766d42b", "Mobile");
			this.MobilePhoneNumberControl.EnableValidStateColor = true;
			this.MobilePhoneNumberControl.Location = ControlDpiScalingHelper.NewScaledPoint(32, 48, true);
			this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
			this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
			this.MobilePhoneNumberControl.Size = ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.MobilePhoneNumberControl.TabIndex = 1;
			//
			// EMailTextBox
			//
			this.BindingSource.SetBindingMember(this.EMailTextBox, "HA_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_EmailAddress)));
			this.EMailTextBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|e092d82b-d6ae-43f7-85f3-5b66b03f26d8", "Email");
			this.EMailTextBox.CharacterCasing = CharacterCasing.Normal;
			this.EMailTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			this.EMailTextBox.Name = "EMailTextBox";
			this.EMailTextBox.Size = ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.EMailTextBox.TabIndex = 0;
			//
			// FaxNumberControl
			//
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "FaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).FaxNumber)));
			this.FaxNumberControl.CaptionResourceString = Res.GetData("HRJobApplicantForm|fbf5ed8f-06aa-4650-95ef-4059e0ae73bb", "Fax");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = ControlDpiScalingHelper.NewScaledPoint(32, 96, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.Size = ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.FaxNumberControl.TabIndex = 3;
			//
			// WorkExtensionNumberControl
			//
			this.WorkExtensionNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkExtensionNumberControl, "WorkExtensionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).WorkExtensionNumber)));
			this.WorkExtensionNumberControl.CaptionResourceString = Res.GetData("HRJobApplicantForm|d594b05b-c263-45db-85f7-e377f786fa9c", "Extension");
			this.WorkExtensionNumberControl.Location = ControlDpiScalingHelper.NewScaledPoint(32, 144, true);
			this.WorkExtensionNumberControl.Name = "WorkExtensionNumberControl";
			this.WorkExtensionNumberControl.ShowDiallerControl = false;
			this.WorkExtensionNumberControl.ShowLocalNumberLabel = false;
			this.WorkExtensionNumberControl.ShowPublishedCheckBox = false;
			this.WorkExtensionNumberControl.Size = ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.WorkExtensionNumberControl.TabIndex = 5;
			//
			// ApplicationsPanel
			//
			this.ApplicationsPanel.Controls.Add(this.ApplicationsGrid);
			this.ApplicationsPanel.Controls.Add(this.buttonsToolStrip);
			this.ApplicationsPanel.Dock = DockStyle.Fill;
			this.ApplicationsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicationsPanel.Name = "ApplicationsPanel";
			this.ApplicationsPanel.Size = ControlDpiScalingHelper.NewScaledSize(856, 300, true);
			this.ApplicationsPanel.TabIndex = 0;
			//
			// ApplicationsGrid
			//
			this.ApplicationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApplicationsGrid, "Applications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).HP_HV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).Lookups.Campaigns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).JobOpening.HV_AdTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).JobOpening.CampaignLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).HP_GS_NKAssignedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).Lookups.AssignedTos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).HP_JobExperienceYears)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).HP_CurrentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).Lookups.ApplicationStatuses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).ApplicationOverallRatingDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Applications)).SyncRoot)).SubmissionTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_SourceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_SourceDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_OH_ReferringOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).HP_PER_ReferringPerson)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplication)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRRecruitmentJobCampaign)(null)).Applications)).SyncRoot)).ReferringStaffCode)));
			this.ApplicationsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+Campaigns";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobApplicantForm|f14e88f1-4649-4484-9b68-ec50ac812ff3", "Campaign ID");
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "HP_HV";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ModuleIDs.HRJobOpenings;
			zGuidFindBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "JobOpening+HV_AdTitle";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobApplicantForm|4f0cf4df-c41d-42d9-93b7-a4f143449867", "Location");
			zTextBoxColumnStyleInfo4.ColumnName = "JobOpening+CampaignLocation";
			zTextBoxColumnStyleInfo4.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups+AssignedTos";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "HP_GS_NKAssignedTo";
			zCodeFindBoxColumnStyleInfo2.ModuleID = ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "HP_JobExperienceYears";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+ApplicationStatuses";
			zDropEditColumnStyleInfo2.ColumnName = "HP_CurrentStatus";
			zDropEditColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.CaptionResourceString = Res.GetData("HRJobApplicantForm|b6882a93-8ae4-4680-a690-ad5bbefc3641", "Overall Rating");
			zDropEditColumnStyleInfo5.ColumnName = "ApplicationOverallRatingDescription";
			zDropEditColumnStyleInfo5.ToolTip = "You may enter a Rating for this applicant that is not based on their skill rating" +
	"s";
			zDropEditColumnStyleInfo5.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDateEditColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobApplicantForm|2d5c97af-5f95-4bc4-a3b6-e013d542225b", "Submission Time");
			zDateEditColumnStyleInfo2.ColumnName = "SubmissionTimeLocal";
			zDateEditColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Res.GetData("HRJobApplicantForm|a0c10943-5b6d-415a-87ec-a52d8ccbecfe", "Source");
			zDropEditColumnStyleInfo4.ColumnName = "HP_SourceType";
			zDropEditColumnStyleInfo4.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Res.GetData("HRJobApplicantForm|6d91e843-5469-416e-ad9c-2e7fd51403e5", "Source Details");
			zTextBoxColumnStyleInfo8.ColumnName = "HP_SourceDetails";
			zTextBoxColumnStyleInfo8.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobApplicantForm|3b251ef1-4400-4ec5-9f92-344a5e795703", "Referring Org. Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "HP_OH_ReferringOrganisation";
			zOrganisationFindBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Res.GetData("HRJobApplicantForm|05b16702-87e1-48f3-9d2b-911bcecdc941", "Referring Org. Name");
			zTextBoxColumnStyleInfo9.ColumnName = "ReferringOrganisation+OH_FullName";
			zTextBoxColumnStyleInfo9.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("HRJobApplicantForm|92282ba4-d969-46ac-8ba8-73f6f2a7c609", "Referring Person");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "HP_PER_ReferringPerson";
			zGuidFindBoxColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo2.ModuleID = ModuleIDs.GlbPerson;
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("HRJobApplicantForm|b7cf710b-5d17-4fc1-9fd7-793c43832976", "Referring Staff");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ReferringStaffCode";
			zCodeFindBoxColumnStyleInfo3.ModuleID = ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo3.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			this.ApplicationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ApplicationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ApplicationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ApplicationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ApplicationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ApplicationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ApplicationsGrid.Dock = DockStyle.Top;
			this.ApplicationsGrid.GridId = "be6c8bdd-13a6-4d15-bef7-5987baeb2974";
			this.ApplicationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicationsGrid.LayoutKey = "ApplicationsGrid";
			this.ApplicationsGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicationsGrid.Name = "ApplicationsGrid";
			this.ApplicationsGrid.Size = ControlDpiScalingHelper.NewScaledSize(856, 214, true);
			this.ApplicationsGrid.TabIndex = 0;
			this.ApplicationsGrid.DoubleClick += new System.EventHandler(this.ApplicationsGrid_DoubleClick);
			//
			// buttonsToolStrip
			//
			this.buttonsToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.buttonsToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.buttonsToolStrip.Dock = DockStyle.None;
			this.buttonsToolStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.buttonsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.EditButton });
			this.buttonsToolStrip.Location = ControlDpiScalingHelper.NewScaledPoint(735, 220, true);
			this.buttonsToolStrip.Name = "buttonsToolStrip";
			this.buttonsToolStrip.Size = ControlDpiScalingHelper.NewScaledSize(112, 21, true);
			this.buttonsToolStrip.TabIndex = 1;
			this.buttonsToolStrip.Text = "zToolStrip1";
			//
			// EditButton
			//
			this.EditButton.CaptionResourceString = Res.GetData("1b7a21c6-24dc-4acd-9400-9a342656967c", "Edit Application");
			this.EditButton.Image = ((System.Drawing.Image)(resources.GetObject("EditButton.Image")));
			this.EditButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = ControlDpiScalingHelper.NewScaledSize(54, 22, true);
			this.EditButton.Alignment = ToolStripItemAlignment.Right;
			this.EditButton.Click += new System.EventHandler(this.ApplicationEditButton_Click);
			//
			// RecruitmentInfoGroupBox
			//
			this.RecruitmentInfoGroupBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|0a7b6f21-28ba-4608-b02b-2fa520c6df41", "Recruitment Info");
			this.RecruitmentInfoGroupBox.Controls.Add(this.CurrentWageCalcFindBox);
			this.RecruitmentInfoGroupBox.Controls.Add(this.WorkPermitStatusDropEdit);
			this.RecruitmentInfoGroupBox.Controls.Add(this.AvailabilityDropEdit);
			this.RecruitmentInfoGroupBox.Controls.Add(this.ExpectedWageCalcFindBox);
			this.RecruitmentInfoGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(3, 321, true);
			this.RecruitmentInfoGroupBox.Name = "RecruitmentInfoGroupBox";

			/* Unmerged change from project 'Enterprise.Recruiter.GUI.Winzor'
			Before:
						this.RecruitmentInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 132, true);
			After:
						this.RecruitmentInfoGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(351, 132, true);
			*/
			this.RecruitmentInfoGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(351, 132, true);
			this.RecruitmentInfoGroupBox.TabIndex = 3;
			this.RecruitmentInfoGroupBox.TabStop = false;
			//
			// CurrentWageCalcFindBox
			//
			this.CurrentWageCalcFindBox.AllowDrop = true;
			this.CurrentWageCalcFindBox.BindToAmount = "HA_CurrentWage";
			this.CurrentWageCalcFindBox.BindToList = "Lookups+CurrentWageCurrencies";
			this.CurrentWageCalcFindBox.BindToUnit = "HA_RX_NKCurrentWageCurrency";
			this.CurrentWageCalcFindBox.Decimals = 0;
			this.CurrentWageCalcFindBox.FindBoxType = FindBoxType.Code;
			this.CurrentWageCalcFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(136, 72, true);
			this.CurrentWageCalcFindBox.ModuleID = ModuleIDs.RefCurrency;
			this.CurrentWageCalcFindBox.Name = "CurrentWageCalcFindBox";
			this.CurrentWageCalcFindBox.Size = ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.CurrentWageCalcFindBox.TabIndex = 2;
			//
			// WorkPermitStatusDropEdit
			//
			this.WorkPermitStatusDropEdit.AllowDrop = true;
			this.WorkPermitStatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WorkPermitStatusDropEdit, "HA_WorkPermitStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_WorkPermitStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Lookups.WorkPermitStatuses)));
			this.WorkPermitStatusDropEdit.BindToList = "Lookups+WorkPermitStatuses";
			this.WorkPermitStatusDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.WorkPermitStatusDropEdit.Name = "WorkPermitStatusDropEdit";
			this.WorkPermitStatusDropEdit.PreBoundMaxLength = 3;
			this.WorkPermitStatusDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.WorkPermitStatusDropEdit.TabIndex = 0;
			//
			// AvailabilityDropEdit
			//
			this.AvailabilityDropEdit.AllowDrop = true;
			this.AvailabilityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AvailabilityDropEdit, "HA_Availability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Availability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Lookups.Availabilities)));
			this.AvailabilityDropEdit.BindToList = "Lookups+Availabilities";
			this.AvailabilityDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(136, 48, true);
			this.AvailabilityDropEdit.Name = "AvailabilityDropEdit";
			this.AvailabilityDropEdit.PreBoundMaxLength = 3;

			/* Unmerged change from project 'Enterprise.Recruiter.GUI.Winzor'
			Before:
						this.AvailabilityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			After:
						this.AvailabilityDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			*/
			this.AvailabilityDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.AvailabilityDropEdit.TabIndex = 1;
			//
			// ExpectedWageCalcFindBox
			//
			this.ExpectedWageCalcFindBox.AllowDrop = true;
			this.ExpectedWageCalcFindBox.BindToAmount = "HA_WageExpectation";
			this.ExpectedWageCalcFindBox.BindToList = "Lookups+CurrentWageCurrencies";
			this.ExpectedWageCalcFindBox.BindToUnit = "HA_RX_NKWageExpectationCurrency";
			this.ExpectedWageCalcFindBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|bab2e6f0-6e4e-49a9-8348-92007cd1289d", "Expected Wage");
			this.ExpectedWageCalcFindBox.Decimals = 0;
			this.ExpectedWageCalcFindBox.FindBoxType = FindBoxType.Code;
			this.ExpectedWageCalcFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(136, 96, true);
			this.ExpectedWageCalcFindBox.ModuleID = ModuleIDs.RefCurrency;
			this.ExpectedWageCalcFindBox.Name = "ExpectedWageCalcFindBox";
			this.ExpectedWageCalcFindBox.Size = ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.ExpectedWageCalcFindBox.TabIndex = 3;
			//
			// PersonalInfoGroupBox
			//
			this.PersonalInfoGroupBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|c5e005bd-359a-4867-8dd2-dd2a70e032fc", "Personal Info");
			this.PersonalInfoGroupBox.Controls.Add(this.GenderDropEdit);
			this.PersonalInfoGroupBox.Controls.Add(this.BirthDateDateEdit);
			this.PersonalInfoGroupBox.Controls.Add(this.FullNameTextBox);
			this.PersonalInfoGroupBox.Controls.Add(this.TitleTextBox);
			this.PersonalInfoGroupBox.Controls.Add(this.NameSuffixTextBox);
			this.PersonalInfoGroupBox.Controls.Add(this.NationalityCodeFindBox);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_CountryFindBox);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_PostcodeBoundText);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_StateBoundDropEdit);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_CityBoundText);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_UserAddress2BoundText);
			this.PersonalInfoGroupBox.Controls.Add(this.HA_UserAddress1BoundText);
			this.PersonalInfoGroupBox.Controls.Add(this.ValidateAddressButton);
			this.PersonalInfoGroupBox.Controls.Add(this.ClearFieldsButton);
			this.PersonalInfoGroupBox.Controls.Add(this.EditToolStrip);
			this.PersonalInfoGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PersonalInfoGroupBox.Name = "PersonalInfoGroupBox";
			this.PersonalInfoGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(351, 310, true);
			this.PersonalInfoGroupBox.TabIndex = 0;
			this.PersonalInfoGroupBox.TabStop = false;
			//
			// EditToolStrip
			//
			this.EditToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.EditToolStrip.Anchor = AnchorStyles.None;
			this.EditToolStrip.Dock = DockStyle.None;
			this.EditToolStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.EditToolStrip.ImageScalingSize = ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.EditToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.EditPersonButton });
			this.EditToolStrip.Location = ControlDpiScalingHelper.NewScaledPoint(309, 46, true);
			this.EditToolStrip.Name = "EditToolStrip";
			this.EditToolStrip.Size = ControlDpiScalingHelper.NewScaledSize(25, 25, true);
			this.EditToolStrip.AutoSize = false;
			this.EditToolStrip.TabIndex = 23;
			//
			// EditPersonButton
			//
			this.EditPersonButton.Image = ((System.Drawing.Image)(resources.GetObject("EditPersonButton.Image")));
			this.EditPersonButton.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.EditPersonButton.ImageScaling = ToolStripItemImageScaling.None;
			this.EditPersonButton.Name = "EditPersonButton";
			this.EditPersonButton.Size = ControlDpiScalingHelper.NewScaledSize(21, 21);
			this.EditPersonButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EditPersonButton.Click += new System.EventHandler(this.EditPersonButton_Click);
			this.EditPersonButton.AutoSize = false;
			//
			// NationalityCodeFindBox
			//
			this.NationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalityCodeFindBox, "HA_RN_NKNationalityCodeISO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_RN_NKNationalityCodeISO)));
			this.NationalityCodeFindBox.CaptionResourceString = Res.GetData("e28ce816-7395-4d2a-9fbe-a3f82c36d5de", "Nationality");
			this.NationalityCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(96, 216, true);
			this.NationalityCodeFindBox.Name = "NationalityCodeFindBox";
			this.NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.NationalityCodeFindBox.ShouldResize = true;
			this.NationalityCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.NationalityCodeFindBox.TabIndex = 28;
			//
			// GenderDropEdit
			//
			this.GenderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GenderDropEdit, "HA_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Gender)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Lookups.Genders)));
			this.GenderDropEdit.BindToList = "Lookups+Genders";
			this.GenderDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(260, 192, true);
			this.GenderDropEdit.PreBoundMaxLength = 1;
			this.GenderDropEdit.Name = "GenderDropEdit";
			this.GenderDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.GenderDropEdit.TabIndex = 27;
			//
			// BirthDateDateEdit
			//
			this.BirthDateDateEdit.AllowDrop = true;
			this.BirthDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BirthDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BirthDateDateEdit, "HA_Birthdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Birthdate)));
			this.BirthDateDateEdit.CaptionResourceString = Res.GetData("HRJobApplicantForm|53de5003-7f05-4644-8d70-43886b0e9f32", "Date of Birth");
			this.BirthDateDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(96, 192, true);
			this.BirthDateDateEdit.Name = "BirthDateDateEdit";
			this.BirthDateDateEdit.TabIndex = 26;
			//
			// HA_CountryFindBox
			//
			this.HA_CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HA_CountryFindBox, "HA_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_RN_NKCountry)));
			this.HA_CountryFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(96, 144, true);
			this.HA_CountryFindBox.Name = "HA_CountryFindBox";
			this.HA_CountryFindBox.PreBoundMaxLength = 3;
			this.HA_CountryFindBox.ShouldResize = true;
			this.HA_CountryFindBox.ShowDescriptionBox = false;
			this.HA_CountryFindBox.Size = ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.HA_CountryFindBox.TabIndex = 22;
			//
			// HA_PostcodeBoundText
			//
			this.BindingSource.SetBindingMember(this.HA_PostcodeBoundText, "HA_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Postcode)));
			this.HA_PostcodeBoundText.CharacterCasing = CharacterCasing.Normal;
			this.HA_PostcodeBoundText.Location = ControlDpiScalingHelper.NewScaledPoint(96, 168, true);
			this.HA_PostcodeBoundText.Name = "HA_PostcodeBoundText";
			this.HA_PostcodeBoundText.Size = ControlDpiScalingHelper.NewScaledSize(82, 18, true);
			this.HA_PostcodeBoundText.TabIndex = 23;
			//
			// HA_StateBoundDropEdit
			//
			this.HA_StateBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HA_StateBoundDropEdit, "HA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_State)));
			this.HA_StateBoundDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(230, 168, true);
			this.HA_StateBoundDropEdit.Name = "HA_StateBoundDropEdit";
			this.HA_StateBoundDropEdit.PreBoundMaxLength = 4;
			this.HA_StateBoundDropEdit.ShowDescriptionBox = false;
			this.HA_StateBoundDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(70, 18, true);
			this.HA_StateBoundDropEdit.TabIndex = 25;
			//
			// HA_CityBoundText
			//
			this.BindingSource.SetBindingMember(this.HA_CityBoundText, "HA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_City)));
			this.HA_CityBoundText.CharacterCasing = CharacterCasing.Normal;
			this.HA_CityBoundText.Location = ControlDpiScalingHelper.NewScaledPoint(230, 144, true);
			this.HA_CityBoundText.Name = "HA_CityBoundText";
			this.HA_CityBoundText.Size = ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.HA_CityBoundText.TabIndex = 24;
			//
			// HA_UserAddress2BoundText
			//
			this.BindingSource.SetBindingMember(this.HA_UserAddress2BoundText, "HA_UserAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_UserAddress2)));
			this.HA_UserAddress2BoundText.CaptionResourceString = Res.GetData("0e52be9a-5ecc-4dc0-946f-99c76e82457b", "Address line 2");
			this.HA_UserAddress2BoundText.CharacterCasing = CharacterCasing.Normal;
			this.HA_UserAddress2BoundText.Location = ControlDpiScalingHelper.NewScaledPoint(96, 120, true);
			this.HA_UserAddress2BoundText.Name = "HA_UserAddress2BoundText";
			this.HA_UserAddress2BoundText.Size = ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.HA_UserAddress2BoundText.TabIndex = 21;
			//
			// HA_UserAddress1BoundText
			//
			this.BindingSource.SetBindingMember(this.HA_UserAddress1BoundText, "HA_UserAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_UserAddress1)));
			this.HA_UserAddress1BoundText.CaptionResourceString = Res.GetData("61470a6a-1cc1-49ea-b382-227ac48d9b3b", "Address line 1");
			this.HA_UserAddress1BoundText.CharacterCasing = CharacterCasing.Normal;
			this.HA_UserAddress1BoundText.Location = ControlDpiScalingHelper.NewScaledPoint(96, 96, true);
			this.HA_UserAddress1BoundText.Name = "HA_UserAddress1BoundText";
			this.HA_UserAddress1BoundText.Size = ControlDpiScalingHelper.NewScaledSize(192, 18, true);
			this.HA_UserAddress1BoundText.TabIndex = 20;
			//
			// ValidateAddressButton
			//
			this.ValidateAddressButton.Location = ControlDpiScalingHelper.NewScaledPoint(308, 95, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.Size = ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 32;
			this.ValidateAddressButton.TabStop = false;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += ValidateAddressButton_Click;
			//
			// ClearFieldsButton
			//
			this.ClearFieldsButton.Image = ((System.Drawing.Image)(resources.GetObject("xIcon")));
			this.ClearFieldsButton.Location = ControlDpiScalingHelper.NewScaledPoint(288, 95, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 33;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += ClearFieldsButton_Click;
			//
			// FullNameTextBox
			//
			this.BindingSource.SetBindingMember(this.FullNameTextBox, "HA_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_FullName)));
			this.FullNameTextBox.CharacterCasing = CharacterCasing.Normal;
			this.FullNameTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(96, 48, true);
			this.FullNameTextBox.Name = "FullNameTextBox";
			this.FullNameTextBox.Size = ControlDpiScalingHelper.NewScaledSize(212, 17, true);
			this.FullNameTextBox.TabIndex = 1;
			//
			// TitleTextBox
			//
			this.BindingSource.SetBindingMember(this.TitleTextBox, "HA_Title");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Title)));
			this.TitleTextBox.CaptionResourceString = Res.GetData("71ceb02f-eef5-41da-b405-c16aeacbc0b7", "Prefix Title");
			this.TitleTextBox.CharacterCasing = CharacterCasing.Normal;
			this.TitleTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(96, 24, true);
			this.TitleTextBox.Name = "TitleTextBox";
			this.TitleTextBox.Size = ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.TitleTextBox.TabIndex = 0;
			//
			// NameSuffixTextBox
			//
			this.BindingSource.SetBindingMember(this.NameSuffixTextBox, "HA_NameSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_NameSuffix)));
			this.NameSuffixTextBox.CharacterCasing = CharacterCasing.Normal;
			this.NameSuffixTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(96, 72, true);
			this.NameSuffixTextBox.Name = "NameSuffixTextBox";
			this.NameSuffixTextBox.Size = ControlDpiScalingHelper.NewScaledSize(212, 17, true);
			this.NameSuffixTextBox.TabIndex = 2;
			//
			// ApplicationsTabPage
			//
			this.ApplicationsTabPage.CaptionResourceString = Res.GetData("HRJobApplicantForm|a777ef44-3eb7-4d20-a2a4-d93cd3dc948f", "Applications");
			this.ApplicationsTabPage.Controls.Add(this.ApplicationsPanel);
			this.ApplicationsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ApplicationsTabPage.Name = "ApplicationsTabPage";
			this.ApplicationsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			this.ApplicationsTabPage.TabIndex = 3;
			//
			// SkillsTabPage
			//
			this.SkillsTabPage.CaptionResourceString = Res.GetData("HRJobApplicantForm|e3bfa464-8ff8-4712-9a1f-8e61934c681e", "Skills");
			this.SkillsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SkillsTabPage.Name = "SkillsTabPage";
			this.SkillsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			this.SkillsTabPage.TabIndex = 4;
			//
			// zPanel1
			//
			this.zPanel1.Controls.Add(this.PersonalInfoGroupBox);
			this.zPanel1.Controls.Add(this.RecruitmentInfoGroupBox);
			this.zPanel1.Dock = DockStyle.Left;
			this.zPanel1.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = ControlDpiScalingHelper.NewScaledSize(359, 511, true);
			this.zPanel1.TabIndex = 5;
			//
			// zPanel2
			//
			this.zPanel2.Controls.Add(this.IdentificationGroupBox);
			this.zPanel2.Controls.Add(this.ContactDetailsGroupBox);
			this.zPanel2.Dock = DockStyle.Fill;
			this.zPanel2.Location = ControlDpiScalingHelper.NewScaledPoint(359, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = ControlDpiScalingHelper.NewScaledSize(497, 511, true);
			this.zPanel2.TabIndex = 6;
			//
			// IdentificationGroupBox
			//
			this.IdentificationGroupBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|47e431ae-1088-4fb5-87a2-180375b0294e", "Identification");
			this.IdentificationGroupBox.Controls.Add(this.DriversLicenseTextBox);
			this.IdentificationGroupBox.Controls.Add(this.PassportTextBox);
			this.IdentificationGroupBox.Controls.Add(this.ISOCodeFindBox);
			this.IdentificationGroupBox.Dock = DockStyle.Top;
			this.IdentificationGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(0, 194, true);
			this.IdentificationGroupBox.Name = "IdentificationGroupBox";
			this.IdentificationGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(497, 126, true);
			this.IdentificationGroupBox.TabIndex = 5;
			this.IdentificationGroupBox.TabStop = false;
			//
			// DriversLicenseTextBox
			//
			this.BindingSource.SetBindingMember(this.DriversLicenseTextBox, "HA_DriversLicenseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_DriversLicenseNumber)));
			this.DriversLicenseTextBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|0b22ce52-bfe5-4d4d-8e56-be46b3e7012e", "Driver\'s License");
			this.DriversLicenseTextBox.CharacterCasing = CharacterCasing.Normal;
			this.DriversLicenseTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(104, 48, true);
			this.DriversLicenseTextBox.Name = "DriversLicenseTextBox";
			this.DriversLicenseTextBox.Size = ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.DriversLicenseTextBox.TabIndex = 1;
			//
			// PassportTextBox
			//
			this.BindingSource.SetBindingMember(this.PassportTextBox, "HA_Passport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_Passport)));
			this.PassportTextBox.CharacterCasing = CharacterCasing.Normal;
			this.PassportTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			this.PassportTextBox.Name = "PassportTextBox";
			this.PassportTextBox.Size = ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.PassportTextBox.TabIndex = 0;
			//
			// ISOCodeFindBox
			//
			this.ISOCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ISOCodeFindBox, "HA_RN_NKNationalityCodeISO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).HA_RN_NKNationalityCodeISO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)).Lookups.Countries)));
			this.ISOCodeFindBox.BindToList = "Lookups+Countries";
			this.ISOCodeFindBox.CaptionResourceString = Res.GetData("HRJobApplicantForm|24d6de28-fc0f-4a80-ac61-6781976be80e", "Nationality");
			this.ISOCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(104, 72, true);
			this.ISOCodeFindBox.ModuleID = ModuleIDs.RefCountry;
			this.ISOCodeFindBox.Name = "ISOCodeFindBox";
			this.ISOCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(176, 17, true);
			this.ISOCodeFindBox.TabIndex = 2;
			//
			// CertificatesTabPage
			//
			this.CertificatesTabPage.CaptionResourceString = Res.GetData("HRJobApplicantForm|47e431ae-1088-4fb5-87a2-180375b02941", "Certificates and ID Numbers");
			this.CertificatesTabPage.Controls.Add(this.CertificatesUserControl);
			this.CertificatesTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CertificatesTabPage.Name = "CertificatesTabPage";
			this.CertificatesTabPage.Size = ControlDpiScalingHelper.NewScaledSize(856, 511, true);
			this.CertificatesTabPage.TabIndex = 5;
			//
			// CertificatesUserControl
			//
			this.CertificatesUserControl.AllowDrop = true;
			this.CertificatesUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CertificatesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ICertificatesProvider)(((Enterprise.Recruiter.Business.HRJobApplicant)(null)))));
			this.CertificatesUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CertificatesUserControl.Name = "CertificatesUserControl";
			this.CertificatesUserControl.Size = ControlDpiScalingHelper.NewScaledSize(704, 197, true);
			this.CertificatesUserControl.TabIndex = 4;
			//
			// HRJobApplicantForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(861, 589, true);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplicant);
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(976, 626, true);
			this.Name = "HRJobApplicantForm";
			this.Load += new System.EventHandler(this.HRJobApplicantForm_Load);
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
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.HomePhoneNumberControl.ResumeLayout(true);
			this.HomePhoneNumberControl.PerformLayout();
			this.WorkPhoneNumberControl.ResumeLayout(true);
			this.WorkPhoneNumberControl.PerformLayout();
			this.MobilePhoneNumberControl.ResumeLayout(true);
			this.MobilePhoneNumberControl.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.WorkExtensionNumberControl.ResumeLayout(true);
			this.WorkExtensionNumberControl.PerformLayout();
			this.ApplicationsPanel.ResumeLayout(false);
			this.ApplicationsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationsGrid)).EndInit();
			this.ApplicationsGrid.ResumeLayout(false);
			this.ApplicationsGrid.PerformLayout();
			this.RecruitmentInfoGroupBox.ResumeLayout(false);
			this.RecruitmentInfoGroupBox.PerformLayout();
			this.CurrentWageCalcFindBox.ResumeLayout(true);
			this.CurrentWageCalcFindBox.PerformLayout();
			this.WorkPermitStatusDropEdit.ResumeLayout(true);
			this.WorkPermitStatusDropEdit.PerformLayout();
			this.AvailabilityDropEdit.ResumeLayout(true);
			this.AvailabilityDropEdit.PerformLayout();
			this.ExpectedWageCalcFindBox.ResumeLayout(true);
			this.ExpectedWageCalcFindBox.PerformLayout();
			this.PersonalInfoGroupBox.ResumeLayout(false);
			this.PersonalInfoGroupBox.PerformLayout();
			this.NationalityCodeFindBox.ResumeLayout(true);
			this.NationalityCodeFindBox.PerformLayout();
			this.GenderDropEdit.ResumeLayout(true);
			this.GenderDropEdit.PerformLayout();
			this.BirthDateDateEdit.ResumeLayout(true);
			this.BirthDateDateEdit.PerformLayout();
			this.HA_CountryFindBox.ResumeLayout(true);
			this.HA_CountryFindBox.PerformLayout();
			this.HA_StateBoundDropEdit.ResumeLayout(true);
			this.HA_StateBoundDropEdit.PerformLayout();
			this.ApplicationsTabPage.ResumeLayout(false);
			this.ApplicationsTabPage.PerformLayout();
			this.SkillsTabPage.ResumeLayout(false);
			this.SkillsTabPage.PerformLayout();
			this.EditToolStrip.ResumeLayout(false);
			this.EditToolStrip.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.IdentificationGroupBox.ResumeLayout(false);
			this.IdentificationGroupBox.PerformLayout();
			this.ISOCodeFindBox.ResumeLayout(true);
			this.ISOCodeFindBox.PerformLayout();
			this.CertificatesTabPage.ResumeLayout(false);
			this.CertificatesTabPage.PerformLayout();
			this.CertificatesUserControl.ResumeLayout(true);
			this.CertificatesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
