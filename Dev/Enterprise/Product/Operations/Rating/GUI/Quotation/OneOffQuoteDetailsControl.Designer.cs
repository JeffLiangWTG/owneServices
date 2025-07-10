using System.ComponentModel;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class OneOffQuoteDetailsControl
	{
		ZDropEdit DeliveryEquipmentDropEdit;
		ZDropEdit PickupEquipmentDropEdit;
		Container components = null;
		ZGroupBox zGroupBox1;
		ZLabel ChargeableUnitLabel;
		ZCalcDropEdit ActualVolumeDropEdit;
		ZCalcEdit ChargeableDropEdit;
		ZCalcDropEdit ActualWeightDropEdit;
		ZGrid ContainersGrid;
		internal ZGrid LooseCargoGrid;
		ZGroupBox zGroupBox2;
		ZCalcEdit EntriesCalcEdit;
		ZCalcEdit EntryInvoiceLinesCalcEdit;
		ZLabel zLabel4;
		ZCodeFindBox CommodityFindBox;
		ZCodeFindBox ServiceLevelFindBox;
		ZGroupBox zGroupBox3;
		ZCodeFindBox ValueOfGoodsCurrencyFindbox;
		ZCalcEdit ValueOfGoodsCalcEdit;
		ZCalcEdit ValueOfInsuranceCalcEdit;
		ZCodeFindBox ValueOfInsuranceCurrencyFindbox;
		ZLabel zLabel3;

		void InitializeComponent()
		{
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			this.zGroupBox1 = new ZGroupBox();
			this.DeliveryEquipmentDropEdit = new ZDropEdit();
			this.PickupEquipmentDropEdit = new ZDropEdit();
			this.ServiceLevelFindBox = new ZCodeFindBox();
			this.CommodityFindBox = new ZCodeFindBox();
			this.LooseCargoGrid = new ZGrid();
			this.ContainersGrid = new ZGrid();
			this.ChargeableUnitLabel = new ZLabel();
			this.ActualVolumeDropEdit = new ZCalcDropEdit();
			this.ChargeableDropEdit = new ZCalcEdit();
			this.ActualWeightDropEdit = new ZCalcDropEdit();
			this.zGroupBox2 = new ZGroupBox();
			this.EntryInvoiceLinesCalcEdit = new ZCalcEdit();
			this.zLabel4 = new ZLabel();
			this.EntriesCalcEdit = new ZCalcEdit();
			this.zLabel3 = new ZLabel();
			this.zGroupBox3 = new ZGroupBox();
			this.ValueOfInsuranceCurrencyFindbox = new ZCodeFindBox();
			this.ValueOfInsuranceCalcEdit = new ZCalcEdit();
			this.ValueOfGoodsCurrencyFindbox = new ZCodeFindBox();
			this.ValueOfGoodsCalcEdit = new ZCalcEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			((ISupportInitialize)(this.LooseCargoGrid)).BeginInit();
			((ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RateOneOffShipment);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.zGroupBox1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|8bead32b-48c5-4822-841a-5017855397f8", "Goods Details");
			this.zGroupBox1.Controls.Add(this.DeliveryEquipmentDropEdit);
			this.zGroupBox1.Controls.Add(this.PickupEquipmentDropEdit);
			this.zGroupBox1.Controls.Add(this.ServiceLevelFindBox);
			this.zGroupBox1.Controls.Add(this.CommodityFindBox);
			this.zGroupBox1.Controls.Add(this.LooseCargoGrid);
			this.zGroupBox1.Controls.Add(this.ContainersGrid);
			this.zGroupBox1.Controls.Add(this.ChargeableUnitLabel);
			this.zGroupBox1.Controls.Add(this.ActualVolumeDropEdit);
			this.zGroupBox1.Controls.Add(this.ChargeableDropEdit);
			this.zGroupBox1.Controls.Add(this.ActualWeightDropEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 159, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// DeliveryEquipmentDropEdit
			// 
			this.DeliveryEquipmentDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.DeliveryEquipmentDropEdit, "TT_DeliveryEquipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_DeliveryEquipment);
			this.DeliveryEquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 128, true);
			this.DeliveryEquipmentDropEdit.Name = "DeliveryEquipmentDropEdit";
			this.DeliveryEquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.DeliveryEquipmentDropEdit.TabIndex = 17;
			// 
			// PickupEquipmentDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PickupEquipmentDropEdit, "TT_PickupEquipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_PickupEquipment);
			this.PickupEquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 128, true);
			this.PickupEquipmentDropEdit.Name = "PickupEquipmentDropEdit";
			this.PickupEquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.PickupEquipmentDropEdit.TabIndex = 15;
			// 
			// ServiceLevelFindBox
			// 
			this.ServiceLevelFindBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ServiceLevelFindBox, "TT_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_RS_NKServiceLevel);
			this.ServiceLevelFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|11d44795-5da8-481d-85f2-3cc162184a4e", "Service Lvl.");
			this.ServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 104, true);
			this.ServiceLevelFindBox.Name = "ServiceLevelFindBox";
			this.ServiceLevelFindBox.ShowDescriptionBox = false;
			this.ServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ServiceLevelFindBox.TabIndex = 9;
			// 
			// CommodityFindBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityFindBox, "TT_RH_NKCommodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_RH_NKCommodity);
			this.CommodityFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|c68f88f3-f34e-461c-8116-da0945338685", "Commodity");
			this.CommodityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 81, true);
			this.CommodityFindBox.Name = "CommodityFindBox";
			this.CommodityFindBox.ShowDescriptionBox = false;
			this.CommodityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.CommodityFindBox.TabIndex = 7;
			// 
			// LooseCargoGrid
			// 
			this.LooseCargoGrid.AllowNavigation = false;
			this.LooseCargoGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.LooseCargoGrid, "LooseCargo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).LooseCargo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_PackLineCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_F3_NKPackType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_Weight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_WeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_Volume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_VolumeUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_Length);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_Width);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_Height);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((RateOneOffShipment)(null)).LooseCargo)).SyncRoot)).TPL_DimensionUQ);
			this.LooseCargoGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|4cdbd6dc-30e1-48d9-ba9f-5d48d4cea75f", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "TPL_PackLineCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|01ec9ed9-c009-4679-a3c7-3a6be12a137f", "Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "TPL_F3_NKPackType";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|01ec9ed9-c009-4679-a3c7-3a6be12a137f", "Packages");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TPL_Weight";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|03e113ad-bfd7-4c56-8136-a3ed596b5293", "Weight");
			zDropEditColumnStyleInfo1.ColumnName = "TPL_WeightUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|03e113ad-bfd7-4c56-8136-a3ed596b5293", "Weight");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TPL_Volume";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|6b1c9f06-adf0-479d-9453-87f2f7c1c153", "Volume");
			zDropEditColumnStyleInfo2.ColumnName = "TPL_VolumeUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|6b1c9f06-adf0-479d-9453-87f2f7c1c153", "Volume");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TPL_Length";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|8c17533f-4f76-4205-ab34-5bf24ec8e012", "Dimensions");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "TPL_Width";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|8c17533f-4f76-4205-ab34-5bf24ec8e012", "Dimensions");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "TPL_Height";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|8c17533f-4f76-4205-ab34-5bf24ec8e012", "Dimensions");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "TPL_DimensionUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|8c17533f-4f76-4205-ab34-5bf24ec8e012", "Dimensions");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			//to do tpl_rc
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LooseCargoGrid.GridId = "981e0024-231f-4e47-9c29-071145a506c6";
			this.LooseCargoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LooseCargoGrid.LayoutKey = "ContainersGrid";
			this.LooseCargoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 29, true);
			this.LooseCargoGrid.Name = "LooseCargoGrid";
			this.LooseCargoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 96, true);
			this.LooseCargoGrid.TabIndex = 13;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).Containers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffContainers)(((System.Collections.IList)(((RateOneOffShipment)(null)).Containers)).SyncRoot)).TC_ContainerCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffContainers)(((System.Collections.IList)(((RateOneOffShipment)(null)).Containers)).SyncRoot)).TC_RC);
			this.ContainersGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "TC_ContainerCount";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TC_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.GridId = "80f6cb0e-4ac4-41a2-88cf-b0e7e974d922";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 29, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 94, true);
			this.ContainersGrid.TabIndex = 11;
			// 
			// ChargeableUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.ChargeableUnitLabel, "TT_ChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_ChargeableUnit);
			this.ChargeableUnitLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|375bacc5-275a-4aa3-92f4-a2e8bf8cb351", "M3");
			this.ChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 57, true);
			this.ChargeableUnitLabel.Name = "ChargeableUnitLabel";
			this.ChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 24, true);
			this.ChargeableUnitLabel.TabIndex = 45;
			// 
			// ActualVolumeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualVolumeDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_ActualVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_UnitOfVolume);
			this.ActualVolumeDropEdit.BindToAmount = "TT_ActualVolume";
			this.ActualVolumeDropEdit.BindToUnit = "TT_UnitOfVolume";
			this.ActualVolumeDropEdit.Decimals = 3;
			this.ActualVolumeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 37, true);
			this.ActualVolumeDropEdit.Name = "ActualVolumeDropEdit";
			this.ActualVolumeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ActualVolumeDropEdit.TabIndex = 3;
			this.ActualVolumeDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ChargeableDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeableDropEdit, "TT_Chargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_Chargeable);
			this.ChargeableDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|c547d15d-7700-490d-8b79-745c2e4d4e9f", "Chargeable");
			this.ChargeableDropEdit.DecimalPlaces = 3;
			this.ChargeableDropEdit.Decimals = 3;
			this.ChargeableDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 59, true);
			this.ChargeableDropEdit.Name = "ChargeableDropEdit";
			this.ChargeableDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.ChargeableDropEdit.TabIndex = 5;
			this.ChargeableDropEdit.Text = "0.000";
			this.ChargeableDropEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualWeightDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_ActualWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_UnitOfWeight);
			this.ActualWeightDropEdit.BindToAmount = "TT_ActualWeight";
			this.ActualWeightDropEdit.BindToUnit = "TT_UnitOfWeight";
			this.ActualWeightDropEdit.Decimals = 3;
			this.ActualWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 15, true);
			this.ActualWeightDropEdit.Name = "ActualWeightDropEdit";
			this.ActualWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ActualWeightDropEdit.TabIndex = 1;
			this.ActualWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Right;
			this.zGroupBox2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|63b9f369-dee5-433c-816c-f688c62e190d", "Brokerage Details");
			this.zGroupBox2.Controls.Add(this.EntryInvoiceLinesCalcEdit);
			this.zGroupBox2.Controls.Add(this.zLabel4);
			this.zGroupBox2.Controls.Add(this.EntriesCalcEdit);
			this.zGroupBox2.Controls.Add(this.zLabel3);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 76, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// EntryInvoiceLinesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EntryInvoiceLinesCalcEdit, "TT_NumberOfEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_NumberOfEntryLines);
			this.EntryInvoiceLinesCalcEdit.DecimalPlaces = 0;
			this.EntryInvoiceLinesCalcEdit.Decimals = 0;
			this.EntryInvoiceLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 49, true);
			this.EntryInvoiceLinesCalcEdit.Name = "EntryInvoiceLinesCalcEdit";
			this.EntryInvoiceLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.EntryInvoiceLinesCalcEdit.TabIndex = 3;
			this.EntryInvoiceLinesCalcEdit.Text = "0";
			this.EntryInvoiceLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|3455695a-5f0d-430e-adbe-1d5805ea9e30", "Lines");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 24, true);
			this.zLabel4.TabIndex = 2;
			// 
			// EntriesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EntriesCalcEdit, "TT_NumberOfEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_NumberOfEntries);
			this.EntriesCalcEdit.DecimalPlaces = 0;
			this.EntriesCalcEdit.Decimals = 0;
			this.EntriesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 23, true);
			this.EntriesCalcEdit.Name = "EntriesCalcEdit";
			this.EntriesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.EntriesCalcEdit.TabIndex = 1;
			this.EntriesCalcEdit.Text = "0";
			this.EntriesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|1687c7f5-8da6-411e-ab97-e7fc84489603", "Entries");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 24, true);
			this.zLabel3.TabIndex = 0;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right);
			this.zGroupBox3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("OneOffQuoteDetailsControl|4090fb97-cc02-41ab-8891-f84b3a39688b", "Monetary Values");
			this.zGroupBox3.Controls.Add(this.ValueOfInsuranceCurrencyFindbox);
			this.zGroupBox3.Controls.Add(this.ValueOfInsuranceCalcEdit);
			this.zGroupBox3.Controls.Add(this.ValueOfGoodsCurrencyFindbox);
			this.zGroupBox3.Controls.Add(this.ValueOfGoodsCalcEdit);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 82, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 77, true);
			this.zGroupBox3.TabIndex = 2;
			this.zGroupBox3.TabStop = false;
			// 
			// ValueOfInsuranceCurrencyFindbox
			// 
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCurrencyFindbox, "TT_RX_NKInsureValCurr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_RX_NKInsureValCurr);
			this.ValueOfInsuranceCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 46, true);
			this.ValueOfInsuranceCurrencyFindbox.Name = "ValueOfInsuranceCurrencyFindbox";
			this.ValueOfInsuranceCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfInsuranceCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfInsuranceCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 21, true);
			this.ValueOfInsuranceCurrencyFindbox.TabIndex = 4;
			// 
			// ValueOfInsuranceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCalcEdit, "TT_InsureVal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_InsureVal);
			this.ValueOfInsuranceCalcEdit.DecimalPlaces = 2;
			this.ValueOfInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 46, true);
			this.ValueOfInsuranceCalcEdit.Name = "ValueOfInsuranceCalcEdit";
			this.ValueOfInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ValueOfInsuranceCalcEdit.TabIndex = 3;
			this.ValueOfInsuranceCalcEdit.Text = "0.00";
			this.ValueOfInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ValueOfGoodsCurrencyFindbox
			// 
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCurrencyFindbox, "TT_RX_NKGoodsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_RX_NKGoodsCurrency);
			this.ValueOfGoodsCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 21, true);
			this.ValueOfGoodsCurrencyFindbox.Name = "ValueOfGoodsCurrencyFindbox";
			this.ValueOfGoodsCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfGoodsCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfGoodsCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.ValueOfGoodsCurrencyFindbox.TabIndex = 1;
			// 
			// ValueOfGoodsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCalcEdit, "TT_ValueOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffShipment)(null)).TT_ValueOfGoods);
			this.ValueOfGoodsCalcEdit.DecimalPlaces = 2;
			this.ValueOfGoodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 21, true);
			this.ValueOfGoodsCalcEdit.Name = "ValueOfGoodsCalcEdit";
			this.ValueOfGoodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ValueOfGoodsCalcEdit.TabIndex = 0;
			this.ValueOfGoodsCalcEdit.Text = "0.00";
			this.ValueOfGoodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OneOffQuoteDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox3);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Name = "OneOffQuoteDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 162, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((ISupportInitialize)(this.LooseCargoGrid)).EndInit();
			((ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
