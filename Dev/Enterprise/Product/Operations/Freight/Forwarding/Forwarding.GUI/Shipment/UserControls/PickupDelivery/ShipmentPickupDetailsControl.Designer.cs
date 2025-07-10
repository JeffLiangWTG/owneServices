using System;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ShipmentPickupDetailsControl
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
			this.components = new System.ComponentModel.Container();
			this.PickupPenaltiesGrid = new Enterprise.Freight.Forwarding.GUI.ShipmentPickupPenaltiesGrid();
			this.JS_OH_ExportBrokerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ConsignorPickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PickupCFSAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PickupFromAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JS_OA_ExportReceivingDepotControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PickupDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACIConsignorOriginZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PickupCartageZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ACIConsignorOriginZoneBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PickupTruckWaitTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.PickupCartageZoneBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PickupLabourTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.JP_PickupCartageCompletedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_EstimatedPickupBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabelDuration = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEditPickupTruckWaitCharge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEditPickupLabourCharge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabelCharge = new Enterprise.ZArchitecture.ZLabel();
			this.JP_PickupRequiredFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_EstimatedPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_FCLPickupEquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JP_PickupCartageAdvisedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOfReceiptDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InterimReceiptTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_BookingReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PickupCartageCoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zAddressControl1 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PickupAgentOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.PickupStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.PickupPenaltiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FreeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DetentionFreeDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetentionDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetentionChargeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetentionDaysLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DetentionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelPickupLabour = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelTruckWaitTime = new Enterprise.ZArchitecture.ZLabel();
			this.zDateEditReceiptRequested = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEditDispatchRequested = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CFSDepartureByTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PickupByTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickupPenaltiesGrid.SuspendLayout();
			this.JS_OH_ExportBrokerOrganisationControl.SuspendLayout();
			this.ConsignorPickupDocAddressControl.SuspendLayout();
			this.PickupCFSAddressGroupBox.SuspendLayout();
			this.PickupFromAddressGroupBox.SuspendLayout();
			this.JS_OA_ExportReceivingDepotControl.SuspendLayout();
			this.PickupDetailsGroupBox.SuspendLayout();
			this.JP_PickupCartageCompletedBoundDateEdit.SuspendLayout();
			this.JE_EstimatedPickupBoundDateEdit.SuspendLayout();
			this.JP_PickupRequiredFromDateEdit.SuspendLayout();
			this.JP_EstimatedPickupDateEdit.SuspendLayout();
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.SuspendLayout();
			this.JP_PickupCartageAdvisedBoundDateEdit.SuspendLayout();
			this.DateOfReceiptDateEdit.SuspendLayout();
			this.PickupCartageCoGroupBox.SuspendLayout();
			this.zAddressControl1.SuspendLayout();
			this.PickupAgentOrganisationControl.SuspendLayout();
			this.PickupStatusGroupBox.SuspendLayout();
			this.ShipmentStatusDropEdit.SuspendLayout();
			this.PickupByTransportModeDropEdit.SuspendLayout();
			this.CFSDepartureByTransportModeDropEdit.SuspendLayout();
			this.zDateEditReceiptRequested.SuspendLayout();
			this.zDateEditDispatchRequested.SuspendLayout();
			this.ContainerTabControl.SuspendLayout();
			this.PickupPenaltiesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonShipment);
			// 
			// PickupPenaltiesGrid
			// 
			this.PickupPenaltiesGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupPenaltiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(((Enterprise.Freight.Business.CommonShipment)(null)))));
			this.PickupPenaltiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickupPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickupPenaltiesGrid.Name = "PickupPenaltiesGrid";
			this.PickupPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 125, true);
			this.PickupPenaltiesGrid.TabIndex = 0;
			// 
			// JS_OH_ExportBrokerOrganisationControl
			// 
			this.JS_OH_ExportBrokerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_OH_ExportBrokerOrganisationControl, "JS_OH_ExportBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_OH_ExportBroker)));
			this.JS_OH_ExportBrokerOrganisationControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Export Broker");
			this.JS_OH_ExportBrokerOrganisationControl.Captions = new string[] {
        "Export Broker"};
			this.JS_OH_ExportBrokerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.JS_OH_ExportBrokerOrganisationControl.IsCaptionOverridden = false;
			this.JS_OH_ExportBrokerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JS_OH_ExportBrokerOrganisationControl.Name = "JS_OH_ExportBrokerOrganisationControl";
			this.JS_OH_ExportBrokerOrganisationControl.PopupCaption = "";
			this.JS_OH_ExportBrokerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 52, true);
			this.JS_OH_ExportBrokerOrganisationControl.TabIndex = 0;
			// 
			// PickupCartageCoGroupBox
			// 
			this.PickupCartageCoGroupBox.Controls.Add(this.zAddressControl1);
			this.PickupCartageCoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 64, true);
			this.PickupCartageCoGroupBox.Name = "PickupCartageCoGroupBox";
			this.PickupCartageCoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 64, true);
			this.PickupCartageCoGroupBox.TabIndex = 1;
			this.PickupCartageCoGroupBox.TabStop = false;
			this.PickupCartageCoGroupBox.Text = "Pickup Cartage Company";
			// 
			// zAddressControl1
			// 
			this.zAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zAddressControl1, "DocsAndCartage.JP_OA_PickupCartageCoAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_OA_PickupCartageCoAddr)));
			this.zAddressControl1.BindToOrgList = "Lookups.LocalTransportAtOrigin_List";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zAddressControl1, false);
			this.zAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zAddressControl1.Name = "zAddressControl1";
			this.zAddressControl1.PopupCaption = "";
			this.zAddressControl1.ReadOnly = false;
			this.zAddressControl1.ShowAddress = false;
			this.zAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 36, true);
			this.zAddressControl1.StackControls = true;
			this.zAddressControl1.TabIndex = 2;
			// 
			// PickupAgentOrganisationControl
			// 
			this.PickupAgentOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupAgentOrganisationControl, "PickupAgentDocumentaryAddress.OrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonShipment)(null)).PickupAgentDocumentaryAddress.OrganisationPK)));
			this.PickupAgentOrganisationControl.BindToOrganisations = "Lookups.Forwarder_List";
			this.PickupAgentOrganisationControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|ce9392ae-36ee-4a3b-93b2-96b3c895f9c0", "Pickup Agent");
			this.PickupAgentOrganisationControl.Captions = new string[] {
		"Pickup Agent"};
			this.PickupAgentOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.PickupAgentOrganisationControl.IsCaptionOverridden = false;
			this.PickupAgentOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 137, true);
			this.PickupAgentOrganisationControl.Name = "PickupAgentOrganisationControl";
			this.PickupAgentOrganisationControl.PopupCaption = "";
			this.PickupAgentOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 52, true);
			this.PickupAgentOrganisationControl.TabIndex = 3;
			// 
			// PickupFromAddressGroupBox
			// 
			this.PickupFromAddressGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|37E0DFB5-9DC9-421C-874F-E29AF1FA93B4", "Pickup From");
			this.PickupFromAddressGroupBox.Controls.Add(this.PickupByTransportModeDropEdit);
			this.PickupFromAddressGroupBox.Controls.Add(this.ConsignorPickupDocAddressControl);
			this.PickupFromAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 197, true);
			this.PickupFromAddressGroupBox.Name = "PickupFromAddressGroupBox";
			this.PickupFromAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 250, true);
			this.PickupFromAddressGroupBox.TabIndex = 4;
			// 
			// PickupCFSAddressGroupBox
			// 
			this.PickupCFSAddressGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|01686392-110e-4e20-abee-e9f054a5689b", "CFS / Transit Warehouse");
			this.PickupCFSAddressGroupBox.Controls.Add(this.zDateEditDispatchRequested);
			this.PickupCFSAddressGroupBox.Controls.Add(this.zDateEditReceiptRequested);
			this.PickupCFSAddressGroupBox.Controls.Add(this.JS_OA_ExportReceivingDepotControl);
			this.PickupCFSAddressGroupBox.Controls.Add(this.InterimReceiptTextBox);
			this.PickupCFSAddressGroupBox.Controls.Add(this.DateOfReceiptDateEdit);
			this.PickupCFSAddressGroupBox.Controls.Add(this.CFSDepartureByTransportModeDropEdit);
			this.PickupCFSAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 55, true);
			this.PickupCFSAddressGroupBox.Name = "PickupCFSAddressGroupBox";
			this.PickupCFSAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(771, 113, true);
			this.PickupCFSAddressGroupBox.TabIndex = 6;
			this.PickupCFSAddressGroupBox.TabStop = false;
			// 
			// JS_OA_ExportReceivingDepotControl
			// 
			this.JS_OA_ExportReceivingDepotControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_OA_ExportReceivingDepotControl, "JS_OA_ExportReceivingDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_OA_ExportReceivingDepot)));
			this.JS_OA_ExportReceivingDepotControl.BindToOrgList = "Lookups.PackDepot_List";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_OA_ExportReceivingDepotControl, false);
			this.JS_OA_ExportReceivingDepotControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.JS_OA_ExportReceivingDepotControl.Name = "JS_OA_ExportReceivingDepotControl";
			this.JS_OA_ExportReceivingDepotControl.PopupCaption = null;
			this.JS_OA_ExportReceivingDepotControl.ReadOnly = false;
			this.JS_OA_ExportReceivingDepotControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 79, true);
			this.JS_OA_ExportReceivingDepotControl.StackControls = true;
			this.JS_OA_ExportReceivingDepotControl.TabIndex = 5;
			// 
			// ConsignorPickupDocAddressControl
			// 
			this.ConsignorPickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorPickupDocAddressControl, "ConsignorPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Business.CommonShipment)(null)).ConsignorPickupAddress)));
			this.ConsignorPickupDocAddressControl.BindToOrganisations = "Lookups.ConsignorForwarderPickup_List";
			this.ConsignorPickupDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|dc13bfb1-ad16-4710-b264-9cc38de96df7", "Pickup From", "The organization and address to pickup the goods from.");
			this.ConsignorPickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.ConsignorPickupDocAddressControl.Name = "ConsignorPickupDocAddressControl";
			this.ConsignorPickupDocAddressControl.ReadOnly = false;
			this.ConsignorPickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorPickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsignorPickupDocAddressControl.TabIndex = 4;
			this.ConsignorPickupDocAddressControl.ValidationJustForced = false;
			this.ConsignorPickupDocAddressControl.Text = " ";
			// 
			// PickupStatusGroupBox
			// 
			this.PickupStatusGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|463b6128-0f88-4b0d-b603-0798bfb3e743", "Pickup Status");
			this.PickupStatusGroupBox.Controls.Add(this.ShipmentStatusDropEdit);
			this.PickupStatusGroupBox.Controls.Add(this.JS_BookingReferenceTextBox);
			this.PickupStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 3, true);
			this.PickupStatusGroupBox.Name = "PickupStatusGroupBox";
			this.PickupStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 46, true);
			this.PickupStatusGroupBox.TabIndex = 5;
			this.PickupStatusGroupBox.TabStop = false;
			// 
			// JS_BookingReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_BookingReferenceTextBox, "JS_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_BookingReference)));
			this.JS_BookingReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.JS_BookingReferenceTextBox.Name = "JS_BookingReferenceTextBox";
			this.JS_BookingReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 18, true);
			this.JS_BookingReferenceTextBox.TabIndex = 1;
			//
			// InterimReceiptTextBox
			// 
			this.BindingSource.SetBindingMember(this.InterimReceiptTextBox, "JS_InterimReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_InterimReceipt)));
			this.InterimReceiptTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 63, true);
			this.InterimReceiptTextBox.Name = "InterimReceiptTextBox";
			this.InterimReceiptTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.InterimReceiptTextBox.TabIndex = 12;
			// 
			// DateOfReceiptDateEdit
			// 
			this.DateOfReceiptDateEdit.AllowDrop = true;
			this.DateOfReceiptDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfReceiptDateEdit.AutoCompleteYear = true;
			this.DateOfReceiptDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.DateOfReceiptDateEdit, "JS_A_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_A_RCV)));
			this.DateOfReceiptDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateOfReceiptDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 86, true);
			this.DateOfReceiptDateEdit.Name = "DateOfReceiptDateEdit";
			this.DateOfReceiptDateEdit.TabIndex = 14;
			// 
			// PickupDetailsGroupBox
			// 
			this.PickupDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|22c34dc7-0e48-4b12-9c28-b0889808d393", "Pickup Details");
			this.PickupDetailsGroupBox.Controls.Add(this.DetentionLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.zLabelPickupLabour);
			this.PickupDetailsGroupBox.Controls.Add(this.zLabelTruckWaitTime);
			this.PickupDetailsGroupBox.Controls.Add(this.DetentionFreeDaysCalcEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.DetentionDaysLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.DetentionDaysCalcEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.DetentionChargeCalcEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.FreeLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.ACIConsignorOriginZoneLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.PickupCartageZoneLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.ACIConsignorOriginZoneBoundLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.PickupTruckWaitTimeEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.PickupCartageZoneBoundLabel);
			this.PickupDetailsGroupBox.Controls.Add(this.PickupLabourTimeEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.JP_PickupCartageCompletedBoundDateEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.JE_EstimatedPickupBoundDateEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.zLabelDuration);
			this.PickupDetailsGroupBox.Controls.Add(this.zCalcEditPickupTruckWaitCharge);
			this.PickupDetailsGroupBox.Controls.Add(this.zCalcEditPickupLabourCharge);
			this.PickupDetailsGroupBox.Controls.Add(this.zLabelCharge);
			this.PickupDetailsGroupBox.Controls.Add(this.JP_PickupRequiredFromDateEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.JP_EstimatedPickupDateEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.JP_FCLPickupEquipmentNeededBoundDropEdit);
			this.PickupDetailsGroupBox.Controls.Add(this.JP_PickupCartageAdvisedBoundDateEdit);
			this.PickupDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 174, true);
			this.PickupDetailsGroupBox.Name = "PickupDetailsGroupBox";
			this.PickupDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 267, true);
			this.PickupDetailsGroupBox.TabIndex = 8;
			this.PickupDetailsGroupBox.TabStop = false;
			// 
			// JP_FCLPickupEquipmentNeededBoundDropEdit
			// 
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JP_FCLPickupEquipmentNeededBoundDropEdit, "DocsAndCartage+JP_FCLPickupEquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_FCLPickupEquipmentNeeded)));
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|2897f3c5-67d4-4bf5-ad06-6f83d4937612", "Drop Mode", "Port Transport Drop Mode", "");
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 16, true);
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.Name = "JP_FCLPickupEquipmentNeededBoundDropEdit";
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 18, true);
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.TabIndex = 0;
			// 
			// JP_EstimatedPickupDateEdit
			// 
			this.JP_EstimatedPickupDateEdit.AllowDrop = true;
			this.JP_EstimatedPickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_EstimatedPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_EstimatedPickupDateEdit, "DocsAndCartage+JP_EstimatedPickup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_EstimatedPickup)));
			this.JP_EstimatedPickupDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|959a1bcd-5572-4f7f-a8ab-ef3e14adcf73", "Estimated Pickup");
			this.JP_EstimatedPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_EstimatedPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 47, true);
			this.JP_EstimatedPickupDateEdit.Name = "JP_EstimatedPickupDateEdit";
			this.JP_EstimatedPickupDateEdit.TabIndex = 2;
			// 
			// JP_PickupRequiredFromDateEdit
			// 
			this.JP_PickupRequiredFromDateEdit.AllowDrop = true;
			this.JP_PickupRequiredFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_PickupRequiredFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_PickupRequiredFromDateEdit, "DocsAndCartage+JP_PickupRequiredFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupRequiredFrom)));
			this.JP_PickupRequiredFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_PickupRequiredFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 47, true);
			this.JP_PickupRequiredFromDateEdit.Name = "JP_PickupRequiredFromDateEdit";
			this.JP_PickupRequiredFromDateEdit.TabIndex = 1;
			// 
			// JE_EstimatedPickupBoundDateEdit
			// 
			this.JE_EstimatedPickupBoundDateEdit.AllowDrop = true;
			this.JE_EstimatedPickupBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_EstimatedPickupBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_EstimatedPickupBoundDateEdit, "DocsAndCartage+JP_PickupRequiredBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupRequiredBy)));
			this.JE_EstimatedPickupBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JE_EstimatedPickupBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 70, true);
			this.JE_EstimatedPickupBoundDateEdit.Name = "JE_EstimatedPickupBoundDateEdit";
			this.JE_EstimatedPickupBoundDateEdit.TabIndex = 3;
			// 
			// JP_PickupCartageAdvisedBoundDateEdit
			// 
			this.JP_PickupCartageAdvisedBoundDateEdit.AllowDrop = true;
			this.JP_PickupCartageAdvisedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_PickupCartageAdvisedBoundDateEdit.AutoCompleteYear = true;
			this.JP_PickupCartageAdvisedBoundDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JP_PickupCartageAdvisedBoundDateEdit, "DocsAndCartage.JP_PickupCartageAdvised");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupCartageAdvised)));
			this.JP_PickupCartageAdvisedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_PickupCartageAdvisedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 93, true);
			this.JP_PickupCartageAdvisedBoundDateEdit.Name = "JP_PickupCartageAdvisedBoundDateEdit";
			this.JP_PickupCartageAdvisedBoundDateEdit.TabIndex = 3;
			// 
			// JP_PickupCartageCompletedBoundDateEdit
			// 
			this.JP_PickupCartageCompletedBoundDateEdit.AllowDrop = true;
			this.JP_PickupCartageCompletedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_PickupCartageCompletedBoundDateEdit.AutoCompleteYear = true;
			this.JP_PickupCartageCompletedBoundDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JP_PickupCartageCompletedBoundDateEdit, "DocsAndCartage+JP_PickupCartageCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupCartageCompleted)));
			this.JP_PickupCartageCompletedBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|b294ea55-0009-4523-a544-962b7d5616d2", "Actual Pickup");
			this.JP_PickupCartageCompletedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_PickupCartageCompletedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 93, true);
			this.JP_PickupCartageCompletedBoundDateEdit.Name = "JP_PickupCartageCompletedBoundDateEdit";
			this.JP_PickupCartageCompletedBoundDateEdit.TabIndex = 4;
			// 
			// zCalcEditPickupLabourCharge
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditPickupLabourCharge, "DocsAndCartage+JP_PickupLabourCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupLabourCharge)));
			this.zCalcEditPickupLabourCharge.CaptionResourceString = null;
			this.zCalcEditPickupLabourCharge.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditPickupLabourCharge, false);
			this.zCalcEditPickupLabourCharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 142, true);
			this.zCalcEditPickupLabourCharge.Name = "zCalcEditPickupLabourCharge";
			this.zCalcEditPickupLabourCharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zCalcEditPickupLabourCharge.TabIndex = 8;
			this.zCalcEditPickupLabourCharge.Text = "0.00";
			this.zCalcEditPickupLabourCharge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickupLabourTimeEdit
			// 
			this.PickupLabourTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PickupLabourTimeEdit, "DocsAndCartage+JP_PickupLabourTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupLabourTime)));
			this.PickupLabourTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupLabourTimeEdit, false);
			this.PickupLabourTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 142, true);
			this.PickupLabourTimeEdit.Name = "PickupLabourTimeEdit";
			this.PickupLabourTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.PickupLabourTimeEdit.TabIndex = 7;
			// 
			// PickupCartageZoneBoundLabel
			// 
			this.PickupCartageZoneBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PickupCartageZoneBoundLabel, "JS_Calc_PickupCartageZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_Calc_PickupCartageZone)));
			this.PickupCartageZoneBoundLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupCartageZoneBoundLabel, false);
			this.PickupCartageZoneBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 221, true);
			this.PickupCartageZoneBoundLabel.Name = "PickupCartageZoneBoundLabel";
			this.PickupCartageZoneBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 13, true);
			this.PickupCartageZoneBoundLabel.TabIndex = 8;
			this.PickupCartageZoneBoundLabel.Text = "PickupCartageZone";
			// 
			// zCalcEditPickupTruckWaitCharge
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditPickupTruckWaitCharge, "DocsAndCartage+JP_PickupTruckWaitCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupTruckWaitCharge)));
			this.zCalcEditPickupTruckWaitCharge.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditPickupTruckWaitCharge, false);
			this.zCalcEditPickupTruckWaitCharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 164, true);
			this.zCalcEditPickupTruckWaitCharge.Name = "zCalcEditPickupTruckWaitCharge";
			this.zCalcEditPickupTruckWaitCharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zCalcEditPickupTruckWaitCharge.TabIndex = 10;
			this.zCalcEditPickupTruckWaitCharge.Text = "0.00";
			this.zCalcEditPickupTruckWaitCharge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACIConsignorOriginZoneBoundLabel
			// 
			this.ACIConsignorOriginZoneBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACIConsignorOriginZoneBoundLabel, "JS_Calc_ACIConsignorOriginZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_Calc_ACIConsignorOriginZone)));
			this.ACIConsignorOriginZoneBoundLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ACIConsignorOriginZoneBoundLabel, false);
			this.ACIConsignorOriginZoneBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 243, true);
			this.ACIConsignorOriginZoneBoundLabel.Name = "ACIConsignorOriginZoneBoundLabel";
			this.ACIConsignorOriginZoneBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.ACIConsignorOriginZoneBoundLabel.TabIndex = 10;
			this.ACIConsignorOriginZoneBoundLabel.Text = "AciZone";
			// 
			// PickupTruckWaitTimeEdit
			// 
			this.PickupTruckWaitTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PickupTruckWaitTimeEdit, "DocsAndCartage+JP_PickupTruckWaitTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_PickupTruckWaitTime)));
			this.PickupTruckWaitTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PickupTruckWaitTimeEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PickupTruckWaitTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 164, true);
			this.PickupTruckWaitTimeEdit.Name = "PickupTruckWaitTimeEdit";
			this.PickupTruckWaitTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.PickupTruckWaitTimeEdit.TabIndex = 9;
			// 
			// zLabelCharge
			// 
			this.zLabelCharge.AutoSize = true;
			this.zLabelCharge.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|4a901797-6104-4fcb-af40-2847d738e26f", "Charge");
			this.zLabelCharge.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabelCharge.IsFontBold = true;
			this.zLabelCharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 125, true);
			this.zLabelCharge.Name = "zLabelCharge";
			this.zLabelCharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 14, true);
			this.zLabelCharge.TabIndex = 11;
			// 
			// zLabelDuration
			// 
			this.zLabelDuration.AutoSize = true;
			this.zLabelDuration.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|16a7dd3e-756a-468f-85be-894115be18d2", "Duration");
			this.zLabelDuration.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabelDuration.IsFontBold = true;
			this.zLabelDuration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 125, true);
			this.zLabelDuration.Name = "zLabelDuration";
			this.zLabelDuration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.zLabelDuration.TabIndex = 13;
			// 
			// ShipmentStatusDropEdit
			// 
			this.ShipmentStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentStatusDropEdit, "JS_ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_ShipmentStatus)));
			this.ShipmentStatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("71fe3661-57ac-4438-af72-a7b43c136771", "Status", "Booking Status", "HBL Booking Status", "");
			this.ShipmentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 17, true);
			this.ShipmentStatusDropEdit.Name = "ShipmentStatusDropEdit";
			this.ShipmentStatusDropEdit.PreBoundMaxLength = 3;
			this.ShipmentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 18, true);
			this.ShipmentStatusDropEdit.TabIndex = 13;
			// 
			// PickupCartageZoneLabel
			// 
			this.PickupCartageZoneLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|c0a8f753-ccb4-46d0-a04e-9a4aaf1b66f6", "Port Trn. Zone:", "Port Transport Zone:", "");
			this.PickupCartageZoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PickupCartageZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 221, true);
			this.PickupCartageZoneLabel.Name = "PickupCartageZoneLabel";
			this.PickupCartageZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.PickupCartageZoneLabel.TabIndex = 15;
			this.PickupCartageZoneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ACIConsignorOriginZoneLabel
			// 
			this.ACIConsignorOriginZoneLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|da821195-e065-460c-91b2-934f45317039", "ACI Zone:");
			this.ACIConsignorOriginZoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ACIConsignorOriginZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 243, true);
			this.ACIConsignorOriginZoneLabel.Name = "ACIConsignorOriginZoneLabel";
			this.ACIConsignorOriginZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.ACIConsignorOriginZoneLabel.TabIndex = 16;
			this.ACIConsignorOriginZoneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FreeLabel
			// 
			this.FreeLabel.AutoSize = true;
			this.FreeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c6edec87-15e9-406d-bf5a-b0aba327a20a", "Free");
			this.FreeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FreeLabel.IsFontBold = true;
			this.FreeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 125, true);
			this.FreeLabel.Name = "FreeLabel";
			this.FreeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.FreeLabel.TabIndex = 17;
			// 
			// DetentionFreeDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DetentionFreeDaysCalcEdit, "DocsAndCartage+JP_FCLPickupDetentionFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_FCLPickupDetentionFreeDays)));
			this.DetentionFreeDaysCalcEdit.CaptionResourceString = null;
			this.DetentionFreeDaysCalcEdit.DecimalPlaces = 0;
			this.DetentionFreeDaysCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetentionFreeDaysCalcEdit, false);
			this.DetentionFreeDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 186, true);
			this.DetentionFreeDaysCalcEdit.Name = "DetentionFreeDaysCalcEdit";
			this.DetentionFreeDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.DetentionFreeDaysCalcEdit.TabIndex = 11;
			this.DetentionFreeDaysCalcEdit.Text = "0";
			this.DetentionFreeDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DetentionDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DetentionDaysCalcEdit, "DocsAndCartage+JP_FCLPickupDetentionDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_FCLPickupDetentionDays)));
			this.DetentionDaysCalcEdit.CaptionResourceString = null;
			this.DetentionDaysCalcEdit.DecimalPlaces = 0;
			this.DetentionDaysCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetentionDaysCalcEdit, false);
			this.DetentionDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 186, true);
			this.DetentionDaysCalcEdit.Name = "DetentionDaysCalcEdit";
			this.DetentionDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.DetentionDaysCalcEdit.TabIndex = 12;
			this.DetentionDaysCalcEdit.Text = "0";
			this.DetentionDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DetentionChargeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DetentionChargeCalcEdit, "DocsAndCartage+JP_FCLPickupDetentionCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(null)).DocsAndCartage.JP_FCLPickupDetentionCharge)));
			this.DetentionChargeCalcEdit.CaptionResourceString = null;
			this.DetentionChargeCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetentionChargeCalcEdit, false);
			this.DetentionChargeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 186, true);
			this.DetentionChargeCalcEdit.Name = "DetentionChargeCalcEdit";
			this.DetentionChargeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.DetentionChargeCalcEdit.TabIndex = 13;
			this.DetentionChargeCalcEdit.Text = "0.00";
			this.DetentionChargeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DetentionDaysLabel
			// 
			this.DetentionDaysLabel.AutoSize = true;
			this.DetentionDaysLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("29ffee24-f028-4f8f-856e-9c5a61e413aa", "Days");
			this.DetentionDaysLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DetentionDaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 190, true);
			this.DetentionDaysLabel.Name = "DetentionDaysLabel";
			this.DetentionDaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.DetentionDaysLabel.TabIndex = 36;
			// 
			// zDateEditReceiptRequested
			// 
			this.zDateEditReceiptRequested.AllowDrop = true;
			this.zDateEditReceiptRequested.AutoCompleteMonthThreshold = 1;
			this.zDateEditReceiptRequested.AutoCompleteYear = true;
			this.zDateEditReceiptRequested.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zDateEditReceiptRequested, "JS_ExportReceivingDepotReceiptRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_ExportReceivingDepotReceiptRequested)));
			this.zDateEditReceiptRequested.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0fb03f53-b960-49e7-a7c4-b9b10749cc41", "Receipt Requested");
			this.zDateEditReceiptRequested.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditReceiptRequested.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 17, true);
			this.zDateEditReceiptRequested.Name = "zDateEditReceiptRequested";
			this.zDateEditReceiptRequested.TabIndex = 6;
			// 
			// zDateEditDispatchRequested
			// 
			this.zDateEditDispatchRequested.AllowDrop = true;
			this.zDateEditDispatchRequested.AutoCompleteMonthThreshold = 1;
			this.zDateEditDispatchRequested.AutoCompleteYear = true;
			this.zDateEditDispatchRequested.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zDateEditDispatchRequested, "JS_ExportReceivingDepotDispatchRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).JS_ExportReceivingDepotDispatchRequested)));
			this.zDateEditDispatchRequested.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e37ff6a0-530f-4ef5-b33b-c3fc74abf5fb", "Dispatch Requested");
			this.zDateEditDispatchRequested.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditDispatchRequested.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 40, true);
			this.zDateEditDispatchRequested.Name = "zDateEditDispatchRequested";
			this.zDateEditDispatchRequested.TabIndex = 7;
			// 
			// PickupByTransportModeDropEdit
			// 
			this.PickupByTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupByTransportModeDropEdit, "PickupByTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).PickupByTransportMode)));
			this.PickupByTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|5CB8155C-1990-427D-9A1F-CB59B3D22FA9", "Pickup By", "Transport Mode.");
			this.PickupByTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 202, true);
			this.PickupByTransportModeDropEdit.Name = "PickupByTransportModeDropEdit";
			this.PickupByTransportModeDropEdit.PreBoundMaxLength = 3;
			this.PickupByTransportModeDropEdit.ShowDescriptionBox = true;
			this.PickupByTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.PickupByTransportModeDropEdit.TabIndex = 5;
			// 
			// CFSDepartureByTransportModeDropEdit
			// 
			this.CFSDepartureByTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CFSDepartureByTransportModeDropEdit, "CFSDepartureByTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonShipment)(null)).CFSDepartureByTransportMode)));
			this.CFSDepartureByTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|5CB8155C-1990-427D-9A1F-CB59B3D22FA9", "CFS Departure By", "Transport Mode.");
			this.CFSDepartureByTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 17, true);
			this.CFSDepartureByTransportModeDropEdit.Name = "CFSDepartureByTransportModeDropEdit";
			this.CFSDepartureByTransportModeDropEdit.PreBoundMaxLength = 3;
			this.CFSDepartureByTransportModeDropEdit.ShowDescriptionBox = true;
			this.CFSDepartureByTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17 , true);
			this.CFSDepartureByTransportModeDropEdit.TabIndex = 15;
			// 
			// DetentionLabel
			// 
			this.DetentionLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("278df211-732b-459d-ba27-4f8e33dd3841", "Detention");
			this.DetentionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DetentionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 186, true);
			this.DetentionLabel.Name = "DetentionLabel";
			this.DetentionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 16, true);
			this.DetentionLabel.TabIndex = 41;
			this.DetentionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelPickupLabour
			// 
			this.zLabelPickupLabour.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("48a628b2-acca-4f8a-8ba1-62420272c39f", "Pickup Labor");
			this.zLabelPickupLabour.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelPickupLabour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 142, true);
			this.zLabelPickupLabour.Name = "zLabelPickupLabour";
			this.zLabelPickupLabour.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 16, true);
			this.zLabelPickupLabour.TabIndex = 39;
			this.zLabelPickupLabour.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelTruckWaitTime
			// 
			this.zLabelTruckWaitTime.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d732bf3d-5c61-4609-a162-b09f9a051485", "Truck Wait Time");
			this.zLabelTruckWaitTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelTruckWaitTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 164, true);
			this.zLabelTruckWaitTime.Name = "zLabelTruckWaitTime";
			this.zLabelTruckWaitTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 16, true);
			this.zLabelTruckWaitTime.TabIndex = 40;
			this.zLabelTruckWaitTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// 
			// ContainerTabControl
			// 
			this.ContainerTabControl.Controls.Add(this.PickupPenaltiesTabPage);
			this.ContainerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 447, true);
			this.ContainerTabControl.Name = "ContainerTabControl";
			this.ContainerTabControl.SelectedIndex = 0;
			this.ContainerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 148, true);
			this.ContainerTabControl.TabIndex = 9;
			// 
			// PickupPenaltiesTabPage
			// 
			this.PickupPenaltiesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPickupDetailsControl|5bd9a621-7daa-452e-b66c-76a2586b7e3d", "Penalties");
			this.PickupPenaltiesTabPage.Controls.Add(this.PickupPenaltiesGrid);
			this.PickupPenaltiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PickupPenaltiesTabPage.Name = "PickupPenaltiesTabPage";
			this.PickupPenaltiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 125, true);
			this.PickupPenaltiesTabPage.TabIndex = 1;
			// ShipmentPickupDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerTabControl);
			this.Controls.Add(this.PickupStatusGroupBox);
			this.Controls.Add(this.PickupAgentOrganisationControl);
			this.Controls.Add(this.PickupCartageCoGroupBox);
			this.Controls.Add(this.PickupDetailsGroupBox);
			this.Controls.Add(this.JS_OH_ExportBrokerOrganisationControl);
			this.Controls.Add(this.PickupCFSAddressGroupBox);
			this.Controls.Add(this.PickupFromAddressGroupBox);
			this.Name = "ShipmentPickupDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickupPenaltiesGrid.ResumeLayout(true);
			this.PickupPenaltiesGrid.PerformLayout();
			this.JS_OH_ExportBrokerOrganisationControl.ResumeLayout(true);
			this.JS_OH_ExportBrokerOrganisationControl.PerformLayout();
			this.ConsignorPickupDocAddressControl.ResumeLayout(true);
			this.ConsignorPickupDocAddressControl.PerformLayout();
			this.PickupCFSAddressGroupBox.ResumeLayout(false);
			this.PickupCFSAddressGroupBox.PerformLayout();
			this.PickupFromAddressGroupBox.ResumeLayout(false);
			this.PickupFromAddressGroupBox.PerformLayout();
			this.JS_OA_ExportReceivingDepotControl.ResumeLayout(true);
			this.JS_OA_ExportReceivingDepotControl.PerformLayout();
			this.PickupDetailsGroupBox.ResumeLayout(false);
			this.PickupDetailsGroupBox.PerformLayout();
			this.JP_PickupCartageCompletedBoundDateEdit.ResumeLayout(true);
			this.JP_PickupCartageCompletedBoundDateEdit.PerformLayout();
			this.JE_EstimatedPickupBoundDateEdit.ResumeLayout(true);
			this.JE_EstimatedPickupBoundDateEdit.PerformLayout();
			this.JP_PickupRequiredFromDateEdit.ResumeLayout(true);
			this.JP_PickupRequiredFromDateEdit.PerformLayout();
			this.JP_EstimatedPickupDateEdit.ResumeLayout(true);
			this.JP_EstimatedPickupDateEdit.PerformLayout();
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.JP_FCLPickupEquipmentNeededBoundDropEdit.PerformLayout();
			this.JP_PickupCartageAdvisedBoundDateEdit.ResumeLayout(true);
			this.JP_PickupCartageAdvisedBoundDateEdit.PerformLayout();
			this.DateOfReceiptDateEdit.ResumeLayout(true);
			this.DateOfReceiptDateEdit.PerformLayout();
			this.PickupCartageCoGroupBox.ResumeLayout(false);
			this.PickupCartageCoGroupBox.PerformLayout();
			this.zAddressControl1.ResumeLayout(true);
			this.zAddressControl1.PerformLayout();
			this.PickupAgentOrganisationControl.ResumeLayout(true);
			this.PickupAgentOrganisationControl.PerformLayout();
			this.PickupStatusGroupBox.ResumeLayout(false);
			this.PickupStatusGroupBox.PerformLayout();
			this.ShipmentStatusDropEdit.ResumeLayout(true);
			this.ShipmentStatusDropEdit.PerformLayout();
			this.PickupByTransportModeDropEdit.ResumeLayout(true);
			this.PickupByTransportModeDropEdit.PerformLayout();
			this.CFSDepartureByTransportModeDropEdit.ResumeLayout(true);
			this.CFSDepartureByTransportModeDropEdit.PerformLayout();
			this.zDateEditReceiptRequested.ResumeLayout(true);
			this.zDateEditReceiptRequested.PerformLayout();
			this.zDateEditDispatchRequested.ResumeLayout(true);
			this.zDateEditDispatchRequested.PerformLayout();
			this.ContainerTabControl.ResumeLayout(false);
			this.ContainerTabControl.PerformLayout();
			this.PickupPenaltiesTabPage.ResumeLayout(false);
			this.PickupPenaltiesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.MasterFiles.GUI.ZOrganisationControl JS_OH_ExportBrokerOrganisationControl;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl ConsignorPickupDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PickupCFSAddressGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PickupFromAddressGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PickupDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx PickupTruckWaitTimeEdit;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx PickupLabourTimeEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_PickupCartageCompletedBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JE_EstimatedPickupBoundDateEdit;
		private Enterprise.ZArchitecture.ZLabel zLabelDuration;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEditPickupTruckWaitCharge;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEditPickupLabourCharge;
		private Enterprise.ZArchitecture.ZLabel zLabelCharge;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_PickupRequiredFromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_EstimatedPickupDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JP_FCLPickupEquipmentNeededBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_PickupCartageAdvisedBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PickupCartageCoGroupBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl zAddressControl1;
		private Enterprise.ZArchitecture.ZLabel PickupCartageZoneBoundLabel;
		private Enterprise.ZArchitecture.ZLabel ACIConsignorOriginZoneBoundLabel;
		internal Enterprise.ZArchitecture.ZLabel ACIConsignorOriginZoneLabel;
		private Enterprise.ZArchitecture.ZLabel PickupCartageZoneLabel;
		private MasterFiles.GUI.ZOrganisationControl PickupAgentOrganisationControl;
		private ZArchitecture.GUI.ZAddressControl JS_OA_ExportReceivingDepotControl;
		private ZArchitecture.GUI.ZGroupBox PickupStatusGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ShipmentStatusDropEdit;
		private ZArchitecture.ZTextBox JS_BookingReferenceTextBox;
		private ZArchitecture.GUI.ZDateEdit DateOfReceiptDateEdit;
		private ZArchitecture.ZTextBox InterimReceiptTextBox;
		private ZArchitecture.GUI.ZTabControl ContainerTabControl;
		internal ZArchitecture.GUI.ZTabPage PickupPenaltiesTabPage;
		internal ZArchitecture.ZLabel FreeLabel;
		internal ZArchitecture.ZCalcEdit DetentionFreeDaysCalcEdit;
		internal ZArchitecture.ZLabel DetentionDaysLabel;
		internal ZArchitecture.ZCalcEdit DetentionDaysCalcEdit;
		internal ZArchitecture.ZCalcEdit DetentionChargeCalcEdit;
		internal ZArchitecture.ZLabel DetentionLabel;
		private ZArchitecture.ZLabel zLabelPickupLabour;
		private ZArchitecture.ZLabel zLabelTruckWaitTime;
		private ZArchitecture.GUI.ZDateEdit zDateEditDispatchRequested;
		private ZArchitecture.GUI.ZDropEdit PickupByTransportModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CFSDepartureByTransportModeDropEdit;
		private ZArchitecture.GUI.ZDateEdit zDateEditReceiptRequested;
		private ShipmentPickupPenaltiesGrid PickupPenaltiesGrid;
	}
}
