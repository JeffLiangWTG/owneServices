namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class ShipperAddressUserControl
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
			this.ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
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
			this.ShipperAddressControl.SuspendLayout();
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
			this.AddressTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("6d522b74-d0ec-4c3e-ae85-fa3cc9a38cdb", "Address");
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
			this.BindingSource.SetBindingMember(this.CityTextBox, "ABL_ShipperCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperCity)));
			this.CityTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("cf5dcf6c-8979-41f5-99ef-a44caa08285a", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 75, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.CityTextBox.TabIndex = 10;
			// 
			// Street1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Street1TextBox, "ABL_ShipperStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperStreet1)));
			this.Street1TextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("d0f9fe69-e65b-4aa1-966f-94d48501ace4", "Address");
			this.Street1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 29, true);
			this.Street1TextBox.Name = "Street1TextBox";
			this.Street1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Street1TextBox.TabIndex = 8;
			// 
			// Street2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Street2TextBox, "ABL_ShipperStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperStreet2)));
			this.Street2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Street2TextBox, false);
			this.Street2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 52, true);
			this.Street2TextBox.Name = "Street2TextBox";
			this.Street2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.Street2TextBox.TabIndex = 9;
			// 
			// PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "ABL_ShipperPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperPostcode)));
			this.PostCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("5f72428f-8927-4e62-9c49-971d3ad69920", "Post Code");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 122, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.PostCodeTextBox.TabIndex = 12;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "ABL_ShipperName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperName)));
			this.NameTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("c73a39fb-4e5e-49f1-82f8-5a6434d28179", "Name");
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 6, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.NameTextBox.TabIndex = 7;
			// 
			// StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StateTextBox, "ABL_ShipperState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperState)));
			this.StateTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("9e6279f2-8b04-45f8-925a-7807c6538095", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 98, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.StateTextBox.TabIndex = 11;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "ABL_RN_NKShipperCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_RN_NKShipperCountry)));
			this.CountryFindBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("28370b76-635b-4981-b90d-f9f5ea49e90a", "Country");
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
			this.LocalAddressTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("df55434e-7760-42e1-9ab2-855b38351c97", "Local Address");
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
			this.BindingSource.SetBindingMember(this.LocalCityTextBox, "ABL_ShipperLocalCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperLocalCity)));
			this.LocalCityTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("6c47627c-db62-434a-a8ee-81d34ebd1f7a", "City");
			this.LocalCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 75, true);
			this.LocalCityTextBox.Name = "LocalCityTextBox";
			this.LocalCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalCityTextBox.TabIndex = 3;
			// 
			// LocalStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStreet1TextBox, "ABL_ShipperLocalStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperLocalStreet1)));
			this.LocalStreet1TextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("14b9052e-04e2-44f9-95fa-5bf2dd4bb37a", "Address");
			this.LocalStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 29, true);
			this.LocalStreet1TextBox.Name = "LocalStreet1TextBox";
			this.LocalStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStreet1TextBox.TabIndex = 1;
			// 
			// LocalStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStreet2TextBox, "ABL_ShipperLocalStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperLocalStreet2)));
			this.LocalStreet2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalStreet2TextBox, false);
			this.LocalStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 52, true);
			this.LocalStreet2TextBox.Name = "LocalStreet2TextBox";
			this.LocalStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStreet2TextBox.TabIndex = 2;
			// 
			// LocalPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalPostCodeTextBox, "ABL_ShipperPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperPostcode)));
			this.LocalPostCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("a9bc06bb-ab27-43c5-9fb6-7b0f7fccff1a", "Post Code");
			this.LocalPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 122, true);
			this.LocalPostCodeTextBox.Name = "LocalPostCodeTextBox";
			this.LocalPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.LocalPostCodeTextBox.TabIndex = 5;
			// 
			// LocalNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalNameTextBox, "ABL_ShipperLocalName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperLocalName)));
			this.LocalNameTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("2da05c90-7feb-4d7f-af71-804c08ffd204", "Name");
			this.LocalNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 6, true);
			this.LocalNameTextBox.Name = "LocalNameTextBox";
			this.LocalNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalNameTextBox.TabIndex = 0;
			// 
			// LocalStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalStateTextBox, "ABL_ShipperLocalState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperLocalState)));
			this.LocalStateTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("04ebb73e-c9d5-45bf-b78e-cbe803c8eddd", "State");
			this.LocalStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 98, true);
			this.LocalStateTextBox.Name = "LocalStateTextBox";
			this.LocalStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.LocalStateTextBox.TabIndex = 4;
			// 
			// LocalCountryCodeFindBox
			// 
			this.LocalCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCountryCodeFindBox, "ABL_RN_NKShipperCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_RN_NKShipperCountry)));
			this.LocalCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("62a1f85a-d425-4eba-aae8-34078157cc87", "Country");
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
			this.CodeTabPage.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("ce982f02-f85e-49d0-8726-fb38375c7da2", "Code");
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
			this.BindingSource.SetBindingMember(this.RegNoTextBox, "ABL_ShipperRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperRegNo)));
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
			this.BindingSource.SetBindingMember(this.RegNoTypeDropEdit, "ABL_ShipperRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ShipperRegNoType)));
			this.RegNoTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("d5a8b96e-72f2-4371-9244-8169d3f73406", "ID");
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
			this.OrganisationPanel.Controls.Add(this.ShipperAddressControl);
			this.OrganisationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrganisationPanel.Name = "OrganisationPanel";
			this.OrganisationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 28, true);
			this.OrganisationPanel.TabIndex = 1;
			// 
			// ShipperAddressControl
			// 
			this.ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressControl, "ABL_OA_Shipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_OA_Shipper)));
			this.ShipperAddressControl.BindToOrgList = "Lookups.Consignors";
			this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 4, true);
			this.ShipperAddressControl.Name = "ShipperAddressControl";
			this.ShipperAddressControl.PopupCaption = "";
			this.ShipperAddressControl.ShowAddress = false;
			this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ShipperAddressControl.TabIndex = 2;
			// 
			// ShipperAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "ShipperAddressUserControl";
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
			this.ShipperAddressControl.ResumeLayout(true);
			this.ShipperAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZTabControl DetailsTabControl;
		private ZArchitecture.GUI.ZPanel OrganisationPanel;
		internal ZArchitecture.GUI.ZAddressControl ShipperAddressControl;
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
