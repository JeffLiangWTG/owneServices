using System;
using System.ComponentModel;
using Enterprise.Freight.Forwarding.GUI.OrderManager.Orders;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderLineForm : ZForm
	{
		private ZTextBox JO_AdditionalTermsBoundTextBox;
		private ZDropEdit JO_INCOBoundDropEdit;
		private ZDateEdit JO_ExWorksDateBoundDateEdit;
		private ZTextBox JO_ConfirmationNumBoundTextBox;
		private ZDateEdit JO_ConfirmationDateBoundDateEdit;
		private ZGroupBox OuterPacksGroupBox;
		private ZCalcEdit WidthCalcEdit;
		private ZCalcEdit LengthCalcEdit;
		private ZGrid DeliveriesBoundGrid;
		private ZCalcDropEdit JO_InnerPacksBoundDropEdit;
		private ZCalcDropEdit JO_OuterPacksBoundDropEdit;
		private ZCalcDropEdit UnitOfDimensionCalcDropEdit;
		private ZCalcDropEdit VolumeCalcDropEdit;
		private ZCalcDropEdit WeightCalcDropEdit;
		protected ZGrid DeliveryContainersBoundGrid;
		private ZGrid RelatedDeliveryContainersBoundGrid;
		private ZCalcDropEdit OuterPacksCalcDropEdit;
		private ZTextBox SerialNumberTextBox;
		private ZLabel AllowableUnderLabel;
		private ZLabel AllowableOverLabel;
		private OrderLineToleranceControl QuantityOverPercentage;
		private ZLabel AvailableLateLabel;
		private OrderLineToleranceControl ExWorksDaysLate;
		private ZLabel AvailableEarlyLabel;
		private OrderLineToleranceControl ExWorksDaysEarly;
		private ZLabel ExWorksLabel;
		private ZTextBox JO_LineReferenceTextBox;
		private ZDateEdit JO_ShipmentWindowStartBoundDateEdit;
		private ZDateEdit JO_ShipmentWindowEndBoundDateEdit;

		ZTemplateTabControl OrderLineFormControl;
		ZTabPage DetailsTabPage;
		ZTextBox PartAttrib3TextBox;
		ZTextBox PartAttrib2TextBox;
		ZTextBox PartAttrib1TextBox;
		protected Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZGroupBox DeliveriesGroupBox;
		ZTemplateTabControl OrderLineTabControl;
		ZTabPage FieldsTabPage;
		ZTabPage CustomFieldsTabPage;
		ZTabPage ShippingToleranceTabPage;
		OrderLineToleranceControl QuantityUnderPercentage;
		ZLabel QuantityLabel;
		ZWorkflowTabPage WorkflowTabPage;
		ZButton ShowHideContainers;
		ZCalcEdit JO_ItemPriceBoundCalcEdit;
		ZTextBox JO_DescriptionBoundTextBox;
		ZGroupBox ContainersGroupBox;
		ZCalcEdit JO_QtyReceivedBoundCalcEdit;
		ZDropEdit JO_F3_NKPackTypeBoundDropEdit;
		protected ProcessTemplateCustomFieldsControl CustomFieldsUserControl;
		ZCalcEdit JO_TotalInnerPacksBoundCalcEdit;
		ZCalcEdit JO_QuantityRemainingBoundCalcEdit;
		ZCodeFindBox JO_PartnoBoundFindBox;
		CustomCodeFindBox JO_RH_NKCommodityCodeFindBox;
		ZCalcEdit JO_TotalPriceBoundCalcEdit;
		ZDropEdit JO_LineStatusBoundDropEdit;
		ZCalcEdit JO_QtyInvoicedBoundCalcEdit;
		ZCalcEdit JO_QuantityBoundCalcEdit;
		ZGroupBox RelatedContainersGroupBox;
		ZButton AttachRelatedContainerToDeliveryButton;
		ZGroupBox AttachDeliveryToContainerGroupBox;
		ZCodeFindBox J5_RL_DestinationPortForAttachBoundFindBox;
		ZCodeFindBox J5_PartnoForAttachBoundFindBox;
		ZTextBox J5_OrderNumberForAttachBoundTextBox;
		ZDateEdit LineDropDatezDateEdit;
		Customs.Universal.GUI.TariffFindBox HarmonisedCodeFindBox;

		protected new void InitializeComponent()
		{
			this.components = new Container();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.DeliveriesBoundGrid = new ZGrid();
			this.DeliveriesGroupBox = new ZGroupBox();
			this.OrderLineFormControl = new ZTemplateTabControl();
			this.DetailsTabPage = new ZTabPage();
			this.AttachDeliveryToContainerGroupBox = new ZGroupBox();
			this.J5_OrderNumberForAttachBoundTextBox = new ZTextBox();
			this.J5_RL_DestinationPortForAttachBoundFindBox = new ZCodeFindBox();
			this.J5_PartnoForAttachBoundFindBox = new ZCodeFindBox();
			this.AttachRelatedContainerToDeliveryButton = new ZButton();
			this.RelatedContainersGroupBox = new ZGroupBox();
			this.RelatedDeliveryContainersBoundGrid = new ZGrid();
			this.ContainersGroupBox = new ZGroupBox();
			this.DeliveryContainersBoundGrid = new ZGrid();
			this.OrderLineTabControl = new ZTemplateTabControl();
			this.FieldsTabPage = new ZTabPage();
			this.JO_ShipmentWindowStartBoundDateEdit = new ZDateEdit();
			this.JO_ShipmentWindowEndBoundDateEdit = new ZDateEdit();
			this.JO_LineReferenceTextBox = new ZTextBox();
			this.SerialNumberTextBox = new ZTextBox();
			this.JO_InnerPacksBoundDropEdit = new ZCalcDropEdit();
			this.JO_OuterPacksBoundDropEdit = new ZCalcDropEdit();
			this.JO_ExWorksDateBoundDateEdit = new ZDateEdit();
			this.JO_ConfirmationDateBoundDateEdit = new ZDateEdit();
			this.JO_ConfirmationNumBoundTextBox = new ZTextBox();
			this.JO_AdditionalTermsBoundTextBox = new ZTextBox();
			this.JO_INCOBoundDropEdit = new ZDropEdit();
			this.HarmonisedCodeFindBox = new Customs.Universal.GUI.TariffFindBox();
			this.PartAttrib3TextBox = new ZTextBox();
			this.PartAttrib2TextBox = new ZTextBox();
			this.PartAttrib1TextBox = new ZTextBox();
			this.LineDropDatezDateEdit = new ZDateEdit();
			this.JO_QuantityBoundCalcEdit = new ZCalcEdit();
			this.JO_LineStatusBoundDropEdit = new ZDropEdit();
			this.JO_PartnoBoundFindBox = new ZCodeFindBox();
			this.JO_RH_NKCommodityCodeFindBox = new CustomCodeFindBox();
			this.JO_QuantityRemainingBoundCalcEdit = new ZCalcEdit();
			this.JO_TotalInnerPacksBoundCalcEdit = new ZCalcEdit();
			this.JO_TotalPriceBoundCalcEdit = new ZCalcEdit();
			this.JO_F3_NKPackTypeBoundDropEdit = new ZDropEdit();
			this.JO_QtyReceivedBoundCalcEdit = new ZCalcEdit();
			this.JO_ItemPriceBoundCalcEdit = new ZCalcEdit();
			this.JO_QtyInvoicedBoundCalcEdit = new ZCalcEdit();
			this.JO_DescriptionBoundTextBox = new ZTextBox();
			this.CustomFieldsTabPage = new ZTabPage();
			this.CustomFieldsUserControl = new ProcessTemplateCustomFieldsControl();
			this.OuterPacksGroupBox = new ZGroupBox();
			this.OuterPacksCalcDropEdit = new ZCalcDropEdit();
			this.UnitOfDimensionCalcDropEdit = new ZCalcDropEdit();
			this.WidthCalcEdit = new ZCalcEdit();
			this.LengthCalcEdit = new ZCalcEdit();
			this.VolumeCalcDropEdit = new ZCalcDropEdit();
			this.WeightCalcDropEdit = new ZCalcDropEdit();
			this.ShippingToleranceTabPage = new ZTabPage();
			this.AvailableLateLabel = new ZLabel();
			this.ExWorksDaysLate = new OrderLineToleranceControl();
			this.AvailableEarlyLabel = new ZLabel();
			this.ExWorksDaysEarly = new OrderLineToleranceControl();
			this.ExWorksLabel = new ZLabel();
			this.AllowableOverLabel = new ZLabel();
			this.QuantityOverPercentage = new OrderLineToleranceControl();
			this.AllowableUnderLabel = new ZLabel();
			this.QuantityUnderPercentage = new OrderLineToleranceControl();
			this.QuantityLabel = new ZLabel();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.ShowHideContainers = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			((ISupportInitialize)(this.DeliveriesBoundGrid)).BeginInit();
			this.DeliveriesBoundGrid.SuspendLayout();
			this.DeliveriesGroupBox.SuspendLayout();
			this.OrderLineFormControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.AttachDeliveryToContainerGroupBox.SuspendLayout();
			this.J5_RL_DestinationPortForAttachBoundFindBox.SuspendLayout();
			this.J5_PartnoForAttachBoundFindBox.SuspendLayout();
			this.RelatedContainersGroupBox.SuspendLayout();
			((ISupportInitialize)(this.RelatedDeliveryContainersBoundGrid)).BeginInit();
			this.RelatedDeliveryContainersBoundGrid.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((ISupportInitialize)(this.DeliveryContainersBoundGrid)).BeginInit();
			this.DeliveryContainersBoundGrid.SuspendLayout();
			this.OrderLineTabControl.SuspendLayout();
			this.FieldsTabPage.SuspendLayout();
			this.JO_ShipmentWindowStartBoundDateEdit.SuspendLayout();
			this.JO_ShipmentWindowEndBoundDateEdit.SuspendLayout();
			this.JO_InnerPacksBoundDropEdit.SuspendLayout();
			this.JO_OuterPacksBoundDropEdit.SuspendLayout();
			this.JO_ExWorksDateBoundDateEdit.SuspendLayout();
			this.JO_ConfirmationDateBoundDateEdit.SuspendLayout();
			this.JO_INCOBoundDropEdit.SuspendLayout();
			this.HarmonisedCodeFindBox.SuspendLayout();
			this.LineDropDatezDateEdit.SuspendLayout();
			this.JO_LineStatusBoundDropEdit.SuspendLayout();
			this.JO_PartnoBoundFindBox.SuspendLayout();
			this.JO_RH_NKCommodityCodeFindBox.SuspendLayout();
			this.JO_F3_NKPackTypeBoundDropEdit.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CustomFieldsUserControl.SuspendLayout();
			this.OuterPacksGroupBox.SuspendLayout();
			this.OuterPacksCalcDropEdit.SuspendLayout();
			this.UnitOfDimensionCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.ShippingToleranceTabPage.SuspendLayout();
			this.ExWorksDaysLate.SuspendLayout();
			this.ExWorksDaysEarly.SuspendLayout();
			this.QuantityOverPercentage.SuspendLayout();
			this.QuantityUnderPercentage.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 569, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 22, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(460);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(461);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrderLine);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 518, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 29, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// DeliveriesBoundGrid
			// 
			this.DeliveriesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeliveriesBoundGrid, "DeliveriesAlwaysEditable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_RL_NKDestinationPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_OA_NKDeliveryPoint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_DeliverPointAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_DeliverPointAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_Allocated)));
			this.DeliveriesBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "J4_RL_NKDestinationPort";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select the Destination Port";
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Select the Destination Port";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "J4_OA_NKDeliveryPoint";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|e7843852-20ef-48d5-a522-1f3278bf0585", "Deliver Point");
			zDropEditColumnStyleInfo1.ToolTip = "Select the Deliver Point";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "J4_DeliverPointAddress1";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|e7843852-20ef-48d5-a522-1f3278bf0585", "Deliver Point");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "J4_DeliverPointAddress2";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|e7843852-20ef-48d5-a522-1f3278bf0585", "Deliver Point");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "J4_Allocated";
			zCalcEditColumnStyleInfo1.ToolTip = "Enter the Allocated Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DeliveriesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DeliveriesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DeliveriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeliveriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DeliveriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DeliveriesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveriesBoundGrid.GridId = "fdd88020-87a7-4886-9090-531296cccd03";
			this.DeliveriesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveriesBoundGrid.LayoutKey = "DeliveriesBoundGrid";
			this.DeliveriesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.DeliveriesBoundGrid.Name = "DeliveriesBoundGrid";
			this.DeliveriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 18, true);
			this.DeliveriesBoundGrid.TabIndex = 0;
			// 
			// DeliveriesGroupBox
			// 
			this.DeliveriesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeliveriesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|e2f844c0-4944-44af-90c7-d19892d9e7ee", "Deliveries");
			this.DeliveriesGroupBox.Controls.Add(this.DeliveriesBoundGrid);
			this.DeliveriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 223, true);
			this.DeliveriesGroupBox.Name = "DeliveriesGroupBox";
			this.DeliveriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 43, true);
			this.DeliveriesGroupBox.TabIndex = 1;
			this.DeliveriesGroupBox.TabStop = false;
			// 
			// OrderLineFormControl
			// 
			this.OrderLineFormControl.Controls.Add(this.DetailsTabPage);
			this.OrderLineFormControl.Controls.Add(this.WorkflowTabPage);
			this.OrderLineFormControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OrderLineFormControl.Name = "OrderLineFormControl";
			this.OrderLineFormControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 506, true);
			this.OrderLineFormControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineFormControl|ed6d5dfc-4927-4520-b0a2-6168a3c0b8c1", "Order Line");
			this.DetailsTabPage.Controls.Add(this.AttachDeliveryToContainerGroupBox);
			this.DetailsTabPage.Controls.Add(this.RelatedContainersGroupBox);
			this.DetailsTabPage.Controls.Add(this.ContainersGroupBox);
			this.DetailsTabPage.Controls.Add(this.OrderLineTabControl);
			this.DetailsTabPage.Controls.Add(this.DeliveriesGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 473, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// AttachDeliveryToContainerGroupBox
			// 
			this.AttachDeliveryToContainerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachDeliveryToContainerGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|001e0edb-234c-46af-b632-a43ed703ccdb", "Attach Container To Delivery");
			this.AttachDeliveryToContainerGroupBox.Controls.Add(this.J5_OrderNumberForAttachBoundTextBox);
			this.AttachDeliveryToContainerGroupBox.Controls.Add(this.J5_RL_DestinationPortForAttachBoundFindBox);
			this.AttachDeliveryToContainerGroupBox.Controls.Add(this.J5_PartnoForAttachBoundFindBox);
			this.AttachDeliveryToContainerGroupBox.Controls.Add(this.AttachRelatedContainerToDeliveryButton);
			this.AttachDeliveryToContainerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 345, true);
			this.AttachDeliveryToContainerGroupBox.Name = "AttachDeliveryToContainerGroupBox";
			this.AttachDeliveryToContainerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 122, true);
			this.AttachDeliveryToContainerGroupBox.TabIndex = 4;
			this.AttachDeliveryToContainerGroupBox.TabStop = false;
			// 
			// J5_OrderNumberForAttachBoundTextBox
			// 
			this.J5_OrderNumberForAttachBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.J5_OrderNumberForAttachBoundTextBox, "DeliveriesAlwaysEditable.Containers.J5_OrderNumberForAttach");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_OrderNumberForAttach)));
			this.J5_OrderNumberForAttachBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|c42c4229-3e59-42df-bd44-dc12f68700cc", "Order Number");
			this.J5_OrderNumberForAttachBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 15, true);
			this.J5_OrderNumberForAttachBoundTextBox.Name = "J5_OrderNumberForAttachBoundTextBox";
			this.J5_OrderNumberForAttachBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 26, true);
			this.J5_OrderNumberForAttachBoundTextBox.TabIndex = 0;
			// 
			// J5_RL_DestinationPortForAttachBoundFindBox
			// 
			this.J5_RL_DestinationPortForAttachBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.J5_RL_DestinationPortForAttachBoundFindBox, "DeliveriesAlwaysEditable.Containers.J5_RL_DestinationPortForAttach");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_RL_DestinationPortForAttach)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).J4_RL_NKDestinationPort_List)));
			this.J5_RL_DestinationPortForAttachBoundFindBox.BindToList = "DeliveriesAlwaysEditable.J4_RL_NKDestinationPort_List";
			this.J5_RL_DestinationPortForAttachBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|64a7c683-8dfb-4b1e-9937-c414c155a54d", "Destination Port");
			this.J5_RL_DestinationPortForAttachBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 59, true);
			this.J5_RL_DestinationPortForAttachBoundFindBox.Name = "J5_RL_DestinationPortForAttachBoundFindBox";
			this.J5_RL_DestinationPortForAttachBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.J5_RL_DestinationPortForAttachBoundFindBox.ParentType = null;
			this.J5_RL_DestinationPortForAttachBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 26, true);
			this.J5_RL_DestinationPortForAttachBoundFindBox.TabIndex = 2;
			// 
			// J5_PartnoForAttachBoundFindBox
			// 
			this.J5_PartnoForAttachBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.J5_PartnoForAttachBoundFindBox, "DeliveriesAlwaysEditable.Containers.J5_PartnoForAttach");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_PartnoForAttach)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderLine)(null)).JO_Partno_List)));
			this.J5_PartnoForAttachBoundFindBox.BindToList = "JO_Partno_List";
			this.J5_PartnoForAttachBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|7f1eaf02-8e73-4d0e-b3ab-f0d054269781", "Part #", "Part Number", "The part number.");
			this.J5_PartnoForAttachBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 37, true);
			this.J5_PartnoForAttachBoundFindBox.Name = "J5_PartnoForAttachBoundFindBox";
			this.J5_PartnoForAttachBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.J5_PartnoForAttachBoundFindBox.ParentType = null;
			this.J5_PartnoForAttachBoundFindBox.ShowDescriptionBox = false;
			this.J5_PartnoForAttachBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 26, true);
			this.J5_PartnoForAttachBoundFindBox.TabIndex = 1;
			// 
			// AttachRelatedContainerToDeliveryButton
			// 
			this.AttachRelatedContainerToDeliveryButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|98d991cc-2bf7-461f-a50b-df0b9cdac0a8", "Attach To Delivery");
			this.AttachRelatedContainerToDeliveryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 82, true);
			this.AttachRelatedContainerToDeliveryButton.Name = "AttachRelatedContainerToDeliveryButton";
			this.AttachRelatedContainerToDeliveryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 22, true);
			this.AttachRelatedContainerToDeliveryButton.TabIndex = 3;
			this.AttachRelatedContainerToDeliveryButton.ToolTipCaption = null;
			this.AttachRelatedContainerToDeliveryButton.Click += new EventHandler(this.OnAttachToDelivery_Click);
			// 
			// RelatedContainersGroupBox
			// 
			this.RelatedContainersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedContainersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|9f96b6a8-4495-4b2a-8f8b-b9caf589100d", "Container Details on other Delivery Lines");
			this.RelatedContainersGroupBox.Controls.Add(this.RelatedDeliveryContainersBoundGrid);
			this.RelatedContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 345, true);
			this.RelatedContainersGroupBox.Name = "RelatedContainersGroupBox";
			this.RelatedContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 124, true);
			this.RelatedContainersGroupBox.TabIndex = 3;
			this.RelatedContainersGroupBox.TabStop = false;
			// 
			// RelatedDeliveryContainersBoundGrid
			// 
			this.RelatedDeliveryContainersBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedDeliveryContainersBoundGrid, "DeliveriesAlwaysEditable.Containers.RelatedContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_JD_OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_JO_Partno)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_J4_RL_NKDestinationPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_QuantityInvoiced)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_QuantityInStore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_SightedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_InstoreDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_ContainerSeal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_PackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).RelatedContainers)).SyncRoot)).J5_WeightUQ)));
			this.RelatedDeliveryContainersBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "J5_JD_OrderNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "J5_JO_Partno";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "J5_J4_RL_NKDestinationPort";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "J5_QuantityInvoiced";
			zCalcEditColumnStyleInfo2.ToolTip = "Enter the Quantity Invoiced";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "J5_QuantityInStore";
			zCalcEditColumnStyleInfo3.ToolTip = "Enter the Quantity In Store";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo1.ColumnName = "J5_SightedDate";
			zDateEditColumnStyleInfo1.ToolTip = "Enter the Date Sighted";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "J5_InstoreDate";
			zDateEditColumnStyleInfo2.ToolTip = "Enter the In Store Date";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "J5_ContainerSeal";
			zTextBoxColumnStyleInfo5.ToolTip = "Enter the Container Seal #";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "J5_RC_NKContainerType";
			zCodeFindBoxColumnStyleInfo3.ToolTip = "Enter the Container Type";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "J5_ETA";
			zDateEditColumnStyleInfo3.ToolTip = "Enter the Estimated Time of Arrival";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo4.ColumnName = "J5_ETD";
			zDateEditColumnStyleInfo4.ToolTip = "Enter the Estimated Time of Departure";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.ColumnName = "J5_MasterBill";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "J5_PackCount";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "J5_F3_NKPackType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "J5_Volume";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "J5_VolumeUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "J5_Weight";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "J5_WeightUQ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.RelatedDeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RelatedDeliveryContainersBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedDeliveryContainersBoundGrid.GridId = "0adc55e8-60bf-446d-a1ea-a99906a664ca";
			this.RelatedDeliveryContainersBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedDeliveryContainersBoundGrid.LayoutKey = "DeliveriesBoundGrid";
			this.RelatedDeliveryContainersBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.RelatedDeliveryContainersBoundGrid.Name = "RelatedDeliveryContainersBoundGrid";
			this.RelatedDeliveryContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 99, true);
			this.RelatedDeliveryContainersBoundGrid.TabIndex = 0;
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ContainersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|6d33fabd-9e06-4fba-bf63-c83116aaa04c", "Containers");
			this.ContainersGroupBox.Controls.Add(this.DeliveryContainersBoundGrid);
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 255, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 79, true);
			this.ContainersGroupBox.TabIndex = 2;
			this.ContainersGroupBox.TabStop = false;
			// 
			// DeliveryContainersBoundGrid
			// 
			this.DeliveryContainersBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeliveryContainersBoundGrid, "DeliveriesAlwaysEditable.Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_QuantityInvoiced)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_QuantityInStore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_SightedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_InstoreDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_ContainerSeal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_RV_NKArrivalVessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_Voyage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_PackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLineDeliverContainer)(((System.Collections.IList)(((OrderLineDelivery)(((System.Collections.IList)(((OrderLine)(null)).DeliveriesAlwaysEditable)).SyncRoot)).Containers)).SyncRoot)).J5_WeightUQ)));
			this.DeliveryContainersBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "J5_QuantityInvoiced";
			zCalcEditColumnStyleInfo7.ToolTip = "Enter the Quantity Invoiced";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "J5_QuantityInStore";
			zCalcEditColumnStyleInfo8.ToolTip = "Enter the Quantity In Store";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo5.ColumnName = "J5_SightedDate";
			zDateEditColumnStyleInfo5.ToolTip = "Enter the Date Sighted";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo6.ColumnName = "J5_InstoreDate";
			zDateEditColumnStyleInfo6.ToolTip = "Enter the In Store Date";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "J5_ContainerNum";
			zDropEditColumnStyleInfo5.IsMandatory = true;
			zDropEditColumnStyleInfo5.ToolTip = "Enter the Container Number";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "J5_ContainerSeal";
			zTextBoxColumnStyleInfo7.ToolTip = "Enter the Container Seal #";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "J5_RC_NKContainerType";
			zCodeFindBoxColumnStyleInfo4.ToolTip = "Enter the Container Type";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo5.ColumnName = "J5_RV_NKArrivalVessel";
			zCodeFindBoxColumnStyleInfo5.PopupCaption = "Select the Arrival Vessel";
			zCodeFindBoxColumnStyleInfo5.ToolTip = "Select the Arrival Vessel";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "J5_Voyage";
			zDropEditColumnStyleInfo6.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo6.ToolTip = "Select the Vessel/Voyage";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.ColumnName = "J5_ETA";
			zDateEditColumnStyleInfo7.ToolTip = "Enter the Estimated Time of Arrival";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo8.ColumnName = "J5_ETD";
			zDateEditColumnStyleInfo8.ToolTip = "Enter the Estimated Time of Departure";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.ColumnName = "J5_MasterBill";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "J5_PackCount";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "J5_F3_NKPackType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "J5_Volume";
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.ColumnName = "J5_VolumeUQ";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "J5_Weight";
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.ColumnName = "J5_WeightUQ";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.DeliveryContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.DeliveryContainersBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryContainersBoundGrid.GridId = "bdfb6fb4-4bed-4b38-aaaa-362f50eed71c";
			this.DeliveryContainersBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveryContainersBoundGrid.LayoutKey = "DeliveriesBoundGrid";
			this.DeliveryContainersBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.DeliveryContainersBoundGrid.Name = "DeliveryContainersBoundGrid";
			this.DeliveryContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 54, true);
			this.DeliveryContainersBoundGrid.TabIndex = 0;
			// 
			// OrderLineTabControl
			// 
			this.OrderLineTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrderLineTabControl.Controls.Add(this.FieldsTabPage);
			this.OrderLineTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.OrderLineTabControl.Controls.Add(this.ShippingToleranceTabPage);
			this.OrderLineTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OrderLineTabControl.Name = "OrderLineTabControl";
			this.OrderLineTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(907, 210, true);
			this.OrderLineTabControl.TabIndex = 0;
			// 
			// FieldsTabPage
			// 
			this.FieldsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|f67c4976-c04e-4785-9f63-54d11b5790ce", "Details");
			this.FieldsTabPage.Controls.Add(this.JO_ShipmentWindowStartBoundDateEdit);
			this.FieldsTabPage.Controls.Add(this.JO_ShipmentWindowEndBoundDateEdit);
			this.FieldsTabPage.Controls.Add(this.JO_LineReferenceTextBox);
			this.FieldsTabPage.Controls.Add(this.SerialNumberTextBox);
			this.FieldsTabPage.Controls.Add(this.JO_InnerPacksBoundDropEdit);
			this.FieldsTabPage.Controls.Add(this.JO_OuterPacksBoundDropEdit);
			this.FieldsTabPage.Controls.Add(this.JO_ExWorksDateBoundDateEdit);
			this.FieldsTabPage.Controls.Add(this.JO_ConfirmationDateBoundDateEdit);
			this.FieldsTabPage.Controls.Add(this.JO_ConfirmationNumBoundTextBox);
			this.FieldsTabPage.Controls.Add(this.JO_AdditionalTermsBoundTextBox);
			this.FieldsTabPage.Controls.Add(this.JO_INCOBoundDropEdit);
			this.FieldsTabPage.Controls.Add(this.HarmonisedCodeFindBox);
			this.FieldsTabPage.Controls.Add(this.PartAttrib3TextBox);
			this.FieldsTabPage.Controls.Add(this.PartAttrib2TextBox);
			this.FieldsTabPage.Controls.Add(this.PartAttrib1TextBox);
			this.FieldsTabPage.Controls.Add(this.LineDropDatezDateEdit);
			this.FieldsTabPage.Controls.Add(this.JO_QuantityBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_LineStatusBoundDropEdit);
			this.FieldsTabPage.Controls.Add(this.JO_PartnoBoundFindBox);
			this.FieldsTabPage.Controls.Add(this.JO_RH_NKCommodityCodeFindBox);
			this.FieldsTabPage.Controls.Add(this.JO_QuantityRemainingBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_TotalInnerPacksBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_TotalPriceBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_F3_NKPackTypeBoundDropEdit);
			this.FieldsTabPage.Controls.Add(this.JO_QtyReceivedBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_ItemPriceBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_QtyInvoicedBoundCalcEdit);
			this.FieldsTabPage.Controls.Add(this.JO_DescriptionBoundTextBox);
			this.FieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.FieldsTabPage.Name = "FieldsTabPage";
			this.FieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 185, true);
			this.FieldsTabPage.TabIndex = 0;
			// 
			// JO_ShipmentWindowStartBoundDateEdit
			// 
			this.JO_ShipmentWindowStartBoundDateEdit.AllowDrop = true;
			this.JO_ShipmentWindowStartBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JO_ShipmentWindowStartBoundDateEdit, "JO_ShipmentWindowStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_ShipmentWindowStart)));
			this.JO_ShipmentWindowStartBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 120, true);
			this.JO_ShipmentWindowStartBoundDateEdit.Name = "JO_ShipmentWindowStartBoundDateEdit";
			this.JO_ShipmentWindowStartBoundDateEdit.TabIndex = 18;
			// 
			// JO_ShipmentWindowEndBoundDateEdit
			// 
			this.JO_ShipmentWindowEndBoundDateEdit.AllowDrop = true;
			this.JO_ShipmentWindowEndBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JO_ShipmentWindowEndBoundDateEdit, "JO_ShipmentWindowEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_ShipmentWindowEnd)));
			this.JO_ShipmentWindowEndBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 120, true);
			this.JO_ShipmentWindowEndBoundDateEdit.Name = "JO_ShipmentWindowEndBoundDateEdit";
			this.JO_ShipmentWindowEndBoundDateEdit.TabIndex = 19;
			// 
			// JO_LineReferenceTextBox
			// 
			this.JO_LineReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_LineReferenceTextBox, "JO_LineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_LineReference)));
			this.JO_LineReferenceTextBox.CaptionResourceString = null;
			this.JO_LineReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JO_LineReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			this.JO_LineReferenceTextBox.Name = "JO_LineReferenceTextBox";
			this.JO_LineReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 26, true);
			this.JO_LineReferenceTextBox.TabIndex = 0;
			// 
			// SerialNumberTextBox
			// 
			this.SerialNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "JO_SerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_SerialNumber)));
			this.SerialNumberTextBox.CaptionResourceString = null;
			this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 121, true);
			this.SerialNumberTextBox.Name = "SerialNumberTextBox";
			this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 26, true);
			this.SerialNumberTextBox.TabIndex = 21;
			// 
			// JO_InnerPacksBoundDropEdit
			// 
			this.JO_InnerPacksBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_InnerPacksBoundDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_InnerPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_InnerPacksUQ)));
			this.JO_InnerPacksBoundDropEdit.BindToAmount = "JO_InnerPacks";
			this.JO_InnerPacksBoundDropEdit.BindToUnit = "JO_InnerPacksUQ";
			this.JO_InnerPacksBoundDropEdit.Decimals = 0;
			this.JO_InnerPacksBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 77, true);
			this.JO_InnerPacksBoundDropEdit.Name = "JO_InnerPacksBoundDropEdit";
			this.JO_InnerPacksBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 26, true);
			this.JO_InnerPacksBoundDropEdit.TabIndex = 8;
			this.JO_InnerPacksBoundDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JO_OuterPacksBoundDropEdit
			// 
			this.JO_OuterPacksBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_OuterPacksBoundDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_OuterPacksUQ)));
			this.JO_OuterPacksBoundDropEdit.BindToAmount = "JO_OuterPacks";
			this.JO_OuterPacksBoundDropEdit.BindToUnit = "JO_OuterPacksUQ";
			this.JO_OuterPacksBoundDropEdit.Decimals = 0;
			this.JO_OuterPacksBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 54, true);
			this.JO_OuterPacksBoundDropEdit.Name = "JO_OuterPacksBoundDropEdit";
			this.JO_OuterPacksBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 26, true);
			this.JO_OuterPacksBoundDropEdit.TabIndex = 4;
			this.JO_OuterPacksBoundDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JO_ExWorksDateBoundDateEdit
			// 
			this.JO_ExWorksDateBoundDateEdit.AllowDrop = true;
			this.JO_ExWorksDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JO_ExWorksDateBoundDateEdit, "JO_ExWorksDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_ExWorksDate)));
			this.JO_ExWorksDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 143, true);
			this.JO_ExWorksDateBoundDateEdit.Name = "JO_ExWorksDateBoundDateEdit";
			this.JO_ExWorksDateBoundDateEdit.TabIndex = 24;
			// 
			// JO_ConfirmationDateBoundDateEdit
			// 
			this.JO_ConfirmationDateBoundDateEdit.AllowDrop = true;
			this.JO_ConfirmationDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JO_ConfirmationDateBoundDateEdit, "JO_ConfirmationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_ConfirmationDate)));
			this.JO_ConfirmationDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 143, true);
			this.JO_ConfirmationDateBoundDateEdit.Name = "JO_ConfirmationDateBoundDateEdit";
			this.JO_ConfirmationDateBoundDateEdit.TabIndex = 23;
			// 
			// JO_ConfirmationNumBoundTextBox
			// 
			this.JO_ConfirmationNumBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_ConfirmationNumBoundTextBox, "JO_ConfirmationNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_ConfirmationNum)));
			this.JO_ConfirmationNumBoundTextBox.CaptionResourceString = null;
			this.JO_ConfirmationNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 143, true);
			this.JO_ConfirmationNumBoundTextBox.Name = "JO_ConfirmationNumBoundTextBox";
			this.JO_ConfirmationNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_ConfirmationNumBoundTextBox.TabIndex = 22;
			// 
			// JO_AdditionalTermsBoundTextBox
			// 
			this.JO_AdditionalTermsBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_AdditionalTermsBoundTextBox, "JO_AdditionalTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_AdditionalTerms)));
			this.JO_AdditionalTermsBoundTextBox.CaptionResourceString = null;
			this.JO_AdditionalTermsBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JO_AdditionalTermsBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 121, true);
			this.JO_AdditionalTermsBoundTextBox.Name = "JO_AdditionalTermsBoundTextBox";
			this.JO_AdditionalTermsBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 26, true);
			this.JO_AdditionalTermsBoundTextBox.TabIndex = 20;
			// 
			// JO_INCOBoundDropEdit
			// 
			this.JO_INCOBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_INCOBoundDropEdit, "JO_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_INCO)));
			this.JO_INCOBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 143, true);
			this.JO_INCOBoundDropEdit.Name = "JO_INCOBoundDropEdit";
			this.JO_INCOBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 26, true);
			this.JO_INCOBoundDropEdit.TabIndex = 25;
			// 
			// HarmonisedCodeFindBox
			// 
			this.HarmonisedCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HarmonisedCodeFindBox, "JO_HSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_HSCode)));
			this.HarmonisedCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("50b46db4-e1ce-45fe-b3d6-ba870b3aea4d", "H.S. Code");
			this.HarmonisedCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(964, 8, true);
			this.HarmonisedCodeFindBox.Name = "HarmonisedCodeFindBox";
			this.HarmonisedCodeFindBox.ShouldResize = true;
			this.HarmonisedCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 26, true);
			this.HarmonisedCodeFindBox.TabIndex = 27;
			this.HarmonisedCodeFindBox.TariffType = "HSN";
			// 
			// PartAttrib3TextBox
			// 
			this.PartAttrib3TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PartAttrib3TextBox, "JO_PartAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_PartAttrib3)));
			this.PartAttrib3TextBox.CaptionResourceString = null;
			this.PartAttrib3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 99, true);
			this.PartAttrib3TextBox.Name = "PartAttrib3TextBox";
			this.PartAttrib3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 26, true);
			this.PartAttrib3TextBox.TabIndex = 17;
			// 
			// PartAttrib2TextBox
			// 
			this.PartAttrib2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PartAttrib2TextBox, "JO_PartAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_PartAttrib2)));
			this.PartAttrib2TextBox.CaptionResourceString = null;
			this.PartAttrib2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 77, true);
			this.PartAttrib2TextBox.Name = "PartAttrib2TextBox";
			this.PartAttrib2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 26, true);
			this.PartAttrib2TextBox.TabIndex = 12;
			// 
			// PartAttrib1TextBox
			// 
			this.PartAttrib1TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PartAttrib1TextBox, "JO_PartAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_PartAttrib1)));
			this.PartAttrib1TextBox.CaptionResourceString = null;
			this.PartAttrib1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 54, true);
			this.PartAttrib1TextBox.Name = "PartAttrib1TextBox";
			this.PartAttrib1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 26, true);
			this.PartAttrib1TextBox.TabIndex = 7;
			// 
			// LineDropDatezDateEdit
			// 
			this.LineDropDatezDateEdit.AllowDrop = true;
			this.LineDropDatezDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LineDropDatezDateEdit, "JO_LineDropDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_LineDropDate)));
			this.LineDropDatezDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 99, true);
			this.LineDropDatezDateEdit.Name = "LineDropDatezDateEdit";
			this.LineDropDatezDateEdit.TabIndex = 16;
			// 
			// JO_QuantityBoundCalcEdit
			// 
			this.JO_QuantityBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_QuantityBoundCalcEdit, "JO_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_Quantity)));
			this.JO_QuantityBoundCalcEdit.CaptionResourceString = null;
			this.JO_QuantityBoundCalcEdit.DecimalPlaces = 5;
			this.JO_QuantityBoundCalcEdit.Decimals = 5;
			this.JO_QuantityBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 54, true);
			this.JO_QuantityBoundCalcEdit.Name = "JO_QuantityBoundCalcEdit";
			this.JO_QuantityBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_QuantityBoundCalcEdit.TabIndex = 5;
			this.JO_QuantityBoundCalcEdit.Text = "0.00000";
			this.JO_QuantityBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_LineStatusBoundDropEdit
			// 
			this.JO_LineStatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_LineStatusBoundDropEdit, "JO_LineStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_LineStatus)));
			this.JO_LineStatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 32, true);
			this.JO_LineStatusBoundDropEdit.Name = "JO_LineStatusBoundDropEdit";
			this.JO_LineStatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 26, true);
			this.JO_LineStatusBoundDropEdit.TabIndex = 2;
			// 
			// JO_PartnoBoundFindBox
			// 
			this.JO_PartnoBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_PartnoBoundFindBox, "JO_Partno");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_Partno)));
			this.JO_PartnoBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 32, true);
			this.JO_PartnoBoundFindBox.Name = "JO_PartnoBoundFindBox";
			this.JO_PartnoBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JO_PartnoBoundFindBox.ParentType = null;
			this.JO_PartnoBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 26, true);
			this.JO_PartnoBoundFindBox.TabIndex = 3;
			// 
			// JO_RH_NKCommodityCodeFindBox;
			//
			this.JO_RH_NKCommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_RH_NKCommodityCodeFindBox, "JO_RH_NKCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((OrderLine)(null)).JO_RH_NKCommodityCode)));
			this.JO_RH_NKCommodityCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("50b46db4-e1ce-45fe-b3d6-ba870b3aea5d", "Commodity");
			this.JO_RH_NKCommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(964, 34, true);
			this.JO_RH_NKCommodityCodeFindBox.Name = "JO_RH_NKCommodityCodeFindBox";
			this.JO_RH_NKCommodityCodeFindBox.PreBoundMaxLength = 4;
			this.JO_RH_NKCommodityCodeFindBox.ShouldResize = true;
			this.JO_RH_NKCommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 26, true);
			this.JO_RH_NKCommodityCodeFindBox.TabIndex = 26;
			// 
			// JO_QuantityRemainingBoundCalcEdit
			// 
			this.JO_QuantityRemainingBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_QuantityRemainingBoundCalcEdit, "JO_QuantityRemaining");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_QuantityRemaining)));
			this.JO_QuantityRemainingBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|03e168fd-ab00-447b-b652-312f7086928e", "Qty Remaining");
			this.JO_QuantityRemainingBoundCalcEdit.DecimalPlaces = 5;
			this.JO_QuantityRemainingBoundCalcEdit.Decimals = 5;
			this.JO_QuantityRemainingBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 99, true);
			this.JO_QuantityRemainingBoundCalcEdit.Name = "JO_QuantityRemainingBoundCalcEdit";
			this.JO_QuantityRemainingBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_QuantityRemainingBoundCalcEdit.TabIndex = 15;
			this.JO_QuantityRemainingBoundCalcEdit.Text = "0.00000";
			this.JO_QuantityRemainingBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_TotalInnerPacksBoundCalcEdit
			// 
			this.JO_TotalInnerPacksBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_TotalInnerPacksBoundCalcEdit, "JO_TotalInnerPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_TotalInnerPacks)));
			this.JO_TotalInnerPacksBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|52d0c070-4575-4c23-9cba-bab595b5f901", "Total Inner Packs");
			this.JO_TotalInnerPacksBoundCalcEdit.DecimalPlaces = 0;
			this.JO_TotalInnerPacksBoundCalcEdit.Decimals = 0;
			this.JO_TotalInnerPacksBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 99, true);
			this.JO_TotalInnerPacksBoundCalcEdit.Name = "JO_TotalInnerPacksBoundCalcEdit";
			this.JO_TotalInnerPacksBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_TotalInnerPacksBoundCalcEdit.TabIndex = 13;
			this.JO_TotalInnerPacksBoundCalcEdit.Text = "0";
			this.JO_TotalInnerPacksBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_TotalPriceBoundCalcEdit
			// 
			this.JO_TotalPriceBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_TotalPriceBoundCalcEdit, "JO_LinePrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_LinePrice)));
			this.JO_TotalPriceBoundCalcEdit.CaptionResourceString = null;
			this.JO_TotalPriceBoundCalcEdit.DecimalPlaces = 2;
			this.JO_TotalPriceBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 77, true);
			this.JO_TotalPriceBoundCalcEdit.Name = "JO_TotalPriceBoundCalcEdit";
			this.JO_TotalPriceBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 26, true);
			this.JO_TotalPriceBoundCalcEdit.TabIndex = 11;
			this.JO_TotalPriceBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_F3_NKPackTypeBoundDropEdit
			// 
			this.JO_F3_NKPackTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JO_F3_NKPackTypeBoundDropEdit, "JO_F3_NKPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderLine)(null)).JO_F3_NKPackType)));
			this.JO_F3_NKPackTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 55, true);
			this.JO_F3_NKPackTypeBoundDropEdit.Name = "JO_F3_NKPackTypeBoundDropEdit";
			this.JO_F3_NKPackTypeBoundDropEdit.PreBoundMaxLength = 2;
			this.JO_F3_NKPackTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 26, true);
			this.JO_F3_NKPackTypeBoundDropEdit.TabIndex = 6;
			// 
			// JO_QtyReceivedBoundCalcEdit
			// 
			this.JO_QtyReceivedBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_QtyReceivedBoundCalcEdit, "JO_QtyReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_QtyReceived)));
			this.JO_QtyReceivedBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|061ef809-4260-497f-b596-d63e7713c6f7", "Qty Received:");
			this.JO_QtyReceivedBoundCalcEdit.DecimalPlaces = 5;
			this.JO_QtyReceivedBoundCalcEdit.Decimals = 5;
			this.JO_QtyReceivedBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 99, true);
			this.JO_QtyReceivedBoundCalcEdit.Name = "JO_QtyReceivedBoundCalcEdit";
			this.JO_QtyReceivedBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_QtyReceivedBoundCalcEdit.TabIndex = 14;
			this.JO_QtyReceivedBoundCalcEdit.Text = "0.00000";
			this.JO_QtyReceivedBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_ItemPriceBoundCalcEdit
			// 
			this.JO_ItemPriceBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_ItemPriceBoundCalcEdit, "JO_ItemPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_ItemPrice)));
			this.JO_ItemPriceBoundCalcEdit.CaptionResourceString = null;
			this.JO_ItemPriceBoundCalcEdit.DecimalPlaces = 4;
			this.JO_ItemPriceBoundCalcEdit.Decimals = 4;
			this.JO_ItemPriceBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 77, true);
			this.JO_ItemPriceBoundCalcEdit.Name = "JO_ItemPriceBoundCalcEdit";
			this.JO_ItemPriceBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_ItemPriceBoundCalcEdit.TabIndex = 10;
			this.JO_ItemPriceBoundCalcEdit.Text = "0.0000";
			this.JO_ItemPriceBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_QtyInvoicedBoundCalcEdit
			// 
			this.JO_QtyInvoicedBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_QtyInvoicedBoundCalcEdit, "JO_QtyInvoiced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_QtyInvoiced)));
			this.JO_QtyInvoicedBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|cd577431-1f91-4244-98cf-5a845b7934bb", "Qty Invoiced:");
			this.JO_QtyInvoicedBoundCalcEdit.DecimalPlaces = 5;
			this.JO_QtyInvoicedBoundCalcEdit.Decimals = 5;
			this.JO_QtyInvoicedBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 77, true);
			this.JO_QtyInvoicedBoundCalcEdit.Name = "JO_QtyInvoicedBoundCalcEdit";
			this.JO_QtyInvoicedBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 26, true);
			this.JO_QtyInvoicedBoundCalcEdit.TabIndex = 9;
			this.JO_QtyInvoicedBoundCalcEdit.Text = "0.00000";
			this.JO_QtyInvoicedBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JO_DescriptionBoundTextBox
			// 
			this.JO_DescriptionBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JO_DescriptionBoundTextBox, "JO_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_Description)));
			this.JO_DescriptionBoundTextBox.CaptionResourceString = null;
			this.JO_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JO_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 8, true);
			this.JO_DescriptionBoundTextBox.Name = "JO_DescriptionBoundTextBox";
			this.JO_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 26, true);
			this.JO_DescriptionBoundTextBox.TabIndex = 1;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|7c276817-51c4-4448-b9e5-95f6e34d3d63", "Additional Details");
			this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsUserControl);
			this.CustomFieldsTabPage.Controls.Add(this.OuterPacksGroupBox);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 163, true);
			this.CustomFieldsTabPage.TabIndex = 1;
			// 
			// CustomFieldsUserControl
			// 
			this.CustomFieldsUserControl.AllowDrop = true;
			this.CustomFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 23, true);
			this.CustomFieldsUserControl.Name = "CustomFieldsUserControl";
			this.CustomFieldsUserControl.NothingSetupMessageLabelText = "To make use of custom fields, please set up Order Line custom fields in Workflow " +
	"Manager or on the Organization record.";
			this.CustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 140, true);
			this.CustomFieldsUserControl.TabIndex = 0;
			// 
			// OuterPacksGroupBox
			// 
			this.OuterPacksGroupBox.Controls.Add(this.OuterPacksCalcDropEdit);
			this.OuterPacksGroupBox.Controls.Add(this.UnitOfDimensionCalcDropEdit);
			this.OuterPacksGroupBox.Controls.Add(this.WidthCalcEdit);
			this.OuterPacksGroupBox.Controls.Add(this.LengthCalcEdit);
			this.OuterPacksGroupBox.Controls.Add(this.VolumeCalcDropEdit);
			this.OuterPacksGroupBox.Controls.Add(this.WeightCalcDropEdit);
			this.OuterPacksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OuterPacksGroupBox.Name = "OuterPacksGroupBox";
			this.OuterPacksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 166, true);
			this.OuterPacksGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("dfe40fcb-dcde-4ab0-96d6-7d22b18977bd", "Outer Packs");
			this.OuterPacksGroupBox.TabIndex = 1;
			this.OuterPacksGroupBox.TabStop = false;
			// 
			// OuterPacksCalcDropEdit
			// 
			this.OuterPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OuterPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_OuterPacksUQ)));
			this.OuterPacksCalcDropEdit.BindToAmount = "JO_OuterPacks";
			this.OuterPacksCalcDropEdit.BindToUnit = "JO_OuterPacksUQ";
			this.OuterPacksCalcDropEdit.Decimals = 0;
			this.OuterPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 19, true);
			this.OuterPacksCalcDropEdit.Name = "OuterPacksCalcDropEdit";
			this.OuterPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.OuterPacksCalcDropEdit.TabIndex = 0;
			// 
			// UnitOfDimensionCalcDropEdit
			// 
			this.UnitOfDimensionCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitOfDimensionCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OuterPackHeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_OuterPackUnitOfDimension)));
			this.UnitOfDimensionCalcDropEdit.BindToAmount = "JO_OuterPackHeight";
			this.UnitOfDimensionCalcDropEdit.BindToUnit = "JO_OuterPackUnitOfDimension";
			this.UnitOfDimensionCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 146, true);
			this.UnitOfDimensionCalcDropEdit.Name = "UnitOfDimensionCalcDropEdit";
			this.UnitOfDimensionCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.UnitOfDimensionCalcDropEdit.TabIndex = 5;
			// 
			// WidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WidthCalcEdit, "JO_OuterPackWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OuterPackWidth)));
			this.WidthCalcEdit.CaptionResourceString = null;
			this.WidthCalcEdit.DecimalPlaces = 2;
			this.WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 120, true);
			this.WidthCalcEdit.Name = "WidthCalcEdit";
			this.WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 26, true);
			this.WidthCalcEdit.TabIndex = 4;
			this.WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LengthCalcEdit, "JO_OuterPackLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OuterPackLength)));
			this.LengthCalcEdit.CaptionResourceString = null;
			this.LengthCalcEdit.DecimalPlaces = 2;
			this.LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 94, true);
			this.LengthCalcEdit.Name = "LengthCalcEdit";
			this.LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 26, true);
			this.LengthCalcEdit.TabIndex = 3;
			this.LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_UnitOfVolume)));
			this.VolumeCalcDropEdit.BindToAmount = "JO_ActualVolume";
			this.VolumeCalcDropEdit.BindToUnit = "JO_UnitOfVolume";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 68, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.VolumeCalcDropEdit.TabIndex = 2;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderLine)(null)).JO_UnitOfWeight)));
			this.WeightCalcDropEdit.BindToAmount = "JO_ActualWeight";
			this.WeightCalcDropEdit.BindToUnit = "JO_UnitOfWeight";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 42, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.WeightCalcDropEdit.TabIndex = 1;
			// 
			// ShippingToleranceTabPage
			// 
			this.ShippingToleranceTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|5e3aea7c-35dd-4204-9dd8-cbcf4e88bd08", "Shipping Tolerance");
			this.ShippingToleranceTabPage.Controls.Add(this.AvailableLateLabel);
			this.ShippingToleranceTabPage.Controls.Add(this.ExWorksDaysLate);
			this.ShippingToleranceTabPage.Controls.Add(this.AvailableEarlyLabel);
			this.ShippingToleranceTabPage.Controls.Add(this.ExWorksDaysEarly);
			this.ShippingToleranceTabPage.Controls.Add(this.ExWorksLabel);
			this.ShippingToleranceTabPage.Controls.Add(this.AllowableOverLabel);
			this.ShippingToleranceTabPage.Controls.Add(this.QuantityOverPercentage);
			this.ShippingToleranceTabPage.Controls.Add(this.AllowableUnderLabel);
			this.ShippingToleranceTabPage.Controls.Add(this.QuantityUnderPercentage);
			this.ShippingToleranceTabPage.Controls.Add(this.QuantityLabel);
			this.ShippingToleranceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.ShippingToleranceTabPage.Name = "ShippingToleranceTabPage";
			this.ShippingToleranceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 163, true);
			this.ShippingToleranceTabPage.TabIndex = 2;
			// 
			// AvailableLateLabel
			// 
			this.AvailableLateLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("fc72cb0f-090b-4077-aa60-b5408088a1d9", "Available Late");
			this.AvailableLateLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AvailableLateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 62, true);
			this.AvailableLateLabel.Name = "AvailableLateLabel";
			this.AvailableLateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
			this.AvailableLateLabel.TabIndex = 6;
			// 
			// ExWorksDaysLate
			// 
			this.ExWorksDaysLate.AllowDrop = true;
			this.ExWorksDaysLate.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ExWorksDaysLate, "JO_LateShipmentLimitDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_LateShipmentLimitDays)));
			this.ExWorksDaysLate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 85, true);
			this.ExWorksDaysLate.Name = "ExWorksDaysLate";
			this.ExWorksDaysLate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 30, true);
			this.ExWorksDaysLate.TabIndex = 9;
			this.ExWorksDaysLate.ToleranceType = Enterprise.Freight.Forwarding.GUI.OrderManager.Orders.ToleranceTypes.Days;
			// 
			// AvailableEarlyLabel
			// 
			this.AvailableEarlyLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b32b9a52-0037-409c-b743-6c794a6532ca", "Available Early");
			this.AvailableEarlyLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AvailableEarlyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 62, true);
			this.AvailableEarlyLabel.Name = "AvailableEarlyLabel";
			this.AvailableEarlyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
			this.AvailableEarlyLabel.TabIndex = 5;
			// 
			// ExWorksDaysEarly
			// 
			this.ExWorksDaysEarly.AllowDrop = true;
			this.ExWorksDaysEarly.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ExWorksDaysEarly, "JO_EarlyShipmentLimitDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_EarlyShipmentLimitDays)));
			this.ExWorksDaysEarly.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 85, true);
			this.ExWorksDaysEarly.Name = "ExWorksDaysEarly";
			this.ExWorksDaysEarly.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 30, true);
			this.ExWorksDaysEarly.TabIndex = 8;
			this.ExWorksDaysEarly.ToleranceType = Enterprise.Freight.Forwarding.GUI.OrderManager.Orders.ToleranceTypes.Days;
			// 
			// ExWorksLabel
			// 
			this.ExWorksLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f3cb38ab-7f4f-4980-b721-093ea34957aa", "Ex Works Date");
			this.ExWorksLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExWorksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 89, true);
			this.ExWorksLabel.Name = "ExWorksLabel";
			this.ExWorksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 22, true);
			this.ExWorksLabel.TabIndex = 7;
			// 
			// AllowableOverLabel
			// 
			this.AllowableOverLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("99310608-f5d7-4544-a051-63d1a301ab44", "Allowable Over");
			this.AllowableOverLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AllowableOverLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 8, true);
			this.AllowableOverLabel.Name = "AllowableOverLabel";
			this.AllowableOverLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 17, true);
			this.AllowableOverLabel.TabIndex = 1;
			// 
			// QuantityOverPercentage
			// 
			this.QuantityOverPercentage.AllowDrop = true;
			this.QuantityOverPercentage.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.QuantityOverPercentage, "JO_OverQuantityPercentageLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_OverQuantityPercentageLimit)));
			this.QuantityOverPercentage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 28, true);
			this.QuantityOverPercentage.Name = "QuantityOverPercentage";
			this.QuantityOverPercentage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 30, true);
			this.QuantityOverPercentage.TabIndex = 4;
			this.QuantityOverPercentage.ToleranceModifier = Enterprise.Freight.Forwarding.GUI.OrderManager.Orders.ToleranceModifiers.Positive;
			// 
			// AllowableUnderLabel
			// 
			this.AllowableUnderLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("76dc9fd2-775e-4f91-9a0f-6ff4169c4df5", "Allowable Under");
			this.AllowableUnderLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AllowableUnderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 8, true);
			this.AllowableUnderLabel.Name = "AllowableUnderLabel";
			this.AllowableUnderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 17, true);
			this.AllowableUnderLabel.TabIndex = 0;
			// 
			// QuantityUnderPercentage
			// 
			this.QuantityUnderPercentage.AllowDrop = true;
			this.QuantityUnderPercentage.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.QuantityUnderPercentage, "JO_UnderQuantityPercentageLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderLine)(null)).JO_UnderQuantityPercentageLimit)));
			this.QuantityUnderPercentage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 28, true);
			this.QuantityUnderPercentage.Name = "QuantityUnderPercentage";
			this.QuantityUnderPercentage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 30, true);
			this.QuantityUnderPercentage.TabIndex = 3;
			this.QuantityUnderPercentage.ToleranceModifier = Enterprise.Freight.Forwarding.GUI.OrderManager.Orders.ToleranceModifiers.Negative;
			// 
			// QuantityLabel
			// 
			this.QuantityLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|2653a1f3-0a7a-4c96-9029-39b756cd8314", "Quantity");
			this.QuantityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.QuantityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 32, true);
			this.QuantityLabel.Name = "QuantityLabel";
			this.QuantityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 22, true);
			this.QuantityLabel.TabIndex = 2;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|67de2801-8ef7-4286-8a60-d3b93f9a4402", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 473, true);
			this.WorkflowTabPage.TabIndex = 2;
			// 
			// ShowHideContainers
			// 
			this.ShowHideContainers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowHideContainers.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderLineForm|ef5e9455-4936-4cca-bce4-8cdbd89d0a19", "Hide Containers");
			this.ShowHideContainers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 523, true);
			this.ShowHideContainers.Name = "ShowHideContainers";
			this.ShowHideContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 22, true);
			this.ShowHideContainers.TabIndex = 5;
			this.ShowHideContainers.ToolTipCaption = null;
			this.ShowHideContainers.Click += new EventHandler(this.ShowHideContainers_Click);
			// 
			// OrderLineForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 591, true);
			this.Controls.Add(this.OrderLineFormControl);
			this.Controls.Add(this.ShowHideContainers);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(OrderLine);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 630, true);
			this.Name = "OrderLineForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.ShowHideContainers, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrderLineFormControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((ISupportInitialize)(this.DeliveriesBoundGrid)).EndInit();
			this.DeliveriesBoundGrid.ResumeLayout(false);
			this.DeliveriesBoundGrid.PerformLayout();
			this.DeliveriesGroupBox.ResumeLayout(false);
			this.DeliveriesGroupBox.PerformLayout();
			this.OrderLineFormControl.ResumeLayout(false);
			this.OrderLineFormControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.AttachDeliveryToContainerGroupBox.ResumeLayout(false);
			this.AttachDeliveryToContainerGroupBox.PerformLayout();
			this.J5_RL_DestinationPortForAttachBoundFindBox.ResumeLayout(true);
			this.J5_RL_DestinationPortForAttachBoundFindBox.PerformLayout();
			this.J5_PartnoForAttachBoundFindBox.ResumeLayout(true);
			this.J5_PartnoForAttachBoundFindBox.PerformLayout();
			this.RelatedContainersGroupBox.ResumeLayout(false);
			this.RelatedContainersGroupBox.PerformLayout();
			((ISupportInitialize)(this.RelatedDeliveryContainersBoundGrid)).EndInit();
			this.RelatedDeliveryContainersBoundGrid.ResumeLayout(false);
			this.RelatedDeliveryContainersBoundGrid.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((ISupportInitialize)(this.DeliveryContainersBoundGrid)).EndInit();
			this.DeliveryContainersBoundGrid.ResumeLayout(false);
			this.DeliveryContainersBoundGrid.PerformLayout();
			this.OrderLineTabControl.ResumeLayout(false);
			this.OrderLineTabControl.PerformLayout();
			this.FieldsTabPage.ResumeLayout(false);
			this.FieldsTabPage.PerformLayout();
			this.JO_ShipmentWindowStartBoundDateEdit.ResumeLayout(true);
			this.JO_ShipmentWindowStartBoundDateEdit.PerformLayout();
			this.JO_ShipmentWindowEndBoundDateEdit.ResumeLayout(true);
			this.JO_ShipmentWindowEndBoundDateEdit.PerformLayout();
			this.JO_InnerPacksBoundDropEdit.ResumeLayout(true);
			this.JO_InnerPacksBoundDropEdit.PerformLayout();
			this.JO_OuterPacksBoundDropEdit.ResumeLayout(true);
			this.JO_OuterPacksBoundDropEdit.PerformLayout();
			this.JO_ExWorksDateBoundDateEdit.ResumeLayout(true);
			this.JO_ExWorksDateBoundDateEdit.PerformLayout();
			this.JO_ConfirmationDateBoundDateEdit.ResumeLayout(true);
			this.JO_ConfirmationDateBoundDateEdit.PerformLayout();
			this.JO_INCOBoundDropEdit.ResumeLayout(true);
			this.JO_INCOBoundDropEdit.PerformLayout();
			this.LineDropDatezDateEdit.ResumeLayout(true);
			this.LineDropDatezDateEdit.PerformLayout();
			this.JO_LineStatusBoundDropEdit.ResumeLayout(true);
			this.JO_LineStatusBoundDropEdit.PerformLayout();
			this.JO_PartnoBoundFindBox.ResumeLayout(true);
			this.JO_PartnoBoundFindBox.PerformLayout();
			this.JO_RH_NKCommodityCodeFindBox.ResumeLayout(true);
			this.JO_RH_NKCommodityCodeFindBox.PerformLayout();
			this.JO_F3_NKPackTypeBoundDropEdit.ResumeLayout(true);
			this.JO_F3_NKPackTypeBoundDropEdit.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.CustomFieldsUserControl.ResumeLayout(true);
			this.CustomFieldsUserControl.PerformLayout();
			this.OuterPacksGroupBox.ResumeLayout(false);
			this.OuterPacksGroupBox.PerformLayout();
			this.OuterPacksCalcDropEdit.ResumeLayout(true);
			this.OuterPacksCalcDropEdit.PerformLayout();
			this.UnitOfDimensionCalcDropEdit.ResumeLayout(true);
			this.UnitOfDimensionCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.ShippingToleranceTabPage.ResumeLayout(false);
			this.ShippingToleranceTabPage.PerformLayout();
			this.ExWorksDaysLate.ResumeLayout(true);
			this.ExWorksDaysLate.PerformLayout();
			this.ExWorksDaysEarly.ResumeLayout(true);
			this.ExWorksDaysEarly.PerformLayout();
			this.QuantityOverPercentage.ResumeLayout(true);
			this.QuantityOverPercentage.PerformLayout();
			this.QuantityUnderPercentage.ResumeLayout(true);
			this.QuantityUnderPercentage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}


		[FormBasherTestPopupExclude]
		class CustomCodeFindBox : ZCodeFindBox
		{
		}
	}
}
