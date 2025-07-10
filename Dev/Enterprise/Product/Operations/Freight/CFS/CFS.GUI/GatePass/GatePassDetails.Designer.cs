using System;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class GatePassDetails
	{
		#region Component Designer generated code

		private ZGroupBox ShipmentDetailsGroupBox;
		public ZTextBox JS_GatePassStatusBoundTextBox;
		private ZTextBox LoadListNoTextBox;
		private ZDateEdit ETDDateEdit;
		private ZTextBox PortOfDischargeTextBox;
		private ZTextBox PortOfLoadingTextBox;
		private ZDateEdit ETADateEdit;
		private ZTextBox VesselNameTextBox;
		private ZTextBox JX_VoyageTextBox;
		private ZTextBox DescriptionTextBox;
		private ZTextBox JS_HouseBillTextBox;
		private ZCodeFindBox ShipmentOriginCodeFindBox;
		private ZCodeFindBox ShipmentDestinationCodeFindBox;
		private ZPanel LeftPanel;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZGroupBox ContainersGroupBox;
		private CargoWise.Windows.UI.KSplitter splitter2;
		private ZTemplateTabControl PackLineNotesTabControl;
		private ZTabPage WarehouseTabPage;
		private ZTabPage OutturnNotesTabPage;
		private ZTextBox JL_OutturnCommentTextBox;
		private CargoWise.Windows.UI.KSplitter splitter3;
		private ZGroupBox PackLinesGroupBox;
		private ZGroupBox ShipmentTotalsGroupBox;
		private ZCalcEdit ShipmentTotalSurplusCalcEdit;
		private ZCalcEdit ShipmentTotalShortCalcEdit;
		private ZCalcEdit ShipmentTotalPillagedCalcEdit;
		private ZCalcEdit ShipmentTotalDamagedCalcEdit;
		private ZCalcEdit ShipmentTotalOutturnedCalcEdit;
		private ZCalcEdit ShipmentTotalInStockCalcEdit;
		private ZCalcEdit ShipmentTotalManifestedCalcEdit;
		private ZCheckBox ContingencyReleaseCheckBox;
		public ZCheckBox PrintOnSaveCheckBox;
		private ZTextBox CustomsNumberTextBox;
		private ZGroupBox DeliveryInformationGroupBox;
		private ZPanel zPanel2;
		private ZPanel ShipmentInfoPanel;
		private ZPanel GatePassDetailsPanel;
		private ZTextBox JU_TruckRegistrationTextBox;
		private ZTextBox JU_DriversLicenseTextBox;
		private ZTextBox JU_DriversNameTextBox;
		private ZTextBox JU_TransportCoNameTextBox;
		private ZTabPage ServicesTabPage;
		private ZTextBox ReasonForContingencyReleaseTextBox;
		private ZGroupBox ContingencyReleaseGroupBox;
		private ZTabPage DatesTabPage;
		private ZDateEdit LCLStorageDateEdit;
		private ZDateEdit LCLAvailableDateEdit;
		private ZTemplateTabControl ServicesTabControl;
		protected internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocumentaryDocAddressControl;
		private ZTabPage MarksAndNumbersTabPage;
		private ZTextBox MarksAndNumbersTextBox;
		internal ZButton DetailsButton;
		private ZAddressControl TransportCoAddressControl;
		public ZButton DeliverButton;
		private ZGrid ServicesGrid;
		private ZCalcDropEdit ShipmentVolumeCalcDropEdit;
		private ZCalcDropEdit ShipmentWeightCalcDropEdit;
		private ZGrid PackLinesGrid;
		private ZGrid JobPackLocGrid;
		private ZGrid ContainersGrid;
		public ZGrid DeliveriesGrid;
		private ZTextBox HouseCCNTextBox;
		private System.ComponentModel.IContainer components;

#if DEBUG
		internal ZTextBox JU_DriversLicenseTextBoxInternal => JU_DriversLicenseTextBox;
#endif

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			this.ShipmentDetailsGroupBox = new ZGroupBox();
			this.DetailsButton = new ZButton();
			this.ServicesTabControl = new ZTemplateTabControl();
			this.ServicesTabPage = new ZTabPage();
			this.ServicesGrid = new ZGrid();
			this.DatesTabPage = new ZTabPage();
			this.LCLStorageDateEdit = new ZDateEdit();
			this.LCLAvailableDateEdit = new ZDateEdit();
			this.JS_GatePassStatusBoundTextBox = new ZTextBox();
			this.LoadListNoTextBox = new ZTextBox();
			this.ETDDateEdit = new ZDateEdit();
			this.PortOfDischargeTextBox = new ZTextBox();
			this.PortOfLoadingTextBox = new ZTextBox();
			this.ETADateEdit = new ZDateEdit();
			this.VesselNameTextBox = new ZTextBox();
			this.JX_VoyageTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.HouseCCNTextBox = new ZTextBox();
			this.JS_HouseBillTextBox = new ZTextBox();
			this.ShipmentOriginCodeFindBox = new ZCodeFindBox();
			this.ShipmentVolumeCalcDropEdit = new ZCalcDropEdit();
			this.ShipmentDestinationCodeFindBox = new ZCodeFindBox();
			this.ShipmentWeightCalcDropEdit = new ZCalcDropEdit();
			this.LeftPanel = new ZPanel();
			this.PackLinesGroupBox = new ZGroupBox();
			this.PackLinesGrid = new ZGrid();
			this.splitter3 = new CargoWise.Windows.UI.KSplitter();
			this.PackLineNotesTabControl = new ZTemplateTabControl();
			this.WarehouseTabPage = new ZTabPage();
			this.JobPackLocGrid = new ZGrid();
			this.OutturnNotesTabPage = new ZTabPage();
			this.JL_OutturnCommentTextBox = new ZTextBox();
			this.MarksAndNumbersTabPage = new ZTabPage();
			this.MarksAndNumbersTextBox = new ZTextBox();
			this.splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.ContainersGroupBox = new ZGroupBox();
			this.ContainersGrid = new ZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.ShipmentInfoPanel = new ZPanel();
			this.DeliveryInformationGroupBox = new ZGroupBox();
			this.GatePassDetailsPanel = new ZPanel();
			this.TransportCoAddressControl = new ZAddressControl();
			this.JU_TransportCoNameTextBox = new ZTextBox();
			this.JU_TruckRegistrationTextBox = new ZTextBox();
			this.JU_DriversLicenseTextBox = new ZTextBox();
			this.JU_DriversNameTextBox = new ZTextBox();
			this.DeliveriesGrid = new ZGrid();
			this.zPanel2 = new ZPanel();
			this.DeliverButton = new ZButton();
			this.ContingencyReleaseGroupBox = new ZGroupBox();
			this.ReasonForContingencyReleaseTextBox = new ZTextBox();
			this.ContingencyReleaseCheckBox = new ZCheckBox();
			this.ConsigneeDocumentaryDocAddressControl = new MasterFiles.GUI.ZDocAddressControl();
			this.ShipmentTotalsGroupBox = new ZGroupBox();
			this.ShipmentTotalSurplusCalcEdit = new ZCalcEdit();
			this.ShipmentTotalShortCalcEdit = new ZCalcEdit();
			this.ShipmentTotalPillagedCalcEdit = new ZCalcEdit();
			this.ShipmentTotalDamagedCalcEdit = new ZCalcEdit();
			this.ShipmentTotalOutturnedCalcEdit = new ZCalcEdit();
			this.ShipmentTotalInStockCalcEdit = new ZCalcEdit();
			this.ShipmentTotalManifestedCalcEdit = new ZCalcEdit();
			this.PrintOnSaveCheckBox = new ZCheckBox();
			this.CustomsNumberTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ServicesTabControl.SuspendLayout();
			this.ServicesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.DatesTabPage.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.PackLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.PackLineNotesTabControl.SuspendLayout();
			this.WarehouseTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobPackLocGrid)).BeginInit();
			this.OutturnNotesTabPage.SuspendLayout();
			this.MarksAndNumbersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ShipmentInfoPanel.SuspendLayout();
			this.DeliveryInformationGroupBox.SuspendLayout();
			this.GatePassDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeliveriesGrid)).BeginInit();
			this.zPanel2.SuspendLayout();
			this.ContingencyReleaseGroupBox.SuspendLayout();
			this.ShipmentTotalsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GatePassShipment);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|b6bb9678-d23b-42ba-9078-2543c029910d", "Shipment Details");
			this.ShipmentDetailsGroupBox.Controls.Add(this.DetailsButton);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ServicesTabControl);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JS_GatePassStatusBoundTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.LoadListNoTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ETDDateEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.PortOfDischargeTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.PortOfLoadingTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ETADateEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.VesselNameTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JX_VoyageTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.HouseCCNTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JS_HouseBillTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ShipmentOriginCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ShipmentVolumeCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ShipmentDestinationCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ShipmentWeightCalcDropEdit);
			this.ShipmentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentDetailsGroupBox.Name = "ShipmentDetailsGroupBox";
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 131, true);
			this.ShipmentDetailsGroupBox.TabIndex = 0;
			this.ShipmentDetailsGroupBox.TabStop = false;
			// 
			// DetailsButton
			// 
			this.DetailsButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("C2D1151C-144B-4607-942B-28B3EC4CFE9F", "Details");
			this.DetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 82, true);
			this.DetailsButton.Name = "DetailsButton";
			this.DetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.DetailsButton.TabIndex = 5;
			this.DetailsButton.UseVisualStyleBackColor = true;
			this.DetailsButton.Click += new EventHandler(this.DetailsButton_Click);
			// 
			// ServicesTabControl
			// 
			this.ServicesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ServicesTabControl.Controls.Add(this.ServicesTabPage);
			this.ServicesTabControl.Controls.Add(this.DatesTabPage);
			this.ServicesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(669, 10, true);
			this.ServicesTabControl.Name = "ServicesTabControl";
			this.ServicesTabControl.SelectedIndex = 0;
			this.ServicesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 118, true);
			this.ServicesTabControl.TabIndex = 15;
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|e64e0ce7-d8e5-41ce-9fa7-1e487c2aa20e", "Services");
			this.ServicesTabPage.Controls.Add(this.ServicesGrid);
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 91, true);
			this.ServicesTabPage.TabIndex = 0;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "DocsAndCartage+Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassService)(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((GatePassService)(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((GatePassService)(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassService)(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)).SyncRoot)).ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassService)(((System.Collections.IList)(((GatePassShipment)(null)).DocsAndCartage.Services)).SyncRoot)).ES_ServiceNote)));
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo2.ColumnName = "ES_ServiceNote";
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.CopySelectedRowsAllowed = true;
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGrid.GridId = "64834738-3f15-4fca-b8bd-a660a15da341";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "zGrid1";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 91, true);
			this.ServicesGrid.TabIndex = 0;
			// 
			// DatesTabPage
			// 
			this.DatesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|ffa3007e-c0e0-4d9d-80ce-3539779b87b8", "Dates");
			this.DatesTabPage.Controls.Add(this.LCLStorageDateEdit);
			this.DatesTabPage.Controls.Add(this.LCLAvailableDateEdit);
			this.DatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DatesTabPage.Name = "DatesTabPage";
			this.DatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 91, true);
			this.DatesTabPage.TabIndex = 2;
			// 
			// LCLStorageDateEdit
			// 
			this.LCLStorageDateEdit.AllowDrop = true;
			this.LCLStorageDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLStorageDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLStorageDateEdit, "DocsAndCartage+JP_LCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GatePassShipment)(null)).DocsAndCartage.JP_LCLStorageCommences)));
			this.LCLStorageDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 5, true);
			this.LCLStorageDateEdit.Name = "LCLStorageDateEdit";
			this.LCLStorageDateEdit.TabIndex = 0;
			// 
			// LCLAvailableDateEdit
			// 
			this.LCLAvailableDateEdit.AllowDrop = true;
			this.LCLAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLAvailableDateEdit, "DocsAndCartage+JP_LCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GatePassShipment)(null)).DocsAndCartage.JP_LCLAvailable)));
			this.LCLAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 27, true);
			this.LCLAvailableDateEdit.Name = "LCLAvailableDateEdit";
			this.LCLAvailableDateEdit.TabIndex = 1;
			// 
			// JS_GatePassStatusBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_GatePassStatusBoundTextBox, "JS_GatePassStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_GatePassStatus)));
			this.JS_GatePassStatusBoundTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|476f98b9-331a-4447-a0f2-4d79f5231e48", "Gate Pass Status", "Customs Controlled Status");
			this.JS_GatePassStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 82, true);
			this.JS_GatePassStatusBoundTextBox.Name = "JS_GatePassStatusBoundTextBox";
			this.JS_GatePassStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.JS_GatePassStatusBoundTextBox.TabIndex = 4;
			// 
			// LoadListNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadListNoTextBox, "JS_JK_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_JK_UniqueConsignRef)));
			this.LoadListNoTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|c0cfae8c-35d2-47fc-ab82-5b0fb8f9ea65", "Load List #", "Load List #", "Consolidations load list number.");
			this.LoadListNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 16, true);
			this.LoadListNoTextBox.Name = "LoadListNoTextBox";
			this.LoadListNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.LoadListNoTextBox.TabIndex = 8;
			// 
			// ETDDateEdit
			// 
			this.ETDDateEdit.AllowDrop = true;
			this.ETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETDDateEdit, "JS_Calc_CurrentETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GatePassShipment)(null)).JS_Calc_CurrentETD)));
			this.ETDDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|8e654d19-b9cb-4373-95d5-869f89721c8b", "ETD");
			this.ETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 82, true);
			this.ETDDateEdit.Name = "ETDDateEdit";
			this.ETDDateEdit.TabIndex = 13;
			// 
			// PortOfDischargeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfDischargeTextBox, "JS_Calc_CurrentDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_Calc_CurrentDischargePort)));
			this.PortOfDischargeTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|a4c0442e-95ee-471e-a22b-10147666fa41", "Discharge", "Port of discharge.");
			this.PortOfDischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 104, true);
			this.PortOfDischargeTextBox.Name = "PortOfDischargeTextBox";
			this.PortOfDischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PortOfDischargeTextBox.TabIndex = 12;
			// 
			// PortOfLoadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfLoadingTextBox, "JS_Calc_CurrentLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_Calc_CurrentLoadPort)));
			this.PortOfLoadingTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|f83a1574-46e5-4281-8684-1ffcdda16a8c", "Load", "Port of loading.");
			this.PortOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 82, true);
			this.PortOfLoadingTextBox.Name = "PortOfLoadingTextBox";
			this.PortOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PortOfLoadingTextBox.TabIndex = 11;
			// 
			// ETADateEdit
			// 
			this.ETADateEdit.AllowDrop = true;
			this.ETADateEdit.AutoCompleteMonthThreshold = 1;
			this.ETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETADateEdit, "JS_Calc_CurrentETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GatePassShipment)(null)).JS_Calc_CurrentETA)));
			this.ETADateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|250aded9-bfcf-46c1-9e65-90212c9151b1", "ETA");
			this.ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 104, true);
			this.ETADateEdit.Name = "ETADateEdit";
			this.ETADateEdit.TabIndex = 14;
			// 
			// VesselNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselNameTextBox, "JS_Calc_CurrentVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_Calc_CurrentVessel)));
			this.VesselNameTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|c03b2002-f93c-47e7-9760-85fff8105f99", "Vessel");
			this.VesselNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 38, true);
			this.VesselNameTextBox.Name = "VesselNameTextBox";
			this.VesselNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.VesselNameTextBox.TabIndex = 9;
			// 
			// JX_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JX_VoyageTextBox, "JS_Calc_CurrentVoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_Calc_CurrentVoyageFlight)));
			this.JX_VoyageTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|81424de7-4aec-491c-943d-08504a3a4866", "Voyage/Flight", "Voyage if a Sea job, or Flight if an Air job.");
			this.JX_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 60, true);
			this.JX_VoyageTextBox.Name = "JX_VoyageTextBox";
			this.JX_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JX_VoyageTextBox.TabIndex = 10;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_GoodsDescription)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// HouseCCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseCCNTextBox, "CanadaHouseCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).CanadaHouseCCN)));
			this.HouseCCNTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|46a425ca-d721-4d40-939b-f1a20b7cbb0c", "House CCN");
			this.HouseCCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 16, true);
			this.HouseCCNTextBox.Name = "HouseCCNTextBox";
			this.HouseCCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.HouseCCNTextBox.TabIndex = 0;
			// 
			// JS_HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_HouseBillTextBox, "JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_HouseBill)));
			this.JS_HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.JS_HouseBillTextBox.Name = "JS_HouseBillTextBox";
			this.JS_HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_HouseBillTextBox.TabIndex = 0;
			// 
			// ShipmentOriginCodeFindBox
			// 
			this.ShipmentOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentOriginCodeFindBox, "JS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_RL_NKOrigin)));
			this.ShipmentOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 60, true);
			this.ShipmentOriginCodeFindBox.Name = "ShipmentOriginCodeFindBox";
			this.ShipmentOriginCodeFindBox.ShowDescriptionBox = false;
			this.ShipmentOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ShipmentOriginCodeFindBox.TabIndex = 2;
			// 
			// ShipmentVolumeCalcDropEdit
			// 
			this.ShipmentVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).TotalOuterPacksVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).TotalPackLineVolumeUnit)));
			this.ShipmentVolumeCalcDropEdit.BindToAmount = "TotalOuterPacksVolume";
			this.ShipmentVolumeCalcDropEdit.BindToUnit = "TotalPackLineVolumeUnit";
			this.ShipmentVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|4e794cf2-f336-4cb5-8d4e-d7eb4d81fd2b", "Volume", "Total Outer Packs Volume", "Shipment Volume.");
			this.ShipmentVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 105, true);
			this.ShipmentVolumeCalcDropEdit.Name = "ShipmentVolumeCalcDropEdit";
			this.ShipmentVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentVolumeCalcDropEdit.TabIndex = 7;
			this.ShipmentVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentDestinationCodeFindBox
			// 
			this.ShipmentDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDestinationCodeFindBox, "JS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).JS_RL_NKDestination)));
			this.ShipmentDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 60, true);
			this.ShipmentDestinationCodeFindBox.Name = "ShipmentDestinationCodeFindBox";
			this.ShipmentDestinationCodeFindBox.ShowDescriptionBox = false;
			this.ShipmentDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ShipmentDestinationCodeFindBox.TabIndex = 3;
			// 
			// ShipmentWeightCalcDropEdit
			// 
			this.ShipmentWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).TotalOuterPacksWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).TotalPackLineWeightUnit)));
			this.ShipmentWeightCalcDropEdit.BindToAmount = "TotalOuterPacksWeight";
			this.ShipmentWeightCalcDropEdit.BindToUnit = "TotalPackLineWeightUnit";
			this.ShipmentWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|bea9d74c-36f3-4457-bb3e-70ba427d318d", "Weight", "Total Outer Packs Weight", "Shipment Weight.");
			this.ShipmentWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 104, true);
			this.ShipmentWeightCalcDropEdit.Name = "ShipmentWeightCalcDropEdit";
			this.ShipmentWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentWeightCalcDropEdit.TabIndex = 6;
			this.ShipmentWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.PackLinesGroupBox);
			this.LeftPanel.Controls.Add(this.splitter3);
			this.LeftPanel.Controls.Add(this.PackLineNotesTabControl);
			this.LeftPanel.Controls.Add(this.splitter2);
			this.LeftPanel.Controls.Add(this.ContainersGroupBox);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 411, true);
			this.LeftPanel.TabIndex = 3;
			// 
			// PackLinesGroupBox
			// 
			this.PackLinesGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|1ed241c0-cd03-4ec0-ac5c-8b7f6551cb17", "Pack Lines");
			this.PackLinesGroupBox.Controls.Add(this.PackLinesGrid);
			this.PackLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.PackLinesGroupBox.Name = "PackLinesGroupBox";
			this.PackLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 178, true);
			this.PackLinesGroupBox.TabIndex = 2;
			this.PackLinesGroupBox.TabStop = false;
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "DestinationCFSDepartures.Divots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).J8_PackagesDelivered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.PackagesToDeliver)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_Calc_VolumeToDeliver)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_Calc_WeightToDeliver)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).J8_DeliveryVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).J8_DeliveryWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_ActualWeightUQ)));
			this.PackLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "J8_PackagesDelivered";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|31647562-9711-4fa1-8375-e2e09cd7aa32", "Packages");
			zCalcEditColumnStyleInfo2.ColumnName = "PackLine+PackagesToDeliver";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|419df6e8-d557-4d07-8494-c8cafd9ea308", "Packs");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "PackLine+JL_F3_NKPackType";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|419df6e8-d557-4d07-8494-c8cafd9ea308", "Packs");
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|3451c078-e183-4dc7-b986-1b7a6a4ae94c", "Volume");
			zCalcEditColumnStyleInfo3.ColumnName = "PackLine+JL_Calc_VolumeToDeliver";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|88f8a960-fcea-449d-a754-4b77fb3cb5d9", "Volume");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|45fc2505-7e6c-4f50-b725-74fb079daf3f", "UV");
			zTextBoxColumnStyleInfo4.ColumnName = "PackLine+JL_ActualVolumeUQ";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|88f8a960-fcea-449d-a754-4b77fb3cb5d9", "Volume");
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|243ae73f-e57c-4ef4-a3b1-c0596234ef9b", "Weight");
			zCalcEditColumnStyleInfo4.ColumnName = "PackLine+JL_Calc_WeightToDeliver";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|0c56244b-db0e-435c-9144-55f8c3a4c1ab", "Weight");
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|7248b0b7-dfdb-4b5f-90ec-b7b010631bf7", "UW");
			zTextBoxColumnStyleInfo5.ColumnName = "PackLine+JL_ActualWeightUQ";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|0c56244b-db0e-435c-9144-55f8c3a4c1ab", "Weight");
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|ca71463b-ad08-4872-8d24-56d8617d529b", "Deliv. Volume", "Delivery Volume", "");
			zCalcEditColumnStyleInfo5.ColumnName = "J8_DeliveryVolume";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|83595b8a-9b1b-42ca-8802-d16d69c18cc2", "Delivered Volume");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|a2b227e4-38b9-46f9-a176-2513fa0a4981", "UV");
			zTextBoxColumnStyleInfo6.ColumnName = "PackLine+JL_ActualVolumeUQ";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|83595b8a-9b1b-42ca-8802-d16d69c18cc2", "Delivered Volume");
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|62b69422-5ca6-4d41-bb70-6686d1f0e6be", "Deliv. Weight", "Delivery Weight", "");
			zCalcEditColumnStyleInfo6.ColumnName = "J8_DeliveryWeight";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|10048ede-e442-485b-9d8b-5a63fce0ffbf", "Delivered Weight");
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|fce29b9a-f70e-4cbb-bd51-b292f40bc845", "UW");
			zTextBoxColumnStyleInfo7.ColumnName = "PackLine+JL_ActualWeightUQ";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|10048ede-e442-485b-9d8b-5a63fce0ffbf", "Delivered Weight");
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PackLinesGrid.CopySelectedRowsAllowed = true;
			this.PackLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesGrid.GridId = "9dfaf8d0-e443-4ff5-a17c-22f7b51900ff";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesDivotGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 159, true);
			this.PackLinesGrid.TabIndex = 0;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 3, true);
			this.splitter3.TabIndex = 6;
			this.splitter3.TabStop = false;
			// 
			// PackLineNotesTabControl
			// 
			this.PackLineNotesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackLineNotesTabControl.Controls.Add(this.WarehouseTabPage);
			this.PackLineNotesTabControl.Controls.Add(this.OutturnNotesTabPage);
			this.PackLineNotesTabControl.Controls.Add(this.MarksAndNumbersTabPage);
			this.PackLineNotesTabControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.PackLineNotesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 99, true);
			this.PackLineNotesTabControl.Name = "PackLineNotesTabControl";
			this.PackLineNotesTabControl.SelectedIndex = 0;
			this.PackLineNotesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 131, true);
			this.PackLineNotesTabControl.TabIndex = 1;
			// 
			// WarehouseTabPage
			// 
			this.WarehouseTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|a3d11dc2-afca-4cb9-9118-1b87464cee10", "Warehouse");
			this.WarehouseTabPage.Controls.Add(this.JobPackLocGrid);
			this.WarehouseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WarehouseTabPage.Name = "WarehouseTabPage";
			this.WarehouseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 104, true);
			this.WarehouseTabPage.TabIndex = 2;
			// 
			// JobPackLocGrid
			// 
			this.JobPackLocGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobPackLocGrid, "DestinationCFSDepartures.Divots.PackLine.PackLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.PackLocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Freight.Business.PackLocation)(((System.Collections.IList)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.PackLocations)).SyncRoot)).JQ_NoPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.PackLocation)(((System.Collections.IList)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.PackLocations)).SyncRoot)).JQ_WarehouseLocation)));
			this.JobPackLocGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "JQ_NoPackages";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "JQ_WarehouseLocation";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.JobPackLocGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.JobPackLocGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JobPackLocGrid.CopySelectedRowsAllowed = true;
			this.JobPackLocGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobPackLocGrid.GridId = "ef371029-d891-4199-bf69-50070a6b02a0";
			this.JobPackLocGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobPackLocGrid.LayoutKey = "JobPackLocGrid";
			this.JobPackLocGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobPackLocGrid.Name = "JobPackLocGrid";
			this.JobPackLocGrid.ReadOnly = true;
			this.JobPackLocGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 104, true);
			this.JobPackLocGrid.TabIndex = 0;
			// 
			// OutturnNotesTabPage
			// 
			this.OutturnNotesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|395a3b8b-b36f-4a20-8065-9e00586dfe46", "Outturn Notes");
			this.OutturnNotesTabPage.Controls.Add(this.JL_OutturnCommentTextBox);
			this.OutturnNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OutturnNotesTabPage.Name = "OutturnNotesTabPage";
			this.OutturnNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 104, true);
			this.OutturnNotesTabPage.TabIndex = 0;
			// 
			// JL_OutturnCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.JL_OutturnCommentTextBox, "DestinationCFSDepartures.Divots.PackLine+JL_OutturnComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_OutturnComment)));
			this.JL_OutturnCommentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JL_OutturnCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JL_OutturnCommentTextBox.Multiline = true;
			this.JL_OutturnCommentTextBox.Name = "JL_OutturnCommentTextBox";
			this.JL_OutturnCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JL_OutturnCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 104, true);
			this.JL_OutturnCommentTextBox.TabIndex = 0;
			// 
			// MarksAndNumbersTabPage
			// 
			this.MarksAndNumbersTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|93a71a60-6a51-4ddd-a731-d14e3773ad0a", "Marks & Numbers");
			this.MarksAndNumbersTabPage.Controls.Add(this.MarksAndNumbersTextBox);
			this.MarksAndNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MarksAndNumbersTabPage.Name = "MarksAndNumbersTabPage";
			this.MarksAndNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MarksAndNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 104, true);
			this.MarksAndNumbersTabPage.TabIndex = 3;
			this.MarksAndNumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "DestinationCFSDepartures.Divots.PackLine+JL_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonConfirmDivot)(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).Divots)).SyncRoot)).PackLine.JL_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 98, true);
			this.MarksAndNumbersTextBox.TabIndex = 0;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 3, true);
			this.splitter2.TabIndex = 4;
			this.splitter2.TabStop = false;
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|ccad38f2-9eea-4f14-9a5e-4fa4b9686b4a", "Containers");
			this.ContainersGroupBox.Controls.Add(this.ContainersGrid);
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 96, true);
			this.ContainersGroupBox.TabIndex = 0;
			this.ContainersGroupBox.TabStop = false;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "ContainersForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GatePassShipment)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_LCLUnpack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_LCLAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_LCLStorageCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_TrainWagonNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_UnpackShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonContainer)(((System.Collections.IList)(((GatePassShipment)(null)).ContainersForBinding)).SyncRoot)).JC_ContainerStorageLocation)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "JC_LCLUnpack";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo4.ColumnName = "JC_LCLAvailable";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|1eaa4aa0-bfc5-45fd-8509-1487172da2bc", "Storage", "Storage Commences", "");
			zDateEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo5.ColumnName = "JC_LCLStorageCommences";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo10.ColumnName = "JC_TrainWagonNumber";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "JC_UnpackShed";
			zTextBoxColumnStyleInfo12.ColumnName = "JC_ContainerStorageLocation";
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ContainersGrid.CopySelectedRowsAllowed = true;
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "b2514468-c685-45ea-9ecd-ab0c244670d0";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.ReadOnly = true;
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 77, true);
			this.ContainersGrid.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 131, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 411, true);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// ShipmentInfoPanel
			// 
			this.ShipmentInfoPanel.Controls.Add(this.DeliveryInformationGroupBox);
			this.ShipmentInfoPanel.Controls.Add(this.zPanel2);
			this.ShipmentInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 131, true);
			this.ShipmentInfoPanel.Name = "ShipmentInfoPanel";
			this.ShipmentInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 411, true);
			this.ShipmentInfoPanel.TabIndex = 5;
			// 
			// DeliveryInformationGroupBox
			// 
			this.DeliveryInformationGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|b96ab9ec-405a-475a-9fdd-b71c7a307a95", "Delivery Information");
			this.DeliveryInformationGroupBox.Controls.Add(this.GatePassDetailsPanel);
			this.DeliveryInformationGroupBox.Controls.Add(this.DeliveriesGrid);
			this.DeliveryInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.DeliveryInformationGroupBox.Name = "DeliveryInformationGroupBox";
			this.DeliveryInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 179, true);
			this.DeliveryInformationGroupBox.TabIndex = 1;
			this.DeliveryInformationGroupBox.TabStop = false;
			// 
			// GatePassDetailsPanel
			// 
			this.GatePassDetailsPanel.Controls.Add(this.TransportCoAddressControl);
			this.GatePassDetailsPanel.Controls.Add(this.JU_TransportCoNameTextBox);
			this.GatePassDetailsPanel.Controls.Add(this.JU_TruckRegistrationTextBox);
			this.GatePassDetailsPanel.Controls.Add(this.JU_DriversLicenseTextBox);
			this.GatePassDetailsPanel.Controls.Add(this.JU_DriversNameTextBox);
			this.GatePassDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GatePassDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 101, true);
			this.GatePassDetailsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.GatePassDetailsPanel.Name = "GatePassDetailsPanel";
			this.GatePassDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 75, true);
			this.GatePassDetailsPanel.TabIndex = 1;
			// 
			// TransportCoAddressControl
			// 
			this.TransportCoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportCoAddressControl, "DestinationCFSDepartures.EU_OA_TransportProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_OA_TransportProvider)));
			this.TransportCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 5, true);
			this.TransportCoAddressControl.Name = "TransportCoAddressControl";
			this.TransportCoAddressControl.PopupCaption = "";
			this.TransportCoAddressControl.ShowAddress = false;
			this.TransportCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 42, true);
			this.TransportCoAddressControl.StackControls = true;
			this.TransportCoAddressControl.TabIndex = 0;
			// 
			// JU_TransportCoNameTextBox
			// 
			this.JU_TransportCoNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JU_TransportCoNameTextBox, "DestinationCFSDepartures.EU_TransportCoName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_TransportCoName)));
			this.JU_TransportCoNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 49, true);
			this.JU_TransportCoNameTextBox.Name = "JU_TransportCoNameTextBox";
			this.JU_TransportCoNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.JU_TransportCoNameTextBox.TabIndex = 1;
			// 
			// JU_TruckRegistrationTextBox
			// 
			this.JU_TruckRegistrationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JU_TruckRegistrationTextBox, "DestinationCFSDepartures.EU_VehicleRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_VehicleRegistration)));
			this.JU_TruckRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 27, true);
			this.JU_TruckRegistrationTextBox.Name = "JU_TruckRegistrationTextBox";
			this.JU_TruckRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JU_TruckRegistrationTextBox.TabIndex = 3;
			// 
			// JU_DriversLicenseTextBox
			// 
			this.JU_DriversLicenseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JU_DriversLicenseTextBox, "DestinationCFSDepartures.EU_DriversLicence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_DriversLicence)));
			this.JU_DriversLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 49, true);
			this.JU_DriversLicenseTextBox.Name = "JU_DriversLicenseTextBox";
			this.JU_DriversLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.JU_DriversLicenseTextBox.TabIndex = 4;
			// 
			// JU_DriversNameTextBox
			// 
			this.JU_DriversNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JU_DriversNameTextBox, "DestinationCFSDepartures.EU_DriversName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_DriversName)));
			this.JU_DriversNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 5, true);
			this.JU_DriversNameTextBox.Name = "JU_DriversNameTextBox";
			this.JU_DriversNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.JU_DriversNameTextBox.TabIndex = 2;
			// 
			// DeliveriesGrid
			// 
			this.DeliveriesGrid.AllowNavigation = false;
			this.DeliveriesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeliveriesGrid, "DestinationCFSDepartures");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_PickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_DriversName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_DriversLicence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_TransportCoName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_OA_TransportProvider_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).BindToLists.TransportProviders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).EU_VehicleRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(((System.Collections.IList)(((GatePassShipment)(null)).DestinationCFSDepartures)).SyncRoot)).FullGatePass)));
			this.DeliveriesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|8a39211c-5363-41aa-9eee-0889ca54dcb1", "Dispatched At", "Departure Time", "");
			zDateEditColumnStyleInfo6.ColumnName = "EU_PickupDeliveryTime";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo13.ColumnName = "EU_DriversName";
			zTextBoxColumnStyleInfo14.ColumnName = "EU_DriversLicence";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo15.ColumnName = "EU_TransportCoName";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "BindToLists+TransportProviders";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|730e74ba-a8cf-4ad9-a3dd-1a1c215e649b", "Transport Provider");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "EU_OA_TransportProvider_ZAddress+OrgPK";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo16.ColumnName = "EU_VehicleRegistration";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|3a19943e-1ee4-4846-99bd-33e78d7c680d", "Gate Pass ID");
			zTextBoxColumnStyleInfo17.ColumnName = "FullGatePass";
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			this.DeliveriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.DeliveriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.DeliveriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.DeliveriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.DeliveriesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DeliveriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.DeliveriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.DeliveriesGrid.CopySelectedRowsAllowed = true;
			this.DeliveriesGrid.GridId = "fbf48368-51cd-487e-bab2-f574a86f4fb4";
			this.DeliveriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveriesGrid.LayoutKey = "DeliveriesGrid";
			this.DeliveriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			this.DeliveriesGrid.Name = "DeliveriesGrid";
			this.DeliveriesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DeliveriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 84, true);
			this.DeliveriesGrid.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.DeliverButton);
			this.zPanel2.Controls.Add(this.ContingencyReleaseGroupBox);
			this.zPanel2.Controls.Add(this.ConsigneeDocumentaryDocAddressControl);
			this.zPanel2.Controls.Add(this.ShipmentTotalsGroupBox);
			this.zPanel2.Controls.Add(this.PrintOnSaveCheckBox);
			this.zPanel2.Controls.Add(this.CustomsNumberTextBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 232, true);
			this.zPanel2.TabIndex = 0;
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|ac0d4746-8f7c-47ad-adbc-71312d74324d", "Deliver");
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 207, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeliverButton.TabIndex = 5;
			this.DeliverButton.UseVisualStyleBackColor = true;
			// 
			// ContingencyReleaseGroupBox
			// 
			this.ContingencyReleaseGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|1e11b701-479d-41e4-9c07-9253dfb70635", "Contingency Release");
			this.ContingencyReleaseGroupBox.Controls.Add(this.ReasonForContingencyReleaseTextBox);
			this.ContingencyReleaseGroupBox.Controls.Add(this.ContingencyReleaseCheckBox);
			this.ContingencyReleaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 116, true);
			this.ContingencyReleaseGroupBox.Name = "ContingencyReleaseGroupBox";
			this.ContingencyReleaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 116, true);
			this.ContingencyReleaseGroupBox.TabIndex = 2;
			this.ContingencyReleaseGroupBox.TabStop = false;
			// 
			// ReasonForContingencyReleaseTextBox
			// 
			this.ReasonForContingencyReleaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonForContingencyReleaseTextBox, "DocsAndCartage+ReasonForContingencyRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).DocsAndCartage.ReasonForContingencyRelease)));
			this.ReasonForContingencyReleaseTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|4376b3ce-21e3-4606-9dbf-5e37d83fa34c", "Reason for Contingency Release");
			this.LabelCaptionRenderProvider.SetLabelTop(this.ReasonForContingencyReleaseTextBox, 0);
			this.ReasonForContingencyReleaseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 40, true);
			this.ReasonForContingencyReleaseTextBox.Multiline = true;
			this.ReasonForContingencyReleaseTextBox.Name = "ReasonForContingencyReleaseTextBox";
			this.ReasonForContingencyReleaseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 73, true);
			this.ReasonForContingencyReleaseTextBox.TabIndex = 1;
			// 
			// ContingencyReleaseCheckBox
			// 
			this.ContingencyReleaseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ContingencyReleaseCheckBox, "DocsAndCartage+JP_IsContingencyRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((GatePassShipment)(null)).DocsAndCartage.JP_IsContingencyRelease)));
			this.ContingencyReleaseCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|3efb7941-0f1d-4c8d-b2a9-030189d209b1", "Contingency Release");
			this.ContingencyReleaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ContingencyReleaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.ContingencyReleaseCheckBox.Name = "ContingencyReleaseCheckBox";
			this.ContingencyReleaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.ContingencyReleaseCheckBox.TabIndex = 0;
			// 
			// ConsigneeDocumentaryDocAddressControl
			// 
			this.ConsigneeDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocumentaryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((GatePassShipment)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignee_List";
			this.ConsigneeDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|030bd527-c1da-457e-92cb-69570707e054", "Consignee");
			this.ConsigneeDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 0, true);
			this.ConsigneeDocumentaryDocAddressControl.Name = "ConsigneeDocumentaryDocAddressControl";
			this.ConsigneeDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeDocumentaryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocumentaryDocAddressControl.TabIndex = 1;
			// 
			// ShipmentTotalsGroupBox
			// 
			this.ShipmentTotalsGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|a1026fd0-b36e-4578-a4cc-268a588b0edb", "Shipment Totals");
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalSurplusCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalShortCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalPillagedCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalDamagedCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalOutturnedCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalInStockCalcEdit);
			this.ShipmentTotalsGroupBox.Controls.Add(this.ShipmentTotalManifestedCalcEdit);
			this.ShipmentTotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.ShipmentTotalsGroupBox.Name = "ShipmentTotalsGroupBox";
			this.ShipmentTotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 111, true);
			this.ShipmentTotalsGroupBox.TabIndex = 0;
			this.ShipmentTotalsGroupBox.TabStop = false;
			// 
			// ShipmentTotalSurplusCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalSurplusCalcEdit, "JS_Calc_Surplus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_Surplus)));
			this.ShipmentTotalSurplusCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|9df10435-8403-4745-8b0d-a4386fe9eabe", "Surplus", "Number of pieces not on manifest");
			this.ShipmentTotalSurplusCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalSurplusCalcEdit.Decimals = 0;
			this.ShipmentTotalSurplusCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 85, true);
			this.ShipmentTotalSurplusCalcEdit.Name = "ShipmentTotalSurplusCalcEdit";
			this.ShipmentTotalSurplusCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalSurplusCalcEdit.TabIndex = 6;
			this.ShipmentTotalSurplusCalcEdit.Text = "0";
			this.ShipmentTotalSurplusCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalShortCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalShortCalcEdit, "JS_Calc_Shortlanded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_Shortlanded)));
			this.ShipmentTotalShortCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|0c3d76b7-f249-4ad7-8647-03524aee079b", "Shortlanded", "Number of pieces missing from manifest");
			this.ShipmentTotalShortCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalShortCalcEdit.Decimals = 0;
			this.ShipmentTotalShortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 85, true);
			this.ShipmentTotalShortCalcEdit.Name = "ShipmentTotalShortCalcEdit";
			this.ShipmentTotalShortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalShortCalcEdit.TabIndex = 3;
			this.ShipmentTotalShortCalcEdit.Text = "0";
			this.ShipmentTotalShortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalPillagedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalPillagedCalcEdit, "JS_Calc_TotalPillaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_TotalPillaged)));
			this.ShipmentTotalPillagedCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|c4cddcd4-b8bc-4294-bbd1-d264d0709e78", "Total Pillaged", "Total pieces outturned pillaged");
			this.ShipmentTotalPillagedCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalPillagedCalcEdit.Decimals = 0;
			this.ShipmentTotalPillagedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 62, true);
			this.ShipmentTotalPillagedCalcEdit.Name = "ShipmentTotalPillagedCalcEdit";
			this.ShipmentTotalPillagedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalPillagedCalcEdit.TabIndex = 5;
			this.ShipmentTotalPillagedCalcEdit.Text = "0";
			this.ShipmentTotalPillagedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalDamagedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalDamagedCalcEdit, "JS_Calc_TotalDamaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_TotalDamaged)));
			this.ShipmentTotalDamagedCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|63a09995-d741-4f04-a36d-3daea496db1e", "Total Damaged", "Total pieces outturned damaged");
			this.ShipmentTotalDamagedCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalDamagedCalcEdit.Decimals = 0;
			this.ShipmentTotalDamagedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 62, true);
			this.ShipmentTotalDamagedCalcEdit.Name = "ShipmentTotalDamagedCalcEdit";
			this.ShipmentTotalDamagedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalDamagedCalcEdit.TabIndex = 2;
			this.ShipmentTotalDamagedCalcEdit.Text = "0";
			this.ShipmentTotalDamagedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalOutturnedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalOutturnedCalcEdit, "JS_Calc_TotalOutturned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_TotalOutturned)));
			this.ShipmentTotalOutturnedCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|4baa7c53-b8c7-44f9-ae61-74982cc49463", "Total Outturned", "Total pieces outturned from container");
			this.ShipmentTotalOutturnedCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalOutturnedCalcEdit.Decimals = 0;
			this.ShipmentTotalOutturnedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 16, true);
			this.ShipmentTotalOutturnedCalcEdit.Name = "ShipmentTotalOutturnedCalcEdit";
			this.ShipmentTotalOutturnedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalOutturnedCalcEdit.TabIndex = 4;
			this.ShipmentTotalOutturnedCalcEdit.Text = "0";
			this.ShipmentTotalOutturnedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalInStockCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalInStockCalcEdit, "JS_Calc_TotalInStock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_TotalInStock)));
			this.ShipmentTotalInStockCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|14d3ab30-3573-414b-bbae-98e049509b39", "Total In Stock", "Total pieces currently in stock");
			this.ShipmentTotalInStockCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalInStockCalcEdit.Decimals = 0;
			this.ShipmentTotalInStockCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 39, true);
			this.ShipmentTotalInStockCalcEdit.Name = "ShipmentTotalInStockCalcEdit";
			this.ShipmentTotalInStockCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalInStockCalcEdit.TabIndex = 1;
			this.ShipmentTotalInStockCalcEdit.Text = "0";
			this.ShipmentTotalInStockCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalManifestedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalManifestedCalcEdit, "JS_Calc_TotalManifested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((GatePassShipment)(null)).JS_Calc_TotalManifested)));
			this.ShipmentTotalManifestedCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|b5c368e5-5a83-48e9-9ff0-3f2112a1d4cb", "Total Manifested", "Total pieces manifested");
			this.ShipmentTotalManifestedCalcEdit.DecimalPlaces = 0;
			this.ShipmentTotalManifestedCalcEdit.Decimals = 0;
			this.ShipmentTotalManifestedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 16, true);
			this.ShipmentTotalManifestedCalcEdit.Name = "ShipmentTotalManifestedCalcEdit";
			this.ShipmentTotalManifestedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ShipmentTotalManifestedCalcEdit.TabIndex = 0;
			this.ShipmentTotalManifestedCalcEdit.Text = "0";
			this.ShipmentTotalManifestedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PrintOnSaveCheckBox
			// 
			this.PrintOnSaveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintOnSaveCheckBox, "JS_PrintNewDeliveriesOnSave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((GatePassShipment)(null)).JS_PrintNewDeliveriesOnSave)));
			this.PrintOnSaveCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|369691f1-b2ab-43b7-a002-51c8c3fe0c97", "Print on Save");
			this.PrintOnSaveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOnSaveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 207, true);
			this.PrintOnSaveCheckBox.Name = "PrintOnSaveCheckBox";
			this.PrintOnSaveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.PrintOnSaveCheckBox.TabIndex = 4;
			// 
			// CustomsNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsNumberTextBox, "CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GatePassShipment)(null)).CustomsEntryNumber)));
			this.CustomsNumberTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("GatePassDetails|028fe2a3-05eb-49a0-a3f3-990e6039ab14", "Customs Entry Number", "Customs release number");
			this.CustomsNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 184, true);
			this.CustomsNumberTextBox.Name = "CustomsNumberTextBox";
			this.CustomsNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.CustomsNumberTextBox.TabIndex = 3;
			// 
			// GatePassDetails
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentInfoPanel);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.LeftPanel);
			this.Controls.Add(this.ShipmentDetailsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 542, true);
			this.Name = "GatePassDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 542, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ServicesTabControl.ResumeLayout(false);
			this.ServicesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.DatesTabPage.ResumeLayout(false);
			this.LeftPanel.ResumeLayout(false);
			this.PackLinesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.PackLineNotesTabControl.ResumeLayout(false);
			this.WarehouseTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.JobPackLocGrid)).EndInit();
			this.OutturnNotesTabPage.ResumeLayout(false);
			this.OutturnNotesTabPage.PerformLayout();
			this.MarksAndNumbersTabPage.ResumeLayout(false);
			this.MarksAndNumbersTabPage.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ShipmentInfoPanel.ResumeLayout(false);
			this.DeliveryInformationGroupBox.ResumeLayout(false);
			this.GatePassDetailsPanel.ResumeLayout(false);
			this.GatePassDetailsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeliveriesGrid)).EndInit();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ContingencyReleaseGroupBox.ResumeLayout(false);
			this.ContingencyReleaseGroupBox.PerformLayout();
			this.ShipmentTotalsGroupBox.ResumeLayout(false);
			this.ShipmentTotalsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}