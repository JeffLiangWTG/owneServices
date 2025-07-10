using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderLineEntryUserControl
	{
		private IContainer components;
		private ZTemplateTabControl DetailTabControl;
		private ZTextBox zTextBox2;
		private ZTextBox zTextBox1;
		private ZGuidFindBox zGuidFindBox1;
		private ZDateEdit zDateEdit2;
		private ZDateEdit zDateEdit1;
		private ZTextBox partAttrib3;
		private ZTextBox partAttrib2;
		private ZTextBox partAttrib1;
		private ZGroupBox zGroupBox1;
		private ZDropEdit zDropEdit2;
		private ZDropEdit zDropEdit1;
		private ZCalcEdit zCalcEdit6;
		private ZCalcEdit zCalcEdit5;
		private ZCalcEdit zCalcEdit7;
		private ZCalcEdit zCalcEdit8;
		private ZGroupBox CustomsRelatedDataGroupBox;
		private ZCalcEdit zCalcEdit4;
		private ZTextBox zTextBox3;
		private ZCalcEdit zCalcEdit3;
		private ZCalcEdit zCalcEdit2;
		private ZTextBox zTextBox4;
		private ZCalcEdit zCalcEdit1;
		private ZTextBox zTextBox8;
		private ZTextBox zTextBox9;
		private ZDateEdit zDateEdit3;
		private ZTextBox zTextBox10;
		private ZCalcEdit zCalcEdit9;
		private ZDropEdit zDropEdit3;
		private ZCalcEdit zCalcEdit11;
		private ZDropEdit zDropEdit5;
		private ZCalcEdit zCalcEdit10;
		private ZDropEdit zDropEdit4;
		private ZTextBox zTextBox11;
		private ZGroupBox CrossDockGroupBox;
		internal CrossDockedInventoryAttachedToOrderLineGrid CrossDockedInventoryModuleButtonGrid;
		private ZTabPage CustomFieldsTabPage;
		private MasterFiles.GUI.CustomLabelsUserControl customLabelsUserControl1;
		private ZPanel CustomsRelatedDataPanel;
		private ZPanel CustomsRelatedDataNotAvailablePanel;
		private ZLabel zLabel1;
		private ZTextBox SerialNumberTextBox;
		private ZTabPage DetailTabPage;

		private void InitializeComponent()
		{
			this.components = new Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			this.DetailTabControl = new ZTemplateTabControl();
			this.DetailTabPage = new ZTabPage();
			this.CrossDockGroupBox = new ZGroupBox();
			this.CrossDockedInventoryModuleButtonGrid = new CrossDockedInventoryAttachedToOrderLineGrid();
			this.CustomsRelatedDataGroupBox = new ZGroupBox();
			this.CustomsRelatedDataNotAvailablePanel = new ZPanel();
			this.zLabel1 = new ZLabel();
			this.CustomsRelatedDataPanel = new ZPanel();
			this.zTextBox9 = new ZTextBox();
			this.zCalcEdit4 = new ZCalcEdit();
			this.zTextBox10 = new ZTextBox();
			this.zTextBox3 = new ZTextBox();
			this.zDateEdit3 = new ZDateEdit();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zTextBox8 = new ZTextBox();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.zTextBox4 = new ZTextBox();
			this.zGroupBox1 = new ZGroupBox();
			this.SerialNumberTextBox = new ZTextBox();
			this.zTextBox11 = new ZTextBox();
			this.zCalcEdit11 = new ZCalcEdit();
			this.zDropEdit5 = new ZDropEdit();
			this.zCalcEdit10 = new ZCalcEdit();
			this.zDropEdit4 = new ZDropEdit();
			this.zCalcEdit9 = new ZCalcEdit();
			this.zDropEdit3 = new ZDropEdit();
			this.zCalcEdit7 = new ZCalcEdit();
			this.zCalcEdit8 = new ZCalcEdit();
			this.zCalcEdit6 = new ZCalcEdit();
			this.zCalcEdit5 = new ZCalcEdit();
			this.zDropEdit2 = new ZDropEdit();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.zDropEdit1 = new ZDropEdit();
			this.zTextBox1 = new ZTextBox();
			this.zTextBox2 = new ZTextBox();
			this.zDateEdit2 = new ZDateEdit();
			this.zDateEdit1 = new ZDateEdit();
			this.partAttrib1 = new ZTextBox();
			this.partAttrib3 = new ZTextBox();
			this.partAttrib2 = new ZTextBox();
			this.CustomFieldsTabPage = new ZTabPage();
			this.customLabelsUserControl1 = new MasterFiles.GUI.CustomLabelsUserControl();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailTabControl.SuspendLayout();
			this.DetailTabPage.SuspendLayout();
			this.CrossDockGroupBox.SuspendLayout();
			((ISupportInitialize)(this.CrossDockedInventoryModuleButtonGrid.InnerGrid)).BeginInit();
			this.CrossDockedInventoryModuleButtonGrid.SuspendLayout();
			this.CustomsRelatedDataGroupBox.SuspendLayout();
			this.CustomsRelatedDataNotAvailablePanel.SuspendLayout();
			this.CustomsRelatedDataPanel.SuspendLayout();
			this.zDateEdit3.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zDropEdit5.SuspendLayout();
			this.zDropEdit4.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.customLabelsUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPickableDocketLine);
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailTabPage);
			this.DetailTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.SelectedIndex = 0;
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 575, true);
			this.DetailTabControl.TabIndex = 0;
			// 
			// DetailTabPage
			// 
			this.DetailTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|cfffb55b-9dce-4450-a9dd-0ae81ca6a29a", "Order Line");
			this.DetailTabPage.Controls.Add(this.CrossDockGroupBox);
			this.DetailTabPage.Controls.Add(this.CustomsRelatedDataGroupBox);
			this.DetailTabPage.Controls.Add(this.zGroupBox1);
			this.DetailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailTabPage.Name = "DetailTabPage";
			this.DetailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 548, true);
			this.DetailTabPage.TabIndex = 0;
			// 
			// CrossDockGroupBox
			// 
			this.CrossDockGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|67196fa6-4996-42c4-86f1-088541c261ff", "Cross-Dock Allocations");
			this.CrossDockGroupBox.Controls.Add(this.CrossDockedInventoryModuleButtonGrid);
			this.CrossDockGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CrossDockGroupBox, false);
			this.CrossDockGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 373, true);
			this.CrossDockGroupBox.Name = "CrossDockGroupBox";
			this.CrossDockGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 175, true);
			this.CrossDockGroupBox.TabIndex = 2;
			this.CrossDockGroupBox.TabStop = false;
			// 
			// CrossDockedInventoryModuleButtonGrid
			// 
			this.CrossDockedInventoryModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CrossDockedInventoryModuleButtonGrid, "ReservedPickLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsPickableDocketLine)(null)).ReservedPickLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsPickableDocketLine)(null)).InventoryFilter)));
			this.CrossDockedInventoryModuleButtonGrid.BindToFindBoxList = "InventoryFilter";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|2a78399a-7991-4713-9462-4c41c95552a0", "Receipt Ref");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "InventoryLine+ReceiptReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|f54db7a3-f0ee-44db-a68d-fd68178b558a", "ETA/Arrival");
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "Inventory+WI_ArrivalDateOrETA";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|0762165a-c89d-4814-be6a-354beea4b749", "Product");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Inventory+WI_OP_PartNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|6266839a-ef7b-4884-baf8-0c491a5dbfdd", "Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Inventory+WI_OP_Desc";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|ce6d78e1-24ea-4270-b10a-d4389e54e908", "Commodity Code");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "Inventory+CommodityCode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "Inventory+SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|6fc2108e-a2d6-4b7b-812d-908801fb2e1f", "Available Qty");
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "Inventory+WI_AvailableForCrossDockQuantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "Inventory+SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo2.ColumnName = "ReservedQuantity";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "Inventory+SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "WZ_OriginalReservedQty";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|30987c1d-d40b-46b1-aeb6-b88d724be7e5", "Status");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "Inventory+StatusDesc";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|0fc603ee-e9da-43d8-8604-57e3702c33ad", "Location Status");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "Inventory+LocationStatus";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|5c4397e1-3d30-4cc7-9c33-879324cf6337", "Location");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "Inventory+LocationString";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|4806f906-dae7-44e3-ac26-f6bfb8978e21", "Area");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "Inventory+LocationPickAreaName";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|2aa21e6c-725c-41f8-9084-5992f211876d", "Area Type");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "Inventory+LocationPickAreaType";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Inventory+PerPackageQty";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|3b166869-eb57-4d09-b11f-f4ad2282fc56", "Pallet ID");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "Inventory+WI_PalletID";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|57f7cbab-b71d-4862-a5bc-93001b0bbd38", "Part Attrib. 1");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "Inventory+WI_PartAttrib1";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|87a168f3-4742-4da7-a2bd-60625b462c95", "Part Attrib. 2");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "Inventory+WI_PartAttrib2";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedInventoryAttachedToOrderLineGrid|43d2eaf9-119e-4016-a7fb-f43e01d01ff5", "Part Attrib. 3");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "Inventory+WI_PartAttrib3";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "Inventory+WI_SerialNumber";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo2.ColumnName = "Inventory+WI_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "Inventory+WI_PackingDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CrossDockedInventoryModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrossDockedInventoryModuleButtonGrid.GridId = "15f8b11b-851a-47c5-97c1-1c38c6a283bf";
			// 
			// 
			// 
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.GridId = null;
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.Name = "Grid";
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 118, true);
			this.CrossDockedInventoryModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.CrossDockedInventoryModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CrossDockedInventoryModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsPickLine;
			this.CrossDockedInventoryModuleButtonGrid.Name = "CrossDockedInventoryModuleButtonGrid";
			this.CrossDockedInventoryModuleButtonGrid.NameOfAGridElement = Enterprise.Warehouse.Transactions.GUI.Res.GetData("B511E4ED-22C2-4D01-8916-EB4756D348D1", "Receipt Line");
			this.CrossDockedInventoryModuleButtonGrid.OrderLine = null;
			this.CrossDockedInventoryModuleButtonGrid.ReadOnly = false;
			this.CrossDockedInventoryModuleButtonGrid.ShowNewButton = false;
			this.CrossDockedInventoryModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 156, true);
			this.CrossDockedInventoryModuleButtonGrid.TabIndex = 0;
			// 
			// CustomsRelatedDataGroupBox
			// 
			this.CustomsRelatedDataGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|b2071ba9-e4fe-4069-83ad-2e095e2d4533", "Customs Related Data");
			this.CustomsRelatedDataGroupBox.Controls.Add(this.CustomsRelatedDataNotAvailablePanel);
			this.CustomsRelatedDataGroupBox.Controls.Add(this.CustomsRelatedDataPanel);
			this.CustomsRelatedDataGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CustomsRelatedDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.CustomsRelatedDataGroupBox.Name = "CustomsRelatedDataGroupBox";
			this.CustomsRelatedDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 145, true);
			this.CustomsRelatedDataGroupBox.TabIndex = 1;
			this.CustomsRelatedDataGroupBox.TabStop = false;
			// 
			// CustomsRelatedDataNotAvailablePanel
			// 
			this.CustomsRelatedDataNotAvailablePanel.Controls.Add(this.zLabel1);
			this.CustomsRelatedDataNotAvailablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsRelatedDataNotAvailablePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsRelatedDataNotAvailablePanel.Name = "CustomsRelatedDataNotAvailablePanel";
			this.CustomsRelatedDataNotAvailablePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 126, true);
			this.CustomsRelatedDataNotAvailablePanel.TabIndex = 20;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|a868e611-83b2-4323-b251-567011196153", "Customs Related Data is only available for Customs Release Orders.");
			this.zLabel1.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 50, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 23, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CustomsRelatedDataPanel
			// 
			this.CustomsRelatedDataPanel.Controls.Add(this.zTextBox9);
			this.CustomsRelatedDataPanel.Controls.Add(this.zCalcEdit4);
			this.CustomsRelatedDataPanel.Controls.Add(this.zTextBox10);
			this.CustomsRelatedDataPanel.Controls.Add(this.zTextBox3);
			this.CustomsRelatedDataPanel.Controls.Add(this.zDateEdit3);
			this.CustomsRelatedDataPanel.Controls.Add(this.zCalcEdit3);
			this.CustomsRelatedDataPanel.Controls.Add(this.zTextBox8);
			this.CustomsRelatedDataPanel.Controls.Add(this.zCalcEdit2);
			this.CustomsRelatedDataPanel.Controls.Add(this.zCalcEdit1);
			this.CustomsRelatedDataPanel.Controls.Add(this.zTextBox4);
			this.CustomsRelatedDataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsRelatedDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsRelatedDataPanel.Name = "CustomsRelatedDataPanel";
			this.CustomsRelatedDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 126, true);
			this.CustomsRelatedDataPanel.TabIndex = 19;
			// 
			// zTextBox9
			// 
			this.BindingSource.SetBindingMember(this.zTextBox9, "CustomsData+WB_DeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CustomsData.WB_DeclarationReference)));
			this.zTextBox9.CaptionResourceString = null;
			this.zTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 73, true);
			this.zTextBox9.Name = "zTextBox9";
			this.zTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.zTextBox9.TabIndex = 7;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "CustomsData+WB_EntryLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).CustomsData.WB_EntryLineNo)));
			this.zCalcEdit4.CaptionResourceString = null;
			this.zCalcEdit4.DecimalPlaces = 0;
			this.zCalcEdit4.Decimals = 0;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 27, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEdit4.TabIndex = 3;
			this.zCalcEdit4.Text = "0";
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "CustomsData+WB_EntryKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CustomsData.WB_EntryKey)));
			this.zTextBox10.CaptionResourceString = null;
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 4, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.zTextBox10.TabIndex = 1;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "CustomsData+WB_AddInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CustomsData.WB_AddInfo)));
			this.zTextBox3.CaptionResourceString = null;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 73, true);
			this.zTextBox3.Multiline = true;
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 46, true);
			this.zTextBox3.TabIndex = 18;
			// 
			// zDateEdit3
			// 
			this.zDateEdit3.AllowDrop = true;
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit3, "CustomsData+WB_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).CustomsData.WB_EntryDate)));
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 50, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 5;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "CustomsData+WB_TILV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).CustomsData.WB_TILV)));
			this.zCalcEdit3.CaptionResourceString = null;
			this.zCalcEdit3.DecimalPlaces = 4;
			this.zCalcEdit3.Decimals = 4;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 50, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit3.TabIndex = 16;
			this.zCalcEdit3.Text = "0.0000";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "CustomsData+WB_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CustomsData.WB_RN_NKCountryOfOrigin)));
			this.zTextBox8.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|9aa4a929-e24a-435b-9dfc-651dd3ca0425", "Country/Region of Origin");
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 96, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.zTextBox8.TabIndex = 9;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "CustomsData+WB_ValueForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).CustomsData.WB_ValueForDuty)));
			this.zCalcEdit2.CaptionResourceString = null;
			this.zCalcEdit2.DecimalPlaces = 4;
			this.zCalcEdit2.Decimals = 4;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 27, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit2.TabIndex = 14;
			this.zCalcEdit2.Text = "0.0000";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "CustomsData+WB_CustomsQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).CustomsData.WB_CustomsQty)));
			this.zCalcEdit1.CaptionResourceString = null;
			this.zCalcEdit1.DecimalPlaces = 4;
			this.zCalcEdit1.Decimals = 4;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 4, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.zCalcEdit1.TabIndex = 11;
			this.zCalcEdit1.Text = "0.0000";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "CustomsData+WB_CustomsUnitOfQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CustomsData.WB_CustomsUnitOfQty)));
			this.zTextBox4.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox4, false);
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(675, 4, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.zTextBox4.TabIndex = 12;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|6a34df10-58d8-4601-905c-ce2bfe8ec553", "Order Line");
			this.zGroupBox1.Controls.Add(this.SerialNumberTextBox);
			this.zGroupBox1.Controls.Add(this.zTextBox11);
			this.zGroupBox1.Controls.Add(this.zCalcEdit11);
			this.zGroupBox1.Controls.Add(this.zDropEdit5);
			this.zGroupBox1.Controls.Add(this.zCalcEdit10);
			this.zGroupBox1.Controls.Add(this.zDropEdit4);
			this.zGroupBox1.Controls.Add(this.zCalcEdit9);
			this.zGroupBox1.Controls.Add(this.zDropEdit3);
			this.zGroupBox1.Controls.Add(this.zCalcEdit7);
			this.zGroupBox1.Controls.Add(this.zCalcEdit8);
			this.zGroupBox1.Controls.Add(this.zCalcEdit6);
			this.zGroupBox1.Controls.Add(this.zCalcEdit5);
			this.zGroupBox1.Controls.Add(this.zDropEdit2);
			this.zGroupBox1.Controls.Add(this.zGuidFindBox1);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Controls.Add(this.zDateEdit2);
			this.zGroupBox1.Controls.Add(this.zDateEdit1);
			this.zGroupBox1.Controls.Add(this.partAttrib1);
			this.zGroupBox1.Controls.Add(this.partAttrib3);
			this.zGroupBox1.Controls.Add(this.partAttrib2);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 228, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// SerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "WE_SerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).WE_SerialNumber)));
			this.SerialNumberTextBox.CaptionResourceString = null;
			this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 153, true);
			this.SerialNumberTextBox.Name = "SerialNumberTextBox";
			this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.SerialNumberTextBox.TabIndex = 32;
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "WE_LineComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).WE_LineComment)));
			this.zTextBox11.CaptionResourceString = null;
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 176, true);
			this.zTextBox11.Multiline = true;
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 46, true);
			this.zTextBox11.TabIndex = 34;
			// 
			// zCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit11, "SumOfUnitsMet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).SumOfUnitsMet)));
			this.zCalcEdit11.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.zCalcEdit11.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|36502f59-6274-4be5-ae65-38f5fd964f11", "Quantity Met");
			this.zCalcEdit11.DecimalPlaces = 2;
			this.zCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 153, true);
			this.zCalcEdit11.Name = "zCalcEdit11";
			this.zCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit11.TabIndex = 29;
			this.zCalcEdit11.Text = "0.00";
			this.zCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit5
			// 
			this.zDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit5, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).ProductUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit5, false);
			this.zDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 153, true);
			this.zDropEdit5.Name = "zDropEdit5";
			this.zDropEdit5.ShouldResizeByMaxLength = true;
			this.zDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.zDropEdit5.TabIndex = 30;
			// 
			// zCalcEdit10
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit10, "WE_ShortfallQuantityCached");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_ShortfallQuantityCached)));
			this.zCalcEdit10.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.zCalcEdit10.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|e65fe524-fd7c-41e1-8168-d7bf3d29efbf", "Shortfall Quantity");
			this.zCalcEdit10.DecimalPlaces = 2;
			this.zCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 130, true);
			this.zCalcEdit10.Name = "zCalcEdit10";
			this.zCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit10.TabIndex = 24;
			this.zCalcEdit10.Text = "0.00";
			this.zCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit4
			// 
			this.zDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit4, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).ProductUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit4, false);
			this.zDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 130, true);
			this.zDropEdit4.Name = "zDropEdit4";
			this.zDropEdit4.ShouldResizeByMaxLength = true;
			this.zDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.zDropEdit4.TabIndex = 25;
			// 
			// zCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "WE_CrossDockQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_CrossDockQuantity)));
			this.zCalcEdit9.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.zCalcEdit9.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|f0959821-daec-4734-80a1-f41bf85b52e4", "Allocated Quantity");
			this.zCalcEdit9.DecimalPlaces = 2;
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 107, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit9.TabIndex = 19;
			this.zCalcEdit9.Text = "0.00";
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).ProductUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit3, false);
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 107, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.ShouldResizeByMaxLength = true;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.zDropEdit3.TabIndex = 20;
			// 
			// zCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "WE_TransactionQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_TransactionQuantity)));
			this.zCalcEdit7.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			this.zCalcEdit7.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|3485125e-5299-49fc-9848-4007c23f3155", "Unit Quantity");
			this.zCalcEdit7.DecimalPlaces = 2;
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 83, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit7.TabIndex = 14;
			this.zCalcEdit7.Text = "0.00";
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit8
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit8, "WE_PackQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_PackQuantity)));
			this.zCalcEdit8.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|78febd4f-a134-445f-aa3c-836e924e1d31", "Pack Quantity");
			this.zCalcEdit8.DecimalPlaces = 2;
			this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 60, true);
			this.zCalcEdit8.Name = "zCalcEdit8";
			this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit8.TabIndex = 9;
			this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "WE_SubLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_SubLineNo)));
			this.zCalcEdit6.CaptionResourceString = null;
			this.zCalcEdit6.DecimalPlaces = 0;
			this.zCalcEdit6.Decimals = 0;
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 60, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit6.TabIndex = 12;
			this.zCalcEdit6.Text = "0";
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit5, "WE_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsPickableDocketLine)(null)).WE_LineNo)));
			this.zCalcEdit5.CaptionResourceString = null;
			this.zCalcEdit5.DecimalPlaces = 0;
			this.zCalcEdit5.Decimals = 0;
			this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 37, true);
			this.zCalcEdit5.Name = "zCalcEdit5";
			this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.zCalcEdit5.TabIndex = 6;
			this.zCalcEdit5.Text = "0";
			this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "ProductUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).ProductUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit2, false);
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 83, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.ShouldResizeByMaxLength = true;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.zDropEdit2.TabIndex = 15;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WE_OP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsPickableDocketLine)(null)).WE_OP)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 14, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PopupCaption = null;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.zGuidFindBox1.TabIndex = 1;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "WE_F3_NKPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).WE_F3_NKPackType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit1, false);
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 60, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.zDropEdit1.TabIndex = 10;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "ProductDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).ProductDesc)));
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|326888ff-3a67-4de4-b61e-835d7f437023", "Product Description");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 37, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "CommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).CommodityCode)));
			this.zTextBox2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|0795c8a6-aec9-4836-8206-23f3abee8fd2", "Commodity");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 14, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "WE_PackingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).WE_PackingDate)));
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 198, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 38;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "WE_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPickableDocketLine)(null)).WE_ExpiryDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 175, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 36;
			// 
			// partAttrib1
			// 
			this.BindingSource.SetBindingMember(this.partAttrib1, "WE_PartAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).WE_PartAttrib1)));
			this.partAttrib1.CaptionResourceString = null;
			this.partAttrib1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 83, true);
			this.partAttrib1.Name = "partAttrib1";
			this.partAttrib1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.partAttrib1.TabIndex = 17;
			// 
			// partAttrib3
			// 
			this.BindingSource.SetBindingMember(this.partAttrib3, "WE_PartAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).WE_PartAttrib3)));
			this.partAttrib3.CaptionResourceString = null;
			this.partAttrib3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 129, true);
			this.partAttrib3.Name = "partAttrib3";
			this.partAttrib3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.partAttrib3.TabIndex = 27;
			// 
			// partAttrib2
			// 
			this.BindingSource.SetBindingMember(this.partAttrib2, "WE_PartAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsPickableDocketLine)(null)).WE_PartAttrib2)));
			this.partAttrib2.CaptionResourceString = null;
			this.partAttrib2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 106, true);
			this.partAttrib2.Name = "partAttrib2";
			this.partAttrib2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.partAttrib2.TabIndex = 22;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryUserControl|fdd2520a-bae2-419b-b31a-db9850cec941", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.customLabelsUserControl1);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 548, true);
			this.CustomFieldsTabPage.TabIndex = 1;
			// 
			// customLabelsUserControl1
			// 
			this.customLabelsUserControl1.AllowDrop = true;
			this.customLabelsUserControl1.BindToMember = "";
			this.customLabelsUserControl1.CustomLabelsProvider = null;
			this.customLabelsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customLabelsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customLabelsUserControl1.Name = "customLabelsUserControl1";
			this.customLabelsUserControl1.PropertyNamesToExclude = Array.Empty<string>();
			this.customLabelsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 548, true);
			this.customLabelsUserControl1.TabIndex = 0;
			// 
			// OrderLineEntryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailTabControl);
			this.Name = "OrderLineEntryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 575, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailTabPage.ResumeLayout(false);
			this.DetailTabPage.PerformLayout();
			this.CrossDockGroupBox.ResumeLayout(false);
			this.CrossDockGroupBox.PerformLayout();
			((ISupportInitialize)(this.CrossDockedInventoryModuleButtonGrid.InnerGrid)).EndInit();
			this.CrossDockedInventoryModuleButtonGrid.ResumeLayout(true);
			this.CrossDockedInventoryModuleButtonGrid.PerformLayout();
			this.CustomsRelatedDataGroupBox.ResumeLayout(false);
			this.CustomsRelatedDataGroupBox.PerformLayout();
			this.CustomsRelatedDataNotAvailablePanel.ResumeLayout(false);
			this.CustomsRelatedDataNotAvailablePanel.PerformLayout();
			this.CustomsRelatedDataPanel.ResumeLayout(false);
			this.CustomsRelatedDataPanel.PerformLayout();
			this.zDateEdit3.ResumeLayout(true);
			this.zDateEdit3.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zDropEdit5.ResumeLayout(true);
			this.zDropEdit5.PerformLayout();
			this.zDropEdit4.ResumeLayout(true);
			this.zDropEdit4.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.customLabelsUserControl1.ResumeLayout(true);
			this.customLabelsUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
