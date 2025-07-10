using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageUserControl
	{
		private ZGuidFindBox BranchGuidFindBox;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl FirstDocAddressControl;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl SecondDocAddressControl;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl ThirdDocAddressControl;
		private ZDateEdit JJ_EstimatedDeliveryDateEdit;
		private ZDateEdit JJ_EstimatedPickupDateEdit;
		private ZDropEdit JJ_ContainerDropModeDropDownEdit;
		private ZGroupBox ContainerLegSummaryGroupBox;
		internal ZGrid SummaryGrid;
		internal ZDropEdit JJ_CartageTypeDropEdit;
		private ZGroupBox ClientDetailsGroupBox;
		private ZTextBox JJ_WaybillNumberTextBox;
		private ZTextBox JJ_QuoteNumberTextBox;
		private ZTextBox JJ_GoodsDescriptionTextBox;
		private ZTextBox JJ_OrderReferenceNumberTextBox;
		private ZDateEdit JJ_A_JCLDateEdit;
		private ZCodeFindBox zCodeFindBox1;
		private ZGroupBox CartageDetailsGroupBox;
		private ZCalcDropEdit JJ_OuterPacksCalcDropEdit;
		private ZCalcDropEdit JJ_WeightCalcDropEdit;
		private ZCalcDropEdit JJ_VolumeCalcDropEdit;
		protected internal ZLinkLabel AddressesLinkedToJobLinkLabel;
		private ZPanel MainJobPanel;
		private ZPanel AddressesPanel;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl FourthDocAddressControl;
		private ZPanel TransportLegSummaryPanel;
		private ZPanel zPanel1;
		internal ZLabel JobHeaderMutexErrorLabel;
		private ZGroupBox SailingDetailsGroupBox;
		private ZPanel ExportDatesPanel;
		private ZDateEdit JJ_JX_LCLReceivalCommencesDateEdit;
		private ZDateEdit JJ_JX_LCLCutOffDateEdit;
		private ZDateEdit JJ_JX_FCLReceivalCommencesDateEdit;
		private ZDateEdit JJ_JX_FCLCutOffDateEdit;
		private ZCodeFindBox JJ_JA_NKPortOfLoadingFindBox;
		private ZCodeFindBox JJ_JB_NKPortOfDischargeFindBox;
		private ZCodeFindBox JJ_JV_NKVesselFindBox;
		private ZDateEdit JJ_JB_E_ARVDateEdit;
		private ZDateEdit JJ_JA_E_DEPDateEdit;
		private ZButton SelectSchedulesButton;
		private ZTextBox JX_VoyageTextBox;
		private ZPanel ImportDatesPanel;
		private ZDateEdit JJ_JX_LCLAvailabilityDateDateEdit;
		private ZDateEdit JJ_JX_LCLStorageDateDateEdit;
		private ZDateEdit JJ_JX_AvailabilityDateDateEdit;
		internal ZOrgAddressControl LocalClientOrgControl;
		private ZCalcDropEdit GrossWeightCalcDropEdit;
		private ZDropEdit TransportModeDropEdit;
		private ZDropEdit DirectionDropEdit;
		private ZDropEdit ContainerModeDropEdit;
		internal ZLabel ClickSaveToCreateJobLabel;
		private ZDateEdit JJ_JX_StorageDateDateEdit;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ThirdDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SecondDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.FirstDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JJ_EstimatedDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_EstimatedPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_ContainerDropModeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerLegSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SummaryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.JJ_CartageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClientDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JJ_WaybillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JJ_QuoteNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JJ_GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JJ_OrderReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JJ_A_JCLDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CartageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_OuterPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AddressesLinkedToJobLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.AddressesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LocalClientOrgControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.SailingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JJ_JA_NKPortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JJ_JB_NKPortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JJ_JV_NKVesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JJ_JB_E_ARVDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JA_E_DEPDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SelectSchedulesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JX_VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportDatesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JJ_JX_LCLAvailabilityDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_LCLStorageDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_AvailabilityDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_StorageDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportDatesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JJ_JX_LCLReceivalCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_LCLCutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_FCLReceivalCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_FCLCutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FourthDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JobHeaderMutexErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainJobPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportLegSummaryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClickSaveToCreateJobLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerLegSummaryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).BeginInit();
			this.ClientDetailsGroupBox.SuspendLayout();
			this.CartageDetailsGroupBox.SuspendLayout();
			this.AddressesPanel.SuspendLayout();
			this.SailingDetailsGroupBox.SuspendLayout();
			this.ImportDatesPanel.SuspendLayout();
			this.ExportDatesPanel.SuspendLayout();
			this.MainJobPanel.SuspendLayout();
			this.TransportLegSummaryPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartage);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JJ_GB");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 26, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PopupCaption = null;
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 21, true);
			this.BranchGuidFindBox.TabIndex = 3;
			// 
			// ThirdDocAddressControl
			// 
			this.ThirdDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ThirdDocAddressControl, "ThirdDocAddresses");
			this.ThirdDocAddressControl.BindToOrganisations = "Lookups+ThirdAddressList";
			this.ThirdDocAddressControl.CaptionResourceString = null;
			this.ThirdDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 3, true);
			this.ThirdDocAddressControl.Name = "ThirdDocAddressControl";
			this.ThirdDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ThirdDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ThirdDocAddressControl.TabIndex = 2;
			// 
			// SecondDocAddressControl
			// 
			this.SecondDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondDocAddressControl, "SecondDocAddresses");
			this.SecondDocAddressControl.BindToOrganisations = "Lookups+SecondAddressList";
			this.SecondDocAddressControl.CaptionResourceString = null;
			this.SecondDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 3, true);
			this.SecondDocAddressControl.Name = "SecondDocAddressControl";
			this.SecondDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SecondDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.SecondDocAddressControl.TabIndex = 1;
			// 
			// FirstDocAddressControl
			// 
			this.FirstDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstDocAddressControl, "FirstDocAddresses");
			this.FirstDocAddressControl.BindToOrganisations = "Lookups+FirstAddressList";
			this.FirstDocAddressControl.CaptionResourceString = null;
			this.FirstDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 3, true);
			this.FirstDocAddressControl.Name = "FirstDocAddressControl";
			this.FirstDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.FirstDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.FirstDocAddressControl.TabIndex = 0;
			// 
			// JJ_EstimatedDeliveryDateEdit
			// 
			this.JJ_EstimatedDeliveryDateEdit.AllowDrop = true;
			this.JJ_EstimatedDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_EstimatedDeliveryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_EstimatedDeliveryDateEdit, "JJ_EstimatedDelivery");
			this.JJ_EstimatedDeliveryDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|167ae499-ab5b-426f-83be-35cdd4b11843", "Est. Delivery", "Est. Delivery", "Est. Delivery", "Estimated Delivery.");
			this.JJ_EstimatedDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_EstimatedDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 26, true);
			this.JJ_EstimatedDeliveryDateEdit.Name = "JJ_EstimatedDeliveryDateEdit";
			this.JJ_EstimatedDeliveryDateEdit.TabIndex = 6;
			// 
			// JJ_EstimatedPickupDateEdit
			// 
			this.JJ_EstimatedPickupDateEdit.AllowDrop = true;
			this.JJ_EstimatedPickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_EstimatedPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_EstimatedPickupDateEdit, "JJ_EstimatedPickup");
			this.JJ_EstimatedPickupDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|bfcdf2db-339f-4557-9e5c-b777a01bb6e0", "Est. Pickup", "Est. Pickup", "Est. Pickup", "Estimated Pickup.");
			this.JJ_EstimatedPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_EstimatedPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 3, true);
			this.JJ_EstimatedPickupDateEdit.Name = "JJ_EstimatedPickupDateEdit";
			this.JJ_EstimatedPickupDateEdit.TabIndex = 5;
			// 
			// JJ_ContainerDropModeDropDownEdit
			// 
			this.JJ_ContainerDropModeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_ContainerDropModeDropDownEdit, "JJ_DropMode");
			this.JJ_ContainerDropModeDropDownEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|30e228bb-de7f-43c6-a36d-0c2c05419e87", "Drop Mode", "Drop Mode", "Drop Mode", "");
			this.JJ_ContainerDropModeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 25, true);
			this.JJ_ContainerDropModeDropDownEdit.Name = "JJ_ContainerDropModeDropDownEdit";
			this.JJ_ContainerDropModeDropDownEdit.PreBoundMaxLength = 3;
			this.JJ_ContainerDropModeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.JJ_ContainerDropModeDropDownEdit.TabIndex = 4;
			// 
			// ContainerLegSummaryGroupBox
			// 
			this.ContainerLegSummaryGroupBox.Controls.Add(this.SummaryGrid);
			this.ContainerLegSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContainerLegSummaryGroupBox, false);
			this.ContainerLegSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerLegSummaryGroupBox.Name = "ContainerLegSummaryGroupBox";
			this.ContainerLegSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 134, true);
			this.ContainerLegSummaryGroupBox.TabIndex = 0;
			this.ContainerLegSummaryGroupBox.TabStop = false;
			// 
			// SummaryGrid
			// 
			this.SummaryGrid.AllowNavigation = false;
			this.SummaryGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SummaryGrid, "CartageLegs");
			this.SummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Container+JC_ContainerNum";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|71abd933-d6e7-4512-81a7-05e1bb5e5ae7", "Pickup", "Pickup Company", "Pickup Company Name", "");
			zTextBoxColumnStyleInfo2.ColumnName = "PickupFromDocAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|5ea98016-1931-466c-8e6b-0be3a7916832", "P.City", "Pickup City", "");
			zTextBoxColumnStyleInfo3.ColumnName = "PickupFromDocAddress+E2_City";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "JU_PlannedPickupTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zDateEditColumnStyleInfo2.ColumnName = "JU_PickupTimeIn";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.ColumnName = "JU_PickupTimeOut";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|f074918d-48f1-4268-925d-2d53ff9073ff", "Delivery", "Delivery Company", "Delivery Company Name", "");
			zTextBoxColumnStyleInfo4.ColumnName = "DeliverToDocAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|d94a1464-a0a6-4855-8132-8af013a44ef1", "D.City", "Delivery City", "");
			zTextBoxColumnStyleInfo5.ColumnName = "DeliverToDocAddress+E2_City";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo4.ColumnName = "JU_EstimatedDeliveryTime";
			zDateEditColumnStyleInfo5.ColumnName = "JU_DeliverTimeIn";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo6.ColumnName = "JU_DeliverTimeOut";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WorkSheet+EY_RQ_Truck";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "WorkSheet+EY_GS_NKTruckDriver";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.ColumnName = "JU_DeliverySignedFor";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCheckBoxColumnStyleInfo1.ColumnName = "JU_IsEmptyContainer";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zDateEditColumnStyleInfo7.ColumnName = "JU_PlannedPickupTimeEnd";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo8.ColumnName = "JU_EstimatedDeliveryTimeEnd";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "JU_E2PickupAddressID";
			zGuidDropEditColumnStyleInfo1.IsReadOnly = true;
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo2.ColumnName = "JU_E2WaitPointAddressID";
			zGuidDropEditColumnStyleInfo2.IsVisible = false;
			zGuidDropEditColumnStyleInfo3.ColumnName = "JU_E2DeliveryAddressID";
			zGuidDropEditColumnStyleInfo3.IsReadOnly = true;
			zGuidDropEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|4256d3d6-58ff-499c-88a9-435644caed4e", "W.City", "Wait Point City", "");
			zTextBoxColumnStyleInfo7.ColumnName = "WaitPointDocAddress+E2_City";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|261f5cc5-c559-4f6a-bae2-130e9908c52b", "P.Postcode", "Pickup Postcode", "");
			zTextBoxColumnStyleInfo8.ColumnName = "PickupFromDocAddress+E2_Postcode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|d0d689bf-e2fd-4686-8146-10af4cc86403", "W.Postcode", "Wait Point Postcode", "");
			zTextBoxColumnStyleInfo9.ColumnName = "WaitPointDocAddress+E2_Postcode";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|dd547eed-18c6-4cf1-8e91-5329a41ed1a1", "D.Postcode", "Delivery Postcode", "");
			zTextBoxColumnStyleInfo10.ColumnName = "DeliverToDocAddress+E2_Postcode";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo11.ColumnName = "WorkSheet+EY_TruckRegistration";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WorkSheet+EY_OH_TransportCo";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo12.ColumnName = "WorkSheet+EY_DriversName";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo13.ColumnName = "WorkSheet+EY_DriversLicence";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JU_RQ_ExtraEquip1";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JU_RQ_ExtraEquip2";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|af3bbf3d-2e8b-4774-b133-2948062acd71", "Wait Point", "Wait Point Company", "Wait Point Company Name", "");
			zTextBoxColumnStyleInfo14.ColumnName = "WaitPointDocAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BookedCtgMove+EW_BookedPackCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|e58cb390-0214-4709-94ae-b5a569880e2c", "Booked Packages");
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo15.ColumnName = "BookedCtgMove+EW_F3_NKPackType";
			zTextBoxColumnStyleInfo15.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|e58cb390-0214-4709-94ae-b5a569880e2c", "Booked Packages");
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BookedCtgMove+EW_BookedHeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|41ffafac-8a07-4a63-94b4-cdec860d6801", "Booked Dimensions");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BookedCtgMove+EW_BookedLength";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|41ffafac-8a07-4a63-94b4-cdec860d6801", "Booked Dimensions");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "BookedCtgMove+EW_BookedWidth";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|41ffafac-8a07-4a63-94b4-cdec860d6801", "Booked Dimensions");
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo16.ColumnName = "BookedCtgMove+EW_DimUnit";
			zTextBoxColumnStyleInfo16.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|41ffafac-8a07-4a63-94b4-cdec860d6801", "Booked Dimensions");
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "BookedCtgMove+EW_BookedWeight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|8f4673d0-1ca6-4500-9e41-1f9f6c9e3df9", "Booked Weight");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo17.ColumnName = "BookedCtgMove+EW_WeightUQ";
			zTextBoxColumnStyleInfo17.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|8f4673d0-1ca6-4500-9e41-1f9f6c9e3df9", "Booked Weight");
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "BookedCtgMove+EW_BookedVolume";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|9421e4a7-5052-47fb-8248-c4205bd6edc7", "Booked Volume");
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo18.ColumnName = "BookedCtgMove+EW_VolumeUQ";
			zTextBoxColumnStyleInfo18.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|9421e4a7-5052-47fb-8248-c4205bd6edc7", "Booked Volume");
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo9.ColumnName = "BookedCtgMove+EW_RequestedPickupTimeStart";
			zDateEditColumnStyleInfo9.IsReadOnly = true;
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo10.ColumnName = "BookedCtgMove+EW_RequestedPickupTimeEnd";
			zDateEditColumnStyleInfo10.IsReadOnly = true;
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo11.ColumnName = "BookedCtgMove+EW_RequestedDeliveryTimeStart";
			zDateEditColumnStyleInfo11.IsReadOnly = true;
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo12.ColumnName = "BookedCtgMove+EW_RequestedDeliveryTimeEnd";
			zDateEditColumnStyleInfo12.IsReadOnly = true;
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JU_LegNotes";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.ColumnName = "TruckDescription";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zDateEditColumnStyleInfo13.ColumnName = "Container+JC_EmptyReturnedBy";
			zDateEditColumnStyleInfo13.IsReadOnly = true;
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo1.ColumnName = "JU_MessageStatus";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "PostcodeDistance";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|cdde143f-e684-4979-b05e-109093851b4b", "Postcode Distance");
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "JU_DistanceUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|cdde143f-e684-4979-b05e-109093851b4b", "Postcode Distance");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "JU_Distance";
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|b39cb7ae-e690-4f96-92dc-6fe3c5eb4dd8", "Driving Distance");
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "JU_DistanceUnit";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|b39cb7ae-e690-4f96-92dc-6fe3c5eb4dd8", "Driving Distance");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.SummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.SummaryGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.SummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.SummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.SummaryGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.SummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.SummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.SummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.SummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SummaryGrid.CopySelectedRowsAllowed = true;
			this.SummaryGrid.GridId = "5105b6e6-580d-49fd-8946-9fae7247da24";
			this.SummaryGrid.GridLineColor = System.Drawing.Color.Gainsboro;
			this.SummaryGrid.LayoutKey = "SummaryGrid";
			this.SummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SummaryGrid.Name = "SummaryGrid";
			this.SummaryGrid.ParentRowsBackColor = System.Drawing.Color.Gainsboro;
			this.SummaryGrid.ReadOnly = true;
			this.SummaryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 115, true);
			this.SummaryGrid.TabIndex = 0;
			// 
			// JJ_CartageTypeDropEdit
			// 
			this.JJ_CartageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_CartageTypeDropEdit, "JJ_E3_NKJobType");
			this.JJ_CartageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 3, true);
			this.JJ_CartageTypeDropEdit.Name = "JJ_CartageTypeDropEdit";
			this.JJ_CartageTypeDropEdit.PreBoundMaxLength = 4;
			this.JJ_CartageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.JJ_CartageTypeDropEdit.TabIndex = 0;
			// 
			// ClientDetailsGroupBox
			// 
			this.ClientDetailsGroupBox.Controls.Add(this.JJ_WaybillNumberTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.JJ_QuoteNumberTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.JJ_GoodsDescriptionTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.JJ_OrderReferenceNumberTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.JJ_A_JCLDateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientDetailsGroupBox, false);
			this.ClientDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 186, true);
			this.ClientDetailsGroupBox.Name = "ClientDetailsGroupBox";
			this.ClientDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 174, true);
			this.ClientDetailsGroupBox.TabIndex = 5;
			this.ClientDetailsGroupBox.TabStop = false;
			// 
			// JJ_WaybillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JJ_WaybillNumberTextBox, "JJ_WaybillNumber");
			this.JJ_WaybillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 16, true);
			this.JJ_WaybillNumberTextBox.Name = "JJ_WaybillNumberTextBox";
			this.JJ_WaybillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.JJ_WaybillNumberTextBox.TabIndex = 0;
			// 
			// JJ_QuoteNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JJ_QuoteNumberTextBox, "JJ_QuoteNumber");
			this.JJ_QuoteNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 82, true);
			this.JJ_QuoteNumberTextBox.Name = "JJ_QuoteNumberTextBox";
			this.JJ_QuoteNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.JJ_QuoteNumberTextBox.TabIndex = 3;
			// 
			// JJ_GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JJ_GoodsDescriptionTextBox, "JJ_GoodsDescription");
			this.JJ_GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 60, true);
			this.JJ_GoodsDescriptionTextBox.Name = "JJ_GoodsDescriptionTextBox";
			this.JJ_GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.JJ_GoodsDescriptionTextBox.TabIndex = 2;
			// 
			// JJ_OrderReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JJ_OrderReferenceNumberTextBox, "JJ_OrderReferenceNumber");
			this.JJ_OrderReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 38, true);
			this.JJ_OrderReferenceNumberTextBox.Name = "JJ_OrderReferenceNumberTextBox";
			this.JJ_OrderReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.JJ_OrderReferenceNumberTextBox.TabIndex = 1;
			// 
			// JJ_A_JCLDateEdit
			// 
			this.JJ_A_JCLDateEdit.AllowDrop = true;
			this.JJ_A_JCLDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_A_JCLDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_A_JCLDateEdit, "JJ_A_JCL");
			this.JJ_A_JCLDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_A_JCLDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 104, true);
			this.JJ_A_JCLDateEdit.Name = "JJ_A_JCLDateEdit";
			this.JJ_A_JCLDateEdit.TabIndex = 4;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "JJ_RS_NKServiceLevel");
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 3, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PopupCaption = null;
			this.zCodeFindBox1.PreBoundMaxLength = 3;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 21, true);
			this.zCodeFindBox1.TabIndex = 3;
			// 
			// CartageDetailsGroupBox
			// 
			this.CartageDetailsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|2e7e5481-f1ca-4d79-a4f1-82f79405eea5", "Totals", "Totals", "Totals", "");
			this.CartageDetailsGroupBox.Controls.Add(this.GrossWeightCalcDropEdit);
			this.CartageDetailsGroupBox.Controls.Add(this.JJ_OuterPacksCalcDropEdit);
			this.CartageDetailsGroupBox.Controls.Add(this.JJ_WeightCalcDropEdit);
			this.CartageDetailsGroupBox.Controls.Add(this.JJ_VolumeCalcDropEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CartageDetailsGroupBox, false);
			this.CartageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(925, 186, true);
			this.CartageDetailsGroupBox.Name = "CartageDetailsGroupBox";
			this.CartageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 174, true);
			this.CartageDetailsGroupBox.TabIndex = 7;
			this.CartageDetailsGroupBox.TabStop = false;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			this.GrossWeightCalcDropEdit.BindToAmount = "GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "JJ_WeightUQ";
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|2dbec566-10c4-49de-b2af-623f34134f9a", "Gross Weight", "Total Gross Weight", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.GrossWeightCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 150, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 3;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JJ_OuterPacksCalcDropEdit
			// 
			this.JJ_OuterPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_OuterPacksCalcDropEdit, ".");
			this.JJ_OuterPacksCalcDropEdit.BindToAmount = "JJ_OuterPacks";
			this.JJ_OuterPacksCalcDropEdit.BindToUnit = "JJ_F3_NKPackType";
			this.JJ_OuterPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|dd621bca-c77b-4ee7-873d-f1c76cd253ff", "Total Goods Packs", "Total Goods Packs", "Total Goods Packs", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_OuterPacksCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_OuterPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 31, true);
			this.JJ_OuterPacksCalcDropEdit.Name = "JJ_OuterPacksCalcDropEdit";
			this.JJ_OuterPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.JJ_OuterPacksCalcDropEdit.TabIndex = 0;
			this.JJ_OuterPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JJ_WeightCalcDropEdit
			// 
			this.JJ_WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_WeightCalcDropEdit, ".");
			this.JJ_WeightCalcDropEdit.BindToAmount = "JJ_Weight";
			this.JJ_WeightCalcDropEdit.BindToUnit = "JJ_WeightUQ";
			this.JJ_WeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|fb7b76c7-7e49-41c0-834e-affe13851d4d", "Total Goods Weight", "Total Goods Weight", "Total Goods Weight", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_WeightCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 70, true);
			this.JJ_WeightCalcDropEdit.Name = "JJ_WeightCalcDropEdit";
			this.JJ_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.JJ_WeightCalcDropEdit.TabIndex = 1;
			this.JJ_WeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JJ_VolumeCalcDropEdit
			// 
			this.JJ_VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_VolumeCalcDropEdit, ".");
			this.JJ_VolumeCalcDropEdit.BindToAmount = "JJ_Volume";
			this.JJ_VolumeCalcDropEdit.BindToUnit = "JJ_VolumeUQ";
			this.JJ_VolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|aff029c3-ca0c-4581-8051-18e54f80e32b", "Total Goods Volume", "Total Goods Volume", "Total Goods Volume", "");
			this.JJ_VolumeCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_VolumeCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 110, true);
			this.JJ_VolumeCalcDropEdit.Name = "JJ_VolumeCalcDropEdit";
			this.JJ_VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.JJ_VolumeCalcDropEdit.TabIndex = 2;
			this.JJ_VolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// AddressesLinkedToJobLinkLabel
			// 
			this.AddressesLinkedToJobLinkLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|c3b1044b-5342-4ac6-a4de-0fe01293fdcd", "Addresses linked to Job XXXXXX - update details on main job");
			this.AddressesLinkedToJobLinkLabel.IsFontBold = false;
			this.AddressesLinkedToJobLinkLabel.LinkColor = System.Drawing.Color.Red;
			this.AddressesLinkedToJobLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 1, true);
			this.AddressesLinkedToJobLinkLabel.Name = "AddressesLinkedToJobLinkLabel";
			this.AddressesLinkedToJobLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 15, true);
			this.AddressesLinkedToJobLinkLabel.TabIndex = 0;
			this.AddressesLinkedToJobLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddressesLinkedToJobLinkLabel_LinkClicked);
			// 
			// AddressesPanel
			// 
			this.AddressesPanel.Controls.Add(this.ClickSaveToCreateJobLabel);
			this.AddressesPanel.Controls.Add(this.LocalClientOrgControl);
			this.AddressesPanel.Controls.Add(this.SailingDetailsGroupBox);
			this.AddressesPanel.Controls.Add(this.FourthDocAddressControl);
			this.AddressesPanel.Controls.Add(this.FirstDocAddressControl);
			this.AddressesPanel.Controls.Add(this.ClientDetailsGroupBox);
			this.AddressesPanel.Controls.Add(this.CartageDetailsGroupBox);
			this.AddressesPanel.Controls.Add(this.ThirdDocAddressControl);
			this.AddressesPanel.Controls.Add(this.SecondDocAddressControl);
			this.AddressesPanel.Controls.Add(this.JobHeaderMutexErrorLabel);
			this.AddressesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AddressesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.AddressesPanel.Name = "AddressesPanel";
			this.AddressesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 364, true);
			this.AddressesPanel.TabIndex = 1;
			// 
			// LocalClientOrgControl
			// 
			this.LocalClientOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalClientOrgControl, "LocalClientAddressPK_ZAddress");
			this.LocalClientOrgControl.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|13c83606-2d8f-4718-a4cb-26c2f132ec0b", "Local Client");
			this.LocalClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 186, true);
			this.LocalClientOrgControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.Name = "LocalClientOrgControl";
			this.LocalClientOrgControl.OnlyStopOnDebtor = false;
			this.LocalClientOrgControl.PopupCaption = "";
			this.LocalClientOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.TabIndex = 4;
			// 
			// SailingDetailsGroupBox
			// 
			this.SailingDetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JA_NKPortOfLoadingFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.DirectionDropEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JB_NKPortOfDischargeFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JV_NKVesselFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JB_E_ARVDateEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JA_E_DEPDateEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.SelectSchedulesButton);
			this.SailingDetailsGroupBox.Controls.Add(this.JX_VoyageTextBox);
			this.SailingDetailsGroupBox.Controls.Add(this.ImportDatesPanel);
			this.SailingDetailsGroupBox.Controls.Add(this.ExportDatesPanel);
			this.SailingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 186, true);
			this.SailingDetailsGroupBox.Name = "SailingDetailsGroupBox";
			this.SailingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 174, true);
			this.SailingDetailsGroupBox.TabIndex = 6;
			this.SailingDetailsGroupBox.TabStop = false;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "JJ_ShippingTransportMode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 16, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.TransportModeDropEdit.TabIndex = 0;
			// 
			// JJ_JA_NKPortOfLoadingFindBox
			// 
			this.JJ_JA_NKPortOfLoadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JA_NKPortOfLoadingFindBox, "PortOfLoading");
			this.JJ_JA_NKPortOfLoadingFindBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|1015200e-1d93-4d2f-b20a-b368e7a9984d", "Load", "Load Port", "");
			this.JJ_JA_NKPortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 82, true);
			this.JJ_JA_NKPortOfLoadingFindBox.Name = "JJ_JA_NKPortOfLoadingFindBox";
			this.JJ_JA_NKPortOfLoadingFindBox.PopupCaption = null;
			this.JJ_JA_NKPortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.JJ_JA_NKPortOfLoadingFindBox.ShowDescriptionBox = false;
			this.JJ_JA_NKPortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JJ_JA_NKPortOfLoadingFindBox.TabIndex = 5;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "JJ_Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 16, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.PreBoundMaxLength = 3;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.DirectionDropEdit.TabIndex = 1;
			// 
			// JJ_JB_NKPortOfDischargeFindBox
			// 
			this.JJ_JB_NKPortOfDischargeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JB_NKPortOfDischargeFindBox, "PortOfDischarge");
			this.JJ_JB_NKPortOfDischargeFindBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|19f8b4b9-efd2-4390-8ecd-f22fcaf5b2dc", "Disch.", "Discharge", "Discharge Port", "");
			this.JJ_JB_NKPortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 104, true);
			this.JJ_JB_NKPortOfDischargeFindBox.Name = "JJ_JB_NKPortOfDischargeFindBox";
			this.JJ_JB_NKPortOfDischargeFindBox.PopupCaption = null;
			this.JJ_JB_NKPortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.JJ_JB_NKPortOfDischargeFindBox.ShowDescriptionBox = false;
			this.JJ_JB_NKPortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JJ_JB_NKPortOfDischargeFindBox.TabIndex = 6;
			// 
			// JJ_JV_NKVesselFindBox
			// 
			this.JJ_JV_NKVesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JV_NKVesselFindBox, "Vessel");
			this.JJ_JV_NKVesselFindBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|081efa38-9e97-4e4b-87ca-cdd992c3efdf", "Vessel", "Vessel", "Vessel", "");
			this.JJ_JV_NKVesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 38, true);
			this.JJ_JV_NKVesselFindBox.Name = "JJ_JV_NKVesselFindBox";
			this.JJ_JV_NKVesselFindBox.PopupCaption = null;
			this.JJ_JV_NKVesselFindBox.PreBoundMaxLength = 35;
			this.JJ_JV_NKVesselFindBox.ShowDescriptionBox = false;
			this.JJ_JV_NKVesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 21, true);
			this.JJ_JV_NKVesselFindBox.TabIndex = 2;
			// 
			// JJ_JB_E_ARVDateEdit
			// 
			this.JJ_JB_E_ARVDateEdit.AllowDrop = true;
			this.JJ_JB_E_ARVDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JB_E_ARVDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JB_E_ARVDateEdit, "E_ARV");
			this.JJ_JB_E_ARVDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|1edf393d-8f34-4d68-b6b7-b51e99f1ab34", "ETA", "ETA", "ETA", "");
			this.JJ_JB_E_ARVDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 104, true);
			this.JJ_JB_E_ARVDateEdit.Name = "JJ_JB_E_ARVDateEdit";
			this.JJ_JB_E_ARVDateEdit.TabIndex = 8;
			// 
			// JJ_JA_E_DEPDateEdit
			// 
			this.JJ_JA_E_DEPDateEdit.AllowDrop = true;
			this.JJ_JA_E_DEPDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JA_E_DEPDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JA_E_DEPDateEdit, "E_DEP");
			this.JJ_JA_E_DEPDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|4d980043-f1ca-4d91-8363-54ab5a2aba20", "ETD", "ETD", "ETD", "");
			this.JJ_JA_E_DEPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 82, true);
			this.JJ_JA_E_DEPDateEdit.Name = "JJ_JA_E_DEPDateEdit";
			this.JJ_JA_E_DEPDateEdit.TabIndex = 7;
			// 
			// SelectSchedulesButton
			// 
			this.SelectSchedulesButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|00a0e357-db88-4063-a048-6cbec5a2e98e", "Select Schedule");
			this.SelectSchedulesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 59, true);
			this.SelectSchedulesButton.Name = "SelectSchedulesButton";
			this.SelectSchedulesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 22, true);
			this.SelectSchedulesButton.TabIndex = 4;
			this.SelectSchedulesButton.Click += new System.EventHandler(this.SelectSchedulesButton_Click);
			// 
			// JX_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JX_VoyageTextBox, "VoyageFlight");
			this.JX_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 60, true);
			this.JX_VoyageTextBox.Name = "JX_VoyageTextBox";
			this.JX_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JX_VoyageTextBox.TabIndex = 3;
			// 
			// ImportDatesPanel
			// 
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_LCLAvailabilityDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_LCLStorageDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_AvailabilityDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_StorageDateDateEdit);
			this.ImportDatesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 127, true);
			this.ImportDatesPanel.Name = "ImportDatesPanel";
			this.ImportDatesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 43, true);
			this.ImportDatesPanel.TabIndex = 15;
			// 
			// JJ_JX_LCLAvailabilityDateDateEdit
			// 
			this.JJ_JX_LCLAvailabilityDateDateEdit.AllowDrop = true;
			this.JJ_JX_LCLAvailabilityDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLAvailabilityDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLAvailabilityDateDateEdit, "LCLAvailabilityDate");
			this.JJ_JX_LCLAvailabilityDateDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|3d23aa61-bd7d-4392-a8d0-f2964fa4cb9a", "CFS Avail");
			this.JJ_JX_LCLAvailabilityDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLAvailabilityDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 22, true);
			this.JJ_JX_LCLAvailabilityDateDateEdit.Name = "JJ_JX_LCLAvailabilityDateDateEdit";
			this.JJ_JX_LCLAvailabilityDateDateEdit.TabIndex = 11;
			// 
			// JJ_JX_LCLStorageDateDateEdit
			// 
			this.JJ_JX_LCLStorageDateDateEdit.AllowDrop = true;
			this.JJ_JX_LCLStorageDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLStorageDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLStorageDateDateEdit, "LCLStorageDate");
			this.JJ_JX_LCLStorageDateDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|2465bea5-1507-4143-943e-28fe99fa0a4d", "CFS Stor.", "CFS Storage Start");
			this.JJ_JX_LCLStorageDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLStorageDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 22, true);
			this.JJ_JX_LCLStorageDateDateEdit.Name = "JJ_JX_LCLStorageDateDateEdit";
			this.JJ_JX_LCLStorageDateDateEdit.TabIndex = 12;
			// 
			// JJ_JX_AvailabilityDateDateEdit
			// 
			this.JJ_JX_AvailabilityDateDateEdit.AllowDrop = true;
			this.JJ_JX_AvailabilityDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_AvailabilityDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_AvailabilityDateDateEdit, "FCLAvailabilityDate");
			this.JJ_JX_AvailabilityDateDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|dcd2e14c-0c6e-42df-ad7c-6a91c4038c99", "CTO Avail");
			this.JJ_JX_AvailabilityDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_AvailabilityDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.JJ_JX_AvailabilityDateDateEdit.Name = "JJ_JX_AvailabilityDateDateEdit";
			this.JJ_JX_AvailabilityDateDateEdit.TabIndex = 9;
			// 
			// JJ_JX_StorageDateDateEdit
			// 
			this.JJ_JX_StorageDateDateEdit.AllowDrop = true;
			this.JJ_JX_StorageDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_StorageDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_StorageDateDateEdit, "FCLStorageDate");
			this.JJ_JX_StorageDateDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|5e1695ce-20da-4c2d-b60f-5d93a3787cd9", "CTO Stor.", "CTO Storage Start");
			this.JJ_JX_StorageDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_StorageDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 0, true);
			this.JJ_JX_StorageDateDateEdit.Name = "JJ_JX_StorageDateDateEdit";
			this.JJ_JX_StorageDateDateEdit.TabIndex = 10;
			// 
			// ExportDatesPanel
			// 
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_LCLReceivalCommencesDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_LCLCutOffDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_FCLReceivalCommencesDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_FCLCutOffDateEdit);
			this.ExportDatesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 127, true);
			this.ExportDatesPanel.Name = "ExportDatesPanel";
			this.ExportDatesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 43, true);
			this.ExportDatesPanel.TabIndex = 13;
			// 
			// JJ_JX_LCLReceivalCommencesDateEdit
			// 
			this.JJ_JX_LCLReceivalCommencesDateEdit.AllowDrop = true;
			this.JJ_JX_LCLReceivalCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLReceivalCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLReceivalCommencesDateEdit, "LCLReceivalCommences");
			this.JJ_JX_LCLReceivalCommencesDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|48a25ff4-a15d-4a48-98f8-f8bf905a998b", "CFS Rec.", "CFS Rec.", "CFS Receival Start", "CFS Receival Start Date.");
			this.JJ_JX_LCLReceivalCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLReceivalCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 22, true);
			this.JJ_JX_LCLReceivalCommencesDateEdit.Name = "JJ_JX_LCLReceivalCommencesDateEdit";
			this.JJ_JX_LCLReceivalCommencesDateEdit.TabIndex = 2;
			// 
			// JJ_JX_LCLCutOffDateEdit
			// 
			this.JJ_JX_LCLCutOffDateEdit.AllowDrop = true;
			this.JJ_JX_LCLCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLCutOffDateEdit, "LCLCutOff");
			this.JJ_JX_LCLCutOffDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|457aa925-388a-47cf-bd6d-ab04849f3ee7", "CFS C. Off", "CFS C. Off", "CFS Cut Off", "CFS Cut Off Date");
			this.JJ_JX_LCLCutOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 22, true);
			this.JJ_JX_LCLCutOffDateEdit.Name = "JJ_JX_LCLCutOffDateEdit";
			this.JJ_JX_LCLCutOffDateEdit.TabIndex = 3;
			// 
			// JJ_JX_FCLReceivalCommencesDateEdit
			// 
			this.JJ_JX_FCLReceivalCommencesDateEdit.AllowDrop = true;
			this.JJ_JX_FCLReceivalCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_FCLReceivalCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_FCLReceivalCommencesDateEdit, "FCLReceivalCommences");
			this.JJ_JX_FCLReceivalCommencesDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|1aa0cfd4-1e91-4163-be7b-0d932cbe14e4", "CTO Rec.", "CTO Receival", "CTO Receival Start", "CTO Receival Start Date");
			this.JJ_JX_FCLReceivalCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_FCLReceivalCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.JJ_JX_FCLReceivalCommencesDateEdit.Name = "JJ_JX_FCLReceivalCommencesDateEdit";
			this.JJ_JX_FCLReceivalCommencesDateEdit.TabIndex = 0;
			// 
			// JJ_JX_FCLCutOffDateEdit
			// 
			this.JJ_JX_FCLCutOffDateEdit.AllowDrop = true;
			this.JJ_JX_FCLCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_FCLCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_FCLCutOffDateEdit, "FCLCutOff");
			this.JJ_JX_FCLCutOffDateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageUserControl|8179bc03-f028-4e9c-82e5-fb9db610ea74", "CTO C. Off", "CTO C. Off", "CTO Cut Off", "CTO Cut Off Date.");
			this.JJ_JX_FCLCutOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_FCLCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 0, true);
			this.JJ_JX_FCLCutOffDateEdit.Name = "JJ_JX_FCLCutOffDateEdit";
			this.JJ_JX_FCLCutOffDateEdit.TabIndex = 1;
			// 
			// FourthDocAddressControl
			// 
			this.FourthDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FourthDocAddressControl, "FourthDocAddresses");
			this.FourthDocAddressControl.BindToOrganisations = "Lookups+FourthAddressList";
			this.FourthDocAddressControl.CaptionResourceString = null;
			this.FourthDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 3, true);
			this.FourthDocAddressControl.Name = "FourthDocAddressControl";
			this.FourthDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.FourthDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.FourthDocAddressControl.TabIndex = 3;
			// 
			// JobHeaderMutexErrorLabel
			// 
			this.JobHeaderMutexErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobHeaderMutexErrorLabel, false);
			this.JobHeaderMutexErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 186, true);
			this.JobHeaderMutexErrorLabel.Name = "JobHeaderMutexErrorLabel";
			this.JobHeaderMutexErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.JobHeaderMutexErrorLabel.TabIndex = 8;
			this.JobHeaderMutexErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MainJobPanel
			// 
			this.MainJobPanel.Controls.Add(this.AddressesLinkedToJobLinkLabel);
			this.MainJobPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainJobPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.MainJobPanel.Name = "MainJobPanel";
			this.MainJobPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 17, true);
			this.MainJobPanel.TabIndex = 0;
			// 
			// TransportLegSummaryPanel
			// 
			this.TransportLegSummaryPanel.Controls.Add(this.ContainerLegSummaryGroupBox);
			this.TransportLegSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportLegSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 429, true);
			this.TransportLegSummaryPanel.Name = "TransportLegSummaryPanel";
			this.TransportLegSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 134, true);
			this.TransportLegSummaryPanel.TabIndex = 46;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.ContainerModeDropEdit);
			this.zPanel1.Controls.Add(this.zCodeFindBox1);
			this.zPanel1.Controls.Add(this.BranchGuidFindBox);
			this.zPanel1.Controls.Add(this.JJ_EstimatedDeliveryDateEdit);
			this.zPanel1.Controls.Add(this.JJ_ContainerDropModeDropDownEdit);
			this.zPanel1.Controls.Add(this.JJ_EstimatedPickupDateEdit);
			this.zPanel1.Controls.Add(this.JJ_CartageTypeDropEdit);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 48, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ContainerModeDropEdit
			// 
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "JJ_ContainerMode");
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 3, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.ContainerModeDropEdit.TabIndex = 1;
			// 
			// ClickSaveToCreateJobLabel
			// 
			this.ClickSaveToCreateJobLabel.ForeColor = System.Drawing.Color.Red;
			this.ClickSaveToCreateJobLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 338, true);
			this.ClickSaveToCreateJobLabel.Name = "ClickSaveToCreateJobLabel";
			this.ClickSaveToCreateJobLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 23, true);
			this.ClickSaveToCreateJobLabel.TabIndex = 11;
			this.ClickSaveToCreateJobLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CartageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportLegSummaryPanel);
			this.Controls.Add(this.AddressesPanel);
			this.Controls.Add(this.MainJobPanel);
			this.Controls.Add(this.zPanel1);
			this.Name = "CartageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerLegSummaryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).EndInit();
			this.ClientDetailsGroupBox.ResumeLayout(false);
			this.ClientDetailsGroupBox.PerformLayout();
			this.CartageDetailsGroupBox.ResumeLayout(false);
			this.AddressesPanel.ResumeLayout(false);
			this.SailingDetailsGroupBox.ResumeLayout(false);
			this.SailingDetailsGroupBox.PerformLayout();
			this.ImportDatesPanel.ResumeLayout(false);
			this.ExportDatesPanel.ResumeLayout(false);
			this.MainJobPanel.ResumeLayout(false);
			this.TransportLegSummaryPanel.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
