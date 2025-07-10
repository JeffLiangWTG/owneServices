using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.TW.GUI
{
	partial class TWJobDocAddressControl
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
            this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.AddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EditAddressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.AdditionalAddressInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AddressLine1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AddressLine2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.StateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
            this.AddressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.AddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
            this.AddressLabel = new Enterprise.ZArchitecture.ZLabel();
            this.LocalAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EditLocalAddressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.LocalAdditionalAddressInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalPostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalCompanyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.LocalCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalCountryCodeFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
            this.LocalAddressDetailLabel = new Enterprise.ZArchitecture.ZLabel();
            this.ContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ContactPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.ContactDetailLabel = new Enterprise.ZArchitecture.ZLabel();
            this.EditContactPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.FaxTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CodeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.FRICodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FRICodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CBPCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CBPCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TPCCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TPCCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AEOCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AEOCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.IDCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.IDCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.OrganisationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            this.OverrideAddressCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MainGroupBox.SuspendLayout();
            this.DetailsTabControl.SuspendLayout();
            this.AddressTabPage.SuspendLayout();
            this.EditAddressPanel.SuspendLayout();
            this.StateDropEdit.SuspendLayout();
            this.CountryFindBox.SuspendLayout();
            this.AddressPanel.SuspendLayout();
            this.AddressDropEdit.SuspendLayout();
            this.LocalAddressTabPage.SuspendLayout();
            this.EditLocalAddressPanel.SuspendLayout();
            this.LocalStateDropEdit.SuspendLayout();
            this.LocalCountryCodeFindBox.SuspendLayout();
            this.ContactTabPage.SuspendLayout();
            this.ContactPanel.SuspendLayout();
            this.ContactDropEdit.SuspendLayout();
            this.EditContactPanel.SuspendLayout();
            this.CodeTabPage.SuspendLayout();
            this.FRICodeTypeDropEdit.SuspendLayout();
            this.CBPCodeTypeDropEdit.SuspendLayout();
            this.TPCCodeTypeDropEdit.SuspendLayout();
            this.AEOCodeTypeDropEdit.SuspendLayout();
            this.IDCodeTypeDropEdit.SuspendLayout();
            this.OrganisationPanel.SuspendLayout();
            this.OrganisationFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWJobDocAddress);
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.Controls.Add(this.DetailsTabControl);
            this.MainGroupBox.Controls.Add(this.OrganisationPanel);
            this.MainGroupBox.Controls.Add(this.OverrideAddressCheckbox);
            this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 228, true);
            this.MainGroupBox.TabIndex = 1;
            this.MainGroupBox.TabStop = false;
            // 
            // DetailsTabControl
            // 
            this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.DetailsTabControl.Controls.Add(this.AddressTabPage);
            this.DetailsTabControl.Controls.Add(this.LocalAddressTabPage);
            this.DetailsTabControl.Controls.Add(this.ContactTabPage);
            this.DetailsTabControl.Controls.Add(this.CodeTabPage);
            this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
            this.DetailsTabControl.Name = "DetailsTabControl";
            this.DetailsTabControl.ShowToolTips = true;
            this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 181, true);
            this.DetailsTabControl.TabIndex = 0;
            // 
            // AddressTabPage
            // 
            this.AddressTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.AddressTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("558e9dba-121d-4b7f-8b8a-8262fba04a52", "Address");
            this.AddressTabPage.Controls.Add(this.EditAddressPanel);
            this.AddressTabPage.Controls.Add(this.AddressPanel);
            this.AddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.AddressTabPage.Name = "AddressTabPage";
            this.AddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.AddressTabPage.TabIndex = 2;
            // 
            // EditAddressPanel
            // 
            this.EditAddressPanel.Controls.Add(this.AdditionalAddressInformationTextBox);
            this.EditAddressPanel.Controls.Add(this.AddressLine1TextBox);
            this.EditAddressPanel.Controls.Add(this.AddressLine2TextBox);
            this.EditAddressPanel.Controls.Add(this.PostCodeTextBox);
            this.EditAddressPanel.Controls.Add(this.CompanyTextBox);
            this.EditAddressPanel.Controls.Add(this.StateDropEdit);
            this.EditAddressPanel.Controls.Add(this.CityTextBox);
            this.EditAddressPanel.Controls.Add(this.CountryFindBox);
            this.EditAddressPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditAddressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EditAddressPanel.Name = "EditAddressPanel";
            this.EditAddressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.EditAddressPanel.TabIndex = 10;
            // 
            // AdditionalAddressInformationTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAddressInformationTextBox, "E2_AdditionalAddressInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_AdditionalAddressInformation)));
            this.AdditionalAddressInformationTextBox.CaptionResourceString = null;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalAddressInformationTextBox, false);
            this.AdditionalAddressInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 75, true);
            this.AdditionalAddressInformationTextBox.Name = "AdditionalAddressInformationTextBox";
            this.AdditionalAddressInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.AdditionalAddressInformationTextBox.TabIndex = 11;
            // 
            // AddressLine1TextBox
            // 
            this.BindingSource.SetBindingMember(this.AddressLine1TextBox, "E2_Address1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Address1)));
            this.AddressLine1TextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f2f36e86-6356-4d44-b090-bcf3413c6b3b", "Address");
            this.AddressLine1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 29, true);
            this.AddressLine1TextBox.Name = "AddressLine1TextBox";
            this.AddressLine1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.AddressLine1TextBox.TabIndex = 9;
            // 
            // AddressLine2TextBox
            // 
            this.BindingSource.SetBindingMember(this.AddressLine2TextBox, "E2_Address2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Address2)));
            this.AddressLine2TextBox.CaptionResourceString = null;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressLine2TextBox, false);
            this.AddressLine2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 52, true);
            this.AddressLine2TextBox.Name = "AddressLine2TextBox";
            this.AddressLine2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.AddressLine2TextBox.TabIndex = 10;
            // 
            // PostCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.PostCodeTextBox, "E2_Postcode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Postcode)));
            this.PostCodeTextBox.CaptionResourceString = null;
            this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 121, true);
            this.PostCodeTextBox.Name = "PostCodeTextBox";
            this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
            this.PostCodeTextBox.TabIndex = 14;
            // 
            // CompanyTextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyTextBox, "E2_CompanyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_CompanyName)));
            this.CompanyTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f855daaa-558b-465d-b21a-6326d99df764", "Name");
            this.CompanyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 6, true);
            this.CompanyTextBox.Name = "CompanyTextBox";
            this.CompanyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.CompanyTextBox.TabIndex = 8;
            // 
            // StateDropEdit
            // 
            this.StateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StateDropEdit, "E2_State");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_State)));
            this.StateDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 120, true);
            this.StateDropEdit.Name = "StateDropEdit";
            this.StateDropEdit.PreBoundMaxLength = 4;
            this.StateDropEdit.ShowDescriptionBox = false;
            this.StateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
            this.StateDropEdit.TabIndex = 15;
            // 
            // CityTextBox
            // 
            this.BindingSource.SetBindingMember(this.CityTextBox, "E2_City");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_City)));
            this.CityTextBox.CaptionResourceString = null;
            this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 97, true);
            this.CityTextBox.Name = "CityTextBox";
            this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
            this.CityTextBox.TabIndex = 13;
            // 
            // CountryFindBox
            // 
            this.CountryFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CountryFindBox, "E2_RN_NKCountryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_RN_NKCountryCode)));
            this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 98, true);
            this.CountryFindBox.Name = "CountryFindBox";
            this.CountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CountryFindBox.ParentType = null;
            this.CountryFindBox.PreBoundMaxLength = 3;
            this.CountryFindBox.ShowDescriptionBox = false;
            this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
            this.CountryFindBox.TabIndex = 12;
            // 
            // AddressPanel
            // 
            this.AddressPanel.Controls.Add(this.AddressDropEdit);
            this.AddressPanel.Controls.Add(this.AddressLabel);
            this.AddressPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AddressPanel.Name = "AddressPanel";
            this.AddressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.AddressPanel.TabIndex = 11;
            // 
            // AddressDropEdit
            // 
            this.AddressDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AddressDropEdit, "E2_OA_Address");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_OA_Address)));
            this.AddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AddressDropEdit.FilterAddressedByDefaultType = true;
            this.AddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 9, true);
            this.AddressDropEdit.Name = "AddressDropEdit";
            this.AddressDropEdit.PreBoundMaxLength = 34;
            this.AddressDropEdit.ShowDescriptionBox = false;
            this.AddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
            this.AddressDropEdit.TabIndex = 1;
            // 
            // AddressLabel
            // 
            this.AddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.AddressLabel, "AddressDetail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).AddressDetail)));
            this.AddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AddressLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressLabel, false);
            this.AddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 35, true);
            this.AddressLabel.Name = "AddressLabel";
            this.AddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 117, true);
            this.AddressLabel.TabIndex = 1;
            this.AddressLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AddressLabel.UseCompatibleTextRendering = true;
            this.AddressLabel.UseMnemonic = false;
            // 
            // LocalAddressTabPage
            // 
            this.LocalAddressTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e2829f81-2586-4367-a001-bf61fcff3c52", "Local Address");
            this.LocalAddressTabPage.Controls.Add(this.EditLocalAddressPanel);
            this.LocalAddressTabPage.Controls.Add(this.LocalAddressDetailLabel);
            this.LocalAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.LocalAddressTabPage.Name = "LocalAddressTabPage";
            this.LocalAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.LocalAddressTabPage.TabIndex = 2;
            // 
            // EditLocalAddressPanel
            // 
            this.EditLocalAddressPanel.Controls.Add(this.LocalAdditionalAddressInformationTextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalAddress1TextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalAddress2TextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalPostcodeTextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalCompanyNameTextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalStateDropEdit);
            this.EditLocalAddressPanel.Controls.Add(this.LocalCityTextBox);
            this.EditLocalAddressPanel.Controls.Add(this.LocalCountryCodeFindBox);
            this.EditLocalAddressPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditLocalAddressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EditLocalAddressPanel.Name = "EditLocalAddressPanel";
            this.EditLocalAddressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.EditLocalAddressPanel.TabIndex = 9;
            // 
            // LocalAdditionalAddressInformationTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalAdditionalAddressInformationTextBox, "LocalAddress+E2_AdditionalAddressInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_AdditionalAddressInformation)));
            this.LocalAdditionalAddressInformationTextBox.CaptionResourceString = null;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalAdditionalAddressInformationTextBox, false);
            this.LocalAdditionalAddressInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 75, true);
            this.LocalAdditionalAddressInformationTextBox.Name = "LocalAdditionalAddressInformationTextBox";
            this.LocalAdditionalAddressInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.LocalAdditionalAddressInformationTextBox.TabIndex = 11;
            // 
            // LocalAddress1TextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalAddress1TextBox, "LocalAddress+E2_Address1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_Address1)));
            this.LocalAddress1TextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("c8f8ea48-b8c7-4830-9052-62ddb24c276c", "Address");
            this.LocalAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 29, true);
            this.LocalAddress1TextBox.Name = "LocalAddress1TextBox";
            this.LocalAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.LocalAddress1TextBox.TabIndex = 9;
            // 
            // LocalAddress2TextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalAddress2TextBox, "LocalAddress+E2_Address2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_Address2)));
            this.LocalAddress2TextBox.CaptionResourceString = null;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalAddress2TextBox, false);
            this.LocalAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 52, true);
            this.LocalAddress2TextBox.Name = "LocalAddress2TextBox";
            this.LocalAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.LocalAddress2TextBox.TabIndex = 10;
            // 
            // LocalPostcodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalPostcodeTextBox, "LocalAddress+E2_Postcode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_Postcode)));
            this.LocalPostcodeTextBox.CaptionResourceString = null;
            this.LocalPostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 121, true);
            this.LocalPostcodeTextBox.Name = "LocalPostcodeTextBox";
            this.LocalPostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
            this.LocalPostcodeTextBox.TabIndex = 14;
            // 
            // LocalCompanyNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalCompanyNameTextBox, "LocalAddress+E2_CompanyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_CompanyName)));
            this.LocalCompanyNameTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("36cdee7e-3e0a-4406-8d6d-e235739b639c", "Name");
            this.LocalCompanyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 6, true);
            this.LocalCompanyNameTextBox.Name = "LocalCompanyNameTextBox";
            this.LocalCompanyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.LocalCompanyNameTextBox.TabIndex = 8;
            // 
            // LocalStateDropEdit
            // 
            this.LocalStateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocalStateDropEdit, "LocalAddress+E2_State");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_State)));
            this.LocalStateDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LocalStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 120, true);
            this.LocalStateDropEdit.Name = "LocalStateDropEdit";
            this.LocalStateDropEdit.PreBoundMaxLength = 4;
            this.LocalStateDropEdit.ShowDescriptionBox = false;
            this.LocalStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
            this.LocalStateDropEdit.TabIndex = 15;
            // 
            // LocalCityTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalCityTextBox, "LocalAddress+E2_City");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_City)));
            this.LocalCityTextBox.CaptionResourceString = null;
            this.LocalCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 97, true);
            this.LocalCityTextBox.Name = "LocalCityTextBox";
            this.LocalCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
            this.LocalCityTextBox.TabIndex = 13;
            // 
            // LocalCountryCodeFindBox
            // 
            this.LocalCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocalCountryCodeFindBox, "LocalAddress+E2_RN_NKCountryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddress.E2_RN_NKCountryCode)));
            this.LocalCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 98, true);
            this.LocalCountryCodeFindBox.Name = "LocalCountryCodeFindBox";
            this.LocalCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.LocalCountryCodeFindBox.ParentType = null;
            this.LocalCountryCodeFindBox.PreBoundMaxLength = 3;
            this.LocalCountryCodeFindBox.ShowDescriptionBox = false;
            this.LocalCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
            this.LocalCountryCodeFindBox.TabIndex = 12;
            // 
            // LocalAddressDetailLabel
            // 
            this.LocalAddressDetailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.LocalAddressDetailLabel, "LocalAddressDetail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).LocalAddressDetail)));
            this.LocalAddressDetailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalAddressDetailLabel, false);
            this.LocalAddressDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
            this.LocalAddressDetailLabel.Name = "LocalAddressDetailLabel";
            this.LocalAddressDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 145, true);
            this.LocalAddressDetailLabel.TabIndex = 8;
            this.LocalAddressDetailLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.LocalAddressDetailLabel.UseCompatibleTextRendering = true;
            this.LocalAddressDetailLabel.UseMnemonic = false;
            // 
            // ContactTabPage
            // 
            this.ContactTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("a55f059e-05ba-4f98-a0b6-4b05159dec67", "Contact");
            this.ContactTabPage.Controls.Add(this.ContactPanel);
            this.ContactTabPage.Controls.Add(this.EditContactPanel);
            this.ContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ContactTabPage.Name = "ContactTabPage";
            this.ContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.ContactTabPage.TabIndex = 3;
            // 
            // ContactPanel
            // 
            this.ContactPanel.Controls.Add(this.ContactDropEdit);
            this.ContactPanel.Controls.Add(this.ContactDetailLabel);
            this.ContactPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContactPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ContactPanel.Name = "ContactPanel";
            this.ContactPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.ContactPanel.TabIndex = 9;
            // 
            // ContactDropEdit
            // 
            this.ContactDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ContactDropEdit, "E2_Contact");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Contact)));
            this.ContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 7, true);
            this.ContactDropEdit.Name = "ContactDropEdit";
            this.ContactDropEdit.PreBoundMaxLength = 20;
            this.ContactDropEdit.ShowDescriptionBox = false;
            this.ContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.ContactDropEdit.TabIndex = 6;
            // 
            // ContactDetailLabel
            // 
            this.ContactDetailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ContactDetailLabel, "ContactDetail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).ContactDetail)));
            this.ContactDetailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContactDetailLabel, false);
            this.ContactDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 31, true);
            this.ContactDetailLabel.Name = "ContactDetailLabel";
            this.ContactDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 118, true);
            this.ContactDetailLabel.TabIndex = 7;
            this.ContactDetailLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ContactDetailLabel.UseCompatibleTextRendering = true;
            this.ContactDetailLabel.UseMnemonic = false;
            // 
            // EditContactPanel
            // 
            this.EditContactPanel.Controls.Add(this.FaxTextBox);
            this.EditContactPanel.Controls.Add(this.EmailTextBox);
            this.EditContactPanel.Controls.Add(this.PhoneTextBox);
            this.EditContactPanel.Controls.Add(this.ContactTextBox);
            this.EditContactPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditContactPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EditContactPanel.Name = "EditContactPanel";
            this.EditContactPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.EditContactPanel.TabIndex = 11;
            // 
            // FaxTextBox
            // 
            this.BindingSource.SetBindingMember(this.FaxTextBox, "E2_Fax");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Fax)));
            this.FaxTextBox.CaptionResourceString = null;
            this.FaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 75, true);
            this.FaxTextBox.Name = "FaxTextBox";
            this.FaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.FaxTextBox.TabIndex = 6;
            // 
            // EmailTextBox
            // 
            this.BindingSource.SetBindingMember(this.EmailTextBox, "E2_Email");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Email)));
            this.EmailTextBox.CaptionResourceString = null;
            this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 31, true);
            this.EmailTextBox.Name = "EmailTextBox";
            this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.EmailTextBox.TabIndex = 4;
            // 
            // PhoneTextBox
            // 
            this.BindingSource.SetBindingMember(this.PhoneTextBox, "E2_Phone");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Phone)));
            this.PhoneTextBox.CaptionResourceString = null;
            this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 54, true);
            this.PhoneTextBox.Name = "PhoneTextBox";
            this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.PhoneTextBox.TabIndex = 5;
            // 
            // ContactTextBox
            // 
            this.BindingSource.SetBindingMember(this.ContactTextBox, "E2_Contact");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_Contact)));
            this.ContactTextBox.CaptionResourceString = null;
            this.ContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 8, true);
            this.ContactTextBox.Name = "ContactTextBox";
            this.ContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.ContactTextBox.TabIndex = 3;
            // 
            // CodeTabPage
            // 
            this.CodeTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8d992bc3-95ba-4e57-8106-856bcd7aa9a4", "Code");
            this.CodeTabPage.Controls.Add(this.FRICodeTextBox);
            this.CodeTabPage.Controls.Add(this.FRICodeTypeDropEdit);
            this.CodeTabPage.Controls.Add(this.CBPCodeTextBox);
            this.CodeTabPage.Controls.Add(this.CBPCodeTypeDropEdit);
            this.CodeTabPage.Controls.Add(this.TPCCodeTextBox);
            this.CodeTabPage.Controls.Add(this.TPCCodeTypeDropEdit);
            this.CodeTabPage.Controls.Add(this.AEOCodeTextBox);
            this.CodeTabPage.Controls.Add(this.AEOCodeTypeDropEdit);
            this.CodeTabPage.Controls.Add(this.IDCodeTextBox);
            this.CodeTabPage.Controls.Add(this.IDCodeTypeDropEdit);
            this.CodeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.CodeTabPage.Name = "CodeTabPage";
            this.CodeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 154, true);
            this.CodeTabPage.TabIndex = 4;
            // 
            // FRICodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.FRICodeTextBox, "FRICode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).FRICode)));
			this.FRICodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("171B0375-FEBE-405C-9FF8-5065A3F00E62", "FRI");
			this.FRICodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 34, true);
            this.FRICodeTextBox.Name = "FRICodeTextBox";
            this.FRICodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
            this.FRICodeTextBox.TabIndex = 9;
            // 
            // FRICodeTypeDropEdit
            // 
            this.FRICodeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FRICodeTypeDropEdit, "FRICodeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).FRICodeType)));
            this.FRICodeTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("3651fe37-c97e-4fce-a0fd-19bd4b1a489d", "FRI");
            this.FRICodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 34, true);
            this.FRICodeTypeDropEdit.Name = "FRICodeTypeDropEdit";
            this.FRICodeTypeDropEdit.PreBoundMaxLength = 3;
            this.FRICodeTypeDropEdit.ShouldResizeByMaxLength = false;
            this.FRICodeTypeDropEdit.ShowDescriptionBox = false;
            this.FRICodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.FRICodeTypeDropEdit.TabIndex = 8;
            // 
            // CBPCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.CBPCodeTextBox, "CBPCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).CBPCode)));
            this.CBPCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("4D4F7275-3C84-49BA-8302-7E66844D92BE", "CBP");
            this.CBPCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 58, true);
            this.CBPCodeTextBox.Name = "CBPCodeTextBox";
            this.CBPCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
            this.CBPCodeTextBox.TabIndex = 5;
            // 
            // CBPCodeTypeDropEdit
            // 
            this.CBPCodeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CBPCodeTypeDropEdit, "CBPCodeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).CBPCodeType)));
            this.CBPCodeTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("3824ab7f-1a97-4b32-8d54-c17cdca8c773", "Bonded ID");
            this.CBPCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 58, true);
            this.CBPCodeTypeDropEdit.Name = "CBPCodeTypeDropEdit";
            this.CBPCodeTypeDropEdit.PreBoundMaxLength = 3;
            this.CBPCodeTypeDropEdit.ShouldResizeByMaxLength = false;
            this.CBPCodeTypeDropEdit.ShowDescriptionBox = false;
            this.CBPCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.CBPCodeTypeDropEdit.TabIndex = 4;
            // 
            // TPCCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.TPCCodeTextBox, "TPCCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).TPCCode)));
            this.TPCCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("0428F735-B36C-4C9E-84CC-50352BB64A94", "TPC");
            this.TPCCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 84, true);
            this.TPCCodeTextBox.Name = "TPCCodeTextBox";
            this.TPCCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
            this.TPCCodeTextBox.TabIndex = 7;
            // 
            // TPCCodeTypeDropEdit
            // 
            this.TPCCodeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TPCCodeTypeDropEdit, "TPCCodeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).TPCCodeType)));
            this.TPCCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 84, true);
            this.TPCCodeTypeDropEdit.Name = "TPCCodeTypeDropEdit";
            this.TPCCodeTypeDropEdit.PreBoundMaxLength = 3;
            this.TPCCodeTypeDropEdit.ShouldResizeByMaxLength = false;
            this.TPCCodeTypeDropEdit.ShowDescriptionBox = false;
            this.TPCCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.TPCCodeTypeDropEdit.TabIndex = 6;
            // 
            // AEOCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.AEOCodeTextBox, "AEOCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).AEOCode)));
            this.AEOCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8D3493D8-5A0D-4FF1-99AC-1FD5E49ADBCD", "AEO");
            this.AEOCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 34, true);
            this.AEOCodeTextBox.Name = "AEOCodeTextBox";
            this.AEOCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
            this.AEOCodeTextBox.TabIndex = 3;
            // 
            // AEOCodeTypeDropEdit
            // 
            this.AEOCodeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AEOCodeTypeDropEdit, "AEOCodeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).AEOCodeType)));
            this.AEOCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 34, true);
            this.AEOCodeTypeDropEdit.Name = "AEOCodeTypeDropEdit";
            this.AEOCodeTypeDropEdit.PreBoundMaxLength = 3;
            this.AEOCodeTypeDropEdit.ShouldResizeByMaxLength = false;
            this.AEOCodeTypeDropEdit.ShowDescriptionBox = false;
            this.AEOCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.AEOCodeTypeDropEdit.TabIndex = 2;
            // 
            // IDCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.IDCodeTextBox, "IDCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).IDCode)));
            this.IDCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("E711B29D-43E9-4EF7-9A5E-88686796BB09", "ID");
            this.IDCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 9, true);
            this.IDCodeTextBox.Name = "IDCodeTextBox";
            this.IDCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
            this.IDCodeTextBox.TabIndex = 1;
            // 
            // IDCodeTypeDropEdit
            // 
            this.IDCodeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.IDCodeTypeDropEdit, "IDCodeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).IDCodeType)));
            this.IDCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 9, true);
            this.IDCodeTypeDropEdit.Name = "IDCodeTypeDropEdit";
            this.IDCodeTypeDropEdit.PreBoundMaxLength = 3;
            this.IDCodeTypeDropEdit.ShouldResizeByMaxLength = false;
            this.IDCodeTypeDropEdit.ShowDescriptionBox = false;
            this.IDCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.IDCodeTypeDropEdit.TabIndex = 0;
            // 
            // OrganisationPanel
            // 
            this.OrganisationPanel.Controls.Add(this.OrganisationFindBox);
            this.OrganisationPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.OrganisationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.OrganisationPanel.Name = "OrganisationPanel";
            this.OrganisationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 28, true);
            this.OrganisationPanel.TabIndex = 1;
            // 
            // OrganisationFindBox
            // 
            this.OrganisationFindBox.AllowDrop = true;
            this.OrganisationFindBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("0AC35CA6-E50E-48B1-B566-104E0C79F9FE", "Organization");
            this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
            this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.OrganisationFindBox.Name = "OrganisationFindBox";
            this.OrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OrganisationFindBox.ParentType = null;
            this.OrganisationFindBox.ShowDescriptionBox = false;
            this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.OrganisationFindBox.TabIndex = 0;
            // 
            // OverrideAddressCheckbox
            // 
            this.BindingSource.SetBindingMember(this.OverrideAddressCheckbox, "E2_AddressOverride");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.TWJobDocAddress)(null)).E2_AddressOverride)));
            this.OverrideAddressCheckbox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2CFAA3E5-8543-464F-9C0E-704230CF858B", "Override");
            this.OverrideAddressCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OverrideAddressCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 0, true);
            this.OverrideAddressCheckbox.Name = "OverrideAddressCheckbox";
            this.OverrideAddressCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
            this.OverrideAddressCheckbox.TabIndex = 3;
            this.OverrideAddressCheckbox.TabStop = false;
            this.OverrideAddressCheckbox.UseVisualStyleBackColor = false;
            // 
            // TWJobDocAddressControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainGroupBox);
            this.Name = "TWJobDocAddressControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 228, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.DetailsTabControl.ResumeLayout(false);
            this.DetailsTabControl.PerformLayout();
            this.AddressTabPage.ResumeLayout(false);
            this.AddressTabPage.PerformLayout();
            this.EditAddressPanel.ResumeLayout(false);
            this.EditAddressPanel.PerformLayout();
            this.StateDropEdit.ResumeLayout(true);
            this.StateDropEdit.PerformLayout();
            this.CountryFindBox.ResumeLayout(true);
            this.CountryFindBox.PerformLayout();
            this.AddressPanel.ResumeLayout(false);
            this.AddressPanel.PerformLayout();
            this.AddressDropEdit.ResumeLayout(true);
            this.AddressDropEdit.PerformLayout();
            this.LocalAddressTabPage.ResumeLayout(false);
            this.LocalAddressTabPage.PerformLayout();
            this.EditLocalAddressPanel.ResumeLayout(false);
            this.EditLocalAddressPanel.PerformLayout();
            this.LocalStateDropEdit.ResumeLayout(true);
            this.LocalStateDropEdit.PerformLayout();
            this.LocalCountryCodeFindBox.ResumeLayout(true);
            this.LocalCountryCodeFindBox.PerformLayout();
            this.ContactTabPage.ResumeLayout(false);
            this.ContactTabPage.PerformLayout();
            this.ContactPanel.ResumeLayout(false);
            this.ContactPanel.PerformLayout();
            this.ContactDropEdit.ResumeLayout(true);
            this.ContactDropEdit.PerformLayout();
            this.EditContactPanel.ResumeLayout(false);
            this.EditContactPanel.PerformLayout();
            this.CodeTabPage.ResumeLayout(false);
            this.CodeTabPage.PerformLayout();
            this.FRICodeTypeDropEdit.ResumeLayout(true);
            this.FRICodeTypeDropEdit.PerformLayout();
            this.CBPCodeTypeDropEdit.ResumeLayout(true);
            this.CBPCodeTypeDropEdit.PerformLayout();
            this.TPCCodeTypeDropEdit.ResumeLayout(true);
            this.TPCCodeTypeDropEdit.PerformLayout();
            this.AEOCodeTypeDropEdit.ResumeLayout(true);
            this.AEOCodeTypeDropEdit.PerformLayout();
            this.IDCodeTypeDropEdit.ResumeLayout(true);
            this.IDCodeTypeDropEdit.PerformLayout();
            this.OrganisationPanel.ResumeLayout(false);
            this.OrganisationPanel.PerformLayout();
            this.OrganisationFindBox.ResumeLayout(true);
            this.OrganisationFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		ZArchitecture.GUI.ZCheckBox OverrideAddressCheckbox;
		ZArchitecture.GUI.ZPanel OrganisationPanel;
		MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		ZArchitecture.GUI.ZDropEditWithFixedWidth ContactDropEdit;
		Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ContactTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage AddressTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage LocalAddressTabPage;
		ZAddressDropEdit.Bare AddressDropEdit;
		Enterprise.ZArchitecture.ZLabel AddressLabel;
		internal ZArchitecture.ZLabel LocalAddressDetailLabel;
		ZArchitecture.ZLabel ContactDetailLabel;
		ZArchitecture.GUI.ZTabPage CodeTabPage;
		protected ZArchitecture.GUI.ZDropEdit IDCodeTypeDropEdit;
		protected ZArchitecture.ZTextBox CBPCodeTextBox;
		protected ZArchitecture.GUI.ZDropEdit CBPCodeTypeDropEdit;
		protected ZArchitecture.ZTextBox TPCCodeTextBox;
		protected ZArchitecture.GUI.ZDropEdit TPCCodeTypeDropEdit;
		protected ZArchitecture.ZTextBox AEOCodeTextBox;
		protected ZArchitecture.GUI.ZDropEdit AEOCodeTypeDropEdit;
		protected ZArchitecture.ZTextBox IDCodeTextBox;
		private ZArchitecture.GUI.ZPanel EditLocalAddressPanel;
		private ZArchitecture.GUI.ZPanel ContactPanel;
		private ZArchitecture.GUI.ZPanel EditAddressPanel;
		private ZArchitecture.GUI.ZPanel AddressPanel;
		private ZArchitecture.GUI.ZPanel EditContactPanel;

		private ZArchitecture.ZTextBox LocalAdditionalAddressInformationTextBox;
		private ZArchitecture.ZTextBox LocalAddress1TextBox;
		private ZArchitecture.ZTextBox LocalAddress2TextBox;
		private ZArchitecture.ZTextBox LocalPostcodeTextBox;
		private ZArchitecture.ZTextBox LocalCompanyNameTextBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth LocalStateDropEdit;
		private ZArchitecture.ZTextBox LocalCityTextBox;
		private MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength LocalCountryCodeFindBox;
		private ZArchitecture.ZTextBox AdditionalAddressInformationTextBox;
		private ZArchitecture.ZTextBox AddressLine1TextBox;
		private ZArchitecture.ZTextBox AddressLine2TextBox;
		private ZArchitecture.ZTextBox PostCodeTextBox;
		private ZArchitecture.ZTextBox CompanyTextBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth StateDropEdit;
		private ZArchitecture.ZTextBox CityTextBox;
		private MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		private ZArchitecture.ZTextBox EmailTextBox;
		private ZArchitecture.ZTextBox PhoneTextBox;
		private ZArchitecture.ZTextBox ContactTextBox;
		private ZArchitecture.ZTextBox FaxTextBox;
		protected ZArchitecture.ZTextBox FRICodeTextBox;
		protected ZArchitecture.GUI.ZDropEdit FRICodeTypeDropEdit;
	}
}
