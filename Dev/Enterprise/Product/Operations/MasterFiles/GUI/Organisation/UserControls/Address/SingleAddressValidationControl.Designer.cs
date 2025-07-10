using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class SingleAddressValidationControl
	{
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AddressDetailGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AddressBoundPanel = new CargoWise.Windows.UI.KPanel();
			this.SearchAddressOnlineLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.CopyAddressInfoLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Address2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalAddressInformationBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.CopyCompanyInfoLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SearchCompanyOnlineLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.OperationPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OpenButtonPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OpenRecordButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NavigationPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.RecordsNavigator = new Enterprise.MasterFiles.GUI.RecordsNavigator();
			this.SaveButtonPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressDetailGroupBox.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.AddressBoundPanel.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.StateBoundDropEdit.SuspendLayout();
			this.OrganisationFindBox.SuspendLayout();
			this.OperationPanel.SuspendLayout();
			this.OpenButtonPanel.SuspendLayout();
			this.NavigationPanel.SuspendLayout();
			this.RecordsNavigator.SuspendLayout();
			this.SaveButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MDMAdminPanelAddressCollection);
			// 
			// AddressDetailGroupBox
			// 
			this.AddressDetailGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e1e03976-8aa8-4201-bafa-aba6fed9c9e1", "Address Details");
			this.AddressDetailGroupBox.Controls.Add(this.MainPanel);
			this.AddressDetailGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressDetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressDetailGroupBox.Name = "AddressDetailGroupBox";
			this.AddressDetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 630, true);
			this.AddressDetailGroupBox.TabIndex = 40;
			this.AddressDetailGroupBox.TabStop = false;
			// 
			// MainPanel
			// 
			this.MainPanel.ColumnCount = 1;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.Controls.Add(this.AddressBoundPanel, 0, 0);
			this.MainPanel.Controls.Add(this.OperationPanel, 0, 2);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 3;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(244)));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40)));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 611, true);
			this.MainPanel.TabIndex = 50;
			// 
			// AddressBoundPanel
			// 
			this.AddressBoundPanel.Controls.Add(this.SearchAddressOnlineLink);
			this.AddressBoundPanel.Controls.Add(this.CopyAddressInfoLink);
			this.AddressBoundPanel.Controls.Add(this.PostCodeBoundTextBox);
			this.AddressBoundPanel.Controls.Add(this.CountryFindBox);
			this.AddressBoundPanel.Controls.Add(this.StateBoundDropEdit);
			this.AddressBoundPanel.Controls.Add(this.Address2BoundTextBox);
			this.AddressBoundPanel.Controls.Add(this.CityBoundTextBox);
			this.AddressBoundPanel.Controls.Add(this.ValidateAddressButton);
			this.AddressBoundPanel.Controls.Add(this.ClearFieldsButton);
			this.AddressBoundPanel.Controls.Add(this.Address1BoundTextBox);
			this.AddressBoundPanel.Controls.Add(this.AdditionalAddressInformationBoundTextBox);
			this.AddressBoundPanel.Controls.Add(this.AddressCodeTextBox);
			this.AddressBoundPanel.Controls.Add(this.OrganisationNameTextbox);
			this.AddressBoundPanel.Controls.Add(this.AddressTypeTextBox);
			this.AddressBoundPanel.Controls.Add(this.OrganisationFindBox);
			this.AddressBoundPanel.Controls.Add(this.CopyCompanyInfoLink);
			this.AddressBoundPanel.Controls.Add(this.SearchCompanyOnlineLink);
			this.AddressBoundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressBoundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddressBoundPanel.Name = "AddressBoundPanel";
			this.AddressBoundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 238, true);
			this.AddressBoundPanel.TabIndex = 49;
			// 
			// SearchAddressOnlineLink
			// 
			this.SearchAddressOnlineLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f5eccebd-02ab-4ed6-9472-8ca3f517b4b9", "Search Address Online");
			this.SearchAddressOnlineLink.IsFontBold = true;
			this.SearchAddressOnlineLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 210, true);
			this.SearchAddressOnlineLink.Name = "SearchAddressOnlineLink";
			this.SearchAddressOnlineLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.SearchAddressOnlineLink.TabIndex = 55;
			this.SearchAddressOnlineLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SearchAddressOnlineLink_LinkClicked);
			// 
			// CopyAddressInfoLink
			// 
			this.CopyAddressInfoLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("440cf147-8a56-4684-a9d7-07575530c362", "Copy Address Information");
			this.CopyAddressInfoLink.IsFontBold = true;
			this.CopyAddressInfoLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 210, true);
			this.CopyAddressInfoLink.Name = "CopyAddressInfoLink";
			this.CopyAddressInfoLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.CopyAddressInfoLink.TabIndex = 54;
			this.CopyAddressInfoLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CopyAddressInfoLink_LinkClicked);
			// 
			// PostCodeBoundTextBox
			// 
			this.PostCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19330d7b-3b7a-4c72-bc93-71a85a5f5e31", "Post Code");
			this.PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 160, true);
			this.PostCodeBoundTextBox.Name = "PostCodeBoundTextBox";
			this.PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PostCodeBoundTextBox.TabIndex = 44;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.CountryFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5d5f7457-6483-4e92-943c-6c5e043f3214", "Country/Region");
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 134, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShouldResize = true;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 43;
			// 
			// StateBoundDropEdit
			// 
			this.StateBoundDropEdit.AllowDrop = true;
			this.StateBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f89a763-1a73-4e03-9bc6-ba4392d2a99f", "State");
			this.StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 160, true);
			this.StateBoundDropEdit.Name = "StateBoundDropEdit";
			this.StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.StateBoundDropEdit.TabIndex = 46;
			// 
			// Address2BoundTextBox
			// 
			this.Address2BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("537aed55-f704-4a93-898a-f12f50991473", "Address 2");
			this.Address2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 108, true);
			this.Address2BoundTextBox.Name = "Address2BoundTextBox";
			this.Address2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.Address2BoundTextBox.TabIndex = 42;
			// 
			// CityBoundTextBox
			// 
			this.CityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f5e4803b-a3a8-46a0-9c85-841f5f5641eb", "City");
			this.CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 134, true);
			this.CityBoundTextBox.Name = "CityBoundTextBox";
			this.CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.CityBoundTextBox.TabIndex = 45;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 81, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 47;
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 82, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 49;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += new System.EventHandler(this.ClearFieldsButton_Click);
			// 
			// Address1BoundTextBox
			// 
			this.Address1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e4fca647-c74a-44c2-8d57-310a0a44758c", "Address 1");
			this.Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 82, true);
			this.Address1BoundTextBox.Name = "Address1BoundTextBox";
			this.Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
			this.Address1BoundTextBox.TabIndex = 41;
			// 
			// AdditionalAddressInformationBoundTextBox
			// 
			this.AdditionalAddressInformationBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cee4507d-88e1-48eb-8de3-7f11bbcc90b9", "Additional Address Info");
			this.AdditionalAddressInformationBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 56, true);
			this.AdditionalAddressInformationBoundTextBox.Name = "AdditionalAddressInformationBoundTextBox";
			this.AdditionalAddressInformationBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.AdditionalAddressInformationBoundTextBox.TabIndex = 40;
			// 
			// AddressCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressCodeTextBox, "MDM_AddressCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MDMAdminPanelAddressView)(null)).MDM_AddressCode)));
			this.AddressCodeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AddressCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("71215d29-392c-429a-8076-94446a336431", "Address Code");
			this.AddressCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 30, true);
			this.AddressCodeTextBox.Name = "AddressCodeTextBox";
			this.AddressCodeTextBox.ReadOnly = true;
			this.AddressCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.AddressCodeTextBox.TabIndex = 51;
			// 
			// OrganisationNameTextbox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationNameTextbox, "MDM_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MDMAdminPanelAddressView)(null)).MDM_CompanyName)));
			this.OrganisationNameTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.OrganisationNameTextbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dc3f61cd-2bbb-403f-9072-491d9b9b6d85", "Organization Name ");
			this.OrganisationNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 5, true);
			this.OrganisationNameTextbox.Name = "OrganisationNameTextbox";
			this.OrganisationNameTextbox.ReadOnly = true;
			this.OrganisationNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.OrganisationNameTextbox.TabIndex = 53;
			// 
			// AddressTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressTypeTextBox, "MDM_AddressTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MDMAdminPanelAddressView)(null)).MDM_AddressTypeDescription)));
			this.AddressTypeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AddressTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f8bb7be2-fb5d-450e-aad9-4bac03e26ead", "Type");
			this.AddressTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 30, true);
			this.AddressTypeTextBox.Name = "AddressTypeTextBox";
			this.AddressTypeTextBox.ReadOnly = true;
			this.AddressTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.AddressTypeTextBox.TabIndex = 52;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.OrganisationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "MDM_ParentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MDMAdminPanelAddressView)(null)).MDM_ParentID)));
			this.OrganisationFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0248c0db-b4d6-4072-bd27-00efae4a903d", "Code");
			this.OrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 5, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.PreBoundMaxLength = 12;
			this.OrganisationFindBox.ShouldResize = true;
			this.OrganisationFindBox.ShowDescriptionBox = false;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.OrganisationFindBox.TabIndex = 41;
			// 
			// CopyCompanyInfoLink
			// 
			this.CopyCompanyInfoLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19330d7b-3b7c-4c72-bc93-71a85a5f5e31", "Copy Company Information");
			this.CopyCompanyInfoLink.IsFontBold = true;
			this.CopyCompanyInfoLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 186, true);
			this.CopyCompanyInfoLink.Name = "CopyCompanyInfoLink";
			this.CopyCompanyInfoLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.CopyCompanyInfoLink.TabIndex = 47;
			this.CopyCompanyInfoLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CopyCompanyInfoLink_LinkClicked);
			// 
			// SearchCompanyOnlineLink
			// 
			this.SearchCompanyOnlineLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19330d7a-3b7c-4c72-bc93-71a85a5f5e31", "Search Company Online");
			this.SearchCompanyOnlineLink.IsFontBold = true;
			this.SearchCompanyOnlineLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 186, true);
			this.SearchCompanyOnlineLink.Name = "SearchCompanyOnlineLink";
			this.SearchCompanyOnlineLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.SearchCompanyOnlineLink.TabIndex = 47;
			this.SearchCompanyOnlineLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SearchCompanyOnlineLink_LinkClicked);
			// 
			// OperationPanel
			// 
			this.OperationPanel.ColumnCount = 3;
			this.OperationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OperationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.OperationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.OperationPanel.Controls.Add(this.OpenButtonPanel, 1, 0);
			this.OperationPanel.Controls.Add(this.NavigationPanel, 0, 0);
			this.OperationPanel.Controls.Add(this.SaveButtonPanel, 2, 0);
			this.OperationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OperationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 574, true);
			this.OperationPanel.Name = "OperationPanel";
			this.OperationPanel.RowCount = 1;
			this.OperationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.OperationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 34, true);
			this.OperationPanel.TabIndex = 51;
			// 
			// OpenButtonPanel
			// 
			this.OpenButtonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenButtonPanel.ColumnCount = 1;
			this.OpenButtonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OpenButtonPanel.Controls.Add(this.OpenRecordButton, 0, 0);
			this.OpenButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 3, true);
			this.OpenButtonPanel.Name = "OpenButtonPanel";
			this.OpenButtonPanel.RowCount = 1;
			this.OpenButtonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OpenButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 28, true);
			this.OpenButtonPanel.TabIndex = 53;
			// 
			// OpenRecordButton
			// 
			this.OpenRecordButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenRecordButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c5fe8de3-3c1e-44da-adb0-5d9c9769b7f3", "Open Record");
			this.OpenRecordButton.Enabled = false;
			this.OpenRecordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 3, true);
			this.OpenRecordButton.Name = "OpenRecordButton";
			this.OpenRecordButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenRecordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.OpenRecordButton.TabIndex = 2;
			this.OpenRecordButton.ToolTipCaption = null;
			this.OpenRecordButton.UseVisualStyleBackColor = true;
			this.OpenRecordButton.Click += new System.EventHandler(this.OpenRecordButton_Click);
			// 
			// NavigationPanel
			// 
			this.NavigationPanel.ColumnCount = 1;
			this.NavigationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.NavigationPanel.Controls.Add(this.RecordsNavigator, 0, 0);
			this.NavigationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NavigationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NavigationPanel.Name = "NavigationPanel";
			this.NavigationPanel.RowCount = 1;
			this.NavigationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.NavigationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 28, true);
			this.NavigationPanel.TabIndex = 52;
			// 
			// RecordsNavigator
			// 
			this.RecordsNavigator.AllowDrop = true;
			this.RecordsNavigator.Enabled = false;
			this.RecordsNavigator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RecordsNavigator.Name = "RecordsNavigator";
			this.RecordsNavigator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 22, true);
			this.RecordsNavigator.TabIndex = 0;
			this.RecordsNavigator.EnabledChanged += new System.EventHandler(this.RecordsNavigator_EnabledChanged);
			// 
			// SaveButtonPanel
			// 
			this.SaveButtonPanel.ColumnCount = 1;
			this.SaveButtonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.SaveButtonPanel.Controls.Add(this.SaveButton, 0, 0);
			this.SaveButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 3, true);
			this.SaveButtonPanel.Name = "SaveButtonPanel";
			this.SaveButtonPanel.RowCount = 1;
			this.SaveButtonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.SaveButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 28, true);
			this.SaveButtonPanel.TabIndex = 4;
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("{A81D4FE8-CB45-4E53-8976-AD98FC3F0C26}", "Save Record");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// SingleAddressValidationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressDetailGroupBox);
			this.Name = "SingleAddressValidationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 630, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressDetailGroupBox.ResumeLayout(false);
			this.AddressDetailGroupBox.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.AddressBoundPanel.ResumeLayout(false);
			this.AddressBoundPanel.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.StateBoundDropEdit.ResumeLayout(true);
			this.StateBoundDropEdit.PerformLayout();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.OperationPanel.ResumeLayout(false);
			this.OperationPanel.PerformLayout();
			this.OpenButtonPanel.ResumeLayout(false);
			this.OpenButtonPanel.PerformLayout();
			this.NavigationPanel.ResumeLayout(false);
			this.NavigationPanel.PerformLayout();
			this.RecordsNavigator.ResumeLayout(true);
			this.RecordsNavigator.PerformLayout();
			this.SaveButtonPanel.ResumeLayout(false);
			this.SaveButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal KPanel AddressBoundPanel;
		private KTableLayoutPanel MainPanel;
		private KTableLayoutPanel OperationPanel;
		private KTableLayoutPanel SaveButtonPanel;
		private KTableLayoutPanel NavigationPanel;
		private KTableLayoutPanel OpenButtonPanel;
		internal ZButton SaveButton;
		internal ZButton OpenRecordButton;
		protected ZButton ClearFieldsButton;
		protected ZButton ValidateAddressButton;
		protected ZTextBox CityBoundTextBox;
		protected ZTextBox AddressTypeTextBox;
		protected ZTextBox AddressCodeTextBox;
		protected ZTextBox PostCodeBoundTextBox;
		protected ZTextBox Address2BoundTextBox;
		protected ZTextBox Address1BoundTextBox;
		protected ZTextBox OrganisationNameTextbox;
		protected ZTextBox AdditionalAddressInformationBoundTextBox;
		protected ZLinkLabel CopyCompanyInfoLink;
		protected ZLinkLabel SearchCompanyOnlineLink;
		internal ZGroupBox AddressDetailGroupBox;
		protected ZDropEdit StateBoundDropEdit;
		private ZOrganisationFindBox OrganisationFindBox;
		internal MasterFiles.GUI.RecordsNavigator RecordsNavigator;
		internal AddressSuggestionControl AddressSuggestionControl;
		private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		protected ZLinkLabel SearchAddressOnlineLink;
		protected ZLinkLabel CopyAddressInfoLink;
	}
}