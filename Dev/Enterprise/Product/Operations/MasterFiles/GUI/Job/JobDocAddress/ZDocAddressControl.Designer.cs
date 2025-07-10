using System.CodeDom;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class ZDocAddressControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OverrideAddressCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompanyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalAddressInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressLine1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressLine2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EMailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.ResidentialAddressCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.ContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.ContactLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GovernmentRegistrationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GovernmentRegistrationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.GovernmentRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PassportDataTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PassportDataEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.convertToOrganizationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OverrideGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressValidationStatusButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DefaultGroupBox = new Enterprise.MasterFiles.GUI.Internal.ZDocAddressOrganisationControl();
			this.CutDownGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CutDownAddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.CutDownOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.CutDownSingleLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CutDownSingleLineAddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.CutDownSingleLineOrgFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.CutDownSingleLineNoGroupBoxAddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.CutDownSingleLineNoGroupBoxOrgFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.SingleLineNoGroupBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SingleLineNoGroupBoxOverridePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CompactLayoutGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CompactLayoutPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CompactOrganizationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.CompactLayoutNameAndAddressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CompactAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompactFullNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompactAddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.CompactOverrideTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CompactAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompactOverriddenPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactOverriddenCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactOverriddenCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CompactOverriddenAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactOverriddenCompanyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompactOverriddenPhoneNumberUserControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.CompactOverriddenEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactOverriddenContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompactOverrideLayoutGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CompactWithContactTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CompactAddressTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompactContactTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompactContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CompactContactEmailAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompactContactPhoneNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PhoneNumberControl.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.AddressTabPage.SuspendLayout();
			this.AddressTypeDropEdit.SuspendLayout();
			this.StateDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.ContactTabPage.SuspendLayout();
			this.MobilePhoneNumberControl.SuspendLayout();
			this.GovernmentRegistrationTabPage.SuspendLayout();
			this.GovernmentRegistrationTypeDropEdit.SuspendLayout();
			this.OverrideGroupBox.SuspendLayout();
			this.DefaultGroupBox.SuspendLayout();
			this.CutDownGroupBox.SuspendLayout();
			this.CutDownAddressDropEdit.SuspendLayout();
			this.CutDownOrganisationFindBox.SuspendLayout();
			this.CutDownSingleLineGroupBox.SuspendLayout();
			this.CutDownSingleLineAddressDropEdit.SuspendLayout();
			this.CutDownSingleLineOrgFindBox.SuspendLayout();
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.SuspendLayout();
			this.CutDownSingleLineNoGroupBoxOrgFindBox.SuspendLayout();
			this.SingleLineNoGroupBoxPanel.SuspendLayout();
			this.SingleLineNoGroupBoxOverridePanel.SuspendLayout();
			this.CompactLayoutGroupBox.SuspendLayout();
			this.CompactOrganizationFindBox.SuspendLayout();
			this.CompactAddressDropEdit.SuspendLayout();
			this.CompactLayoutNameAndAddressPanel.SuspendLayout();
			this.CompactOverrideTabControl.SuspendLayout();
			this.CompactAddressTabPage.SuspendLayout();
			this.CompactOverriddenCountryFindBox.SuspendLayout();
			this.CompactContactTabPage.SuspendLayout();
			this.CompactOverriddenPhoneNumberUserControl.SuspendLayout();
			this.CompactOverrideLayoutGroupBox.SuspendLayout();
			this.CompactWithContactTabControl.SuspendLayout();
			this.CompactContactTab.SuspendLayout();
			this.CompactAddressTab.SuspendLayout();
			this.CompactContactDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobDocAddress);
			// 
			// OverrideAddressCheckbox
			// 
			this.BindingSource.SetBindingMember(this.OverrideAddressCheckbox, "E2_AddressOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_AddressOverride)));
			this.OverrideAddressCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OverrideAddressCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 0, true);
			this.OverrideAddressCheckbox.Name = "OverrideAddressCheckbox";
			this.OverrideAddressCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.OverrideAddressCheckbox.TabIndex = 2;
			this.OverrideAddressCheckbox.TabStop = false;
			this.OverrideAddressCheckbox.UseVisualStyleBackColor = false;
			this.OverrideAddressCheckbox.CheckedChanged += OverrideAddressCheckbox_CheckedChanged;
			// 
			// CompanyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyTextBox, "E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			this.CompanyTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|6b7eb65d-1f66-4ba6-9bde-27b6f57893eb", "Co.");
			this.CompanyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 3, true);
			this.CompanyTextBox.Name = "CompanyTextBox";
			this.CompanyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.CompanyTextBox.TabIndex = 0;
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 45, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 12;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += ClearFieldsButton_Click;
			// 
			// AdditionalAddressInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalAddressInformationTextBox, "UnrestrictedAdditionalAddressInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).UnrestrictedAdditionalAddressInformation)));
			this.AdditionalAddressInformationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|344dfc83-fe15-4a7f-ad34-30b8fe89b231", "Note");
			this.AdditionalAddressInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 24, true);
			this.AdditionalAddressInformationTextBox.Name = "AdditionalAddressInformationTextBox";
			this.AdditionalAddressInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.AdditionalAddressInformationTextBox.TabIndex = 1;
			this.AdditionalAddressInformationTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// AddressLine1TextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressLine1TextBox, "E2_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address1)));
			this.AddressLine1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 45, true);
			this.AddressLine1TextBox.Name = "AddressLine1TextBox";
			this.AddressLine1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.AddressLine1TextBox.TabIndex = 1;
			this.AddressLine1TextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// AddressLine2TextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressLine2TextBox, "E2_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address2)));
			this.AddressLine2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 67, true);
			this.AddressLine2TextBox.Name = "AddressLine2TextBox";
			this.AddressLine2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.AddressLine2TextBox.TabIndex = 2;
			this.AddressLine2TextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "E2_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_City)));
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 89, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.CityTextBox.TabIndex = 5;
			this.CityTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "E2_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Postcode)));
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 111, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.PostCodeTextBox.TabIndex = 4;
			this.PostCodeTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// EMailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EMailTextBox, "E2_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Email)));
			this.EMailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EMailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 32, true);
			this.EMailTextBox.Name = "EMailTextBox";
			this.EMailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.EMailTextBox.TabIndex = 2;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).PhoneNumber)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|E2_Phone", "Ph", "Phone", "Telephone Number");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.ShowLocalNumberLabel = false;
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.ShowToolTip = true;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.PhoneNumberControl.TabIndex = 3;
			this.PhoneNumberControl.UnscaledLeftPadding = -29;
			// 
			// FaxNumberControl
			// 
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "FaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).FaxNumber)));
			this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|E2_Fax", "Fax", "Fax Number");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.ShowToolTip = true;
			this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.FaxNumberControl.TabIndex = 5;
			this.FaxNumberControl.UnscaledLeftPadding = -29;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.AddressTabPage);
			this.DetailsTabControl.Controls.Add(this.ContactTabPage);
			this.DetailsTabControl.Controls.Add(this.GovernmentRegistrationTabPage);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.ShowToolTips = true;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 159, true);
			this.DetailsTabControl.TabIndex = 0;
			this.DetailsTabControl.SelectedIndexChanged += new System.EventHandler(this.DetailsTabControl_SelectedIndexChanged);
			// 
			// AddressTabPage
			// 
			this.AddressTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|6890cf7c-4e81-43d3-a7d5-ae3636a202cc", "Addr.", "Address", "Address Details");
			this.AddressTabPage.ToolTipText = AddressTabPage.CaptionResourceString.Caption;
			this.AddressTabPage.Controls.Add(this.AddressTypeDropEdit);
			this.AddressTabPage.Controls.Add(this.ResidentialAddressCheckBox);
			this.AddressTabPage.Controls.Add(this.AdditionalAddressInformationTextBox);
			this.AddressTabPage.Controls.Add(this.AddressLine1TextBox);
			this.AddressTabPage.Controls.Add(this.AddressLine2TextBox);
			this.AddressTabPage.Controls.Add(this.PostCodeTextBox);
			this.AddressTabPage.Controls.Add(this.CompanyTextBox);
			this.AddressTabPage.Controls.Add(this.StateDropEdit);
			this.AddressTabPage.Controls.Add(this.CityTextBox);
			this.AddressTabPage.Controls.Add(this.CountryFindBox);
			this.AddressTabPage.Controls.Add(this.ClearFieldsButton);
			this.AddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressTabPage.Name = "AddressTabPage";
			this.AddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 132, true);
			this.AddressTabPage.TabIndex = 0;
			// 
			// AddressTypeDropEdit
			// 
			this.AddressTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressTypeDropEdit, "ResidentialCommercialAddressType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).ResidentialCommercialAddressType)));
			this.AddressTypeDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AddressTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bd083b37-76fd-4da1-9a1f-7da455f52f26", "Type");
			this.AddressTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 24, true);
			this.AddressTypeDropEdit.Name = "AddressTypeDropEdit";
			this.AddressTypeDropEdit.PreBoundMaxLength = 4;
			this.AddressTypeDropEdit.ShouldResizeByMaxLength = true;
			this.AddressTypeDropEdit.ShowDescriptionBox = false;
			this.AddressTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.AddressTypeDropEdit.TabIndex = 6;
			this.AddressTypeDropEdit.Visible = false;
			// 
			// ResidentialAddressCheckBox
			// 
			this.ResidentialAddressCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ResidentialAddressCheckBox, "E2_IsResidential");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_IsResidential)));
			this.ResidentialAddressCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 23, true);
			this.ResidentialAddressCheckBox.Name = "ResidentialAddressCheckBox";
			this.ResidentialAddressCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.ResidentialAddressCheckBox.TabIndex = 9;
			this.ResidentialAddressCheckBox.TabStop = false;
			this.ResidentialAddressCheckBox.UseVisualStyleBackColor = false;
			this.ResidentialAddressCheckBox.Visible = false;
			// 
			// StateDropEdit
			// 
			this.StateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateDropEdit, "E2_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_State)));
			this.StateDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.StateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 111, true);
			this.StateDropEdit.Name = "StateDropEdit";
			this.StateDropEdit.PreBoundMaxLength = 4;
			this.StateDropEdit.ShowDescriptionBox = false;
			this.StateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.StateDropEdit.TabIndex = 6;
			this.StateDropEdit.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "E2_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 89, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryFindBox.ParentType = null;
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 3;
			this.CountryFindBox.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// ContactTabPage
			// 
			this.ContactTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|73bb175f-51e3-457c-9a21-a51ea5770933", "Con.", "Contact", "Contact Details");
			this.ContactTabPage.Controls.Add(this.ContactTextBox);
			this.ContactTabPage.Controls.Add(this.EMailTextBox);
			this.ContactTabPage.Controls.Add(this.FaxNumberControl);
			this.ContactTabPage.Controls.Add(this.PhoneNumberControl);
			this.ContactTabPage.Controls.Add(this.MobilePhoneNumberControl);
			this.ContactTabPage.Controls.Add(this.ContactLabel);
			this.ContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContactTabPage.Name = "ContactTabPage";
			this.ContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 132, true);
			this.ContactTabPage.TabIndex = 1;
			this.ContactTabPage.ToolTipText = ContactTabPage.CaptionResourceString.Caption;
			// 
			// ContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactTextBox, "E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			this.ContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 8, true);
			this.ContactTextBox.Name = "ContactTextBox";
			this.ContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.ContactTextBox.TabIndex = 1;
			// 
			// MobilePhoneNumberControl
			// 
			this.MobilePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).MobilePhoneNumber)));
			this.MobilePhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|E2_Mobile", "Mob", "Mobile", "Mobile Number");
			this.MobilePhoneNumberControl.EnableValidStateColor = true;
			this.MobilePhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
			this.MobilePhoneNumberControl.ShowLocalNumberLabel = false;
			this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
			this.MobilePhoneNumberControl.ShowToolTip = true;
			this.MobilePhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.MobilePhoneNumberControl.TabIndex = 4;
			this.MobilePhoneNumberControl.UnscaledLeftPadding = -29;
			// 
			// ContactLabel
			// 
			this.ContactLabel.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContactLabel, false);
			this.ContactLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 11, true);
			this.ContactLabel.Name = "ContactLabel";
			this.ContactLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ContactLabel.TabIndex = 0;
			// 
			// GovernmentRegistrationTabPage
			// 
			this.GovernmentRegistrationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|A1848D03-8935-4561-99CE-AB4BB2EA7D85", "Cod.", "Code", "Code Details");
			this.GovernmentRegistrationTabPage.Controls.Add(this.GovernmentRegistrationTypeDropEdit);
			this.GovernmentRegistrationTabPage.Controls.Add(this.GovernmentRegistrationNumberTextBox);
			this.GovernmentRegistrationTabPage.Controls.Add(this.PassportDataTextBox);
			this.GovernmentRegistrationTabPage.Controls.Add(this.PassportDataEditButton);
			this.GovernmentRegistrationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GovernmentRegistrationTabPage.Name = "GovernmentRegistrationTabPage";
			this.GovernmentRegistrationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 132, true);
			this.GovernmentRegistrationTabPage.TabIndex = 2;
			this.GovernmentRegistrationTabPage.ToolTipText = GovernmentRegistrationTabPage.CaptionResourceString.Caption;
			// 
			// GovernmentRegistrationTypeDropEdit
			// 
			this.GovernmentRegistrationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GovernmentRegistrationTypeDropEdit, "E2_GovRegNumType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_GovRegNumType)));
			this.GovernmentRegistrationTypeDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GovernmentRegistrationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 8, true);
			this.GovernmentRegistrationTypeDropEdit.Name = "GovernmentRegistrationTypeDropEdit";
			this.GovernmentRegistrationTypeDropEdit.PreBoundMaxLength = 3;
			this.GovernmentRegistrationTypeDropEdit.ShowDescriptionBox = false;
			this.GovernmentRegistrationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.GovernmentRegistrationTypeDropEdit.TabIndex = 7;
			// 
			// GovernmentRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.GovernmentRegistrationNumberTextBox, "E2_GovRegNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_GovRegNum)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GovernmentRegistrationNumberTextBox, false);
			this.GovernmentRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 8, true);
			this.GovernmentRegistrationNumberTextBox.Name = "GovernmentRegistrationNumberTextBox";
			this.GovernmentRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20, true);
			this.GovernmentRegistrationNumberTextBox.TabIndex = 8;
			// 
			// PassportDataTextBox
			// 
			this.BindingSource.SetBindingMember(this.PassportDataTextBox, "E2_PassportDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_PassportDetails)));
			this.PassportDataTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 8, true);
			this.PassportDataTextBox.Name = "PassportDataTextBox";
			this.PassportDataTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.PassportDataTextBox.TabIndex = 10;
			// 
			// PassportDataEditButton
			// 
			this.PassportDataEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|7a47e852-56d7-4581-b00b-d530b61e84f0", "&Edit");
			this.PassportDataEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 5, true);
			this.PassportDataEditButton.Name = "PassportDataEditButton";
			this.PassportDataEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23, true);
			this.PassportDataEditButton.TabIndex = 11;
			this.PassportDataEditButton.ToolTipCaption = null;
			this.PassportDataEditButton.UseVisualStyleBackColor = true;
			this.PassportDataEditButton.Click += new System.EventHandler(this.PassportDataEditButton_Click);
			// 
			// convertToOrganizationButton
			// 
			this.convertToOrganizationButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.convert_to_organization;
			this.convertToOrganizationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 17, true);
			this.convertToOrganizationButton.Name = "convertToOrganizationButton";
			this.convertToOrganizationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 22, true);
			this.convertToOrganizationButton.TabIndex = 12;
			this.convertToOrganizationButton.ToolTipCaption = null;
			this.convertToOrganizationButton.UseVisualStyleBackColor = true;
			this.convertToOrganizationButton.Click += new System.EventHandler(this.ConvertToOrganizationButton_Click);
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 17, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 11;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// OverrideGroupBox
			// 
			this.OverrideGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|8c23b09f-c738-44a6-b6f4-cfb874de6599", "Overridden");
			this.OverrideGroupBox.Controls.Add(this.ValidateAddressButton);
			this.OverrideGroupBox.Controls.Add(this.convertToOrganizationButton);
			this.OverrideGroupBox.Controls.Add(this.DetailsTabControl);
			this.OverrideGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true);
			this.OverrideGroupBox.Name = "OverrideGroupBox";
			this.OverrideGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.OverrideGroupBox.TabIndex = 0;
			this.OverrideGroupBox.TabStop = false;
			this.OverrideGroupBox.Text = "Overridden";
			this.OverrideGroupBox.Visible = false;
			// 
			// AddressValidationStatusButton
			// 
			this.AddressValidationStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 17, true);
			this.AddressValidationStatusButton.Name = "AddressValidationStatusButton";
			this.AddressValidationStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.AddressValidationStatusButton.TabIndex = 3;
			this.AddressValidationStatusButton.Text = " ";
			this.AddressValidationStatusButton.Visible = false;
			this.AddressValidationStatusButton.Click += new System.EventHandler(this.AddressValidationStatusButton_Click);
			// 
			// DefaultGroupBox
			// 
			this.DefaultGroupBox.AdditionalAddressInfoBindTo = "UnrestrictedAdditionalAddressInformation";
			this.DefaultGroupBox.AddressBindTo = "E2_OA_Address";
			this.DefaultGroupBox.AllowDrop = true;
			this.DefaultGroupBox.Captions = new string[0];
			this.DefaultGroupBox.ContactBindTo = "E2_Contact";
			this.DefaultGroupBox.IsCaptionOverridden = false;
			this.DefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultGroupBox.Name = "DefaultGroupBox";
			this.DefaultGroupBox.PopupCaption = "";
			this.DefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 172, true);
			this.DefaultGroupBox.TabIndex = 1;
			this.DefaultGroupBox.AddressEdit.CodeBox.TextChanged += CodeBox_TextChanged;
			// 
			// CutDownGroupBox
			// 
			this.CutDownGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZDocAddressControl|4d95e609-8c9f-4a4f-81e5-86d7dfb69798", "Cut Down");
			this.CutDownGroupBox.Controls.Add(this.CutDownAddressDropEdit);
			this.CutDownGroupBox.Controls.Add(this.CutDownOrganisationFindBox);
			this.CutDownGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 0, true);
			this.CutDownGroupBox.Name = "CutDownGroupBox";
			this.CutDownGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 68, true);
			this.CutDownGroupBox.TabIndex = 4;
			this.CutDownGroupBox.TabStop = false;
			this.CutDownGroupBox.Text = "Cut Down";
			this.CutDownGroupBox.Visible = false;
			// 
			// CutDownAddressDropEdit
			// 
			this.CutDownAddressDropEdit.AllowDrop = true;
			this.CutDownAddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CutDownAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 39, true);
			this.CutDownAddressDropEdit.Name = "CutDownAddressDropEdit";
			this.CutDownAddressDropEdit.PreBoundMaxLength = 36;
			this.CutDownAddressDropEdit.ShowDescriptionBox = false;
			this.CutDownAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.CutDownAddressDropEdit.TabIndex = 1;
			// 
			// CutDownOrganisationFindBox
			// 
			this.CutDownOrganisationFindBox.AllowDrop = true;
			this.CutDownOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 17, true);
			this.CutDownOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CutDownOrganisationFindBox.Name = "CutDownOrganisationFindBox";
			this.CutDownOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CutDownOrganisationFindBox.ParentType = null;
			this.CutDownOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.CutDownOrganisationFindBox.TabIndex = 0;
			// 
			// CutDownSingleLineGroupBox
			// 
			this.CutDownSingleLineGroupBox.Controls.Add(this.CutDownSingleLineAddressDropEdit);
			this.CutDownSingleLineGroupBox.Controls.Add(this.CutDownSingleLineOrgFindBox);
			this.CutDownSingleLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 0, true);
			this.CutDownSingleLineGroupBox.Name = "CutDownSingleLineGroupBox";
			this.CutDownSingleLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 45, true);
			this.CutDownSingleLineGroupBox.TabIndex = 5;
			this.CutDownSingleLineGroupBox.TabStop = false;
			this.CutDownSingleLineGroupBox.Text = "Cut Down Single Line";
			this.CutDownSingleLineGroupBox.Visible = false;
			// 
			// CutDownSingleLineAddressDropEdit
			// 
			this.CutDownSingleLineAddressDropEdit.AllowDrop = true;
			this.CutDownSingleLineAddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CutDownSingleLineAddressDropEdit.FilterAddressedByDefaultType = true;
			this.CutDownSingleLineAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 17, true);
			this.CutDownSingleLineAddressDropEdit.Name = "CutDownSingleLineAddressDropEdit";
			this.CutDownSingleLineAddressDropEdit.PreBoundMaxLength = 36;
			this.CutDownSingleLineAddressDropEdit.ShowDescriptionBox = false;
			this.CutDownSingleLineAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.CutDownSingleLineAddressDropEdit.TabIndex = 1;
			// 
			// CutDownSingleLineOrgFindBox
			// 
			this.CutDownSingleLineOrgFindBox.AllowDrop = true;
			this.CutDownSingleLineOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 17, true);
			this.CutDownSingleLineOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CutDownSingleLineOrgFindBox.Name = "CutDownSingleLineOrgFindBox";
			this.CutDownSingleLineOrgFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CutDownSingleLineOrgFindBox.ParentType = null;
			this.CutDownSingleLineOrgFindBox.ShowDescriptionBox = false;
			this.CutDownSingleLineOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CutDownSingleLineOrgFindBox.TabIndex = 0;
			// 
			// CutDownSingleLineNoGroupBoxAddressDropEdit
			// 
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.AllowDrop = true;
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.FilterAddressedByDefaultType = true;
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.Name = "CutDownSingleLineNoGroupBoxAddressDropEdit";
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength = 30;
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.ShowDescriptionBox = false;
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.TabIndex = 7;
			// 
			// CutDownSingleLineNoGroupBoxOrgFindBox
			// 
			this.CutDownSingleLineNoGroupBoxOrgFindBox.AllowDrop = true;
			this.CutDownSingleLineNoGroupBoxOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CutDownSingleLineNoGroupBoxOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CutDownSingleLineNoGroupBoxOrgFindBox.Name = "CutDownSingleLineNoGroupBoxOrgFindBox";
			this.CutDownSingleLineNoGroupBoxOrgFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CutDownSingleLineNoGroupBoxOrgFindBox.ParentType = null;
			this.CutDownSingleLineNoGroupBoxOrgFindBox.ShowDescriptionBox = false;
			this.CutDownSingleLineNoGroupBoxOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CutDownSingleLineNoGroupBoxOrgFindBox.TabIndex = 6;
			// 
			// SingleLineNoGroupBoxPanel
			// 
			this.SingleLineNoGroupBoxPanel.Controls.Add(this.CutDownSingleLineNoGroupBoxOrgFindBox);
			this.SingleLineNoGroupBoxPanel.Controls.Add(this.CutDownSingleLineNoGroupBoxAddressDropEdit);
			this.SingleLineNoGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1116, 0, true);
			this.SingleLineNoGroupBoxPanel.Name = "SingleLineNoGroupBoxPanel";
			this.SingleLineNoGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.SingleLineNoGroupBoxPanel.TabIndex = 8;
			// 
			// CutDownSingleLineNoGroupBoxOrgOverrideTextBox
			// 
			this.BindingSource.SetBindingMember(this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox, "E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Name = "CutDownSingleLineNoGroupBoxOrgOverrideTextBox";
			this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.TabIndex = 9;
			// 
			// SingleLineNoGroupBoxOverridePanel
			// 
			this.SingleLineNoGroupBoxOverridePanel.Controls.Add(this.CutDownSingleLineNoGroupBoxOrgOverrideTextBox);
			this.SingleLineNoGroupBoxOverridePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1116, 25, true);
			this.SingleLineNoGroupBoxOverridePanel.Name = "SingleLineNoGroupBoxOverridePanel";
			this.SingleLineNoGroupBoxOverridePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.SingleLineNoGroupBoxOverridePanel.TabIndex = 10;
			// 
			// CompactLayoutGroupBox
			// 
			this.CompactLayoutGroupBox.Controls.Add(this.CompactLayoutPanel);
			this.CompactLayoutGroupBox.Controls.Add(this.CompactOrganizationFindBox);
			this.CompactLayoutGroupBox.Controls.Add(this.CompactAddressDropEdit);
			this.CompactLayoutGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1418, 0, true);
			this.CompactLayoutGroupBox.Name = "CompactLayoutGroupBox";
			this.CompactLayoutGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 130, true);
			this.CompactLayoutGroupBox.TabIndex = 11;
			this.CompactLayoutGroupBox.TabStop = false;
			this.CompactLayoutGroupBox.Text = "Compact";
			this.CompactLayoutGroupBox.Visible = false;
			// 
			// CompactLayoutPanel
			// 
			this.CompactLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 37, true);
			this.CompactLayoutPanel.Name = "CompactLayoutPanel";
			this.CompactLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 91, true);
			this.CompactLayoutPanel.TabIndex = 3;
			// 
			// CompactOrganizationFindBox
			// 
			this.CompactOrganizationFindBox.AllowDrop = true;
			this.CompactOrganizationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.CompactOrganizationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CompactOrganizationFindBox.Name = "CompactOrganizationFindBox";
			this.CompactOrganizationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CompactOrganizationFindBox.ParentType = null;
			this.CompactOrganizationFindBox.ShowDescriptionBox = false;
			this.CompactOrganizationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CompactOrganizationFindBox.TabIndex = 1;
			// 
			// CompactAddressDropEdit
			// 
			this.CompactAddressDropEdit.AllowDrop = true;
			this.CompactAddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CompactAddressDropEdit.FilterAddressedByDefaultType = true;
			this.CompactAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.CompactAddressDropEdit.Name = "CompactAddressDropEdit";
			this.CompactAddressDropEdit.PreBoundMaxLength = 14;
			this.CompactAddressDropEdit.ShowDescriptionBox = false;
			this.CompactAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.CompactAddressDropEdit.TabIndex = 2;
			// 
			// CompactLayoutNameAndAddressPanel
			// 
			this.CompactLayoutNameAndAddressPanel.Controls.Add(this.CompactAddressLabel);
			this.CompactLayoutNameAndAddressPanel.Controls.Add(this.CompactFullNameLabel);
			this.CompactLayoutNameAndAddressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1419, 234, true);
			this.CompactLayoutNameAndAddressPanel.Name = "CompactLayoutNameAndAddressPanel";
			this.CompactLayoutNameAndAddressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 87, true);
			this.CompactLayoutNameAndAddressPanel.TabIndex = 5;
			this.CompactLayoutNameAndAddressPanel.Visible = false;
			// 
			// CompactAddressLabel
			// 
			this.CompactAddressLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CompactAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompactAddressLabel, false);
			this.CompactAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.CompactAddressLabel.Name = "CompactAddressLabel";
			this.CompactAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 75, true);
			this.CompactAddressLabel.TabIndex = 5;
			this.CompactAddressLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// CompactFullNameLabel
			// 
			this.CompactFullNameLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CompactFullNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CompactFullNameLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompactFullNameLabel, false);
			this.CompactFullNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompactFullNameLabel.Name = "CompactFullNameLabel";
			this.CompactFullNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 12, true);
			this.CompactFullNameLabel.TabIndex = 4;
			this.CompactFullNameLabel.UseMnemonic = false;
			// 
			// CompactOverrideTabControl
			// 
			this.CompactOverrideTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CompactOverrideTabControl.Controls.Add(this.CompactAddressTabPage);
			this.CompactOverrideTabControl.Controls.Add(this.CompactContactTabPage);
			this.CompactOverrideTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompactOverrideTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.CompactOverrideTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CompactOverrideTabControl.Name = "CompactOverrideTabControl";
			this.CompactOverrideTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 116, true);
			this.CompactOverrideTabControl.TabIndex = 12;
			// 
			// CompactAddressTabPage
			//
			this.CompactAddressTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6CCAC330-671B-47FA-8389-72B4CE4F253B", "Addr.", "Address", "Address Details");
			this.CompactAddressTabPage.Controls.Add(this.CompactOverriddenPostCodeTextBox);
			this.CompactAddressTabPage.Controls.Add(this.CompactOverriddenCityTextBox);
			this.CompactAddressTabPage.Controls.Add(this.CompactOverriddenCountryFindBox);
			this.CompactAddressTabPage.Controls.Add(this.CompactOverriddenAddressTextBox);
			this.CompactAddressTabPage.Controls.Add(this.CompactOverriddenCompanyNameTextBox);
			this.CompactAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompactAddressTabPage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CompactAddressTabPage.Name = "CompactAddressTabPage";
			this.CompactAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 89, true);
			this.CompactAddressTabPage.TabIndex = 0;
			this.CompactAddressTabPage.ToolTipText = CompactAddressTabPage.CaptionResourceString.Caption;
			// 
			// CompactOverriddenPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenPostCodeTextBox, "E2_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Postcode)));
			this.CompactOverriddenPostCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7AB87982-285C-4A70-BF9D-5EA093AFC1D9", "Postc.", "Postcode", "Address Postcode");
			this.CompactOverriddenPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 68, true);
			this.CompactOverriddenPostCodeTextBox.Name = "CompactOverriddenPostCodeTextBox";
			this.CompactOverriddenPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CompactOverriddenPostCodeTextBox.TabIndex = 4;
			// 
			// CompactOverriddenCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenCityTextBox, "E2_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_City)));
			this.CompactOverriddenCityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("70A7C70C-85D0-4351-AAA4-4AD255E25EE1", "City");
			this.CompactOverriddenCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompactOverriddenCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 46, true);
			this.CompactOverriddenCityTextBox.Name = "CompactOverriddenCityTextBox";
			this.CompactOverriddenCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.CompactOverriddenCityTextBox.TabIndex = 3;
			// 
			// CompactOverriddenCountryFindBox
			// 
			this.CompactOverriddenCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompactOverriddenCountryFindBox, "E2_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_RN_NKCountryCode)));
			this.CompactOverriddenCountryFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EAC931F1-3109-4D3B-A8C4-76171FA3FAFF", "Ctry.", "Country", "Country Code");
			this.CompactOverriddenCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 46, true);
			this.CompactOverriddenCountryFindBox.Name = "CompactOverriddenCountryFindBox";
			this.CompactOverriddenCountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CompactOverriddenCountryFindBox.ParentType = null;
			this.CompactOverriddenCountryFindBox.PreBoundMaxLength = 2;
			this.CompactOverriddenCountryFindBox.ShowDescriptionBox = false;
			this.CompactOverriddenCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.CompactOverriddenCountryFindBox.TabIndex = 2;
			// 
			// CompactOverriddenAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenAddressTextBox, "E2_Address1AndE2_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address1AndE2_Address2)));
			this.CompactOverriddenAddressTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("923D0223-6412-4547-9AC6-1E2109E3713B", "Addr.", "Address", "Address Line");
			this.CompactOverriddenAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompactOverriddenAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 24, true);
			this.CompactOverriddenAddressTextBox.Name = "CompactOverriddenAddressTextBox";
			this.CompactOverriddenAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CompactOverriddenAddressTextBox.TabIndex = 1;
			// 
			// CompactOverriddenCompanyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenCompanyNameTextBox, "E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			this.CompactOverriddenCompanyNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E49C9E89-B33E-47B0-B101-A63F9231E96E", "Co.", "Company", "Company Name", "Address Company Name");
			this.CompactOverriddenCompanyNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompactOverriddenCompanyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 2, true);
			this.CompactOverriddenCompanyNameTextBox.Name = "CompactOverriddenCompanyNameTextBox";
			this.CompactOverriddenCompanyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CompactOverriddenCompanyNameTextBox.TabIndex = 0;
			// 
			// CompactContactTabPage
			//
			this.CompactContactTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C6D14667-59CB-4176-8D74-21883254656C", "Con.", "Contact", "Contact Tab");
			this.CompactContactTabPage.Controls.Add(this.CompactOverriddenPhoneNumberUserControl);
			this.CompactContactTabPage.Controls.Add(this.CompactOverriddenEmailTextBox);
			this.CompactContactTabPage.Controls.Add(this.CompactOverriddenContactNameTextBox);
			this.CompactContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompactContactTabPage.Name = "CompactContactTabPage";
			this.CompactContactTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompactContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 89, true);
			this.CompactContactTabPage.TabIndex = 1;
			this.CompactContactTabPage.ToolTipText = CompactContactTabPage.CaptionResourceString.Caption;
			// 
			// CompactOverriddenPhoneNumberUserControl
			// 
			this.CompactOverriddenPhoneNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompactOverriddenPhoneNumberUserControl, "PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).PhoneNumber)));
			this.CompactOverriddenPhoneNumberUserControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3231EE75-3585-42EF-B9EB-0D66E95879A4", "Phone", "Phone Number", "Contact Phone Number");
			this.CompactOverriddenPhoneNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 60, true);
			this.CompactOverriddenPhoneNumberUserControl.Name = "CompactOverriddenPhoneNumberUserControl";
			this.CompactOverriddenPhoneNumberUserControl.NumberTextBoxMaxLength = 20;
			this.CompactOverriddenPhoneNumberUserControl.ShowLocalNumberLabel = false;
			this.CompactOverriddenPhoneNumberUserControl.ShowPublishedCheckBox = false;
			this.CompactOverriddenPhoneNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.CompactOverriddenPhoneNumberUserControl.TabIndex = 2;
			this.CompactOverriddenPhoneNumberUserControl.UnscaledLeftPadding = -29;
			// 
			// CompactOverriddenEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenEmailTextBox, "E2_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Email)));
			this.CompactOverriddenEmailTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7E476356-144C-463D-B920-74313E8C1FAF", "Email");
			this.CompactOverriddenEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompactOverriddenEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 34, true);
			this.CompactOverriddenEmailTextBox.Name = "CompactOverriddenEmailTextBox";
			this.CompactOverriddenEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CompactOverriddenEmailTextBox.TabIndex = 1;
			// 
			// CompactOverriddenContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompactOverriddenContactNameTextBox, "E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			this.CompactOverriddenContactNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("526DFC01-20D6-4560-8A98-2D2E3154A70E", "Name");
			this.CompactOverriddenContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompactOverriddenContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 8, true);
			this.CompactOverriddenContactNameTextBox.Name = "CompactOverriddenContactNameTextBox";
			this.CompactOverriddenContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CompactOverriddenContactNameTextBox.TabIndex = 0;
			// 
			// CompactOverrideLayoutGroupBox
			// 
			this.CompactOverrideLayoutGroupBox.Controls.Add(this.CompactOverrideTabControl);
			this.CompactOverrideLayoutGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1682, 0, true);
			this.CompactOverrideLayoutGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.CompactOverrideLayoutGroupBox.Name = "CompactOverrideLayoutGroupBox";
			this.CompactOverrideLayoutGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 1, 0, true);
			this.CompactOverrideLayoutGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 130, true);
			this.CompactOverrideLayoutGroupBox.TabIndex = 13;
			this.CompactOverrideLayoutGroupBox.TabStop = false;
			this.CompactOverrideLayoutGroupBox.Text = "Compact Override";
			this.CompactOverrideLayoutGroupBox.Visible = false;
			// 
			// CompactWithContactTabControl
			// 
			this.CompactWithContactTabControl.Controls.Add(this.CompactAddressTab);
			this.CompactWithContactTabControl.Controls.Add(this.CompactContactTab);
			this.CompactWithContactTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1418, 136, true);
			this.CompactWithContactTabControl.Name = "CompactWithContactTabControl";
			this.CompactWithContactTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 92, true);
			this.CompactWithContactTabControl.TabIndex = 14;
			// 
			// CompactAddressTab
			//
			this.CompactAddressTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F9911E6D-62CA-48FC-9F34-3AEA0CDC4BDF", "Addr.", "Address", "Address Tab");
			this.CompactAddressTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompactAddressTab.Name = "CompactAddressTab";
			this.CompactAddressTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompactAddressTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 65, true);
			this.CompactAddressTab.TabIndex = 0;
			// 
			// CompactContactTab
			//
			this.CompactContactTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C5C117BD-CD61-47EE-9039-B6899D043D3B", "Con.", "Contact", "Contact Tab");
			this.CompactContactTab.Controls.Add(this.CompactContactPhoneNumberLabel);
			this.CompactContactTab.Controls.Add(this.CompactContactEmailAddressLabel);
			this.CompactContactTab.Controls.Add(this.CompactContactDropEdit);
			this.CompactContactTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompactContactTab.Name = "CompactContactTab";
			this.CompactContactTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompactContactTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 65, true);
			this.CompactContactTab.TabIndex = 1;
			// 
			// CompactContactPhoneNumberLabel
			// 
			this.CompactContactPhoneNumberLabel.AutoEllipsis = true;
			this.CompactContactPhoneNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompactContactPhoneNumberLabel, false);
			this.CompactContactPhoneNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
			this.CompactContactPhoneNumberLabel.Name = "CompactContactPhoneNumberLabel";
			this.CompactContactPhoneNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
			this.CompactContactPhoneNumberLabel.TabIndex = 3;
			this.CompactContactPhoneNumberLabel.UseMnemonic = false;
			// 
			// CompactContactEmailAddressLabel
			// 
			this.CompactContactEmailAddressLabel.AutoEllipsis = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompactContactEmailAddressLabel, false);
			this.CompactContactEmailAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CompactContactEmailAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.CompactContactEmailAddressLabel.Name = "CompactContactEmailAddressLabel";
			this.CompactContactEmailAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
			this.CompactContactEmailAddressLabel.TabIndex = 2;
			this.CompactContactEmailAddressLabel.UseMnemonic = false;
			// 
			// CompactContactDropEdit
			// 
			this.CompactContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompactContactDropEdit, "E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).Organisation.ContactsActive)));
			this.CompactContactDropEdit.BindToList = "Organisation+ContactsActive";
			this.CompactContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CompactContactDropEdit.Name = "CompactContactDropEdit";
			this.CompactContactDropEdit.PreBoundMaxLength = 20;
			this.CompactContactDropEdit.ShouldResizeByMaxLength = false;
			this.CompactContactDropEdit.ShowDescriptionBox = false;
			this.CompactContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.CompactContactDropEdit.TabIndex = 1;
			// 
			// ZDocAddressControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompactLayoutNameAndAddressPanel);
			this.Controls.Add(this.CompactWithContactTabControl);
			this.Controls.Add(this.CompactOverrideLayoutGroupBox);
			this.Controls.Add(this.CompactLayoutGroupBox);
			this.Controls.Add(this.AddressValidationStatusButton);
			this.Controls.Add(this.SingleLineNoGroupBoxOverridePanel);
			this.Controls.Add(this.SingleLineNoGroupBoxPanel);
			this.Controls.Add(this.CutDownSingleLineGroupBox);
			this.Controls.Add(this.OverrideAddressCheckbox);
			this.Controls.Add(this.DefaultGroupBox);
			this.Controls.Add(this.OverrideGroupBox);
			this.Controls.Add(this.CutDownGroupBox);
			this.Name = "ZDocAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2366, 363, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.AddressTabPage.ResumeLayout(false);
			this.AddressTabPage.PerformLayout();
			this.AddressTypeDropEdit.ResumeLayout(true);
			this.AddressTypeDropEdit.PerformLayout();
			this.StateDropEdit.ResumeLayout(true);
			this.StateDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.ContactTabPage.ResumeLayout(false);
			this.ContactTabPage.PerformLayout();
			this.MobilePhoneNumberControl.ResumeLayout(true);
			this.MobilePhoneNumberControl.PerformLayout();
			this.GovernmentRegistrationTabPage.ResumeLayout(false);
			this.GovernmentRegistrationTabPage.PerformLayout();
			this.GovernmentRegistrationTypeDropEdit.ResumeLayout(true);
			this.GovernmentRegistrationTypeDropEdit.PerformLayout();
			this.OverrideGroupBox.ResumeLayout(false);
			this.OverrideGroupBox.PerformLayout();
			this.DefaultGroupBox.ResumeLayout(true);
			this.DefaultGroupBox.PerformLayout();
			this.CutDownGroupBox.ResumeLayout(false);
			this.CutDownGroupBox.PerformLayout();
			this.CutDownAddressDropEdit.ResumeLayout(true);
			this.CutDownAddressDropEdit.PerformLayout();
			this.CutDownOrganisationFindBox.ResumeLayout(true);
			this.CutDownOrganisationFindBox.PerformLayout();
			this.CutDownSingleLineGroupBox.ResumeLayout(false);
			this.CutDownSingleLineGroupBox.PerformLayout();
			this.CutDownSingleLineAddressDropEdit.ResumeLayout(true);
			this.CutDownSingleLineAddressDropEdit.PerformLayout();
			this.CutDownSingleLineOrgFindBox.ResumeLayout(true);
			this.CutDownSingleLineOrgFindBox.PerformLayout();
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.ResumeLayout(true);
			this.CutDownSingleLineNoGroupBoxAddressDropEdit.PerformLayout();
			this.CutDownSingleLineNoGroupBoxOrgFindBox.ResumeLayout(true);
			this.CutDownSingleLineNoGroupBoxOrgFindBox.PerformLayout();
			this.SingleLineNoGroupBoxPanel.ResumeLayout(false);
			this.SingleLineNoGroupBoxPanel.PerformLayout();
			this.SingleLineNoGroupBoxOverridePanel.ResumeLayout(false);
			this.SingleLineNoGroupBoxOverridePanel.PerformLayout();
			this.CompactLayoutGroupBox.ResumeLayout(false);
			this.CompactLayoutGroupBox.PerformLayout();
			this.CompactOrganizationFindBox.ResumeLayout(true);
			this.CompactOrganizationFindBox.PerformLayout();
			this.CompactAddressDropEdit.ResumeLayout(true);
			this.CompactAddressDropEdit.PerformLayout();
			this.CompactLayoutNameAndAddressPanel.ResumeLayout(false);
			this.CompactLayoutNameAndAddressPanel.PerformLayout();
			this.CompactOverrideTabControl.ResumeLayout(false);
			this.CompactOverrideTabControl.PerformLayout();
			this.CompactAddressTabPage.ResumeLayout(false);
			this.CompactAddressTabPage.PerformLayout();
			this.CompactOverriddenCountryFindBox.ResumeLayout(true);
			this.CompactOverriddenCountryFindBox.PerformLayout();
			this.CompactContactTabPage.ResumeLayout(false);
			this.CompactContactTabPage.PerformLayout();
			this.CompactOverriddenPhoneNumberUserControl.ResumeLayout(true);
			this.CompactOverriddenPhoneNumberUserControl.PerformLayout();
			this.CompactOverrideLayoutGroupBox.ResumeLayout(false);
			this.CompactOverrideLayoutGroupBox.PerformLayout();
			this.CompactWithContactTabControl.ResumeLayout(false);
			this.CompactWithContactTabControl.PerformLayout();
			this.CompactContactTab.ResumeLayout(false);
			this.CompactContactTab.PerformLayout();
			this.CompactAddressTab.ResumeLayout(false);
			this.CompactAddressTab.PerformLayout();
			this.CompactContactDropEdit.ResumeLayout(true);
			this.CompactContactDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox CutDownSingleLineGroupBox;
		internal Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare CutDownSingleLineAddressDropEdit;
		internal ZOrganisationFindBox.Bare CutDownSingleLineOrgFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth GovernmentRegistrationTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CutDownGroupBox;
		internal Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare CutDownAddressDropEdit;
		internal ZOrganisationFindBox.Bare CutDownOrganisationFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ResidentialAddressCheckBox;
		protected Enterprise.ZArchitecture.ZTextBox GovernmentRegistrationNumberTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth StateDropEdit;
		protected Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		internal Internal.ZDocAddressOrganisationControl DefaultGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OverrideAddressCheckbox;
		protected Enterprise.ZArchitecture.ZTextBox CompanyTextBox;
		protected Enterprise.ZArchitecture.ZTextBox AdditionalAddressInformationTextBox;
		protected Enterprise.ZArchitecture.ZTextBox AddressLine1TextBox;
		protected Enterprise.ZArchitecture.ZTextBox AddressLine2TextBox;
		protected Enterprise.ZArchitecture.ZTextBox CityTextBox;
		protected Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox EMailTextBox;
		protected PhoneNumberUserControl PhoneNumberControl;
		protected PhoneNumberUserControl FaxNumberControl;
		protected PhoneNumberUserControl MobilePhoneNumberControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AddressTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ContactTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage GovernmentRegistrationTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		internal Enterprise.ZArchitecture.ZTextBox ContactTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox OverrideGroupBox;
		private Enterprise.ZArchitecture.ZLabel ContactLabel;
		protected internal Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare CutDownSingleLineNoGroupBoxAddressDropEdit;
		protected internal ZOrganisationFindBox.Bare CutDownSingleLineNoGroupBoxOrgFindBox;
		private Enterprise.ZArchitecture.GUI.ZPanel SingleLineNoGroupBoxPanel;
		internal Enterprise.ZArchitecture.ZTextBox CutDownSingleLineNoGroupBoxOrgOverrideTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel SingleLineNoGroupBoxOverridePanel;
		protected Enterprise.ZArchitecture.ZTextBox PassportDataTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton convertToOrganizationButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ValidateAddressButton;
		protected Enterprise.ZArchitecture.GUI.ZButton PassportDataEditButton;
		internal ZArchitecture.GUI.ZTabControl CompactOverrideTabControl;
		internal ZArchitecture.GUI.ZTabPage CompactAddressTabPage;
		internal ZArchitecture.GUI.ZTabPage CompactContactTabPage;
		internal ZArchitecture.ZTextBox CompactOverriddenCityTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox CompactOverriddenCountryFindBox;
		internal ZArchitecture.ZTextBox CompactOverriddenAddressTextBox;
		internal ZArchitecture.ZTextBox CompactOverriddenCompanyNameTextBox;
		internal ZArchitecture.ZTextBox CompactOverriddenEmailTextBox;
		internal ZArchitecture.ZTextBox CompactOverriddenContactNameTextBox;
		internal ZArchitecture.ZTextBox CompactOverriddenPostCodeTextBox;
		internal ZArchitecture.GUI.ZGroupBox CompactOverrideLayoutGroupBox;
		internal PhoneNumberUserControl CompactOverriddenPhoneNumberUserControl;
		internal ZArchitecture.GUI.ZTabControl CompactWithContactTabControl;
		internal ZArchitecture.GUI.ZTabPage CompactAddressTab;
		internal ZArchitecture.GUI.ZTabPage CompactContactTab;
		internal ZArchitecture.GUI.ZPanel CompactLayoutPanel;
		internal ZArchitecture.GUI.ZDropEdit CompactContactDropEdit;
		internal ZArchitecture.ZLabel CompactContactEmailAddressLabel;
		internal ZArchitecture.ZLabel CompactContactPhoneNumberLabel;
	}
}
