using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbPersonForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlbPersonForm));
			this.ProfilePicture = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.PersonName = new Enterprise.ZArchitecture.ZLabel();
			this.AccreditationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccreditationsControl = new Enterprise.MasterFiles.GUI.GlbAccreditationsTabControl();
			this.PrimaryJobTitle = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentCompanyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BirthCountry = new Enterprise.ZArchitecture.ZLabel();
			this.BirthCountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentCompany = new Enterprise.ZArchitecture.ZLabel();
			this.DateOfBirthLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DateOfBirth = new Enterprise.ZArchitecture.ZLabel();
			this.MaleIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.FemaleIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.HomeAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HomeAddressUserControl = new Enterprise.MasterFiles.GUI.GlbPersonAddressControl();
			this.editToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.editToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HomePhoneTextBox = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.MobilePhoneTextBox = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.PersonalInfo = new Enterprise.ZArchitecture.ZTextBox();
			this.WebAccessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ActiveAssociationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PersonAssociationsUserControl = new Enterprise.MasterFiles.GUI.PersonAssociationsControl();
			this.PersonalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.UnlockButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LockoutDateTimeBoundDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfilePicture.SuspendLayout();
			this.AccreditationsTabPage.SuspendLayout();
			this.SecurityTabPage.SuspendLayout();
			this.AccreditationsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaleIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FemaleIcon)).BeginInit();
			this.HomeAddressGroupBox.SuspendLayout();
			this.HomeAddressUserControl.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.HomePhoneTextBox.SuspendLayout();
			this.MobilePhoneTextBox.SuspendLayout();
			this.ActiveAssociationsGroupBox.SuspendLayout();
			this.PersonAssociationsUserControl.SuspendLayout();
			this.PersonalInformationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 624, true);
			this.MainTabControl.Controls.Add(this.AccreditationsTabPage);
			this.MainTabControl.Controls.Add(this.SecurityTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.PersonalInformationGroupBox);
			this.MainTabPage.Controls.Add(this.ActiveAssociationsGroupBox);
			this.MainTabPage.Controls.Add(this.ContactDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.HomeAddressGroupBox);
			this.MainTabPage.Controls.Add(this.MaleIcon);
			this.MainTabPage.Controls.Add(this.FemaleIcon);
			this.MainTabPage.Controls.Add(this.DateOfBirthLabel);
			this.MainTabPage.Controls.Add(this.DateOfBirth);
			this.MainTabPage.Controls.Add(this.CurrentCompany);
			this.MainTabPage.Controls.Add(this.BirthCountryLabel);
			this.MainTabPage.Controls.Add(this.BirthCountry);
			this.MainTabPage.Controls.Add(this.CurrentCompanyLabel);
			this.MainTabPage.Controls.Add(this.PrimaryJobTitle);
			this.MainTabPage.Controls.Add(this.PersonName);
			this.MainTabPage.Controls.Add(this.ProfilePicture);
			this.MainTabPage.Controls.Add(this.editToolStrip);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 717, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 717, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 717, true);
			//
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 458, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 725, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			// 
			// ProfilePicture
			// 
			this.ProfilePicture.AllowDrop = true;
			this.ProfilePicture.BackColor = System.Drawing.SystemColors.Window;
			this.ProfilePicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.BindingSource.SetBindingMember(this.ProfilePicture, "PER_ProfilePicture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_ProfilePicture)));
			this.ProfilePicture.CanSelectImage = false;
			this.ProfilePicture.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 13, true);
			this.ProfilePicture.Name = "ProfilePicture";
			this.ProfilePicture.ReadOnly = false;
			this.ProfilePicture.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 95, true);
			this.ProfilePicture.TabIndex = 0;
			this.ProfilePicture.TabStop = false;
			// 
			// PersonName
			// 
			this.BindingSource.SetBindingMember(this.PersonName, "DisplayText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).DisplayText)));
			this.PersonName.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.PersonName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 9, true);
			this.PersonName.Name = "PersonName";
			this.PersonName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 18, true);
			this.PersonName.TabIndex = 1;
			this.PersonName.Text = "Name placeholder";
			// 
			// AccreditationsTabPage
			// 
			this.AccreditationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HRJobApplicantForm|0201D4EE-B867-4EFA-B2EC-C4FB388A83D0", "Accreditations");
			this.AccreditationsTabPage.Controls.Add(this.AccreditationsControl);
			this.AccreditationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AccreditationsTabPage.Name = "AccreditationsTabPage";
			this.AccreditationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1171, 618, true);
			this.AccreditationsTabPage.TabIndex = 4;
			// 
			// WebAccessCheckBox
			// 
			this.WebAccessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WebAccessCheckBox, "PER_WebAccessEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_WebAccessEnabled)));
			this.WebAccessCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|WebAccessEnabled", "Web Access Enabled");
			this.WebAccessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WebAccessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.WebAccessCheckBox.Name = "WebAccessCheckBox";
			this.WebAccessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WebAccessCheckBox.TabIndex = 0;
			// 
			// UnlockButton
			// 
			this.UnlockButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|6f2ad86d-0684-4091-8d41-c26bb0662386", "Unlock");
			this.UnlockButton.IsCaptionOverridden = false;
			this.UnlockButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 16, true);
			this.UnlockButton.Name = "UnlockButton";
			this.UnlockButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnlockButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.UnlockButton.TabIndex = 37;
			this.UnlockButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UnlockButton.ToolTipCaption = null;
			this.UnlockButton.Click += new System.EventHandler(this.UnlockButton_Click);
			//
			// LockoutDateTimeBoundDate
			// 
			this.LockoutDateTimeBoundDate.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|864a8baa-9d2d-4081-b3de-adc5d68e7ceb", "Locked Out Until");
			this.LockoutDateTimeBoundDate.AllowDrop = true;
			this.LockoutDateTimeBoundDate.AutoCompleteMonthThreshold = 1;
			this.LockoutDateTimeBoundDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LockoutDateTimeBoundDate, "LockoutDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_LoginDisabledUntilUtc)));
			this.LockoutDateTimeBoundDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LockoutDateTimeBoundDate.Enabled = false;
			this.LockoutDateTimeBoundDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 16, true);
			this.LockoutDateTimeBoundDate.Name = "LockoutDateTimeBoundDate";
			this.LockoutDateTimeBoundDate.TabIndex = 38;
			// 
			// SecurityTabPage
			// 
			this.SecurityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|8a4e38af-b8a7-4e58-abc0-9249814e27fd0", "Security");
			this.SecurityTabPage.Controls.Add(this.WebAccessCheckBox);
			this.SecurityTabPage.Controls.Add(this.UnlockButton);
			this.SecurityTabPage.Controls.Add(this.LockoutDateTimeBoundDate);
			this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SecurityTabPage.Name = "SecurityTabPage";
			this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1171, 618, true);
			this.SecurityTabPage.TabIndex = 5;
			// 
			// AccreditationsControl
			// 
			this.AccreditationsControl.AllowDrop = true;
			this.AccreditationsControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.AccreditationsControl, ".");
			this.AccreditationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccreditationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccreditationsControl.Name = "AccreditationsControl";
			this.AccreditationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1171, 618, true);
			this.AccreditationsControl.TabIndex = 0;
			// 
			// PrimaryJobTitle
			// 
			this.PrimaryJobTitle.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrimaryJobTitle, "PrimaryJobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PrimaryJobTitle)));
			this.PrimaryJobTitle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PrimaryJobTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 46, true);
			this.PrimaryJobTitle.Name = "PrimaryJobTitle";
			this.PrimaryJobTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.PrimaryJobTitle.TabIndex = 2;
			// 
			// CurrentCompanyLabel
			// 
			this.CurrentCompanyLabel.AutoSize = true;
			this.CurrentCompanyLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Current", "Current:");
			this.CurrentCompanyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CurrentCompanyLabel.IsFontBold = true;
			this.CurrentCompanyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 62, true);
			this.CurrentCompanyLabel.Name = "CurrentCompanyLabel";
			this.CurrentCompanyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 13, true);
			this.CurrentCompanyLabel.TabIndex = 3;
			// 
			// BirthCountry
			// 
			this.BirthCountry.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BirthCountry, "PER_RN_NKNationalityCodeISO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_RN_NKNationalityCodeISO)));
			this.BirthCountry.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BirthCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 78, true);
			this.BirthCountry.Name = "BirthCountry";
			this.BirthCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.BirthCountry.TabIndex = 6;
			// 
			// BirthCountryLabel
			// 
			this.BirthCountryLabel.AutoSize = true;
			this.BirthCountryLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Nationality", "Nationality:");
			this.BirthCountryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BirthCountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 78, true);
			this.BirthCountryLabel.Name = "BirthCountryLabel";
			this.BirthCountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.BirthCountryLabel.TabIndex = 5;
			// 
			// CurrentCompany
			// 
			this.CurrentCompany.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CurrentCompany, "CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).CompanyName)));
			this.CurrentCompany.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CurrentCompany.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 62, true);
			this.CurrentCompany.Name = "CurrentCompany";
			this.CurrentCompany.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
			this.CurrentCompany.TabIndex = 4;
			// 
			// DateOfBirthLabel
			// 
			this.DateOfBirthLabel.AutoSize = true;
			this.DateOfBirthLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|DateOfBirth", "Date of Birth:");
			this.DateOfBirthLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DateOfBirthLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 94, true);
			this.DateOfBirthLabel.Name = "DateOfBirthLabel";
			this.DateOfBirthLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.DateOfBirthLabel.TabIndex = 7;
			// 
			// DateOfBirth
			// 
			this.DateOfBirth.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DateOfBirth, "PER_BirthDateAndAge_Formatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_BirthDateAndAge_Formatted)));
			this.DateOfBirth.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DateOfBirth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 94, true);
			this.DateOfBirth.Name = "DateOfBirth";
			this.DateOfBirth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 13, true);
			this.DateOfBirth.TabIndex = 8;
			// 
			// MaleIcon
			// 
			this.MaleIcon.AllowDrop = true;
			this.MaleIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.MaleIcon.Image = ((System.Drawing.Image)(resources.GetObject("MaleIcon.Image")));
			this.MaleIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 75, true);
			this.MaleIcon.Name = "MaleIcon";
			this.MaleIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.MaleIcon.TabIndex = 9;
			this.MaleIcon.TabStop = false;
			this.MaleIcon.Visible = false;
			// 
			// FemaleIcon
			// 
			this.FemaleIcon.AllowDrop = true;
			this.FemaleIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.FemaleIcon.Image = ((System.Drawing.Image)(resources.GetObject("FemaleIcon.Image")));
			this.FemaleIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 75, true);
			this.FemaleIcon.Name = "FemaleIcon";
			this.FemaleIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.FemaleIcon.TabIndex = 9;
			this.FemaleIcon.TabStop = false;
			this.FemaleIcon.Visible = false;
			// 
			// HomeAddressGroupBox
			// 
			this.HomeAddressGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|HomeAddressUserControl", "Home Address");
			this.HomeAddressGroupBox.Controls.Add(this.HomeAddressUserControl);
			this.HomeAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 7, true);
			this.HomeAddressGroupBox.Name = "HomeAddressGroupBox";
			this.HomeAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 107, true);
			this.HomeAddressGroupBox.TabIndex = 10;
			this.HomeAddressGroupBox.TabStop = false;
			// 
			// HomeAddressUserControl
			// 
			this.HomeAddressUserControl.AllowDrop = true;
			this.HomeAddressUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HomeAddressUserControl, ".");
			this.HomeAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.HomeAddressUserControl.Name = "HomeAddressUserControl";
			this.HomeAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 87, true);
			this.HomeAddressUserControl.TabIndex = 1;
			this.HomeAddressUserControl.ValidationJustForced = false;
			// 
			// editToolStrip
			// 
			this.editToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.editToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.editToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.editToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17);
			this.editToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.editToolStripButton});
			this.editToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 9, true);
			this.editToolStrip.Name = "editToolStrip";
			this.editToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
			this.editToolStrip.TabIndex = 22;
			// 
			// editToolStripButton
			// 
			this.editToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("editToolStripButton.Image")));
			this.editToolStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.editToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.editToolStripButton.Name = "editToolStripButton";
			this.editToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17);
			this.editToolStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.editToolStripButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|ContactDetails", "Personal Contact Details");
			this.ContactDetailsGroupBox.Controls.Add(this.EmailAddressTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.HomePhoneTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.MobilePhoneTextBox);
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(741, 7, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 107, true);
			this.ContactDetailsGroupBox.TabIndex = 47;
			this.ContactDetailsGroupBox.TabStop = false;
			// 
			// EmailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailAddressTextBox, "PER_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_EmailAddress)));
			this.EmailAddressTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Email", "Email");
			this.EmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 19, true);
			this.EmailAddressTextBox.Name = "EmailAddressTextBox";
			this.EmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
			this.EmailAddressTextBox.TabIndex = 7;
			// 
			// HomePhoneTextBox
			// 
			this.HomePhoneTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomePhoneTextBox, "HomePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).HomePhoneNumber)));
			this.HomePhoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Home", "Home");
			this.HomePhoneTextBox.EnableValidStateColor = true;
			this.HomePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.HomePhoneTextBox.Name = "HomePhoneTextBox";
			this.HomePhoneTextBox.ShowLocalNumberLabel = false;
			this.HomePhoneTextBox.ShowPublishedCheckBox = false;
			this.HomePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.HomePhoneTextBox.TabIndex = 9;
			// 
			// MobilePhoneTextBox
			// 
			this.MobilePhoneTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneTextBox, "MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).MobilePhoneNumber)));
			this.MobilePhoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Mobile", "Mobile");
			this.MobilePhoneTextBox.EnableValidStateColor = true;
			this.MobilePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
			this.MobilePhoneTextBox.Name = "MobilePhoneTextBox";
			this.MobilePhoneTextBox.ShowLocalNumberLabel = false;
			this.MobilePhoneTextBox.ShowPublishedCheckBox = false;
			this.MobilePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.MobilePhoneTextBox.TabIndex = 8;
			// 
			// PersonalInfo
			// 
			this.PersonalInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PersonalInfo, "PER_PersonalInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PersonalInfo)));
			this.PersonalInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|PersonalFamilyInfo", "Personal / Family Information");
			this.PersonalInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PersonalInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 20, true);
			this.PersonalInfo.Multiline = true;
			this.PersonalInfo.Name = "PersonalInfo";
			this.PersonalInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 570, true);
			this.PersonalInfo.TabIndex = 7;
			// 
			// ActiveAssociationsGroupBox
			// 
			this.ActiveAssociationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ActiveAssociationsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|ActiveAssociations", "Active Associations");
			this.ActiveAssociationsGroupBox.Controls.Add(this.PersonAssociationsUserControl);
			this.ActiveAssociationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 118, true);
			this.ActiveAssociationsGroupBox.Name = "ActiveAssociationsGroupBox";
			this.ActiveAssociationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 594, true);
			this.ActiveAssociationsGroupBox.TabIndex = 48;
			this.ActiveAssociationsGroupBox.TabStop = false;
			// 
			// PersonAssociationsUserControl
			// 
			this.PersonAssociationsUserControl.AllowDrop = true;
			this.PersonAssociationsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PersonAssociationsUserControl, "PersonAssociationsTreeModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PersonAssociationsTreeModel)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PersonAssociationsTreeModel)));
			this.PersonAssociationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 17, true);
			this.PersonAssociationsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PersonAssociationsUserControl.Name = "PersonAssociationsUserControl";
			this.PersonAssociationsUserControl.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("D4FF790E-383D-4854-89AC-C9A3B4D234FF", "Person Association");
			this.PersonAssociationsUserControl.NameOfTreeElementsPlural = Enterprise.MasterFiles.GUI.Res.GetData("780F39FB-D252-4A76-990A-DE7356B00B0F", "Person Associations");
			this.PersonAssociationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 570, true);
			this.PersonAssociationsUserControl.TabIndex = 0;
			// 
			// PersonalInformationGroupBox
			// 
			this.PersonalInformationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PersonalInformationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|PersonalInformation", "Personal Information");
			this.PersonalInformationGroupBox.Controls.Add(this.PersonalInfo);
			this.PersonalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 118, true);
			this.PersonalInformationGroupBox.Name = "PersonalInformationGroupBox";
			this.PersonalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 594, true);
			this.PersonalInformationGroupBox.TabIndex = 47;
			this.PersonalInformationGroupBox.TabStop = false;
			// 
			// GlbPersonForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbPersonForm|Title", "Person");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1305, 717, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1310, 725, true);
			this.Name = "GlbPersonForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.AccreditationsTabPage.ResumeLayout(false);
			this.AccreditationsTabPage.PerformLayout();
			this.SecurityTabPage.ResumeLayout(false);
			this.SecurityTabPage.PerformLayout();
			this.AccreditationsControl.ResumeLayout(true);
			this.AccreditationsControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfilePicture.ResumeLayout(true);
			this.ProfilePicture.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaleIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FemaleIcon)).EndInit();
			this.HomeAddressGroupBox.ResumeLayout(false);
			this.HomeAddressGroupBox.PerformLayout();
			this.HomeAddressUserControl.ResumeLayout(true);
			this.HomeAddressUserControl.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.HomePhoneTextBox.ResumeLayout(true);
			this.HomePhoneTextBox.PerformLayout();
			this.MobilePhoneTextBox.ResumeLayout(true);
			this.MobilePhoneTextBox.PerformLayout();
			this.ActiveAssociationsGroupBox.ResumeLayout(false);
			this.ActiveAssociationsGroupBox.PerformLayout();
			this.PersonAssociationsUserControl.ResumeLayout(true);
			this.PersonAssociationsUserControl.PerformLayout();
			this.PersonalInformationGroupBox.ResumeLayout(false);
			this.PersonalInformationGroupBox.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox WebAccessCheckBox;
		private ZArchitecture.ZTextBox PersonalInfo;
		private ZArchitecture.ZTextBox EmailAddressTextBox;
		private PhoneNumberUserControl HomePhoneTextBox;
		private PhoneNumberUserControl MobilePhoneTextBox;
		private ImageSelectionControl ProfilePicture;
		private ZLabel PersonName;
		private ZLabel DateOfBirthLabel;
		private ZLabel DateOfBirth;
		private ZLabel CurrentCompany;
		private ZLabel BirthCountryLabel;
		private ZLabel BirthCountry;
		private ZLabel CurrentCompanyLabel;
		private ZLabel PrimaryJobTitle;
		private KPictureBox MaleIcon;
		private KPictureBox FemaleIcon;
		private ZGroupBox HomeAddressGroupBox;
		private ZGroupBox ContactDetailsGroupBox;
		private ZGroupBox ActiveAssociationsGroupBox;
		private ZGroupBox PersonalInformationGroupBox;
		private PersonAssociationsControl PersonAssociationsUserControl;
		private GlbPersonAddressControl HomeAddressUserControl;
		private ZToolStrip editToolStrip;
		private ZToolStripButton editToolStripButton;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage AccreditationsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage SecurityTabPage;
		private GlbAccreditationsTabControl AccreditationsControl;
		protected ZButton UnlockButton;
		protected ZDateEdit LockoutDateTimeBoundDate;
	}
}
