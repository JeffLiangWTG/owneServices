using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class ProductDeliveryForm : ZChildForm
	{
		ZTextBox zTextBox1;
		ZGrid OrderLinesGrid;
		ZButton CloseButton;
		ZCheckedListBox OrdersCheckboxList;
		ZLabel zLabel2;
		ZLabel zLabel3;
		ZLabel zLabel4;
		internal ZButton ReceiveAllButton;

		protected override void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			this.zTextBox1 = new ZTextBox();
			this.OrderLinesGrid = new ZGrid();
			this.CloseButton = new ZButton();
			this.OrdersCheckboxList = new ZCheckedListBox();
			this.zLabel2 = new ZLabel();
			this.zLabel3 = new ZLabel();
			this.zLabel4 = new ZLabel();
			this.ReceiveAllButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 532, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 24, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobShipmentPreplanning);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EF_PreshipID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_PreshipID)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 52, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.zTextBox1.TabIndex = 2;
			// 
			// OrderLinesGrid
			// 
			this.OrderLinesGrid.AllowNavigation = false;
			this.OrderLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrderLinesGrid, "OrderLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).OrderNumberAndSplit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).OrderLineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_LineSplitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_Partno)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_InnerPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_TotalInnerPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_QtyInvoiced)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_QtyReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_QuantityRemaining)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_ItemPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_LineDropDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_LineStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_SubLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_ContainerPackingOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_CommercialInvoiceNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_RN_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderDeliveryLine)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).OrderLines)).SyncRoot)).Line.JO_ContainerNumber)));
			this.OrderLinesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|b4e6752a-ff40-446f-a689-dd06c216331e", "Order No");
			zDropEditColumnStyleInfo1.ColumnName = "OrderNumberAndSplit";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|ba32591d-d4ae-43e5-993f-0e4623beb105", "Line N");
			zDropEditColumnStyleInfo2.ColumnName = "OrderLineNumber";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Line+JO_LineSplitNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Line+JO_Partno";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select the Product";
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Enter the Product Number";
			zTextBoxColumnStyleInfo1.ColumnName = "Line+JO_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.ToolTip = "Enter a Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Line+JO_InnerPacks";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.ToolTip = "Enter the number of Inner Packs";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Line+JO_OuterPacks";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.ToolTip = "Enter the number of Outer Packs";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|1884891b-a951-4b9e-b8c6-c9fefa97d7de", "Total Inner Packs");
			zCalcEditColumnStyleInfo4.ColumnName = "Line+JO_TotalInnerPacks";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "Line+JO_F3_NKPackType";
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "Line+JO_Quantity";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.ToolTip = "Enter the Quantity Ordered";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Line+JO_QtyInvoiced";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.ToolTip = "Enter the Quantity Invoiced";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "Line+JO_QtyReceived";
			zCalcEditColumnStyleInfo7.ToolTip = "Enter the Quantity Received";
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|16e477c6-f4f9-41ca-b440-f48f36f56589", "Qty Remaining");
			zCalcEditColumnStyleInfo8.ColumnName = "Line+JO_QuantityRemaining";
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.ToolTip = "Enter the Quantity Remaining";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "Line+JO_ItemPrice";
			zCalcEditColumnStyleInfo9.Decimals = 4;
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.ToolTip = "Enter the item price";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "Line+JO_LinePrice";
			zCalcEditColumnStyleInfo10.IsReadOnly = true;
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.ToolTip = "The Total Price for this line";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "Line+JO_LineDropDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "Line+JO_LineStatus";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "Line+JO_SubLineNo";
			zCalcEditColumnStyleInfo11.Decimals = 0;
			zCalcEditColumnStyleInfo11.IsReadOnly = true;
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "Line+JO_ContainerPackingOrder";
			zCalcEditColumnStyleInfo12.Decimals = 0;
			zTextBoxColumnStyleInfo3.ColumnName = "Line+JO_CommercialInvoiceNo";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Line+JO_RN_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "Line+JO_ContainerNumber";
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.OrderLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrderLinesGrid.GridId = "0a4537f2-0e17-4dc4-89f4-81e103c25a5e";
			this.OrderLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderLinesGrid.LayoutKey = "OrderLinesGrid";
			this.OrderLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 97, true);
			this.OrderLinesGrid.Name = "OrderLinesGrid";
			this.OrderLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 404, true);
			this.OrderLinesGrid.TabIndex = 7;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|2632a528-52cc-4d38-ad1f-59cb4a479945", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(883, 503, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// OrdersCheckboxList
			// 
			this.OrdersCheckboxList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.OrdersCheckboxList.BindingItems = null;
			this.BindingSource.SetBindingMember(this.OrdersCheckboxList, "OrderNumberList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBoolDescriptionPairList)(((JobShipmentPreplanning)(null)).OrderNumberList)));
			this.OrdersCheckboxList.FormattingEnabled = true;
			this.OrdersCheckboxList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 97, true);
			this.OrdersCheckboxList.Name = "OrdersCheckboxList";
			this.OrdersCheckboxList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 394, true);
			this.OrdersCheckboxList.TabIndex = 5;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|86886cdd-2c45-48e0-a696-1eec31e7cf23", "Orders");
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 81, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.zLabel2.TabIndex = 4;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|9fc84dd4-3382-4a93-8844-fab8c06208d3", "Order Lines");
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 81, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.zLabel3.TabIndex = 6;
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 36, true);
			this.zLabel4.TabIndex = 0;
			// 
			// ReceiveAllButton
			// 
			this.ReceiveAllButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ProductDeliveryForm|721dd536-cb12-4b9b-8dae-2120cfc5c5af", "Receive All");
			this.ReceiveAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 52, true);
			this.ReceiveAllButton.Name = "ReceiveAllButton";
			this.ReceiveAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.ReceiveAllButton.TabIndex = 3;
			this.ReceiveAllButton.UseVisualStyleBackColor = true;
			this.ReceiveAllButton.Click += new EventHandler(this.ReceiveAllButton_Click);
			// 
			// ProductDeliveryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 556, true);
			this.Controls.Add(this.ReceiveAllButton);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.OrdersCheckboxList);
			this.Controls.Add(this.OrderLinesGrid);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.zTextBox1);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(JobShipmentPreplanning);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning";
			this.DoubleBuffered = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 568, true);
			this.Name = "ProductDeliveryForm";
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.OrderLinesGrid, 0);
			this.Controls.SetChildIndex(this.OrdersCheckboxList, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.ReceiveAllButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
