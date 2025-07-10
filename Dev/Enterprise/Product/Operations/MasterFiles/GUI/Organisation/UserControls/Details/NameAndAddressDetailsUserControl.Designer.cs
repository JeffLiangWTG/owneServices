using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class NameAndAddressDetailsUserControl
	{
		private System.ComponentModel.IContainer components = null;
		private Enterprise.ZArchitecture.GUI.ZPanel MainDetailsPanel;
		protected ZGroupBox MainAddressDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit LanguageDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox OH_GBBoundGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox OH_RL_NKClosestPortBoundCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox OH_CodeBoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit StateBoundDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox OH_WebBoundTextBox;
		private PhoneNumberUserControl PhoneNumberControl;
		private PhoneNumberUserControl MobilePhoneNumberControl;
		protected Enterprise.ZArchitecture.ZTextBox EmailBoundTextBox;
		private PhoneNumberUserControl FaxNumberControl;
		protected Enterprise.ZArchitecture.ZTextBox PostCodeBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox CityBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox Address2BoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox Address1BoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton ClearFieldsButton;
		public Enterprise.ZArchitecture.ZTextBox OH_FullNameBoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton GoToUrlButton;
		public ZArchitecture.ZLabel DuplicateDetectionStatusLabel;
		protected ZArchitecture.ZLabel DuplicateExclusionsLabel;
		protected Enterprise.ZArchitecture.GUI.ZPictureBox DuplicateDetectionStatusIcon;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OverrideAdditionalAddressInformationCheckBox;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainAddressDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrgSecurityGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OverrideAdditionalAddressInformationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ScreenButtonCoverLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.AllBranchesLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.RegistrationNumberTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNumberTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GoToUrlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OH_GBBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OH_RL_NKClosestPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OH_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OH_WebBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.EmailBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OH_FullNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DuplicateDetectionStatusIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.DuplicateDetectionStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DuplicateExclusionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainDetailsPanel.SuspendLayout();
			this.MainAddressDetailsGroupBox.SuspendLayout();
			this.OrgSecurityGroupFindBox.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.RegistrationNumberTypeDropEdit.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			this.OH_GBBoundGuidFindBox.SuspendLayout();
			this.OH_RL_NKClosestPortBoundCodeFindBox.SuspendLayout();
			this.StateBoundDropEdit.SuspendLayout();
			this.PhoneNumberControl.SuspendLayout();
			this.MobilePhoneNumberControl.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.DuplicateDetectionStatusIcon.SuspendLayout();
			this.DuplicateDetectionStatusLabel.SuspendLayout();
			this.DuplicateExclusionsLabel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// MainDetailsPanel
			// 
			this.MainDetailsPanel.Controls.Add(this.MainAddressDetailsGroupBox);
			this.MainDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainDetailsPanel.Name = "MainDetailsPanel";
			this.MainDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.MainDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 502, true);
			this.MainDetailsPanel.TabIndex = 0;
			// 
			// MainAddressDetailsGroupBox
			// 
			this.MainAddressDetailsGroupBox.Controls.Add(this.OrgSecurityGroupFindBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OverrideAdditionalAddressInformationCheckBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.CategoryDropEdit);
			this.MainAddressDetailsGroupBox.Controls.Add(this.CountryFindBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.ValidateAddressButton);
			this.MainAddressDetailsGroupBox.Controls.Add(this.ScreenButton);
			this.MainAddressDetailsGroupBox.Controls.Add(this.ScreenButtonCoverLabel);
			this.MainAddressDetailsGroupBox.Controls.Add(this.ScreeningStatusDropEdit);
			this.MainAddressDetailsGroupBox.Controls.Add(this.AllBranchesLink);
			this.MainAddressDetailsGroupBox.Controls.Add(this.RegistrationNumberTypeDropEdit);
			this.MainAddressDetailsGroupBox.Controls.Add(this.RegistrationNumberTypeLabel);
			this.MainAddressDetailsGroupBox.Controls.Add(this.GoToUrlButton);
			this.MainAddressDetailsGroupBox.Controls.Add(this.LanguageDropEdit);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OH_GBBoundGuidFindBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OH_RL_NKClosestPortBoundCodeFindBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OH_CodeBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.StateBoundDropEdit);
			this.MainAddressDetailsGroupBox.Controls.Add(this.RegistrationNumberTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OH_WebBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.PhoneNumberControl);
			this.MainAddressDetailsGroupBox.Controls.Add(this.MobilePhoneNumberControl);
			this.MainAddressDetailsGroupBox.Controls.Add(this.EmailBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.FaxNumberControl);
			this.MainAddressDetailsGroupBox.Controls.Add(this.PostCodeBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.CityBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.Address2BoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.Address1BoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.ClearFieldsButton);
			this.MainAddressDetailsGroupBox.Controls.Add(this.OH_FullNameBoundTextBox);
			this.MainAddressDetailsGroupBox.Controls.Add(this.DuplicateDetectionStatusIcon);
			this.MainAddressDetailsGroupBox.Controls.Add(this.DuplicateDetectionStatusLabel);
			this.MainAddressDetailsGroupBox.Controls.Add(this.DuplicateExclusionsLabel);
			this.MainAddressDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainAddressDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.MainAddressDetailsGroupBox.Name = "MainAddressDetailsGroupBox";
			this.MainAddressDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 492, true);
			this.MainAddressDetailsGroupBox.TabIndex = 0;
			this.MainAddressDetailsGroupBox.TabStop = false;
			// 
			// OrgSecurityGroupFindBox
			// 
			this.OrgSecurityGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgSecurityGroupFindBox, "MiscServ.OM_GG_OrgSecurityGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_GG_OrgSecurityGroup)));
			this.OrgSecurityGroupFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NameAndAddressDetailsUserControl|OM_GG_OrgSecurityGroup", "Security Group");
			this.OrgSecurityGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 49, true);
			this.OrgSecurityGroupFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.OrgSecurityGroupFindBox.Name = "OrgSecurityGroupFindBox";
			this.OrgSecurityGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.OrgSecurityGroupFindBox.TabIndex = 40;
			this.OrgSecurityGroupFindBox.TabStop = false;
			// 
			// OverrideAdditionalAddressInformationCheckBox
			// 
			this.OverrideAdditionalAddressInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideAdditionalAddressInformationCheckBox, "OH_OverrideAdditionalAddressInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_OverrideAdditionalAddressInformation)));
			this.OverrideAdditionalAddressInformationCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("01bf21da-eb97-40b9-a976-1c193b60ef64", "Override Additional Address Information");
			this.OverrideAdditionalAddressInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 100, true);
			this.OverrideAdditionalAddressInformationCheckBox.Name = "OverrideAdditionalAddressInformationCheckBox";
			this.OverrideAdditionalAddressInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.OverrideAdditionalAddressInformationCheckBox.TabIndex = 5;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "OH_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_Category)));
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 49, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.ShouldResizeByMaxLength = true;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CategoryDropEdit.TabIndex = 39;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "MainAddressCollection.OA_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 179, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 8;
			this.CountryFindBox.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.IsCaptionOverridden = false;
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 127, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 38;
			this.ValidateAddressButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 24, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 35;
			this.ScreenButton.Text = "...";
			this.ScreenButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// ScreenButtonCoverLabel
			//
			this.ScreenButtonCoverLabel.BackColor = System.Drawing.Color.LightGray;
			this.ScreenButtonCoverLabel.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButtonCoverLabel.IsFontBold = true;
			this.ScreenButtonCoverLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 24, true);
			this.ScreenButtonCoverLabel.Name = "ScreenButtonCoverLabel";
			this.ScreenButtonCoverLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButtonCoverLabel.TabIndex = 36;
			this.ScreenButtonCoverLabel.Text = "...";
			this.ScreenButtonCoverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ScreenButtonCoverLabel.Visible = false;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatusDropEdit, "OH_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_ScreeningStatus)));
			this.ScreeningStatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 24, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.ShouldResizeByMaxLength = true;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.ScreeningStatusDropEdit.TabIndex = 34;
			this.ScreeningStatusDropEdit.TabStop = false;
			// 
			// AllBranchesLink
			// 
			this.AllBranchesLink.AutoSize = true;
			this.AllBranchesLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d369b537-bfa6-4b59-8435-7347340f126b", "View all");
			this.AllBranchesLink.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AllBranchesLink.IsFontBold = false;
			this.AllBranchesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.AllBranchesLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 263, true);
			this.AllBranchesLink.Name = "AllBranchesLink";
			this.AllBranchesLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			this.AllBranchesLink.TabIndex = 33;
			this.AllBranchesLink.TabStop = false;
			this.AllBranchesLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AllBranchesLink_LinkClicked);
			// 
			// RegistrationNumberTypeDropEdit
			// 
			this.RegistrationNumberTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegistrationNumberTypeDropEdit, "PrimaryRegistrationNumber+NumberTypeForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PrimaryRegistrationNumber.NumberTypeForDisplay)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegistrationNumberTypeDropEdit, false);
			this.RegistrationNumberTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 441, true);
			this.RegistrationNumberTypeDropEdit.Name = "RegistrationNumberTypeDropEdit";
			this.RegistrationNumberTypeDropEdit.PreBoundMaxLength = 5;
			this.RegistrationNumberTypeDropEdit.ShouldResizeByMaxLength = true;
			this.RegistrationNumberTypeDropEdit.ShowDescriptionBox = false;
			this.RegistrationNumberTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.RegistrationNumberTypeDropEdit.TabIndex = 30;
			this.RegistrationNumberTypeDropEdit.SelectedIndexChanged += RegistrationNumberTypeDropEdit_SelectedIndexChanged;
			// 
			// RegistrationNumberTypeLabel
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTypeLabel, "PrimaryRegistrationNumber+NumberTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PrimaryRegistrationNumber.NumberTypeDescription)));
			this.RegistrationNumberTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 441, true);
			this.RegistrationNumberTypeLabel.Name = "RegistrationNumberTypeLabel";
			this.RegistrationNumberTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.RegistrationNumberTypeLabel.TabIndex = 32;
			// 
			// GoToUrlButton
			// 
			this.GoToUrlButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.GoToUrlButton.IsCaptionOverridden = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoToUrlButton, false);
			this.GoToUrlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 389, true);
			this.GoToUrlButton.Name = "GoToUrlButton";
			this.GoToUrlButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GoToUrlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.GoToUrlButton.TabIndex = 27;
			this.GoToUrlButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.GoToUrlButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.GoToUrlButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.GoToUrlButton.ToolTipCaption = null;
			this.GoToUrlButton.UseVisualStyleBackColor = true;
			this.GoToUrlButton.Click += new System.EventHandler(this.GoToUrlButton_Click);
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "OH_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_Language)));
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 415, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.ShouldResizeByMaxLength = true;
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.LanguageDropEdit.TabIndex = 28;
			// 
			// OH_GBBoundGuidFindBox
			// 
			this.OH_GBBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OH_GBBoundGuidFindBox, "CompanyData+OB_GB_ControllingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_GB_ControllingBranch)));
			this.OH_GBBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 259, true);
			this.OH_GBBoundGuidFindBox.Name = "OH_GBBoundGuidFindBox";
			this.OH_GBBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.OH_GBBoundGuidFindBox.TabIndex = 13;
			// 
			// OH_RL_NKClosestPortBoundCodeFindBox
			// 
			this.OH_RL_NKClosestPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OH_RL_NKClosestPortBoundCodeFindBox, "OH_RL_NKClosestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_RL_NKClosestPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Lookups.ClosestPorts)));
			this.OH_RL_NKClosestPortBoundCodeFindBox.BindToList = "Lookups.ClosestPorts";
			this.OH_RL_NKClosestPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 233, true);
			this.OH_RL_NKClosestPortBoundCodeFindBox.Name = "OH_RL_NKClosestPortBoundCodeFindBox";
			this.OH_RL_NKClosestPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.OH_RL_NKClosestPortBoundCodeFindBox.TabIndex = 12;
			// 
			// OH_CodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OH_CodeBoundTextBox, "OH_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_Code)));
			this.OH_CodeBoundTextBox.CaptionResourceString = null;
			this.OH_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 24, true);
			this.OH_CodeBoundTextBox.Name = "OH_CodeBoundTextBox";
			this.OH_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OH_CodeBoundTextBox.TabIndex = 0;
			// 
			// StateBoundDropEdit
			// 
			this.StateBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateBoundDropEdit, "MainAddressCollection.OA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_State)));
			this.StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 207, true);
			this.StateBoundDropEdit.Name = "StateBoundDropEdit";
			this.StateBoundDropEdit.ShouldResizeByMaxLength = true;
			this.StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.StateBoundDropEdit.TabIndex = 11;
			this.StateBoundDropEdit.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "PrimaryRegistrationNumber+NumberForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PrimaryRegistrationNumber.NumberForDisplay)));
			this.RegistrationNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegistrationNumberTextBox, false);
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 441, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 31;
			// 
			// OH_WebBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OH_WebBoundTextBox, "MainWebURL+PU_URL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainWebURL.PU_URL)));
			this.OH_WebBoundTextBox.CaptionResourceString = null;
			this.OH_WebBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.OH_WebBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 389, true);
			this.OH_WebBoundTextBox.Name = "OH_WebBoundTextBox";
			this.OH_WebBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.OH_WebBoundTextBox.TabIndex = 25;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "MainAddressCollection.PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).PhoneNumber)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NameAndAddressDetailsUserControl|OA_Phone_Formatted", "Phone");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 285, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.PhoneNumberControl.TabIndex = 18;
			// 
			// MobilePhoneNumberControl
			// 
			this.MobilePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "MainAddressCollection.MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).MobilePhoneNumber)));
			this.MobilePhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NameAndAddressDetailsUserControl|OA_Mobile_Formatted", "Mobile");
			this.MobilePhoneNumberControl.EnableValidStateColor = true;
			this.MobilePhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 311, true);
			this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
			this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
			this.MobilePhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.MobilePhoneNumberControl.TabIndex = 20;
			// 
			// EmailBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailBoundTextBox, "MainAddressCollection.OA_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_Email)));
			this.EmailBoundTextBox.CaptionResourceString = null;
			this.EmailBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.EmailBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 363, true);
			this.EmailBoundTextBox.Name = "EmailBoundTextBox";
			this.EmailBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EmailBoundTextBox.TabIndex = 24;
			// 
			// FaxNumberControl
			// 
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "MainAddressCollection.FaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).FaxNumber)));
			this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NameAndAddressDetailsUserControl|OA_Fax_Formatted", "Fax");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 337, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.FaxNumberControl.TabIndex = 22;
			// 
			// PostCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeBoundTextBox, "MainAddressCollection.OA_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_PostCode)));
			this.PostCodeBoundTextBox.CaptionResourceString = null;
			this.PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 207, true);
			this.PostCodeBoundTextBox.Name = "PostCodeBoundTextBox";
			this.PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PostCodeBoundTextBox.TabIndex = 9;
			this.PostCodeBoundTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// CityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityBoundTextBox, "MainAddressCollection.OA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_City)));
			this.CityBoundTextBox.CaptionResourceString = null;
			this.CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 181, true);
			this.CityBoundTextBox.Name = "CityBoundTextBox";
			this.CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.CityBoundTextBox.TabIndex = 10;
			this.CityBoundTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// Address2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.Address2BoundTextBox, "MainAddressCollection.OA_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_Address2)));
			this.Address2BoundTextBox.CaptionResourceString = null;
			this.Address2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 153, true);
			this.Address2BoundTextBox.Name = "Address2BoundTextBox";
			this.Address2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.Address2BoundTextBox.TabIndex = 7;
			this.Address2BoundTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// Address1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.Address1BoundTextBox, "MainAddressCollection.OA_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MainAddressCollection)).SyncRoot)).OA_Address1)));
			this.Address1BoundTextBox.CaptionResourceString = null;
			this.Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 127, true);
			this.Address1BoundTextBox.Name = "Address1BoundTextBox";
			this.Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 20, true);
			this.Address1BoundTextBox.TabIndex = 6;
			this.Address1BoundTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.IsCaptionOverridden = false;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(517, 125, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 12;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += ClearFieldsButton_Click;
			// 
			// OH_FullNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OH_FullNameBoundTextBox, "OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_FullName)));
			this.OH_FullNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 74, true);
			this.OH_FullNameBoundTextBox.Name = "OH_FullNameBoundTextBox";
			this.OH_FullNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.OH_FullNameBoundTextBox.TabIndex = 3;
			// 
			// DuplicateDetectionStatusIcon
			// 
			this.DuplicateDetectionStatusIcon.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.loader;
			this.DuplicateDetectionStatusIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 74, true);
			this.DuplicateDetectionStatusIcon.Name = "DuplicateDetectionStatusIcon";
			this.DuplicateDetectionStatusIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.DuplicateDetectionStatusIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.DuplicateDetectionStatusIcon.TabIndex = 42;
			this.DuplicateDetectionStatusIcon.TabStop = false;
			this.DuplicateDetectionStatusIcon.Visible = false;
			// 
			// DuplicateDetectionStatusLabel
			// 
			this.DuplicateDetectionStatusLabel.AutoSize = true;
			this.DuplicateDetectionStatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ECF03C60-8216-4EC8-BBA9-3CB30F0A8E4A", "Detecting duplicates");
			this.DuplicateDetectionStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DuplicateDetectionStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 74, true);
			this.DuplicateDetectionStatusLabel.Name = "DuplicateDetectionStatusLabel";
			this.DuplicateDetectionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DuplicateDetectionStatusLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2, true);
			this.DuplicateDetectionStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.DuplicateDetectionStatusLabel.TabIndex = 43;
			this.DuplicateDetectionStatusLabel.Visible = false;
			// 
			// DuplicateExclusionsLabel
			// 
			this.DuplicateExclusionsLabel.AutoSize = true;
			this.DuplicateExclusionsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42c6a5c7-ab89-4cbd-9247-7c600b85d5be", "Excluded records: 0");
			this.DuplicateExclusionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DuplicateExclusionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 74, true);
			this.DuplicateExclusionsLabel.Name = "DuplicateExclusionsLabel";
			this.DuplicateExclusionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DuplicateExclusionsLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2, true);
			this.DuplicateExclusionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.DuplicateExclusionsLabel.TabIndex = 44;
			this.DuplicateExclusionsLabel.Visible = false;
			// 
			// NameAndAddressDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainDetailsPanel);
			this.Name = "NameAndAddressDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 502, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainDetailsPanel.ResumeLayout(false);
			this.MainDetailsPanel.PerformLayout();
			this.MainAddressDetailsGroupBox.ResumeLayout(false);
			this.MainAddressDetailsGroupBox.PerformLayout();
			this.OrgSecurityGroupFindBox.ResumeLayout(true);
			this.OrgSecurityGroupFindBox.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.RegistrationNumberTypeDropEdit.ResumeLayout(true);
			this.RegistrationNumberTypeDropEdit.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.OH_GBBoundGuidFindBox.ResumeLayout(true);
			this.OH_GBBoundGuidFindBox.PerformLayout();
			this.OH_RL_NKClosestPortBoundCodeFindBox.ResumeLayout(true);
			this.OH_RL_NKClosestPortBoundCodeFindBox.PerformLayout();
			this.StateBoundDropEdit.ResumeLayout(true);
			this.StateBoundDropEdit.PerformLayout();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.MobilePhoneNumberControl.ResumeLayout(true);
			this.MobilePhoneNumberControl.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.DuplicateDetectionStatusIcon.ResumeLayout(true);
			this.DuplicateDetectionStatusIcon.PerformLayout();
			this.DuplicateDetectionStatusLabel.ResumeLayout(true);
			this.DuplicateDetectionStatusLabel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel RegistrationNumberTypeLabel;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit RegistrationNumberTypeDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox RegistrationNumberTextBox;
		protected ZLinkLabel AllBranchesLink;
		protected Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
		protected ZButton ScreenButton;
		protected ZButton ValidateAddressButton;
		private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		private ZDropEdit CategoryDropEdit;
		private ZGuidFindBox OrgSecurityGroupFindBox;
		private ZLabel ScreenButtonCoverLabel;
	}
}
