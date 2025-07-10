using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ReceiveEntryForm
	{
		ZButton FinaliseButton;
		ZTemplateTabControl DetailsTabControl;
		ZTabPage DetailsTabPage;
		ZTabPage ReferencesTabPage;
		ZTabPage ContainersTabPage;
		ZTabPage PalletsTabPage;
		ZTabPage LinesTabPage;
		KPanel PanelTopDetails;
		ZPanel ReceiveTopPanel;
		ZOrganisationControl OrgWhsFindControl;
		ZGuidFindBox WhsGuidFindBox;
		ZWorkflowTabPage WorkflowTabPage;
		ZDocAddressControl SupplierDocAddressControl;
		ZDocAddressControl TransportCoDocAddressControl;
		ZButton TestDataButton;
		ZGroupBox ReceiveDetailsGroupBox;
		ZTextBox VehicleNoTextBox;
		ZTextBox ReferenceNoTextBox;
		ZDateTimeOffsetEdit ArrivalDateEdit;
		ZCalcEdit TotalUnitsCalcEdit;
		ZDateTimeOffsetEdit BookingDateEdit;
		ZDateTimeOffsetEdit ETADateEdit;
		ZCalcEdit TotPalletsCalcEdit;
		ZDateTimeOffsetEdit ETDDateEdit;
		ZGroupBox zGroupBox1;
		ZDropEdit SubTypeDropEdit;
		ZTextBox StatusTextBox;
		ZTextBox ExtRefTextBox;
		ZTextBox CustomerRefTextBox;
		ZGroupBox zGroupBox2;
		ZGroupBox zGroupBox3;
		ZGroupBox zGroupBox4;
		ZGroupBox zGroupBox5;
		ZTabPage AsnLinesTabPage;
		ZCalcEdit ExtRefSplitCalcEdit;
		ZTabPage RelatedSplitsTabPage;
		ZModuleButtonGrid RelatedSplitsGrid;
		ZTabPage CustomFieldsTabPage;
		ProcessTemplateCustomFieldsControl customLabelsUserControl1;
		ZTabPage ServicesTabPage;
		ZPanel ServicesPanel;
		ServicesControl servicesControl1;
		RelatedJobsTabPage RelatedJobsPage;
		ZCalcEdit WD_TotalLineUnitsOnReceiveTabCalcEdit;
		ZCalcEdit TotalPalletsReceivedCalcEdit;
		ReceiveDocketLinesGridUserControl inventoryGridUserControl2;
		ZCalcDropEdit WD_TotalCubicCalcDropEdit;
		ZCalcDropEdit WD_TotalWeightCalcDropEdit;
		ZButtonTransportCoHotlink zButtonTransportCoHotlink1;
		ZCalcDropEdit WD_PackagesSentCalcDropEdit;
		DocketReferenceGridUserControl DocketReferenceGridUserControl;
		DocketContainerGridUserControl docketContainerGridUserControl1;
		DocketPalletGridUserControl docketPalletGridUserControl1;
		ZPanel ASNLinesTabAdditionalDataPanel;
		ZCalcEdit TotalASNLineUnitsCalcEdit;
		ZPanel InventoryGridPanel;
		ReceiveDocketLinesGridUserControl InventoryGridControl;
		ZPanel LinesTabDataPanel;
		ZCalcEdit WD_TotalLineUnitsOnLinesTabCalcEdit;
		ZPanel ASNLinesGridPanel;
		AsnLinesGridUserControl asnLinesGridUserControl;
		ReceiveProductSummaryGridUserControl receiveProductSummaryUserControl;
		ZGroupBox zGroupBox6;
		ZCheckBox HoldPalletIDPutawayCheckbox;
		ZTextBox DGContactzTextBox;
		ZDropEdit DropModeDropEdit;
		ZDropEdit CarrierServiceDropEdit;
		ZLabel AwaitingCustomsResponseLabel;
		ZLabel ReceiveTaskPlanningStatusPromptLabel;
		ZCodeFindBoxWithSelectedEvent ServiceLevelCodeBox;
		ZTabPage ProductSummaryTabPage;
		DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
		ZDropEdit ReceiveCategoryDropEdit;
		ZButton ScreenButton;
		private System.ComponentModel.IContainer components;
		ZPanel ReceiveBottomPanel;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReceiveEntryForm));
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.FinaliseButton = new ZButton();
			this.DetailsTabControl = new ZTemplateTabControl();
			this.DetailsTabPage = new ZTabPage();
			this.ReceiveBottomPanel = new ZPanel();
			this.inventoryGridUserControl2 = new ReceiveDocketLinesGridUserControl();
			this.ReceiveDetailsGroupBox = new ZGroupBox();
			this.zGroupBox4 = new ZGroupBox();
			this.CarrierServiceDropEdit = new ZDropEdit();
			this.zButtonTransportCoHotlink1 = new ZButtonTransportCoHotlink();
			this.DropModeDropEdit = new ZDropEdit();
			this.ReferenceNoTextBox = new ZTextBox();
			this.VehicleNoTextBox = new ZTextBox();
			this.zGroupBox6 = new ZGroupBox();
			this.ScreenButton = new ZButton();
			this.ScreeningStatusDropEdit = new DeniedPartyScreeningStatusDropEdit();
			this.HoldPalletIDPutawayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DGContactzTextBox = new ZTextBox();
			this.zGroupBox5 = new ZGroupBox();
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit = new ZCalcEdit();
			this.TotalPalletsReceivedCalcEdit = new ZCalcEdit();
			this.WD_TotalCubicCalcDropEdit = new ZCalcDropEdit();
			this.WD_TotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.zGroupBox3 = new ZGroupBox();
			this.ETDDateEdit = new ZDateTimeOffsetEdit();
			this.ETADateEdit = new ZDateTimeOffsetEdit();
			this.BookingDateEdit = new ZDateTimeOffsetEdit();
			this.ArrivalDateEdit = new ZDateTimeOffsetEdit();
			this.zGroupBox2 = new ZGroupBox();
			this.TotPalletsCalcEdit = new ZCalcEdit();
			this.WD_PackagesSentCalcDropEdit = new ZCalcDropEdit();
			this.TotalUnitsCalcEdit = new ZCalcEdit();
			this.zGroupBox1 = new ZGroupBox();
			this.ServiceLevelCodeBox = new ZCodeFindBoxWithSelectedEvent();
			this.StatusTextBox = new ZTextBox();
			this.ExtRefTextBox = new ZTextBox();
			this.CustomerRefTextBox = new ZTextBox();
			this.ExtRefSplitCalcEdit = new ZCalcEdit();
			this.ReceiveTopPanel = new ZPanel();
			this.ReceiveCategoryDropEdit = new ZDropEdit();
			this.SubTypeDropEdit = new ZDropEdit();
			this.TransportCoDocAddressControl = new ZDocAddressControl();
			this.WhsGuidFindBox = new ZGuidFindBox();
			this.OrgWhsFindControl = new ZOrganisationControl();
			this.SupplierDocAddressControl = new ZDocAddressControl();
			this.LinesTabPage = new ZTabPage();
			this.InventoryGridPanel = new ZPanel();
			this.InventoryGridControl = new ReceiveDocketLinesGridUserControl();
			this.LinesTabDataPanel = new ZPanel();
			this.WD_TotalLineUnitsOnLinesTabCalcEdit = new ZCalcEdit();
			this.AsnLinesTabPage = new ZTabPage();
			this.ASNLinesGridPanel = new ZPanel();
			this.asnLinesGridUserControl = new AsnLinesGridUserControl();
			this.ASNLinesTabAdditionalDataPanel = new ZPanel();
			this.TotalASNLineUnitsCalcEdit = new ZCalcEdit();
			this.ProductSummaryTabPage = new ZTabPage();
			this.receiveProductSummaryUserControl = new ReceiveProductSummaryGridUserControl();
			this.RelatedSplitsTabPage = new ZTabPage();
			this.RelatedSplitsGrid = new ZModuleButtonGrid();
			this.CustomFieldsTabPage = new ZTabPage();
			this.customLabelsUserControl1 = new ProcessTemplateCustomFieldsControl();
			this.ReferencesTabPage = new ZTabPage();
			this.DocketReferenceGridUserControl = new DocketReferenceGridUserControl();
			this.ContainersTabPage = new ZTabPage();
			this.docketContainerGridUserControl1 = new DocketContainerGridUserControl();
			this.ServicesTabPage = new ZTabPage();
			this.ServicesPanel = new ZPanel();
			this.servicesControl1 = new ServicesControl();
			this.PalletsTabPage = new ZTabPage();
			this.docketPalletGridUserControl1 = new DocketPalletGridUserControl();
			this.TestDataButton = new ZButton();
			this.PanelTopDetails = new KPanel();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.RelatedJobsPage = new RelatedJobsTabPage();
			this.AwaitingCustomsResponseLabel = new ZLabel();
			this.ReceiveTaskPlanningStatusPromptLabel = new ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ReceiveBottomPanel.SuspendLayout();
			this.inventoryGridUserControl2.SuspendLayout();
			this.ReceiveDetailsGroupBox.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.CarrierServiceDropEdit.SuspendLayout();
			this.DropModeDropEdit.SuspendLayout();
			this.zGroupBox6.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.zGroupBox5.SuspendLayout();
			this.WD_TotalCubicCalcDropEdit.SuspendLayout();
			this.WD_TotalWeightCalcDropEdit.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.ETDDateEdit.SuspendLayout();
			this.ETADateEdit.SuspendLayout();
			this.BookingDateEdit.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.WD_PackagesSentCalcDropEdit.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.ServiceLevelCodeBox.SuspendLayout();
			this.ReceiveTopPanel.SuspendLayout();
			this.ReceiveTaskPlanningStatusPromptLabel.SuspendLayout();
			this.ReceiveCategoryDropEdit.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.TransportCoDocAddressControl.SuspendLayout();
			this.WhsGuidFindBox.SuspendLayout();
			this.OrgWhsFindControl.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.InventoryGridPanel.SuspendLayout();
			this.InventoryGridControl.SuspendLayout();
			this.LinesTabDataPanel.SuspendLayout();
			this.AsnLinesTabPage.SuspendLayout();
			this.ASNLinesGridPanel.SuspendLayout();
			this.asnLinesGridUserControl.SuspendLayout();
			this.ASNLinesTabAdditionalDataPanel.SuspendLayout();
			this.ProductSummaryTabPage.SuspendLayout();
			this.receiveProductSummaryUserControl.SuspendLayout();
			this.RelatedSplitsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedSplitsGrid.InnerGrid)).BeginInit();
			this.RelatedSplitsGrid.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.customLabelsUserControl1.SuspendLayout();
			this.ReferencesTabPage.SuspendLayout();
			this.DocketReferenceGridUserControl.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.docketContainerGridUserControl1.SuspendLayout();
			this.ServicesTabPage.SuspendLayout();
			this.ServicesPanel.SuspendLayout();
			this.servicesControl1.SuspendLayout();
			this.PalletsTabPage.SuspendLayout();
			this.docketPalletGridUserControl1.SuspendLayout();
			this.PanelTopDetails.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.RelatedJobsPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.RelatedJobsPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 696, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RelatedJobsPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.PanelTopDetails);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 663, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 598, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 598, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 696, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.TestDataButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.FinaliseButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(997);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsReceive);
			// 
			// FinaliseButton
			// 
			this.FinaliseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FinaliseButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|85ddade9-0678-4669-8b1e-28e5aa589a67", "Finalize");
			this.FinaliseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 5, true);
			this.FinaliseButton.Name = "FinaliseButton";
			this.FinaliseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FinaliseButton.TabIndex = 5;
			this.FinaliseButton.ToolTipCaption = null;
			this.FinaliseButton.Click += new EventHandler(this.FinaliseButton_Click);
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.DetailsTabPage);
			this.DetailsTabControl.Controls.Add(this.LinesTabPage);
			this.DetailsTabControl.Controls.Add(this.AsnLinesTabPage);
			this.DetailsTabControl.Controls.Add(this.ProductSummaryTabPage);
			this.DetailsTabControl.Controls.Add(this.RelatedSplitsTabPage);
			this.DetailsTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.DetailsTabControl.Controls.Add(this.ReferencesTabPage);
			this.DetailsTabControl.Controls.Add(this.ContainersTabPage);
			this.DetailsTabControl.Controls.Add(this.ServicesTabPage);
			this.DetailsTabControl.Controls.Add(this.PalletsTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 663, true);
			this.DetailsTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|11c899bf-483a-4b95-a3e3-cc32d04fa751", "Receive");
			this.DetailsTabPage.Controls.Add(this.ReceiveBottomPanel);
			this.DetailsTabPage.Controls.Add(this.ReceiveTopPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 629, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// ReceiveBottomPanel
			// 
			this.ReceiveBottomPanel.Controls.Add(this.inventoryGridUserControl2);
			this.ReceiveBottomPanel.Controls.Add(this.ReceiveDetailsGroupBox);
			this.ReceiveBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiveBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.ReceiveBottomPanel.Name = "ReceiveBottomPanel";
			this.ReceiveBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 387, true);
			this.ReceiveBottomPanel.TabIndex = 1;
			// 
			// inventoryGridUserControl2
			// 
			this.inventoryGridUserControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.inventoryGridUserControl2, ".");
			this.inventoryGridUserControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.inventoryGridUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
			this.inventoryGridUserControl2.Name = "inventoryGridUserControl2";
			this.inventoryGridUserControl2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.inventoryGridUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 131, true);
			this.inventoryGridUserControl2.TabIndex = 1;
			// 
			// ReceiveDetailsGroupBox
			// 
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox4);
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox6);
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox5);
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox3);
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox2);
			this.ReceiveDetailsGroupBox.Controls.Add(this.zGroupBox1);
			this.ReceiveDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceiveDetailsGroupBox, false);
			this.ReceiveDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiveDetailsGroupBox.Name = "ReceiveDetailsGroupBox";
			this.ReceiveDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 257, true);
			this.ReceiveDetailsGroupBox.TabIndex = 0;
			this.ReceiveDetailsGroupBox.TabStop = false;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.Controls.Add(this.CarrierServiceDropEdit);
			this.zGroupBox4.Controls.Add(this.zButtonTransportCoHotlink1);
			this.zGroupBox4.Controls.Add(this.DropModeDropEdit);
			this.zGroupBox4.Controls.Add(this.ReferenceNoTextBox);
			this.zGroupBox4.Controls.Add(this.VehicleNoTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox4, false);
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 8, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 115, true);
			this.zGroupBox4.TabIndex = 1;
			this.zGroupBox4.TabStop = false;
			// 
			// CarrierServiceDropEdit
			// 
			this.CarrierServiceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceDropEdit, "WD_PL_NKCarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_PL_NKCarrierServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsReceive)(null)))));
			this.CarrierServiceDropEdit.BindToList = ".";
			this.CarrierServiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 62, true);
			this.CarrierServiceDropEdit.Name = "CarrierServiceDropEdit";
			this.CarrierServiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 29, true);
			this.CarrierServiceDropEdit.TabIndex = 3;
			// 
			// zButtonTransportCoHotlink1
			// 
			this.BindingSource.SetBindingMember(this.zButtonTransportCoHotlink1, ".");
			this.zButtonTransportCoHotlink1.BindToTransportCo = "TransportCoDocAddress.Organisation";
			this.zButtonTransportCoHotlink1.BindToTransportRef = "WD_TransportReference";
			this.zButtonTransportCoHotlink1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 14, true);
			this.zButtonTransportCoHotlink1.Name = "zButtonTransportCoHotlink1";
			this.zButtonTransportCoHotlink1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 22, true);
			this.zButtonTransportCoHotlink1.TabIndex = 10;
			this.zButtonTransportCoHotlink1.TabStop = false;
			this.zButtonTransportCoHotlink1.ToolTipCaption = null;
			this.zButtonTransportCoHotlink1.UseVisualStyleBackColor = true;
			this.zButtonTransportCoHotlink1.Click += new EventHandler(this.zButtonTransportCoHotlink1_Click);
			// 
			// DropModeDropEdit
			// 
			this.DropModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "WD_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 86, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 29, true);
			this.DropModeDropEdit.TabIndex = 4;
			// 
			// ReferenceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNoTextBox, "WD_TransportReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_TransportReference)));
			this.ReferenceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 14, true);
			this.ReferenceNoTextBox.Name = "ReferenceNoTextBox";
			this.ReferenceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 29, true);
			this.ReferenceNoTextBox.TabIndex = 1;
			// 
			// VehicleNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleNoTextBox, "VehicleNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).VehicleNo)));
			this.VehicleNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|2aae99ce-4cc9-412c-a8cf-2156a854c4a5", "Vehicle No");
			this.VehicleNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 38, true);
			this.VehicleNoTextBox.Name = "VehicleNoTextBox";
			this.VehicleNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 29, true);
			this.VehicleNoTextBox.TabIndex = 2;
			// 
			// zGroupBox6
			// 
			this.zGroupBox6.Controls.Add(this.ScreenButton);
			this.zGroupBox6.Controls.Add(this.ScreeningStatusDropEdit);
			this.zGroupBox6.Controls.Add(this.HoldPalletIDPutawayCheckbox);
			this.zGroupBox6.Controls.Add(this.DGContactzTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox6, false);
			this.zGroupBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 124, true);
			this.zGroupBox6.Name = "zGroupBox6";
			this.zGroupBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 112, true);
			this.zGroupBox6.TabIndex = 4;
			this.zGroupBox6.TabStop = false;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 62, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 14;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_ScreeningStatus)));
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 62, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 29, true);
			this.ScreeningStatusDropEdit.TabIndex = 13;
			// 
			// DGContactzTextBox
			// 
			this.BindingSource.SetBindingMember(this.DGContactzTextBox, "DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).DGContact)));
			this.DGContactzTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|5d2b1e1f-78d7-4351-859a-18cb459a0f3f", "DG Contact");
			this.DGContactzTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 38, true);
			this.DGContactzTextBox.Name = "DGContactzTextBox";
			this.DGContactzTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 29, true);
			this.DGContactzTextBox.TabIndex = 12;
			// 
			// HoldPalletIDPutawayCheckbox
			//
			this.HoldPalletIDPutawayCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HoldPalletIDPutawayCheckbox, "WD_HoldPalletIDPutaway");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).WD_HoldPalletIDPutaway)));
			this.HoldPalletIDPutawayCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HoldPalletIDPutawayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 14, true);
			this.HoldPalletIDPutawayCheckbox.Name = "HoldPalletIDPutawayCheckbox";
			this.HoldPalletIDPutawayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.HoldPalletIDPutawayCheckbox.TabIndex = 11;
			this.HoldPalletIDPutawayCheckbox.UseVisualStyleBackColor = true;
			// 
			// zGroupBox5
			// 
			this.zGroupBox5.Controls.Add(this.WD_TotalLineUnitsOnReceiveTabCalcEdit);
			this.zGroupBox5.Controls.Add(this.TotalPalletsReceivedCalcEdit);
			this.zGroupBox5.Controls.Add(this.WD_TotalCubicCalcDropEdit);
			this.zGroupBox5.Controls.Add(this.WD_TotalWeightCalcDropEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox5, false);
			this.zGroupBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 124, true);
			this.zGroupBox5.Name = "zGroupBox5";
			this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 112, true);
			this.zGroupBox5.TabIndex = 5;
			this.zGroupBox5.TabStop = false;
			// 
			// WD_TotalLineUnitsOnReceiveTabCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WD_TotalLineUnitsOnReceiveTabCalcEdit, "WD_TotalUnitsFromLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalUnitsFromLines)));
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|752f30ec-fd38-4442-b16a-49f362289d47", "Total Line Units");
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.DecimalPlaces = 2;
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.IsCalculatorEnabled = false;
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 14, true);
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.Name = "WD_TotalLineUnitsOnReceiveTabCalcEdit";
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.TabIndex = 1;
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.TabStop = false;
			this.WD_TotalLineUnitsOnReceiveTabCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPalletsReceivedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPalletsReceivedCalcEdit, "TotalPalletsReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).TotalPalletsReceived)));
			this.TotalPalletsReceivedCalcEdit.DecimalPlaces = 2;
			this.TotalPalletsReceivedCalcEdit.IsCalculatorEnabled = false;
			this.TotalPalletsReceivedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 38, true);
			this.TotalPalletsReceivedCalcEdit.Name = "TotalPalletsReceivedCalcEdit";
			this.TotalPalletsReceivedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.TotalPalletsReceivedCalcEdit.TabIndex = 2;
			this.TotalPalletsReceivedCalcEdit.TabStop = false;
			this.TotalPalletsReceivedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WD_TotalCubicCalcDropEdit
			// 
			this.WD_TotalCubicCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_TotalCubicCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_TotalCubicUnit)));
			this.WD_TotalCubicCalcDropEdit.BindToAmount = "WD_TotalCubic";
			this.WD_TotalCubicCalcDropEdit.BindToUnit = "WD_TotalCubicUnit";
			this.WD_TotalCubicCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|0cf80f36-264a-4836-bb24-2575881bc6ab", "Total Line Volume");
			this.WD_TotalCubicCalcDropEdit.Decimals = 3;
			this.WD_TotalCubicCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 85, true);
			this.WD_TotalCubicCalcDropEdit.Name = "WD_TotalCubicCalcDropEdit";
			this.WD_TotalCubicCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.WD_TotalCubicCalcDropEdit.TabIndex = 4;
			// 
			// WD_TotalWeightCalcDropEdit
			// 
			this.WD_TotalWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_TotalWeightUnit)));
			this.WD_TotalWeightCalcDropEdit.BindToAmount = "WD_TotalWeight";
			this.WD_TotalWeightCalcDropEdit.BindToUnit = "WD_TotalWeightUnit";
			this.WD_TotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|dad9053d-8990-4d24-93b1-d7e51524ef47", "Total Line Weight");
			this.WD_TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 62, true);
			this.WD_TotalWeightCalcDropEdit.Name = "WD_TotalWeightCalcDropEdit";
			this.WD_TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.WD_TotalWeightCalcDropEdit.TabIndex = 3;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.ETDDateEdit);
			this.zGroupBox3.Controls.Add(this.ETADateEdit);
			this.zGroupBox3.Controls.Add(this.BookingDateEdit);
			this.zGroupBox3.Controls.Add(this.ArrivalDateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox3, false);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 8, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 115, true);
			this.zGroupBox3.TabIndex = 2;
			this.zGroupBox3.TabStop = false;
			// 
			// ETDDateEdit
			// 
			this.ETDDateEdit.AllowDrop = true;
			this.ETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ETDDateEdit, "WD_ETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_ETD)));
			this.ETDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 38, true);
			this.ETDDateEdit.Name = "ETDDateEdit";
			this.ETDDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.ETDDateEdit.TabIndex = 3;
			// 
			// ETADateEdit
			// 
			this.ETADateEdit.AllowDrop = true;
			this.ETADateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ETADateEdit, "WD_ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_ETA)));
			this.ETADateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 62, true);
			this.ETADateEdit.Name = "ETADateEdit";
			this.ETADateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.ETADateEdit.TabIndex = 5;
			// 
			// BookingDateEdit
			// 
			this.BookingDateEdit.AllowDrop = true;
			this.BookingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BookingDateEdit, "WD_BookingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_BookingDate)));
			this.BookingDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.BookingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 14, true);
			this.BookingDateEdit.Name = "BookingDateEdit";
			this.BookingDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.BookingDateEdit.TabIndex = 1;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "WD_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_ArrivalDate)));
			this.ArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 86, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 29, true);
			this.ArrivalDateEdit.TabIndex = 7;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.TotPalletsCalcEdit);
			this.zGroupBox2.Controls.Add(this.WD_PackagesSentCalcDropEdit);
			this.zGroupBox2.Controls.Add(this.TotalUnitsCalcEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 124, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 112, true);
			this.zGroupBox2.TabIndex = 3;
			this.zGroupBox2.TabStop = false;
			// 
			// TotPalletsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotPalletsCalcEdit, "WD_TotalPallets");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalPallets)));
			this.TotPalletsCalcEdit.DecimalPlaces = 2;
			this.TotPalletsCalcEdit.IsCalculatorEnabled = false;
			this.TotPalletsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.TotPalletsCalcEdit.Name = "TotPalletsCalcEdit";
			this.TotPalletsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.TotPalletsCalcEdit.TabIndex = 2;
			this.TotPalletsCalcEdit.Text = "0";
			this.TotPalletsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WD_PackagesSentCalcDropEdit
			// 
			this.WD_PackagesSentCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WD_PackagesSentCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_PackagesSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_F3_NKTotalPackType)));
			this.WD_PackagesSentCalcDropEdit.BindToAmount = "WD_PackagesSent";
			this.WD_PackagesSentCalcDropEdit.BindToUnit = "WD_F3_NKTotalPackType";
			this.WD_PackagesSentCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|f32aa95b-04b4-4347-8663-aebd9fd2064e", "Total Packages");
			this.WD_PackagesSentCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 62, true);
			this.WD_PackagesSentCalcDropEdit.Name = "WD_PackagesSentCalcDropEdit";
			this.WD_PackagesSentCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.WD_PackagesSentCalcDropEdit.TabIndex = 3;
			// 
			// TotalUnitsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalUnitsCalcEdit, "WD_TotalUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalUnits)));
			this.TotalUnitsCalcEdit.DecimalPlaces = 2;
			this.TotalUnitsCalcEdit.IsCalculatorEnabled = false;
			this.TotalUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 14, true);
			this.TotalUnitsCalcEdit.Name = "TotalUnitsCalcEdit";
			this.TotalUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.TotalUnitsCalcEdit.TabIndex = 1;
			this.TotalUnitsCalcEdit.Text = "0";
			this.TotalUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalUnitsCalcEdit.WordWrap = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.ServiceLevelCodeBox);
			this.zGroupBox1.Controls.Add(this.StatusTextBox);
			this.zGroupBox1.Controls.Add(this.ExtRefTextBox);
			this.zGroupBox1.Controls.Add(this.CustomerRefTextBox);
			this.zGroupBox1.Controls.Add(this.ExtRefSplitCalcEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 8, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 115, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// ServiceLevelCodeBox
			// 
			this.ServiceLevelCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeBox, "WD_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_RS_NKServiceLevel)));
			this.ServiceLevelCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 62, true);
			this.ServiceLevelCodeBox.Name = "ServiceLevelCodeBox";
			this.ServiceLevelCodeBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ServiceLevelCodeBox.ParentType = null;
			this.ServiceLevelCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.ServiceLevelCodeBox.TabIndex = 10;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WD_DocketStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_DocketStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|5cb71eba-5594-41ac-9ecc-ec24ab5a9b6b", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.StatusTextBox.TabIndex = 7;
			// 
			// ExtRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtRefTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_ExternalReference)));
			this.ExtRefTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|9b7c8ace-0bb8-44c4-b4d0-a3e07df16a01", "Receive Ref");
			this.ExtRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 14, true);
			this.ExtRefTextBox.Name = "ExtRefTextBox";
			this.ExtRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 29, true);
			this.ExtRefTextBox.TabIndex = 3;
			// 
			// CustomerRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerRefTextBox, "WD_CustomerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).WD_CustomerReference)));
			this.CustomerRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.CustomerRefTextBox.Name = "CustomerRefTextBox";
			this.CustomerRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 29, true);
			this.CustomerRefTextBox.TabIndex = 5;
			// 
			// ExtRefSplitCalcEdit
			// 
			this.ExtRefSplitCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExtRefSplitCalcEdit, "WD_ExternalReferenceSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_ExternalReferenceSplit)));
			this.ExtRefSplitCalcEdit.DecimalPlaces = 2;
			this.ExtRefSplitCalcEdit.IsCalculatorEnabled = false;
			this.ExtRefSplitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 14, true);
			this.ExtRefSplitCalcEdit.Name = "ExtRefSplitCalcEdit";
			this.ExtRefSplitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 29, true);
			this.ExtRefSplitCalcEdit.TabIndex = 8;
			this.ExtRefSplitCalcEdit.Text = "0";
			this.ExtRefSplitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ReceiveTopPanel
			// 
			this.ReceiveTopPanel.Controls.Add(this.ReceiveCategoryDropEdit);
			this.ReceiveTopPanel.Controls.Add(this.SubTypeDropEdit);
			this.ReceiveTopPanel.Controls.Add(this.TransportCoDocAddressControl);
			this.ReceiveTopPanel.Controls.Add(this.WhsGuidFindBox);
			this.ReceiveTopPanel.Controls.Add(this.OrgWhsFindControl);
			this.ReceiveTopPanel.Controls.Add(this.SupplierDocAddressControl);
			this.ReceiveTopPanel.Controls.Add(this.AwaitingCustomsResponseLabel);
			this.ReceiveTopPanel.Controls.Add(this.ReceiveTaskPlanningStatusPromptLabel);
			this.ReceiveTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ReceiveTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiveTopPanel.Name = "ReceiveTopPanel";
			this.ReceiveTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 242, true);
			this.ReceiveTopPanel.TabIndex = 0;
			// 
			// ReceiveCategoryDropEdit
			// 
			this.ReceiveCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiveCategoryDropEdit, "WD_ReceiveCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_ReceiveCategory)));
			this.ReceiveCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 199, true);
			this.ReceiveCategoryDropEdit.Name = "ReceiveCategoryDropEdit";
			this.ReceiveCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 29, true);
			this.ReceiveCategoryDropEdit.TabIndex = 5;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "WD_DocketSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsReceive)(null)).WD_DocketSubType)));
			this.SubTypeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|b1b1928e-dcf3-4664-9a68-758ceb85c48b", "Type");
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 199, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 29, true);
			this.SubTypeDropEdit.TabIndex = 2;
			// 
			// TransportCoDocAddressControl
			// 
			this.TransportCoDocAddressControl.AddressValidationProcessCmdKey = null;
			this.TransportCoDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportCoDocAddressControl, "TransportCoDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsReceive)(null)).TransportCoDocAddress)));
			this.TransportCoDocAddressControl.BindToOrganisations = "Lookups+TransportCos";
			this.TransportCoDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|1d3ff8fd-96e1-452c-a1c3-9c04c5ae87d7", "Transport");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TransportCoDocAddressControl, false);
			this.TransportCoDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.TransportCoDocAddressControl.Name = "TransportCoDocAddressControl";
			this.TransportCoDocAddressControl.ReadOnly = false;
			this.TransportCoDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.TransportCoDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.TransportCoDocAddressControl.TabIndex = 3;
			this.TransportCoDocAddressControl.ValidationJustForced = false;
			// 
			// WhsGuidFindBox
			// 
			this.WhsGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsGuidFindBox, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsReceive)(null)).WD_WW_Whs)));
			this.WhsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 167, true);
			this.WhsGuidFindBox.Name = "WhsGuidFindBox";
			this.WhsGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WhsGuidFindBox.ParentType = null;
			this.WhsGuidFindBox.PreBoundMaxLength = 3;
			this.WhsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 29, true);
			this.WhsGuidFindBox.TabIndex = 1;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsReceive)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.Captions = Array.Empty<string>();
			this.OrgWhsFindControl.IsCaptionOverridden = false;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.OrgAddressFormatter = null;
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 151, true);
			this.OrgWhsFindControl.TabIndex = 0;
			// 
			// SupplierDocAddressControl
			// 
			this.SupplierDocAddressControl.AddressValidationProcessCmdKey = null;
			this.SupplierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((WhsReceive)(null)).SupplierDocAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Lookups+Suppliers";
			this.SupplierDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|19894578-1309-48f9-8325-08c40cdbc1d1", "Supplier");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SupplierDocAddressControl, false);
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 3, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.ReadOnly = false;
			this.SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddressControl.TabIndex = 4;
			this.SupplierDocAddressControl.ValidationJustForced = false;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|6c187343-dc6c-48d1-95cb-66f1afc634b8", "Lines");
			this.LinesTabPage.Controls.Add(this.InventoryGridPanel);
			this.LinesTabPage.Controls.Add(this.LinesTabDataPanel);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.LinesTabPage.TabIndex = 4;
			// 
			// InventoryGridPanel
			// 
			this.InventoryGridPanel.Controls.Add(this.InventoryGridControl);
			this.InventoryGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InventoryGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InventoryGridPanel.Name = "InventoryGridPanel";
			this.InventoryGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 531, true);
			this.InventoryGridPanel.TabIndex = 9;
			// 
			// InventoryGridControl
			// 
			this.InventoryGridControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InventoryGridControl, ".");
			this.InventoryGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InventoryGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InventoryGridControl.Name = "InventoryGridControl";
			this.InventoryGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 531, true);
			this.InventoryGridControl.TabIndex = 1;
			// 
			// LinesTabDataPanel
			// 
			this.LinesTabDataPanel.Controls.Add(this.WD_TotalLineUnitsOnLinesTabCalcEdit);
			this.LinesTabDataPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LinesTabDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 531, true);
			this.LinesTabDataPanel.Name = "LinesTabDataPanel";
			this.LinesTabDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 34, true);
			this.LinesTabDataPanel.TabIndex = 8;
			// 
			// WD_TotalLineUnitsOnLinesTabCalcEdit
			// 
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalLineUnitsOnLinesTabCalcEdit, "WD_TotalUnitsFromLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).WD_TotalUnitsFromLines)));
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|98913412-e43d-45c3-90bc-353fc790514a", "Total Line Units");
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.DecimalPlaces = 2;
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 7, true);
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Name = "WD_TotalLineUnitsOnLinesTabCalcEdit";
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 29, true);
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.TabIndex = 19;
			this.WD_TotalLineUnitsOnLinesTabCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AsnLinesTabPage
			// 
			this.AsnLinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|340827d2-352c-458f-b81a-d26c478b75c8", "ASN Lines");
			this.AsnLinesTabPage.Controls.Add(this.ASNLinesGridPanel);
			this.AsnLinesTabPage.Controls.Add(this.ASNLinesTabAdditionalDataPanel);
			this.AsnLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.AsnLinesTabPage.Name = "AsnLinesTabPage";
			this.AsnLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.AsnLinesTabPage.TabIndex = 5;
			this.AsnLinesTabPage.UseVisualStyleBackColor = true;
			// 
			// ASNLinesGridPanel
			// 
			this.ASNLinesGridPanel.Controls.Add(this.asnLinesGridUserControl);
			this.ASNLinesGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ASNLinesGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ASNLinesGridPanel.Name = "ASNLinesGridPanel";
			this.ASNLinesGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 531, true);
			this.ASNLinesGridPanel.TabIndex = 5;
			// 
			// asnLinesGridUserControl
			// 
			this.asnLinesGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.asnLinesGridUserControl, ".");
			this.asnLinesGridUserControl.BindTo = "AsnLines";
			this.asnLinesGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asnLinesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asnLinesGridUserControl.Name = "asnLinesGridUserControl";
			this.asnLinesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 531, true);
			this.asnLinesGridUserControl.TabIndex = 2;
			// 
			// ASNLinesTabAdditionalDataPanel
			// 
			this.ASNLinesTabAdditionalDataPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ASNLinesTabAdditionalDataPanel.Controls.Add(this.TotalASNLineUnitsCalcEdit);
			this.ASNLinesTabAdditionalDataPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ASNLinesTabAdditionalDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 531, true);
			this.ASNLinesTabAdditionalDataPanel.Name = "ASNLinesTabAdditionalDataPanel";
			this.ASNLinesTabAdditionalDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 34, true);
			this.ASNLinesTabAdditionalDataPanel.TabIndex = 4;
			// 
			// TotalASNLineUnitsCalcEdit
			// 
			this.TotalASNLineUnitsCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalASNLineUnitsCalcEdit, "TotalASNLineUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsReceive)(null)).TotalASNLineUnits)));
			this.TotalASNLineUnitsCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|c31772bc-0a1a-4d1d-8206-dd571434f855", "Total ASN Line Units");
			this.TotalASNLineUnitsCalcEdit.DecimalPlaces = 2;
			this.TotalASNLineUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 7, true);
			this.TotalASNLineUnitsCalcEdit.Name = "TotalASNLineUnitsCalcEdit";
			this.TotalASNLineUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 29, true);
			this.TotalASNLineUnitsCalcEdit.TabIndex = 0;
			this.TotalASNLineUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductSummaryTabPage
			// 
			this.ProductSummaryTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("7e8acac4-255c-460c-8cff-41fc1e57f4db", "Product Summary");
			this.ProductSummaryTabPage.Controls.Add(this.receiveProductSummaryUserControl);
			this.ProductSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ProductSummaryTabPage.Name = "ProductSummaryTabPage";
			this.ProductSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.ProductSummaryTabPage.TabIndex = 8;
			this.ProductSummaryTabPage.UseVisualStyleBackColor = true;
			// 
			// receiveProductSummaryUserControl
			// 
			this.receiveProductSummaryUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.receiveProductSummaryUserControl, ".");
			this.receiveProductSummaryUserControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("7353602b-fb77-435f-a12f-a5608ae48782", "Product Summary Grid");
			this.receiveProductSummaryUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.receiveProductSummaryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.receiveProductSummaryUserControl.Name = "receiveProductSummaryUserControl";
			this.receiveProductSummaryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 559, true);
			this.receiveProductSummaryUserControl.TabIndex = 0;
			// 
			// RelatedSplitsTabPage
			// 
			this.RelatedSplitsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|7732c5f6-a8cc-444d-a0ef-3e957b307b73", "Related Splits");
			this.RelatedSplitsTabPage.Controls.Add(this.RelatedSplitsGrid);
			this.RelatedSplitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.RelatedSplitsTabPage.Name = "RelatedSplitsTabPage";
			this.RelatedSplitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.RelatedSplitsTabPage.TabIndex = 6;
			// 
			// RelatedSplitsGrid
			// 
			this.RelatedSplitsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedSplitsGrid, "RelatedSplits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsReceive)(null)).RelatedSplits)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ZModuleButtonGrid|7728e8a2-5920-41c4-8681-525e6ee088df", "Split Type");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SplitRelationshipTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ZModuleButtonGrid|7728e8a2-5920-41c4-8681-525e6ee088de", "Receive Reference");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WD_ExternalReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "WD_ExternalReferenceSplit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedSplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedSplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedSplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RelatedSplitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedSplitsGrid.GridId = "70b41d1c-cea6-44ca-9f64-dca9038bbc97";
			// 
			// 
			// 
			this.RelatedSplitsGrid.InnerGrid.AllowNavigation = false;
			this.RelatedSplitsGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedSplitsGrid.InnerGrid.CaptionVisible = false;
			this.RelatedSplitsGrid.InnerGrid.GridId = "70b41d1c-cea6-44ca-9f64-dca9038bbc97";
			this.RelatedSplitsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedSplitsGrid.InnerGrid.LayoutKey = "Grid";
			this.RelatedSplitsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.RelatedSplitsGrid.InnerGrid.Name = "Grid";
			this.RelatedSplitsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 529, true);
			this.RelatedSplitsGrid.InnerGrid.TabIndex = 0;
			this.RelatedSplitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedSplitsGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.RelatedSplitsGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsReceive;
			this.RelatedSplitsGrid.Name = "RelatedSplitsGrid";
			this.RelatedSplitsGrid.NameOfAGridElement = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|11c899bf-483a-4b95-a3e3-cc32d04fa751", "Receive");
			this.RelatedSplitsGrid.ReadOnly = false;
			this.RelatedSplitsGrid.ShowAttachButton = false;
			this.RelatedSplitsGrid.ShowDetachButton = false;
			this.RelatedSplitsGrid.ShowNewButton = false;
			this.RelatedSplitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.RelatedSplitsGrid.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|44bb75e7-9585-4283-86d3-512833d9af61", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.customLabelsUserControl1);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.CustomFieldsTabPage.TabIndex = 6;
			// 
			// customLabelsUserControl1
			// 
			this.customLabelsUserControl1.AllowDrop = true;
			this.customLabelsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customLabelsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customLabelsUserControl1.Name = "customLabelsUserControl1";
			this.customLabelsUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.customLabelsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.customLabelsUserControl1.TabIndex = 0;
			// 
			// ReferencesTabPage
			// 
			this.ReferencesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|22341603-3831-438a-a2d4-1ba00dffecf9", "References");
			this.ReferencesTabPage.Controls.Add(this.DocketReferenceGridUserControl);
			this.ReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ReferencesTabPage.Name = "ReferencesTabPage";
			this.ReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.ReferencesTabPage.TabIndex = 1;
			// 
			// DocketReferenceGridUserControl
			// 
			this.DocketReferenceGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocketReferenceGridUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsDocket)(((WhsReceive)(null)))));
			this.DocketReferenceGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocketReferenceGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocketReferenceGridUserControl.Name = "DocketReferenceGridUserControl";
			this.DocketReferenceGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.DocketReferenceGridUserControl.TabIndex = 0;
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|6b537d76-f279-4f62-ba6d-2f1035fcace0", "Containers");
			this.ContainersTabPage.Controls.Add(this.docketContainerGridUserControl1);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.ContainersTabPage.TabIndex = 2;
			// 
			// docketContainerGridUserControl1
			// 
			this.docketContainerGridUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.docketContainerGridUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsDocket)(((WhsReceive)(null)))));
			this.docketContainerGridUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.docketContainerGridUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.docketContainerGridUserControl1.Name = "docketContainerGridUserControl1";
			this.docketContainerGridUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.docketContainerGridUserControl1.TabIndex = 0;
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|3b925093-5dc0-41bd-b9ac-cecfdcf32306", "Services");
			this.ServicesTabPage.Controls.Add(this.ServicesPanel);
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.ServicesTabPage.TabIndex = 7;
			this.ServicesTabPage.UseVisualStyleBackColor = true;
			// 
			// ServicesPanel
			// 
			this.ServicesPanel.Controls.Add(this.servicesControl1);
			this.ServicesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServicesPanel.Name = "ServicesPanel";
			this.ServicesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 559, true);
			this.ServicesPanel.TabIndex = 0;
			// 
			// servicesControl1
			// 
			this.servicesControl1.AllowDrop = true;
			this.servicesControl1.BindToServices = "Services";
			this.servicesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.servicesControl1.IsContextVisibleInGrid = false;
			this.servicesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.servicesControl1.Name = "servicesControl1";
			this.servicesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 559, true);
			this.servicesControl1.TabIndex = 0;
			// 
			// PalletsTabPage
			// 
			this.PalletsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|7deee1ea-d13d-45f5-beb5-bce571201deb", "Pallets");
			this.PalletsTabPage.Controls.Add(this.docketPalletGridUserControl1);
			this.PalletsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.PalletsTabPage.Name = "PalletsTabPage";
			this.PalletsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.PalletsTabPage.TabIndex = 3;
			// 
			// docketPalletGridUserControl1
			// 
			this.docketPalletGridUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.docketPalletGridUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsDocket)(((WhsReceive)(null)))));
			this.docketPalletGridUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.docketPalletGridUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.docketPalletGridUserControl1.Name = "docketPalletGridUserControl1";
			this.docketPalletGridUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.docketPalletGridUserControl1.TabIndex = 0;
			// 
			// TestDataButton
			// 
			this.TestDataButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TestDataButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveEntryForm|33a23729-91d8-4d06-8fd2-2c52fd87c43b", "Test Data");
			this.TestDataButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.TestDataButton.Name = "TestDataButton";
			this.TestDataButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.TestDataButton.TabIndex = 0;
			this.TestDataButton.ToolTipCaption = null;
			this.TestDataButton.Click += new EventHandler(this.TestDataButton_Click);
			// 
			// PanelTopDetails
			// 
			this.PanelTopDetails.Controls.Add(this.DetailsTabControl);
			this.PanelTopDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelTopDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PanelTopDetails.Name = "PanelTopDetails";
			this.PanelTopDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 663, true);
			this.PanelTopDetails.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 542, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// RelatedJobsPage
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.RelatedJobCollection)(((WhsReceive)(null)).RelatedJobs)));
			this.RelatedJobsPage.ExcludeFromBindingOnSave = true;
			this.RelatedJobsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.RelatedJobsPage.Name = "RelatedJobsPage";
			this.RelatedJobsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 542, true);
			this.RelatedJobsPage.TabIndex = 4;
			// 
			// AwaitingCustomsResponseLabel
			// 
			this.AwaitingCustomsResponseLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AwaitingCustomsResponseLabel, "AwaitingCustomsResponseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).AwaitingCustomsResponseStatus)));
			this.AwaitingCustomsResponseLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AwaitingCustomsResponseLabel.ForeColor = System.Drawing.Color.Red;
			this.AwaitingCustomsResponseLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AwaitingCustomsResponseLabel, false);
			this.AwaitingCustomsResponseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 221, true);
			this.AwaitingCustomsResponseLabel.Name = "AwaitingCustomsResponseLabel";
			this.AwaitingCustomsResponseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 18, true);
			this.AwaitingCustomsResponseLabel.TabIndex = 1;
			this.AwaitingCustomsResponseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// ReceiveTaskPlanningStatusPromptLabel
			// 
			this.ReceiveTaskPlanningStatusPromptLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ReceiveTaskPlanningStatusPromptLabel, "TaskPlanningStatusPrompt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsReceive)(null)).TaskPlanningStatusPrompt)));
			this.ReceiveTaskPlanningStatusPromptLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ReceiveTaskPlanningStatusPromptLabel.ForeColor = System.Drawing.Color.Red;
			this.ReceiveTaskPlanningStatusPromptLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceiveTaskPlanningStatusPromptLabel, false);
			this.ReceiveTaskPlanningStatusPromptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 199, true);
			this.ReceiveTaskPlanningStatusPromptLabel.Name = "ReceiveTaskPlanningStatusPromptLabel";
			this.ReceiveTaskPlanningStatusPromptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 29, true);
			this.ReceiveTaskPlanningStatusPromptLabel.TabIndex = 1;
			this.ReceiveTaskPlanningStatusPromptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ReceiveEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 752, true);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsReceive);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsReceive";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 725, true);
			this.Name = "ReceiveEntryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Receive Goods Entry";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ReceiveBottomPanel.ResumeLayout(false);
			this.ReceiveBottomPanel.PerformLayout();
			this.inventoryGridUserControl2.ResumeLayout(true);
			this.inventoryGridUserControl2.PerformLayout();
			this.ReceiveDetailsGroupBox.ResumeLayout(false);
			this.ReceiveDetailsGroupBox.PerformLayout();
			this.zGroupBox4.ResumeLayout(false);
			this.zGroupBox4.PerformLayout();
			this.CarrierServiceDropEdit.ResumeLayout(true);
			this.CarrierServiceDropEdit.PerformLayout();
			this.DropModeDropEdit.ResumeLayout(true);
			this.DropModeDropEdit.PerformLayout();
			this.zGroupBox6.ResumeLayout(false);
			this.zGroupBox6.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.zGroupBox5.ResumeLayout(false);
			this.zGroupBox5.PerformLayout();
			this.WD_TotalCubicCalcDropEdit.ResumeLayout(true);
			this.WD_TotalCubicCalcDropEdit.PerformLayout();
			this.WD_TotalWeightCalcDropEdit.ResumeLayout(true);
			this.WD_TotalWeightCalcDropEdit.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.ETDDateEdit.ResumeLayout(true);
			this.ETDDateEdit.PerformLayout();
			this.ETADateEdit.ResumeLayout(true);
			this.ETADateEdit.PerformLayout();
			this.BookingDateEdit.ResumeLayout(true);
			this.BookingDateEdit.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.WD_PackagesSentCalcDropEdit.ResumeLayout(true);
			this.WD_PackagesSentCalcDropEdit.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ServiceLevelCodeBox.ResumeLayout(true);
			this.ServiceLevelCodeBox.PerformLayout();
			this.ReceiveTopPanel.ResumeLayout(false);
			this.ReceiveTopPanel.PerformLayout();
			this.ReceiveTaskPlanningStatusPromptLabel.ResumeLayout(false);
			this.ReceiveTaskPlanningStatusPromptLabel.PerformLayout();
			this.ReceiveCategoryDropEdit.ResumeLayout(true);
			this.ReceiveCategoryDropEdit.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.TransportCoDocAddressControl.ResumeLayout(true);
			this.TransportCoDocAddressControl.PerformLayout();
			this.WhsGuidFindBox.ResumeLayout(true);
			this.WhsGuidFindBox.PerformLayout();
			this.OrgWhsFindControl.ResumeLayout(true);
			this.OrgWhsFindControl.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.InventoryGridPanel.ResumeLayout(false);
			this.InventoryGridPanel.PerformLayout();
			this.InventoryGridControl.ResumeLayout(true);
			this.InventoryGridControl.PerformLayout();
			this.LinesTabDataPanel.ResumeLayout(false);
			this.LinesTabDataPanel.PerformLayout();
			this.AsnLinesTabPage.ResumeLayout(false);
			this.AsnLinesTabPage.PerformLayout();
			this.ASNLinesGridPanel.ResumeLayout(false);
			this.ASNLinesGridPanel.PerformLayout();
			this.asnLinesGridUserControl.ResumeLayout(true);
			this.asnLinesGridUserControl.PerformLayout();
			this.ASNLinesTabAdditionalDataPanel.ResumeLayout(false);
			this.ASNLinesTabAdditionalDataPanel.PerformLayout();
			this.ProductSummaryTabPage.ResumeLayout(false);
			this.ProductSummaryTabPage.PerformLayout();
			this.receiveProductSummaryUserControl.ResumeLayout(true);
			this.receiveProductSummaryUserControl.PerformLayout();
			this.RelatedSplitsTabPage.ResumeLayout(false);
			this.RelatedSplitsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedSplitsGrid.InnerGrid)).EndInit();
			this.RelatedSplitsGrid.ResumeLayout(true);
			this.RelatedSplitsGrid.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.customLabelsUserControl1.ResumeLayout(true);
			this.customLabelsUserControl1.PerformLayout();
			this.ReferencesTabPage.ResumeLayout(false);
			this.ReferencesTabPage.PerformLayout();
			this.DocketReferenceGridUserControl.ResumeLayout(true);
			this.DocketReferenceGridUserControl.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.docketContainerGridUserControl1.ResumeLayout(true);
			this.docketContainerGridUserControl1.PerformLayout();
			this.ServicesTabPage.ResumeLayout(false);
			this.ServicesTabPage.PerformLayout();
			this.ServicesPanel.ResumeLayout(false);
			this.ServicesPanel.PerformLayout();
			this.servicesControl1.ResumeLayout(true);
			this.servicesControl1.PerformLayout();
			this.PalletsTabPage.ResumeLayout(false);
			this.PalletsTabPage.PerformLayout();
			this.docketPalletGridUserControl1.ResumeLayout(true);
			this.docketPalletGridUserControl1.PerformLayout();
			this.PanelTopDetails.ResumeLayout(false);
			this.PanelTopDetails.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.RelatedJobsPage.ResumeLayout(false);
			this.RelatedJobsPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
