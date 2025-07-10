using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.MasterFiles.GUI.Global.GlbPerson
{
	partial class GlbPersonNewForm
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
			this.genderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PersonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FullName = new Enterprise.ZArchitecture.ZTextBox();
			this.FriendlyName = new Enterprise.ZArchitecture.ZTextBox();
			this.LegalName = new Enterprise.ZArchitecture.ZTextBox();
			this.HomeAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HomeAddress = new Enterprise.MasterFiles.GUI.GlbPersonAddressControl();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EmailTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.HomePhoneTextBox = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.MobilePhoneTextBox = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.PersonalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NationalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDateEdit21 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IdentificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PassportPlaceOfIssueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PassportExpiryDateTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DriversLicenseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PassportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.genderDropEdit.SuspendLayout();
			this.PersonGroupBox.SuspendLayout();
			this.HomeAddressGroupBox.SuspendLayout();
			this.HomeAddress.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.HomePhoneTextBox.SuspendLayout();
			this.MobilePhoneTextBox.SuspendLayout();
			this.PersonalInformationGroupBox.SuspendLayout();
			this.NationalityDropEdit.SuspendLayout();
			this.zDateEdit21.SuspendLayout();
			this.IdentificationGroupBox.SuspendLayout();
			this.PassportPlaceOfIssueDropEdit.SuspendLayout();
			this.PassportExpiryDateTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 529, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.IdentificationGroupBox);
			this.MainTabPage.Controls.Add(this.PersonalInformationGroupBox);
			this.MainTabPage.Controls.Add(this.ContactDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.HomeAddressGroupBox);
			this.MainTabPage.Controls.Add(this.PersonGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 502, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 481, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 502, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 529, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			// 
			// genderDropEdit
			// 
			this.genderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.genderDropEdit, "PER_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Gender)));
			this.genderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 82, true);
			this.genderDropEdit.Name = "genderDropEdit";
			this.genderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.genderDropEdit.TabIndex = 5;
			// 
			// PersonGroupBox
			// 
			this.PersonGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|Person", "Person");
			this.PersonGroupBox.Controls.Add(this.FullName);
			this.PersonGroupBox.Controls.Add(this.FriendlyName);
			this.PersonGroupBox.Controls.Add(this.LegalName);
			this.PersonGroupBox.Controls.Add(this.genderDropEdit);
			this.PersonGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PersonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PersonGroupBox.Name = "PersonGroupBox";
			this.PersonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 108, true);
			this.PersonGroupBox.TabIndex = 0;
			this.PersonGroupBox.TabStop = false;
			// 
			// FullName
			// 
			this.BindingSource.SetBindingMember(this.FullName, "PER_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_FullName)));
			this.FullName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FullName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 17, true);
			this.FullName.Name = "FullName";
			this.FullName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.FullName.TabIndex = 1;
			// 
			// FriendlyName
			// 
			this.BindingSource.SetBindingMember(this.FriendlyName, "PER_FriendlyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_FriendlyName)));
			this.FriendlyName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|FriendlyName", "Preferred Name");
			this.FriendlyName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FriendlyName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 38, true);
			this.FriendlyName.Name = "FriendlyName";
			this.FriendlyName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.FriendlyName.TabIndex = 3;
			// 
			// LegalName
			// 
			this.BindingSource.SetBindingMember(this.LegalName, "PER_LegalName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_LegalName)));
			this.LegalName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|LegalName", "Legal Name");
			this.LegalName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LegalName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 60, true);
			this.LegalName.Name = "LegalName";
			this.LegalName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.LegalName.TabIndex = 4;
			// 
			// HomeAddressGroupBox
			// 
			this.HomeAddressGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|HomeAddress", "Home Address");
			this.HomeAddressGroupBox.Controls.Add(this.HomeAddress);
			this.HomeAddressGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HomeAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
			this.HomeAddressGroupBox.Name = "HomeAddressGroupBox";
			this.HomeAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 111, true);
			this.HomeAddressGroupBox.TabIndex = 1;
			this.HomeAddressGroupBox.TabStop = false;
			// 
			// HomeAddress
			// 
			this.HomeAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomeAddress, ".");
			this.HomeAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 17, true);
			this.HomeAddress.Name = "HomeAddress";
			this.HomeAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 93, true);
			this.HomeAddress.TabIndex = 0;
			this.HomeAddress.ValidationJustForced = false;
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|ContactDetails", "Contact Details");
			this.ContactDetailsGroupBox.Controls.Add(this.EmailTextbox);
			this.ContactDetailsGroupBox.Controls.Add(this.HomePhoneTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.MobilePhoneTextBox);
			this.ContactDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 219, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 86, true);
			this.ContactDetailsGroupBox.TabIndex = 2;
			this.ContactDetailsGroupBox.TabStop = false;
			// 
			// EmailTextbox
			// 
			this.BindingSource.SetBindingMember(this.EmailTextbox, "PER_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_EmailAddress)));
			this.EmailTextbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fe2c0fc5-c7c7-487f-b82c-75ca92d1dd7a", "Email");
			this.EmailTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 17, true);
			this.EmailTextbox.Name = "EmailTextbox";
			this.EmailTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.EmailTextbox.TabIndex = 12;
			// 
			// HomePhoneTextBox
			// 
			this.HomePhoneTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomePhoneTextBox, "HomePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).HomePhoneNumber)));
			this.HomePhoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5cc913d6-6267-42b4-a3ea-97f8d2d44868", "Home");
			this.HomePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 61, true);
			this.HomePhoneTextBox.Name = "HomePhoneTextBox";
			this.HomePhoneTextBox.ShowLocalNumberLabel = false;
			this.HomePhoneTextBox.ShowPublishedCheckBox = false;
			this.HomePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 22, true);
			this.HomePhoneTextBox.TabIndex = 14;
			// 
			// MobilePhoneTextBox
			// 
			this.MobilePhoneTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneTextBox, "MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).MobilePhoneNumber)));
			this.MobilePhoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("46ec057f-5255-4078-8d9c-7494624806e5", "Mobile");
			this.MobilePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 39, true);
			this.MobilePhoneTextBox.Name = "MobilePhoneTextBox";
			this.MobilePhoneTextBox.ShowLocalNumberLabel = false;
			this.MobilePhoneTextBox.ShowPublishedCheckBox = false;
			this.MobilePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 22, true);
			this.MobilePhoneTextBox.TabIndex = 13;
			// 
			// PersonalInformationGroupBox
			// 
			this.PersonalInformationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|PersonalInformation", "Personal Information");
			this.PersonalInformationGroupBox.Controls.Add(this.NationalityDropEdit);
			this.PersonalInformationGroupBox.Controls.Add(this.zDateEdit21);
			this.PersonalInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PersonalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 305, true);
			this.PersonalInformationGroupBox.Name = "PersonalInformationGroupBox";
			this.PersonalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 63, true);
			this.PersonalInformationGroupBox.TabIndex = 3;
			this.PersonalInformationGroupBox.TabStop = false;
			// 
			// NationalityDropEdit
			// 
			this.NationalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalityDropEdit, "PER_RN_NKNationalityCodeISO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_RN_NKNationalityCodeISO)));
			this.NationalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 39, true);
			this.NationalityDropEdit.Name = "NationalityDropEdit";
			this.NationalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.NationalityDropEdit.TabIndex = 17;
			// 
			// zDateEdit21
			// 
			this.zDateEdit21.AllowDrop = true;
			this.zDateEdit21.AutoCompleteMonthThreshold = 1;
			this.zDateEdit21.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit21, "PER_BirthDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_BirthDate)));
			this.zDateEdit21.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9413624d-c3d2-4a80-a309-3c6f6d5e2237", "Date of Birth");
			this.zDateEdit21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 17, true);
			this.zDateEdit21.Name = "zDateEdit21";
			this.zDateEdit21.TabIndex = 16;
			// 
			// IdentificationGroupBox
			// 
			this.IdentificationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|Identification", "Identification");
			this.IdentificationGroupBox.Controls.Add(this.PassportPlaceOfIssueDropEdit);
			this.IdentificationGroupBox.Controls.Add(this.PassportExpiryDateTextBox);
			this.IdentificationGroupBox.Controls.Add(this.DriversLicenseNumberTextBox);
			this.IdentificationGroupBox.Controls.Add(this.PassportTextBox);
			this.IdentificationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.IdentificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.IdentificationGroupBox.Name = "IdentificationGroupBox";
			this.IdentificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 108, true);
			this.IdentificationGroupBox.TabIndex = 4;
			this.IdentificationGroupBox.TabStop = false;
			// 
			// PassportPlaceOfIssueDropEdit
			// 
			this.PassportPlaceOfIssueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PassportPlaceOfIssueDropEdit, "PER_PassportPlaceOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PassportPlaceOfIssue)));
			this.PassportPlaceOfIssueDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("97014bd5-1214-4fd7-a9ef-172564af89f4", "Issue Ctry/Rgn.", "Issue Country/Region", "Country/Region Of Issue");
			this.PassportPlaceOfIssueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 38, true);
			this.PassportPlaceOfIssueDropEdit.Name = "PassportPlaceOfIssueDropEdit";
			this.PassportPlaceOfIssueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.PassportPlaceOfIssueDropEdit.TabIndex = 1;
			// 
			// PassportExpiryDateTextBox
			// 
			this.PassportExpiryDateTextBox.AllowDrop = true;
			this.PassportExpiryDateTextBox.AutoCompleteMonthThreshold = 1;
			this.PassportExpiryDateTextBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PassportExpiryDateTextBox, "PER_PassportExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PassportExpiryDate)));
			this.PassportExpiryDateTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5394dbaa-dfca-4dfa-a31d-c3487b8807e2", "Passport Expiry");
			this.PassportExpiryDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 60, true);
			this.PassportExpiryDateTextBox.Name = "PassportExpiryDateTextBox";
			this.PassportExpiryDateTextBox.TabIndex = 2;
			// 
			// DriversLicenseNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DriversLicenseNumberTextBox, "PER_DriversLicenseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_DriversLicenseNumber)));
			this.DriversLicenseNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c56dd321-7cd0-4762-9ecb-d4d50afc958e", "Drivers License / ID");
			this.DriversLicenseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 82, true);
			this.DriversLicenseNumberTextBox.Name = "DriversLicenseNumberTextBox";
			this.DriversLicenseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.DriversLicenseNumberTextBox.TabIndex = 3;
			// 
			// PassportTextBox
			// 
			this.BindingSource.SetBindingMember(this.PassportTextBox, "PER_Passport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Passport)));
			this.PassportTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a192f281-76b0-4344-9e34-f35a7e392a52", "Passport");
			this.PassportTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PassportTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 17, true);
			this.PassportTextBox.Name = "PassportTextBox";
			this.PassportTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.PassportTextBox.TabIndex = 0;
			// 
			// GlbPersonNewForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonNewForm|Title", "Person");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 585, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 624, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 624, true);
			this.Name = "GlbPersonNewForm";
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
			this.genderDropEdit.ResumeLayout(true);
			this.genderDropEdit.PerformLayout();
			this.PersonGroupBox.ResumeLayout(false);
			this.PersonGroupBox.PerformLayout();
			this.HomeAddressGroupBox.ResumeLayout(false);
			this.HomeAddressGroupBox.PerformLayout();
			this.HomeAddress.ResumeLayout(true);
			this.HomeAddress.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.HomePhoneTextBox.ResumeLayout(true);
			this.HomePhoneTextBox.PerformLayout();
			this.MobilePhoneTextBox.ResumeLayout(true);
			this.MobilePhoneTextBox.PerformLayout();
			this.PersonalInformationGroupBox.ResumeLayout(false);
			this.PersonalInformationGroupBox.PerformLayout();
			this.NationalityDropEdit.ResumeLayout(true);
			this.NationalityDropEdit.PerformLayout();
			this.zDateEdit21.ResumeLayout(true);
			this.zDateEdit21.PerformLayout();
			this.IdentificationGroupBox.ResumeLayout(false);
			this.IdentificationGroupBox.PerformLayout();
			this.PassportPlaceOfIssueDropEdit.ResumeLayout(true);
			this.PassportPlaceOfIssueDropEdit.PerformLayout();
			this.PassportExpiryDateTextBox.ResumeLayout(true);
			this.PassportExpiryDateTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox PersonGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox HomeAddressGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PersonalInformationGroupBox;

		#endregion
		private ZArchitecture.ZTextBox FullName;
		private ZArchitecture.ZTextBox LegalName;
		private ZArchitecture.ZTextBox FriendlyName;
		private ZArchitecture.GUI.ZDateEdit zDateEdit21;
		private ZArchitecture.GUI.ZDropEdit NationalityDropEdit;
		private ZArchitecture.ZTextBox EmailTextbox;
		private PhoneNumberUserControl HomePhoneTextBox;
		private PhoneNumberUserControl MobilePhoneTextBox;
		private ZArchitecture.GUI.ZDropEdit genderDropEdit;
		private GlbPersonAddressControl HomeAddress;
		private ZArchitecture.GUI.ZGroupBox IdentificationGroupBox;
		private ZArchitecture.GUI.ZDateEdit PassportExpiryDateTextBox;
		private ZArchitecture.ZTextBox DriversLicenseNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit PassportPlaceOfIssueDropEdit;
		private ZArchitecture.ZTextBox PassportTextBox;
	}
}
