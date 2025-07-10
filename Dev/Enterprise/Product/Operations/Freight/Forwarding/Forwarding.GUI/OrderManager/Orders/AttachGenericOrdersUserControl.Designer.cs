using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class AttachGenericOrdersUserControl : ZUserControl
	{
		ZGroupBox OrdersGroupBox;
		internal GenericOrdersModuleButtonGrid GenericOrdersButtonGrid;
		internal ZTextBox JP_OrderItemsAsStringTextBox;
		internal ZButton OrderItemsEditButton;
		ZPanel OrderReferencesPanel;
		ZPanel OrderReferencesTextBoxPanel;
		ZPanel OrderReferencesButtonPanel;

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo transportModeTextColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo etdDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo etaDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo totalWeightColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo weightUnitColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo totalVolumeColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo volumeUnitColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo quantityRemainingColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo quantityInvoicedColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo quantityOrderedColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo quantityReceivedColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo totalPacksColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo packsTypeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo buyerColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo supplierColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo requiredExWorksColumnStyleInfo = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo requiredInStoreColumnStyleInfo = new ZDateEditColumnStyleInfo();

			this.OrdersGroupBox = new ZGroupBox();
			this.OrderReferencesPanel = new ZPanel();
			this.OrderReferencesTextBoxPanel = new ZPanel();
			this.JP_OrderItemsAsStringTextBox = new ZTextBox();
			this.OrderReferencesButtonPanel = new ZPanel();
			this.OrderItemsEditButton = new ZButton();
			this.GenericOrdersButtonGrid = new GenericOrdersModuleButtonGrid();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrdersGroupBox.SuspendLayout();
			this.OrderReferencesPanel.SuspendLayout();
			this.OrderReferencesTextBoxPanel.SuspendLayout();
			this.OrderReferencesButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IAttachGenericOrders);
			// 
			// OrdersGroupBox
			// 
			this.OrdersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c303addd-eec6-4401-8abb-88fecf495529", "Job Management Links");
			this.OrdersGroupBox.Controls.Add(this.OrderReferencesPanel);
			this.OrdersGroupBox.Controls.Add(this.GenericOrdersButtonGrid);
			this.OrdersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersGroupBox.Name = "OrdersGroupBox";
			this.OrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 336, true);
			this.OrdersGroupBox.TabIndex = 16;
			this.OrdersGroupBox.TabStop = false;
			// 
			// OrderReferencesPanel
			// 
			this.OrderReferencesPanel.Controls.Add(this.OrderReferencesTextBoxPanel);
			this.OrderReferencesPanel.Controls.Add(this.OrderReferencesButtonPanel);
			this.OrderReferencesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrderReferencesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrderReferencesPanel.Name = "OrderReferencesPanel";
			this.OrderReferencesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 22, true);
			this.OrderReferencesPanel.TabIndex = 22;
			// 
			// OrderReferencesTextBoxPanel
			// 
			this.OrderReferencesTextBoxPanel.Controls.Add(this.JP_OrderItemsAsStringTextBox);
			this.OrderReferencesTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderReferencesTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderReferencesTextBoxPanel.Name = "OrderReferencesTextBoxPanel";
			this.OrderReferencesTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 22, true);
			this.OrderReferencesTextBoxPanel.TabIndex = 1;
			// 
			// JP_OrderItemsAsStringTextBox
			// 
			this.JP_OrderItemsAsStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JP_OrderItemsAsStringTextBox, "DocsAndCartage.JP_OrderItemsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((IAttachGenericOrders)(null)).DocsAndCartage.JP_OrderItemsAsString)));
			this.JP_OrderItemsAsStringTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersAttachUserControl|d367ad48-b5d4-4b94-b4ef-7623eee7afdf", "Order Refs", "Order References", "Enter the Order References.");
			this.JP_OrderItemsAsStringTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JP_OrderItemsAsStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 1, true);
			this.JP_OrderItemsAsStringTextBox.Name = "JP_OrderItemsAsStringTextBox";
			this.JP_OrderItemsAsStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.JP_OrderItemsAsStringTextBox.TabIndex = 20;
			// 
			// OrderReferencesButtonPanel
			// 
			this.OrderReferencesButtonPanel.Controls.Add(this.OrderItemsEditButton);
			this.OrderReferencesButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.OrderReferencesButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.OrderReferencesButtonPanel.Name = "OrderReferencesButtonPanel";
			this.OrderReferencesButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 22, true);
			this.OrderReferencesButtonPanel.TabIndex = 0;
			// 
			// OrderItemsEditButton
			// 
			this.OrderItemsEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OrderItemsEditButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersAttachUserControl|080893bd-92be-4bec-af36-32fb2e4330f9", "More...");
			this.OrderItemsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.OrderItemsEditButton.Name = "OrderItemsEditButton";
			this.OrderItemsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.OrderItemsEditButton.TabIndex = 21;
			this.OrderItemsEditButton.Click += new EventHandler(this.OrderReferencesButton_Click);
			// 
			// GenericOrdersButtonGrid
			// 
			this.GenericOrdersButtonGrid.AllowDrop = true;
			this.GenericOrdersButtonGrid.AllowNewWithoutSaving = true;
			this.GenericOrdersButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GenericOrdersButtonGrid, "GenericOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IAttachGenericOrders)(null)).GenericOrders)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JobNo";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("649b6b87-f577-46ec-b1e2-27e03ae50581", "Job Type");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JobType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5ed85504-11e7-4940-bafe-0c66d7a53a18", "Job Description");
			zTextBoxColumnStyleInfo3.ColumnName = "JobDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f89f94ca-059d-4079-8865-8d14dd657354", "Job Status");
			zTextBoxColumnStyleInfo4.ColumnName = "JobStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("bd7711a7-a6c1-48c7-9431-6c52635135dd", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "JobDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1b973fad-f89f-406c-8cf1-c2bca64f6abf", "Goods Description");
			zTextBoxColumnStyleInfo5.ColumnName = "GoodsDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			transportModeTextColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b5a656d8-b720-4efd-9c05-31492286eb08", "Transport Mode");
			transportModeTextColumnStyleInfo.ColumnName = "TransportMode";
			transportModeTextColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			transportModeTextColumnStyleInfo.IsVisible = false;
			etdDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("bb046aac-3957-497e-8d91-12fed108b829", "ETD");
			etdDateEditColumnStyleInfo.ColumnName = "ETD";
			etdDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			etdDateEditColumnStyleInfo.IsVisible = false;
			etaDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9628c3ff-5d93-4b6f-8c19-54bf55633007", "ETA");
			etaDateEditColumnStyleInfo.ColumnName = "ETA";
			etaDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			etaDateEditColumnStyleInfo.IsVisible = false;
			totalWeightColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("025f9872-de33-4815-be43-a0b55d4eaea9", "Total Weight");
			totalWeightColumnStyleInfo.ColumnName = "TotalWeight";
			totalWeightColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			totalWeightColumnStyleInfo.IsVisible = false;
			totalWeightColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("42c65d21-f921-4af6-b887-0cef6ef8cf26", "Total Weight");
			weightUnitColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("71229350-efb8-43ba-af04-fc3e1f4b05ca", "Unit of Weight");
			weightUnitColumnStyleInfo.ColumnName = "WeightUnit";
			weightUnitColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			weightUnitColumnStyleInfo.IsVisible = false;
			weightUnitColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("42c65d21-f921-4af6-b887-0cef6ef8cf26", "Total Weight");
			totalVolumeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("cde2fcb8-cc5a-4271-b395-241ff010cf2c", "Total Volume");
			totalVolumeColumnStyleInfo.ColumnName = "TotalVolume";
			totalVolumeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			totalVolumeColumnStyleInfo.IsVisible = false;
			totalVolumeColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("6c621b06-0b76-406b-bd60-bfcce34dc813", "Total Volume");
			volumeUnitColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("abc39960-b539-4c31-8df6-293ca9c9d0e6", "Unit of Volume");
			volumeUnitColumnStyleInfo.ColumnName = "VolumeUnit";
			volumeUnitColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			volumeUnitColumnStyleInfo.IsVisible = false;
			volumeUnitColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("6c621b06-0b76-406b-bd60-bfcce34dc813", "Total Volume");
			quantityRemainingColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5be734e5-6d45-4af5-b448-1efd3c56a346", "Quantity Remaining");
			quantityRemainingColumnStyleInfo.ColumnName = "QuantityRemaining";
			quantityRemainingColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			quantityRemainingColumnStyleInfo.IsVisible = false;
			quantityInvoicedColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a06a6e9c-2e65-4661-a1fd-6670ec848f44", "Quantity Invoiced");
			quantityInvoicedColumnStyleInfo.ColumnName = "QuantityInvoiced";
			quantityInvoicedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			quantityInvoicedColumnStyleInfo.IsVisible = false;
			quantityOrderedColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b55a03c7-ef2d-4bc2-a5b1-5aad22e59531", "Quantity Ordered");
			quantityOrderedColumnStyleInfo.ColumnName = "QuantityOrdered";
			quantityOrderedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			quantityOrderedColumnStyleInfo.IsVisible = false;
			quantityReceivedColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("80a976b6-3ad6-4afa-83ad-2298cd801103", "Quantity Received");
			quantityReceivedColumnStyleInfo.ColumnName = "QuantityReceived";
			quantityReceivedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			quantityReceivedColumnStyleInfo.IsVisible = false;
			totalPacksColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c8a20193-5def-46f5-97fc-257e2bb08d45", "Packs");
			totalPacksColumnStyleInfo.ColumnName = "TotalPacks";
			totalPacksColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			totalPacksColumnStyleInfo.IsVisible = false;
			totalPacksColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("6012bfa7-ea2c-4585-8f56-b6dec1f201ae", "Total Packs");
			packsTypeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e8e8cb1f-b897-41a9-861c-23ac179da9f2", "Packs Type");
			packsTypeColumnStyleInfo.ColumnName = "PacksType";
			packsTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			packsTypeColumnStyleInfo.IsVisible = false;
			packsTypeColumnStyleInfo.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("6012bfa7-ea2c-4585-8f56-b6dec1f201ae", "Total Packs");
			buyerColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4eb2d801-5419-4d2a-9a0a-0a6e27c3811c", "Buyer");
			buyerColumnStyleInfo.ColumnName = "BuyerOrgCode";
			buyerColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			buyerColumnStyleInfo.IsVisible = false;
			supplierColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a62c35b2-5532-4990-8dc6-72cbc19ee39a", "Supplier");
			supplierColumnStyleInfo.ColumnName = "SupplierOrgCode";
			supplierColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			supplierColumnStyleInfo.IsVisible = false;
			requiredExWorksColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("de224796-defc-4368-aa06-bf21524ffdde", "Req. Ex Works", "Required Ex Works", "Ex Works Required By");
			requiredExWorksColumnStyleInfo.ColumnName = "RequiredExWorks";
			requiredExWorksColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			requiredExWorksColumnStyleInfo.IsVisible = false;
			requiredInStoreColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("884a322d-c675-4dd7-9b40-8c0e78130318", "Req. In Store", "Required In Store", "The date that the Order is required in store");
			requiredInStoreColumnStyleInfo.ColumnName = "RequiredInStore";
			requiredInStoreColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			requiredInStoreColumnStyleInfo.IsVisible = false;

			this.GenericOrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(transportModeTextColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(etdDateEditColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(etaDateEditColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(totalWeightColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(weightUnitColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(totalVolumeColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(volumeUnitColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(quantityRemainingColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(quantityInvoicedColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(quantityOrderedColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(quantityReceivedColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(totalPacksColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(packsTypeColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(buyerColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(supplierColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(requiredExWorksColumnStyleInfo);
			this.GenericOrdersButtonGrid.ColumnStyles.Add(requiredInStoreColumnStyleInfo);
			this.GenericOrdersButtonGrid.GridId = "98b8677d-e4c0-4025-9019-1bc4d37d2f49";
			// 
			// 
			// 
			this.GenericOrdersButtonGrid.InnerGrid.AllowNavigation = false;
			this.GenericOrdersButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GenericOrdersButtonGrid.InnerGrid.CaptionVisible = false;
			this.GenericOrdersButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.GenericOrdersButtonGrid.InnerGrid.GridId = null;
			this.GenericOrdersButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GenericOrdersButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.GenericOrdersButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.GenericOrdersButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.GenericOrdersButtonGrid.InnerGrid.Name = "Grid";
			this.GenericOrdersButtonGrid.InnerGrid.ReadOnly = true;
			this.GenericOrdersButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 259, true);
			this.GenericOrdersButtonGrid.InnerGrid.TabIndex = 0;
			this.GenericOrdersButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.GenericOrdersButtonGrid.Name = "GenericOrdersButtonGrid";
			this.GenericOrdersButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("30C5411A-9C41-41A0-ADD3-3E3AFC5F4C0C", "Order");
			this.GenericOrdersButtonGrid.ReadOnly = true;
			this.GenericOrdersButtonGrid.ShowAttachButton = false;
			this.GenericOrdersButtonGrid.ShowDetachButton = false;
			this.GenericOrdersButtonGrid.ShowEditButton = false;
			this.GenericOrdersButtonGrid.ShowNewButton = false;
			this.GenericOrdersButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 289, true);
			this.GenericOrdersButtonGrid.TabIndex = 10;
			// 
			// AttachGenericOrdersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrdersGroupBox);
			this.Name = "AttachGenericOrdersUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 336, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrdersGroupBox.ResumeLayout(false);
			this.OrderReferencesPanel.ResumeLayout(false);
			this.OrderReferencesTextBoxPanel.ResumeLayout(false);
			this.OrderReferencesTextBoxPanel.PerformLayout();
			this.OrderReferencesButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
