using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class GlobalBusinessIdentifierForm
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
		private new void InitializeComponent()
		{
			this.FirmNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WebsiteURLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DUNSTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GLNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LEITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SelectedAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ManufacturerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipperCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SellerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PackagerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DistributorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendGBIAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendGBIUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendGBIDeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectedAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirmNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Address1Label = new Enterprise.ZArchitecture.ZLabel();
			this.Address2Label = new Enterprise.ZArchitecture.ZLabel();
			this.CountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebSiteURLLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DUNSLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GLNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LEILabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManufacturerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShipperLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SellerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExporterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackagerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DistributorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PotentialRolesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SelectedAddressDropEdit.SuspendLayout();
			this.PotentialRolesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 513, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.GlobalBusinessIdentifierData);
			// 
			// FirmNameTextBox
			// 
			this.FirmNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FirmNameTextBox, "US_FirmName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_FirmName)));
			this.FirmNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59000", "Firm Name");
			this.FirmNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 41, true);
			this.FirmNameTextBox.Name = "FirmNameTextBox";
			this.FirmNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.FirmNameTextBox.TabIndex = 4;
			// 
			// Address1TextBox
			// 
			this.Address1TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Address1TextBox, "US_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_Address1)));
			this.Address1TextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59001", "Address 1");
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 65, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.Address1TextBox.TabIndex = 6;
			// 
			// Address2TextBox
			// 
			this.Address2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Address2TextBox, "US_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_Address2)));
			this.Address2TextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59002", "Address 2");
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 89, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.Address2TextBox.TabIndex = 8;
			// 
			// CountryTextBox
			// 
			this.CountryTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CountryTextBox, "US_Country");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_Country)));
			this.CountryTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59003", "Country");
			this.CountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 113, true);
			this.CountryTextBox.Name = "CountryTextBox";
			this.CountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.CountryTextBox.TabIndex = 10;
			// 
			// CityTextBox
			// 
			this.CityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CityTextBox, "US_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_City)));
			this.CityTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59004", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 113, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.CityTextBox.TabIndex = 12;
			// 
			// PostCodeTextBox
			// 
			this.PostCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "US_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_PostCode)));
			this.PostCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59005", "Post Code");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 137, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.PostCodeTextBox.TabIndex = 14;
			// 
			// StateTextBox
			// 
			this.StateTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StateTextBox, "US_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_State)));
			this.StateTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59006", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 137, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.StateTextBox.TabIndex = 16;
			// 
			// PhoneTextBox
			// 
			this.PhoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "US_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_Phone)));
			this.PhoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59007", "Phone");
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 161, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PhoneTextBox.TabIndex = 18;
			// 
			// WebsiteURLTextBox
			// 
			this.WebsiteURLTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.WebsiteURLTextBox, "US_WebsiteURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_WebsiteURL)));
			this.WebsiteURLTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59008", "Website URL");
			this.WebsiteURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 185, true);
			this.WebsiteURLTextBox.Name = "WebsiteURLTextBox";
			this.WebsiteURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.WebsiteURLTextBox.TabIndex = 20;
			// 
			// DUNSTextBox
			// 
			this.DUNSTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DUNSTextBox, "US_DUNS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_DUNS)));
			this.DUNSTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59009", "DUNS");
			this.DUNSTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 209, true);
			this.DUNSTextBox.Name = "DUNSTextBox";
			this.DUNSTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.DUNSTextBox.TabIndex = 22;
			// 
			// GLNTextBox
			// 
			this.GLNTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GLNTextBox, "US_GLN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_GLN)));
			this.GLNTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F590010", "GLN");
			this.GLNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 233, true);
			this.GLNTextBox.Name = "GLNTextBox";
			this.GLNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.GLNTextBox.TabIndex = 24;
			// 
			// LEITextBox
			// 
			this.LEITextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LEITextBox, "US_LEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_LEI)));
			this.LEITextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59011", "LEI");
			this.LEITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 257, true);
			this.LEITextBox.Name = "LEITextBox";
			this.LEITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.LEITextBox.TabIndex = 26;
			// 
			// SelectedAddressDropEdit
			// 
			this.SelectedAddressDropEdit.AllowDrop = true;
			this.SelectedAddressDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SelectedAddressDropEdit, "US_OA_AddressDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_OA_AddressDetails)));
			this.SelectedAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
			this.SelectedAddressDropEdit.Name = "SelectedAddressDropEdit";
			this.SelectedAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.SelectedAddressDropEdit.TabIndex = 2;
			// 
			// ManufacturerCheckBox
			// 
			this.ManufacturerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManufacturerCheckBox, "US_IsManufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsManufacturer)));
			this.ManufacturerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ManufacturerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 32, true);
			this.ManufacturerCheckBox.Name = "ManufacturerCheckBox";
			this.ManufacturerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ManufacturerCheckBox.TabIndex = 29;
			// 
			// ShipperCheckBox
			// 
			this.ShipperCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShipperCheckBox, "US_IsShipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsShipper)));
			this.ShipperCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShipperCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 56, true);
			this.ShipperCheckBox.Name = "ShipperCheckBox";
			this.ShipperCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShipperCheckBox.TabIndex = 31;
			// 
			// SellerCheckBox
			// 
			this.SellerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SellerCheckBox, "US_IsSeller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsSeller)));
			this.SellerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SellerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 80, true);
			this.SellerCheckBox.Name = "SellerCheckBox";
			this.SellerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SellerCheckBox.TabIndex = 33;
			// 
			// ExporterCheckBox
			// 
			this.ExporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExporterCheckBox, "US_IsExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsExporter)));
			this.ExporterCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 104, true);
			this.ExporterCheckBox.Name = "ExporterCheckBox";
			this.ExporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExporterCheckBox.TabIndex = 35;
			// 
			// PackagerCheckBox
			// 
			this.PackagerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PackagerCheckBox, "US_IsPackager");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsPackager)));
			this.PackagerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PackagerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 128, true);
			this.PackagerCheckBox.Name = "PackagerCheckBox";
			this.PackagerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PackagerCheckBox.TabIndex = 37;
			// 
			// DistributorCheckBox
			// 
			this.DistributorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DistributorCheckBox, "US_IsDistributor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.GlobalBusinessIdentifierData)(null)).US_IsDistributor)));
			this.DistributorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DistributorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 152, true);
			this.DistributorCheckBox.Name = "DistributorCheckBox";
			this.DistributorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DistributorCheckBox.TabIndex = 39;
			// 
			// SendGBIAddButton
			// 
			this.SendGBIAddButton.IsCaptionOverridden = true;
			this.SendGBIAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 469, true);
			this.SendGBIAddButton.Name = "SendGBIAddButton";
			this.SendGBIAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 32, true);
			this.SendGBIAddButton.TabIndex = 40;
			this.SendGBIAddButton.Text = "Send GBI Add";
			this.SendGBIAddButton.ToolTipCaption = null;
			this.SendGBIAddButton.UseVisualStyleBackColor = true;
			this.SendGBIAddButton.Click += new System.EventHandler(this.SendGBIAddButton_Click);
			// 
			// SendGBIUpdateButton
			// 
			this.SendGBIUpdateButton.IsCaptionOverridden = true;
			this.SendGBIUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 469, true);
			this.SendGBIUpdateButton.Name = "SendGBIUpdateButton";
			this.SendGBIUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 32, true);
			this.SendGBIUpdateButton.TabIndex = 41;
			this.SendGBIUpdateButton.Text = "Send GBI Update";
			this.SendGBIUpdateButton.ToolTipCaption = null;
			this.SendGBIUpdateButton.UseVisualStyleBackColor = true;
			this.SendGBIUpdateButton.Click += new System.EventHandler(this.SendGBIUpdateButton_Click);
			// 
			// SendGBIDeleteButton
			// 
			this.SendGBIDeleteButton.IsCaptionOverridden = true;
			this.SendGBIDeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 469, true);
			this.SendGBIDeleteButton.Name = "SendGBIUpdateButton";
			this.SendGBIDeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 32, true);
			this.SendGBIDeleteButton.TabIndex = 42;
			this.SendGBIDeleteButton.Text = "Send GBI Delete";
			this.SendGBIDeleteButton.ToolTipCaption = null;
			this.SendGBIDeleteButton.UseVisualStyleBackColor = true;
			this.SendGBIDeleteButton.Click += new System.EventHandler(this.SendGBIDeleteButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.IsCaptionOverridden = true;
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 469, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 32, true);
			this.CancelButton2.TabIndex = 43;
			this.CancelButton2.Text = "&Cancel";
			this.CancelButton2.ToolTipCaption = null;
			this.CancelButton2.UseVisualStyleBackColor = true;
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton2_Click);
			// 
			// SelectedAddressLabel
			// 
			this.SelectedAddressLabel.AutoSize = true;
			this.SelectedAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectedAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 17, true);
			this.SelectedAddressLabel.Name = "SelectedAddressLabel";
			this.SelectedAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.SelectedAddressLabel.TabIndex = 1;
			this.SelectedAddressLabel.Text = "Selected Address:";
			// 
			// FirmNameLabel
			// 
			this.FirmNameLabel.AutoSize = true;
			this.FirmNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FirmNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 41, true);
			this.FirmNameLabel.Name = "FirmNameLabel";
			this.FirmNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.FirmNameLabel.TabIndex = 3;
			this.FirmNameLabel.Text = "Firm Name:";
			// 
			// Address1Label
			// 
			this.Address1Label.AutoSize = true;
			this.Address1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Address1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 65, true);
			this.Address1Label.Name = "Address1Label";
			this.Address1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.Address1Label.TabIndex = 5;
			this.Address1Label.Text = "Address 1:";
			// 
			// Address2Label
			// 
			this.Address2Label.AutoSize = true;
			this.Address2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Address2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 89, true);
			this.Address2Label.Name = "Address2Label";
			this.Address2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.Address2Label.TabIndex = 7;
			this.Address2Label.Text = "Address 2:";
			// 
			// CountryLabel
			// 
			this.CountryLabel.AutoSize = true;
			this.CountryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 113, true);
			this.CountryLabel.Name = "CountryLabel";
			this.CountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.CountryLabel.TabIndex = 9;
			this.CountryLabel.Text = "Country:";
			// 
			// CityLabel
			// 
			this.CityLabel.AutoSize = true;
			this.CityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 113, true);
			this.CityLabel.Name = "CityLabel";
			this.CityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 13, true);
			this.CityLabel.TabIndex = 15;
			this.CityLabel.Text = "City:";
			// 
			// PostCodeLabel
			// 
			this.PostCodeLabel.AutoSize = true;
			this.PostCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 137, true);
			this.PostCodeLabel.Name = "PostCodeLabel";
			this.PostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.PostCodeLabel.TabIndex = 13;
			this.PostCodeLabel.Text = "Post Code:";
			// 
			// StateLabel
			// 
			this.StateLabel.AutoSize = true;
			this.StateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.StateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 137, true);
			this.StateLabel.Name = "StateLabel";
			this.StateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.StateLabel.TabIndex = 11;
			this.StateLabel.Text = "State:";
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.AutoSize = true;
			this.PhoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 161, true);
			this.PhoneLabel.Name = "PhoneLabel";
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.PhoneLabel.TabIndex = 17;
			this.PhoneLabel.Text = "Phone:";
			// 
			// WebSiteURLLabel
			// 
			this.WebSiteURLLabel.AutoSize = true;
			this.WebSiteURLLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WebSiteURLLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 185, true);
			this.WebSiteURLLabel.Name = "WebSiteURLLabel";
			this.WebSiteURLLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.WebSiteURLLabel.TabIndex = 19;
			this.WebSiteURLLabel.Text = "WebSite URL:";
			// 
			// DUNSLabel
			// 
			this.DUNSLabel.AutoSize = true;
			this.DUNSLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DUNSLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 209, true);
			this.DUNSLabel.Name = "DUNSLabel";
			this.DUNSLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.DUNSLabel.TabIndex = 21;
			this.DUNSLabel.Text = "DUNS:";
			// 
			// GLNLabel
			// 
			this.GLNLabel.AutoSize = true;
			this.GLNLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GLNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 233, true);
			this.GLNLabel.Name = "GLNLabel";
			this.GLNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.GLNLabel.TabIndex = 23;
			this.GLNLabel.Text = "GLN:";
			// 
			// LEILabel
			// 
			this.LEILabel.AutoSize = true;
			this.LEILabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LEILabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 257, true);
			this.LEILabel.Name = "LEILabel";
			this.LEILabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.LEILabel.TabIndex = 25;
			this.LEILabel.Text = "LEI:";
			// 
			// ManufacturerLabel
			// 
			this.ManufacturerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ManufacturerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 32, true);
			this.ManufacturerLabel.Name = "ManufacturerLabel";
			this.ManufacturerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.ManufacturerLabel.TabIndex = 28;
			this.ManufacturerLabel.Text = "Manufacturer";
			// 
			// ShipperLabel
			// 
			this.ShipperLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ShipperLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 56, true);
			this.ShipperLabel.Name = "ShipperLabel";
			this.ShipperLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.ShipperLabel.TabIndex = 30;
			this.ShipperLabel.Text = "Shipper";
			// 
			// SellerLabel
			// 
			this.SellerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SellerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 80, true);
			this.SellerLabel.Name = "SellerLabel";
			this.SellerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.SellerLabel.TabIndex = 32;
			this.SellerLabel.Text = "Seller";
			// 
			// ExporterLabel
			// 
			this.ExporterLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExporterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 104, true);
			this.ExporterLabel.Name = "ExporterLabel";
			this.ExporterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 18, true);
			this.ExporterLabel.TabIndex = 34;
			this.ExporterLabel.Text = "Exporter";
			// 
			// PackagerLabel
			// 
			this.PackagerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackagerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 128, true);
			this.PackagerLabel.Name = "PackagerLabel";
			this.PackagerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 18, true);
			this.PackagerLabel.TabIndex = 36;
			this.PackagerLabel.Text = "Packager";
			// 
			// DistributorLabel
			// 
			this.DistributorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DistributorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 152, true);
			this.DistributorLabel.Name = "DistributorLabel";
			this.DistributorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.DistributorLabel.TabIndex = 38;
			this.DistributorLabel.Text = "Distributor";
			// 
			// PotentialRolesGroupBox
			// 
			this.PotentialRolesGroupBox.AccessibleDescription = "1";
			this.PotentialRolesGroupBox.Controls.Add(this.ManufacturerLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.ShipperLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.SellerLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.ExporterLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.PackagerLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.DistributorLabel);
			this.PotentialRolesGroupBox.Controls.Add(this.ManufacturerCheckBox);
			this.PotentialRolesGroupBox.Controls.Add(this.ShipperCheckBox);
			this.PotentialRolesGroupBox.Controls.Add(this.SellerCheckBox);
			this.PotentialRolesGroupBox.Controls.Add(this.ExporterCheckBox);
			this.PotentialRolesGroupBox.Controls.Add(this.PackagerCheckBox);
			this.PotentialRolesGroupBox.Controls.Add(this.DistributorCheckBox);
			this.PotentialRolesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 284, true);
			this.PotentialRolesGroupBox.Name = "PotentialRolesGroupBox";
			this.PotentialRolesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 179, true);
			this.PotentialRolesGroupBox.TabIndex = 27;
			this.PotentialRolesGroupBox.TabStop = false;
			this.PotentialRolesGroupBox.Text = "Please select the potential roles that may apply to the above entity (tick at lea" +
    "st one):";
			// 
			// GlobalBusinessIdentifierForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 537, true);
			this.Controls.Add(this.FirmNameTextBox);
			this.Controls.Add(this.Address1TextBox);
			this.Controls.Add(this.Address2TextBox);
			this.Controls.Add(this.CountryTextBox);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.PostCodeTextBox);
			this.Controls.Add(this.StateTextBox);
			this.Controls.Add(this.PhoneTextBox);
			this.Controls.Add(this.WebsiteURLTextBox);
			this.Controls.Add(this.DUNSTextBox);
			this.Controls.Add(this.GLNTextBox);
			this.Controls.Add(this.LEITextBox);
			this.Controls.Add(this.SelectedAddressDropEdit);
			this.Controls.Add(this.SendGBIAddButton);
			this.Controls.Add(this.SendGBIUpdateButton);
			this.Controls.Add(this.SendGBIDeleteButton);
			this.Controls.Add(this.CancelButton2);
			this.Controls.Add(this.SelectedAddressLabel);
			this.Controls.Add(this.FirmNameLabel);
			this.Controls.Add(this.Address1Label);
			this.Controls.Add(this.Address2Label);
			this.Controls.Add(this.CountryLabel);
			this.Controls.Add(this.CityLabel);
			this.Controls.Add(this.PostCodeLabel);
			this.Controls.Add(this.StateLabel);
			this.Controls.Add(this.PhoneLabel);
			this.Controls.Add(this.WebSiteURLLabel);
			this.Controls.Add(this.DUNSLabel);
			this.Controls.Add(this.GLNLabel);
			this.Controls.Add(this.LEILabel);
			this.Controls.Add(this.PotentialRolesGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.GlobalBusinessIdentifierData);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GlobalBusinessIdentifierForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "GlobalBusinessIdentifierForm";
			this.Controls.SetChildIndex(this.PotentialRolesGroupBox, 0);
			this.Controls.SetChildIndex(this.LEILabel, 0);
			this.Controls.SetChildIndex(this.GLNLabel, 0);
			this.Controls.SetChildIndex(this.DUNSLabel, 0);
			this.Controls.SetChildIndex(this.WebSiteURLLabel, 0);
			this.Controls.SetChildIndex(this.PhoneLabel, 0);
			this.Controls.SetChildIndex(this.StateLabel, 0);
			this.Controls.SetChildIndex(this.PostCodeLabel, 0);
			this.Controls.SetChildIndex(this.CityLabel, 0);
			this.Controls.SetChildIndex(this.CountryLabel, 0);
			this.Controls.SetChildIndex(this.Address2Label, 0);
			this.Controls.SetChildIndex(this.Address1Label, 0);
			this.Controls.SetChildIndex(this.FirmNameLabel, 0);
			this.Controls.SetChildIndex(this.SelectedAddressLabel, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.SendGBIDeleteButton, 0);
			this.Controls.SetChildIndex(this.SendGBIUpdateButton, 0);
			this.Controls.SetChildIndex(this.SendGBIAddButton, 0);
			this.Controls.SetChildIndex(this.SelectedAddressDropEdit, 0);
			this.Controls.SetChildIndex(this.LEITextBox, 0);
			this.Controls.SetChildIndex(this.GLNTextBox, 0);
			this.Controls.SetChildIndex(this.DUNSTextBox, 0);
			this.Controls.SetChildIndex(this.WebsiteURLTextBox, 0);
			this.Controls.SetChildIndex(this.PhoneTextBox, 0);
			this.Controls.SetChildIndex(this.StateTextBox, 0);
			this.Controls.SetChildIndex(this.PostCodeTextBox, 0);
			this.Controls.SetChildIndex(this.CityTextBox, 0);
			this.Controls.SetChildIndex(this.CountryTextBox, 0);
			this.Controls.SetChildIndex(this.Address2TextBox, 0);
			this.Controls.SetChildIndex(this.Address1TextBox, 0);
			this.Controls.SetChildIndex(this.FirmNameTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SelectedAddressDropEdit.ResumeLayout(true);
			this.SelectedAddressDropEdit.PerformLayout();
			this.PotentialRolesGroupBox.ResumeLayout(false);
			this.PotentialRolesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox FirmNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox Address1TextBox;
		private Enterprise.ZArchitecture.ZTextBox Address2TextBox;
		private Enterprise.ZArchitecture.ZTextBox CountryTextBox;
		private Enterprise.ZArchitecture.ZTextBox CityTextBox;
		private Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox StateTextBox;
		private Enterprise.ZArchitecture.ZTextBox PhoneTextBox;
		private Enterprise.ZArchitecture.ZTextBox WebsiteURLTextBox;
		private Enterprise.ZArchitecture.ZTextBox DUNSTextBox;
		private Enterprise.ZArchitecture.ZTextBox GLNTextBox;
		private Enterprise.ZArchitecture.ZTextBox LEITextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit SelectedAddressDropEdit;
		private Enterprise.ZArchitecture.ZLabel SelectedAddressLabel;
		private Enterprise.ZArchitecture.ZLabel FirmNameLabel;
		private Enterprise.ZArchitecture.ZLabel Address1Label;
		private Enterprise.ZArchitecture.ZLabel Address2Label;
		private Enterprise.ZArchitecture.ZLabel CountryLabel;
		private Enterprise.ZArchitecture.ZLabel CityLabel;
		private Enterprise.ZArchitecture.ZLabel PostCodeLabel;
		private Enterprise.ZArchitecture.ZLabel StateLabel;
		private Enterprise.ZArchitecture.ZLabel PhoneLabel;
		private Enterprise.ZArchitecture.ZLabel WebSiteURLLabel;
		private Enterprise.ZArchitecture.ZLabel DUNSLabel;
		private Enterprise.ZArchitecture.ZLabel GLNLabel;
		private Enterprise.ZArchitecture.ZLabel LEILabel;
		private ZArchitecture.GUI.ZCheckBox ManufacturerCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShipperCheckBox;
		private ZArchitecture.GUI.ZCheckBox SellerCheckBox;
		private ZArchitecture.GUI.ZCheckBox ExporterCheckBox;
		private ZArchitecture.GUI.ZCheckBox PackagerCheckBox;
		private ZArchitecture.GUI.ZCheckBox DistributorCheckBox;
		private Enterprise.ZArchitecture.ZLabel ManufacturerLabel;
		private Enterprise.ZArchitecture.ZLabel ShipperLabel;
		private Enterprise.ZArchitecture.ZLabel SellerLabel;
		private Enterprise.ZArchitecture.ZLabel ExporterLabel;
		private Enterprise.ZArchitecture.ZLabel PackagerLabel;
		private Enterprise.ZArchitecture.ZLabel DistributorLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton SendGBIAddButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SendGBIUpdateButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SendGBIDeleteButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		public ZGroupBox PotentialRolesGroupBox;
	}
}
