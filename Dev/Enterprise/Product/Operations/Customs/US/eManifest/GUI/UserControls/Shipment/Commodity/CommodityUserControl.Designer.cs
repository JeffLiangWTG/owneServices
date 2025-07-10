namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class CommodityUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.Customs.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CommodityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.EquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.CommodityDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.HarmonizedNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HarmonizedNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HarmonizedNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HazardousMaterialsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HazardousMaterialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HazardousMaterialsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.VehicleIdentificationNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.VehicleIdentificationNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VehicleIdentificationNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.C4CodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.C4CodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.C4CodesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.C4CodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ShippingMarksControl = new Enterprise.Customs.GUI.LongTextControl();
			this.CommodityDescriptionControl = new Enterprise.Customs.GUI.LongTextControl();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackagesCalcDropEdit.SuspendLayout();
			this.CommodityDetailsGroupBox.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.CustomsValueCalcDropEdit.SuspendLayout();
			this.EquipmentDropEdit.SuspendLayout();
			this.CommodityDetailsTabControl.SuspendLayout();
			this.HarmonizedNumbersTabPage.SuspendLayout();
			this.HarmonizedNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HarmonizedNumbersGrid)).BeginInit();
			this.HarmonizedNumbersGrid.SuspendLayout();
			this.HazardousMaterialsTabPage.SuspendLayout();
			this.HazardousMaterialsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HazardousMaterialsGrid)).BeginInit();
			this.HazardousMaterialsGrid.SuspendLayout();
			this.VehicleIdentificationNumbersTabPage.SuspendLayout();
			this.VehicleIdentificationNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VehicleIdentificationNumbersGrid)).BeginInit();
			this.VehicleIdentificationNumbersGrid.SuspendLayout();
			this.C4CodesTabPage.SuspendLayout();
			this.C4CodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.C4CodesGrid)).BeginInit();
			this.C4CodesGrid.SuspendLayout();
			this.ShippingMarksControl.SuspendLayout();
			this.CommodityDescriptionControl.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Commodity);
			// 
			// PackagesCalcDropEdit
			// 
			this.PackagesCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_PieceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_ManifestUnitCode)));
			this.PackagesCalcDropEdit.BindToAmount = "BY_PieceCount";
			this.PackagesCalcDropEdit.BindToUnit = "BY_ManifestUnitCode";
			this.PackagesCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|f607f40c-7a14-4836-9935-5c8ce6214fd0", "Number Of Packages", "Total Number shown on Bill of Lading for this specific commodity.");
			this.PackagesCalcDropEdit.Decimals = 0;
			this.PackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 19, true);
			this.PackagesCalcDropEdit.Name = "PackagesCalcDropEdit";
			this.PackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PackagesCalcDropEdit.TabIndex = 0;
			this.PackagesCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CommodityDetailsGroupBox
			// 
			this.CommodityDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|2bcf8c0e-e758-4e45-a3da-22634e18ee7e", "Commodity Details");
			this.CommodityDetailsGroupBox.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.CustomsValueCalcDropEdit);
			this.CommodityDetailsGroupBox.Controls.Add(this.EquipmentDropEdit);
			this.CommodityDetailsGroupBox.Controls.Add(this.CommodityDetailsTabControl);
			this.CommodityDetailsGroupBox.Controls.Add(this.ShippingMarksControl);
			this.CommodityDetailsGroupBox.Controls.Add(this.CommodityDescriptionControl);
			this.CommodityDetailsGroupBox.Controls.Add(this.WeightCalcDropEdit);
			this.CommodityDetailsGroupBox.Controls.Add(this.PackagesCalcDropEdit);
			this.CommodityDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityDetailsGroupBox.Name = "CommodityDetailsGroupBox";
			this.CommodityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 300, true);
			this.CommodityDetailsGroupBox.TabIndex = 0;
			this.CommodityDetailsGroupBox.TabStop = false;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.CountryOfOriginCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "BY_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_RN_NKCountryOfOrigin)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|a0a1c3b1-2779-48f9-9cff-5042b4d9e2a4", "Country Of Origin", "The country of manufacture, production, or growth of any article of foreign origin entering the U.S.");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 151, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 6;
			// 
			// CustomsValueCalcDropEdit
			// 
			this.CustomsValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_MonetaryValueCurrency)));
			this.CustomsValueCalcDropEdit.BindToAmount = "BY_MonetaryValue";
			this.CustomsValueCalcDropEdit.BindToUnit = "BY_MonetaryValueCurrency";
			this.CustomsValueCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|835e24d5-c75d-4ee6-bafd-1df7efe5df7c", "Customs Value", "Customs shipment value. In whole dollars. For Sec 321 releases this will be the actual value. The estimated value will be used for IE and TE.");
			this.CustomsValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 129, true);
			this.CustomsValueCalcDropEdit.Name = "CustomsValueCalcDropEdit";
			this.CustomsValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.CustomsValueCalcDropEdit.TabIndex = 5;
			this.CustomsValueCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// EquipmentDropEdit
			// 
			this.EquipmentDropEdit.AllowDrop = true;
			this.EquipmentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EquipmentDropEdit, "BY_BJ_Equipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_BJ_Equipment)));
			this.EquipmentDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|f0aeda74-1065-4c5b-8f11-057c0661e14a", "Equipment", "The goods are loaded into or on to equipment.");
			this.EquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 107, true);
			this.EquipmentDropEdit.Name = "EquipmentDropEdit";
			this.EquipmentDropEdit.PreBoundMaxLength = 11;
			this.EquipmentDropEdit.ShouldResizeByMaxLength = true;
			this.EquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.EquipmentDropEdit.TabIndex = 4;
			// 
			// CommodityDetailsTabControl
			// 
			this.CommodityDetailsTabControl.Controls.Add(this.HarmonizedNumbersTabPage);
			this.CommodityDetailsTabControl.Controls.Add(this.HazardousMaterialsTabPage);
			this.CommodityDetailsTabControl.Controls.Add(this.VehicleIdentificationNumbersTabPage);
			this.CommodityDetailsTabControl.Controls.Add(this.C4CodesTabPage);
			this.CommodityDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 177, true);
			this.CommodityDetailsTabControl.Name = "CommodityDetailsTabControl";
			this.CommodityDetailsTabControl.SelectedIndex = 0;
			this.CommodityDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 120, true);
			this.CommodityDetailsTabControl.TabIndex = 7;
			// 
			// HarmonizedNumbersTabPage
			// 
			this.HarmonizedNumbersTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|c5771007-a6a2-467b-a60a-15268cd77439", "Harmonized Numbers");
			this.HarmonizedNumbersTabPage.Controls.Add(this.HarmonizedNumbersGroupBox);
			this.HarmonizedNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HarmonizedNumbersTabPage.Name = "HarmonizedNumbersTabPage";
			this.HarmonizedNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HarmonizedNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 93, true);
			this.HarmonizedNumbersTabPage.TabIndex = 0;
			// 
			// HarmonizedNumbersGroupBox
			// 
			this.HarmonizedNumbersGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|a41d6d16-ec3d-4150-991b-a893fd43009b", "Harmonized Tariff Numbers");
			this.HarmonizedNumbersGroupBox.Controls.Add(this.HarmonizedNumbersGrid);
			this.HarmonizedNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HarmonizedNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HarmonizedNumbersGroupBox.Name = "HarmonizedNumbersGroupBox";
			this.HarmonizedNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 87, true);
			this.HarmonizedNumbersGroupBox.TabIndex = 0;
			this.HarmonizedNumbersGroupBox.TabStop = false;
			// 
			// HarmonizedNumbersGrid
			// 
			this.HarmonizedNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HarmonizedNumbersGrid, "HarmonizedNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).HarmonizedNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.TariffPropertyInfo)(((Enterprise.Customs.US.eManifest.Business.HarmonizedNumber)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).HarmonizedNumbers)).SyncRoot)).CY_TariffFormattedTariffInfo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.HarmonizedNumber)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).HarmonizedNumbers)).SyncRoot)).CY_TariffFormatted)));
			this.HarmonizedNumbersGrid.CaptionVisible = false;
			tariffColumnStyleInfo1.BindToTariffPropertyInfo = "CY_TariffFormattedTariffInfo";
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|ac7baa27-cec9-4b35-a881-92b2522d5534", "Harmonized Number");
			tariffColumnStyleInfo1.ColumnName = "CY_TariffFormatted";
			tariffColumnStyleInfo1.IsMandatory = true;
			tariffColumnStyleInfo1.TariffCode = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.HarmonizedNumbersGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.HarmonizedNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HarmonizedNumbersGrid.GridId = "1c003e69-2c90-4024-9202-cbab5db2ad59";
			this.HarmonizedNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HarmonizedNumbersGrid.LayoutKey = "HarmonizedNumbersGrid";
			this.HarmonizedNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HarmonizedNumbersGrid.Name = "HarmonizedNumbersGrid";
			this.HarmonizedNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 68, true);
			this.HarmonizedNumbersGrid.TabIndex = 0;
			// 
			// HazardousMaterialsTabPage
			// 
			this.HazardousMaterialsTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|afded1e6-2274-4618-be17-15b31e2d17c6", "Hazardous Materials");
			this.HazardousMaterialsTabPage.Controls.Add(this.HazardousMaterialsGroupBox);
			this.HazardousMaterialsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HazardousMaterialsTabPage.Name = "HazardousMaterialsTabPage";
			this.HazardousMaterialsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HazardousMaterialsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 93, true);
			this.HazardousMaterialsTabPage.TabIndex = 1;
			// 
			// HazardousMaterialsGroupBox
			// 
			this.HazardousMaterialsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|b4647ddd-9b50-4bcb-a1b6-4581ef56c7a3", "Hazardous Materials");
			this.HazardousMaterialsGroupBox.Controls.Add(this.HazardousMaterialsGrid);
			this.HazardousMaterialsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HazardousMaterialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HazardousMaterialsGroupBox.Name = "HazardousMaterialsGroupBox";
			this.HazardousMaterialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 87, true);
			this.HazardousMaterialsGroupBox.TabIndex = 1;
			this.HazardousMaterialsGroupBox.TabStop = false;
			// 
			// HazardousMaterialsGrid
			// 
			this.HazardousMaterialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HazardousMaterialsGrid, "UNDGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).UNDGs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).UNDGs)).SyncRoot)).SubstancePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).UNDGs)).SyncRoot)).DI_OC_DGContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).UNDGs)).SyncRoot)).DGContact.OC_Phone)));
			this.HazardousMaterialsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|4696ea07-e669-4088-8f08-bf94e179426f", "Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SubstancePK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DI_OC_DGContact";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("858948b8-34c5-4a52-a558-74c3b6759503", "Phone", "Contact Phone", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "DGContact+OC_Phone";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HazardousMaterialsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.HazardousMaterialsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.HazardousMaterialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HazardousMaterialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HazardousMaterialsGrid.GridId = "92f09574-2abf-464b-ba5a-694fc769e5bf";
			this.HazardousMaterialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HazardousMaterialsGrid.LayoutKey = "HazardousMaterialsGrid";
			this.HazardousMaterialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HazardousMaterialsGrid.Name = "HazardousMaterialsGrid";
			this.HazardousMaterialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 68, true);
			this.HazardousMaterialsGrid.TabIndex = 0;
			// 
			// VehicleIdentificationNumbersTabPage
			// 
			this.VehicleIdentificationNumbersTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|ba529a45-dec8-488f-8137-9745610d2d5e", "VINs");
			this.VehicleIdentificationNumbersTabPage.Controls.Add(this.VehicleIdentificationNumbersGroupBox);
			this.VehicleIdentificationNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.VehicleIdentificationNumbersTabPage.Name = "VehicleIdentificationNumbersTabPage";
			this.VehicleIdentificationNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.VehicleIdentificationNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 93, true);
			this.VehicleIdentificationNumbersTabPage.TabIndex = 2;
			// 
			// VehicleIdentificationNumbersGroupBox
			// 
			this.VehicleIdentificationNumbersGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|2302fe94-9a56-49e1-bc03-7342ec831c0b", "Vehicle Identification Numbers");
			this.VehicleIdentificationNumbersGroupBox.Controls.Add(this.VehicleIdentificationNumbersGrid);
			this.VehicleIdentificationNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehicleIdentificationNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.VehicleIdentificationNumbersGroupBox.Name = "VehicleIdentificationNumbersGroupBox";
			this.VehicleIdentificationNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 87, true);
			this.VehicleIdentificationNumbersGroupBox.TabIndex = 1;
			this.VehicleIdentificationNumbersGroupBox.TabStop = false;
			// 
			// VehicleIdentificationNumbersGrid
			// 
			this.VehicleIdentificationNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VehicleIdentificationNumbersGrid, "VehicleIdentificationNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).VehicleIdentificationNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.VehicleIdentificationNumber)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).VehicleIdentificationNumbers)).SyncRoot)).CY_Data)));
			this.VehicleIdentificationNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|9997f3a5-0d72-47d8-9d9a-28ec78ead3a8", "VIN", "Vehicle identification number if commodity is a vehicle.");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.VehicleIdentificationNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VehicleIdentificationNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehicleIdentificationNumbersGrid.GridId = "1f9dc5c0-8293-4203-99e7-74292a4d922f";
			this.VehicleIdentificationNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VehicleIdentificationNumbersGrid.LayoutKey = "VehicleIdentificationNumbersGrid";
			this.VehicleIdentificationNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.VehicleIdentificationNumbersGrid.Name = "VehicleIdentificationNumbersGrid";
			this.VehicleIdentificationNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 68, true);
			this.VehicleIdentificationNumbersGrid.TabIndex = 0;
			// 
			// C4CodesTabPage
			// 
			this.C4CodesTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|33e95f1a-bfea-49b1-a2b9-51b6a778477d", "C4 Codes");
			this.C4CodesTabPage.Controls.Add(this.C4CodesGroupBox);
			this.C4CodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.C4CodesTabPage.Name = "C4CodesTabPage";
			this.C4CodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.C4CodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 93, true);
			this.C4CodesTabPage.TabIndex = 3;
			// 
			// C4CodesGroupBox
			// 
			this.C4CodesGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|14884ec8-730c-4964-a495-66f9d00599af", "C4 Codes");
			this.C4CodesGroupBox.Controls.Add(this.C4CodesLabel);
			this.C4CodesGroupBox.Controls.Add(this.C4CodesGrid);
			this.C4CodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.C4CodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.C4CodesGroupBox.Name = "C4CodesGroupBox";
			this.C4CodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 87, true);
			this.C4CodesGroupBox.TabIndex = 2;
			this.C4CodesGroupBox.TabStop = false;
			// 
			// C4CodesLabel
			// 
			this.C4CodesLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c810e623-096a-43f4-8978-26944fe57bda", "Available only for the RAS(BRASS) shipment type with at least one commodity line entered");
			this.C4CodesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.C4CodesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.C4CodesLabel.IsFontBold = true;
			this.C4CodesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.C4CodesLabel.Name = "C4CodesLabel";
			this.C4CodesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 68, true);
			this.C4CodesLabel.TabIndex = 1;
			this.C4CodesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// C4CodesGrid
			// 
			this.C4CodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.C4CodesGrid, "C4Codes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).C4Codes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.C4Code)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).C4Codes)).SyncRoot)).CY_Data)));
			this.C4CodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|ae00344c-929f-4cd8-be66-f3540ece1e80", "C4 Code", "14 Character code assigned by Customs. Required for BRASS (Border Release Advanced Screening and Selectivity) shipments.");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.C4CodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.C4CodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.C4CodesGrid.GridId = "d78316c1-1976-490b-8ffc-cb89c0fb0a8c";
			this.C4CodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.C4CodesGrid.LayoutKey = "C4CodesGrid";
			this.C4CodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.C4CodesGrid.Name = "C4CodesGrid";
			this.C4CodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 68, true);
			this.C4CodesGrid.TabIndex = 0;
			// 
			// ShippingMarksControl
			// 
			this.ShippingMarksControl.AllowDrop = true;
			this.ShippingMarksControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ShippingMarksControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|a902c74d-734a-4df7-adb6-bf5b032e15b3", "Marks And Numbers", "The shipping marks & numbers found on the outside of packaging units.");
			this.ShippingMarksControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ShippingMarksControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 85, true);
			this.ShippingMarksControl.Name = "ShippingMarksControl";
			this.ShippingMarksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.ShippingMarksControl.TabIndex = 3;
			// 
			// CommodityDescriptionControl
			// 
			this.CommodityDescriptionControl.AllowDrop = true;
			this.CommodityDescriptionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommodityDescriptionControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|21f0c8ec-7acc-4fe5-831c-2dab11d9fc97", "Description of Cargo", "A description of the cargo in common trade terms. \"No Freight of All Kinds\" or \"Said to Contain\" will be accepted.");
			this.CommodityDescriptionControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.CommodityDescriptionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 63, true);
			this.CommodityDescriptionControl.Name = "CommodityDescriptionControl";
			this.CommodityDescriptionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.CommodityDescriptionControl.TabIndex = 2;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(null)).BY_GrossWeightUnit)));
			this.WeightCalcDropEdit.BindToAmount = "BY_GrossWeight";
			this.WeightCalcDropEdit.BindToUnit = "BY_GrossWeightUnit";
			this.WeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|8ab406dd-63b8-4a75-b8e0-eb5e2f7683bb", "Cargo Gross Weight", "Weight of the listed cargo plus any packaging, but excluding weight of the carrier\'s equipment.");
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 41, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.WeightCalcDropEdit.TabIndex = 1;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CommodityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommodityDetailsGroupBox);
			this.Name = "CommodityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesCalcDropEdit.ResumeLayout(true);
			this.PackagesCalcDropEdit.PerformLayout();
			this.CommodityDetailsGroupBox.ResumeLayout(false);
			this.CommodityDetailsGroupBox.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.CustomsValueCalcDropEdit.ResumeLayout(true);
			this.CustomsValueCalcDropEdit.PerformLayout();
			this.EquipmentDropEdit.ResumeLayout(true);
			this.EquipmentDropEdit.PerformLayout();
			this.CommodityDetailsTabControl.ResumeLayout(false);
			this.CommodityDetailsTabControl.PerformLayout();
			this.HarmonizedNumbersTabPage.ResumeLayout(false);
			this.HarmonizedNumbersTabPage.PerformLayout();
			this.HarmonizedNumbersGroupBox.ResumeLayout(false);
			this.HarmonizedNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HarmonizedNumbersGrid)).EndInit();
			this.HarmonizedNumbersGrid.ResumeLayout(false);
			this.HarmonizedNumbersGrid.PerformLayout();
			this.HazardousMaterialsTabPage.ResumeLayout(false);
			this.HazardousMaterialsTabPage.PerformLayout();
			this.HazardousMaterialsGroupBox.ResumeLayout(false);
			this.HazardousMaterialsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HazardousMaterialsGrid)).EndInit();
			this.HazardousMaterialsGrid.ResumeLayout(false);
			this.HazardousMaterialsGrid.PerformLayout();
			this.VehicleIdentificationNumbersTabPage.ResumeLayout(false);
			this.VehicleIdentificationNumbersTabPage.PerformLayout();
			this.VehicleIdentificationNumbersGroupBox.ResumeLayout(false);
			this.VehicleIdentificationNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.VehicleIdentificationNumbersGrid)).EndInit();
			this.VehicleIdentificationNumbersGrid.ResumeLayout(false);
			this.VehicleIdentificationNumbersGrid.PerformLayout();
			this.C4CodesTabPage.ResumeLayout(false);
			this.C4CodesTabPage.PerformLayout();
			this.C4CodesGroupBox.ResumeLayout(false);
			this.C4CodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.C4CodesGrid)).EndInit();
			this.C4CodesGrid.ResumeLayout(false);
			this.C4CodesGrid.PerformLayout();
			this.ShippingMarksControl.ResumeLayout(true);
			this.ShippingMarksControl.PerformLayout();
			this.CommodityDescriptionControl.ResumeLayout(true);
			this.CommodityDescriptionControl.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCalcDropEdit PackagesCalcDropEdit;
		private ZArchitecture.GUI.ZGroupBox CommodityDetailsGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private Customs.GUI.LongTextControl CommodityDescriptionControl;
		private Customs.GUI.LongTextControl ShippingMarksControl;
		private ZArchitecture.GUI.ZTabControl CommodityDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage HarmonizedNumbersTabPage;
		private ZArchitecture.GUI.ZTabPage HazardousMaterialsTabPage;
		private ZArchitecture.GUI.ZTabPage VehicleIdentificationNumbersTabPage;
		private ZArchitecture.GUI.ZTabPage C4CodesTabPage;
		private ZArchitecture.GUI.ZGroupBox HarmonizedNumbersGroupBox;
		private ZArchitecture.ZGrid HarmonizedNumbersGrid;
		private ZArchitecture.GUI.ZGroupBox VehicleIdentificationNumbersGroupBox;
		private ZArchitecture.ZGrid VehicleIdentificationNumbersGrid;
		private ZArchitecture.GUI.ZGroupBox C4CodesGroupBox;
		private ZArchitecture.ZGrid C4CodesGrid;
		private ZArchitecture.GUI.ZGroupBox HazardousMaterialsGroupBox;
		private ZArchitecture.ZGrid HazardousMaterialsGrid;
		private ZArchitecture.GUI.ZGuidDropEdit EquipmentDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsValueCalcDropEdit;
		private ZArchitecture.ZLabel C4CodesLabel;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		private System.ComponentModel.IContainer components;
	}
}
