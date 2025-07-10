using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickOrdersUserControl
	{
		ZGroupBox OrdersGroupBox;
		ZArchitecture.ZCalcEdit numberOfOrders;
		ZArchitecture.ZCalcEdit totalQty;
		ZArchitecture.ZCalcEdit totalWeight;
		ZArchitecture.ZTextBox totalWeightUnit;
		ZArchitecture.ZCalcEdit totalVolume;
		ZArchitecture.ZTextBox totalVolumeUnit;
		ZArchitecture.ZLabel orderCountLabel;
		ZArchitecture.ZLabel totalQtyLabel;
		ZArchitecture.ZLabel totalWeightLabel;
		ZArchitecture.ZLabel totalVolumeLabel;
		ZArchitecture.ZLabel totalComponentLabel;
		ZArchitecture.ZCalcEdit totalComponentQty;
		public PickOrdersModuleButtonGrid PickOrdersModuleButtonGrid;

		void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new ZMultiControlColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PickOrdersModuleButtonGrid = new PickOrdersModuleButtonGrid();
			this.OrdersGroupBox = new ZGroupBox();
			this.totalComponentLabel = new ZArchitecture.ZLabel();
			this.totalComponentQty = new ZArchitecture.ZCalcEdit();
			this.totalVolumeLabel = new ZArchitecture.ZLabel();
			this.totalWeightLabel = new ZArchitecture.ZLabel();
			this.totalQtyLabel = new ZArchitecture.ZLabel();
			this.orderCountLabel = new ZArchitecture.ZLabel();
			this.totalVolumeUnit = new ZArchitecture.ZTextBox();
			this.totalVolume = new ZArchitecture.ZCalcEdit();
			this.totalWeightUnit = new ZArchitecture.ZTextBox();
			this.totalWeight = new ZArchitecture.ZCalcEdit();
			this.totalQty = new ZArchitecture.ZCalcEdit();
			this.numberOfOrders = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PickOrdersModuleButtonGrid.InnerGrid)).BeginInit();
			this.PickOrdersModuleButtonGrid.SuspendLayout();
			this.OrdersGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPick);
			// 
			// PickOrdersModuleButtonGrid
			// 
			this.PickOrdersModuleButtonGrid.AllowDrop = true;
			this.PickOrdersModuleButtonGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickOrdersModuleButtonGrid, "Orders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsPick)(null)).Orders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsPick)(null)).Lookups.OrderFindBoxList)));
			this.PickOrdersModuleButtonGrid.BindToFindBoxList = "Lookups+OrderFindBoxList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WD_WW_Whs";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|38785fe7-9f23-4040-9657-64930913a5b9", "Client");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WD_OH_Client";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WD_ExternalReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "WD_RS_NKServiceLevel";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|81d80ccb-db46-4f67-89ca-009dfa9ad9f0", "Consignee");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsigneeFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("438e9633-8201-450a-8f63-18381091f2f5", "Cne. Ctry/Rgn.", "Cne. Country/Region", "Consignee Country/Region", "");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ConsigneeDocAddress+E2_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WD_DocketID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "WD_PickOption";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|67886490-8455-40b0-b872-62519123b303", "Status");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "WD_DocketStatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|4b4a9bec-97b5-4d73-bf27-46ae6d483fd6", "Sub Type");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "SubTypeDesc";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "RequiredDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|05b461c6-0283-4ce6-93df-b7bc83a0b214", "Finalized Date");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WD_FinalisedDate";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|1e355bde-79ee-45be-bf3c-7e0c80fd3f29", "Transport Co");
			zMultiControlColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo2.ColumnName = "TransportCoNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "TransportCoFieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "WD_TransportReference";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "WD_PL_NKCarrierServiceLevel";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "WD_WhsOrderFulfillmentRule";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("30c885f7-4bf7-4ab3-9a5a-5280eb877458", "Pick Priority");
			zCalcEditColumnStyleInfo1.ColumnName = "WD_PickPriority";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WD_TotalUnitsFromLines";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "WD_TotalWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("1e1a40ef-1a9d-41d2-bfb7-a772e12b64a7", "Total Weight");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "WD_TotalWeightUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("1e1a40ef-1a9d-41d2-bfb7-a772e12b64a7", "Total Weight");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "WD_TotalCubic";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("8fa75bcb-4734-479c-9ef2-cadc1b87cd24", "Total Volume");
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "WD_TotalCubicUnit";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("8fa75bcb-4734-479c-9ef2-cadc1b87cd24", "Total Volume");
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "WD_TotalOrderValue";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("b51af701-393e-42d6-a47c-754991fc98b9", "Total Order Value");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "WD_RX_NKTotalOrderCurrency";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("b51af701-393e-42d6-a47c-754991fc98b9", "Total Order Value");
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "HasDangerousGoods";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PickOrdersModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PickOrdersModuleButtonGrid.DetachMessage = Enterprise.Warehouse.Transactions.GUI.Res.GetData("EE686F86-12E2-44FE-808C-1AC55D509789", "Are you sure you want to detach the selected order?");
			this.PickOrdersModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.PickOrdersModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.PickOrdersModuleButtonGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PickOrdersModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.PickOrdersModuleButtonGrid.InnerGrid.GridId = null;
			this.PickOrdersModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PickOrdersModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.PickOrdersModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PickOrdersModuleButtonGrid.InnerGrid.Name = "Grid";
			this.PickOrdersModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PickOrdersModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(962, 365, true);
			this.PickOrdersModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.PickOrdersModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.PickOrdersModuleButtonGrid.Name = "PickOrdersModuleButtonGrid";
			this.PickOrdersModuleButtonGrid.ReadOnly = false;
			this.PickOrdersModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 401, true);
			this.PickOrdersModuleButtonGrid.TabIndex = 0;
			// 
			// OrdersGroupBox
			// 
			this.OrdersGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersUserControl|cacafe9a-bda6-417a-9727-7f1cefdc8c60", "Orders on this Pick");
			this.OrdersGroupBox.Controls.Add(this.totalComponentLabel);
			this.OrdersGroupBox.Controls.Add(this.totalComponentQty);
			this.OrdersGroupBox.Controls.Add(this.totalVolumeLabel);
			this.OrdersGroupBox.Controls.Add(this.totalWeightLabel);
			this.OrdersGroupBox.Controls.Add(this.totalQtyLabel);
			this.OrdersGroupBox.Controls.Add(this.orderCountLabel);
			this.OrdersGroupBox.Controls.Add(this.totalVolumeUnit);
			this.OrdersGroupBox.Controls.Add(this.totalVolume);
			this.OrdersGroupBox.Controls.Add(this.totalWeightUnit);
			this.OrdersGroupBox.Controls.Add(this.totalWeight);
			this.OrdersGroupBox.Controls.Add(this.totalQty);
			this.OrdersGroupBox.Controls.Add(this.numberOfOrders);
			this.OrdersGroupBox.Controls.Add(this.PickOrdersModuleButtonGrid);
			this.OrdersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersGroupBox.Name = "OrdersGroupBox";
			this.OrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 426, true);
			this.OrdersGroupBox.TabIndex = 2;
			this.OrdersGroupBox.TabStop = false;
			// 
			// totalComponentLabel
			// 
			this.totalComponentLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.totalComponentLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("73e7ccd5-ea6f-49a8-957f-ba9d83263eea", "Components");
			this.totalComponentLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalComponentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 386, true);
			this.totalComponentLabel.Name = "totalComponentLabel";
			this.totalComponentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.totalComponentLabel.TabIndex = 10;
			// 
			// totalComponentQty
			// 
			this.totalComponentQty.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalComponentQty, "TotalComponentQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPick)(null)).TotalComponentQty)));
			this.totalComponentQty.DecimalPlaces = 2;
			this.totalComponentQty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 403, true);
			this.totalComponentQty.Name = "totalComponentQty";
			this.totalComponentQty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.totalComponentQty.TabIndex = 3;
			this.totalComponentQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalVolumeLabel
			// 
			this.totalVolumeLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.totalVolumeLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("849fe772-3a75-459b-9e18-d952ef8329cd", "Total Volume");
			this.totalVolumeLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalVolumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 386, true);
			this.totalVolumeLabel.Name = "totalVolumeLabel";
			this.totalVolumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.totalVolumeLabel.TabIndex = 12;
			// 
			// totalWeightLabel
			// 
			this.totalWeightLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.totalWeightLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("0b7b6a2c-9235-4600-b52d-e2573518d7ef", "Total Weight");
			this.totalWeightLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 386, true);
			this.totalWeightLabel.Name = "totalWeightLabel";
			this.totalWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.totalWeightLabel.TabIndex = 11;
			// 
			// totalQtyLabel
			// 
			this.totalQtyLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.totalQtyLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("c2a3a978-5b20-4457-993d-4cb114bbfba4", "Total Qty");
			this.totalQtyLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalQtyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 386, true);
			this.totalQtyLabel.Name = "totalQtyLabel";
			this.totalQtyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.totalQtyLabel.TabIndex = 9;
			// 
			// orderCountLabel
			// 
			this.orderCountLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.orderCountLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ad268721-5bdb-4b69-aa9d-98eb60a2269c", "Order Count");
			this.orderCountLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.orderCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 386, true);
			this.orderCountLabel.Name = "orderCountLabel";
			this.orderCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.orderCountLabel.TabIndex = 8;
			// 
			// totalVolumeUnit
			// 
			this.totalVolumeUnit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalVolumeUnit, "TotalVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPick)(null)).TotalVolumeUnit)));
			this.totalVolumeUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(675, 403, true);
			this.totalVolumeUnit.Name = "totalVolumeUnit";
			this.totalVolumeUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 17, true);
			this.totalVolumeUnit.TabIndex = 7;
			// 
			// totalVolume
			// 
			this.totalVolume.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalVolume, "TotalVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPick)(null)).TotalVolume)));
			this.totalVolume.DecimalPlaces = 2;
			this.totalVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 403, true);
			this.totalVolume.Name = "totalVolume";
			this.totalVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.totalVolume.TabIndex = 6;
			this.totalVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalWeightUnit
			// 
			this.totalWeightUnit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalWeightUnit, "TotalWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPick)(null)).TotalWeightUnit)));
			this.totalWeightUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 403, true);
			this.totalWeightUnit.Name = "totalWeightUnit";
			this.totalWeightUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 17, true);
			this.totalWeightUnit.TabIndex = 5;
			// 
			// totalWeight
			// 
			this.totalWeight.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalWeight, "TotalWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPick)(null)).TotalWeight)));
			this.totalWeight.DecimalPlaces = 2;
			this.totalWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 403, true);
			this.totalWeight.Name = "totalWeight";
			this.totalWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.totalWeight.TabIndex = 4;
			this.totalWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalQty
			// 
			this.totalQty.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.totalQty, "TotalLineQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPick)(null)).TotalLineQty)));
			this.totalQty.DecimalPlaces = 2;
			this.totalQty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 403, true);
			this.totalQty.Name = "totalQty";
			this.totalQty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.totalQty.TabIndex = 2;
			this.totalQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// numberOfOrders
			// 
			this.numberOfOrders.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.numberOfOrders, "NumberOfOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPick)(null)).NumberOfOrders)));
			this.numberOfOrders.DecimalPlaces = 2;
			this.numberOfOrders.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 403, true);
			this.numberOfOrders.Name = "numberOfOrders";
			this.numberOfOrders.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.numberOfOrders.TabIndex = 1;
			this.numberOfOrders.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickOrdersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrdersGroupBox);
			this.Name = "PickOrdersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 426, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PickOrdersModuleButtonGrid.InnerGrid)).EndInit();
			this.PickOrdersModuleButtonGrid.ResumeLayout(true);
			this.PickOrdersModuleButtonGrid.PerformLayout();
			this.OrdersGroupBox.ResumeLayout(false);
			this.OrdersGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
