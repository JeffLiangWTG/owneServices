using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.GUI
{
	partial class RoutingPluginControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				UnhookConsol();

				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo19 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo20 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo21 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo22 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo23 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo24 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo routeNumberColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo aircraftTypeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo serviceStringColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo globalFlightScheduleStatusColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo totalCO2eColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo arrivalPortRouteIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo departurePortRouteIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalModeColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JW_LegNotesBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LegDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JW_IsCargoOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_ParentDescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_IsLinkedBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_IsCharterBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_LegOrderBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JW_StatusBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JW_TransportTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JW_TransportModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JW_AdditionalTransportModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginDestinationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DestinationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JW_TerminalAvailabilityDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DestinationJW_ETABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_TerminalStorageDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JW_DepotStorageDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_DepotAvailabilityDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DestinationJW_ATABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VoyageFlightDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierPKFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ServiceStringBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_JX_JV_RegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VesselBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VoyageFlightBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_AircraftTypeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VesselBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JW_CarrierBookingReferenceBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_OA_CarrierAddressZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JW_OA_CreditorAddressZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.OriginDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginJW_ATDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_DepotReceivalCommencesBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_DepotCutOffBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginJW_OA_DepartureLocationBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JW_TerminalCutOffBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_TerminalReceivalCommencesBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_DocumentaryCutOffBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_VGMCutOffBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginJW_ETDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceivalAvailabilityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReceivalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceivalJW_ETABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ReceivalJW_ATABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.ZAddressControl1 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JW_OA_CreditorAddressZAddressControl2 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ServiceLevelDropEditControl = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AvailabilityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AvailabilityJW_ATDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.AvailabilityJW_ETDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Legend = new Enterprise.ZArchitecture.GUI.ColourLegend();
			this.legendPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShowMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GlobalSchedulesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectScheduleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportGlobalScheduleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomTabControl = new ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new ZArchitecture.GUI.ZTabPage();
			this.ActualTabPage = new ZArchitecture.GUI.ZTabPage();
			this.BookingConfirmationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BookingConfirmationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BookingConfirmationUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ActualEventsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ActualEventsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ActualEventsUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TotalPiecesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalWeightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalVolumeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveredTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActualPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.NotesGroupBox.SuspendLayout();
			this.LegDetailsGroupBox.SuspendLayout();
			this.JW_StatusBoundDropEdit.SuspendLayout();
			this.JW_TransportTypeBoundDropEdit.SuspendLayout();
			this.JW_TransportModeBoundDropEdit.SuspendLayout();
			this.JW_AdditionalTransportModeBoundDropEdit.SuspendLayout();
			this.OriginDestinationPanel.SuspendLayout();
			this.DestinationDetailsGroupBox.SuspendLayout();
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.SuspendLayout();
			this.JW_TerminalAvailabilityDateBoundDateEdit.SuspendLayout();
			this.DestinationJW_ETABoundDateEdit.SuspendLayout();
			this.JW_TerminalStorageDateBoundDateEdit.SuspendLayout();
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.SuspendLayout();
			this.JW_DepotStorageDateBoundDateEdit.SuspendLayout();
			this.JW_DepotAvailabilityDateBoundDateEdit.SuspendLayout();
			this.DestinationJW_ATABoundDateEdit.SuspendLayout();
			this.VoyageFlightDetailsGroupBox.SuspendLayout();
			this.CarrierPKFindBox.SuspendLayout();
			this.ServiceStringBoundTextBox.SuspendLayout();
			this.JW_VesselBoundCodeFindBox.SuspendLayout();
			this.JW_OA_CarrierAddressZAddressControl.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.JW_OA_CreditorAddressZAddressControl.SuspendLayout();
			this.OriginDetailsGroupBox.SuspendLayout();
			this.OriginJW_ATDBoundDateEdit.SuspendLayout();
			this.JW_DepotReceivalCommencesBoundDateEdit.SuspendLayout();
			this.JW_DepotCutOffBoundDateEdit.SuspendLayout();
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.SuspendLayout();
			this.OriginJW_OA_DepartureLocationBoundAddressControl.SuspendLayout();
			this.JW_TerminalCutOffBoundDateEdit.SuspendLayout();
			this.JW_TerminalReceivalCommencesBoundDateEdit.SuspendLayout();
			this.JW_DocumentaryCutOffBoundDateEdit.SuspendLayout();
			this.JW_VGMCutOffBoundDateEdit.SuspendLayout();
			this.OriginJW_ETDBoundDateEdit.SuspendLayout();
			this.ReceivalAvailabilityPanel.SuspendLayout();
			this.ReceivalGroupBox.SuspendLayout();
			this.ReceivalJW_ETABoundDateEdit.SuspendLayout();
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.SuspendLayout();
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.SuspendLayout();
			this.ReceivalJW_ATABoundDateEdit.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.ZAddressControl1.SuspendLayout();
			this.JW_OA_CreditorAddressZAddressControl2.SuspendLayout();
			this.ServiceLevelDropEditControl.SuspendLayout();
			this.AvailabilityGroupBox.SuspendLayout();
			this.AvailabilityJW_ATDBoundDateEdit.SuspendLayout();
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.SuspendLayout();
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.SuspendLayout();
			this.AvailabilityJW_ETDBoundDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).BeginInit();
			this.TransportsGrid.SuspendLayout();
			this.legendPanel.SuspendLayout();
			this.BookingConfirmationGroupBox.SuspendLayout();
			this.ActualEventsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BookingConfirmationGrid)).BeginInit();
			this.BookingConfirmationGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ActualEventsGrid)).BeginInit();
			this.ActualEventsGrid.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ActualPanel.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ActualTabPage.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.ITransportCollection);
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.NotesGroupBox);
			this.BottomPanel.Controls.Add(this.LegDetailsGroupBox);
			this.BottomPanel.Controls.Add(this.OriginDestinationPanel);
			this.BottomPanel.Controls.Add(this.ReceivalAvailabilityPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 298, true);
			this.BottomPanel.TabIndex = 2;
			//
			// NotesGroupBox
			//
			this.NotesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|2680cdb1-6445-440d-91b3-efe1e74f0f47", "Notes");
			this.NotesGroupBox.Controls.Add(this.JW_LegNotesBoundTextBox);
			this.NotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 2, true);
			this.NotesGroupBox.Name = "NotesGroupBox";
			this.NotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 60, true);
			this.NotesGroupBox.TabIndex = 1;
			this.NotesGroupBox.TabStop = false;
			//
			// JW_LegNotesBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_LegNotesBoundTextBox, "JW_LegNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_LegNotes)));
			this.JW_LegNotesBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JW_LegNotesBoundTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JW_LegNotesBoundTextBox, false);
			this.JW_LegNotesBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.JW_LegNotesBoundTextBox.Multiline = true;
			this.JW_LegNotesBoundTextBox.Name = "JW_LegNotesBoundTextBox";
			this.JW_LegNotesBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JW_LegNotesBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 43, true);
			this.JW_LegNotesBoundTextBox.TabIndex = 0;
			//
			// LegDetailsGroupBox
			//
			this.LegDetailsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|01789068-9b7e-4331-bb3a-cbdd38a487ac", "Leg Details");
			this.LegDetailsGroupBox.Controls.Add(this.JW_IsCargoOnlyCheckBox);
			this.LegDetailsGroupBox.Controls.Add(this.JW_ParentDescriptionBoundTextBox);
			this.LegDetailsGroupBox.Controls.Add(this.JW_IsLinkedBoundCheckBox);
			this.LegDetailsGroupBox.Controls.Add(this.JW_IsCharterBoundCheckBox);
			this.LegDetailsGroupBox.Controls.Add(this.JW_LegOrderBoundCalcEdit);
			this.LegDetailsGroupBox.Controls.Add(this.JW_StatusBoundDropEdit);
			this.LegDetailsGroupBox.Controls.Add(this.JW_TransportTypeBoundDropEdit);
			this.LegDetailsGroupBox.Controls.Add(this.JW_TransportModeBoundDropEdit);
			this.LegDetailsGroupBox.Controls.Add(this.JW_AdditionalTransportModeBoundDropEdit);
			this.LegDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 2, true);
			this.LegDetailsGroupBox.Name = "LegDetailsGroupBox";
			this.LegDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 60, true);
			this.LegDetailsGroupBox.TabIndex = 0;
			this.LegDetailsGroupBox.TabStop = false;
			//
			// JW_IsCargoOnlyCheckBox
			//
			this.BindingSource.SetBindingMember(this.JW_IsCargoOnlyCheckBox, "JW_IsCargoOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsCargoOnly)));
			this.JW_IsCargoOnlyCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("09c119b2-63d5-4810-957d-15225a67b70d", "Is Cargo Only");
			this.JW_IsCargoOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JW_IsCargoOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 37, true);
			this.JW_IsCargoOnlyCheckBox.Name = "JW_IsCargoOnlyCheckBox";
			this.JW_IsCargoOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 16, true);
			this.JW_IsCargoOnlyCheckBox.TabIndex = 11;
			this.JW_IsCargoOnlyCheckBox.UseVisualStyleBackColor = true;
			//
			// JW_ParentDescriptionBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_ParentDescriptionBoundTextBox, "JW_ParentDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_ParentDescription)));
			this.JW_ParentDescriptionBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|ae08ea9a-2471-494f-a24a-cb365834757f", "Defined", "Defined By", "The job on which this route is defined.");
			this.JW_ParentDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 12, true);
			this.JW_ParentDescriptionBoundTextBox.Name = "JW_ParentDescriptionBoundTextBox";
			this.JW_ParentDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JW_ParentDescriptionBoundTextBox.TabIndex = 1;
			//
			// JW_IsLinkedBoundCheckBox
			//
			this.BindingSource.SetBindingMember(this.JW_IsLinkedBoundCheckBox, "JW_IsLinked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsLinked)));
			this.JW_IsLinkedBoundCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|1b5fe766-c49a-4716-a47f-8e59c6e8951b", "Is Linked");
			this.JW_IsLinkedBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JW_IsLinkedBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 37, true);
			this.JW_IsLinkedBoundCheckBox.Name = "JW_IsLinkedBoundCheckBox";
			this.JW_IsLinkedBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 14, true);
			this.JW_IsLinkedBoundCheckBox.TabIndex = 13;
			//
			// JW_IsCharterBoundCheckBox
			//
			this.JW_IsCharterBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JW_IsCharterBoundCheckBox, "JW_IsCharter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsCharter)));
			this.JW_IsCharterBoundCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|744c8512-53e9-4125-a5b3-aa9af5c6e199", "Charter Route", "Indicates if this route is chartered. If this route is from a published sailing schedule, this should be set to off.");
			this.JW_IsCharterBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JW_IsCharterBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 37, true);
			this.JW_IsCharterBoundCheckBox.Name = "JW_IsCharterBoundCheckBox";
			this.JW_IsCharterBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.JW_IsCharterBoundCheckBox.TabIndex = 12;
			//
			// JW_LegOrderBoundCalcEdit
			//
			this.JW_LegOrderBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JW_LegOrderBoundCalcEdit, "JW_LegOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.Transport)(null)).JW_LegOrder)));
			this.JW_LegOrderBoundCalcEdit.DecimalPlaces = 0;
			this.JW_LegOrderBoundCalcEdit.Decimals = 0;
			this.JW_LegOrderBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 34, true);
			this.JW_LegOrderBoundCalcEdit.Name = "JW_LegOrderBoundCalcEdit";
			this.JW_LegOrderBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.JW_LegOrderBoundCalcEdit.TabIndex = 9;
			this.JW_LegOrderBoundCalcEdit.Text = "0";
			this.JW_LegOrderBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JW_StatusBoundDropEdit
			//
			this.JW_StatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_StatusBoundDropEdit, "JW_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_Status)));
			this.JW_StatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 12, true);
			this.JW_StatusBoundDropEdit.Name = "JW_StatusBoundDropEdit";
			this.JW_StatusBoundDropEdit.PreBoundMaxLength = 3;
			this.JW_StatusBoundDropEdit.ShowDescriptionBox = false;
			this.JW_StatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.JW_StatusBoundDropEdit.TabIndex = 7;
			//
			// JW_TransportTypeBoundDropEdit
			//
			this.JW_TransportTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_TransportTypeBoundDropEdit, "JW_TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TransportType)));
			this.JW_TransportTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 12, true);
			this.JW_TransportTypeBoundDropEdit.Name = "JW_TransportTypeBoundDropEdit";
			this.JW_TransportTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.JW_TransportTypeBoundDropEdit.ShowDescriptionBox = false;
			this.JW_TransportTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.JW_TransportTypeBoundDropEdit.TabIndex = 5;
			//
			// JW_TransportModeBoundDropEdit
			//
			this.JW_TransportModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_TransportModeBoundDropEdit, "JW_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TransportMode)));
			this.JW_TransportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 12, true);
			this.JW_TransportModeBoundDropEdit.Name = "JW_TransportModeBoundDropEdit";
			this.JW_TransportModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JW_TransportModeBoundDropEdit.ShowDescriptionBox = false;
			this.JW_TransportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.JW_TransportModeBoundDropEdit.TabIndex = 3;
			//
			// JW_AdditionalTransportModeBoundDropEdit
			//
			this.JW_AdditionalTransportModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_AdditionalTransportModeBoundDropEdit, "JW_AdditionalTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_AdditionalTransportMode)));
			this.JW_AdditionalTransportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 37, true);
			this.JW_AdditionalTransportModeBoundDropEdit.Name = "JW_AdditionalTransportModeBoundDropEdit";
			this.JW_AdditionalTransportModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JW_AdditionalTransportModeBoundDropEdit.ShowDescriptionBox = false;
			this.JW_AdditionalTransportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.JW_AdditionalTransportModeBoundDropEdit.TabIndex = 10;
			this.JW_AdditionalTransportModeBoundDropEdit.Visible = false;
			//
			// OriginDestinationPanel
			//
			this.OriginDestinationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OriginDestinationPanel.Controls.Add(this.DestinationDetailsGroupBox);
			this.OriginDestinationPanel.Controls.Add(this.VoyageFlightDetailsGroupBox);
			this.OriginDestinationPanel.Controls.Add(this.OriginDetailsGroupBox);
			this.OriginDestinationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.OriginDestinationPanel.Name = "OriginDestinationPanel";
			this.OriginDestinationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 234, true);
			this.OriginDestinationPanel.TabIndex = 1;
			this.OriginDestinationPanel.Visible = false;
			//
			// DestinationDetailsGroupBox
			//
			this.DestinationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DestinationDetailsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|820c1df1-005b-4d04-a710-cdc4404dbd3f", "Destination Details");
			this.DestinationDetailsGroupBox.Controls.Add(this.DestinationJW_RL_NKDiscPortBoundCodeFindBox);
			this.DestinationDetailsGroupBox.Controls.Add(this.JW_TerminalAvailabilityDateBoundDateEdit);
			this.DestinationDetailsGroupBox.Controls.Add(this.DestinationJW_ETABoundDateEdit);
			this.DestinationDetailsGroupBox.Controls.Add(this.JW_TerminalStorageDateBoundDateEdit);
			this.DestinationDetailsGroupBox.Controls.Add(this.DestinationJW_OA_ArrivalLocationBoundAddressControl);
			this.DestinationDetailsGroupBox.Controls.Add(this.JW_DepotStorageDateBoundDateEdit);
			this.DestinationDetailsGroupBox.Controls.Add(this.JW_DepotAvailabilityDateBoundDateEdit);
			this.DestinationDetailsGroupBox.Controls.Add(this.DestinationJW_ATABoundDateEdit);
			this.DestinationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 76, true);
			this.DestinationDetailsGroupBox.Name = "DestinationDetailsGroupBox";
			this.DestinationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 152, true);
			this.DestinationDetailsGroupBox.TabIndex = 4;
			this.DestinationDetailsGroupBox.TabStop = false;
			//
			// DestinationJW_RL_NKDiscPortBoundCodeFindBox
			//
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationJW_RL_NKDiscPortBoundCodeFindBox, "JW_RL_NKDiscPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKDiscPortForBinding)));
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 20, true);
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.Name = "DestinationJW_RL_NKDiscPortBoundCodeFindBox";
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 18, true);
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.TabIndex = 1;
			//
			// JW_TerminalAvailabilityDateBoundDateEdit
			//
			this.JW_TerminalAvailabilityDateBoundDateEdit.AllowDrop = true;
			this.JW_TerminalAvailabilityDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_TerminalAvailabilityDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_TerminalAvailabilityDateBoundDateEdit, "JW_TerminalAvailabilityDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalAvailabilityDateForBinding)));
			this.JW_TerminalAvailabilityDateBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_TerminalAvailabilityDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 89, true);
			this.JW_TerminalAvailabilityDateBoundDateEdit.Name = "JW_TerminalAvailabilityDateBoundDateEdit";
			this.JW_TerminalAvailabilityDateBoundDateEdit.TabIndex = 9;
			//
			// DestinationJW_ETABoundDateEdit
			//
			this.DestinationJW_ETABoundDateEdit.AllowDrop = true;
			this.DestinationJW_ETABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.DestinationJW_ETABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DestinationJW_ETABoundDateEdit, "JW_ETAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ETAForBinding)));
			this.DestinationJW_ETABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DestinationJW_ETABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 67, true);
			this.DestinationJW_ETABoundDateEdit.Name = "DestinationJW_ETABoundDateEdit";
			this.DestinationJW_ETABoundDateEdit.TabIndex = 5;
			//
			// JW_TerminalStorageDateBoundDateEdit
			//
			this.JW_TerminalStorageDateBoundDateEdit.AllowDrop = true;
			this.JW_TerminalStorageDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_TerminalStorageDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_TerminalStorageDateBoundDateEdit, "JW_TerminalStorageDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalStorageDateForBinding)));
			this.JW_TerminalStorageDateBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_TerminalStorageDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 111, true);
			this.JW_TerminalStorageDateBoundDateEdit.Name = "JW_TerminalStorageDateBoundDateEdit";
			this.JW_TerminalStorageDateBoundDateEdit.TabIndex = 13;
			//
			// DestinationJW_OA_ArrivalLocationBoundAddressControl
			//
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationJW_OA_ArrivalLocationBoundAddressControl, "JW_OA_ArrivalLocationForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_ArrivalLocationForBinding)));
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.BindToOrgList = "ArrivalAddressOrgs";
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 44, true);
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.Name = "DestinationJW_OA_ArrivalLocationBoundAddressControl";
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.PopupCaption = "";
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.ShowAddress = false;
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.TabIndex = 3;
			//
			// JW_DepotStorageDateBoundDateEdit
			//
			this.JW_DepotStorageDateBoundDateEdit.AllowDrop = true;
			this.JW_DepotStorageDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_DepotStorageDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_DepotStorageDateBoundDateEdit, "JW_DepotStorageDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotStorageDateForBinding)));
			this.JW_DepotStorageDateBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_DepotStorageDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 111, true);
			this.JW_DepotStorageDateBoundDateEdit.Name = "JW_DepotStorageDateBoundDateEdit";
			this.JW_DepotStorageDateBoundDateEdit.TabIndex = 15;
			//
			// JW_DepotAvailabilityDateBoundDateEdit
			//
			this.JW_DepotAvailabilityDateBoundDateEdit.AllowDrop = true;
			this.JW_DepotAvailabilityDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_DepotAvailabilityDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_DepotAvailabilityDateBoundDateEdit, "JW_DepotAvailabilityDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotAvailabilityDateForBinding)));
			this.JW_DepotAvailabilityDateBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_DepotAvailabilityDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 89, true);
			this.JW_DepotAvailabilityDateBoundDateEdit.Name = "JW_DepotAvailabilityDateBoundDateEdit";
			this.JW_DepotAvailabilityDateBoundDateEdit.TabIndex = 11;
			//
			// DestinationJW_ATABoundDateEdit
			//
			this.DestinationJW_ATABoundDateEdit.AllowDrop = true;
			this.DestinationJW_ATABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.DestinationJW_ATABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DestinationJW_ATABoundDateEdit, "JW_ATAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ATAForBinding)));
			this.DestinationJW_ATABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DestinationJW_ATABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 67, true);
			this.DestinationJW_ATABoundDateEdit.Name = "DestinationJW_ATABoundDateEdit";
			this.DestinationJW_ATABoundDateEdit.TabIndex = 7;
			//
			// VoyageFlightDetailsGroupBox
			//
			this.VoyageFlightDetailsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|d4757a43-1aae-4974-8d57-7cfe6c9000a2", "Voyage / Flight Details");
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.CarrierPKFindBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.IsPublishedCheckBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_AircraftTypeBoundTextBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_JX_JV_RegistrationNoTextBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_VesselBoundTextBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_VoyageFlightBoundTextBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_VesselBoundCodeFindBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_CarrierBookingReferenceBoundTextBox);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_OA_CarrierAddressZAddressControl);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.ServiceLevelDropEdit);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.JW_OA_CreditorAddressZAddressControl);
			this.VoyageFlightDetailsGroupBox.Controls.Add(this.ServiceStringBoundTextBox);
			this.VoyageFlightDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.VoyageFlightDetailsGroupBox.Name = "VoyageFlightDetailsGroupBox";
			this.VoyageFlightDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 67, true);
			this.VoyageFlightDetailsGroupBox.TabIndex = 0;
			this.VoyageFlightDetailsGroupBox.TabStop = false;
			//
			// CarrierPKFindBox
			//
			this.CarrierPKFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierPKFindBox, "CarrierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).CarrierPK)));
			this.CarrierPKFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.CarrierPKFindBox.Name = "CarrierPKFindBox";
			this.CarrierPKFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.CarrierPKFindBox.TabIndex = 6;
			this.CarrierPKFindBox.Visible = false;
			//
			// ServiceStringBoundTextBox
			//
			this.ServiceStringBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceStringBoundTextBox, "JW_ServiceStringForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_ServiceStringForBinding)));
			this.ServiceStringBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 38, true);
			this.ServiceStringBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|67dcb0b7-ff37-a984-4e3a-a91a45cae1d5", "Service String");
			this.ServiceStringBoundTextBox.Name = "ServiceStringTextBox";
			this.ServiceStringBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 18, true);
			this.ServiceStringBoundTextBox.TabIndex = 6;
			this.ServiceStringBoundTextBox.Visible = false;
			//
			// IsPublishedCheckBox
			//
			this.IsPublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPublishedCheckBox, "JW_JX_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_IsPublished)));
			this.IsPublishedCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|8d884f39-c05f-4540-ab2f-6a21c7a16a15", "Published", "Published", "Published", "");
			this.IsPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 14, true);
			this.IsPublishedCheckBox.Name = "IsPublishedCheckBox";
			this.IsPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 14, true);
			this.IsPublishedCheckBox.TabIndex = 5;
			//
			// JW_AircraftTypeBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_AircraftTypeBoundTextBox, "JW_AircraftTypeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_AircraftTypeForBinding)));
			this.JW_AircraftTypeBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|4b636c4f-aa2c-43db-92b0-9a8dec691443", "Aircraft Type", "Aircraft Type", "The aircraft type for a flight.");
			this.JW_AircraftTypeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 14, true);
			this.JW_AircraftTypeBoundTextBox.Name = "JW_AircraftTypeBoundTextBox";
			this.JW_AircraftTypeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 18, true);
			this.JW_AircraftTypeBoundTextBox.TabIndex = 1;
			this.JW_AircraftTypeBoundTextBox.Visible = false;
			//
			// JW_JX_JV_RegistrationNoTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_JX_JV_RegistrationNoTextBox, "JW_JX_JV_RegistrationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_JV_RegistrationNo)));
			this.JW_JX_JV_RegistrationNoTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|5d690cd0-5bdb-49e9-8c5a-61eaefaee577", "Aircraft Reg.", "Aircraft Registration Number", "The aircraft registration number for a chartered flight.");
			this.JW_JX_JV_RegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 14, true);
			this.JW_JX_JV_RegistrationNoTextBox.Name = "JW_JX_JV_RegistrationNoTextBox";
			this.JW_JX_JV_RegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 18, true);
			this.JW_JX_JV_RegistrationNoTextBox.TabIndex = 2;
			this.JW_JX_JV_RegistrationNoTextBox.Visible = false;
			//
			// JW_VesselBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_VesselBoundTextBox, "JW_VesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VesselForBinding)));
			this.JW_VesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 14, true);
			this.JW_VesselBoundTextBox.Name = "JW_VesselBoundTextBox";
			this.JW_VesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 18, true);
			this.JW_VesselBoundTextBox.TabIndex = 3;
			this.JW_VesselBoundTextBox.Visible = false;
			//
			// JW_VoyageFlightBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_VoyageFlightBoundTextBox, "JW_VoyageFlightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VoyageFlightForBinding)));
			this.JW_VoyageFlightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 14, true);
			this.JW_VoyageFlightBoundTextBox.Name = "JW_VoyageFlightBoundTextBox";
			this.JW_VoyageFlightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.JW_VoyageFlightBoundTextBox.TabIndex = 0;
			this.JW_VoyageFlightBoundTextBox.Visible = false;
			//
			// JW_VesselBoundCodeFindBox
			//
			this.JW_VesselBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_VesselBoundCodeFindBox, "JW_VesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VesselForBinding)));
			this.JW_VesselBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 14, true);
			this.JW_VesselBoundCodeFindBox.Name = "JW_VesselBoundCodeFindBox";
			this.JW_VesselBoundCodeFindBox.PreBoundMaxLength = 35;
			this.JW_VesselBoundCodeFindBox.ShowDescriptionBox = false;
			this.JW_VesselBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.JW_VesselBoundCodeFindBox.TabIndex = 2;
			this.JW_VesselBoundCodeFindBox.Visible = false;
			//
			// JW_CarrierBookingReferenceBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JW_CarrierBookingReferenceBoundTextBox, "JW_CarrierBookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_CarrierBookingReference)));
			this.JW_CarrierBookingReferenceBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 38, true);
			this.JW_CarrierBookingReferenceBoundTextBox.Name = "JW_CarrierBookingReferenceBoundTextBox";
			this.JW_CarrierBookingReferenceBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.JW_CarrierBookingReferenceBoundTextBox.TabIndex = 7;
			//
			// JW_OA_CarrierAddressZAddressControl
			//
			this.JW_OA_CarrierAddressZAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_OA_CarrierAddressZAddressControl, "JW_OA_CarrierAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_CarrierAddress)));
			this.JW_OA_CarrierAddressZAddressControl.BindToOrgList = "Lookups.Carriers";
			this.JW_OA_CarrierAddressZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.JW_OA_CarrierAddressZAddressControl.Name = "JW_OA_CarrierAddressZAddressControl";
			this.JW_OA_CarrierAddressZAddressControl.PopupCaption = "";
			this.JW_OA_CarrierAddressZAddressControl.ShowAddress = false;
			this.JW_OA_CarrierAddressZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 18, true);
			this.JW_OA_CarrierAddressZAddressControl.TabIndex = 6;
			//
			// ServiceLevelDropEdit
			//
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "JW_PL_NKCarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_PL_NKCarrierServiceLevel)));
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(831, 39, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.ServiceLevelDropEdit.TabIndex = 8;
			//
			// JW_OA_CreditorAddressZAddressControl
			//
			this.JW_OA_CreditorAddressZAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_OA_CreditorAddressZAddressControl, "JW_OA_CreditorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_CreditorAddress)));
			this.JW_OA_CreditorAddressZAddressControl.BindToOrgList = "Lookups.CreditorList";
			this.JW_OA_CreditorAddressZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 14, true);
			this.JW_OA_CreditorAddressZAddressControl.Name = "JW_OA_CreditorAddressZAddressControl";
			this.JW_OA_CreditorAddressZAddressControl.PopupCaption = "";
			this.JW_OA_CreditorAddressZAddressControl.ShowAddress = false;
			this.JW_OA_CreditorAddressZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.JW_OA_CreditorAddressZAddressControl.TabIndex = 4;
			//
			// OriginDetailsGroupBox
			//
			this.OriginDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OriginDetailsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|9b935fd7-4122-4ab7-b6b7-faf038fab0c4", "Origin Details");
			this.OriginDetailsGroupBox.Controls.Add(this.OriginJW_ATDBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_DepotReceivalCommencesBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_DepotCutOffBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.OriginJW_RL_NKLoadPortBoundCodeFindBox);
			this.OriginDetailsGroupBox.Controls.Add(this.OriginJW_OA_DepartureLocationBoundAddressControl);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_TerminalCutOffBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_TerminalReceivalCommencesBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_DocumentaryCutOffBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.JW_VGMCutOffBoundDateEdit);
			this.OriginDetailsGroupBox.Controls.Add(this.OriginJW_ETDBoundDateEdit);
			this.OriginDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 76, true);
			this.OriginDetailsGroupBox.Name = "OriginDetailsGroupBox";
			this.OriginDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 152, true);
			this.OriginDetailsGroupBox.TabIndex = 3;
			this.OriginDetailsGroupBox.TabStop = false;
			//
			// OriginJW_ATDBoundDateEdit
			//
			this.OriginJW_ATDBoundDateEdit.AllowDrop = true;
			this.OriginJW_ATDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.OriginJW_ATDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OriginJW_ATDBoundDateEdit, "JW_ATDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ATDForBinding)));
			this.OriginJW_ATDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.OriginJW_ATDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 62, true);
			this.OriginJW_ATDBoundDateEdit.Name = "OriginJW_ATDBoundDateEdit";
			this.OriginJW_ATDBoundDateEdit.TabIndex = 7;
			//
			// JW_DepotReceivalCommencesBoundDateEdit
			//
			this.JW_DepotReceivalCommencesBoundDateEdit.AllowDrop = true;
			this.JW_DepotReceivalCommencesBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_DepotReceivalCommencesBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_DepotReceivalCommencesBoundDateEdit, "JW_DepotReceivalCommencesForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotReceivalCommences)));
			this.JW_DepotReceivalCommencesBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_DepotReceivalCommencesBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 84, true);
			this.JW_DepotReceivalCommencesBoundDateEdit.Name = "JW_DepotReceivalCommencesBoundDateEdit";
			this.JW_DepotReceivalCommencesBoundDateEdit.TabIndex = 11;
			//
			// JW_DepotCutOffBoundDateEdit
			//
			this.JW_DepotCutOffBoundDateEdit.AllowDrop = true;
			this.JW_DepotCutOffBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_DepotCutOffBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_DepotCutOffBoundDateEdit, "JW_DepotCutOffForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotCutOffForBinding)));
			this.JW_DepotCutOffBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_DepotCutOffBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 106, true);
			this.JW_DepotCutOffBoundDateEdit.Name = "JW_DepotCutOffBoundDateEdit";
			this.JW_DepotCutOffBoundDateEdit.TabIndex = 15;
			//
			// OriginJW_RL_NKLoadPortBoundCodeFindBox
			//
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginJW_RL_NKLoadPortBoundCodeFindBox, "JW_RL_NKLoadPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKLoadPortForBinding)));
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 18, true);
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.Name = "OriginJW_RL_NKLoadPortBoundCodeFindBox";
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 18, true);
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.TabIndex = 1;
			//
			// OriginJW_OA_DepartureLocationBoundAddressControl
			//
			this.OriginJW_OA_DepartureLocationBoundAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginJW_OA_DepartureLocationBoundAddressControl, "JW_OA_DepartureLocationForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_DepartureLocationForBinding)));
			this.OriginJW_OA_DepartureLocationBoundAddressControl.BindToOrgList = "DepartureAddressOrgs";
			this.OriginJW_OA_DepartureLocationBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 40, true);
			this.OriginJW_OA_DepartureLocationBoundAddressControl.Name = "OriginJW_OA_DepartureLocationBoundAddressControl";
			this.OriginJW_OA_DepartureLocationBoundAddressControl.PopupCaption = "";
			this.OriginJW_OA_DepartureLocationBoundAddressControl.ShowAddress = false;
			this.OriginJW_OA_DepartureLocationBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.OriginJW_OA_DepartureLocationBoundAddressControl.TabIndex = 3;
			//
			// JW_TerminalCutOffBoundDateEdit
			//
			this.JW_TerminalCutOffBoundDateEdit.AllowDrop = true;
			this.JW_TerminalCutOffBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_TerminalCutOffBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_TerminalCutOffBoundDateEdit, "JW_TerminalCutOffForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalCutOffForBinding)));
			this.JW_TerminalCutOffBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_TerminalCutOffBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 106, true);
			this.JW_TerminalCutOffBoundDateEdit.Name = "JW_TerminalCutOffBoundDateEdit";
			this.JW_TerminalCutOffBoundDateEdit.TabIndex = 13;
			//
			// JW_TerminalReceivalCommencesBoundDateEdit
			//
			this.JW_TerminalReceivalCommencesBoundDateEdit.AllowDrop = true;
			this.JW_TerminalReceivalCommencesBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_TerminalReceivalCommencesBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_TerminalReceivalCommencesBoundDateEdit, "JW_TerminalReceivalCommencesForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalReceivalCommencesForBinding)));
			this.JW_TerminalReceivalCommencesBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_TerminalReceivalCommencesBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 84, true);
			this.JW_TerminalReceivalCommencesBoundDateEdit.Name = "JW_TerminalReceivalCommencesBoundDateEdit";
			this.JW_TerminalReceivalCommencesBoundDateEdit.TabIndex = 9;
			//
			// JW_DocumentaryCutOffBoundDateEdit
			//
			this.JW_DocumentaryCutOffBoundDateEdit.AllowDrop = true;
			this.JW_DocumentaryCutOffBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_DocumentaryCutOffBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_DocumentaryCutOffBoundDateEdit, "JW_DocumentaryCutOffForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_DocumentaryCutOffForBinding)));
			this.JW_DocumentaryCutOffBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_DocumentaryCutOffBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 128, true);
			this.JW_DocumentaryCutOffBoundDateEdit.Name = "JW_DocumentaryCutOffBoundDateEdit";
			this.JW_DocumentaryCutOffBoundDateEdit.TabIndex = 17;
			//
			// JW_VGMCutOffBoundDateEdit
			//
			this.JW_VGMCutOffBoundDateEdit.AllowDrop = true;
			this.JW_VGMCutOffBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_VGMCutOffBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_VGMCutOffBoundDateEdit, "JW_VGMCutOffForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_VGMCutOffForBinding)));
			this.JW_VGMCutOffBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_VGMCutOffBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 128, true);
			this.JW_VGMCutOffBoundDateEdit.Name = "JW_VGMCutOffBoundDateEdit";
			this.JW_VGMCutOffBoundDateEdit.TabIndex = 18;
			//
			// OriginJW_ETDBoundDateEdit
			//
			this.OriginJW_ETDBoundDateEdit.AllowDrop = true;
			this.OriginJW_ETDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.OriginJW_ETDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OriginJW_ETDBoundDateEdit, "JW_ETDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ETDForBinding)));
			this.OriginJW_ETDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.OriginJW_ETDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 62, true);
			this.OriginJW_ETDBoundDateEdit.Name = "OriginJW_ETDBoundDateEdit";
			this.OriginJW_ETDBoundDateEdit.TabIndex = 5;
			//
			// ReceivalAvailabilityPanel
			//
			this.ReceivalAvailabilityPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ReceivalAvailabilityPanel.Controls.Add(this.ReceivalGroupBox);
			this.ReceivalAvailabilityPanel.Controls.Add(this.zGroupBox1);
			this.ReceivalAvailabilityPanel.Controls.Add(this.AvailabilityGroupBox);
			this.ReceivalAvailabilityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.ReceivalAvailabilityPanel.Name = "ReceivalAvailabilityPanel";
			this.ReceivalAvailabilityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 234, true);
			this.ReceivalAvailabilityPanel.TabIndex = 5;
			this.ReceivalAvailabilityPanel.Visible = false;
			//
			// ReceivalGroupBox
			//
			this.ReceivalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ReceivalGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|860f3b4a-df30-418c-90c8-085027dfa700", "Receival Details");
			this.ReceivalGroupBox.Controls.Add(this.ReceivalJW_ETABoundDateEdit);
			this.ReceivalGroupBox.Controls.Add(this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox);
			this.ReceivalGroupBox.Controls.Add(this.ReceivalJW_OA_DepartureLocationBoundAddressControl);
			this.ReceivalGroupBox.Controls.Add(this.ReceivalJW_ATABoundDateEdit);
			this.ReceivalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 76, true);
			this.ReceivalGroupBox.Name = "ReceivalGroupBox";
			this.ReceivalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 155, true);
			this.ReceivalGroupBox.TabIndex = 3;
			this.ReceivalGroupBox.TabStop = false;
			//
			// ReceivalJW_ETABoundDateEdit
			//
			this.ReceivalJW_ETABoundDateEdit.AllowDrop = true;
			this.ReceivalJW_ETABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivalJW_ETABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivalJW_ETABoundDateEdit, "JW_ETAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ETAForBinding)));
			this.ReceivalJW_ETABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivalJW_ETABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 64, true);
			this.ReceivalJW_ETABoundDateEdit.Name = "ReceivalJW_ETABoundDateEdit";
			this.ReceivalJW_ETABoundDateEdit.TabIndex = 5;
			//
			// ReceivalJW_RL_NKLoadPortBoundCodeFindBox
			//
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox, "JW_RL_NKLoadPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKLoadPortForBinding)));
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.Name = "ReceivalJW_RL_NKLoadPortBoundCodeFindBox";
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 18, true);
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.TabIndex = 1;
			//
			// ReceivalJW_OA_DepartureLocationBoundAddressControl
			//
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivalJW_OA_DepartureLocationBoundAddressControl, "JW_OA_DepartureLocationForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_DepartureLocationForBinding)));
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.BindToOrgList = "DepartureAddressOrgs";
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 40, true);
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.Name = "ReceivalJW_OA_DepartureLocationBoundAddressControl";
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.PopupCaption = "";
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.ShowAddress = false;
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.TabIndex = 3;
			//
			// ReceivalJW_ATABoundDateEdit
			//
			this.ReceivalJW_ATABoundDateEdit.AllowDrop = true;
			this.ReceivalJW_ATABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivalJW_ATABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivalJW_ATABoundDateEdit, "JW_ATAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ATAForBinding)));
			this.ReceivalJW_ATABoundDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|e623c566-3148-43d6-a411-e24eb536ae15", "ATA");
			this.ReceivalJW_ATABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivalJW_ATABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 64, true);
			this.ReceivalJW_ATABoundDateEdit.Name = "ReceivalJW_ATABoundDateEdit";
			this.ReceivalJW_ATABoundDateEdit.TabIndex = 7;
			//
			// zGroupBox1
			//
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|2fe77167-0169-4c64-ac17-614a883c4db5", "Storage Details");
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.ZAddressControl1);
			this.zGroupBox1.Controls.Add(this.JW_OA_CreditorAddressZAddressControl2);
			this.zGroupBox1.Controls.Add(this.ServiceLevelDropEditControl);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 67, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			//
			// zTextBox1
			//
			this.BindingSource.SetBindingMember(this.zTextBox1, "JW_CarrierBookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_CarrierBookingReference)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.zTextBox1.TabIndex = 2;
			//
			// ZAddressControl1
			//
			this.ZAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZAddressControl1, "JW_OA_CarrierAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_CarrierAddress)));
			this.ZAddressControl1.BindToOrgList = "Lookups.Carriers";
			this.ZAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 14, true);
			this.ZAddressControl1.Name = "ZAddressControl1";
			this.ZAddressControl1.PopupCaption = "";
			this.ZAddressControl1.ShowAddress = false;
			this.ZAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.ZAddressControl1.TabIndex = 0;
			//
			// JW_OA_CreditorAddressZAddressControl2
			//
			this.JW_OA_CreditorAddressZAddressControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JW_OA_CreditorAddressZAddressControl2, "JW_OA_CreditorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_CreditorAddress)));
			this.JW_OA_CreditorAddressZAddressControl2.BindToOrgList = "Lookups.CreditorList";
			this.JW_OA_CreditorAddressZAddressControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 14, true);
			this.JW_OA_CreditorAddressZAddressControl2.Name = "JW_OA_CreditorAddressZAddressControl2";
			this.JW_OA_CreditorAddressZAddressControl2.PopupCaption = "";
			this.JW_OA_CreditorAddressZAddressControl2.ShowAddress = false;
			this.JW_OA_CreditorAddressZAddressControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.JW_OA_CreditorAddressZAddressControl2.TabIndex = 1;
			//
			// ServiceLevelDropEditControl
			//
			this.ServiceLevelDropEditControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEditControl, "JW_PL_NKCarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_PL_NKCarrierServiceLevel)));
			this.ServiceLevelDropEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 38, true);
			this.ServiceLevelDropEditControl.Name = "ServiceLevelDropEditControl";
			this.ServiceLevelDropEditControl.PreBoundMaxLength = 3;
			this.ServiceLevelDropEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
			this.ServiceLevelDropEditControl.TabIndex = 3;
			//
			// AvailabilityGroupBox
			//
			this.AvailabilityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AvailabilityGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|f69dba1f-ad11-4e77-a5de-8e1dbc3cb694", "Availability Details");
			this.AvailabilityGroupBox.Controls.Add(this.AvailabilityJW_ATDBoundDateEdit);
			this.AvailabilityGroupBox.Controls.Add(this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox);
			this.AvailabilityGroupBox.Controls.Add(this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl);
			this.AvailabilityGroupBox.Controls.Add(this.AvailabilityJW_ETDBoundDateEdit);
			this.AvailabilityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 76, true);
			this.AvailabilityGroupBox.Name = "AvailabilityGroupBox";
			this.AvailabilityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 155, true);
			this.AvailabilityGroupBox.TabIndex = 4;
			this.AvailabilityGroupBox.TabStop = false;
			//
			// AvailabilityJW_ATDBoundDateEdit
			//
			this.AvailabilityJW_ATDBoundDateEdit.AllowDrop = true;
			this.AvailabilityJW_ATDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AvailabilityJW_ATDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AvailabilityJW_ATDBoundDateEdit, "JW_ATDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ATDForBinding)));
			this.AvailabilityJW_ATDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AvailabilityJW_ATDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
			this.AvailabilityJW_ATDBoundDateEdit.Name = "AvailabilityJW_ATDBoundDateEdit";
			this.AvailabilityJW_ATDBoundDateEdit.TabIndex = 7;
			//
			// AvailabilityJW_RL_NKDiscPortBoundCodeFindBox
			//
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox, "JW_RL_NKDiscPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKDiscPortForBinding)));
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.Name = "AvailabilityJW_RL_NKDiscPortBoundCodeFindBox";
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 18, true);
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.TabIndex = 1;
			//
			// AvailabilityJW_OA_ArrivalLocationBoundAddressControl
			//
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl, "JW_OA_ArrivalLocationForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).JW_OA_ArrivalLocationForBinding)));
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.BindToOrgList = "ArrivalAddressOrgs";
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.Name = "AvailabilityJW_OA_ArrivalLocationBoundAddressControl";
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.PopupCaption = "";
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.ShowAddress = false;
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.TabIndex = 3;
			//
			// AvailabilityJW_ETDBoundDateEdit
			//
			this.AvailabilityJW_ETDBoundDateEdit.AllowDrop = true;
			this.AvailabilityJW_ETDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AvailabilityJW_ETDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AvailabilityJW_ETDBoundDateEdit, "JW_ETDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(null)).JW_ETDForBinding)));
			this.AvailabilityJW_ETDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AvailabilityJW_ETDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.AvailabilityJW_ETDBoundDateEdit.Name = "AvailabilityJW_ETDBoundDateEdit";
			this.AvailabilityJW_ETDBoundDateEdit.TabIndex = 5;
			//
			// TransportsGrid
			//
			this.TransportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransportsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.Transport)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_ParentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsLinked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsCharterForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.Transport)(null)).JW_LegOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_AdditionalTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_PL_NKCarrierServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VesselForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VesselFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_VoyageFlightForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKLoadPortForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_RL_NKDiscPortForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).IsDomestic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_ETDForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_ETAForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_ATDForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_ATAForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalAvailabilityDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_DocumentaryCutOffForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalCutOffForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalReceivalCommencesForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotAvailabilityDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotCutOffForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotReceivalCommencesForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_DepotStorageDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_TerminalStorageDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_Load_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_Load_ATA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_VGMCutOffForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_STAForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(null)).JW_STDForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).CarrierPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(null)).CreditorPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_CarrierBookingReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_JV_RegistrationNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_LegNotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.Transport)(null)).JW_Distance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_DistanceUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(null)).JW_IsCargoOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_JX_JV_VoyageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_AircraftTypeForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_OnlineScheduleStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_ArrivalPortRouteId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(null)).JW_DeparturePortRouteId)));
			this.TransportsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|7f49f16e-2fce-4923-a73c-d806adc592c9", "Defined By", "The reference number of the parent of this leg.\r\nIf the leg is defined by a consol then this is the consol number.\r\nIf the leg is defined by a shipment then it is the shipments reference number.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JW_ParentDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|9a24eb9b-5c78-40d0-9f22-988bee039ffc", "Is Linked");
			zCheckBoxColumnStyleInfo1.ColumnName = "JW_IsLinked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|7aba0a47-51d5-425f-9fc4-6baa12c2c5b4", "Published", "Published", "Published", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "JW_JX_IsPublished";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo3.ColumnName = "JW_IsCharterForBinding";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JW_LegOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.ColumnName = "JW_TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(37);
			zDropEditColumnStyleInfo2.ColumnName = "JW_TransportType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo3.ColumnName = "JW_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zDropEditColumnStyleInfo4.ColumnName = "JW_PL_NKCarrierServiceLevel";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zMultiControlColumnStyleInfo1.ColumnName = "JW_VesselForBinding";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "JW_VesselFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JW_VoyageFlightForBinding";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|ceac1158-0be1-4438-a1f1-24c789923099", "Voyage / Flight / Truck Ref. / Journey No.");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JW_RL_NKLoadPortForBinding";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JW_RL_NKDiscPortForBinding";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|7d3bb0f6-cfe0-4aad-a09d-352be84b4258", "Is Domestic");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsDomestic";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zDateEditColumnStyleInfo1.ColumnName = "JW_ETDForBinding";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "JW_ETAForBinding";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.ColumnName = "JW_ATDForBinding";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.ColumnName = "JW_ATAForBinding";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.ColumnName = "JW_TerminalAvailabilityDateForBinding";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zDateEditColumnStyleInfo6.ColumnName = "JW_DocumentaryCutOffForBinding";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo7.ColumnName = "JW_TerminalCutOffForBinding";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo8.ColumnName = "JW_TerminalReceivalCommencesForBinding";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo9.ColumnName = "JW_DepotAvailabilityDateForBinding";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo10.ColumnName = "JW_DepotCutOffForBinding";
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo11.ColumnName = "JW_DepotReceivalCommencesForBinding";
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo12.ColumnName = "JW_DepotStorageDateForBinding";
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo13.ColumnName = "JW_TerminalStorageDateForBinding";
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zDateEditColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|e9c392fc-3fbb-4615-8530-3a5679c68baa", "Load Port ETA");
			zDateEditColumnStyleInfo14.ColumnName = "JW_JX_Load_ETA";
			zDateEditColumnStyleInfo14.IsVisible = false;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|9f699db6-12db-42bf-b2a0-448608225173", "Load Port ATA");
			zDateEditColumnStyleInfo15.ColumnName = "JW_JX_Load_ATA";
			zDateEditColumnStyleInfo15.IsVisible = false;
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|2ecb6888-ea98-4868-b557-b7e1d3338c7b", "VGM Cut Off");
			zDateEditColumnStyleInfo16.ColumnName = "JW_VGMCutOffForBinding";
			zDateEditColumnStyleInfo16.IsVisible = false;
			zDateEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo17.ColumnName = "JW_STAForBinding";
			zDateEditColumnStyleInfo17.IsVisible = false;
			zDateEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo18.ColumnName = "JW_STDForBinding";
			zDateEditColumnStyleInfo18.IsVisible = false;
			zDateEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo19.ColumnName = "JW_EmptyReceivalCommencesForBinding";
			zDateEditColumnStyleInfo19.IsVisible = false;
			zDateEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo20.ColumnName = "JW_EmptyCutOffForBinding";
			zDateEditColumnStyleInfo20.IsVisible = false;
			zDateEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo21.ColumnName = "JW_ReeferReceivalCommencesForBinding";
			zDateEditColumnStyleInfo21.IsVisible = false;
			zDateEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo22.ColumnName = "JW_ReeferCutOffForBinding";
			zDateEditColumnStyleInfo22.IsVisible = false;
			zDateEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo23.ColumnName = "JW_DGReceivalCommencesForBinding";
			zDateEditColumnStyleInfo23.IsVisible = false;
			zDateEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo24.ColumnName = "JW_DGCutOffForBinding";
			zDateEditColumnStyleInfo24.IsVisible = false;
			zDateEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CarrierPK";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CreditorPK";
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "JW_CarrierBookingReference";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|84330440-650e-4c5b-810f-3d30e6c5e040", "Aircraft Reg.", "Aircraft Registration Number", "The aircraft registration number for a chartered flight.");
			zTextBoxColumnStyleInfo4.ColumnName = "JW_JX_JV_RegistrationNo";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JW_LegNotes";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|ad017760-8a5c-4763-bc3d-7bb1d0a17989", "Distance");
			zCalcEditColumnStyleInfo2.ColumnName = "JW_Distance";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|f29ae480-c373-4dcc-890d-328a72cf1ad8", "Distance Unit");
			zDropEditColumnStyleInfo5.ColumnName = "JW_DistanceUnit";
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("780afa8f-a866-4f93-b293-4d4936cccdea", "Is Cargo Only");
			zCheckBoxColumnStyleInfo5.ColumnName = "JW_IsCargoOnly";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("e5a82193-91fe-4c83-ac62-8d1a67cb1b61", "Voyage Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JW_JX_JV_VoyageType";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			routeNumberColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("a752ec27-d8e3-4507-a0a7-9d23d95a08cb", "Route Set Number");
			routeNumberColumn.ColumnName = "RouteSetNumber";
			routeNumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			routeNumberColumn.IsVisible = false;
			aircraftTypeColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|b977dc4f-b6fb-4e4c-9a7d-66677d317259", "Aircraft Type", "Aircraft Type", "The aircraft type for a flight.");
			aircraftTypeColumn.ColumnName = "JW_AircraftTypeForBinding";
			aircraftTypeColumn.IsVisible = false;
			aircraftTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			serviceStringColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|d1fb18c0-b1eb-fda8-4a9f-6fc819a62be5", "Service String");
			serviceStringColumn.ColumnName = "JW_ServiceString";
			serviceStringColumn.IsVisible = false;
			serviceStringColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			globalFlightScheduleStatusColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|259accaf-a236-bcab-4101-e7050d2bffe1", "Flight Status");
			globalFlightScheduleStatusColumnStyleInfo.ColumnName = "OnlineScheduleStatusDescription";
			globalFlightScheduleStatusColumnStyleInfo.IsVisible = false;
			globalFlightScheduleStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);

			totalCO2eColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|10cdc55a-eb6a-47e4-bab7-addbd8d4ad87", "CO2e (kg)");
			totalCO2eColumnStyleInfo.ColumnName = "TotalCO2eForSorting";
			totalCO2eColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			totalCO2eColumnStyleInfo.IsVisible = false;
			totalCO2eColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);

			AdditionalModeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("1FF74471-3F9B-4248-92D7-C1EFF5BDE08A", "Additional Mode");
			AdditionalModeColumnStyleInfo.ColumnName = "JW_AdditionalTransportMode";
			AdditionalModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			arrivalPortRouteIdColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("787616B0-58B4-4314-9E4E-4BE2D4DC7508", "Arrival Port Route ID");
			arrivalPortRouteIdColumn.ColumnName = "JW_ArrivalPortRouteId";
			arrivalPortRouteIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			arrivalPortRouteIdColumn.IsVisible = false;

			departurePortRouteIdColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("60CDCE44-1DC7-479A-9F31-8580B3CACB38", "Departure Port Route ID");
			departurePortRouteIdColumn.ColumnName = "JW_DeparturePortRouteId";
			departurePortRouteIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			departurePortRouteIdColumn.IsVisible = false;

			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(AdditionalModeColumnStyleInfo);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo17);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo18);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo19);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo20);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo21);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo22);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo23);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo24);
			this.TransportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TransportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransportsGrid.ColumnStyles.Add(globalFlightScheduleStatusColumnStyleInfo);
			this.TransportsGrid.ColumnStyles.Add(routeNumberColumn);
			this.TransportsGrid.ColumnStyles.Add(aircraftTypeColumn);
			this.TransportsGrid.ColumnStyles.Add(serviceStringColumn);
			this.TransportsGrid.ColumnStyles.Add(arrivalPortRouteIdColumn);
			this.TransportsGrid.ColumnStyles.Add(departurePortRouteIdColumn);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				this.TransportsGrid.ColumnStyles.Add(totalCO2eColumnStyleInfo);
			}
			this.TransportsGrid.CopySelectedRowsAllowed = true;
			this.TransportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportsGrid.GridId = "3ef9da34-304c-4d59-8380-1ed0cdd254ce";
			this.TransportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransportsGrid.LayoutKey = "TransportsGrid";
			this.TransportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportsGrid.Name = "TransportsGrid";
			this.TransportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 228, true);
			this.TransportsGrid.TabIndex = 0;
			this.TransportsGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.ColourCode);
			//
			// Legend
			//
			this.Legend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.Legend.AutoScroll = true;
			this.Legend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Legend.Name = "Legend";
			this.Legend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 24, true);
			this.Legend.TabIndex = 0;
			//
			// legendPanel
			//
			this.legendPanel.Controls.Add(this.Legend);
			this.legendPanel.Controls.Add(this.ShowMapButton);
			this.legendPanel.Controls.Add(this.GlobalSchedulesButton);
			this.legendPanel.Controls.Add(this.SelectScheduleButton);
			this.legendPanel.Controls.Add(this.ImportGlobalScheduleButton);
			this.legendPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.legendPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.legendPanel.Name = "legendPanel";
			this.legendPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 33, true);
			this.legendPanel.TabIndex = 1;
			//
			// ShowMapButton
			//
			this.ShowMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowMapButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|c9e7086b-c9c4-4c09-807b-17b47742fdb1", "Map", "Display the Route Visualizer Map for the currently selected routing leg.");
			this.ShowMapButton.IsCaptionOverridden = false;
			this.ShowMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 4, true);
			this.ShowMapButton.Name = "ShowMapButton";
			this.ShowMapButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ShowMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.ShowMapButton.TabIndex = 1;
			this.ShowMapButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ShowMapButton.UseVisualStyleBackColor = true;
			this.ShowMapButton.Click += new System.EventHandler(this.ShowMapButton_Click);
			//
			// GlobalSchedulesButton
			//
			this.GlobalSchedulesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GlobalSchedulesButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|6ade9e44-f2f0-4243-ba68-2f38580d6496", "Import Global Flights");
			this.GlobalSchedulesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(839, 4, true);
			this.GlobalSchedulesButton.Name = "GlobalSchedulesButton";
			this.GlobalSchedulesButton.Tag = "ShouldNotDisable";
			this.GlobalSchedulesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 23, true);
			this.GlobalSchedulesButton.TabIndex = 4;
			this.GlobalSchedulesButton.UseVisualStyleBackColor = true;
			this.GlobalSchedulesButton.Click += new System.EventHandler(this.GlobalSchedulesButton_Click);
			//
			// SelectScheduleButton
			//
			this.SelectScheduleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectScheduleButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|a65b13c2-88e0-4bde-8497-9ab3e5840c0d", "Published Schedules");
			this.SelectScheduleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 4, true);
			this.SelectScheduleButton.Name = "SelectScheduleButton";
			this.SelectScheduleButton.Tag = "ShouldNotDisable";
			this.SelectScheduleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.SelectScheduleButton.TabIndex = 2;
			this.SelectScheduleButton.UseVisualStyleBackColor = true;
			this.SelectScheduleButton.Click += new System.EventHandler(this.SelectScheduleButton_Click);
			//
			// ImportGlobalScheduleButton
			//
			this.ImportGlobalScheduleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportGlobalScheduleButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|d89f99cc-3647-466b-b077-ce8ee8f29cf7", "Import Global Sailing Schedules");
			this.ImportGlobalScheduleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 4, true);
			this.ImportGlobalScheduleButton.Name = "ImportGlobalScheduleButton";
			this.ImportGlobalScheduleButton.Tag = "ShouldNotDisable";
			this.ImportGlobalScheduleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.ImportGlobalScheduleButton.TabIndex = 3;
			this.ImportGlobalScheduleButton.UseVisualStyleBackColor = true;
			this.ImportGlobalScheduleButton.Click += new System.EventHandler(this.ImportFromOnlineSchedulesButton_Click);
			//
			// DetailsTabPage
			//
			this.DetailsTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|0b709c14-b113-40b4-beec-ceb9544348b2", "Details");
			this.DetailsTabPage.Controls.Add(this.BottomPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 298, true);
			this.DetailsTabPage.TabIndex = 1;
			// 
			// BookingConfirmationGroupBox
			// 
			this.BookingConfirmationGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|96674a91-02c9-4370-813d-6ae8a2111aa3", "Booking Confirmation");
			this.BookingConfirmationGroupBox.Controls.Add(this.BookingConfirmationGrid);
			this.BookingConfirmationGroupBox.Controls.Add(this.BookingConfirmationUpdateButton);
			this.BookingConfirmationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingConfirmationGroupBox.Name = "BookingConfirmationGroupBox";
			this.BookingConfirmationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 150, true);
			this.BookingConfirmationGroupBox.TabIndex = 1;
			this.BookingConfirmationGroupBox.TabStop = false;
			//
			// BookingConfirmationGrid
			//
			this.BookingConfirmationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BookingConfirmationGrid, "BookingConfirmations");
			this.BookingConfirmationGrid.CaptionVisible = false;
			this.BookingConfirmationGrid.CopySelectedRowsAllowed = false;
			this.BookingConfirmationGrid.GridId = "a4ea753d-1fa3-4f8b-9db8-1e7876f802e4";
			this.BookingConfirmationGrid.LayoutKey = "BookingConfirmationGrid";
			this.BookingConfirmationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.BookingConfirmationGrid.Name = "BookingConfirmationGrid";
			this.BookingConfirmationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 91, true);
			this.BookingConfirmationGrid.TabIndex = 0;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo loadPortTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo dischargePortTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo voyageFlightTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo bookedPiecesCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo departureTimeDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo arrivalTimeDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			loadPortTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|cb4c958b-3790-4625-a33f-dde9d09fd6b0", "Load Port");
			loadPortTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			loadPortTextBoxColumnStyleInfo1.ColumnName = "LoadPort";
			loadPortTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			dischargePortTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|51428d8a-e19f-4243-97be-fa1b9ab977d3", "Discharge", "Discharge Port", "");
			dischargePortTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			dischargePortTextBoxColumnStyleInfo1.ColumnName = "DischargePort";
			dischargePortTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			voyageFlightTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|e3a2c85a-254f-46e2-b0d1-50519973bea5", "Voyage / Flight");
			voyageFlightTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			voyageFlightTextBoxColumnStyleInfo1.ColumnName = "VoyageFlight";
			voyageFlightTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			bookedPiecesCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|517a436d-022d-470f-a621-7ff43410e4f2", "Booked Pce.", "Booked Pieces", "");
			bookedPiecesCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			bookedPiecesCalcEditColumnStyleInfo.ColumnName = "BookedPieces";
			bookedPiecesCalcEditColumnStyleInfo.Decimals = 0;
			bookedPiecesCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			departureTimeDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|b38d75b4-91a7-4cca-a1e7-49d182c3e378", "Departure (STD)");
			departureTimeDateEditColumnStyleInfo.ColumnName = "DepartureTime";
			departureTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			arrivalTimeDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|d24cc61d-f42f-403c-8401-9bfbbd772078", "Arrival (STA)");
			arrivalTimeDateEditColumnStyleInfo.ColumnName = "ArrivalTime";
			arrivalTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.BookingConfirmationGrid.ColumnStyles.Add(loadPortTextBoxColumnStyleInfo1);
			this.BookingConfirmationGrid.ColumnStyles.Add(dischargePortTextBoxColumnStyleInfo1);
			this.BookingConfirmationGrid.ColumnStyles.Add(voyageFlightTextBoxColumnStyleInfo1);
			this.BookingConfirmationGrid.ColumnStyles.Add(bookedPiecesCalcEditColumnStyleInfo);
			this.BookingConfirmationGrid.ColumnStyles.Add(departureTimeDateEditColumnStyleInfo);
			this.BookingConfirmationGrid.ColumnStyles.Add(arrivalTimeDateEditColumnStyleInfo);
			//
			// BookingConfirmationUpdateButton
			//
			this.BookingConfirmationUpdateButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|ad2bde86-1601-4aea-9e56-bd9acc836594", "Update Routing Leg");
			this.BookingConfirmationUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 119, true);
			this.BookingConfirmationUpdateButton.Name = "BookingConfirmationUpdateButton";
			this.BookingConfirmationUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.BookingConfirmationUpdateButton.TabIndex = 1;
			this.BookingConfirmationUpdateButton.Click += BookingConfirmationUpdateButton_Click;
			// 
			// ActualEventsGroupBox
			// 
			this.ActualEventsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|3b18c8b1-867e-425a-acfb-ab1b3b582e8a", "Actual Events");
			this.ActualEventsGroupBox.Controls.Add(this.ActualEventsGrid);
			this.ActualEventsGroupBox.Controls.Add(this.ActualEventsUpdateButton);
			this.ActualEventsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.ActualEventsGroupBox.Name = "ActualEventsGroupBox";
			this.ActualEventsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 150, true);
			this.ActualEventsGroupBox.TabIndex = 2;
			this.ActualEventsGroupBox.TabStop = false;
			//
			// ActualEventsGrid
			//
			this.ActualEventsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ActualEventsGrid, "ActualEvents");
			this.ActualEventsGrid.CaptionVisible = false;
			this.ActualEventsGrid.CopySelectedRowsAllowed = false;
			this.ActualEventsGrid.GridId = "65d43e2e-88d1-4d80-9610-b09a0c396b96";
			this.ActualEventsGrid.LayoutKey = "ActualEventsGrid";
			this.ActualEventsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.ActualEventsGrid.Name = "ActualEventsGrid";
			this.ActualEventsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 91, true);
			this.ActualEventsGrid.TabIndex = 0;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo loadPortTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo dischargePortTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo voyageFlightTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo shippedPiecesCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo departedTimeDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo arrivedAndUnloadedPiecesTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo arrivedTimeDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			loadPortTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|5bb81d5e-4e2c-46dc-93f1-7923b0c4e961", "Load Port");
			loadPortTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			loadPortTextBoxColumnStyleInfo2.ColumnName = "LoadPort";
			loadPortTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			dischargePortTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|465e126d-d8bb-4644-834d-cb03dd17204d", "Discharge", "Discharge Port", "");
			dischargePortTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			dischargePortTextBoxColumnStyleInfo2.ColumnName = "DischargePort";
			dischargePortTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			voyageFlightTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|9d539b17-1b2d-46b4-bef9-a9948acca1d9", "Voyage / Flight");
			voyageFlightTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			voyageFlightTextBoxColumnStyleInfo2.ColumnName = "VoyageFlight";
			voyageFlightTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			shippedPiecesCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|3b7f20d1-54f1-4397-ba46-5697b365da42", "Shipped Pce.", "Shipped Pieces", "");
			shippedPiecesCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			shippedPiecesCalcEditColumnStyleInfo.ColumnName = "ShippedPieces";
			shippedPiecesCalcEditColumnStyleInfo.Decimals = 0;
			shippedPiecesCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			departedTimeDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|b09a9987-e4e3-47b8-a2a0-e8439483edb5", "Departed (ATD)");
			departedTimeDateEditColumnStyleInfo.ColumnName = "DepartedTime";
			departedTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			arrivedAndUnloadedPiecesTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|f402f3fe-4d44-483b-84c5-789b6b588946", "Arrived/Unloaded Pce.", "Arrived/Unloaded Pieces", "");
			arrivedAndUnloadedPiecesTextBoxColumnStyleInfo.ColumnName = "ArrivedAndUnloadedPieces";
			arrivedAndUnloadedPiecesTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			arrivedAndUnloadedPiecesTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			arrivedTimeDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|8b6cdcc6-7b12-4af1-abcd-fb07310b84c0", "Arrived (ATA)");
			arrivedTimeDateEditColumnStyleInfo.ColumnName = "ArrivedTime";
			arrivedTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.ActualEventsGrid.ColumnStyles.Add(loadPortTextBoxColumnStyleInfo2);
			this.ActualEventsGrid.ColumnStyles.Add(dischargePortTextBoxColumnStyleInfo2);
			this.ActualEventsGrid.ColumnStyles.Add(voyageFlightTextBoxColumnStyleInfo2);
			this.ActualEventsGrid.ColumnStyles.Add(shippedPiecesCalcEditColumnStyleInfo);
			this.ActualEventsGrid.ColumnStyles.Add(departedTimeDateEditColumnStyleInfo);
			this.ActualEventsGrid.ColumnStyles.Add(arrivedAndUnloadedPiecesTextBoxColumnStyleInfo);
			this.ActualEventsGrid.ColumnStyles.Add(arrivedTimeDateEditColumnStyleInfo);
			//
			// ActualEventsUpdateButton
			//
			this.ActualEventsUpdateButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|8db07ce5-da7a-4836-b2b4-2d8f32fb582b", "Update Routing Leg");
			this.ActualEventsUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 119, true);
			this.ActualEventsUpdateButton.Name = "ActualEventsUpdateButton";
			this.ActualEventsUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.ActualEventsUpdateButton.TabIndex = 1;
			this.ActualEventsUpdateButton.Click += ActualEventsUpdateButton_Click;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|6e2acddb-c167-414b-84e1-5b2190b680ad", "Shipment Details");
			this.ShipmentDetailsGroupBox.Controls.Add(this.TotalPiecesCalcEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.TotalWeightTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.TotalVolumeTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.DeliveredTimeEdit);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 0, true);
			this.ShipmentDetailsGroupBox.Name = "ShipmentDetailsGroupBox";
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 115, true);
			this.ShipmentDetailsGroupBox.TabIndex = 3;
			this.ShipmentDetailsGroupBox.TabStop = false;
			//
			// TotalPieceCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TotalPiecesCalcEdit, "TotalShipmentPieces");
			this.TotalPiecesCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|8fac9174-337f-443c-a0ae-9a8b4d6178da", "Total Pieces");
			this.TotalPiecesCalcEdit.DecimalPlaces = 0;
			this.TotalPiecesCalcEdit.Decimals = 0;
			this.TotalPiecesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.TotalPiecesCalcEdit.Name = "TotalPieceCalcEdit";
			this.TotalPiecesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 18, true);
			this.TotalPiecesCalcEdit.TabIndex = 1;
			this.TotalPiecesCalcEdit.Text = "0";
			this.TotalPiecesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			//
			// TotalWeightTextBox
			//
			this.BindingSource.SetBindingMember(this.TotalWeightTextBox, "TotalShipmentWeight");
			this.TotalWeightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 42, true);
			this.TotalWeightTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|9f7ec064-41c6-40c3-b5bc-b130405c3618", "Total Weight");
			this.TotalWeightTextBox.Name = "TotalWeightTextBox";
			this.TotalWeightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 18, true);
			this.TotalWeightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.TotalWeightTextBox.TabIndex = 2;
			//
			// TotalVolumeTextBox
			//
			this.BindingSource.SetBindingMember(this.TotalVolumeTextBox, "TotalShipmentVolume");
			this.TotalVolumeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 64, true);
			this.TotalVolumeTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|b9269e18-7738-43f3-ad3a-825e655271e8", "Total Volume");
			this.TotalVolumeTextBox.Name = "TotalVolumeTextBox";
			this.TotalVolumeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 18, true);
			this.TotalVolumeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.TotalVolumeTextBox.TabIndex = 3;
			//
			// DeliveredTimeEdit
			//
			this.DeliveredTimeEdit.AllowDrop = true;
			this.DeliveredTimeEdit.AutoCompleteMonthThreshold = 1;
			this.DeliveredTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DeliveredTimeEdit, "ShipmentDeliveredTime");
			this.DeliveredTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveredTimeEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|4347f512-69f1-42c5-a8aa-a7366199fa6a", "Delivered");
			this.DeliveredTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 88, true);
			this.DeliveredTimeEdit.Name = "DeliveredTimeEdit";
			this.DeliveredTimeEdit.TabIndex = 4;
			//
			// ActualPanel
			//
			this.ActualPanel.Controls.Add(this.BookingConfirmationGroupBox);
			this.ActualPanel.Controls.Add(this.ActualEventsGroupBox);
			this.ActualPanel.Controls.Add(this.ShipmentDetailsGroupBox);
			this.ActualPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ActualPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.ActualPanel.Name = "ActualPanel";
			this.ActualPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 298, true);
			this.ActualPanel.TabIndex = 1;
			//
			// ActualTabPage
			//
			this.ActualTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("RoutingPluginControl|3f70eb5b-f100-41e9-8333-a381c310c32c", "Actual");
			this.ActualTabPage.Controls.Add(this.ActualPanel);
			this.ActualTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ActualTabPage.Name = "ActualTabPage";
			this.ActualTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ActualTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 298, true);
			this.ActualTabPage.TabIndex = 2;
			this.ActualTabPage.BindingOrFirstShown += ActualTabPage_BindingOrFirstShown;
			//
			// BottomTabControl
			// 
			this.BottomTabControl.Controls.Add(this.DetailsTabPage);
			this.BottomTabControl.Controls.Add(this.ActualTabPage);
			this.BottomTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 260, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 328, true);
			this.BottomTabControl.TabIndex = 3;
			this.BottomTabControl.Visible = true;
			//
			// RoutingPluginControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportsGrid);
			this.Controls.Add(this.legendPanel);
			this.Controls.Add(this.BottomTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 539, true);
			this.Name = "RoutingPluginControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.NotesGroupBox.ResumeLayout(false);
			this.NotesGroupBox.PerformLayout();
			this.LegDetailsGroupBox.ResumeLayout(false);
			this.LegDetailsGroupBox.PerformLayout();
			this.JW_StatusBoundDropEdit.ResumeLayout(true);
			this.JW_StatusBoundDropEdit.PerformLayout();
			this.JW_TransportTypeBoundDropEdit.ResumeLayout(true);
			this.JW_TransportTypeBoundDropEdit.PerformLayout();
			this.JW_TransportModeBoundDropEdit.ResumeLayout(true);
			this.JW_TransportModeBoundDropEdit.PerformLayout();
			this.JW_AdditionalTransportModeBoundDropEdit.ResumeLayout(true);
			this.JW_AdditionalTransportModeBoundDropEdit.PerformLayout();
			this.OriginDestinationPanel.ResumeLayout(false);
			this.OriginDestinationPanel.PerformLayout();
			this.DestinationDetailsGroupBox.ResumeLayout(false);
			this.DestinationDetailsGroupBox.PerformLayout();
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.ResumeLayout(true);
			this.DestinationJW_RL_NKDiscPortBoundCodeFindBox.PerformLayout();
			this.JW_TerminalAvailabilityDateBoundDateEdit.ResumeLayout(true);
			this.JW_TerminalAvailabilityDateBoundDateEdit.PerformLayout();
			this.DestinationJW_ETABoundDateEdit.ResumeLayout(true);
			this.DestinationJW_ETABoundDateEdit.PerformLayout();
			this.JW_TerminalStorageDateBoundDateEdit.ResumeLayout(true);
			this.JW_TerminalStorageDateBoundDateEdit.PerformLayout();
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.ResumeLayout(true);
			this.DestinationJW_OA_ArrivalLocationBoundAddressControl.PerformLayout();
			this.JW_DepotStorageDateBoundDateEdit.ResumeLayout(true);
			this.JW_DepotStorageDateBoundDateEdit.PerformLayout();
			this.JW_DepotAvailabilityDateBoundDateEdit.ResumeLayout(true);
			this.JW_DepotAvailabilityDateBoundDateEdit.PerformLayout();
			this.DestinationJW_ATABoundDateEdit.ResumeLayout(true);
			this.DestinationJW_ATABoundDateEdit.PerformLayout();
			this.VoyageFlightDetailsGroupBox.ResumeLayout(false);
			this.VoyageFlightDetailsGroupBox.PerformLayout();
			this.CarrierPKFindBox.ResumeLayout(true);
			this.CarrierPKFindBox.PerformLayout();
			this.ServiceStringBoundTextBox.ResumeLayout(true);
			this.ServiceStringBoundTextBox.PerformLayout();
			this.JW_VesselBoundCodeFindBox.ResumeLayout(true);
			this.JW_VesselBoundCodeFindBox.PerformLayout();
			this.JW_OA_CarrierAddressZAddressControl.ResumeLayout(true);
			this.JW_OA_CarrierAddressZAddressControl.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.JW_OA_CreditorAddressZAddressControl.ResumeLayout(true);
			this.JW_OA_CreditorAddressZAddressControl.PerformLayout();
			this.OriginDetailsGroupBox.ResumeLayout(false);
			this.OriginDetailsGroupBox.PerformLayout();
			this.OriginJW_ATDBoundDateEdit.ResumeLayout(true);
			this.OriginJW_ATDBoundDateEdit.PerformLayout();
			this.JW_DepotReceivalCommencesBoundDateEdit.ResumeLayout(true);
			this.JW_DepotReceivalCommencesBoundDateEdit.PerformLayout();
			this.JW_DepotCutOffBoundDateEdit.ResumeLayout(true);
			this.JW_DepotCutOffBoundDateEdit.PerformLayout();
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.ResumeLayout(true);
			this.OriginJW_RL_NKLoadPortBoundCodeFindBox.PerformLayout();
			this.OriginJW_OA_DepartureLocationBoundAddressControl.ResumeLayout(true);
			this.OriginJW_OA_DepartureLocationBoundAddressControl.PerformLayout();
			this.JW_TerminalCutOffBoundDateEdit.ResumeLayout(true);
			this.JW_TerminalCutOffBoundDateEdit.PerformLayout();
			this.JW_TerminalReceivalCommencesBoundDateEdit.ResumeLayout(true);
			this.JW_TerminalReceivalCommencesBoundDateEdit.PerformLayout();
			this.JW_DocumentaryCutOffBoundDateEdit.ResumeLayout(true);
			this.JW_DocumentaryCutOffBoundDateEdit.PerformLayout();
			this.JW_VGMCutOffBoundDateEdit.ResumeLayout(true);
			this.JW_VGMCutOffBoundDateEdit.PerformLayout();
			this.OriginJW_ETDBoundDateEdit.ResumeLayout(true);
			this.OriginJW_ETDBoundDateEdit.PerformLayout();
			this.ReceivalAvailabilityPanel.ResumeLayout(false);
			this.ReceivalAvailabilityPanel.PerformLayout();
			this.ReceivalGroupBox.ResumeLayout(false);
			this.ReceivalGroupBox.PerformLayout();
			this.ReceivalJW_ETABoundDateEdit.ResumeLayout(true);
			this.ReceivalJW_ETABoundDateEdit.PerformLayout();
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.ResumeLayout(true);
			this.ReceivalJW_RL_NKLoadPortBoundCodeFindBox.PerformLayout();
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.ResumeLayout(true);
			this.ReceivalJW_OA_DepartureLocationBoundAddressControl.PerformLayout();
			this.ReceivalJW_ATABoundDateEdit.ResumeLayout(true);
			this.ReceivalJW_ATABoundDateEdit.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ZAddressControl1.ResumeLayout(true);
			this.ZAddressControl1.PerformLayout();
			this.JW_OA_CreditorAddressZAddressControl2.ResumeLayout(true);
			this.JW_OA_CreditorAddressZAddressControl2.PerformLayout();
			this.ServiceLevelDropEditControl.ResumeLayout(true);
			this.ServiceLevelDropEditControl.PerformLayout();
			this.AvailabilityGroupBox.ResumeLayout(false);
			this.AvailabilityGroupBox.PerformLayout();
			this.AvailabilityJW_ATDBoundDateEdit.ResumeLayout(true);
			this.AvailabilityJW_ATDBoundDateEdit.PerformLayout();
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.ResumeLayout(true);
			this.AvailabilityJW_RL_NKDiscPortBoundCodeFindBox.PerformLayout();
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.ResumeLayout(true);
			this.AvailabilityJW_OA_ArrivalLocationBoundAddressControl.PerformLayout();
			this.AvailabilityJW_ETDBoundDateEdit.ResumeLayout(true);
			this.AvailabilityJW_ETDBoundDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).EndInit();
			this.TransportsGrid.ResumeLayout(false);
			this.TransportsGrid.PerformLayout();
			this.legendPanel.ResumeLayout(false);
			this.legendPanel.PerformLayout();
			this.BookingConfirmationGroupBox.ResumeLayout(false);
			this.BookingConfirmationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BookingConfirmationGrid)).EndInit();
			this.BookingConfirmationGrid.ResumeLayout(false);
			this.BookingConfirmationGrid.PerformLayout();
			this.ActualEventsGroupBox.ResumeLayout(false);
			this.ActualEventsGroupBox.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ActualPanel.ResumeLayout(false);
			this.ActualPanel.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ActualTabPage.ResumeLayout(false);
			this.ActualTabPage.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		internal Enterprise.ZArchitecture.ZGrid TransportsGrid;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginJW_RL_NKLoadPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationJW_RL_NKDiscPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OriginJW_ETDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DestinationJW_ATABoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OriginJW_ATDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DestinationJW_ETABoundDateEdit;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JW_DepotStorageDateBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_TerminalStorageDateBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_DepotAvailabilityDateBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_TerminalAvailabilityDateBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_TerminalCutOffBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_DepotCutOffBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_TerminalReceivalCommencesBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit JW_DepotReceivalCommencesBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_DocumentaryCutOffBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_VGMCutOffBoundDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl JW_OA_CarrierAddressZAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JW_OA_CreditorAddressZAddressControl;
		private Enterprise.ZArchitecture.ZTextBox JW_CarrierBookingReferenceBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ColourLegend Legend;
		private Enterprise.ZArchitecture.GUI.ZPanel legendPanel;
		private Enterprise.ZArchitecture.GUI.ZAddressControl DestinationJW_OA_ArrivalLocationBoundAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl OriginJW_OA_DepartureLocationBoundAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DestinationDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OriginDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox VoyageFlightDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZButton SelectScheduleButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportGlobalScheduleButton;
		private Enterprise.ZArchitecture.GUI.ZPanel ReceivalAvailabilityPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ReceivalGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit AvailabilityJW_ATDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ReceivalJW_RL_NKLoadPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ReceivalJW_OA_DepartureLocationBoundAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit AvailabilityJW_ETDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AvailabilityGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox AvailabilityJW_RL_NKDiscPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReceivalJW_ETABoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZAddressControl AvailabilityJW_OA_ArrivalLocationBoundAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReceivalJW_ATABoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel OriginDestinationPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ZAddressControl1;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JW_OA_CreditorAddressZAddressControl2;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ServiceLevelDropEditControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NotesGroupBox;
		private Enterprise.ZArchitecture.ZTextBox JW_LegNotesBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LegDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox JW_ParentDescriptionBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JW_IsLinkedBoundCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JW_IsCharterBoundCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit JW_LegOrderBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JW_StatusBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JW_TransportTypeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JW_TransportModeBoundDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit JW_AdditionalTransportModeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ServiceLevelDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox JW_VoyageFlightBoundTextBox;
		internal Enterprise.ZArchitecture.ZTextBox JW_AircraftTypeBoundTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox JW_VesselBoundCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox JW_VesselBoundTextBox;
		internal Enterprise.ZArchitecture.ZTextBox JW_JX_JV_RegistrationNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton GlobalSchedulesButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsPublishedCheckBox;
		internal ZArchitecture.GUI.ZCheckBox JW_IsCargoOnlyCheckBox;
		internal ZArchitecture.GUI.ZGuidFindBox CarrierPKFindBox;
		internal Enterprise.ZArchitecture.ZTextBox ServiceStringBoundTextBox;
		internal ZArchitecture.GUI.ZButton ShowMapButton;
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo AdditionalModeColumnStyleInfo;
		Enterprise.ZArchitecture.GUI.ZTabControl BottomTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage ActualTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel ActualPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox BookingConfirmationGroupBox;
		Enterprise.ZArchitecture.ZGrid BookingConfirmationGrid;
		Enterprise.ZArchitecture.GUI.ZButton BookingConfirmationUpdateButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox ActualEventsGroupBox;
		Enterprise.ZArchitecture.ZGrid ActualEventsGrid;
		Enterprise.ZArchitecture.GUI.ZButton ActualEventsUpdateButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox ShipmentDetailsGroupBox;
		Enterprise.ZArchitecture.ZCalcEdit TotalPiecesCalcEdit;
		Enterprise.ZArchitecture.ZTextBox TotalWeightTextBox;
		Enterprise.ZArchitecture.ZTextBox TotalVolumeTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit DeliveredTimeEdit;
	}
}
