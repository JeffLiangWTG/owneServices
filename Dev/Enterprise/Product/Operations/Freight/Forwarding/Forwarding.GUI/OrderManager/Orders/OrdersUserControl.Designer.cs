using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersUserControl : ZUserControl
	{
		protected ZGroupBox OrderDetailsGroupBox;
		protected ZButton IncoTermsExplainButton;
		protected ZCodeFindBox zCodeFindBox1;
		private ZGuidFindBox JD_EFBoundGuidFindBox;
		private ZDateEdit ReqInStoreDateEdit;
		private ZDateEdit ReqExWorksDateEdit;
		protected ZExchangeRateControl JD_RXBoundCurrency;
		private ZTextBox JD_AdditionalTermsBoundDropEdit;
		private ZDropEdit JD_OrderStatusBoundDropEdit;
		private ZCheckBox JD_IsReleasedCheckbox;
		protected ZCodeFindBox JD_RSBoundFindBox;
		private ZDateEdit JD_FollowUpDateBoundDateEdit;
		private ZDropEdit JD_ContainerModeBoundDropEdit;
		private ZDropEdit JD_TransportModeBoundDropEdit;
		private ZDropEdit JD_IncoTermBoundDropEdit;
		private ZDateEdit JD_InvoiceDateBoundDateEdit;
		private ZTextBox JD_InvoiceNumberBoundTextBox;
		private ZDateEdit JD_OrderDateBoundDateEdit;
		private ZTextBox JD_OrderNumberBoundTextBox;
		private ZDateEdit JD_BookingConfDateBoundDateEdit;
		private ZTextBox JD_OrderGoodsDescriptionBoundTextBox;
		private ZTextBox JD_BookingConfRefBoundTextBox;
		private ZCalcEdit JD_OrderNumberSplitBoundTextBox;
		protected OrderLinesButtonGrid OrderLinesBoundButtonGrid;
		private OrderPlanningVesselVoyageAndDatesControl OrderPlanningVesselVoyageAndDates;
		protected OrderSplitsButtonGrid OrderSplitsButtonGrid;
		private ZLabel JD_JELabel;
		protected ZTabPage OrdersTab;
		protected ZTabPage PlanningTab;
		ZDateEdit JD_DepartureVesselCutoffDateBoundDateEdit;
		ZGuidFindBox JD_OH_SendingAgentBoundFindBox;
		ZGuidFindBox JD_OH_ReceivingAgentBoundFindBox;
		protected ZTabPage ShipmentTab;
		ZGroupBox ConsolsGroupBox;
		ZGroupBox ShipmentGroupBox;
		ZTabPage CustomFieldsTab;
		protected ZButton ShipmentDetachButton;
		ZLinkLabel SendAgentContactsLinkButton;
		ZLinkLabel ReceiveAgentContactsLinkButton;
		protected ZTemplateTabControl BottomTabControl;
		protected ZModuleButtonGrid ConsolsBoundButtonGrid;
		ZCodeFindBox JD_RL_NKGoodsDeliveredToBoundFindBox;
		ZCodeFindBox JD_RL_NKGoodsAvailableAtBoundFindBox;
		ZCodeFindBox JD_RL_NKPortOfDischargeBoundFindBox;
		ZCodeFindBox JD_RL_NKPortOfLoadingBoundFindBox;
		ZGuidFindBox JD_OH_CarrierBoundFindBox;
		ZLinkLabel CarrierContactsLinkButton;
		protected ZGrid PlannedContainersGrid;
		protected ZLabel JD_RSLabel;
		ZGroupBox PlannedContainersGroupBox;
		ZCalcDropEdit JD_ActualVolumeBoundCalcDropEdit;
		ZCalcDropEdit JD_ActualWeightBoundCalcDropEdit;
		ZCalcDropEdit JD_PacksBoundCalcDropEdit;
		protected ZGuidFindBox JD_VBBoundFindBox;
		protected ZGuidFindBox JD_JSBoundFindBox;
		protected ZGuidFindBox JD_JEBoundFindBox;
		ZCodeFindBox AttachedShipment_RL_NKOriginBoundCodeFindBox;
		ZCodeFindBox AttachedShipment_RS_NKServiceLevelBoundFindBox;
		ZTextBox AttachedShipment_HouseBillBoundTextBox;
		ZCodeFindBox AttachedShipment_RL_NKDestinationBoundCodeFindBox;
		ZTextBox JD_Waybill;
		private ZTabPage ProductQuantitySummaryTabPage;
		private ZGrid ProductQuantitySummaryGrid;
		internal ZTabPage ChargesTabPage;
		private ZGrid ChargesGrid;
		protected ZTabControl RightTabControl;
		protected ZTabPage MilestonesTabPage;
		private ZTabPage OrderSplitsTabPage;
		private OrderMilestonesUserControl OrderMilestones;
		private OrderTotalsControl OrderTotals;
		protected ProcessTemplateCustomFieldsControl OrderCustomFieldsDisplayControl;
		private MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl BuyerZAddressWithContactControl;
		private MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl SupplierZAddressWithContactControl;
		private ZDateEdit JD_ShipmentWindowStartBoundDateEdit;
		private ZDateEdit JD_ShipmentWindowEndBoundDateEdit;
		ZTextBox JD_MasterWaybillTextBox;

		void InitializeComponent()
		{
			this.components = new Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new ZCodeFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo16 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo17 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo18 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo19 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo20 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo21 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			this.JD_ShipmentWindowStartBoundDateEdit = new ZDateEdit();
			this.JD_ShipmentWindowEndBoundDateEdit = new ZDateEdit();
			this.OrderSplitsButtonGrid = new OrderSplitsButtonGrid();
			this.OrdersTab = new ZTabPage();
			this.OrderLinesBoundButtonGrid = new OrderLinesButtonGrid();
			this.PlanningTab = new ZTabPage();
			this.OrderPlanningVesselVoyageAndDates = new OrderPlanningVesselVoyageAndDatesControl();
			this.JD_Waybill = new ZTextBox();
			this.JD_MasterWaybillTextBox = new ZTextBox();
			this.PlannedContainersGroupBox = new ZGroupBox();
			this.PlannedContainersGrid = new ZGrid();
			this.CarrierContactsLinkButton = new ZLinkLabel();
			this.JD_OH_CarrierBoundFindBox = new ZGuidFindBox();
			this.JD_RL_NKGoodsDeliveredToBoundFindBox = new ZCodeFindBox();
			this.JD_RL_NKGoodsAvailableAtBoundFindBox = new ZCodeFindBox();
			this.JD_RL_NKPortOfDischargeBoundFindBox = new ZCodeFindBox();
			this.JD_RL_NKPortOfLoadingBoundFindBox = new ZCodeFindBox();
			this.ReceiveAgentContactsLinkButton = new ZLinkLabel();
			this.SendAgentContactsLinkButton = new ZLinkLabel();
			this.JD_DepartureVesselCutoffDateBoundDateEdit = new ZDateEdit();
			this.JD_OH_SendingAgentBoundFindBox = new ZGuidFindBox();
			this.JD_OH_ReceivingAgentBoundFindBox = new ZGuidFindBox();
			this.JD_PacksBoundCalcDropEdit = new ZCalcDropEdit();
			this.JD_ActualVolumeBoundCalcDropEdit = new ZCalcDropEdit();
			this.JD_ActualWeightBoundCalcDropEdit = new ZCalcDropEdit();
			this.ShipmentTab = new ZTabPage();
			this.ConsolsGroupBox = new ZGroupBox();
			this.ConsolsBoundButtonGrid = new ZModuleButtonGrid();
			this.ShipmentGroupBox = new ZGroupBox();
			this.JD_JEBoundFindBox = new ZGuidFindBox();
			this.JD_JSBoundFindBox = new ZGuidFindBox();
			this.JD_VBBoundFindBox = new ZGuidFindBox();
			this.ShipmentDetachButton = new ZButton();
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox = new ZCodeFindBox();
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox = new ZCodeFindBox();
			this.AttachedShipment_HouseBillBoundTextBox = new ZTextBox();
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox = new ZCodeFindBox();
			this.JD_JELabel = new ZLabel();
			this.CustomFieldsTab = new ZTabPage();
			this.OrderCustomFieldsDisplayControl = new ProcessTemplateCustomFieldsControl();
			this.BottomTabControl = new ZTemplateTabControl();
			this.ProductQuantitySummaryTabPage = new ZTabPage();
			this.ProductQuantitySummaryGrid = new ZGrid();
			this.OrderTotals = new OrderTotalsControl();
			this.ChargesTabPage = new ZTabPage();
			this.ChargesGrid = new ZGrid();
			this.OrderDetailsGroupBox = new ZGroupBox();
			this.IncoTermsExplainButton = new ZButton();
			this.zCodeFindBox1 = new ZCodeFindBox();
			this.JD_EFBoundGuidFindBox = new ZGuidFindBox();
			this.ReqInStoreDateEdit = new ZDateEdit();
			this.ReqExWorksDateEdit = new ZDateEdit();
			this.JD_RXBoundCurrency = new ZExchangeRateControl();
			this.JD_AdditionalTermsBoundDropEdit = new ZTextBox();
			this.JD_OrderStatusBoundDropEdit = new ZDropEdit();
			this.JD_IsReleasedCheckbox = new ZCheckBox();
			this.JD_RSBoundFindBox = new ZCodeFindBox();
			this.JD_FollowUpDateBoundDateEdit = new ZDateEdit();
			this.JD_ContainerModeBoundDropEdit = new ZDropEdit();
			this.JD_TransportModeBoundDropEdit = new ZDropEdit();
			this.JD_IncoTermBoundDropEdit = new ZDropEdit();
			this.JD_InvoiceDateBoundDateEdit = new ZDateEdit();
			this.JD_InvoiceNumberBoundTextBox = new ZTextBox();
			this.JD_OrderDateBoundDateEdit = new ZDateEdit();
			this.JD_OrderNumberBoundTextBox = new ZTextBox();
			this.JD_RSLabel = new ZLabel();
			this.JD_BookingConfDateBoundDateEdit = new ZDateEdit();
			this.JD_OrderGoodsDescriptionBoundTextBox = new ZTextBox();
			this.JD_BookingConfRefBoundTextBox = new ZTextBox();
			this.JD_OrderNumberSplitBoundTextBox = new ZCalcEdit();
			this.RightTabControl = new ZTabControl();
			this.MilestonesTabPage = new ZTabPage();
			this.OrderMilestones = new OrderMilestonesUserControl();
			this.OrderSplitsTabPage = new ZTabPage();
			this.BuyerZAddressWithContactControl = new MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			this.SupplierZAddressWithContactControl = new MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JD_ShipmentWindowStartBoundDateEdit.SuspendLayout();
			this.JD_ShipmentWindowEndBoundDateEdit.SuspendLayout();
			((ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).BeginInit();
			this.OrderSplitsButtonGrid.SuspendLayout();
			this.OrdersTab.SuspendLayout();
			((ISupportInitialize)(this.OrderLinesBoundButtonGrid.InnerGrid)).BeginInit();
			this.OrderLinesBoundButtonGrid.SuspendLayout();
			this.PlanningTab.SuspendLayout();
			this.OrderPlanningVesselVoyageAndDates.SuspendLayout();
			this.PlannedContainersGroupBox.SuspendLayout();
			((ISupportInitialize)(this.PlannedContainersGrid)).BeginInit();
			this.PlannedContainersGrid.SuspendLayout();
			this.JD_OH_CarrierBoundFindBox.SuspendLayout();
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.SuspendLayout();
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.SuspendLayout();
			this.JD_RL_NKPortOfDischargeBoundFindBox.SuspendLayout();
			this.JD_RL_NKPortOfLoadingBoundFindBox.SuspendLayout();
			this.JD_DepartureVesselCutoffDateBoundDateEdit.SuspendLayout();
			this.JD_OH_SendingAgentBoundFindBox.SuspendLayout();
			this.JD_OH_ReceivingAgentBoundFindBox.SuspendLayout();
			this.JD_PacksBoundCalcDropEdit.SuspendLayout();
			this.JD_ActualVolumeBoundCalcDropEdit.SuspendLayout();
			this.JD_ActualWeightBoundCalcDropEdit.SuspendLayout();
			this.ShipmentTab.SuspendLayout();
			this.ConsolsGroupBox.SuspendLayout();
			((ISupportInitialize)(this.ConsolsBoundButtonGrid.InnerGrid)).BeginInit();
			this.ConsolsBoundButtonGrid.SuspendLayout();
			this.ShipmentGroupBox.SuspendLayout();
			this.JD_JEBoundFindBox.SuspendLayout();
			this.JD_JSBoundFindBox.SuspendLayout();
			this.JD_VBBoundFindBox.SuspendLayout();
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.SuspendLayout();
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.SuspendLayout();
			this.CustomFieldsTab.SuspendLayout();
			this.OrderCustomFieldsDisplayControl.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.ProductQuantitySummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.ProductQuantitySummaryGrid)).BeginInit();
			this.ProductQuantitySummaryGrid.SuspendLayout();
			this.OrderTotals.SuspendLayout();
			this.ChargesTabPage.SuspendLayout();
			((ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			this.OrderDetailsGroupBox.SuspendLayout();
			this.zCodeFindBox1.SuspendLayout();
			this.JD_EFBoundGuidFindBox.SuspendLayout();
			this.ReqInStoreDateEdit.SuspendLayout();
			this.ReqExWorksDateEdit.SuspendLayout();
			this.JD_RXBoundCurrency.SuspendLayout();
			this.JD_OrderStatusBoundDropEdit.SuspendLayout();
			this.JD_IsReleasedCheckbox.SuspendLayout();
			this.JD_RSBoundFindBox.SuspendLayout();
			this.JD_FollowUpDateBoundDateEdit.SuspendLayout();
			this.JD_ContainerModeBoundDropEdit.SuspendLayout();
			this.JD_TransportModeBoundDropEdit.SuspendLayout();
			this.JD_IncoTermBoundDropEdit.SuspendLayout();
			this.JD_InvoiceDateBoundDateEdit.SuspendLayout();
			this.JD_OrderDateBoundDateEdit.SuspendLayout();
			this.JD_BookingConfDateBoundDateEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.MilestonesTabPage.SuspendLayout();
			this.OrderMilestones.SuspendLayout();
			this.OrderSplitsTabPage.SuspendLayout();
			this.BuyerZAddressWithContactControl.SuspendLayout();
			this.SupplierZAddressWithContactControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Order);
			// 
			// JD_ShipmentWindowStartBoundDateEdit
			// 
			this.JD_ShipmentWindowStartBoundDateEdit.AllowDrop = true;
			this.JD_ShipmentWindowStartBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JD_ShipmentWindowStartBoundDateEdit, "JD_ShipmentWindowStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Order)(null)).JD_ShipmentWindowStart)));
			this.JD_ShipmentWindowStartBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 116, true);
			this.JD_ShipmentWindowStartBoundDateEdit.Name = "JD_ShipmentWindowStartBoundDateEdit";
			this.JD_ShipmentWindowStartBoundDateEdit.TabIndex = 8;
			// 
			// JD_ShipmentWindowEndBoundDateEdit
			// 
			this.JD_ShipmentWindowEndBoundDateEdit.AllowDrop = true;
			this.JD_ShipmentWindowEndBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JD_ShipmentWindowEndBoundDateEdit, "JD_ShipmentWindowEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Order)(null)).JD_ShipmentWindowEnd)));
			this.JD_ShipmentWindowEndBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 138, true);
			this.JD_ShipmentWindowEndBoundDateEdit.Name = "JD_ShipmentWindowEndBoundDateEdit";
			this.JD_ShipmentWindowEndBoundDateEdit.TabIndex = 10;
			// 
			// OrderSplitsButtonGrid
			// 
			this.OrderSplitsButtonGrid.AllowDrop = true;
			this.OrderSplitsButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.OrderSplitsButtonGrid, "OrderSplitSiblings");
			this.OrderSplitsButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderSplitsButtonGrid.GridId = "6987413c-33a7-4c92-8485-b2e33ef3fdef";
			// 
			// 
			// 
			this.OrderSplitsButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrderSplitsButtonGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrderSplitsButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrderSplitsButtonGrid.InnerGrid.GridId = null;
			this.OrderSplitsButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderSplitsButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OrderSplitsButtonGrid.InnerGrid.Name = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.ReadOnly = true;
			this.OrderSplitsButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 247, true);
			this.OrderSplitsButtonGrid.InnerGrid.TabIndex = 0;
			this.OrderSplitsButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrderSplitsButtonGrid.Name = "OrderSplitsButtonGrid";
			this.OrderSplitsButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("97D726F4-347C-4E56-B442-4608A8CDE483", "Order Line");
			this.OrderSplitsButtonGrid.Order = null;
			this.OrderSplitsButtonGrid.ReadOnly = true;
			this.OrderSplitsButtonGrid.ShowAttachButton = false;
			this.OrderSplitsButtonGrid.ShowDetachButton = false;
			this.OrderSplitsButtonGrid.ShowEditButton = false;
			this.OrderSplitsButtonGrid.ShowNewButton = false;
			this.OrderSplitsButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 284, true);
			this.OrderSplitsButtonGrid.TabIndex = 0;
			// 
			// OrdersTab
			// 
			this.OrdersTab.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|ce374107-88fe-4198-9edf-783244334d6f", "Order Lines");
			this.OrdersTab.Controls.Add(this.OrderLinesBoundButtonGrid);
			this.OrdersTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.OrdersTab.Name = "OrdersTab";
			this.OrdersTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.OrdersTab.TabIndex = 0;
			// 
			// OrderLinesBoundButtonGrid
			// 
			this.OrderLinesBoundButtonGrid.AllowDrop = true;
			this.OrderLinesBoundButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.OrderLinesBoundButtonGrid, "OrderLines");
			this.OrderLinesBoundButtonGrid.BindToFindBoxList = "OrderLines";
			this.OrderLinesBoundButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderLinesBoundButtonGrid.GridId = "d8b61363-6171-4020-a349-b4d91a08a432";
			// 
			// 
			// 
			this.OrderLinesBoundButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrderLinesBoundButtonGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrderLinesBoundButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrderLinesBoundButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderLinesBoundButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrderLinesBoundButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OrderLinesBoundButtonGrid.InnerGrid.Name = "Grid";
			this.OrderLinesBoundButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 173, true);
			this.OrderLinesBoundButtonGrid.InnerGrid.TabIndex = 0;
			this.OrderLinesBoundButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderLinesBoundButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrderLine;
			this.OrderLinesBoundButtonGrid.Name = "OrderLinesBoundButtonGrid";
			this.OrderLinesBoundButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("97D726F4-347C-4E56-B442-4608A8CDE483", "Order Line");
			this.OrderLinesBoundButtonGrid.ReadOnly = false;
			this.OrderLinesBoundButtonGrid.ShowAttachButton = false;
			this.OrderLinesBoundButtonGrid.ShowDetachButton = false;
			this.OrderLinesBoundButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.OrderLinesBoundButtonGrid.TabIndex = 0;
			// 
			// PlanningTab
			// 
			this.PlanningTab.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|84b43a91-8d29-4f6f-9d00-21f2dfd4aa96", "Planning");
			this.PlanningTab.Controls.Add(this.OrderPlanningVesselVoyageAndDates);
			this.PlanningTab.Controls.Add(this.JD_Waybill);
			this.PlanningTab.Controls.Add(this.JD_MasterWaybillTextBox);
			this.PlanningTab.Controls.Add(this.PlannedContainersGroupBox);
			this.PlanningTab.Controls.Add(this.CarrierContactsLinkButton);
			this.PlanningTab.Controls.Add(this.JD_OH_CarrierBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_RL_NKGoodsDeliveredToBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_RL_NKGoodsAvailableAtBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_RL_NKPortOfDischargeBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_RL_NKPortOfLoadingBoundFindBox);
			this.PlanningTab.Controls.Add(this.ReceiveAgentContactsLinkButton);
			this.PlanningTab.Controls.Add(this.SendAgentContactsLinkButton);
			this.PlanningTab.Controls.Add(this.JD_DepartureVesselCutoffDateBoundDateEdit);
			this.PlanningTab.Controls.Add(this.JD_OH_SendingAgentBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_OH_ReceivingAgentBoundFindBox);
			this.PlanningTab.Controls.Add(this.JD_PacksBoundCalcDropEdit);
			this.PlanningTab.Controls.Add(this.JD_ActualVolumeBoundCalcDropEdit);
			this.PlanningTab.Controls.Add(this.JD_ActualWeightBoundCalcDropEdit);
			this.PlanningTab.Controls.Add(this.JD_ShipmentWindowStartBoundDateEdit);
			this.PlanningTab.Controls.Add(this.JD_ShipmentWindowEndBoundDateEdit);
			this.PlanningTab.Name = "PlanningTab";
			this.PlanningTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.PlanningTab.TabIndex = 4;
			this.PlanningTab.AutoScroll = true;
			this.PlanningTab.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			// 
			// OrderPlanningVesselVoyageAndDates
			// 
			this.OrderPlanningVesselVoyageAndDates.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrderPlanningVesselVoyageAndDates, ".");
			this.OrderPlanningVesselVoyageAndDates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.OrderPlanningVesselVoyageAndDates.Name = "OrderPlanningVesselVoyageAndDates";
			this.OrderPlanningVesselVoyageAndDates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 85, true);
			this.OrderPlanningVesselVoyageAndDates.TabIndex = 5;
			// 
			// JD_Waybill
			// 
			this.BindingSource.SetBindingMember(this.JD_Waybill, "JD_Waybill");
			this.JD_Waybill.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|a9d849f8-cab6-4313-9978-7d91f401a4ad", "House Bill");
			this.JD_Waybill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 4, true);
			this.JD_Waybill.Name = "JD_Waybill";
			this.JD_Waybill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.JD_Waybill.TabIndex = 3;
			// 
			// JD_MasterWaybillTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_MasterWaybillTextBox, "JD_MasterWaybill");
			this.JD_MasterWaybillTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|54df3e7b-c447-4e73-a54e-38e9386fec5c", "Master Bill");
			this.JD_MasterWaybillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(860, 4, true);
			this.JD_MasterWaybillTextBox.Name = "JD_MasterWaybillTextBox";
			this.JD_MasterWaybillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.JD_MasterWaybillTextBox.TabIndex = 4;
			// 
			// PlannedContainersGroupBox
			// 
			this.PlannedContainersGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PlannedContainersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|cf9f7313-5e80-4736-b0ee-b0764698a216", "Planned Containers");
			this.PlannedContainersGroupBox.Controls.Add(this.PlannedContainersGrid);
			this.PlannedContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(663, 26, true);
			this.PlannedContainersGroupBox.Name = "PlannedContainersGroupBox";
			this.PlannedContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 110, true);
			this.PlannedContainersGroupBox.TabIndex = 40;
			this.PlannedContainersGroupBox.TabStop = false;
			// 
			// PlannedContainersGrid
			// 
			this.PlannedContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PlannedContainersGrid, "PlannedContainers");
			this.PlannedContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo20.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "J1_ContainerNumber";
			zTextBoxColumnStyleInfo20.ToolTip = "Enter the Container Number";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "J1_ContainerCount";
			zCalcEditColumnStyleInfo12.Decimals = 0;
			zCalcEditColumnStyleInfo12.ToolTip = "Enter the container count";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "J1_RC";
			zGuidFindBoxColumnStyleInfo12.ToolTip = "Enter the container type";
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo21.ColumnName = "J1_SealNum";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.ColumnName = "J1_AdditionalSealNum";
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.ColumnName = "J1_Additional2SealNum";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PlannedContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.PlannedContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.PlannedContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.PlannedContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.PlannedContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.PlannedContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.PlannedContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlannedContainersGrid.GridId = "6731e63f-2857-4b76-b9b4-600944e71634";
			this.PlannedContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PlannedContainersGrid.LayoutKey = "zGrid1";
			this.PlannedContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.PlannedContainersGrid.Name = "PlannedContainersGrid";
			this.PlannedContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 93, true);
			this.PlannedContainersGrid.TabIndex = 30;
			// 
			// CarrierContactsLinkButton
			// 
			this.CarrierContactsLinkButton.AutoSize = true;
			this.CarrierContactsLinkButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|a955114b-ccde-43be-9aca-8390f77726cc", "View Contacts");
			this.CarrierContactsLinkButton.IsFontBold = false;
			this.CarrierContactsLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 8, true);
			this.CarrierContactsLinkButton.Name = "CarrierContactsLinkButton";
			this.CarrierContactsLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.CarrierContactsLinkButton.TabIndex = 2;
			this.CarrierContactsLinkButton.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnCarrierContactsLinkButton_Click);
			// 
			// JD_OH_CarrierBoundFindBox
			// 
			this.JD_OH_CarrierBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_OH_CarrierBoundFindBox, "JD_OH_Carrier");
			this.JD_OH_CarrierBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 4, true);
			this.JD_OH_CarrierBoundFindBox.Name = "JD_OH_CarrierBoundFindBox";
			this.JD_OH_CarrierBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 18, true);
			this.JD_OH_CarrierBoundFindBox.TabIndex = 1;
			// 
			// JD_RL_NKGoodsDeliveredToBoundFindBox
			// 
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RL_NKGoodsDeliveredToBoundFindBox, "JD_RL_NKGoodsDeliveredTo");
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 138, true);
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 25, 1, true);
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.Name = "JD_RL_NKGoodsDeliveredToBoundFindBox";
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.PreBoundMaxLength = 5;
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.TabIndex = 9;
			// 
			// JD_RL_NKGoodsAvailableAtBoundFindBox
			// 
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RL_NKGoodsAvailableAtBoundFindBox, "JD_RL_NKGoodsAvailableAt");
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 116, true);
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 25, 1, true);
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.Name = "JD_RL_NKGoodsAvailableAtBoundFindBox";
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.PreBoundMaxLength = 5;
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.TabIndex = 7;
			// 
			// JD_RL_NKPortOfDischargeBoundFindBox
			// 
			this.JD_RL_NKPortOfDischargeBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RL_NKPortOfDischargeBoundFindBox, "JD_RL_NKPortOfDischarge");
			this.JD_RL_NKPortOfDischargeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 182, true);
			this.JD_RL_NKPortOfDischargeBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 25, 1, true);
			this.JD_RL_NKPortOfDischargeBoundFindBox.Name = "JD_RL_NKPortOfDischargeBoundFindBox";
			this.JD_RL_NKPortOfDischargeBoundFindBox.PreBoundMaxLength = 5;
			this.JD_RL_NKPortOfDischargeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.JD_RL_NKPortOfDischargeBoundFindBox.TabIndex = 14;
			// 
			// JD_RL_NKPortOfLoadingBoundFindBox
			// 
			this.JD_RL_NKPortOfLoadingBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RL_NKPortOfLoadingBoundFindBox, "JD_RL_NKPortOfLoading");
			this.JD_RL_NKPortOfLoadingBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 160, true);
			this.JD_RL_NKPortOfLoadingBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 25, 1, true);
			this.JD_RL_NKPortOfLoadingBoundFindBox.Name = "JD_RL_NKPortOfLoadingBoundFindBox";
			this.JD_RL_NKPortOfLoadingBoundFindBox.PreBoundMaxLength = 5;
			this.JD_RL_NKPortOfLoadingBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.JD_RL_NKPortOfLoadingBoundFindBox.TabIndex = 11;
			// 
			// ReceiveAgentContactsLinkButton
			// 
			this.ReceiveAgentContactsLinkButton.AutoSize = true;
			this.ReceiveAgentContactsLinkButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|f763e84b-7c37-498e-a95f-770255a03221", "View Contacts");
			this.ReceiveAgentContactsLinkButton.IsFontBold = false;
			this.ReceiveAgentContactsLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 186, true);
			this.ReceiveAgentContactsLinkButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ReceiveAgentContactsLinkButton.Name = "ReceiveAgentContactsLinkButton";
			this.ReceiveAgentContactsLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.ReceiveAgentContactsLinkButton.TabIndex = 16;
			this.ReceiveAgentContactsLinkButton.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnReceiveAgentContactsLinkButton_Click);
			// 
			// SendAgentContactsLinkButton
			// 
			this.SendAgentContactsLinkButton.AutoSize = true;
			this.SendAgentContactsLinkButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|143b449e-0974-4e07-9ea0-757470634e65", "View Contacts");
			this.SendAgentContactsLinkButton.IsFontBold = false;
			this.SendAgentContactsLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 164, true);
			this.SendAgentContactsLinkButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.SendAgentContactsLinkButton.Name = "SendAgentContactsLinkButton";
			this.SendAgentContactsLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.SendAgentContactsLinkButton.TabIndex = 13;
			this.SendAgentContactsLinkButton.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnSendAgentContactsLinkButton_Click);
			// 
			// JD_DepartureVesselCutoffDateBoundDateEdit
			// 
			this.JD_DepartureVesselCutoffDateBoundDateEdit.AllowDrop = true;
			this.JD_DepartureVesselCutoffDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_DepartureVesselCutoffDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_DepartureVesselCutoffDateBoundDateEdit, "JD_DepartureVesselCutoffDate");
			this.JD_DepartureVesselCutoffDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|66fffe88-86d9-4e51-aae8-d9681655fa8f", "Origin Cutoff");
			this.JD_DepartureVesselCutoffDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 4, true);
			this.JD_DepartureVesselCutoffDateBoundDateEdit.Name = "JD_DepartureVesselCutoffDateBoundDateEdit";
			this.JD_DepartureVesselCutoffDateBoundDateEdit.TabIndex = 0;
			// 
			// JD_OH_SendingAgentBoundFindBox
			// 
			this.JD_OH_SendingAgentBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_OH_SendingAgentBoundFindBox, "JD_OH_SendingAgent");
			this.JD_OH_SendingAgentBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 160, true);
			this.JD_OH_SendingAgentBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 18, 1, true);
			this.JD_OH_SendingAgentBoundFindBox.Name = "JD_OH_SendingAgentBoundFindBox";
			this.JD_OH_SendingAgentBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 18, true);
			this.JD_OH_SendingAgentBoundFindBox.TabIndex = 12;
			// 
			// JD_OH_ReceivingAgentBoundFindBox
			// 
			this.JD_OH_ReceivingAgentBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_OH_ReceivingAgentBoundFindBox, "JD_OH_ReceivingAgent");
			this.JD_OH_ReceivingAgentBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 182, true);
			this.JD_OH_ReceivingAgentBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 18, 1, true);
			this.JD_OH_ReceivingAgentBoundFindBox.Name = "JD_OH_ReceivingAgentBoundFindBox";
			this.JD_OH_ReceivingAgentBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 18, true);
			this.JD_OH_ReceivingAgentBoundFindBox.TabIndex = 15;
			// 
			// JD_PacksBoundCalcDropEdit
			// 
			this.JD_PacksBoundCalcDropEdit.AllowDrop = true;
			this.JD_PacksBoundCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JD_PacksBoundCalcDropEdit, ".");
			this.JD_PacksBoundCalcDropEdit.BindToAmount = "JD_Packs";
			this.JD_PacksBoundCalcDropEdit.BindToUnit = "JD_F3_NKPackType";
			this.JD_PacksBoundCalcDropEdit.Decimals = 0;
			this.JD_PacksBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(850, 138, true);
			this.JD_PacksBoundCalcDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_PacksBoundCalcDropEdit.Name = "JD_PacksBoundCalcDropEdit";
			this.JD_PacksBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.JD_PacksBoundCalcDropEdit.TabIndex = 17;
			this.JD_PacksBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JD_ActualVolumeBoundCalcDropEdit
			// 
			this.JD_ActualVolumeBoundCalcDropEdit.AllowDrop = true;
			this.JD_ActualVolumeBoundCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JD_ActualVolumeBoundCalcDropEdit, ".");
			this.JD_ActualVolumeBoundCalcDropEdit.BindToAmount = "JD_ActualVolume";
			this.JD_ActualVolumeBoundCalcDropEdit.BindToUnit = "JD_UnitOfVolume";
			this.JD_ActualVolumeBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(850, 160, true);
			this.JD_ActualVolumeBoundCalcDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_ActualVolumeBoundCalcDropEdit.Name = "JD_ActualVolumeBoundCalcDropEdit";
			this.JD_ActualVolumeBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.JD_ActualVolumeBoundCalcDropEdit.TabIndex = 18;
			this.JD_ActualVolumeBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JD_ActualWeightBoundCalcDropEdit
			// 
			this.JD_ActualWeightBoundCalcDropEdit.AllowDrop = true;
			this.JD_ActualWeightBoundCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JD_ActualWeightBoundCalcDropEdit, ".");
			this.JD_ActualWeightBoundCalcDropEdit.BindToAmount = "JD_ActualWeight";
			this.JD_ActualWeightBoundCalcDropEdit.BindToUnit = "JD_UnitOfWeight";
			this.JD_ActualWeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(850, 182, true);
			this.JD_ActualWeightBoundCalcDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_ActualWeightBoundCalcDropEdit.Name = "JD_ActualWeightBoundCalcDropEdit";
			this.JD_ActualWeightBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.JD_ActualWeightBoundCalcDropEdit.TabIndex = 19;
			this.JD_ActualWeightBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentTab
			// 
			this.ShipmentTab.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|c95bf899-7d7b-4dd4-8232-66b50d5c7bb3", "Shipment");
			this.ShipmentTab.Controls.Add(this.ConsolsGroupBox);
			this.ShipmentTab.Controls.Add(this.ShipmentGroupBox);
			this.ShipmentTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.ShipmentTab.Name = "ShipmentTab";
			this.ShipmentTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.ShipmentTab.AutoScroll = true;
			this.ShipmentTab.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.ShipmentTab.TabIndex = 1;
			// 
			// ConsolsGroupBox
			// 
			this.ConsolsGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|adda02fa-8eae-4113-ba71-072eacbe4bac", "Consols");
			this.ConsolsGroupBox.Controls.Add(this.ConsolsBoundButtonGrid);
			this.ConsolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 3, true);
			this.ConsolsGroupBox.Name = "ConsolsGroupBox";
			this.ConsolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 201, true);
			this.ConsolsGroupBox.TabIndex = 2;
			this.ConsolsGroupBox.TabStop = false;
			// 
			// ConsolsBoundButtonGrid
			// 
			this.ConsolsBoundButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolsBoundButtonGrid, "ConsolsForBinding");
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zDropEditColumnStyleInfo4.ColumnName = "JK_TransportMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|2d33dcdc-ce7a-44f5-bda8-f15fbaeccc52", "Load");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "JK_JX_JA_RL_NKPortOfLoading";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|ef88ad0e-84fc-4cde-9cfa-eec488b160b7", "ETD");
			zDateEditColumnStyleInfo7.ColumnName = "JK_JX_JA_E_DEP";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|530508a7-f6ae-4eec-b2fd-7d6b5594faa7", "Discharge");
			zCodeFindBoxColumnStyleInfo6.ColumnName = "JK_JX_JB_RL_NKPortOfDischarge";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|dbea2ac2-9007-4042-865d-c7a1ea381736", "ETA");
			zDateEditColumnStyleInfo8.ColumnName = "JK_JX_JB_E_ARV";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCodeFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|76606531-dc03-41fd-8da5-37351aad743b", "Voyage/Flight");
			zCodeFindBoxColumnStyleInfo7.ColumnName = "JK_JX_JV_NKVessel";
			zCodeFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|7a13dd97-c534-4c93-9c43-7b772a8094df", "Carrier", "Carrier", "Carrier", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ShippingLinePK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|9ba0e8da-83ba-46e1-995e-b4e66dba19cb", "Sending Agent");
			zGuidFindBoxColumnStyleInfo13.ColumnName = "SendingForwarderPK";
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "JK_ConsolMode";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JK_AgentType";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.ColumnName = "JK_IsNeutralMaster";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.ColumnName = "JK_BookingReference";
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo25.ColumnName = "JK_AgentsReference";
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.ColumnName = "JK_CustomsReference";
			zTextBoxColumnStyleInfo26.IsVisible = false;
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|706129ed-8f61-46d7-a18b-78d5b79c0c00", "DEP", "DEP", "");
			zDateEditColumnStyleInfo9.ColumnName = "JK_JX_JA_A_DEP";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|ee3a5e6b-9a11-46f9-ad04-2818c8012dfe", "ARV", "ARV", "");
			zDateEditColumnStyleInfo10.ColumnName = "JK_JX_JB_A_ARV";
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo27.ColumnName = "JK_ConsolStatus";
			zTextBoxColumnStyleInfo27.IsVisible = false;
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JK_ConsolChargeable";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "JK_ConsolChargeableRate";
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo28.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|533abad6-3554-47b5-8c8b-b3d5bc294de7", "Voyage", "Voyage / Flight No", "The Voyage or Flight Number.");
			zTextBoxColumnStyleInfo28.ColumnName = "JK_JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo28.IsVisible = false;
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo29.ColumnName = "JK_PrepaidCollect";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|41540eef-9460-4c4d-8df4-84334f5e5a57", "Co-Loader", "Co-Loader", "Coload Agent if the Consol Type is a Co-Load.A co loader is a forwarder that you use if your organization has chosen not to cut their own direct bill or consol.If your organization is not using a co loader as an Agent Type, this will be a read only field.");
			zGuidFindBoxColumnStyleInfo14.ColumnName = "CreditorPK";
			zGuidFindBoxColumnStyleInfo14.IsVisible = false;
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|53f16a9e-1e23-415c-a886-ad47071c6771", "Receiving Agent");
			zGuidFindBoxColumnStyleInfo15.ColumnName = "ReceivingForwarderPK";
			zGuidFindBoxColumnStyleInfo15.IsVisible = false;
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo30.ColumnName = "JK_RL_NKLastForeignPort";
			zTextBoxColumnStyleInfo30.IsVisible = false;
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo31.ColumnName = "JK_RL_NKFirstForeignPort";
			zTextBoxColumnStyleInfo31.IsVisible = false;
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo32.ColumnName = "JK_RL_NKPortOfFirstArrival";
			zTextBoxColumnStyleInfo32.IsVisible = false;
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo11.ColumnName = "JK_DateLastForeignPort";
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo12.ColumnName = "JK_DatePortOfFirstArrival";
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|B8F79FC0-30F4-4C4B-983E-BAE77CEBFB87", "CTO Arrival Address");
			zGuidFindBoxColumnStyleInfo16.ColumnName = "JK_OA_ArrivalCTOAddress";
			zGuidFindBoxColumnStyleInfo16.IsVisible = false;
			zGuidFindBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|0B68B04A-EFBA-45A0-B88F-762E237A5F68", "CTO Departure Address");
			zGuidFindBoxColumnStyleInfo17.ColumnName = "JK_OA_DepartureCTOAddress";
			zGuidFindBoxColumnStyleInfo17.IsVisible = false;
			zGuidFindBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo18.ColumnName = "JK_OA_PackDepotAddress";
			zGuidFindBoxColumnStyleInfo18.IsVisible = false;
			zGuidFindBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo19.ColumnName = "JK_OA_UnpackDepotAddress";
			zGuidFindBoxColumnStyleInfo19.IsVisible = false;
			zGuidFindBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|8DAE74D6-15C0-4CC1-9DC4-3EBD6322F612", "Container Yard Departure Address");
			zGuidFindBoxColumnStyleInfo20.ColumnName = "JK_OA_ContainerYardEmptyPickupAddress";
			zGuidFindBoxColumnStyleInfo20.IsVisible = false;
			zGuidFindBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|83DA7C23-1A65-4B0D-98B9-6E25AB784E0C", "Container Yard Arrival Address");
			zGuidFindBoxColumnStyleInfo21.ColumnName = "JK_OA_ContainerYardEmptyReturnAddress";
			zGuidFindBoxColumnStyleInfo21.IsVisible = false;
			zGuidFindBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo16);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo17);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo18);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo19);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo20);
			this.ConsolsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo21);
			this.ConsolsBoundButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolsBoundButtonGrid.GridId = "ec854c5c-7d93-450b-88d7-169c504fd51d";
			// 
			// 
			// 
			this.ConsolsBoundButtonGrid.InnerGrid.AllowNavigation = false;
			this.ConsolsBoundButtonGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolsBoundButtonGrid.InnerGrid.CaptionVisible = false;
			this.ConsolsBoundButtonGrid.InnerGrid.GridId = null;
			this.ConsolsBoundButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolsBoundButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ConsolsBoundButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ConsolsBoundButtonGrid.InnerGrid.Name = "Grid";
			this.ConsolsBoundButtonGrid.InnerGrid.ReadOnly = true;
			this.ConsolsBoundButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 147, true);
			this.ConsolsBoundButtonGrid.InnerGrid.TabIndex = 0;
			this.ConsolsBoundButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ConsolsBoundButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
			this.ConsolsBoundButtonGrid.Name = "ConsolsBoundButtonGrid";
			this.ConsolsBoundButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("C2785986-5A35-4E29-B901-90B10FEBBFE1", "Consol");
			this.ConsolsBoundButtonGrid.ReadOnly = true;
			this.ConsolsBoundButtonGrid.ShowAttachButton = false;
			this.ConsolsBoundButtonGrid.ShowDetachButton = false;
			this.ConsolsBoundButtonGrid.ShowNewButton = false;
			this.ConsolsBoundButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 184, true);
			this.ConsolsBoundButtonGrid.TabIndex = 20;
			// 
			// ShipmentGroupBox
			// 
			this.ShipmentGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|bb51429a-0b04-4983-b744-c4d01c9b5526", "Shipment");
			this.ShipmentGroupBox.Controls.Add(this.JD_JEBoundFindBox);
			this.ShipmentGroupBox.Controls.Add(this.JD_JSBoundFindBox);
			this.ShipmentGroupBox.Controls.Add(this.JD_VBBoundFindBox);
			this.ShipmentGroupBox.Controls.Add(this.ShipmentDetachButton);
			this.ShipmentGroupBox.Controls.Add(this.AttachedShipment_RL_NKOriginBoundCodeFindBox);
			this.ShipmentGroupBox.Controls.Add(this.AttachedShipment_RS_NKServiceLevelBoundFindBox);
			this.ShipmentGroupBox.Controls.Add(this.AttachedShipment_HouseBillBoundTextBox);
			this.ShipmentGroupBox.Controls.Add(this.AttachedShipment_RL_NKDestinationBoundCodeFindBox);
			this.ShipmentGroupBox.Controls.Add(this.JD_JELabel);
			this.ShipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShipmentGroupBox.Name = "ShipmentGroupBox";
			this.ShipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 201, true);
			this.ShipmentGroupBox.TabIndex = 1;
			this.ShipmentGroupBox.TabStop = false;
			// 
			// JD_JEBoundFindBox
			// 
			this.JD_JEBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_JEBoundFindBox, "JD_JE");
			this.JD_JEBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 72, true);
			this.JD_JEBoundFindBox.Name = "JD_JEBoundFindBox";
			this.JD_JEBoundFindBox.ShowDescriptionBox = false;
			this.JD_JEBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.JD_JEBoundFindBox.TabIndex = 1;
			// 
			// JD_JSBoundFindBox
			// 
			this.JD_JSBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_JSBoundFindBox, "JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder");
			this.JD_JSBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|eb5f7258-bc8d-46f1-8502-35a08c748e82", "Shipment No.");
			this.JD_JSBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 24, true);
			this.JD_JSBoundFindBox.Name = "JD_JSBoundFindBox";
			this.JD_JSBoundFindBox.ShowDescriptionBox = false;
			this.JD_JSBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.JD_JSBoundFindBox.TabIndex = 0;
			// 
			// ShipmentDetachButton
			// 
			this.ShipmentDetachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|305fd78b-8bbb-449b-b630-53294ef8f067", "Detach");
			this.ShipmentDetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 24, true);
			this.ShipmentDetachButton.Name = "ShipmentDetachButton";
			this.ShipmentDetachButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ShipmentDetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.ShipmentDetachButton.TabIndex = 2;
			this.ShipmentDetachButton.Click += new EventHandler(this.ShipmentDetachButton_Click);
			// 
			// JD_VBBoundFindBox
			// 
			this.JD_VBBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_VBBoundFindBox, "JD_VB_ThatAutoUpdatesBookingDetailsFromOrder");
			this.JD_VBBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|160eeedc-1158-4f40-9759-475c58bcc2cd", "Booking No.");
			this.JD_VBBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 48, true);
			this.JD_VBBoundFindBox.Name = "JD_VBBoundFindBox";
			this.JD_VBBoundFindBox.ShowDescriptionBox = false;
			this.JD_VBBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.JD_VBBoundFindBox.TabIndex = 0;
			// 
			// AttachedShipment_RL_NKOriginBoundCodeFindBox
			// 
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachedShipment_RL_NKOriginBoundCodeFindBox, "AttachedShipment_RL_NKOrigin");
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|9e8d5270-bba6-43f2-aae5-27fac5373624", "Origin");
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 120, true);
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.Name = "AttachedShipment_RL_NKOriginBoundCodeFindBox";
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.ShowDescriptionBox = false;
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.TabIndex = 4;
			// 
			// AttachedShipment_RS_NKServiceLevelBoundFindBox
			// 
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachedShipment_RS_NKServiceLevelBoundFindBox, "AttachedShipment_RS_NKServiceLevel");
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|32c920d3-afa3-487f-b145-eb2822eb93ce", "Service Level");
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 168, true);
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.Name = "AttachedShipment_RS_NKServiceLevelBoundFindBox";
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 18, true);
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.TabIndex = 8;
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.Tag = "Select the Service Level";
			// 
			// AttachedShipment_HouseBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AttachedShipment_HouseBillBoundTextBox, "AttachedShipment_HouseBill");
			this.AttachedShipment_HouseBillBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|7301bc56-0cf6-4ce2-ac60-01ee3fc26fe6", "House Bill");
			this.AttachedShipment_HouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 96, true);
			this.AttachedShipment_HouseBillBoundTextBox.Name = "AttachedShipment_HouseBillBoundTextBox";
			this.AttachedShipment_HouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 18, true);
			this.AttachedShipment_HouseBillBoundTextBox.TabIndex = 3;
			// 
			// AttachedShipment_RL_NKDestinationBoundCodeFindBox
			// 
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachedShipment_RL_NKDestinationBoundCodeFindBox, "AttachedShipment_RL_NKDestination");
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|a924eff2-2a1d-4ad7-8eb6-8ccfa012b2d3", "Destination");
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 144, true);
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.Name = "AttachedShipment_RL_NKDestinationBoundCodeFindBox";
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.ShowDescriptionBox = false;
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.TabIndex = 5;
			// 
			// JD_JELabel
			// 
			this.JD_JELabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|39178c7e-8f23-464d-9846-e07e9beaf434", "Declaration Ref:");
			this.JD_JELabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 72, true);
			this.JD_JELabel.Name = "JD_JELabel";
			this.JD_JELabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.JD_JELabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.JD_JELabel.TabIndex = 20;
			// 
			// CustomFieldsTab
			// 
			this.CustomFieldsTab.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|11119dca-704b-4000-ae0c-a7c7c0c65583", "Additional Detail");
			this.CustomFieldsTab.Controls.Add(this.OrderCustomFieldsDisplayControl);
			this.CustomFieldsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.CustomFieldsTab.Name = "CustomFieldsTab";
			this.CustomFieldsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.CustomFieldsTab.TabIndex = 3;
			// 
			// OrderCustomFieldsDisplayControl
			// 
			this.OrderCustomFieldsDisplayControl.AllowDrop = true;
			this.OrderCustomFieldsDisplayControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderCustomFieldsDisplayControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderCustomFieldsDisplayControl.Name = "OrderCustomFieldsDisplayControl";
			this.OrderCustomFieldsDisplayControl.NothingSetupMessageLabelText = "-- No Custom Fields Defined --";
			this.OrderCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.OrderCustomFieldsDisplayControl.TabIndex = 0;
			// 
			// BottomTabControl
			// 
			this.BottomTabControl.Controls.Add(this.OrdersTab);
			this.BottomTabControl.Controls.Add(this.PlanningTab);
			this.BottomTabControl.Controls.Add(this.ShipmentTab);
			this.BottomTabControl.Controls.Add(this.CustomFieldsTab);
			this.BottomTabControl.Controls.Add(this.ProductQuantitySummaryTabPage);
			this.BottomTabControl.Controls.Add(this.ChargesTabPage);
			this.BottomTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 19, true);
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 323, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 236, true);
			this.BottomTabControl.TabIndex = 5;
			this.BottomTabControl.Selecting += new TabControlCancelEventHandler(this.BottomTabControl_Selecting);
			// 
			// ProductQuantitySummaryTabPage
			// 
			this.ProductQuantitySummaryTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|466a490f-a21b-4535-be14-8c80060b8b50", "Product Quantity Summary");
			this.ProductQuantitySummaryTabPage.Controls.Add(this.ProductQuantitySummaryGrid);
			this.ProductQuantitySummaryTabPage.Controls.Add(this.OrderTotals);
			this.ProductQuantitySummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.ProductQuantitySummaryTabPage.Name = "ProductQuantitySummaryTabPage";
			this.ProductQuantitySummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductQuantitySummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 210, true);
			this.ProductQuantitySummaryTabPage.TabIndex = 5;
			this.ProductQuantitySummaryTabPage.UseVisualStyleBackColor = true;
			// 
			// ProductQuantitySummaryGrid
			// 
			this.ProductQuantitySummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductQuantitySummaryGrid, "ProductQuantitySummary");
			this.ProductQuantitySummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "Product";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.ColumnName = "ProductDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "QuantityInvoiced";
			zCalcEditColumnStyleInfo3.Decimals = 5;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "QuantityReceived";
			zCalcEditColumnStyleInfo14.Decimals = 5;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.ColumnName = "QuantityRemaining";
			zCalcEditColumnStyleInfo15.Decimals = 5;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|41f9fcf3-a755-4d7f-b4e9-cd14288102e8", "Inners", "Inner Packs", "The number of Inner Packs.");
			zCalcEditColumnStyleInfo16.ColumnName = "InnerPacks";
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|14719213-4608-4355-a3f6-5c125ab21dd6", "Outers", "Outer Packs", "The number of Outer Packs.");
			zCalcEditColumnStyleInfo17.ColumnName = "OuterPacks";
			zCalcEditColumnStyleInfo17.IsVisible = false;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.ProductQuantitySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.ProductQuantitySummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductQuantitySummaryGrid.GridId = "aae55d97-cecd-4bd6-a3d6-38a5dd7b01ad";
			this.ProductQuantitySummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductQuantitySummaryGrid.LayoutKey = "ProductQuantitySummaryGrid";
			this.ProductQuantitySummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProductQuantitySummaryGrid.Name = "ProductQuantitySummaryGrid";
			this.ProductQuantitySummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 179, true);
			this.ProductQuantitySummaryGrid.TabIndex = 0;
			// 
			// OrderTotals
			// 
			this.OrderTotals.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrderTotals, ".");
			this.OrderTotals.CaptionRenderingEnabled = true;
			this.OrderTotals.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OrderTotals.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 182, true);
			this.OrderTotals.Name = "OrderTotals";
			this.OrderTotals.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 25, true);
			this.OrderTotals.TabIndex = 1;
			// 
			// ChargesTabPage
			//
			this.ChargesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|3a568945-7964-4a42-b32a-127c0ccf2342", "Customs Valuation Charges");
			this.ChargesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ChargesTabPage.Controls.Add(this.ChargesGrid);
			this.ChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ChargesTabPage.Name = "ChargesTabPage";
			this.ChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 57, true);
			this.ChargesTabPage.TabIndex = 6;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargesGrid, "Charges");
			this.ChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "J7_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "J7_Amount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "J7_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "J7_ExchangeRate";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "J7_IsDutiable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "J7_IsGSTApplicable";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo7.ColumnName = "J7_IsIncludedInITOT";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo5.ColumnName = "J7_DistributeBy";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.ChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.GridId = "332083e1-6305-4377-8fd9-92f870280b0e";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 50, true);
			this.ChargesGrid.TabIndex = 1;
			// 
			// OrderDetailsGroupBox
			// 
			this.OrderDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|1d74365a-6778-4680-bf31-55b25d62a91b", "Order Details");
			this.OrderDetailsGroupBox.Controls.Add(this.IncoTermsExplainButton);
			this.OrderDetailsGroupBox.Controls.Add(this.zCodeFindBox1);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_EFBoundGuidFindBox);
			this.OrderDetailsGroupBox.Controls.Add(this.ReqInStoreDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.ReqExWorksDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_RXBoundCurrency);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_AdditionalTermsBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_OrderStatusBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_IsReleasedCheckbox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_RSBoundFindBox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_FollowUpDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_ContainerModeBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_TransportModeBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_IncoTermBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_InvoiceDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_InvoiceNumberBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_OrderDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_OrderNumberBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_RSLabel);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_BookingConfDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_OrderGoodsDescriptionBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_BookingConfRefBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.JD_OrderNumberSplitBoundTextBox);
			this.OrderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 3, true);
			this.OrderDetailsGroupBox.Name = "OrderDetailsGroupBox";
			this.OrderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 314, true);
			this.OrderDetailsGroupBox.TabIndex = 3;
			this.OrderDetailsGroupBox.TabStop = false;
			// 
			// IncoTermsExplainButton
			// 
			this.IncoTermsExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 194, true);
			this.IncoTermsExplainButton.Name = "IncoTermsExplainButton";
			this.IncoTermsExplainButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IncoTermsExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.IncoTermsExplainButton.TabIndex = 16;
			this.IncoTermsExplainButton.Text = "...";
			this.IncoTermsExplainButton.UseVisualStyleBackColor = true;
			this.IncoTermsExplainButton.Click += new EventHandler(this.IncoTermsExplainButton_Click);
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "JD_RN_NKCountryOfSupply");
			this.zCodeFindBox1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|7985fdab-ebbe-4d1f-b0e7-035f6b136be5", "Country/Region of Origin");
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 260, true);
			this.zCodeFindBox1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 2;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 18, true);
			this.zCodeFindBox1.TabIndex = 20;
			// 
			// JD_EFBoundGuidFindBox
			// 
			this.JD_EFBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_EFBoundGuidFindBox, "JD_EF_ShipmentPrePlanning");
			this.JD_EFBoundGuidFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|04773559-6944-43b2-a617-ad9dfd84a49b", "Pre Advice ID", "Shipment Pre-Advice the order is attached to.");
			this.JD_EFBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 282, true);
			this.JD_EFBoundGuidFindBox.Name = "JD_EFBoundGuidFindBox";
			this.JD_EFBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 18, true);
			this.JD_EFBoundGuidFindBox.TabIndex = 21;
			// 
			// ReqInStoreDateEdit
			// 
			this.ReqInStoreDateEdit.AllowDrop = true;
			this.ReqInStoreDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqInStoreDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqInStoreDateEdit, "JD_DeliveryRequiredBy");
			this.ReqInStoreDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 106, true);
			this.ReqInStoreDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ReqInStoreDateEdit.Name = "ReqInStoreDateEdit";
			this.ReqInStoreDateEdit.TabIndex = 11;
			// 
			// ReqExWorksDateEdit
			// 
			this.ReqExWorksDateEdit.AllowDrop = true;
			this.ReqExWorksDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqExWorksDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqExWorksDateEdit, "JD_ExWorksRequiredBy");
			this.ReqExWorksDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 106, true);
			this.ReqExWorksDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ReqExWorksDateEdit.Name = "ReqExWorksDateEdit";
			this.ReqExWorksDateEdit.TabIndex = 10;
			// 
			// JD_RXBoundCurrency
			// 
			this.JD_RXBoundCurrency.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RXBoundCurrency, "JD_Calc_Currency");
			this.JD_RXBoundCurrency.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|26e909b6-9dd2-459d-82bb-23b40bd8983e", "Currency");
			this.JD_RXBoundCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 150, true);
			this.JD_RXBoundCurrency.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_RXBoundCurrency.Name = "JD_RXBoundCurrency";
			this.JD_RXBoundCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.JD_RXBoundCurrency.TabIndex = 13;
			// 
			// JD_AdditionalTermsBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JD_AdditionalTermsBoundDropEdit, "JD_AdditionalTerms");
			this.JD_AdditionalTermsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 216, true);
			this.JD_AdditionalTermsBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_AdditionalTermsBoundDropEdit.Name = "JD_AdditionalTermsBoundDropEdit";
			this.JD_AdditionalTermsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 18, true);
			this.JD_AdditionalTermsBoundDropEdit.TabIndex = 17;
			// 
			// JD_OrderStatusBoundDropEdit
			// 
			this.JD_OrderStatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_OrderStatusBoundDropEdit, "JD_OrderStatus");
			this.JD_OrderStatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 40, true);
			this.JD_OrderStatusBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_OrderStatusBoundDropEdit.Name = "JD_OrderStatusBoundDropEdit";
			this.JD_OrderStatusBoundDropEdit.PreBoundMaxLength = 3;
			this.JD_OrderStatusBoundDropEdit.ShowDescriptionBox = false;
			this.JD_OrderStatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.JD_OrderStatusBoundDropEdit.TabIndex = 4;
			// 
			// JD_IsReleasedCheckbox
			// 
			this.BindingSource.SetBindingMember(this.JD_IsReleasedCheckbox, "JD_IsReleased");
			this.JD_IsReleasedCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 40, true);
			this.JD_IsReleasedCheckbox.Name = "JD_IsReleasedCheckbox";
			this.JD_IsReleasedCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 18, true);
			this.JD_IsReleasedCheckbox.TabIndex = 5;
			// 
			// JD_RSBoundFindBox
			// 
			this.JD_RSBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_RSBoundFindBox, "JD_RS_NKServiceLevel_NI");
			this.JD_RSBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 172, true);
			this.JD_RSBoundFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_RSBoundFindBox.Name = "JD_RSBoundFindBox";
			this.JD_RSBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 18, true);
			this.JD_RSBoundFindBox.TabIndex = 14;
			// 
			// JD_FollowUpDateBoundDateEdit
			// 
			this.JD_FollowUpDateBoundDateEdit.AllowDrop = true;
			this.JD_FollowUpDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_FollowUpDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_FollowUpDateBoundDateEdit, "JD_FollowUpDate");
			this.JD_FollowUpDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 150, true);
			this.JD_FollowUpDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_FollowUpDateBoundDateEdit.Name = "JD_FollowUpDateBoundDateEdit";
			this.JD_FollowUpDateBoundDateEdit.TabIndex = 13;
			// 
			// JD_ContainerModeBoundDropEdit
			// 
			this.JD_ContainerModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_ContainerModeBoundDropEdit, "JD_ContainerMode");
			this.JD_ContainerModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 239, true);
			this.JD_ContainerModeBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_ContainerModeBoundDropEdit.Name = "JD_ContainerModeBoundDropEdit";
			this.JD_ContainerModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JD_ContainerModeBoundDropEdit.ShowDescriptionBox = false;
			this.JD_ContainerModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.JD_ContainerModeBoundDropEdit.TabIndex = 19;
			// 
			// JD_TransportModeBoundDropEdit
			// 
			this.JD_TransportModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_TransportModeBoundDropEdit, "JD_TransportMode");
			this.JD_TransportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 238, true);
			this.JD_TransportModeBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_TransportModeBoundDropEdit.Name = "JD_TransportModeBoundDropEdit";
			this.JD_TransportModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JD_TransportModeBoundDropEdit.ShowDescriptionBox = false;
			this.JD_TransportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.JD_TransportModeBoundDropEdit.TabIndex = 18;
			// 
			// JD_IncoTermBoundDropEdit
			// 
			this.JD_IncoTermBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JD_IncoTermBoundDropEdit, "JD_IncoTerm");
			this.JD_IncoTermBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 194, true);
			this.JD_IncoTermBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_IncoTermBoundDropEdit.Name = "JD_IncoTermBoundDropEdit";
			this.JD_IncoTermBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 18, true);
			this.JD_IncoTermBoundDropEdit.TabIndex = 15;
			// 
			// JD_InvoiceDateBoundDateEdit
			// 
			this.JD_InvoiceDateBoundDateEdit.AllowDrop = true;
			this.JD_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_InvoiceDateBoundDateEdit, "JD_InvoiceDate");
			this.JD_InvoiceDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|643770D3-35EF-45FE-9A49-65A235069978", "Invoice Date");
			this.JD_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 84, true);
			this.JD_InvoiceDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_InvoiceDateBoundDateEdit.Name = "JD_InvoiceDateBoundDateEdit";
			this.JD_InvoiceDateBoundDateEdit.TabIndex = 9;
			// 
			// JD_InvoiceNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_InvoiceNumberBoundTextBox, "JD_InvoiceNumber");
			this.JD_InvoiceNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|1504fe71-3c92-473d-914e-39dce4e28de0", "Invoice No.", "Invoice Number.");
			this.JD_InvoiceNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 84, true);
			this.JD_InvoiceNumberBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_InvoiceNumberBoundTextBox.Name = "JD_InvoiceNumberBoundTextBox";
			this.JD_InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.JD_InvoiceNumberBoundTextBox.TabIndex = 8;
			// 
			// JD_OrderDateBoundDateEdit
			// 
			this.JD_OrderDateBoundDateEdit.AllowDrop = true;
			this.JD_OrderDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_OrderDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_OrderDateBoundDateEdit, "JD_OrderDate");
			this.JD_OrderDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|ACB99E90-A659-4A20-B0AF-E33E12541831", "Order Date");
			this.JD_OrderDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 18, true);
			this.JD_OrderDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_OrderDateBoundDateEdit.Name = "JD_OrderDateBoundDateEdit";
			this.JD_OrderDateBoundDateEdit.TabIndex = 3;
			// 
			// JD_OrderNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_OrderNumberBoundTextBox, "JD_OrderNumber");
			this.JD_OrderNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 18, true);
			this.JD_OrderNumberBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_OrderNumberBoundTextBox.Name = "JD_OrderNumberBoundTextBox";
			this.JD_OrderNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JD_OrderNumberBoundTextBox.TabIndex = 1;
			// 
			// JD_RSLabel
			// 
			this.JD_RSLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|901a3283-6102-4cdf-864a-b967c1d4ddeb", "Service Level");
			this.JD_RSLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 172, true);
			this.JD_RSLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_RSLabel.Name = "JD_RSLabel";
			this.JD_RSLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.JD_RSLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// JD_BookingConfDateBoundDateEdit
			// 
			this.JD_BookingConfDateBoundDateEdit.AllowDrop = true;
			this.JD_BookingConfDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_BookingConfDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_BookingConfDateBoundDateEdit, "JD_BookingConfDate");
			this.JD_BookingConfDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|3B17BE49-E4B2-4807-A662-D9DCAFA51B1A", "Booking Conf. Date");
			this.JD_BookingConfDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 62, true);
			this.JD_BookingConfDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_BookingConfDateBoundDateEdit.Name = "JD_BookingConfDateBoundDateEdit";
			this.JD_BookingConfDateBoundDateEdit.TabIndex = 7;
			// 
			// JD_OrderGoodsDescriptionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_OrderGoodsDescriptionBoundTextBox, "JD_OrderGoodsDescription");
			this.JD_OrderGoodsDescriptionBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|378ab01b-1236-4970-b120-401df1b60adb", "Goods Description");
			this.JD_OrderGoodsDescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JD_OrderGoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 128, true);
			this.JD_OrderGoodsDescriptionBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_OrderGoodsDescriptionBoundTextBox.Name = "JD_OrderGoodsDescriptionBoundTextBox";
			this.JD_OrderGoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 18, true);
			this.JD_OrderGoodsDescriptionBoundTextBox.TabIndex = 12;
			// 
			// JD_BookingConfRefBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_BookingConfRefBoundTextBox, "JD_BookingConfRef");
			this.JD_BookingConfRefBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|cecb8862-d809-401f-b320-f2548f891059", "Confirm No.", "Confirmation Number.");
			this.JD_BookingConfRefBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 62, true);
			this.JD_BookingConfRefBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_BookingConfRefBoundTextBox.Name = "JD_BookingConfRefBoundTextBox";
			this.JD_BookingConfRefBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.JD_BookingConfRefBoundTextBox.TabIndex = 6;
			// 
			// JD_OrderNumberSplitBoundTextBox
			// 
			this.JD_OrderNumberSplitBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JD_OrderNumberSplitBoundTextBox, "JD_OrderNumberSplit");
			this.JD_OrderNumberSplitBoundTextBox.DecimalPlaces = 0;
			this.JD_OrderNumberSplitBoundTextBox.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_OrderNumberSplitBoundTextBox, false);
			this.JD_OrderNumberSplitBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 18, true);
			this.JD_OrderNumberSplitBoundTextBox.Name = "JD_OrderNumberSplitBoundTextBox";
			this.JD_OrderNumberSplitBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.JD_OrderNumberSplitBoundTextBox.TabIndex = 2;
			this.JD_OrderNumberSplitBoundTextBox.Text = "0";
			this.JD_OrderNumberSplitBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RightTabControl.Controls.Add(this.MilestonesTabPage);
			this.RightTabControl.Controls.Add(this.OrderSplitsTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(738, 3, true);
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.SelectedIndex = 0;
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 314, true);
			this.RightTabControl.TabIndex = 4;
			// 
			// MilestonesTabPage
			// 
			this.MilestonesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|f5abc497-0b60-4321-9ded-419b53912cb0", "Tracking Dates");
			this.MilestonesTabPage.Controls.Add(this.OrderMilestones);
			this.MilestonesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MilestonesTabPage.Name = "MilestonesTabPage";
			this.MilestonesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MilestonesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 290, true);
			this.MilestonesTabPage.TabIndex = 0;
			this.MilestonesTabPage.UseVisualStyleBackColor = true;
			// 
			// OrderMilestones
			// 
			this.OrderMilestones.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrderMilestones, ".");
			this.OrderMilestones.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderMilestones.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrderMilestones.Name = "OrderMilestones";
			this.OrderMilestones.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 283, true);
			this.OrderMilestones.TabIndex = 0;
			// 
			// OrderSplitsTabPage
			// 
			this.OrderSplitsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersUserControl|8b976883-8cb9-4084-8448-74af25eec8f0", "Order Splits");
			this.OrderSplitsTabPage.Controls.Add(this.OrderSplitsButtonGrid);
			this.OrderSplitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OrderSplitsTabPage.Name = "OrderSplitsTabPage";
			this.OrderSplitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OrderSplitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 290, true);
			this.OrderSplitsTabPage.TabIndex = 1;
			this.OrderSplitsTabPage.UseVisualStyleBackColor = true;
			// 
			// BuyerZAddressWithContactControl
			// 
			this.BuyerZAddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerZAddressWithContactControl, "BuyerZAddressWithContact");
			this.BuyerZAddressWithContactControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0dabd5f7-6f01-4622-a83e-a7e491fd63c4", "Buyer");
			this.BuyerZAddressWithContactControl.ContactInfoTabVisible = false;
			this.BuyerZAddressWithContactControl.InvoiceContactTabCaption = Enterprise.Freight.Forwarding.GUI.Res.GetData("6b2e79c1-4d89-448c-9db5-2ca503beec79", "Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BuyerZAddressWithContactControl, false);
			this.BuyerZAddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BuyerZAddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.Name = "BuyerZAddressWithContactControl";
			this.BuyerZAddressWithContactControl.OnlyStopOnDebtor = false;
			this.BuyerZAddressWithContactControl.PopupCaption = "";
			this.BuyerZAddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.TabIndex = 1;
			// 
			// SupplierZAddressWithContactControl
			// 
			this.SupplierZAddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierZAddressWithContactControl, "SupplierZAddressWithContact");
			this.SupplierZAddressWithContactControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("72e2ee12-1fbd-40a2-a058-ab978e8b75ba", "Supplier");
			this.SupplierZAddressWithContactControl.ContactInfoTabVisible = false;
			this.SupplierZAddressWithContactControl.InvoiceContactTabCaption = Enterprise.Freight.Forwarding.GUI.Res.GetData("3dfd4ef6-ff52-481b-b2f0-3b43fbdac53d", "Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SupplierZAddressWithContactControl, false);
			this.SupplierZAddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 165, true);
			this.SupplierZAddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.SupplierZAddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.SupplierZAddressWithContactControl.Name = "SupplierZAddressWithContactControl";
			this.SupplierZAddressWithContactControl.OnlyStopOnDebtor = false;
			this.SupplierZAddressWithContactControl.PopupCaption = "";
			this.SupplierZAddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.SupplierZAddressWithContactControl.TabIndex = 2;
			// 
			// OrdersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplierZAddressWithContactControl);
			this.Controls.Add(this.BuyerZAddressWithContactControl);
			this.Controls.Add(this.RightTabControl);
			this.Controls.Add(this.OrderDetailsGroupBox);
			this.Controls.Add(this.BottomTabControl);
			this.Name = "OrdersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 559, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.JD_ShipmentWindowStartBoundDateEdit.ResumeLayout(true);
			this.JD_ShipmentWindowStartBoundDateEdit.PerformLayout();
			this.JD_ShipmentWindowEndBoundDateEdit.ResumeLayout(true);
			this.JD_ShipmentWindowEndBoundDateEdit.PerformLayout();
			((ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).EndInit();
			this.OrderSplitsButtonGrid.ResumeLayout(true);
			this.OrderSplitsButtonGrid.PerformLayout();
			this.OrdersTab.ResumeLayout(false);
			this.OrdersTab.PerformLayout();
			((ISupportInitialize)(this.OrderLinesBoundButtonGrid.InnerGrid)).EndInit();
			this.OrderLinesBoundButtonGrid.ResumeLayout(true);
			this.OrderLinesBoundButtonGrid.PerformLayout();
			this.PlanningTab.ResumeLayout(false);
			this.PlanningTab.PerformLayout();
			this.OrderPlanningVesselVoyageAndDates.ResumeLayout(true);
			this.OrderPlanningVesselVoyageAndDates.PerformLayout();
			this.PlannedContainersGroupBox.ResumeLayout(false);
			this.PlannedContainersGroupBox.PerformLayout();
			((ISupportInitialize)(this.PlannedContainersGrid)).EndInit();
			this.PlannedContainersGrid.ResumeLayout(false);
			this.PlannedContainersGrid.PerformLayout();
			this.JD_OH_CarrierBoundFindBox.ResumeLayout(true);
			this.JD_OH_CarrierBoundFindBox.PerformLayout();
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.ResumeLayout(true);
			this.JD_RL_NKGoodsDeliveredToBoundFindBox.PerformLayout();
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.ResumeLayout(true);
			this.JD_RL_NKGoodsAvailableAtBoundFindBox.PerformLayout();
			this.JD_RL_NKPortOfDischargeBoundFindBox.ResumeLayout(true);
			this.JD_RL_NKPortOfDischargeBoundFindBox.PerformLayout();
			this.JD_RL_NKPortOfLoadingBoundFindBox.ResumeLayout(true);
			this.JD_RL_NKPortOfLoadingBoundFindBox.PerformLayout();
			this.JD_DepartureVesselCutoffDateBoundDateEdit.ResumeLayout(true);
			this.JD_DepartureVesselCutoffDateBoundDateEdit.PerformLayout();
			this.JD_OH_SendingAgentBoundFindBox.ResumeLayout(true);
			this.JD_OH_SendingAgentBoundFindBox.PerformLayout();
			this.JD_OH_ReceivingAgentBoundFindBox.ResumeLayout(true);
			this.JD_OH_ReceivingAgentBoundFindBox.PerformLayout();
			this.JD_PacksBoundCalcDropEdit.ResumeLayout(true);
			this.JD_PacksBoundCalcDropEdit.PerformLayout();
			this.JD_ActualVolumeBoundCalcDropEdit.ResumeLayout(true);
			this.JD_ActualVolumeBoundCalcDropEdit.PerformLayout();
			this.JD_ActualWeightBoundCalcDropEdit.ResumeLayout(true);
			this.JD_ActualWeightBoundCalcDropEdit.PerformLayout();
			this.ShipmentTab.ResumeLayout(false);
			this.ShipmentTab.PerformLayout();
			this.ConsolsGroupBox.ResumeLayout(false);
			this.ConsolsGroupBox.PerformLayout();
			((ISupportInitialize)(this.ConsolsBoundButtonGrid.InnerGrid)).EndInit();
			this.ConsolsBoundButtonGrid.ResumeLayout(true);
			this.ConsolsBoundButtonGrid.PerformLayout();
			this.ShipmentGroupBox.ResumeLayout(false);
			this.ShipmentGroupBox.PerformLayout();
			this.JD_JEBoundFindBox.ResumeLayout(true);
			this.JD_JEBoundFindBox.PerformLayout();
			this.JD_JSBoundFindBox.ResumeLayout(true);
			this.JD_JSBoundFindBox.PerformLayout();
			this.JD_VBBoundFindBox.ResumeLayout(true);
			this.JD_VBBoundFindBox.PerformLayout();
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.ResumeLayout(true);
			this.AttachedShipment_RL_NKOriginBoundCodeFindBox.PerformLayout();
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.AttachedShipment_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.ResumeLayout(true);
			this.AttachedShipment_RL_NKDestinationBoundCodeFindBox.PerformLayout();
			this.CustomFieldsTab.ResumeLayout(false);
			this.CustomFieldsTab.PerformLayout();
			this.OrderCustomFieldsDisplayControl.ResumeLayout(true);
			this.OrderCustomFieldsDisplayControl.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.ProductQuantitySummaryTabPage.ResumeLayout(false);
			this.ProductQuantitySummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.ProductQuantitySummaryGrid)).EndInit();
			this.ProductQuantitySummaryGrid.ResumeLayout(false);
			this.ProductQuantitySummaryGrid.PerformLayout();
			this.OrderTotals.ResumeLayout(true);
			this.OrderTotals.PerformLayout();
			this.ChargesTabPage.ResumeLayout(false);
			this.ChargesTabPage.PerformLayout();
			((ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.OrderDetailsGroupBox.ResumeLayout(false);
			this.OrderDetailsGroupBox.PerformLayout();
			this.zCodeFindBox1.ResumeLayout(true);
			this.zCodeFindBox1.PerformLayout();
			this.JD_EFBoundGuidFindBox.ResumeLayout(true);
			this.JD_EFBoundGuidFindBox.PerformLayout();
			this.ReqInStoreDateEdit.ResumeLayout(true);
			this.ReqInStoreDateEdit.PerformLayout();
			this.ReqExWorksDateEdit.ResumeLayout(true);
			this.ReqExWorksDateEdit.PerformLayout();
			this.JD_RXBoundCurrency.ResumeLayout(true);
			this.JD_RXBoundCurrency.PerformLayout();
			this.JD_OrderStatusBoundDropEdit.ResumeLayout(true);
			this.JD_OrderStatusBoundDropEdit.PerformLayout();
			this.JD_RSBoundFindBox.ResumeLayout(true);
			this.JD_RSBoundFindBox.PerformLayout();
			this.JD_FollowUpDateBoundDateEdit.ResumeLayout(true);
			this.JD_FollowUpDateBoundDateEdit.PerformLayout();
			this.JD_ContainerModeBoundDropEdit.ResumeLayout(true);
			this.JD_ContainerModeBoundDropEdit.PerformLayout();
			this.JD_TransportModeBoundDropEdit.ResumeLayout(true);
			this.JD_TransportModeBoundDropEdit.PerformLayout();
			this.JD_IncoTermBoundDropEdit.ResumeLayout(true);
			this.JD_IncoTermBoundDropEdit.PerformLayout();
			this.JD_InvoiceDateBoundDateEdit.ResumeLayout(true);
			this.JD_InvoiceDateBoundDateEdit.PerformLayout();
			this.JD_OrderDateBoundDateEdit.ResumeLayout(true);
			this.JD_OrderDateBoundDateEdit.PerformLayout();
			this.JD_BookingConfDateBoundDateEdit.ResumeLayout(true);
			this.JD_BookingConfDateBoundDateEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.MilestonesTabPage.ResumeLayout(false);
			this.MilestonesTabPage.PerformLayout();
			this.OrderMilestones.ResumeLayout(true);
			this.OrderMilestones.PerformLayout();
			this.OrderSplitsTabPage.ResumeLayout(false);
			this.OrderSplitsTabPage.PerformLayout();
			this.BuyerZAddressWithContactControl.ResumeLayout(true);
			this.BuyerZAddressWithContactControl.PerformLayout();
			this.SupplierZAddressWithContactControl.ResumeLayout(true);
			this.SupplierZAddressWithContactControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
