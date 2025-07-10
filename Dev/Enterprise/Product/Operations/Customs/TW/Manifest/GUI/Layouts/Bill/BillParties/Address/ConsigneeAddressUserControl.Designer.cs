namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class ConsigneeAddressUserControl
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
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Street1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Street2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.LocalAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LocalCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalCountryCodeFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.CodeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegNoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrganisationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.AddressTabPage.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.LocalAddressTabPage.SuspendLayout();
			this.LocalCountryCodeFindBox.SuspendLayout();
			this.CodeTabPage.SuspendLayout();
			this.RegNoTypeDropEdit.SuspendLayout();
			this.OrganisationPanel.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.AsycudaBill);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.DetailsTabControl);
			this.MainGroupBox.Controls.Add(this.OrganisationPanel);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 240, true);
			this.MainGroupBox.TabIndex = 2;
			this.MainGroupBox.TabStop = false;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.AddressTabPage);
			this.DetailsTabControl.Controls.Add(this.LocalAddressTabPage);
			this.DetailsTabControl.Controls.Add(this.CodeTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.ShowToolTips = true;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 193, true);
			this.DetailsTabControl.TabIndex = 0;
			// 
			// AddressTabPage
			// 
			this.AddressTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AddressTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("5f59c8df-ed98-4cfd-9a4a-764117cb7b7f", "Address");
			this.AddressTabPage.Controls.Add(this.CityTextBox);
			this.AddressTabPage.Controls.Add(this.Street1TextBox);
			this.AddressTabPage.Controls.Add(this.Street2TextBox);
			this.AddressTabPage.Controls.Add(this.PostCodeTextBox);
			this.AddressTabPage.Controls.Add(this.NameTextBox);
			this.AddressTabPage.Controls.Add(this.StateTextBox);
			this.AddressTabPage.Controls.Add(this.CountryFindBox);
			this.AddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressTabPage.Name = "AddressTabPage";
			this.AddressTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 166, true);
			this.AddressTabPage.TabIndex = 0;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "ABL_ConsigneeCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeCity)));
			this.CityTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("5df50e3c-c973-4be3-85ad-42a59a2671c0", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 75, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.CityTextBox.TabIndex = 10;
			// 
			// Street1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Street1TextBox, "ABL_ConsigneeStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeStreet1)));
			this.Street1TextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("8e77af74-554b-48a5-a200-dbd3f43d5bca", "Address");
			this.Street1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 29, true);
			this.Street1TextBox.Name = "Street1TextBox";
			this.Street1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Street1TextBox.TabIndex = 8;
			// 
			// Street2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Street2TextBox, "ABL_ConsigneeStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeStreet2)));
			this.Street2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Street2TextBox, false);
			this.Street2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 52, true);
			this.Street2TextBox.Name = "Street2TextBox";
			this.Street2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Street2TextBox.TabIndex = 9;
			// 
			// PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "ABL_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneePostcode)));
			this.PostCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("b623a92d-a1f9-4a6f-8bb8-e373856727bd", "Post Code");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 122, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.PostCodeTextBox.TabIndex = 12;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "ABL_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeName)));
			this.NameTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("8d9a33bc-376a-4d00-9f48-9c7e48e04369", "Name");
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 6, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.NameTextBox.TabIndex = 7;
			// 
			// StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StateTextBox, "ABL_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeState)));
			this.StateTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("119afeb7-381d-4382-b55a-501b0cee6d80", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 98, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.StateTextBox.TabIndex = 11;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "ABL_RN_NKConsigneeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_RN_NKConsigneeCountry)));
			this.CountryFindBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("f0ee8897-3f51-4e8c-9608-b0854e5a1730", "Country");
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 122, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryFindBox.ParentType = null;
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 13;
			// 
			// LocalAddressTabPage
			// 
			this.LocalAddressTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LocalAddressTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("ea6e22ab-eb40-48f1-92ff-ede1ad4923c6", "Local Address");
			this.LocalAddressTabPage.Controls.Add(this.LocalCityTextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalStreet1TextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalStreet2TextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalPostCodeTextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalNameTextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalStateTextBox);
			this.LocalAddressTabPage.Controls.Add(this.LocalCountryCodeFindBox);
			this.LocalAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LocalAddressTabPage.Name = "LocalAddressTabPage";
			this.LocalAddressTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LocalAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 166, true);
			this.LocalAddressTabPage.TabIndex = 1;
			// 
			// LocalCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalCityTextBox, "ABL_ConsigneeLocalCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeLocalCity)));
			this.LocalCityTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("5e452a67-4362-4141-a39e-27704406089c", "City");
			this.LocalCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 75, true);
			this.LocalCityTextBox.Name = "LocalCityTextBox";
			this.LocalCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalCityTextBox.TabIndex = 3;
			// 
			// LocalStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStreet1TextBox, "ABL_ConsigneeLocalStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeLocalStreet1)));
			this.LocalStreet1TextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("0ee28e96-8bbe-4146-a051-cf788d042229", "Address");
			this.LocalStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 29, true);
			this.LocalStreet1TextBox.Name = "LocalStreet1TextBox";
			this.LocalStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStreet1TextBox.TabIndex = 1;
			// 
			// LocalStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStreet2TextBox, "ABL_ConsigneeLocalStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeLocalStreet2)));
			this.LocalStreet2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalStreet2TextBox, false);
			this.LocalStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 52, true);
			this.LocalStreet2TextBox.Name = "LocalStreet2TextBox";
			this.LocalStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStreet2TextBox.TabIndex = 2;
			// 
			// LocalPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalPostCodeTextBox, "ABL_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneePostcode)));
			this.LocalPostCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("019fe60b-6e0f-48b2-948d-fa4873f2a596", "Post Code");
			this.LocalPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 122, true);
			this.LocalPostCodeTextBox.Name = "LocalPostCodeTextBox";
			this.LocalPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.LocalPostCodeTextBox.TabIndex = 5;
			// 
			// LocalNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalNameTextBox, "ABL_ConsigneeLocalName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeLocalName)));
			this.LocalNameTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("4723a0e7-5704-493d-b0db-58f538a8f0b1", "Name");
			this.LocalNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 6, true);
			this.LocalNameTextBox.Name = "LocalNameTextBox";
			this.LocalNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalNameTextBox.TabIndex = 0;
			// 
			// LocalStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStateTextBox, "ABL_ConsigneeLocalState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeLocalState)));
			this.LocalStateTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("6ecb14ee-9f94-486a-9724-61477c02e692", "State");
			this.LocalStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 98, true);
			this.LocalStateTextBox.Name = "LocalStateTextBox";
			this.LocalStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStateTextBox.TabIndex = 4;
			// 
			// LocalCountryCodeFindBox
			// 
			this.LocalCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCountryCodeFindBox, "ABL_RN_NKConsigneeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_RN_NKConsigneeCountry)));
			this.LocalCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("bc909a8b-275e-45aa-b83d-dabc65e328fd", "Country");
			this.LocalCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 122, true);
			this.LocalCountryCodeFindBox.Name = "LocalCountryCodeFindBox";
			this.LocalCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocalCountryCodeFindBox.ParentType = null;
			this.LocalCountryCodeFindBox.PreBoundMaxLength = 3;
			this.LocalCountryCodeFindBox.ShowDescriptionBox = false;
			this.LocalCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.LocalCountryCodeFindBox.TabIndex = 6;
			// 
			// CodeTabPage
			// 
			this.CodeTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CodeTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("7057c843-7c4b-45a2-8f8b-ce32d5357f13", "Code");
			this.CodeTabPage.Controls.Add(this.RegNoTextBox);
			this.CodeTabPage.Controls.Add(this.RegNoTypeDropEdit);
			this.CodeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CodeTabPage.Name = "CodeTabPage";
			this.CodeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CodeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 166, true);
			this.CodeTabPage.TabIndex = 2;
			// 
			// RegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegNoTextBox, "ABL_ConsigneeRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeRegNo)));
			this.RegNoTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegNoTextBox, false);
			this.RegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 6, true);
			this.RegNoTextBox.Name = "RegNoTextBox";
			this.RegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.RegNoTextBox.TabIndex = 1;
			// 
			// RegNoTypeDropEdit
			// 
			this.RegNoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegNoTypeDropEdit, "ABL_ConsigneeRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeRegNoType)));
			this.RegNoTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("a51e20c9-9f84-48fb-b804-170fc5c0ca20", "ID");
			this.RegNoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 6, true);
			this.RegNoTypeDropEdit.Name = "RegNoTypeDropEdit";
			this.RegNoTypeDropEdit.PreBoundMaxLength = 3;
			this.RegNoTypeDropEdit.ShouldResizeByMaxLength = false;
			this.RegNoTypeDropEdit.ShowDescriptionBox = false;
			this.RegNoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RegNoTypeDropEdit.TabIndex = 0;
			// 
			// OrganisationPanel
			// 
			this.OrganisationPanel.Controls.Add(this.ConsigneeAddressControl);
			this.OrganisationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrganisationPanel.Name = "OrganisationPanel";
			this.OrganisationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 28, true);
			this.OrganisationPanel.TabIndex = 1;
			// 
			// ConsigneeAddressControl
			// 
			this.ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressControl, "ABL_OA_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_OA_Consignee)));
			this.ConsigneeAddressControl.BindToOrgList = "Lookups.Consignors";
			this.ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 4, true);
			this.ConsigneeAddressControl.Name = "ConsigneeAddressControl";
			this.ConsigneeAddressControl.PopupCaption = "";
			this.ConsigneeAddressControl.ShowAddress = false;
			this.ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsigneeAddressControl.TabIndex = 2;
			// 
			// ConsigneeAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "ConsigneeAddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.AddressTabPage.ResumeLayout(false);
			this.AddressTabPage.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.LocalAddressTabPage.ResumeLayout(false);
			this.LocalAddressTabPage.PerformLayout();
			this.LocalCountryCodeFindBox.ResumeLayout(true);
			this.LocalCountryCodeFindBox.PerformLayout();
			this.CodeTabPage.ResumeLayout(false);
			this.CodeTabPage.PerformLayout();
			this.RegNoTypeDropEdit.ResumeLayout(true);
			this.RegNoTypeDropEdit.PerformLayout();
			this.OrganisationPanel.ResumeLayout(false);
			this.OrganisationPanel.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZTabControl DetailsTabControl;
		private ZArchitecture.GUI.ZPanel OrganisationPanel;
		internal ZArchitecture.GUI.ZAddressControl ConsigneeAddressControl;
		private ZArchitecture.GUI.ZTabPage AddressTabPage;
		private ZArchitecture.GUI.ZTabPage LocalAddressTabPage;
		private ZArchitecture.GUI.ZTabPage CodeTabPage;
		private ZArchitecture.ZTextBox LocalCityTextBox;
		private ZArchitecture.ZTextBox LocalStreet1TextBox;
		private ZArchitecture.ZTextBox LocalStreet2TextBox;
		private ZArchitecture.ZTextBox LocalPostCodeTextBox;
		private ZArchitecture.ZTextBox LocalNameTextBox;
		private ZArchitecture.ZTextBox LocalStateTextBox;
		private MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength LocalCountryCodeFindBox;
		protected ZArchitecture.ZTextBox RegNoTextBox;
		protected ZArchitecture.GUI.ZDropEdit RegNoTypeDropEdit;
		private ZArchitecture.ZTextBox CityTextBox;
		private ZArchitecture.ZTextBox Street1TextBox;
		private ZArchitecture.ZTextBox Street2TextBox;
		private ZArchitecture.ZTextBox PostCodeTextBox;
		private ZArchitecture.ZTextBox NameTextBox;
		private ZArchitecture.ZTextBox StateTextBox;
		private MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
	}
}
