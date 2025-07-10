using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class InventoryUserControl
	{
		ZGroupBox zGroupBox1;
		ZCalcEdit zCalcEdit4;
		ZCalcEdit zCalcEdit3;
		ZCalcEdit zCalcEdit2;
		ZTextBox zTextBox5;
		ZCalcEdit zCalcEdit1;
		ZTextBox zTextBox4;
		ZTextBox zTextBox3;
		ZDateEdit zDateEdit1;
		ZTextBox zTextBox1;
		ZTextBox AllocationKeyTextBox;
		ZGroupBox AttributesGroupBox;
		protected ZTextBox WE_BondedEntryKeyTextBox;
		protected ZTextBox WE_PartAttrib3TextBox;
		protected ZTextBox WE_PartAttrib2TextBox;
		protected ZDateEdit WE_ExpiryDateEdit;
		protected ZDateEdit WE_PackingDateEdit;
		protected ZTextBox WE_PartAttrib1TextBox;
		ZDateTimeOffsetEdit WI_ArrivalDateEdit;
		ZDropEdit HeldCodeDropEdit;
		ZTextBox LocationStringTextBox;
		ZCalcEdit AvailableToPickQuantityCalcEdit;
		ZCalcEdit WI_TotalUnitsCalcEdit;
		ZCalcEdit CommittedQuantityIncludingUnfinalisedReceiptCalcEdit;
		ZTextBox TotalUnitsUQTextBox;
		ZTextBox CommittedUnitsUQTextBox;
		ZTextBox AvailableUnitsUQTextBox;
		ZTextBox LocationPickAreaNameTextBox;
		ZTextBox LocationPickAreaTypeTextBox;
		ZButton ViewStatusButton;
		ZButton ViewReceiptButton;
		ZButton CommittedButton;
		ZTextBox ReceiptReferenceTextBox;
		ZCalcEdit WI_InDocketLineUnitsCalcEdit;
		ZTextBox ReceiptUnitsUQTextBox;
		ZGuidFindBox zGuidFindBox1;
		ZGuidFindBox zGuidFindBox2;
		ZGuidFindBox WE_OPGuidFindBox;
		ZCodeFindBox CommodityCodeCodeFindBox;
		ZButton AllocatedToPickButton;
		ZTextBox PickAllocationsAsString;
		ZCalcEdit zCalcEdit5;
		ZTextBox zTextBox10;
		ZButton CrossDockButton;
		ZTextBox WI_PalletIDTextBox;
		ZDropEdit StatusDropEdit;
		ZCalcEdit HeldCodeChangeQuantity;
		ZDropEdit HeldCodeToChangeToDropEdit;
		ZTextBox zTextBox2;
		ZCalcEdit zCalcEdit6;
		ZTextBox zTextBox8;
		ZTextBox zTextBox7;
		ZTextBox zTextBox9;
		ZCalcEdit zCalcEdit7;
		ZAddressControl ManufacturerAddressControl;
		ZLabel zLabelAddInfo;
		ZGroupBox MainGroupBox;
		ZDateEdit zDateEdit2;
		ZTextBox zTextBox12;
		ZTextBox zTextBox11;
		ZTextBox HoldReasonToChangeToTextBox;
		ZTextBox HoldCodeReasonTextBox;
		protected ZTextBox WE_SerialNumberTextBox;
		ZTextBox AvailableToTransferUnitsUQTextBox;
		ZCalcEdit AvailableToTransferQuantityCalcEdit;
		ZGrid zGridBondedAdditionalInfo;

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.zGroupBox1 = new ZGroupBox();
			this.zDateEdit2 = new ZDateEdit();
			this.zTextBox12 = new ZTextBox();
			this.zTextBox11 = new ZTextBox();
			this.zLabelAddInfo = new ZLabel();
			this.zGridBondedAdditionalInfo = new ZGrid();
			this.ManufacturerAddressControl = new ZAddressControl();
			this.zTextBox9 = new ZTextBox();
			this.zCalcEdit7 = new ZCalcEdit();
			this.zTextBox8 = new ZTextBox();
			this.zTextBox7 = new ZTextBox();
			this.zTextBox2 = new ZTextBox();
			this.zCalcEdit6 = new ZCalcEdit();
			this.zCalcEdit4 = new ZCalcEdit();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zTextBox5 = new ZTextBox();
			this.zCalcEdit1 = new ZCalcEdit();
			this.zTextBox4 = new ZTextBox();
			this.zTextBox3 = new ZTextBox();
			this.zDateEdit1 = new ZDateEdit();
			this.zTextBox1 = new ZTextBox();
			this.AllocationKeyTextBox = new ZTextBox();
			this.AttributesGroupBox = new ZGroupBox();
			this.WE_SerialNumberTextBox = new ZTextBox();
			this.WE_BondedEntryKeyTextBox = new ZTextBox();
			this.WE_PartAttrib3TextBox = new ZTextBox();
			this.WE_PartAttrib2TextBox = new ZTextBox();
			this.WE_ExpiryDateEdit = new ZDateEdit();
			this.WE_PackingDateEdit = new ZDateEdit();
			this.WE_PartAttrib1TextBox = new ZTextBox();
			this.WI_ArrivalDateEdit = new ZDateTimeOffsetEdit();
			this.HeldCodeDropEdit = new ZDropEdit();
			this.LocationStringTextBox = new ZTextBox();
			this.AvailableToPickQuantityCalcEdit = new ZCalcEdit();
			this.WI_TotalUnitsCalcEdit = new ZCalcEdit();
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit = new ZCalcEdit();
			this.TotalUnitsUQTextBox = new ZTextBox();
			this.CommittedUnitsUQTextBox = new ZTextBox();
			this.AvailableUnitsUQTextBox = new ZTextBox();
			this.LocationPickAreaNameTextBox = new ZTextBox();
			this.LocationPickAreaTypeTextBox = new ZTextBox();
			this.ViewStatusButton = new ZButton();
			this.ViewReceiptButton = new ZButton();
			this.CommittedButton = new ZButton();
			this.ReceiptReferenceTextBox = new ZTextBox();
			this.WI_InDocketLineUnitsCalcEdit = new ZCalcEdit();
			this.ReceiptUnitsUQTextBox = new ZTextBox();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.zGuidFindBox2 = new ZGuidFindBox();
			this.WE_OPGuidFindBox = new ZGuidFindBox();
			this.CommodityCodeCodeFindBox = new ZCodeFindBox();
			this.AllocatedToPickButton = new ZButton();
			this.PickAllocationsAsString = new ZTextBox();
			this.zCalcEdit5 = new ZCalcEdit();
			this.zTextBox10 = new ZTextBox();
			this.CrossDockButton = new ZButton();
			this.WI_PalletIDTextBox = new ZTextBox();
			this.MainGroupBox = new ZGroupBox();
			this.HoldReasonToChangeToTextBox = new ZTextBox();
			this.HoldCodeReasonTextBox = new ZTextBox();
			this.HeldCodeToChangeToDropEdit = new ZDropEdit();
			this.HeldCodeChangeQuantity = new ZCalcEdit();
			this.StatusDropEdit = new ZDropEdit();
			this.AvailableToTransferUnitsUQTextBox = new ZTextBox();
			this.AvailableToTransferQuantityCalcEdit = new ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridBondedAdditionalInfo)).BeginInit();
			this.zGridBondedAdditionalInfo.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.AttributesGroupBox.SuspendLayout();
			this.WE_ExpiryDateEdit.SuspendLayout();
			this.WE_PackingDateEdit.SuspendLayout();
			this.WI_ArrivalDateEdit.SuspendLayout();
			this.HeldCodeDropEdit.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.zGuidFindBox2.SuspendLayout();
			this.WE_OPGuidFindBox.SuspendLayout();
			this.CommodityCodeCodeFindBox.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.HeldCodeToChangeToDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsDocketLine);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|1ec91311-7a79-4f43-b1fd-cee30e07b55d", "Customs Related Data");
			this.zGroupBox1.Controls.Add(this.zDateEdit2);
			this.zGroupBox1.Controls.Add(this.zTextBox12);
			this.zGroupBox1.Controls.Add(this.zTextBox11);
			this.zGroupBox1.Controls.Add(this.zLabelAddInfo);
			this.zGroupBox1.Controls.Add(this.zGridBondedAdditionalInfo);
			this.zGroupBox1.Controls.Add(this.ManufacturerAddressControl);
			this.zGroupBox1.Controls.Add(this.zTextBox9);
			this.zGroupBox1.Controls.Add(this.zCalcEdit7);
			this.zGroupBox1.Controls.Add(this.zTextBox8);
			this.zGroupBox1.Controls.Add(this.zTextBox7);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Controls.Add(this.zCalcEdit6);
			this.zGroupBox1.Controls.Add(this.zCalcEdit4);
			this.zGroupBox1.Controls.Add(this.zCalcEdit3);
			this.zGroupBox1.Controls.Add(this.zCalcEdit2);
			this.zGroupBox1.Controls.Add(this.zTextBox5);
			this.zGroupBox1.Controls.Add(this.zCalcEdit1);
			this.zGroupBox1.Controls.Add(this.zTextBox4);
			this.zGroupBox1.Controls.Add(this.zTextBox3);
			this.zGroupBox1.Controls.Add(this.zDateEdit1);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.AllocationKeyTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 371, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "CustomsData+WB_CustomsDeadline");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).CustomsData.WB_CustomsDeadline)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("97de268f-8d2b-4fbb-9cda-73f2ac8137aa", "Customs Deadline");
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 114, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 4;
			// 
			// zTextBox12
			// 
			this.BindingSource.SetBindingMember(this.zTextBox12, "CustomsData+WB_InwardProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_InwardProcedure)));
			this.zTextBox12.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("8d6eeb44-6eb5-4728-b4fa-cbf31e562778", "Inward Procedure");
			this.zTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 160, true);
			this.zTextBox12.Name = "zTextBox12";
			this.zTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox12.TabIndex = 6;
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "CustomsData+WB_InwardStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_InwardStyle)));
			this.zTextBox11.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f38c7095-4fa4-4045-9574-efcd0eba0e23", "Inward Style");
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 137, true);
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox11.TabIndex = 5;
			// 
			// zLabelAddInfo
			// 
			this.zLabelAddInfo.AutoSize = true;
			this.zLabelAddInfo.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|fdb4e7a1-46ec-452c-a325-ff9a81259be5", "Additional Info");
			this.zLabelAddInfo.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelAddInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 301, true);
			this.zLabelAddInfo.Name = "zLabelAddInfo";
			this.zLabelAddInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.zLabelAddInfo.TabIndex = 21;
			// 
			// zGridBondedAdditionalInfo
			// 
			this.zGridBondedAdditionalInfo.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridBondedAdditionalInfo, "CustomsData+BondedAdditionalInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsDocketLine)(null)).CustomsData.BondedAdditionalInfo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsBWAAddInfo)(((System.Collections.IList)(((WhsDocketLine)(null)).CustomsData.BondedAdditionalInfo)).SyncRoot)).KeyString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsBWAAddInfo)(((System.Collections.IList)(((WhsDocketLine)(null)).CustomsData.BondedAdditionalInfo)).SyncRoot)).ValueString)));
			this.zGridBondedAdditionalInfo.CaptionText = "Additional Info";
			this.zGridBondedAdditionalInfo.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("2b949b37-7ab1-4ff1-9eea-a4a97d07e057", "Key");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "KeyString";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ee1f566e-c9ae-4815-bbd4-24bb0c756400", "Value");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ValueString";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.zGridBondedAdditionalInfo.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridBondedAdditionalInfo.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGridBondedAdditionalInfo.CopyColumnCaptionsToBoundFields = false;
			this.zGridBondedAdditionalInfo.GridId = "4601c11a-288f-4cba-b6d2-61ae888b1a45";
			this.zGridBondedAdditionalInfo.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridBondedAdditionalInfo.LayoutKey = "zGridBondedAdditionalInfo";
			this.zGridBondedAdditionalInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 271, true);
			this.zGridBondedAdditionalInfo.Name = "zGridBondedAdditionalInfo";
			this.zGridBondedAdditionalInfo.ReadOnly = true;
			this.zGridBondedAdditionalInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 96, true);
			this.zGridBondedAdditionalInfo.TabIndex = 20;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "CustomsData+WB_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsDocketLine)(null)).CustomsData.WB_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.BindToOrgList = "CustomsData.Lookups.Manufacturers";
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|444732a8-8c36-47f1-be29-ce3abcaf4f97", "Manufacturer");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 207, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowOrganisationName = true;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 58, true);
			this.ManufacturerAddressControl.TabIndex = 19;
			// 
			// zTextBox9
			// 
			this.BindingSource.SetBindingMember(this.zTextBox9, "CustomsData+WB_CustomsThirdUnitQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_CustomsThirdUnitQty)));
			this.zTextBox9.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox9, false);
			this.zTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 66, true);
			this.zTextBox9.Name = "zTextBox9";
			this.zTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
			this.zTextBox9.TabIndex = 13;
			// 
			// zCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "CustomsData+WB_CustomsThirdQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_CustomsThirdQuantity)));
			this.zCalcEdit7.CaptionResourceString = null;
			this.zCalcEdit7.DecimalPlaces = 4;
			this.zCalcEdit7.Decimals = 4;
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 66, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit7.TabIndex = 12;
			this.zCalcEdit7.Text = "0.0000";
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "CustomsData+WB_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_Tariff)));
			this.zTextBox8.CaptionResourceString = null;
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 137, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.zTextBox8.TabIndex = 16;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "CustomsData+WB_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_PrimaryPreference)));
			this.zTextBox7.CaptionResourceString = null;
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 161, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox7.TabIndex = 17;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "CustomsData+WB_CustomsSecondUnitQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_CustomsSecondUnitQty)));
			this.zTextBox2.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 42, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
			this.zTextBox2.TabIndex = 11;
			// 
			// zCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "CustomsData+WB_CustomsSecondQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_CustomsSecondQuantity)));
			this.zCalcEdit6.CaptionResourceString = null;
			this.zCalcEdit6.DecimalPlaces = 4;
			this.zCalcEdit6.Decimals = 4;
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 42, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit6.TabIndex = 10;
			this.zCalcEdit6.Text = "0.0000";
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "CustomsData+WB_EntryLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_EntryLineNo)));
			this.zCalcEdit4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsBondedWarehouseTransactionLine|EntryLineNo|Caption", "Entry Line No");
			this.zCalcEdit4.DecimalPlaces = 2;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 42, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEdit4.TabIndex = 1;
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "CustomsData+WB_TILV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_TILV)));
			this.zCalcEdit3.CaptionResourceString = null;
			this.zCalcEdit3.DecimalPlaces = 4;
			this.zCalcEdit3.Decimals = 4;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 114, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit3.TabIndex = 15;
			this.zCalcEdit3.Text = "0.0000";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "CustomsData+WB_ValueForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_ValueForDuty)));
			this.zCalcEdit2.CaptionResourceString = null;
			this.zCalcEdit2.DecimalPlaces = 4;
			this.zCalcEdit2.Decimals = 4;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 90, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit2.TabIndex = 14;
			this.zCalcEdit2.Text = "0.0000";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "CustomsData+WB_CustomsUnitOfQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_CustomsUnitOfQty)));
			this.zTextBox5.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox5, false);
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 18, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
			this.zTextBox5.TabIndex = 9;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "CustomsData+WB_CustomsQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CustomsData.WB_CustomsQty)));
			this.zCalcEdit1.CaptionResourceString = null;
			this.zCalcEdit1.DecimalPlaces = 4;
			this.zCalcEdit1.Decimals = 4;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 18, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit1.TabIndex = 8;
			this.zCalcEdit1.Text = "0.0000";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "CustomsData+WB_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_RN_NKCountryOfOrigin)));
			this.zTextBox4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|261a6e1a-12e8-4fc7-bd86-e63a808b7707", "Org. Ctry/Rgn.", "Country/Region of Origin", "Country/Region of Origin of the Goods as declared.");
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 183, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox4.TabIndex = 7;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "CustomsData+WB_DeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_DeclarationReference)));
			this.zTextBox3.CaptionResourceString = null;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 90, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox3.TabIndex = 3;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "CustomsData+WB_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).CustomsData.WB_EntryDate)));
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 66, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 2;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CustomsData+WB_EntryKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CustomsData.WB_EntryKey)));
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsBondedWarehouseTransactionLine|EntryNo|Caption", "Entry No");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 18, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// AllocationKeyTextBox
			// 
			this.BindingSource.SetBindingMember(this.AllocationKeyTextBox, "WE_AllocationKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_AllocationKey)));
			this.AllocationKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 185, true);
			this.AllocationKeyTextBox.Name = "AllocationKeyTextBox";
			this.AllocationKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 15, true);
			this.AllocationKeyTextBox.TabIndex = 18;
			// 
			// AttributesGroupBox
			// 
			this.AttributesGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|7804179e-9595-446e-be89-c3095e1a9fda", "Attributes");
			this.AttributesGroupBox.Controls.Add(this.WE_SerialNumberTextBox);
			this.AttributesGroupBox.Controls.Add(this.WE_BondedEntryKeyTextBox);
			this.AttributesGroupBox.Controls.Add(this.WE_PartAttrib3TextBox);
			this.AttributesGroupBox.Controls.Add(this.WE_PartAttrib2TextBox);
			this.AttributesGroupBox.Controls.Add(this.WE_ExpiryDateEdit);
			this.AttributesGroupBox.Controls.Add(this.WE_PackingDateEdit);
			this.AttributesGroupBox.Controls.Add(this.WE_PartAttrib1TextBox);
			this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 371, true);
			this.AttributesGroupBox.Name = "AttributesGroupBox";
			this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 122, true);
			this.AttributesGroupBox.TabIndex = 0;
			this.AttributesGroupBox.TabStop = false;
			// 
			// WE_SerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WE_SerialNumberTextBox, "WE_SerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_SerialNumber)));
			this.WE_SerialNumberTextBox.CaptionResourceString = null;
			this.WE_SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 86, true);
			this.WE_SerialNumberTextBox.Name = "WE_SerialNumberTextBox";
			this.WE_SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.WE_SerialNumberTextBox.TabIndex = 6;
			// 
			// WE_BondedEntryKeyTextBox
			// 
			this.BindingSource.SetBindingMember(this.WE_BondedEntryKeyTextBox, "WE_BondedEntryKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_BondedEntryKey)));
			this.WE_BondedEntryKeyTextBox.CaptionResourceString = null;
			this.WE_BondedEntryKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 62, true);
			this.WE_BondedEntryKeyTextBox.Name = "WE_BondedEntryKeyTextBox";
			this.WE_BondedEntryKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.WE_BondedEntryKeyTextBox.TabIndex = 2;
			// 
			// WE_PartAttrib3TextBox
			// 
			this.BindingSource.SetBindingMember(this.WE_PartAttrib3TextBox, "WE_PartAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_PartAttrib3)));
			this.WE_PartAttrib3TextBox.CaptionResourceString = null;
			this.WE_PartAttrib3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 62, true);
			this.WE_PartAttrib3TextBox.Name = "WE_PartAttrib3TextBox";
			this.WE_PartAttrib3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.WE_PartAttrib3TextBox.TabIndex = 5;
			// 
			// WE_PartAttrib2TextBox
			// 
			this.BindingSource.SetBindingMember(this.WE_PartAttrib2TextBox, "WE_PartAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_PartAttrib2)));
			this.WE_PartAttrib2TextBox.CaptionResourceString = null;
			this.WE_PartAttrib2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 38, true);
			this.WE_PartAttrib2TextBox.Name = "WE_PartAttrib2TextBox";
			this.WE_PartAttrib2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.WE_PartAttrib2TextBox.TabIndex = 4;
			// 
			// WE_ExpiryDateEdit
			// 
			this.WE_ExpiryDateEdit.AllowDrop = true;
			this.WE_ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.WE_ExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.WE_ExpiryDateEdit, "WE_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).WE_ExpiryDate)));
			this.WE_ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 14, true);
			this.WE_ExpiryDateEdit.Name = "WE_ExpiryDateEdit";
			this.WE_ExpiryDateEdit.TabIndex = 0;
			// 
			// WE_PackingDateEdit
			// 
			this.WE_PackingDateEdit.AllowDrop = true;
			this.WE_PackingDateEdit.AutoCompleteMonthThreshold = 1;
			this.WE_PackingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.WE_PackingDateEdit, "WE_PackingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).WE_PackingDate)));
			this.WE_PackingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 38, true);
			this.WE_PackingDateEdit.Name = "WE_PackingDateEdit";
			this.WE_PackingDateEdit.TabIndex = 1;
			// 
			// WE_PartAttrib1TextBox
			// 
			this.BindingSource.SetBindingMember(this.WE_PartAttrib1TextBox, "WE_PartAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_PartAttrib1)));
			this.WE_PartAttrib1TextBox.CaptionResourceString = null;
			this.WE_PartAttrib1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 14, true);
			this.WE_PartAttrib1TextBox.Name = "WE_PartAttrib1TextBox";
			this.WE_PartAttrib1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.WE_PartAttrib1TextBox.TabIndex = 3;
			// 
			// WI_ArrivalDateEdit
			// 
			this.WI_ArrivalDateEdit.AllowDrop = true;
			this.WI_ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.WI_ArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.WI_ArrivalDateEdit, "WE_AdjustmentArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).WE_AdjustmentArrivalDate)));
			this.WI_ArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.WI_ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 114, true);
			this.WI_ArrivalDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.WI_ArrivalDateEdit.Name = "WI_ArrivalDateEdit";
			this.WI_ArrivalDateEdit.TabIndex = 4;
			// 
			// HeldCodeDropEdit
			// 
			this.HeldCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeldCodeDropEdit, "WE_WHC_NKCurrentInventoryHeldCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).WE_WHC_NKCurrentInventoryHeldCode)));
			this.HeldCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 161, true);
			this.HeldCodeDropEdit.Name = "HeldCodeDropEdit";
			this.HeldCodeDropEdit.PreBoundMaxLength = 8;
			this.HeldCodeDropEdit.ShouldResizeByMaxLength = true;
			this.HeldCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.HeldCodeDropEdit.TabIndex = 6;
			// 
			// LocationStringTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationStringTextBox, "CurrentLocationString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CurrentLocationString)));
			this.LocationStringTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|6e2a0870-0cf9-4616-834e-6a76c3c14a24", "Location");
			this.LocationStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 233, true);
			this.LocationStringTextBox.Name = "LocationStringTextBox";
			this.LocationStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.LocationStringTextBox.TabIndex = 10;
			// 
			// AvailableToPickQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AvailableToPickQuantityCalcEdit, "AvailableToPickQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).AvailableToPickQuantity)));
			this.AvailableToPickQuantityCalcEdit.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.AvailableToPickQuantityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|19181942-afdb-45fb-8aaa-0c726cc008da", "Available Pick Quantity");
			this.AvailableToPickQuantityCalcEdit.DecimalPlaces = 2;
			this.AvailableToPickQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 401, true);
			this.AvailableToPickQuantityCalcEdit.Name = "AvailableToPickQuantityCalcEdit";
			this.AvailableToPickQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AvailableToPickQuantityCalcEdit.TabIndex = 24;
			this.AvailableToPickQuantityCalcEdit.Text = "0.00";
			this.AvailableToPickQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WI_TotalUnitsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WI_TotalUnitsCalcEdit, "WE_StockOnHand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).WE_StockOnHand)));
			this.WI_TotalUnitsCalcEdit.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.WI_TotalUnitsCalcEdit.CaptionResourceString = null;
			this.WI_TotalUnitsCalcEdit.DecimalPlaces = 2;
			this.WI_TotalUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 329, true);
			this.WI_TotalUnitsCalcEdit.Name = "WI_TotalUnitsCalcEdit";
			this.WI_TotalUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.WI_TotalUnitsCalcEdit.TabIndex = 16;
			this.WI_TotalUnitsCalcEdit.Text = "0.00";
			this.WI_TotalUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommittedQuantityIncludingUnfinalisedReceiptCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit, "CommittedQuantityIncludingUnfinalisedReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).CommittedQuantityIncludingUnfinalisedReceipt)));
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|194a6ff8-62ee-4c2d-8746-97b20b37bc2a", "Committed Quantity");
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.DecimalPlaces = 2;
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 377, true);
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.Name = "CommittedQuantityIncludingUnfinalisedReceiptCalcEdit";
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.TabIndex = 21;
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.Text = "0.00";
			this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalUnitsUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalUnitsUQTextBox, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.TotalUnitsUQTextBox.CaptionResourceString = null;
			this.TotalUnitsUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 329, true);
			this.TotalUnitsUQTextBox.Name = "TotalUnitsUQTextBox";
			this.TotalUnitsUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.TotalUnitsUQTextBox.TabIndex = 17;
			// 
			// CommittedUnitsUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommittedUnitsUQTextBox, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.CommittedUnitsUQTextBox.CaptionResourceString = null;
			this.CommittedUnitsUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 377, true);
			this.CommittedUnitsUQTextBox.Name = "CommittedUnitsUQTextBox";
			this.CommittedUnitsUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.CommittedUnitsUQTextBox.TabIndex = 22;
			// 
			// AvailableUnitsUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.AvailableUnitsUQTextBox, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.AvailableUnitsUQTextBox.CaptionResourceString = null;
			this.AvailableUnitsUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 401, true);
			this.AvailableUnitsUQTextBox.Name = "AvailableUnitsUQTextBox";
			this.AvailableUnitsUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.AvailableUnitsUQTextBox.TabIndex = 25;
			// 
			// LocationPickAreaNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationPickAreaNameTextBox, "CurrentLocationPickAreaName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CurrentLocationPickAreaName)));
			this.LocationPickAreaNameTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|f738fa2d-6750-4e4f-b125-42b67b2d8eb0", "Pick Area / Type");
			this.LocationPickAreaNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 257, true);
			this.LocationPickAreaNameTextBox.Name = "LocationPickAreaNameTextBox";
			this.LocationPickAreaNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.LocationPickAreaNameTextBox.TabIndex = 11;
			// 
			// LocationPickAreaTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationPickAreaTypeTextBox, "CurrentLocationPickAreaType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CurrentLocationPickAreaType)));
			this.LocationPickAreaTypeTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|f9c2a3c4-0ada-4b11-9518-050f8d027ce2", "Pick Type");
			this.LocationPickAreaTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 257, true);
			this.LocationPickAreaTypeTextBox.Name = "LocationPickAreaTypeTextBox";
			this.LocationPickAreaTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.LocationPickAreaTypeTextBox.TabIndex = 12;
			// 
			// ViewStatusButton
			// 
			this.ViewStatusButton.IsCaptionOverridden = true;
			this.ViewStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 135, true);
			this.ViewStatusButton.Name = "ViewStatusButton";
			this.ViewStatusButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 24, true);
			this.ViewStatusButton.TabIndex = 29;
			this.ViewStatusButton.Text = "...";
			this.ViewStatusButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ViewStatusButton.ToolTipCaption = null;
			this.ViewStatusButton.UseVisualStyleBackColor = true;
			this.ViewStatusButton.Click += new EventHandler(this.ViewStatusMenuItem_Click);
			//
			// ViewReceiptButton
			// 
			this.ViewReceiptButton.IsCaptionOverridden = true;
			this.ViewReceiptButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 207, true);
			this.ViewReceiptButton.Name = "ViewReceiptButton";
			this.ViewReceiptButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewReceiptButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 24, true);
			this.ViewReceiptButton.TabIndex = 9;
			this.ViewReceiptButton.Text = "...";
			this.ViewReceiptButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ViewReceiptButton.ToolTipCaption = null;
			this.ViewReceiptButton.UseVisualStyleBackColor = true;
			this.ViewReceiptButton.Click += new EventHandler(this.ViewReceiptMenuItem_Click);
			// 
			// CommittedButton
			// 
			this.CommittedButton.IsCaptionOverridden = true;
			this.CommittedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 375, true);
			this.CommittedButton.Name = "CommittedButton";
			this.CommittedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CommittedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 24, true);
			this.CommittedButton.TabIndex = 23;
			this.CommittedButton.Text = "...";
			this.CommittedButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CommittedButton.ToolTipCaption = null;
			this.CommittedButton.UseVisualStyleBackColor = true;
			this.CommittedButton.Click += new EventHandler(this.CommittedStockMenuItem_Click);
			// 
			// ReceiptReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptReferenceTextBox, "ReceiptReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ReceiptReference)));
			this.ReceiptReferenceTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|94cc4bf9-7881-4539-8084-b59337835462", "Receipt Reference");
			this.ReceiptReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 209, true);
			this.ReceiptReferenceTextBox.Name = "ReceiptReferenceTextBox";
			this.ReceiptReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.ReceiptReferenceTextBox.TabIndex = 8;
			// 
			// WI_InDocketLineUnitsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WI_InDocketLineUnitsCalcEdit, "WE_TransactionQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).WE_TransactionQuantity)));
			this.WI_InDocketLineUnitsCalcEdit.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.WI_InDocketLineUnitsCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|f040fc60-a48e-43ea-af28-5af6cddff9dd", "Receipt Quantity");
			this.WI_InDocketLineUnitsCalcEdit.DecimalPlaces = 2;
			this.WI_InDocketLineUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 305, true);
			this.WI_InDocketLineUnitsCalcEdit.Name = "WI_InDocketLineUnitsCalcEdit";
			this.WI_InDocketLineUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.WI_InDocketLineUnitsCalcEdit.TabIndex = 14;
			this.WI_InDocketLineUnitsCalcEdit.Text = "0.00";
			this.WI_InDocketLineUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReceiptUnitsUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptUnitsUQTextBox, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.ReceiptUnitsUQTextBox.CaptionResourceString = null;
			this.ReceiptUnitsUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 305, true);
			this.ReceiptUnitsUQTextBox.Name = "ReceiptUnitsUQTextBox";
			this.ReceiptUnitsUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.ReceiptUnitsUQTextBox.TabIndex = 15;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WarehousePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsDocketLine)(null)).WarehousePK)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 18, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PopupCaption = null;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.zGuidFindBox1.TabIndex = 0;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "ClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsDocketLine)(null)).ClientPK)));
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 42, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.PopupCaption = null;
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.zGuidFindBox2.TabIndex = 1;
			// 
			// WE_OPGuidFindBox
			// 
			this.WE_OPGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WE_OPGuidFindBox, "WE_OP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsDocketLine)(null)).WE_OP)));
			this.WE_OPGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|44d65755-f198-43bd-bbb8-8bdd58414e95", "Product");
			this.WE_OPGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 66, true);
			this.WE_OPGuidFindBox.Name = "WE_OPGuidFindBox";
			this.WE_OPGuidFindBox.PopupCaption = null;
			this.WE_OPGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.WE_OPGuidFindBox.TabIndex = 2;
			// 
			// CommodityCodeCodeFindBox
			// 
			this.CommodityCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeCodeFindBox, "CommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).CommodityCode)));
			this.CommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 90, true);
			this.CommodityCodeCodeFindBox.Name = "CommodityCodeCodeFindBox";
			this.CommodityCodeCodeFindBox.PopupCaption = null;
			this.CommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.CommodityCodeCodeFindBox.TabIndex = 3;
			// 
			// AllocatedToPickButton
			// 
			this.AllocatedToPickButton.IsCaptionOverridden = true;
			this.AllocatedToPickButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 495, true);
			this.AllocatedToPickButton.Name = "AllocatedToPickButton";
			this.AllocatedToPickButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocatedToPickButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 24, true);
			this.AllocatedToPickButton.TabIndex = 33;
			this.AllocatedToPickButton.Text = "...";
			this.AllocatedToPickButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AllocatedToPickButton.ToolTipCaption = null;
			this.AllocatedToPickButton.UseVisualStyleBackColor = true;
			this.AllocatedToPickButton.Click += new EventHandler(this.AllocatedToPickButton_Click);
			// 
			// PickAllocationsAsString
			// 
			this.BindingSource.SetBindingMember(this.PickAllocationsAsString, "PickAllocationsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).PickAllocationsAsString)));
			this.PickAllocationsAsString.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|44e5bb31-bcb2-48f1-862e-23941572d4c5", "Pick Allocation");
			this.PickAllocationsAsString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 497, true);
			this.PickAllocationsAsString.Name = "PickAllocationsAsString";
			this.PickAllocationsAsString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PickAllocationsAsString.TabIndex = 32;
			// 
			// zCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit5, "ReservedQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).ReservedQuantity)));
			this.zCalcEdit5.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.zCalcEdit5.CaptionResourceString = null;
			this.zCalcEdit5.DecimalPlaces = 2;
			this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 353, true);
			this.zCalcEdit5.Name = "zCalcEdit5";
			this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit5.TabIndex = 18;
			this.zCalcEdit5.Text = "0.00";
			this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.zTextBox10.CaptionResourceString = null;
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 353, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.zTextBox10.TabIndex = 19;
			// 
			// CrossDockButton
			// 
			this.CrossDockButton.IsCaptionOverridden = true;
			this.CrossDockButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 351, true);
			this.CrossDockButton.Name = "CrossDockButton";
			this.CrossDockButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CrossDockButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 24, true);
			this.CrossDockButton.TabIndex = 20;
			this.CrossDockButton.Text = "...";
			this.CrossDockButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CrossDockButton.ToolTipCaption = null;
			this.CrossDockButton.UseVisualStyleBackColor = true;
			this.CrossDockButton.Click += new EventHandler(this.CrossDockButton_Click);
			// 
			// WI_PalletIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.WI_PalletIDTextBox, "WE_PalletID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_PalletID)));
			this.WI_PalletIDTextBox.CaptionResourceString = null;
			this.WI_PalletIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 281, true);
			this.WI_PalletIDTextBox.Name = "WI_PalletIDTextBox";
			this.WI_PalletIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.WI_PalletIDTextBox.TabIndex = 13;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryUserControl|725118b3-59a9-4fad-9487-2027539a3b7b", "Details");
			this.MainGroupBox.Controls.Add(this.AvailableToTransferUnitsUQTextBox);
			this.MainGroupBox.Controls.Add(this.AvailableToTransferQuantityCalcEdit);
			this.MainGroupBox.Controls.Add(this.HoldReasonToChangeToTextBox);
			this.MainGroupBox.Controls.Add(this.HoldCodeReasonTextBox);
			this.MainGroupBox.Controls.Add(this.HeldCodeToChangeToDropEdit);
			this.MainGroupBox.Controls.Add(this.HeldCodeChangeQuantity);
			this.MainGroupBox.Controls.Add(this.StatusDropEdit);
			this.MainGroupBox.Controls.Add(this.WI_PalletIDTextBox);
			this.MainGroupBox.Controls.Add(this.CrossDockButton);
			this.MainGroupBox.Controls.Add(this.zTextBox10);
			this.MainGroupBox.Controls.Add(this.zCalcEdit5);
			this.MainGroupBox.Controls.Add(this.PickAllocationsAsString);
			this.MainGroupBox.Controls.Add(this.AllocatedToPickButton);
			this.MainGroupBox.Controls.Add(this.CommodityCodeCodeFindBox);
			this.MainGroupBox.Controls.Add(this.WE_OPGuidFindBox);
			this.MainGroupBox.Controls.Add(this.zGuidFindBox2);
			this.MainGroupBox.Controls.Add(this.zGuidFindBox1);
			this.MainGroupBox.Controls.Add(this.ReceiptUnitsUQTextBox);
			this.MainGroupBox.Controls.Add(this.WI_InDocketLineUnitsCalcEdit);
			this.MainGroupBox.Controls.Add(this.ReceiptReferenceTextBox);
			this.MainGroupBox.Controls.Add(this.CommittedButton);
			this.MainGroupBox.Controls.Add(this.ViewStatusButton);
			this.MainGroupBox.Controls.Add(this.ViewReceiptButton);
			this.MainGroupBox.Controls.Add(this.LocationPickAreaTypeTextBox);
			this.MainGroupBox.Controls.Add(this.LocationPickAreaNameTextBox);
			this.MainGroupBox.Controls.Add(this.AvailableUnitsUQTextBox);
			this.MainGroupBox.Controls.Add(this.CommittedUnitsUQTextBox);
			this.MainGroupBox.Controls.Add(this.TotalUnitsUQTextBox);
			this.MainGroupBox.Controls.Add(this.CommittedQuantityIncludingUnfinalisedReceiptCalcEdit);
			this.MainGroupBox.Controls.Add(this.WI_TotalUnitsCalcEdit);
			this.MainGroupBox.Controls.Add(this.AvailableToPickQuantityCalcEdit);
			this.MainGroupBox.Controls.Add(this.LocationStringTextBox);
			this.MainGroupBox.Controls.Add(this.HeldCodeDropEdit);
			this.MainGroupBox.Controls.Add(this.WI_ArrivalDateEdit);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 525, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// HoldReasonToChangeToTextBox
			// 
			this.BindingSource.SetBindingMember(this.HoldReasonToChangeToTextBox, "HoldReasonToChangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).HoldReasonToChangeTo)));
			this.HoldReasonToChangeToTextBox.CaptionResourceString = null;
			this.HoldReasonToChangeToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HoldReasonToChangeToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 473, true);
			this.HoldReasonToChangeToTextBox.Name = "HoldReasonToChangeToTextBox";
			this.HoldReasonToChangeToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.HoldReasonToChangeToTextBox.TabIndex = 30;
			// 
			// HoldCodeReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.HoldCodeReasonTextBox, "WE_CurrentHoldReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).WE_CurrentHoldReason)));
			this.HoldCodeReasonTextBox.CaptionResourceString = null;
			this.HoldCodeReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HoldCodeReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 185, true);
			this.HoldCodeReasonTextBox.Name = "HoldCodeReasonTextBox";
			this.HoldCodeReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.HoldCodeReasonTextBox.TabIndex = 7;
			// 
			// HeldCodeToChangeToDropEdit
			// 
			this.HeldCodeToChangeToDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeldCodeToChangeToDropEdit, "HeldCodeToChangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).HeldCodeToChangeTo)));
			this.HeldCodeToChangeToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 449, true);
			this.HeldCodeToChangeToDropEdit.Name = "HeldCodeToChangeToDropEdit";
			this.HeldCodeToChangeToDropEdit.PreBoundMaxLength = 3;
			this.HeldCodeToChangeToDropEdit.ShouldResizeByMaxLength = true;
			this.HeldCodeToChangeToDropEdit.ShowDescriptionBox = false;
			this.HeldCodeToChangeToDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.HeldCodeToChangeToDropEdit.TabIndex = 31;
			// 
			// HeldCodeChangeQuantity
			// 
			this.BindingSource.SetBindingMember(this.HeldCodeChangeQuantity, "HeldCodeChangeQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).HeldCodeChangeQuantity)));
			this.HeldCodeChangeQuantity.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.HeldCodeChangeQuantity.CaptionResourceString = null;
			this.HeldCodeChangeQuantity.DecimalPlaces = 2;
			this.HeldCodeChangeQuantity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 449, true);
			this.HeldCodeChangeQuantity.Name = "HeldCodeChangeQuantity";
			this.HeldCodeChangeQuantity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.HeldCodeChangeQuantity.TabIndex = 28;
			this.HeldCodeChangeQuantity.Text = "0.00";
			this.HeldCodeChangeQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "WE_CurrentInventoryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsDocketLine)(null)).WE_CurrentInventoryStatus)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("50506cfc-8ea0-43da-87fd-57156909e46c", "Inventory Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 137, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 8;
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.StatusDropEdit.TabIndex = 5;
			// 
			// AvailableToTransferUnitsUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.AvailableToTransferUnitsUQTextBox, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsDocketLine)(null)).ProductUQ)));
			this.AvailableToTransferUnitsUQTextBox.CaptionResourceString = null;
			this.AvailableToTransferUnitsUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 425, true);
			this.AvailableToTransferUnitsUQTextBox.Name = "AvailableToTransferUnitsUQTextBox";
			this.AvailableToTransferUnitsUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AvailableToTransferUnitsUQTextBox.TabIndex = 27;
			// 
			// AvailableToTransferQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AvailableToTransferQuantityCalcEdit, "AvailableToTransferQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsDocketLine)(null)).AvailableToTransferQuantity)));
			this.AvailableToTransferQuantityCalcEdit.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.AvailableToTransferQuantityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("a18b2aef-134e-4838-b948-879fd1d5dcda", "Available Transfer Qty", "Available To Transfer Quantity");
			this.AvailableToTransferQuantityCalcEdit.DecimalPlaces = 2;
			this.AvailableToTransferQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 425, true);
			this.AvailableToTransferQuantityCalcEdit.Name = "AvailableToTransferQuantityCalcEdit";
			this.AvailableToTransferQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.AvailableToTransferQuantityCalcEdit.TabIndex = 26;
			this.AvailableToTransferQuantityCalcEdit.Text = "0.00";
			this.AvailableToTransferQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InventoryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.AttributesGroupBox);
			this.Controls.Add(this.MainGroupBox);
			this.Name = "InventoryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridBondedAdditionalInfo)).EndInit();
			this.zGridBondedAdditionalInfo.ResumeLayout(false);
			this.zGridBondedAdditionalInfo.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.AttributesGroupBox.ResumeLayout(false);
			this.AttributesGroupBox.PerformLayout();
			this.WE_ExpiryDateEdit.ResumeLayout(true);
			this.WE_ExpiryDateEdit.PerformLayout();
			this.WE_PackingDateEdit.ResumeLayout(true);
			this.WE_PackingDateEdit.PerformLayout();
			this.WI_ArrivalDateEdit.ResumeLayout(true);
			this.WI_ArrivalDateEdit.PerformLayout();
			this.HeldCodeDropEdit.ResumeLayout(true);
			this.HeldCodeDropEdit.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.zGuidFindBox2.ResumeLayout(true);
			this.zGuidFindBox2.PerformLayout();
			this.WE_OPGuidFindBox.ResumeLayout(true);
			this.WE_OPGuidFindBox.PerformLayout();
			this.CommodityCodeCodeFindBox.ResumeLayout(true);
			this.CommodityCodeCodeFindBox.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.HeldCodeToChangeToDropEdit.ResumeLayout(true);
			this.HeldCodeToChangeToDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
