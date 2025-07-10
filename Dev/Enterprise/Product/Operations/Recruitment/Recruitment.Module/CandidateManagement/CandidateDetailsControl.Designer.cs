using Enterprise.MarketingManager.GUI;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	partial class CandidateDetailsControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.pdfPreviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.documentPreview = new Enterprise.DocumentScanning.GUI.GraphicalDisplayControl();
			this.profileTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.sourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.alternatePhoneControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.mobileNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.stateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.cityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.submissionDateBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.experienceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.countryCodeBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.emailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.peopleAndCommunicationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.peopleAndCommunicationControl = new Enterprise.Recruitment.Module.CandidateManagement.PeopleAndCommunicationControl();
			this.edocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.eDocsControl = new Enterprise.MarketingManager.GUI.EdocsSwappableControl();
			this.eConversationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.eConversation = new Enterprise.EConversation.GUI.CandidateEConversationControl();
			this.notesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.notesControl = new Enterprise.ZArchitecture.GUI.ZStmNoteUserControl();
			this.logsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.logControl = new Enterprise.ZArchitecture.GUI.ZStmALogUserControl();
			this.tasksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.nameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.roleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HiringRequestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createWorkItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createWorkItemDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.detailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tabControl.SuspendLayout();
			this.pdfPreviewTabPage.SuspendLayout();
			this.documentPreview.SuspendLayout();
			this.profileTabPage.SuspendLayout();
			this.sourceDropEdit.SuspendLayout();
			this.alternatePhoneControl.SuspendLayout();
			this.mobileNumberControl.SuspendLayout();
			this.stateDropEdit.SuspendLayout();
			this.submissionDateBox.SuspendLayout();
			this.countryCodeBox.SuspendLayout();
			this.peopleAndCommunicationTabPage.SuspendLayout();
			this.peopleAndCommunicationControl.SuspendLayout();
			this.edocsTabPage.SuspendLayout();
			this.eDocsControl.SuspendLayout();
			this.eConversationTabPage.SuspendLayout();
			this.eConversation.SuspendLayout();
			this.notesTabPage.SuspendLayout();
			this.notesControl.SuspendLayout();
			this.logsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tasksGrid)).BeginInit();
			this.tasksGrid.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.createWorkItemDropDown.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.postingButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).BeginInit();
			this.detailsSplitContainer.Panel1.SuspendLayout();
			this.detailsSplitContainer.Panel2.SuspendLayout();
			this.detailsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruitment.Common.Candidate);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControl.Controls.Add(this.pdfPreviewTabPage);
			this.tabControl.Controls.Add(this.profileTabPage);
			this.tabControl.Controls.Add(this.peopleAndCommunicationTabPage);
			this.tabControl.Controls.Add(this.edocsTabPage);
			this.tabControl.Controls.Add(this.eConversationTabPage);
			this.tabControl.Controls.Add(this.notesTabPage);
			this.tabControl.Controls.Add(this.logsTabPage);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 549, true);
			this.tabControl.TabIndex = 0;
			// 
			// pdfPreviewTabPage
			// 
			this.pdfPreviewTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("d81903e8-6d57-4ad6-b8bf-474738669579", "CV");
			this.pdfPreviewTabPage.Controls.Add(this.documentPreview);
			this.pdfPreviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.pdfPreviewTabPage.Name = "pdfPreviewTabPage";
			this.pdfPreviewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.pdfPreviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.pdfPreviewTabPage.TabIndex = 0;
			// 
			// documentPreview
			// 
			this.documentPreview.AllowDrop = true;
			this.documentPreview.AllowRotate = false;
			this.documentPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.documentPreview.Document = null;
			this.documentPreview.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.documentPreview.Name = "documentPreview";
			this.documentPreview.ReadOnly = false;
			this.documentPreview.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.documentPreview.TabIndex = 0;
			// 
			// profileTabPage
			// 
			this.profileTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("0661b05b-d0fb-429f-aa4c-165df67683d6", "Profile");
			this.profileTabPage.Controls.Add(this.sourceDropEdit);
			this.profileTabPage.Controls.Add(this.alternatePhoneControl);
			this.profileTabPage.Controls.Add(this.mobileNumberControl);
			this.profileTabPage.Controls.Add(this.stateDropEdit);
			this.profileTabPage.Controls.Add(this.cityTextBox);
			this.profileTabPage.Controls.Add(this.submissionDateBox);
			this.profileTabPage.Controls.Add(this.experienceTextBox);
			this.profileTabPage.Controls.Add(this.countryCodeBox);
			this.profileTabPage.Controls.Add(this.nameTextBox);
			this.profileTabPage.Controls.Add(this.emailTextBox);
			this.profileTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.profileTabPage.Name = "profileTabPage";
			this.profileTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.profileTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.profileTabPage.TabIndex = 1;
			// 
			// sourceDropEdit
			// 
			this.sourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sourceDropEdit, "Application.HP_SourceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruitment.Common.Candidate)(null)).Application.HP_SourceType)));
			this.sourceDropEdit.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("9f5410ad-56e8-4ab2-877c-f41ce7e3b4df", "CV Source");
			this.sourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 162, true);
			this.sourceDropEdit.Name = "sourceDropEdit";
			this.sourceDropEdit.ShouldResizeByMaxLength = true;
			this.sourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.sourceDropEdit.TabIndex = 13;
			// 
			// alternatePhoneControl
			// 
			this.alternatePhoneControl.AllowDrop = true;
			this.alternatePhoneControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.alternatePhoneControl, "Applicant.HomePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HomePhoneNumber)));
			this.alternatePhoneControl.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("421c7247-e4ae-4ce8-85f8-62dc8db89978", "Alt. Phone", "Alternate Phone", "An alternate number to contact this person on");
			this.alternatePhoneControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 32, true);
			this.alternatePhoneControl.Name = "alternatePhoneControl";
			this.alternatePhoneControl.ShowPublishedCheckBox = false;
			this.alternatePhoneControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.alternatePhoneControl.TabIndex = 10;
			// 
			// mobileNumberControl
			// 
			this.mobileNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mobileNumberControl, "Applicant.MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.MobilePhoneNumber)));
			this.mobileNumberControl.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("b0a37df3-b3a8-4fb3-a9db-1723d79bae95", "Mobile");
			this.mobileNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.mobileNumberControl.Name = "mobileNumberControl";
			this.mobileNumberControl.ShowPublishedCheckBox = false;
			this.mobileNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.mobileNumberControl.TabIndex = 0;
			// 
			// stateDropEdit
			// 
			this.stateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.stateDropEdit, "Applicant.HA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_State)));
			this.stateDropEdit.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("ab97d5e6-3e5a-477c-969a-36e789cf676c", "State");
			this.stateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 110, true);
			this.stateDropEdit.Name = "stateDropEdit";
			this.stateDropEdit.ShouldResizeByMaxLength = true;
			this.stateDropEdit.ShowDescriptionBox = false;
			this.stateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.stateDropEdit.TabIndex = 8;
			// 
			// cityTextBox
			// 
			this.cityTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.cityTextBox, "Applicant.HA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_City)));
			this.cityTextBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("aae41ef2-a894-452c-879f-5003fc59d77b", "City");
			this.cityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 110, true);
			this.cityTextBox.Name = "cityTextBox";
			this.cityTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.cityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.cityTextBox.TabIndex = 7;
			// 
			// submissionDateBox
			// 
			this.submissionDateBox.AllowDrop = true;
			this.submissionDateBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.submissionDateBox.AutoCompleteMonthThreshold = 1;
			this.submissionDateBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.submissionDateBox, "Application.AppliedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruitment.Common.Candidate)(null)).Application.AppliedDate)));
			this.submissionDateBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("6167dcff-66fc-414f-bc3a-aed7f3b58a9d", "Last Submission");
			this.submissionDateBox.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.submissionDateBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 162, true);
			this.submissionDateBox.Name = "submissionDateBox";
			this.submissionDateBox.TabIndex = 11;
			// 
			// experienceTextBox
			// 
			this.experienceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.experienceTextBox, "PreviousExperienceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).PreviousExperienceDetails)));
			this.experienceTextBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("7b352279-a248-4949-8509-04b0bf6d0d0f", "Past Roles");
			this.experienceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.experienceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 188, true);
			this.experienceTextBox.Multiline = true;
			this.experienceTextBox.Name = "experienceTextBox";
			this.experienceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.experienceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.experienceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 118, true);
			this.experienceTextBox.TabIndex = 5;
			// 
			// countryCodeBox
			// 
			this.countryCodeBox.AllowDrop = true;
			this.countryCodeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.countryCodeBox, "Applicant.HA_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_RN_NKCountry)));
			this.countryCodeBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("2233f9f9-bbb3-4e83-a886-f76ae264e247", "Country/Region");
			this.countryCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 84, true);
			this.countryCodeBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.countryCodeBox.Name = "countryCodeBox";
			this.countryCodeBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.countryCodeBox.ParentType = null;
			this.countryCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			this.countryCodeBox.TabIndex = 3;
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nameTextBox, "Applicant.HA_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_FullName)));
			this.nameTextBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("7c6d78ab-d825-4243-8d79-7b2099dcd149", "Name");
			this.nameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 6, true);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			this.nameTextBox.TabIndex = 1;
			// 
			// emailTextBox
			// 
			this.emailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.emailTextBox, "Applicant.HA_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_EmailAddress)));
			this.emailTextBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("5ef5a333-34bb-4198-a8d7-501a9b33811d", "Email");
			this.emailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.emailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 58, true);
			this.emailTextBox.Name = "emailTextBox";
			this.emailTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.emailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			this.emailTextBox.TabIndex = 0;
			// 
			// peopleAndCommunicationTabPage
			// 
			this.peopleAndCommunicationTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("c5408d88-fe80-435c-90c6-ebb66762f910", "People && Communication");
			this.peopleAndCommunicationTabPage.Controls.Add(this.peopleAndCommunicationControl);
			this.peopleAndCommunicationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.peopleAndCommunicationTabPage.Name = "peopleAndCommunicationTabPage";
			this.peopleAndCommunicationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.peopleAndCommunicationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.peopleAndCommunicationTabPage.TabIndex = 3;
			// 
			// peopleAndCommunicationControl
			// 
			this.peopleAndCommunicationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.peopleAndCommunicationControl, "CommunicationContactRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Recruitment.Common.CommunicationContactRowBusinessObjectCollection)(((Enterprise.Recruitment.Common.Candidate)(null)).CommunicationContactRows)));
			this.peopleAndCommunicationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.peopleAndCommunicationControl.Enabled = false;
			this.peopleAndCommunicationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.peopleAndCommunicationControl.Name = "peopleAndCommunicationControl";
			this.peopleAndCommunicationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 516, true);
			this.peopleAndCommunicationControl.TabIndex = 0;
			// 
			// edocsTabPage
			// 
			this.edocsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.edocsTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("7c5051a1-61fe-4ab7-b77f-d4e4eab8ccc0", "eDocs");
			this.edocsTabPage.Controls.Add(this.eDocsControl);
			this.edocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.edocsTabPage.Name = "edocsTabPage";
			this.edocsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.edocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.edocsTabPage.TabIndex = 2;
			// 
			// eDocsControl
			// 
			this.eDocsControl.AllowDrop = true;
			this.eDocsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eDocsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.eDocsControl.Name = "eDocsControl";
			this.eDocsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 516, true);
			this.eDocsControl.TabIndex = 0;
			// 
			// eConversationTabPage
			// 
			this.eConversationTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("fca8d91d-7781-4c52-9f9e-f11493bcd30b", "eConversation");
			this.eConversationTabPage.Controls.Add(this.eConversation);
			this.eConversationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.eConversationTabPage.Name = "eConversationTabPage";
			this.eConversationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.eConversationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.eConversationTabPage.TabIndex = 0;
			// 
			// eConversation
			// 
			this.eConversation.AllowDrop = true;
			this.eConversation.AutoSize = true;
			this.BindingSource.SetBindingMember(this.eConversation, "EConversation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Recruiter.Business.GroupedEConversation)(((Enterprise.Recruitment.Common.Candidate)(null)).EConversation)));
			this.eConversation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversation.Enabled = false;
			this.eConversation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.eConversation.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 345, true);
			this.eConversation.Name = "eConversation";
			this.eConversation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 516, true);
			this.eConversation.TabIndex = 0;
			// 
			// notesTabPage
			// 
			this.notesTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("6EEFF229-9400-4786-9227-F2F5677219EE", "Notes");
			this.notesTabPage.Controls.Add(this.notesControl);
			this.notesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.notesTabPage.Name = "notesTabPage";
			this.notesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.notesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.notesTabPage.TabIndex = 0;
			// 
			// notesControl
			// 
			this.notesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.notesControl, "Application");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.Notes)(((Enterprise.Recruitment.Common.Candidate)(null)).Application.Notes)));
			this.notesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.notesControl.Name = "notesControl";
			this.notesControl.Enabled = false;
			this.notesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 516, true);
			this.notesControl.TabIndex = 0;
			// 
			// logsTabPage
			// 
			this.logsTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("51fc84db-353e-42fb-a5fb-af0d1d007f19", "Logs");
			this.logsTabPage.Controls.Add(this.logControl);
			this.logsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.logsTabPage.Name = "logsTabPage";
			this.logsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.logsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 522, true);
			this.logsTabPage.TabIndex = 4;
			this.logsTabPage.UseVisualStyleBackColor = true;
			// 
			// logControl
			// 
			this.logControl.AllowDrop = true;
			this.logControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.logControl.Name = "logControl";
			this.logControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 516, true);
			this.logControl.TabIndex = 0;
			// 
			// tasksGrid
			// 
			this.tasksGrid.AllowBeginDrag = false;
			this.tasksGrid.AllowCopyToNewRowMenuItem = false;
			this.tasksGrid.AllowDragDropWithChanges = false;
			this.tasksGrid.AllowNavigation = false;
			this.tasksGrid.BackColor = System.Drawing.SystemColors.Control;
			this.tasksGrid.BackgroundColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.tasksGrid, "TasksView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Common.Candidate)(null)).TasksView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Recruitment.Common.ProcessTaskView)(((System.Collections.IList)(((Enterprise.Recruitment.Common.Candidate)(null)).TasksView)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.ProcessTaskView)(((System.Collections.IList)(((Enterprise.Recruitment.Common.Candidate)(null)).TasksView)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.ProcessTaskView)(((System.Collections.IList)(((Enterprise.Recruitment.Common.Candidate)(null)).TasksView)).SyncRoot)).Stage)));
			this.tasksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("0A517151-E306-4F0C-B930-4F1C1678FA61", "Sequence");
			zTextBoxColumnStyleInfo1.ColumnName = "Sequence";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("8419759d-3e93-4465-8bf9-3d57e7ae04e2", "Status");
			zDropEditColumnStyleInfo1.ColumnName = "Status";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("0ea72aff-7368-4bea-ae16-e43e45f20da9", "Stage");
			zTextBoxColumnStyleInfo2.ColumnName = "Stage";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.tasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tasksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tasksGrid.CopySelectedRowsAllowed = false;
			this.tasksGrid.DisableImportDataMenuItem = true;
			this.tasksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tasksGrid.Enabled = false;
			this.tasksGrid.GridId = "d47f2ffa-69a0-415e-ab9c-29e3ec2916a0";
			this.tasksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tasksGrid.IsDataVersionLogsMenuItemVisible = false;
			this.tasksGrid.LayoutKey = "tasksGrid";
			this.tasksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.tasksGrid.Name = "tasksGrid";
			this.tasksGrid.ParentRowsVisible = false;
			this.tasksGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.tasksGrid.RowHeadersVisible = false;
			this.tasksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 114, true);
			this.tasksGrid.TabIndex = 1;
			// 
			// nameLabel
			// 
			this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nameLabel, "Applicant.HA_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Applicant.HA_FullName)));
			this.nameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.nameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.nameLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 23, true);
			this.nameLabel.TabIndex = 0;
			// 
			// roleLabel
			// 
			this.roleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.roleLabel, "Role.HJ_JobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.Candidate)(null)).Role.HJ_JobTitle)));
			this.roleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.roleLabel.IsFontBold = true;
			this.roleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 35, true);
			this.roleLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.roleLabel.Name = "roleLabel";
			this.roleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 23, true);
			this.roleLabel.TabIndex = 1;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.HiringRequestButton);
			this.topPanel.Controls.Add(this.roleLabel);
			this.topPanel.Controls.Add(this.nameLabel);
			this.topPanel.Controls.Add(this.createWorkItemButton);
			this.topPanel.Controls.Add(this.createWorkItemDropDown);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100000, 128, true);
			this.topPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 64, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 64, true);
			this.topPanel.TabIndex = 15;
			// 
			// HiringRequestButton
			// 
			this.HiringRequestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HiringRequestButton.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("77c131a6-81da-45ac-8233-f255f16079a1", "Hiring Request");
			this.HiringRequestButton.Enabled = false;
			this.HiringRequestButton.IsCaptionOverridden = false;
			this.HiringRequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 34, true);
			this.HiringRequestButton.Name = "HiringRequestButton";
			this.HiringRequestButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.HiringRequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.HiringRequestButton.TabIndex = 3;
			this.HiringRequestButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.HiringRequestButton.ToolTipCaption = null;
			this.HiringRequestButton.UseVisualStyleBackColor = true;
			this.HiringRequestButton.Click += new System.EventHandler(this.HiringRequestButton_Click);
			// 
			// createWorkItemButton
			// 
			this.createWorkItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createWorkItemButton.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("63393a97-f2d8-4bbd-891b-8417075e5082", "Create Work Item");
			this.createWorkItemButton.Enabled = false;
			this.createWorkItemButton.IsCaptionOverridden = false;
			this.createWorkItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 34, true);
			this.createWorkItemButton.Name = "createWorkItemButton";
			this.createWorkItemButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.createWorkItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.createWorkItemButton.TabIndex = 2;
			this.createWorkItemButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.createWorkItemButton.ToolTipCaption = null;
			this.createWorkItemButton.UseVisualStyleBackColor = true;
			this.createWorkItemButton.Click += new System.EventHandler(this.CreateWorkItemButton_Click);
			// 
			// createWorkItemDropDown
			// 
			this.createWorkItemDropDown.AllowDrop = true;
			this.createWorkItemDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.createWorkItemDropDown, "SelectedTemplateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruitment.Common.Candidate)(null)).SelectedTemplateDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Common.Candidate)(null)).PairListFromWorkItemList)));
			this.createWorkItemDropDown.BindToList = "PairListFromWorkItemList";
			this.createWorkItemDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 11, true);
			this.createWorkItemDropDown.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.createWorkItemDropDown.Name = "createWorkItemDropDown";
			this.createWorkItemDropDown.ShouldResizeByMaxLength = true;
			this.createWorkItemDropDown.ShowDescriptionBox = false;
			this.createWorkItemDropDown.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.createWorkItemDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.createWorkItemDropDown.TabIndex = 1;
			// 
			// bottomPanel
			// 
			this.bottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bottomPanel.Controls.Add(this.postingButtons);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 743, true);
			this.bottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 25, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 25, true);
			this.bottomPanel.TabIndex = 16;
			// 
			// postingButtons
			// 
			this.postingButtons.AllowDrop = true;
			this.postingButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.postingButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 0, true);
			this.postingButtons.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 0, true);
			this.postingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 25, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 25, true);
			this.postingButtons.TabIndex = 0;
			// 
			// detailsSplitContainer
			// 
			this.detailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.detailsSplitContainer.Name = "detailsSplitContainer";
			this.detailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// detailsSplitContainer.Panel1
			// 
			this.detailsSplitContainer.Panel1.Controls.Add(this.tasksGrid);
			this.detailsSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 679, true);
			this.detailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// detailsSplitContainer.Panel2
			// 
			this.detailsSplitContainer.Panel2.Controls.Add(this.tabControl);
			this.detailsSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400);
			this.detailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.detailsSplitContainer.TabIndex = 3;
			// 
			// CandidateDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsSplitContainer);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 600, true);
			this.Name = "CandidateDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 768, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.pdfPreviewTabPage.ResumeLayout(false);
			this.pdfPreviewTabPage.PerformLayout();
			this.documentPreview.ResumeLayout(true);
			this.documentPreview.PerformLayout();
			this.profileTabPage.ResumeLayout(false);
			this.profileTabPage.PerformLayout();
			this.sourceDropEdit.ResumeLayout(true);
			this.sourceDropEdit.PerformLayout();
			this.alternatePhoneControl.ResumeLayout(true);
			this.alternatePhoneControl.PerformLayout();
			this.mobileNumberControl.ResumeLayout(true);
			this.mobileNumberControl.PerformLayout();
			this.stateDropEdit.ResumeLayout(true);
			this.stateDropEdit.PerformLayout();
			this.submissionDateBox.ResumeLayout(true);
			this.submissionDateBox.PerformLayout();
			this.countryCodeBox.ResumeLayout(true);
			this.countryCodeBox.PerformLayout();
			this.peopleAndCommunicationTabPage.ResumeLayout(false);
			this.peopleAndCommunicationTabPage.PerformLayout();
			this.peopleAndCommunicationControl.ResumeLayout(true);
			this.peopleAndCommunicationControl.PerformLayout();
			this.edocsTabPage.ResumeLayout(false);
			this.edocsTabPage.PerformLayout();
			this.eDocsControl.ResumeLayout(true);
			this.eDocsControl.PerformLayout();
			this.eConversationTabPage.ResumeLayout(false);
			this.eConversationTabPage.PerformLayout();
			this.eConversation.ResumeLayout(true);
			this.eConversation.PerformLayout();
			this.notesTabPage.ResumeLayout(false);
			this.notesTabPage.PerformLayout();
			this.notesControl.ResumeLayout(true);
			this.notesControl.PerformLayout();
			this.logsTabPage.ResumeLayout(false);
			this.logsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tasksGrid)).EndInit();
			this.tasksGrid.ResumeLayout(false);
			this.tasksGrid.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.createWorkItemDropDown.ResumeLayout(true);
			this.createWorkItemDropDown.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.postingButtons.ResumeLayout(true);
			this.postingButtons.PerformLayout();
			this.detailsSplitContainer.Panel1.ResumeLayout(false);
			this.detailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).EndInit();
			this.detailsSplitContainer.ResumeLayout(false);
			this.detailsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl tabControl;
		private ZArchitecture.GUI.ZTabPage pdfPreviewTabPage;
		private DocumentScanning.GUI.GraphicalDisplayControl documentPreview;
		private ZArchitecture.GUI.ZTabPage profileTabPage;
		private ZArchitecture.GUI.ZTabPage eConversationTabPage;
		private Enterprise.EConversation.GUI.CandidateEConversationControl eConversation;
		private Enterprise.ZArchitecture.GUI.ZTabPage notesTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteUserControl notesControl;
		private Enterprise.ZArchitecture.ZTextBox emailTextBox;
		private ZArchitecture.ZTextBox nameTextBox;
		private ZArchitecture.ZLabel nameLabel;
		private ZArchitecture.ZLabel roleLabel;
		private ZArchitecture.ZTextBox experienceTextBox;
		private ZArchitecture.GUI.ZCodeFindBox countryCodeBox;
		private ZArchitecture.GUI.ZDropEdit stateDropEdit;
		private ZArchitecture.ZTextBox cityTextBox;
		private MasterFiles.GUI.PhoneNumberUserControl mobileNumberControl;
		private MasterFiles.GUI.PhoneNumberUserControl alternatePhoneControl;
		private ZArchitecture.GUI.ZDropEdit sourceDropEdit;
		private ZArchitecture.GUI.ZDateEdit submissionDateBox;
		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtons;
		private ZArchitecture.GUI.ZTabPage edocsTabPage;
		protected EdocsSwappableControl eDocsControl;
		private ZArchitecture.GUI.ZTabPage peopleAndCommunicationTabPage;
		protected PeopleAndCommunicationControl peopleAndCommunicationControl;
		private ZArchitecture.GUI.ZStmALogUserControl logControl;
		private ZArchitecture.ZGrid tasksGrid;
		private ZArchitecture.GUI.ZDropEdit createWorkItemDropDown;
		private ZArchitecture.GUI.ZButton createWorkItemButton;
		private ZArchitecture.GUI.ZTabPage logsTabPage;
		private CargoWise.Windows.UI.KSplitContainer detailsSplitContainer;
		private ZArchitecture.GUI.ZButton HiringRequestButton;
	}
}
