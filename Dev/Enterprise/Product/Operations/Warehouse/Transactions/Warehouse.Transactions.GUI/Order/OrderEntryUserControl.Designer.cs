using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderEntryUserControl
	{
		IContainer components = new Container();
		ZTemplateTabControl DetailTabControl;
		ZTabPage DetailTabPage;
		ZTextBox StatusTextBox;
		ZCalcEdit TotalUnitsCalcEdit;
		ZDropEdit SubTypeDropEdit;
		ZLabel AwaitingResponseLabel;
		ZTextBox WD_ExternalReferenceTextBox;
		ZDateEdit RequiredDateEdit;
		ZTextBox PickNoTextBox;
		ZTabPage ReferenceTabPage;
		ZTabPage ContainersTabPage;
		ZTabPage LinesTabPage;
		ZTextBox CustomerRefTextBox;
		ZDropEdit PickOptionDropEdit;
		ZTabPage ForwardingTabPage;
		ZGroupBox DetailsBottomGroupBox;
		ZTextBox VehicleNoTextBox;
		ZDropEdit DropModeDropEdit;
		ZTextBox ReferenceNoTextBox;
		ZPanel DetailTopPanel;
		ZGuidFindBox zGuidFindBox1;
		ZOrganisationControl OrgWhsFindControl;
		ZTemplateTabControl CompaniesTabControl;
		ZTabPage ConsigneeTabPage;
		ZDocAddressControl ConsigneeDocAddressControl;
		ZTabPage GoodsBilledToTabPage;
		ZDocAddressControl GoodsBillToDocAddressControl;
		ZGroupBox JobGroupBox;
		ZGroupBox TransportGroupBox;
		ZGroupBox DatesAndTotalsGroupBox;
		ZCalcEdit zCalcEdit1;
		ZTemplateTabControl zTemplateTabControl1;
		ZTabPage zTabPage1;
		ZDocAddressControl TransportDocAddressControl;
		ZTabPage zTabPage2;
		ZDocAddressControl TransportBilledToDocAddressControl;
		ZTabPage StagingTabPage;
		ZTabPage CustomFieldsTabPage;
		ZTabPage ServicesTab;
		ZPanel ServicesPanel;
		ServicesControl ServicesControl;
		ZPanel ContainersPanel;
		ZCalcEdit OrderNoSplitCalcEdit;
		internal OrderDocketLinesGridUserControl orderDocketLinesGridUserControl1;
		ZButtonTransportCoHotlink TransportRefLinkButton;
		internal OrderDocketLinesGridUserControl DocketLinesGridUserControl;
		OrderEntryStagingUserControl orderEntryStagingUserControl21;
		ForwardingControl OrderForwardingControl;
		DocketReferenceGridUserControl DocketReferenceGridUserControl;
		OrderContainersUserControl ContainersUserControl;
		CargoWise.Windows.UI.KSplitter splitter1;
		CargoWise.Windows.UI.KPanel bottomPanel;
		ZPanel OrderLinesGridAndAdditionalDataPanel;
		ZPanel OrderLinesGridPanel;
		ZPanel OrderLinesTabAdditionalDataPanel;
		ZLinkLabel TransportJobLinkLabel;
		ZLabel TransportJobLabel;
		ZCalcEdit WD_TotalLineUnitsOnLinesTabCalcEdit;
		ZCalcDropEdit WD_TotalWeightCalcDropEdit;
		ZCalcDropEdit WD_TotalCubicCalcDropEdit;
		ZCalcEdit WD_TotalUnitsFromLinesCalcEdit;
		ZDropEdit FulfillmentRuleDropEdit;
		ZDropEdit CarrierServiceLevelDropEdit;
		ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl;
		ZTabPage FreightForwarderTab;
		ZPanel FreightForwarderPanel;
		ZOrganisationControl FreightForwarderOrganisationControl;
		ZTextBox GoodsDescriptionTextBox;
		RelatedJobsTabPage RelatedJobsTabPage;
		ZCodeFindBox ServiceLevelCodeBox;
		private ZTabPage DistributionCentreTabPage;
		private ZDocAddressControl DistributionCentreDocAddressControl;
		private ZCalcEdit PropertyCalcEdit;
		private ZGroupBox OrderConfigFlagsGroupBox;
		private ZCheckBox QualityAuditRequiredCheckbox;
		private ZCheckBox HoldOrderCheckBox;
		private ZCheckBox PackingRequiredCheckBox;
		private ZCheckBox AuthorisedToLeaveCheckBox;
		private ZCheckBox ExcludeFromTotePickingCheckBox;
		private ZGuidFindBox WD_TZ_TransportZone;
		ZButton ScreenButton;
		DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
		private ZGuidFindBox SalesChannelGuidFindBox;
		ZCheckBox IsLoadingRequiredCheckBox;
		private ZCheckBox UsePackingConsolidationCheckBox;
		ZCalcFindBox WD_TotalOrderValue;

		private void InitializeComponent()
		{
			this.components = new Container();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(OrderEntryUserControl));
			this.WD_TotalOrderValue = new ZCalcFindBox();
			this.DetailTabControl = new ZTemplateTabControl();
			this.DetailTabPage = new ZTabPage();
			this.orderDocketLinesGridUserControl1 = new OrderDocketLinesGridUserControl();
			this.DetailsBottomGroupBox = new ZGroupBox();
			this.OrderConfigFlagsGroupBox = new ZGroupBox();
			this.UsePackingConsolidationCheckBox = new ZCheckBox();
			this.IsLoadingRequiredCheckBox = new ZCheckBox();
			this.AuthorisedToLeaveCheckBox = new ZCheckBox();
			this.QualityAuditRequiredCheckbox = new ZCheckBox();
			this.HoldOrderCheckBox = new ZCheckBox();
			this.PackingRequiredCheckBox = new ZCheckBox();
			this.ExcludeFromTotePickingCheckBox = new ZCheckBox();
			this.DatesAndTotalsGroupBox = new ZGroupBox();
			this.WD_TZ_TransportZone = new ZGuidFindBox();
			this.ScreenButton = new ZButton();
			this.ScreeningStatusDropEdit = new DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.WD_TotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.WD_TotalCubicCalcDropEdit = new ZCalcDropEdit();
			this.WD_TotalUnitsFromLinesCalcEdit = new ZCalcEdit();
			this.TotalUnitsCalcEdit = new ZCalcEdit();
			this.RequiredDateEdit = new ZDateEdit();
			this.TransportGroupBox = new ZGroupBox();
			this.SalesChannelGuidFindBox = new ZGuidFindBox();
			this.GoodsDescriptionTextBox = new ZTextBox();
			this.CarrierServiceLevelDropEdit = new ZDropEdit();
			this.TransportJobLabel = new ZLabel();
			this.TransportJobLinkLabel = new ZLinkLabel();
			this.TransportRefLinkButton = new ZButtonTransportCoHotlink();
			this.zCalcEdit1 = new ZCalcEdit();
			this.VehicleNoTextBox = new ZTextBox();
			this.ReferenceNoTextBox = new ZTextBox();
			this.DropModeDropEdit = new ZDropEdit();
			this.JobGroupBox = new ZGroupBox();
			this.PropertyCalcEdit = new ZCalcEdit();
			this.ServiceLevelCodeBox = new ZCodeFindBox();
			this.FulfillmentRuleDropEdit = new ZDropEdit();
			this.OrderNoSplitCalcEdit = new ZCalcEdit();
			this.StatusTextBox = new ZTextBox();
			this.CustomerRefTextBox = new ZTextBox();
			this.PickNoTextBox = new ZTextBox();
			this.PickOptionDropEdit = new ZDropEdit();
			this.WD_ExternalReferenceTextBox = new ZTextBox();
			this.DetailTopPanel = new ZPanel();
			this.zTemplateTabControl1 = new ZTemplateTabControl();
			this.zTabPage1 = new ZTabPage();
			this.TransportDocAddressControl = new ZDocAddressControl();
			this.zTabPage2 = new ZTabPage();
			this.TransportBilledToDocAddressControl = new ZDocAddressControl();
			this.FreightForwarderTab = new ZTabPage();
			this.FreightForwarderPanel = new ZPanel();
			this.FreightForwarderOrganisationControl = new ZOrganisationControl();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.OrgWhsFindControl = new ZOrganisationControl();
			this.CompaniesTabControl = new ZTemplateTabControl();
			this.ConsigneeTabPage = new ZTabPage();
			this.ConsigneeDocAddressControl = new ZDocAddressControl();
			this.GoodsBilledToTabPage = new ZTabPage();
			this.GoodsBillToDocAddressControl = new ZDocAddressControl();
			this.DistributionCentreTabPage = new ZTabPage();
			this.DistributionCentreDocAddressControl = new ZDocAddressControl();
			this.SubTypeDropEdit = new ZDropEdit();
			this.AwaitingResponseLabel = new ZLabel();
			this.LinesTabPage = new ZTabPage();
			this.OrderLinesGridAndAdditionalDataPanel = new ZPanel();
			this.OrderLinesGridPanel = new ZPanel();
			this.DocketLinesGridUserControl = new OrderDocketLinesGridUserControl();
			this.OrderLinesTabAdditionalDataPanel = new ZPanel();
			this.WD_TotalLineUnitsOnLinesTabCalcEdit = new ZCalcEdit();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.bottomPanel = new CargoWise.Windows.UI.KPanel();
			this.StagingTabPage = new ZTabPage();
			this.orderEntryStagingUserControl21 = new OrderEntryStagingUserControl();
			this.CustomFieldsTabPage = new ZTabPage();
			this.processTemplateCustomFieldsControl = new ProcessTemplateCustomFieldsControl();
			this.ForwardingTabPage = new ZTabPage();
			this.OrderForwardingControl = new ForwardingControl();
			this.ReferenceTabPage = new ZTabPage();
			this.DocketReferenceGridUserControl = new DocketReferenceGridUserControl();
			this.ContainersTabPage = new ZTabPage();
			this.ContainersPanel = new ZPanel();
			this.ContainersUserControl = new OrderContainersUserControl();
			this.ServicesTab = new ZTabPage();
			this.ServicesPanel = new ZPanel();
			this.ServicesControl = new ServicesControl();
			this.RelatedJobsTabPage = new RelatedJobsTabPage();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WD_TotalOrderValue.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DetailTabPage.SuspendLayout();
			this.orderDocketLinesGridUserControl1.SuspendLayout();
			this.DetailsBottomGroupBox.SuspendLayout();
			this.OrderConfigFlagsGroupBox.SuspendLayout();
			this.DatesAndTotalsGroupBox.SuspendLayout();
			this.WD_TZ_TransportZone.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.WD_TotalWeightCalcDropEdit.SuspendLayout();
			this.WD_TotalCubicCalcDropEdit.SuspendLayout();
			this.RequiredDateEdit.SuspendLayout();
			this.TransportGroupBox.SuspendLayout();
			this.SalesChannelGuidFindBox.SuspendLayout();
			this.CarrierServiceLevelDropEdit.SuspendLayout();
			this.DropModeDropEdit.SuspendLayout();
			this.JobGroupBox.SuspendLayout();
			this.ServiceLevelCodeBox.SuspendLayout();
			this.FulfillmentRuleDropEdit.SuspendLayout();
			this.PickOptionDropEdit.SuspendLayout();
			this.DetailTopPanel.SuspendLayout();
			this.zTemplateTabControl1.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.TransportDocAddressControl.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			this.TransportBilledToDocAddressControl.SuspendLayout();
			this.FreightForwarderTab.SuspendLayout();
			this.FreightForwarderPanel.SuspendLayout();
			this.FreightForwarderOrganisationControl.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.OrgWhsFindControl.SuspendLayout();
			this.CompaniesTabControl.SuspendLayout();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.GoodsBilledToTabPage.SuspendLayout();
			this.GoodsBillToDocAddressControl.SuspendLayout();
			this.DistributionCentreTabPage.SuspendLayout();
			this.DistributionCentreDocAddressControl.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.OrderLinesGridAndAdditionalDataPanel.SuspendLayout();
			this.OrderLinesGridPanel.SuspendLayout();
			this.DocketLinesGridUserControl.SuspendLayout();
			this.OrderLinesTabAdditionalDataPanel.SuspendLayout();
			this.StagingTabPage.SuspendLayout();
			this.orderEntryStagingUserControl21.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.processTemplateCustomFieldsControl.SuspendLayout();
			this.ForwardingTabPage.SuspendLayout();
			this.OrderForwardingControl.SuspendLayout();
			this.ReferenceTabPage.SuspendLayout();
			this.DocketReferenceGridUserControl.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersPanel.SuspendLayout();
			this.ContainersUserControl.SuspendLayout();
			this.ServicesTab.SuspendLayout();
			this.ServicesPanel.SuspendLayout();
			this.ServicesControl.SuspendLayout();
			this.RelatedJobsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsOrder);
			// 
			// WD_TotalOrderValue
			// 
			this.WD_TotalOrderValue.AllowDrop = true;
			this.WD_TotalOrderValue.BindToAmount = "WD_TotalOrderValue";
			this.WD_TotalOrderValue.BindToUnit = "WD_RX_NKTotalOrderCurrency";
			this.WD_TotalOrderValue.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.WD_TotalOrderValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 138, true);
			this.WD_TotalOrderValue.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WD_TotalOrderValue.Name = "WD_TotalOrderValue";
			this.WD_TotalOrderValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.WD_TotalOrderValue.TabIndex = 5;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailTabPage);
			this.DetailTabControl.Controls.Add(this.LinesTabPage);
			this.DetailTabControl.Controls.Add(this.StagingTabPage);
			this.DetailTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.DetailTabControl.Controls.Add(this.ForwardingTabPage);
			this.DetailTabControl.Controls.Add(this.ReferenceTabPage);
			this.DetailTabControl.Controls.Add(this.ContainersTabPage);
			this.DetailTabControl.Controls.Add(this.ServicesTab);
			this.DetailTabControl.Controls.Add(this.RelatedJobsTabPage);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 758, true);
			this.DetailTabControl.TabIndex = 0;
			// 
			// DetailTabPage
			// 
			this.DetailTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|b7974d40-39c0-4d61-b12d-ff9ff7a43d2e", "Order");
			this.DetailTabPage.Controls.Add(this.orderDocketLinesGridUserControl1);
			this.DetailTabPage.Controls.Add(this.DetailsBottomGroupBox);
			this.DetailTabPage.Controls.Add(this.DetailTopPanel);
			this.DetailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.DetailTabPage.Name = "DetailTabPage";
			this.DetailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.DetailTabPage.TabIndex = 0;
			// 
			// orderDocketLinesGridUserControl1
			// 
			this.orderDocketLinesGridUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orderDocketLinesGridUserControl1, ".");
			this.orderDocketLinesGridUserControl1.BindTo = "ParentLines";
			this.orderDocketLinesGridUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orderDocketLinesGridUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 496, true);
			this.orderDocketLinesGridUserControl1.Name = "orderDocketLinesGridUserControl1";
			this.orderDocketLinesGridUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.orderDocketLinesGridUserControl1.ReduceStockOptionsAreVisible = false;
			this.orderDocketLinesGridUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 218, true);
			this.orderDocketLinesGridUserControl1.TabIndex = 2;
			// 
			// DetailsBottomGroupBox
			// 
			this.DetailsBottomGroupBox.Controls.Add(this.OrderConfigFlagsGroupBox);
			this.DetailsBottomGroupBox.Controls.Add(this.DatesAndTotalsGroupBox);
			this.DetailsBottomGroupBox.Controls.Add(this.TransportGroupBox);
			this.DetailsBottomGroupBox.Controls.Add(this.JobGroupBox);
			this.DetailsBottomGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsBottomGroupBox, false);
			this.DetailsBottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.DetailsBottomGroupBox.Name = "DetailsBottomGroupBox";
			this.DetailsBottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 268, true);
			this.DetailsBottomGroupBox.TabIndex = 1;
			this.DetailsBottomGroupBox.TabStop = false;
			// 
			// OrderConfigFlagsGroupBox
			// 
			this.OrderConfigFlagsGroupBox.Controls.Add(this.UsePackingConsolidationCheckBox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.IsLoadingRequiredCheckBox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.AuthorisedToLeaveCheckBox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.QualityAuditRequiredCheckbox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.HoldOrderCheckBox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.PackingRequiredCheckBox);
			this.OrderConfigFlagsGroupBox.Controls.Add(this.ExcludeFromTotePickingCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrderConfigFlagsGroupBox, false);
			this.OrderConfigFlagsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 228, true);
			this.OrderConfigFlagsGroupBox.Name = "OrderConfigFlagsGroupBox";
			this.OrderConfigFlagsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 33, true);
			this.OrderConfigFlagsGroupBox.TabIndex = 3;
			this.OrderConfigFlagsGroupBox.TabStop = false;
			// 
			// UsePackingConsolidationCheckBox
			// 
			this.UsePackingConsolidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UsePackingConsolidationCheckBox, "WD_UseDirectedPackingConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_UseDirectedPackingConsolidation)));
			this.UsePackingConsolidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UsePackingConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 12, true);
			this.UsePackingConsolidationCheckBox.Name = "UsePackingConsolidationCheckBox";
			this.UsePackingConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UsePackingConsolidationCheckBox.TabIndex = 7;
			this.UsePackingConsolidationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsLoadingRequiredCheckBox
			// 
			this.IsLoadingRequiredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsLoadingRequiredCheckBox, "WD_IsLoadingRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_IsLoadingRequired)));
			this.IsLoadingRequiredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsLoadingRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(764, 12, true);
			this.IsLoadingRequiredCheckBox.Name = "IsLoadingRequiredCheckBox";
			this.IsLoadingRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsLoadingRequiredCheckBox.TabIndex = 6;
			this.IsLoadingRequiredCheckBox.UseVisualStyleBackColor = true;
			// 
			// AuthorisedToLeaveCheckBox
			// 
			this.AuthorisedToLeaveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AuthorisedToLeaveCheckBox, "WD_IsAuthorisedToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_IsAuthorisedToLeave)));
			this.AuthorisedToLeaveCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|25B8CD03-D650-48EB-879B-0E72BA3AC5D7", "Authorized to Leave:");
			this.AuthorisedToLeaveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AuthorisedToLeaveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 12, true);
			this.AuthorisedToLeaveCheckBox.Name = "AuthorisedToLeaveCheckBox";
			this.AuthorisedToLeaveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AuthorisedToLeaveCheckBox.TabIndex = 5;
			this.AuthorisedToLeaveCheckBox.UseVisualStyleBackColor = true;
			// 
			// QualityAuditRequiredCheckbox
			// 
			this.QualityAuditRequiredCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QualityAuditRequiredCheckbox, "WD_QualityAuditRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_QualityAuditRequired)));
			this.QualityAuditRequiredCheckbox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("37C300CB-5E28-49B2-89AB-239C5F995E10", "Quality Audit Required:");
			this.QualityAuditRequiredCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.QualityAuditRequiredCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 12, true);
			this.QualityAuditRequiredCheckbox.Name = "QualityAuditRequiredCheckbox";
			this.QualityAuditRequiredCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.QualityAuditRequiredCheckbox.TabIndex = 2;
			this.QualityAuditRequiredCheckbox.UseVisualStyleBackColor = true;
			// 
			// HoldOrderCheckBox
			// 
			this.HoldOrderCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HoldOrderCheckBox, "IsOrderHeld");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).IsOrderHeld)));
			this.HoldOrderCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|b2c45583-073a-4460-b1cf-13587c076f0a", "Hold Order:");
			this.HoldOrderCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HoldOrderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 12, true);
			this.HoldOrderCheckBox.Name = "HoldOrderCheckBox";
			this.HoldOrderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.HoldOrderCheckBox.TabIndex = 1;
			this.HoldOrderCheckBox.UseVisualStyleBackColor = true;
			// 
			// PackingRequiredCheckBox
			// 
			this.PackingRequiredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PackingRequiredCheckBox, "WD_PackingAfterPickingRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_PackingAfterPickingRequired)));
			this.PackingRequiredCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|3985d7fa-fcd4-46d7-963e-5afcf94b2f38", "Packing Required:");
			this.PackingRequiredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PackingRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 12, true);
			this.PackingRequiredCheckBox.Name = "PackingRequiredCheckBox";
			this.PackingRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PackingRequiredCheckBox.TabIndex = 3;
			this.PackingRequiredCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExcludeFromTotePickingCheckBox
			// 
			this.ExcludeFromTotePickingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcludeFromTotePickingCheckBox, "WD_ExcludeFromTotePicking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsOrder)(null)).WD_ExcludeFromTotePicking)));
			this.ExcludeFromTotePickingCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|69aa5d2a-59cf-415d-a797-367ee6bdcb00", "Exclude from Tote Picking:");
			this.ExcludeFromTotePickingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExcludeFromTotePickingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 12, true);
			this.ExcludeFromTotePickingCheckBox.Name = "ExcludeFromTotePickingCheckBox";
			this.ExcludeFromTotePickingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExcludeFromTotePickingCheckBox.TabIndex = 4;
			this.ExcludeFromTotePickingCheckBox.UseVisualStyleBackColor = true;
			// 
			// DatesAndTotalsGroupBox
			// 
			this.DatesAndTotalsGroupBox.Controls.Add(this.WD_TZ_TransportZone);
			this.DatesAndTotalsGroupBox.Controls.Add(this.ScreenButton);
			this.DatesAndTotalsGroupBox.Controls.Add(this.ScreeningStatusDropEdit);
			this.DatesAndTotalsGroupBox.Controls.Add(this.WD_TotalOrderValue);
			this.DatesAndTotalsGroupBox.Controls.Add(this.WD_TotalWeightCalcDropEdit);
			this.DatesAndTotalsGroupBox.Controls.Add(this.WD_TotalCubicCalcDropEdit);
			this.DatesAndTotalsGroupBox.Controls.Add(this.WD_TotalUnitsFromLinesCalcEdit);
			this.DatesAndTotalsGroupBox.Controls.Add(this.TotalUnitsCalcEdit);
			this.DatesAndTotalsGroupBox.Controls.Add(this.RequiredDateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DatesAndTotalsGroupBox, false);
			this.DatesAndTotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 8, true);
			this.DatesAndTotalsGroupBox.Name = "DatesAndTotalsGroupBox";
			this.DatesAndTotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 220, true);
			this.DatesAndTotalsGroupBox.TabIndex = 2;
			this.DatesAndTotalsGroupBox.TabStop = false;
			// 
			// WD_TZ_TransportZone
			// 
			this.WD_TZ_TransportZone.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_TZ_TransportZone, "WD_TZ_TransportZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsOrder)(null)).WD_TZ_TransportZone)));
			this.WD_TZ_TransportZone.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("1A2CB352-8FF0-4CBC-8085-AB17C8E1562E", "Transport Zone");
			this.WD_TZ_TransportZone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 163, true);
			this.WD_TZ_TransportZone.Name = "WD_TZ_TransportZone";
			this.WD_TZ_TransportZone.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WD_TZ_TransportZone.ParentType = null;
			this.WD_TZ_TransportZone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.WD_TZ_TransportZone.TabIndex = 6;
			this.WD_TZ_TransportZone.TabStop = false;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 190, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 0;
			this.ScreenButton.TabStop = false;
			this.ScreenButton.Text = "...";
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new EventHandler(this.ScreenButton_Click);
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatusDropEdit, "WD_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_ScreeningStatus)));
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 190, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 38, true);
			this.ScreeningStatusDropEdit.TabIndex = 6;
			// 
			// WD_TotalWeightCalcDropEdit
			// 
			this.WD_TotalWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_TotalWeightUnit)));
			this.WD_TotalWeightCalcDropEdit.BindToAmount = "WD_TotalWeight";
			this.WD_TotalWeightCalcDropEdit.BindToUnit = "WD_TotalWeightUnit";
			this.WD_TotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|2c671f5e-e5ae-41bf-a4f6-be19260381fe", "Total Line Weight");
			this.WD_TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 64, true);
			this.WD_TotalWeightCalcDropEdit.Name = "WD_TotalWeightCalcDropEdit";
			this.WD_TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.WD_TotalWeightCalcDropEdit.TabIndex = 2;
			// 
			// WD_TotalCubicCalcDropEdit
			// 
			this.WD_TotalCubicCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_TotalCubicCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_TotalCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_TotalCubicUnit)));
			this.WD_TotalCubicCalcDropEdit.BindToAmount = "WD_TotalCubic";
			this.WD_TotalCubicCalcDropEdit.BindToUnit = "WD_TotalCubicUnit";
			this.WD_TotalCubicCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|875c06d4-82dd-4e87-b6e1-241185126b5a", "Total Line Volume");
			this.WD_TotalCubicCalcDropEdit.Decimals = 3;
			this.WD_TotalCubicCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 89, true);
			this.WD_TotalCubicCalcDropEdit.Name = "WD_TotalCubicCalcDropEdit";
			this.WD_TotalCubicCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.WD_TotalCubicCalcDropEdit.TabIndex = 3;
			// 
			// WD_TotalUnitsFromLinesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WD_TotalUnitsFromLinesCalcEdit, "WD_TotalUnitsFromLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_TotalUnitsFromLines)));
			this.WD_TotalUnitsFromLinesCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|b2b334b2-a41d-45f7-87f0-12754f9eead6", "Total Line Units");
			this.WD_TotalUnitsFromLinesCalcEdit.DecimalPlaces = 2;
			this.WD_TotalUnitsFromLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 114, true);
			this.WD_TotalUnitsFromLinesCalcEdit.Name = "WD_TotalUnitsFromLinesCalcEdit";
			this.WD_TotalUnitsFromLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.WD_TotalUnitsFromLinesCalcEdit.TabIndex = 4;
			this.WD_TotalUnitsFromLinesCalcEdit.TabStop = false;
			this.WD_TotalUnitsFromLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WD_TotalUnitsFromLinesCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalUnitsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalUnitsCalcEdit, "WD_TotalUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_TotalUnits)));
			this.TotalUnitsCalcEdit.DecimalPlaces = 2;
			this.TotalUnitsCalcEdit.IsCalculatorEnabled = false;
			this.TotalUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 39, true);
			this.TotalUnitsCalcEdit.Name = "TotalUnitsCalcEdit";
			this.TotalUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.TotalUnitsCalcEdit.TabIndex = 1;
			this.TotalUnitsCalcEdit.Text = "0";
			this.TotalUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalUnitsCalcEdit.TrackDisposedAccess = true;
			this.TotalUnitsCalcEdit.WordWrap = false;
			// 
			// RequiredDateEdit
			//
			this.RequiredDateEdit.AllowDrop = true;
			this.RequiredDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RequiredDateEdit, "RequiredDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).RequiredDate)));
			this.RequiredDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RequiredDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 14, true);
			this.RequiredDateEdit.Name = "RequiredDateEdit";
			this.RequiredDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 38, true);
			this.RequiredDateEdit.TabIndex = 0;
			// 
			// TransportGroupBox
			// 
			this.TransportGroupBox.Controls.Add(this.SalesChannelGuidFindBox);
			this.TransportGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.TransportGroupBox.Controls.Add(this.CarrierServiceLevelDropEdit);
			this.TransportGroupBox.Controls.Add(this.TransportJobLabel);
			this.TransportGroupBox.Controls.Add(this.TransportJobLinkLabel);
			this.TransportGroupBox.Controls.Add(this.TransportRefLinkButton);
			this.TransportGroupBox.Controls.Add(this.zCalcEdit1);
			this.TransportGroupBox.Controls.Add(this.VehicleNoTextBox);
			this.TransportGroupBox.Controls.Add(this.ReferenceNoTextBox);
			this.TransportGroupBox.Controls.Add(this.DropModeDropEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TransportGroupBox, false);
			this.TransportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 8, true);
			this.TransportGroupBox.Name = "TransportGroupBox";
			this.TransportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 220, true);
			this.TransportGroupBox.TabIndex = 1;
			this.TransportGroupBox.TabStop = false;
			// 
			// SalesChannelGuidFindBox
			// 
			this.SalesChannelGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesChannelGuidFindBox, "WD_WSH_SalesChannel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsOrder)(null)).WD_WSH_SalesChannel)));
			this.SalesChannelGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("4f78ea1b-4718-4710-b931-a4ec3d03b476", "Sales Channel");
			this.SalesChannelGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 190, true);
			this.SalesChannelGuidFindBox.Name = "SalesChannelGuidFindBox";
			this.SalesChannelGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SalesChannelGuidFindBox.ParentType = null;
			this.SalesChannelGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 38, true);
			this.SalesChannelGuidFindBox.TabIndex = 7;
			this.SalesChannelGuidFindBox.TabStop = false;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "WD_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f674aa38-b10b-4dbd-a8c8-9d471c6263b9", "Description", "Goods Description");
			this.GoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 164, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// CarrierServiceLevelDropEdit
			// 
			this.CarrierServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceLevelDropEdit, "WD_PL_NKCarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_PL_NKCarrierServiceLevel)));
			this.CarrierServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 89, true);
			this.CarrierServiceLevelDropEdit.Name = "CarrierServiceLevelDropEdit";
			this.CarrierServiceLevelDropEdit.PreBoundMaxLength = 4;
			this.CarrierServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.CarrierServiceLevelDropEdit.TabIndex = 4;
			// 
			// TransportJobLabel
			// 
			this.TransportJobLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|2420f5d0-6991-4feb-8f35-0952a2ccbb61", "Transport Job:");
			this.TransportJobLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TransportJobLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 36, true);
			this.TransportJobLabel.Name = "TransportJobLabel";
			this.TransportJobLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.TransportJobLabel.TabIndex = 14;
			this.TransportJobLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.TransportJobLabel.UseMnemonic = false;
			// 
			// TransportJobLinkLabel
			// 
			this.TransportJobLinkLabel.AutoSize = true;
			this.TransportJobLinkLabel.IsFontBold = false;
			this.TransportJobLinkLabel.LinkColor = System.Drawing.Color.Red;
			this.TransportJobLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 39, true);
			this.TransportJobLinkLabel.Name = "TransportJobLinkLabel";
			this.TransportJobLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.TransportJobLinkLabel.TabIndex = 2;
			this.TransportJobLinkLabel.TabStop = false;
			// 
			// TransportRefLinkButton
			// 
			this.BindingSource.SetBindingMember(this.TransportRefLinkButton, ".");
			this.TransportRefLinkButton.BindToTransportCo = "TransportCoDocAddress.Organisation";
			this.TransportRefLinkButton.BindToTransportRef = "WD_TransportReference";
			this.TransportRefLinkButton.Image = ((System.Drawing.Image)(resources.GetObject("TransportRefLinkButton.Image")));
			this.TransportRefLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 12, true);
			this.TransportRefLinkButton.Name = "TransportRefLinkButton";
			this.TransportRefLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 22, true);
			this.TransportRefLinkButton.TabIndex = 1;
			this.TransportRefLinkButton.TabStop = false;
			this.TransportRefLinkButton.ToolTipCaption = null;
			this.TransportRefLinkButton.UseVisualStyleBackColor = true;
			this.TransportRefLinkButton.Click += new EventHandler(this.TransportRefLinkButton_Click);
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "WD_LocalCartInsuranceCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_LocalCartInsuranceCost)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.IsCalculatorEnabled = false;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 114, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.Text = "0.00";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit1.TrackDisposedAccess = true;
			this.zCalcEdit1.WordWrap = false;
			// 
			// VehicleNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleNoTextBox, "VehicleNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).VehicleNo)));
			this.VehicleNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|7db4cf8b-053c-4424-bc5a-c42b74e5e546", "Vehicle No");
			this.VehicleNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 64, true);
			this.VehicleNoTextBox.Name = "VehicleNoTextBox";
			this.VehicleNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.VehicleNoTextBox.TabIndex = 3;
			// 
			// ReferenceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNoTextBox, "WD_TransportReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_TransportReference)));
			this.ReferenceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 14, true);
			this.ReferenceNoTextBox.Name = "ReferenceNoTextBox";
			this.ReferenceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.ReferenceNoTextBox.TabIndex = 0;
			// 
			// DropModeDropEdit
			// 
			this.DropModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "WD_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 139, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.DropModeDropEdit.TabIndex = 6;
			// 
			// JobGroupBox
			// 
			this.JobGroupBox.Controls.Add(this.PropertyCalcEdit);
			this.JobGroupBox.Controls.Add(this.ServiceLevelCodeBox);
			this.JobGroupBox.Controls.Add(this.FulfillmentRuleDropEdit);
			this.JobGroupBox.Controls.Add(this.OrderNoSplitCalcEdit);
			this.JobGroupBox.Controls.Add(this.StatusTextBox);
			this.JobGroupBox.Controls.Add(this.CustomerRefTextBox);
			this.JobGroupBox.Controls.Add(this.PickNoTextBox);
			this.JobGroupBox.Controls.Add(this.PickOptionDropEdit);
			this.JobGroupBox.Controls.Add(this.WD_ExternalReferenceTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobGroupBox, false);
			this.JobGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 8, true);
			this.JobGroupBox.Name = "JobGroupBox";
			this.JobGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 220, true);
			this.JobGroupBox.TabIndex = 0;
			this.JobGroupBox.TabStop = false;
			// 
			// PropertyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyCalcEdit, "WD_PickPriority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_PickPriority)));
			this.PropertyCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|B9BEA94A-94CF-4B89-B34B-C56665A26895", "Priority");
			this.PropertyCalcEdit.DecimalPlaces = 0;
			this.PropertyCalcEdit.Decimals = 0;
			this.PropertyCalcEdit.IsCalculatorEnabled = false;
			this.PropertyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 139, true);
			this.PropertyCalcEdit.Name = "PropertyCalcEdit";
			this.PropertyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 38, true);
			this.PropertyCalcEdit.TabIndex = 7;
			this.PropertyCalcEdit.Text = "0";
			this.PropertyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PropertyCalcEdit.TrackDisposedAccess = true;
			this.PropertyCalcEdit.WordWrap = false;
			// 
			// ServiceLevelCodeBox
			// 
			this.ServiceLevelCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeBox, "WD_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_RS_NKServiceLevel)));
			this.ServiceLevelCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 64, true);
			this.ServiceLevelCodeBox.Name = "ServiceLevelCodeBox";
			this.ServiceLevelCodeBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ServiceLevelCodeBox.ParentType = null;
			this.ServiceLevelCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 38, true);
			this.ServiceLevelCodeBox.TabIndex = 4;
			// 
			// FulfillmentRuleDropEdit
			// 
			this.FulfillmentRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FulfillmentRuleDropEdit, "WD_WhsOrderFulfillmentRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_WhsOrderFulfillmentRule)));
			this.FulfillmentRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 114, true);
			this.FulfillmentRuleDropEdit.Name = "FulfillmentRuleDropEdit";
			this.FulfillmentRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 38, true);
			this.FulfillmentRuleDropEdit.TabIndex = 5;
			// 
			// OrderNoSplitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OrderNoSplitCalcEdit, "WD_ExternalReferenceSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_ExternalReferenceSplit)));
			this.OrderNoSplitCalcEdit.DecimalPlaces = 2;
			this.OrderNoSplitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 14, true);
			this.OrderNoSplitCalcEdit.Name = "OrderNoSplitCalcEdit";
			this.OrderNoSplitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 38, true);
			this.OrderNoSplitCalcEdit.TabIndex = 2;
			this.OrderNoSplitCalcEdit.Text = "0";
			this.OrderNoSplitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.OrderNoSplitCalcEdit.TrackDisposedAccess = true;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WarehouseOrderStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WarehouseOrderStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|ec649621-6ff8-4d45-8b90-59eb9bf26b74", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 89, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 38, true);
			this.StatusTextBox.TabIndex = 4;
			// 
			// CustomerRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerRefTextBox, "WD_CustomerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_CustomerReference)));
			this.CustomerRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 40, true);
			this.CustomerRefTextBox.Name = "CustomerRefTextBox";
			this.CustomerRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 38, true);
			this.CustomerRefTextBox.TabIndex = 3;
			// 
			// PickNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PickNoTextBox, "Pick.WP_PickNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).Pick.WP_PickNo)));
			this.PickNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|70ec5906-2d49-433f-8c38-468faba919c3", "Pick No");
			this.PickNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 139, true);
			this.PickNoTextBox.Name = "PickNoTextBox";
			this.PickNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 38, true);
			this.PickNoTextBox.TabIndex = 6;
			// 
			// PickOptionDropEdit
			// 
			this.PickOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickOptionDropEdit, "WD_PickOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_PickOption)));
			this.PickOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 164, true);
			this.PickOptionDropEdit.Name = "PickOptionDropEdit";
			this.PickOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 38, true);
			this.PickOptionDropEdit.TabIndex = 7;
			// 
			// WD_ExternalReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.WD_ExternalReferenceTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).WD_ExternalReference)));
			this.WD_ExternalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 14, true);
			this.WD_ExternalReferenceTextBox.Name = "WD_ExternalReferenceTextBox";
			this.WD_ExternalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 38, true);
			this.WD_ExternalReferenceTextBox.TabIndex = 1;
			// 
			// DetailTopPanel
			// 
			this.DetailTopPanel.Controls.Add(this.zTemplateTabControl1);
			this.DetailTopPanel.Controls.Add(this.zGuidFindBox1);
			this.DetailTopPanel.Controls.Add(this.OrgWhsFindControl);
			this.DetailTopPanel.Controls.Add(this.CompaniesTabControl);
			this.DetailTopPanel.Controls.Add(this.SubTypeDropEdit);
			this.DetailTopPanel.Controls.Add(this.AwaitingResponseLabel);
			this.DetailTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTopPanel.Name = "DetailTopPanel";
			this.DetailTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 228, true);
			this.DetailTopPanel.TabIndex = 0;
			// 
			// zTemplateTabControl1
			// 
			this.zTemplateTabControl1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTemplateTabControl1.Controls.Add(this.zTabPage1);
			this.zTemplateTabControl1.Controls.Add(this.zTabPage2);
			this.zTemplateTabControl1.Controls.Add(this.FreightForwarderTab);
			this.zTemplateTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 1, true);
			this.zTemplateTabControl1.Name = "zTemplateTabControl1";
			this.zTemplateTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.zTemplateTabControl1.TabIndex = 3;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|0f25e79a-8257-4502-9017-ec6f0c3541e3", "Transport");
			this.zTabPage1.Controls.Add(this.TransportDocAddressControl);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.zTabPage1.TabIndex = 0;
			// 
			// TransportDocAddressControl
			// 
			this.TransportDocAddressControl.AddressValidationProcessCmdKey = null;
			this.TransportDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocAddressControl, "TransportCoDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsOrder)(null)).TransportCoDocAddress)));
			this.TransportDocAddressControl.BindToOrganisations = "Lookups+TransportCos";
			this.TransportDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|daf21212-6eb4-4599-be21-61d3044ece63", "Transport");
			this.TransportDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.TransportDocAddressControl.Name = "TransportDocAddressControl";
			this.TransportDocAddressControl.ReadOnly = false;
			this.TransportDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.TransportDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.TransportDocAddressControl.TabIndex = 0;
			this.TransportDocAddressControl.ValidationJustForced = false;
			// 
			// zTabPage2
			// 
			this.zTabPage2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|fbea0775-8550-44b7-9715-09e7128d5a7b", "Transport Billed To");
			this.zTabPage2.Controls.Add(this.TransportBilledToDocAddressControl);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.zTabPage2.TabIndex = 0;
			// 
			// TransportBilledToDocAddressControl
			// 
			this.TransportBilledToDocAddressControl.AddressValidationProcessCmdKey = null;
			this.TransportBilledToDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportBilledToDocAddressControl, "TransportBillToDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsOrder)(null)).TransportBillToDocAddress)));
			this.TransportBilledToDocAddressControl.BindToOrganisations = "Lookups+Debtors";
			this.TransportBilledToDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|c4a7de64-dfb4-47c3-a322-734e4ec4cdc8", "Transport Billed To");
			this.TransportBilledToDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.TransportBilledToDocAddressControl.Name = "TransportBilledToDocAddressControl";
			this.TransportBilledToDocAddressControl.ReadOnly = false;
			this.TransportBilledToDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.TransportBilledToDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.TransportBilledToDocAddressControl.TabIndex = 0;
			this.TransportBilledToDocAddressControl.ValidationJustForced = false;
			// 
			// FreightForwarderTab
			// 
			this.FreightForwarderTab.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("19e99546-efeb-4af4-a1cf-7964855e25ad", "Forwarder");
			this.FreightForwarderTab.Controls.Add(this.FreightForwarderPanel);
			this.FreightForwarderTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.FreightForwarderTab.Name = "FreightForwarderTab";
			this.FreightForwarderTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FreightForwarderTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.FreightForwarderTab.TabIndex = 1;
			// 
			// FreightForwarderPanel
			// 
			this.FreightForwarderPanel.Controls.Add(this.FreightForwarderOrganisationControl);
			this.FreightForwarderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreightForwarderPanel.Name = "FreightForwarderPanel";
			this.FreightForwarderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 188, true);
			this.FreightForwarderPanel.TabIndex = 0;
			// 
			// FreightForwarderOrganisationControl
			// 
			this.FreightForwarderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightForwarderOrganisationControl, "WD_OH_Forwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsOrder)(null)).WD_OH_Forwarder)));
			this.FreightForwarderOrganisationControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f195aef7-4761-4984-b644-d623a53236ff", "Freight Forwarder");
			this.FreightForwarderOrganisationControl.Captions = new string[] { "Freight Forwarder" };
			this.FreightForwarderOrganisationControl.IsCaptionOverridden = false;
			this.FreightForwarderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.FreightForwarderOrganisationControl.Name = "FreightForwarderOrganisationControl";
			this.FreightForwarderOrganisationControl.OrgAddressFormatter = null;
			this.FreightForwarderOrganisationControl.PopupCaption = "";
			this.FreightForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.FreightForwarderOrganisationControl.TabIndex = 1;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsOrder)(null)).WD_WW_Whs)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 162, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox1.ParentType = null;
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 38, true);
			this.zGuidFindBox1.TabIndex = 1;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsOrder)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|43636b0d-8a85-437a-9faa-579ec861d1ef", "Client");
			this.OrgWhsFindControl.Captions = new string[] { "Client" };
			this.OrgWhsFindControl.IsCaptionOverridden = false;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.OrgAddressFormatter = null;
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.OrgWhsFindControl.TabIndex = 0;
			// 
			// CompaniesTabControl
			// 
			this.CompaniesTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CompaniesTabControl.Controls.Add(this.ConsigneeTabPage);
			this.CompaniesTabControl.Controls.Add(this.GoodsBilledToTabPage);
			this.CompaniesTabControl.Controls.Add(this.DistributionCentreTabPage);
			this.CompaniesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 1, true);
			this.CompaniesTabControl.Name = "CompaniesTabControl";
			this.CompaniesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.CompaniesTabControl.TabIndex = 4;
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|378dda5c-14a7-4f24-b80a-1f4489cabb4b", "Consignee");
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeDocAddressControl);
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.ConsigneeTabPage.TabIndex = 0;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "ConsigneeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsOrder)(null)).ConsigneeDocAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups+Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|8c307753-672a-49c1-b910-65d45f3cca56", "Consignee");
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.ShowResidentialAddressOnOverride = true;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 0;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// GoodsBilledToTabPage
			// 
			this.GoodsBilledToTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|f29d35ca-44ea-4460-8160-f6a1a3f5409b", "Goods Billed To");
			this.GoodsBilledToTabPage.Controls.Add(this.GoodsBillToDocAddressControl);
			this.GoodsBilledToTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.GoodsBilledToTabPage.Name = "GoodsBilledToTabPage";
			this.GoodsBilledToTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.GoodsBilledToTabPage.TabIndex = 0;
			// 
			// GoodsBillToDocAddressControl
			// 
			this.GoodsBillToDocAddressControl.AddressValidationProcessCmdKey = null;
			this.GoodsBillToDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsBillToDocAddressControl, "GoodsBillToDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsOrder)(null)).GoodsBillToDocAddress)));
			this.GoodsBillToDocAddressControl.BindToOrganisations = "Lookups+Debtors";
			this.GoodsBillToDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|7377f65a-70ea-42d2-b281-981344fb3988", "Goods Billed To");
			this.GoodsBillToDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.GoodsBillToDocAddressControl.Name = "GoodsBillToDocAddressControl";
			this.GoodsBillToDocAddressControl.ReadOnly = false;
			this.GoodsBillToDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.GoodsBillToDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.GoodsBillToDocAddressControl.TabIndex = 0;
			this.GoodsBillToDocAddressControl.ValidationJustForced = false;
			// 
			// DistributionCentreTabPage
			// 
			this.DistributionCentreTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|a6c86df9-8559-4732-8d0a-3f594a0c3f97", "DC", "Distribution Center", "");
			this.DistributionCentreTabPage.Controls.Add(this.DistributionCentreDocAddressControl);
			this.DistributionCentreTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.DistributionCentreTabPage.Name = "DistributionCentreTabPage";
			this.DistributionCentreTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DistributionCentreTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 171, true);
			this.DistributionCentreTabPage.TabIndex = 1;
			this.DistributionCentreTabPage.Text = "Distribution Centre";
			// 
			// DistributionCentreDocAddressControl
			// 
			this.DistributionCentreDocAddressControl.AddressValidationProcessCmdKey = null;
			this.DistributionCentreDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DistributionCentreDocAddressControl, "DistributionCentreDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsOrder)(null)).DistributionCentreDocAddress)));
			this.DistributionCentreDocAddressControl.BindToOrganisations = "Lookups+DistributionCentres";
			this.DistributionCentreDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DistributionCentreDocAddressControl|fffa02bf-325c-4966-8c23-6faf85cd5e52", "DC", "Distribution Center", "");
			this.DistributionCentreDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.DistributionCentreDocAddressControl.Name = "DistributionCentreDocAddressControl";
			this.DistributionCentreDocAddressControl.ReadOnly = false;
			this.DistributionCentreDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DistributionCentreDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DistributionCentreDocAddressControl.TabIndex = 0;
			this.DistributionCentreDocAddressControl.ValidationJustForced = false;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "WD_DocketSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsOrder)(null)).WD_DocketSubType)));
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 188, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 38, true);
			this.SubTypeDropEdit.TabIndex = 2;
			// 
			// AwaitingResponseLabel
			// 
			this.AwaitingResponseLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AwaitingResponseLabel, "AwaitingCustomsResponseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsOrder)(null)).AwaitingCustomsResponseStatus)));
			this.AwaitingResponseLabel.ForeColor = System.Drawing.Color.Red;
			this.AwaitingResponseLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AwaitingResponseLabel, false);
			this.AwaitingResponseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 212, true);
			this.AwaitingResponseLabel.Name = "AwaitingResponseLabel";
			this.AwaitingResponseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.AwaitingResponseLabel.TabIndex = 6;
			this.AwaitingResponseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|08faf273-bb35-411f-b833-10ca5879b83c", "Lines");
			this.LinesTabPage.Controls.Add(this.OrderLinesGridAndAdditionalDataPanel);
			this.LinesTabPage.Controls.Add(this.splitter1);
			this.LinesTabPage.Controls.Add(this.bottomPanel);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.LinesTabPage.TabIndex = 2;
			// 
			// OrderLinesGridAndAdditionalDataPanel
			// 
			this.OrderLinesGridAndAdditionalDataPanel.Controls.Add(this.OrderLinesGridPanel);
			this.OrderLinesGridAndAdditionalDataPanel.Controls.Add(this.OrderLinesTabAdditionalDataPanel);
			this.OrderLinesGridAndAdditionalDataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderLinesGridAndAdditionalDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderLinesGridAndAdditionalDataPanel.Name = "OrderLinesGridAndAdditionalDataPanel";
			this.OrderLinesGridAndAdditionalDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 457, true);
			this.OrderLinesGridAndAdditionalDataPanel.TabIndex = 5;
			// 
			// OrderLinesGridPanel
			// 
			this.OrderLinesGridPanel.Controls.Add(this.DocketLinesGridUserControl);
			this.OrderLinesGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderLinesGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderLinesGridPanel.Name = "OrderLinesGridPanel";
			this.OrderLinesGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 422, true);
			this.OrderLinesGridPanel.TabIndex = 6;
			// 
			// DocketLinesGridUserControl
			// 
			this.DocketLinesGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocketLinesGridUserControl, ".");
			this.DocketLinesGridUserControl.BindTo = "ParentLines";
			this.DocketLinesGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocketLinesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocketLinesGridUserControl.Name = "DocketLinesGridUserControl";
			this.DocketLinesGridUserControl.ReduceStockOptionsAreVisible = false;
			this.DocketLinesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 422, true);
			this.DocketLinesGridUserControl.TabIndex = 1;
			// 
			// OrderLinesTabAdditionalDataPanel
			// 
			this.OrderLinesTabAdditionalDataPanel.Controls.Add(this.WD_TotalLineUnitsOnLinesTabCalcEdit);
			this.OrderLinesTabAdditionalDataPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OrderLinesTabAdditionalDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 422, true);
			this.OrderLinesTabAdditionalDataPanel.Name = "OrderLinesTabAdditionalDataPanel";
			this.OrderLinesTabAdditionalDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 35, true);
			this.OrderLinesTabAdditionalDataPanel.TabIndex = 5;
			// 
			// WD_TotalLineUnitsOnLinesTabCalcEdit
			// 
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalLineUnitsOnLinesTabCalcEdit, "WD_TotalUnitsFromLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsOrder)(null)).WD_TotalUnitsFromLines)));
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|2ffbab0d-34a4-4695-aead-e2f8ca00fca7", "Total Line Units");
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.DecimalPlaces = 2;
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(886, 7, true);
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Name = "WD_TotalLineUnitsOnLinesTabCalcEdit";
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 38, true);
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.TabIndex = 9;
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.TrackDisposedAccess = true;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 457, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 6, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 463, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 251, true);
			this.bottomPanel.TabIndex = 3;
			// 
			// StagingTabPage
			// 
			this.StagingTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|01011cf5-6fd0-4b5f-bd8d-c8d377f71a46", "Cross-Dock");
			this.StagingTabPage.Controls.Add(this.orderEntryStagingUserControl21);
			this.StagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.StagingTabPage.Name = "StagingTabPage";
			this.StagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.StagingTabPage.TabIndex = 4;
			// 
			// orderEntryStagingUserControl21
			// 
			this.orderEntryStagingUserControl21.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orderEntryStagingUserControl21, ".");
			this.orderEntryStagingUserControl21.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orderEntryStagingUserControl21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.orderEntryStagingUserControl21.Name = "orderEntryStagingUserControl21";
			this.orderEntryStagingUserControl21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.orderEntryStagingUserControl21.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|9080b3f9-bdab-430f-aea9-760a2c26f1e3", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.processTemplateCustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.CustomFieldsTabPage.TabIndex = 5;
			// 
			// processTemplateCustomFieldsControl
			// 
			this.processTemplateCustomFieldsControl.AllowDrop = true;
			this.processTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.processTemplateCustomFieldsControl.Name = "processTemplateCustomFieldsControl";
			this.processTemplateCustomFieldsControl.NothingSetupMessageLabelText = "";
			this.processTemplateCustomFieldsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.processTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.processTemplateCustomFieldsControl.TabIndex = 1;
			// 
			// ForwardingTabPage
			// 
			this.ForwardingTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|99a642b9-11ff-4879-acfd-9cc446366b02", "Forwarding");
			this.ForwardingTabPage.Controls.Add(this.OrderForwardingControl);
			this.ForwardingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ForwardingTabPage.Name = "ForwardingTabPage";
			this.ForwardingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ForwardingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ForwardingTabPage.TabIndex = 3;
			// 
			// OrderForwardingControl
			// 
			this.OrderForwardingControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrderForwardingControl, ".");
			this.OrderForwardingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.OrderForwardingControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 207, true);
			this.OrderForwardingControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 207, true);
			this.OrderForwardingControl.Name = "OrderForwardingControl";
			this.OrderForwardingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 207, true);
			this.OrderForwardingControl.TabIndex = 0;
			// 
			// ReferenceTabPage
			// 
			this.ReferenceTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|e37addb6-68a4-41bb-b64f-42033864a80b", "References");
			this.ReferenceTabPage.Controls.Add(this.DocketReferenceGridUserControl);
			this.ReferenceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ReferenceTabPage.Name = "ReferenceTabPage";
			this.ReferenceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ReferenceTabPage.TabIndex = 0;
			// 
			// DocketReferenceGridUserControl
			// 
			this.DocketReferenceGridUserControl.AllowDrop = true;
			this.DocketReferenceGridUserControl.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocketReferenceGridUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsDocket)(((WhsOrder)(null)))));
			this.DocketReferenceGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.DocketReferenceGridUserControl.Name = "DocketReferenceGridUserControl";
			this.DocketReferenceGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 674, true);
			this.DocketReferenceGridUserControl.TabIndex = 0;
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|b4266f22-c08a-4cea-88a9-bfe95f3d6163", "Containers");
			this.ContainersTabPage.Controls.Add(this.ContainersPanel);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ContainersTabPage.TabIndex = 1;
			// 
			// ContainersPanel
			// 
			this.ContainersPanel.Controls.Add(this.ContainersUserControl);
			this.ContainersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersPanel.Name = "ContainersPanel";
			this.ContainersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ContainersPanel.TabIndex = 0;
			// 
			// ContainersUserControl
			// 
			this.ContainersUserControl.AllowDrop = true;
			this.ContainersUserControl.BindTo = "Containers";
			this.ContainersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersUserControl.Name = "ContainersUserControl";
			this.ContainersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ContainersUserControl.TabIndex = 0;
			// 
			// ServicesTab
			// 
			this.ServicesTab.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|ddcf5509-94ee-4afe-9f9e-b20034c918a7", "Services");
			this.ServicesTab.Controls.Add(this.ServicesPanel);
			this.ServicesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ServicesTab.Name = "ServicesTab";
			this.ServicesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.ServicesTab.TabIndex = 6;
			this.ServicesTab.UseVisualStyleBackColor = true;
			// 
			// ServicesPanel
			// 
			this.ServicesPanel.Controls.Add(this.ServicesControl);
			this.ServicesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServicesPanel.Name = "ServicesPanel";
			this.ServicesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 708, true);
			this.ServicesPanel.TabIndex = 0;
			// 
			// ServicesControl
			// 
			this.ServicesControl.AllowDrop = true;
			this.ServicesControl.BindToServices = "Services";
			this.ServicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesControl.IsContextVisibleInGrid = false;
			this.ServicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesControl.Name = "ServicesControl";
			this.ServicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 708, true);
			this.ServicesControl.TabIndex = 0;
			// 
			// RelatedJobsTabPage
			// 
			this.RelatedJobsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("c2105b16-d276-4c3a-ad11-bf9d2e3c3ce2", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.RelatedJobCollection)(((WhsOrder)(null)).RelatedJobs)));
			this.RelatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.RelatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.RelatedJobsTabPage.Name = "RelatedJobsTabPage";
			this.RelatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 714, true);
			this.RelatedJobsTabPage.TabIndex = 7;
			// 
			// OrderEntryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailTabControl);
			this.Name = "OrderEntryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 758, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.WD_TotalOrderValue.ResumeLayout(true);
			this.WD_TotalOrderValue.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailTabPage.ResumeLayout(false);
			this.DetailTabPage.PerformLayout();
			this.orderDocketLinesGridUserControl1.ResumeLayout(true);
			this.orderDocketLinesGridUserControl1.PerformLayout();
			this.DetailsBottomGroupBox.ResumeLayout(false);
			this.DetailsBottomGroupBox.PerformLayout();
			this.OrderConfigFlagsGroupBox.ResumeLayout(false);
			this.OrderConfigFlagsGroupBox.PerformLayout();
			this.DatesAndTotalsGroupBox.ResumeLayout(false);
			this.DatesAndTotalsGroupBox.PerformLayout();
			this.WD_TZ_TransportZone.ResumeLayout(true);
			this.WD_TZ_TransportZone.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.WD_TotalWeightCalcDropEdit.ResumeLayout(true);
			this.WD_TotalWeightCalcDropEdit.PerformLayout();
			this.WD_TotalCubicCalcDropEdit.ResumeLayout(true);
			this.WD_TotalCubicCalcDropEdit.PerformLayout();
			this.RequiredDateEdit.ResumeLayout(true);
			this.RequiredDateEdit.PerformLayout();
			this.TransportGroupBox.ResumeLayout(false);
			this.TransportGroupBox.PerformLayout();
			this.SalesChannelGuidFindBox.ResumeLayout(true);
			this.SalesChannelGuidFindBox.PerformLayout();
			this.CarrierServiceLevelDropEdit.ResumeLayout(true);
			this.CarrierServiceLevelDropEdit.PerformLayout();
			this.DropModeDropEdit.ResumeLayout(true);
			this.DropModeDropEdit.PerformLayout();
			this.JobGroupBox.ResumeLayout(false);
			this.JobGroupBox.PerformLayout();
			this.ServiceLevelCodeBox.ResumeLayout(true);
			this.ServiceLevelCodeBox.PerformLayout();
			this.FulfillmentRuleDropEdit.ResumeLayout(true);
			this.FulfillmentRuleDropEdit.PerformLayout();
			this.PickOptionDropEdit.ResumeLayout(true);
			this.PickOptionDropEdit.PerformLayout();
			this.DetailTopPanel.ResumeLayout(false);
			this.DetailTopPanel.PerformLayout();
			this.zTemplateTabControl1.ResumeLayout(false);
			this.zTemplateTabControl1.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.TransportDocAddressControl.ResumeLayout(true);
			this.TransportDocAddressControl.PerformLayout();
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			this.TransportBilledToDocAddressControl.ResumeLayout(true);
			this.TransportBilledToDocAddressControl.PerformLayout();
			this.FreightForwarderTab.ResumeLayout(false);
			this.FreightForwarderTab.PerformLayout();
			this.FreightForwarderPanel.ResumeLayout(false);
			this.FreightForwarderPanel.PerformLayout();
			this.FreightForwarderOrganisationControl.ResumeLayout(true);
			this.FreightForwarderOrganisationControl.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.OrgWhsFindControl.ResumeLayout(true);
			this.OrgWhsFindControl.PerformLayout();
			this.CompaniesTabControl.ResumeLayout(false);
			this.CompaniesTabControl.PerformLayout();
			this.ConsigneeTabPage.ResumeLayout(false);
			this.ConsigneeTabPage.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.GoodsBilledToTabPage.ResumeLayout(false);
			this.GoodsBilledToTabPage.PerformLayout();
			this.GoodsBillToDocAddressControl.ResumeLayout(true);
			this.GoodsBillToDocAddressControl.PerformLayout();
			this.DistributionCentreTabPage.ResumeLayout(false);
			this.DistributionCentreTabPage.PerformLayout();
			this.DistributionCentreDocAddressControl.ResumeLayout(true);
			this.DistributionCentreDocAddressControl.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.OrderLinesGridAndAdditionalDataPanel.ResumeLayout(false);
			this.OrderLinesGridAndAdditionalDataPanel.PerformLayout();
			this.OrderLinesGridPanel.ResumeLayout(false);
			this.OrderLinesGridPanel.PerformLayout();
			this.DocketLinesGridUserControl.ResumeLayout(true);
			this.DocketLinesGridUserControl.PerformLayout();
			this.OrderLinesTabAdditionalDataPanel.ResumeLayout(false);
			this.OrderLinesTabAdditionalDataPanel.PerformLayout();
			this.StagingTabPage.ResumeLayout(false);
			this.StagingTabPage.PerformLayout();
			this.orderEntryStagingUserControl21.ResumeLayout(true);
			this.orderEntryStagingUserControl21.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.processTemplateCustomFieldsControl.ResumeLayout(true);
			this.processTemplateCustomFieldsControl.PerformLayout();
			this.ForwardingTabPage.ResumeLayout(false);
			this.ForwardingTabPage.PerformLayout();
			this.OrderForwardingControl.ResumeLayout(true);
			this.OrderForwardingControl.PerformLayout();
			this.ReferenceTabPage.ResumeLayout(false);
			this.ReferenceTabPage.PerformLayout();
			this.DocketReferenceGridUserControl.ResumeLayout(true);
			this.DocketReferenceGridUserControl.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainersPanel.ResumeLayout(false);
			this.ContainersPanel.PerformLayout();
			this.ContainersUserControl.ResumeLayout(true);
			this.ContainersUserControl.PerformLayout();
			this.ServicesTab.ResumeLayout(false);
			this.ServicesTab.PerformLayout();
			this.ServicesPanel.ResumeLayout(false);
			this.ServicesPanel.PerformLayout();
			this.ServicesControl.ResumeLayout(true);
			this.ServicesControl.PerformLayout();
			this.RelatedJobsTabPage.ResumeLayout(false);
			this.RelatedJobsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
