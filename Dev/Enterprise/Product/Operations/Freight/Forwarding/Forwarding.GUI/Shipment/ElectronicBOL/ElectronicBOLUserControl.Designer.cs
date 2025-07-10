using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.MasterFiles.Business;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ElectronicBOLUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private void InitializeComponent()
		{
			this.MainPanel = new ZPanel();
			this.RightPanel = new ZPanel();
			this.LeftPanel = new ZPanel();
			this.BillOfLadingBillTypeDropEdit = new ZDropEdit();
			this.BillOfLadingBillTermsDropEdit = new ZDropEdit();
			this.FirstHolderDocAddressControl = new ZDocAddressControl();
			this.SurrenderPartyDocAddressControl = new ZDocAddressControl();
			this.ShipperDocAddressControl = new ZDocAddressControl();
			this.ConsigneeDocAddressControl = new ZDocAddressControl();
			this.ToOrderDocAddressControl = new ZDocAddressControl();
			this.ConsigneeTextBox = new ZTextBox();
			this.ConsigneeTextBoxLabel = new ZLabel();
			this.AmendmentRequestDetailsTextBox = new ZTextBox();
			this.PublishButton = new ZButton();
			this.HouseBillNumberTextBox = new ZTextBox();
			this.VersionTextBox = new ZTextBox();
			this.EBLIdentifierTextBox = new ZTextBox();
			this.BillStatusDropEdit = new ZDropEdit();
			this.EBLDateDateEdit = new ZDateEdit();
			this.ViewEditBillButton = new ZButton();
			this.MainPanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.RightPanel);
			this.MainPanel.Controls.Add(this.LeftPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			this.MainPanel.TabIndex = 0;
			// 
			// LeftPanel
			//
			this.LeftPanel.Controls.Add(this.BillOfLadingBillTypeDropEdit);
			this.LeftPanel.Controls.Add(this.FirstHolderDocAddressControl);
			this.LeftPanel.Controls.Add(this.SurrenderPartyDocAddressControl);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 464, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// BillOfLadingBillTypeDropEdit
			// 
			this.BillOfLadingBillTypeDropEdit.AllowDrop = true;
			this.BillOfLadingBillTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillTypeDropEdit, "JS_ElectronicBillOfLadingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingType)));
			this.BillOfLadingBillTypeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5181a23f-214a-44f6-8b67-8b980225a218", "Bill Type");
			this.BillOfLadingBillTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 16, true);
			this.BillOfLadingBillTypeDropEdit.Name = "BillOfLadingBillTypeDropEdit";
			this.BillOfLadingBillTypeDropEdit.PreBoundMaxLength = 3;
			this.BillOfLadingBillTypeDropEdit.ShouldResizeByMaxLength = true;
			this.BillOfLadingBillTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillOfLadingBillTypeDropEdit.TabIndex = 1;
			this.BillOfLadingBillTypeDropEdit.SelectedIndexChanged += new System.EventHandler(this.BillOfLadingBillTypeDropEdit_SelectedIndexChanged);
			// 
			// FirstHolderDocAddressControl
			// 
			this.FirstHolderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstHolderDocAddressControl, "HolderDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).HolderDocAddress)));
			this.FirstHolderDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("da554c2d-3c30-4492-a005-2f5a0cd4c4b8", "First Holder");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FirstHolderDocAddressControl, false);
			this.FirstHolderDocAddressControl.BindToOrganisations = "Lookups.Holder_List";
			this.FirstHolderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 55, true);
			this.FirstHolderDocAddressControl.Name = "FirstHolderDocAddressControl";
			this.FirstHolderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.FirstHolderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.FirstHolderDocAddressControl.TabIndex = 2;
			// 
			// SurrenderPartyDocAddressControl
			// 
			this.SurrenderPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SurrenderPartyDocAddressControl, "SurrenderPartyDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).SurrenderPartyDocAddress)));
			this.SurrenderPartyDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1F5BD41E-270B-43DE-879D-F83E02C4A036", "Surrender Party");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SurrenderPartyDocAddressControl, false);
			this.SurrenderPartyDocAddressControl.BindToOrganisations = "Lookups.SurrenderParty_List";
			this.SurrenderPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 245, true);
			this.SurrenderPartyDocAddressControl.Name = "SurrenderPartyDocAddressControl";
			this.SurrenderPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SurrenderPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.SurrenderPartyDocAddressControl.TabIndex = 3;
			// 
			// RightPanel
			//
			this.RightPanel.Controls.Add(this.BillOfLadingBillTermsDropEdit);
			this.RightPanel.Controls.Add(this.ShipperDocAddressControl);
			this.RightPanel.Controls.Add(this.ConsigneeDocAddressControl);
			this.RightPanel.Controls.Add(this.ConsigneeTextBox);
			this.RightPanel.Controls.Add(this.ConsigneeTextBoxLabel);
			this.RightPanel.Controls.Add(this.ToOrderDocAddressControl);
			this.RightPanel.Controls.Add(this.AmendmentRequestDetailsTextBox);
			this.RightPanel.Controls.Add(this.PublishButton);
			this.RightPanel.Controls.Add(this.HouseBillNumberTextBox);
			this.RightPanel.Controls.Add(this.VersionTextBox);
			this.RightPanel.Controls.Add(this.EBLIdentifierTextBox);
			this.RightPanel.Controls.Add(this.BillStatusDropEdit);
			this.RightPanel.Controls.Add(this.EBLDateDateEdit);
			this.RightPanel.Controls.Add(this.ViewEditBillButton);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 464, true);
			this.RightPanel.TabIndex = 1;
			// 
			// BillingOfLadingBillTermsDropEdit
			// 
			this.BillOfLadingBillTermsDropEdit.AllowDrop = true;
			this.BillOfLadingBillTermsDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillTermsDropEdit, "JS_ElectronicBillOfLadingTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingTerms)));
			this.BillOfLadingBillTermsDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2ed44a5f-86d2-462e-879c-a96baa989164", "Bill Terms");
			this.BillOfLadingBillTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.BillOfLadingBillTermsDropEdit.Name = "BillOfLadingBillTermsDropEdit";
			this.BillOfLadingBillTermsDropEdit.PreBoundMaxLength = 3;
			this.BillOfLadingBillTermsDropEdit.ShouldResizeByMaxLength = true;
			this.BillOfLadingBillTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 40, true);
			this.BillOfLadingBillTermsDropEdit.ReadOnly = true;
			this.BillOfLadingBillTermsDropEdit.TabIndex = 1;
			// 
			// ShipperDocAddressControl
			// 
			this.ShipperDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperDocAddressControl, "ShipperDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).ShipperDocAddress)));
			this.ShipperDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ea4af3f0-daa8-4cef-9442-c353c5226fa0", "Shipper");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipperDocAddressControl, false);
			this.ShipperDocAddressControl.BindToOrganisations = "Lookups.Consignor_List";
			this.ShipperDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 56, true);
			this.ShipperDocAddressControl.Name = "ShipperDocAddressControl";
			this.ShipperDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ShipperDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.ShipperDocAddressControl.ReadOnly = true;
			this.ShipperDocAddressControl.TabIndex = 2;
			// 
			//ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "JS_ElectronicBillOfLadingConsigneeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingConsigneeDocAddress)));
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ea4af3f0-daa8-4cef-9442-c353c5226fa0", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocAddressControl, false);
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignee_List";
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 56, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.ConsigneeDocAddressControl.ReadOnly = true;
			this.ConsigneeDocAddressControl.TabIndex = 3;
			//
			// ConsigneeTextBoxLabel
			//
			this.ConsigneeTextBoxLabel.Name = "ConsigneeTextBoxLabel";
			this.ConsigneeTextBoxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 55, true);
			this.ConsigneeTextBoxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ConsigneeTextBoxLabel.IsFontBold = true;
			this.ConsigneeTextBoxLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1e146d18-997c-4a44-a55e-238e0cfd7363", "Consignee");
			this.ConsigneeTextBoxLabel.TabIndex = 3;
			//
			// ConsigneeTextBox
			//
			this.BindingSource.SetBindingMember(this.ConsigneeTextBox, "JS_ElectronicBillOfLadingConsigneeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingConsigneeForBinding)));
			this.ConsigneeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 70, true);
			this.ConsigneeTextBox.Multiline = true;
			this.ConsigneeTextBox.Name = "ConsigneeTextBox";
			this.ConsigneeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ConsigneeTextBox.ReadOnly = true;
			this.ConsigneeTextBox.TabIndex = 3;
			// 
			//ToOrderDocAddressControl
			// 
			this.ToOrderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToOrderDocAddressControl, "JS_ElectronicBillOfLadingToOrderDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingToOrderDocAddress)));
			this.ToOrderDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1106fad4-fa73-45fa-9f04-f0aa8cd6f7ca", "To Order");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToOrderDocAddressControl, false);
			this.ToOrderDocAddressControl.BindToOrganisations = "Lookups.ToOrder_List";
			this.ToOrderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 56, true);
			this.ToOrderDocAddressControl.Name = "ToOrderDocAddressControl";
			this.ToOrderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ToOrderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.ToOrderDocAddressControl.TabIndex = 3;
			//
			// AmendmentRequestDetailsTextBox
			//
			this.BindingSource.SetBindingMember(this.AmendmentRequestDetailsTextBox, "JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding)));
			this.AmendmentRequestDetailsTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("918f8616-096c-45f0-8a8f-f4fa744ee702", "Instruction Details");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AmendmentRequestDetailsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AmendmentRequestDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 255, true);
			this.AmendmentRequestDetailsTextBox.Multiline = true;
			this.AmendmentRequestDetailsTextBox.Name = "AmendmentRequestDetailsTextBox";
			this.AmendmentRequestDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 100, true);
			this.AmendmentRequestDetailsTextBox.ReadOnly = true;
			this.AmendmentRequestDetailsTextBox.TabIndex = 4;
			// 
			// PublishButton
			//
			this.PublishButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("240EBE22-C69A-4337-9E75-BFF8CFAA049C", "Publish");
			this.PublishButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 375, true);
			this.PublishButton.Name = "PublishButton";
			this.PublishButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 30, true);
			this.PublishButton.TabIndex = 5;
			this.PublishButton.ToolTipCaption = null;
			this.PublishButton.Click += new System.EventHandler(this.ButtonPublish_Click);
			//
			// HouseBillNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.HouseBillNumberTextBox, "JS_ElectronicBillOfLadingHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingHouseBill)));
			this.HouseBillNumberTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f60d08fe-92d1-4174-9a6d-7b5744bbabe5", "House Bill Number");
			this.HouseBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 415, true);
			this.HouseBillNumberTextBox.Multiline = false;
			this.HouseBillNumberTextBox.Name = "HouseBillNumberTextBox";
			this.HouseBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.HouseBillNumberTextBox.ReadOnly = true;
			this.HouseBillNumberTextBox.TabIndex = 6;
			//
			// VersionTextBox
			//
			this.BindingSource.SetBindingMember(this.VersionTextBox, "JS_ElectronicBillOfLadingVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZShort)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingVersion)));
			this.VersionTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ff4b78ed-94fd-42f9-9cf6-1f453af01261", "Version");
			this.VersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 415, true);
			this.VersionTextBox.Multiline = false;
			this.VersionTextBox.Name = "VersionTextBox";
			this.VersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VersionTextBox.ReadOnly = true;
			this.VersionTextBox.TabIndex = 7;
			//
			// EBLIdentifierTextBox
			//
			this.BindingSource.SetBindingMember(this.EBLIdentifierTextBox, "JS_ElectronicBillOfLadingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingReference)));
			this.EBLIdentifierTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4d196865-ce32-4838-aa15-9b21e31c4014", "eBL Identifier");
			this.EBLIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 445, true);
			this.EBLIdentifierTextBox.Multiline = false;
			this.EBLIdentifierTextBox.Name = "EBLIdentifierTextBox";
			this.EBLIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.EBLIdentifierTextBox.ReadOnly = true;
			this.EBLIdentifierTextBox.TabIndex = 8;
			this.EBLIdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			//
			// BillStatusDropEdit
			//
			this.BillStatusDropEdit.AllowDrop = true;
			this.BillStatusDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillStatusDropEdit, "JS_ElectronicBillOfLadingStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ElectronicBillOfLadingStatus)));
			this.BillStatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b74244ec-8130-4223-9068-6a73a3c68bb0", "Bill Status");
			this.BillStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 475, true);
			this.BillStatusDropEdit.Name = "BillStatusDropEdit";
			this.BillStatusDropEdit.PreBoundMaxLength = 3;
			this.BillStatusDropEdit.ShouldResizeByMaxLength = true;
			this.BillStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillStatusDropEdit.TabIndex = 9;
			// 
			// EBLDateDateEdit
			// 
			this.EBLDateDateEdit.AllowDrop = true;
			this.EBLDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EBLDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EBLDateDateEdit, "JS_Calc_ElectronicBillOfLadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_Calc_ElectronicBillOfLadingDate)));
			this.EBLDateDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1b2254ab-b867-46d4-b0b2-33416abad20d", "Date");
			this.EBLDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EBLDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 475, true);
			this.EBLDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.EBLDateDateEdit.Name = "EBLDateDateEdit";
			this.EBLDateDateEdit.TabIndex = 10;

			//
			// ViewEditBillButton
			//
			this.ViewEditBillButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ElectronicBOLUserControl|bbac6ec0-46ca-4d4f-8d80-c571478e400b", "View/Edit Bill Of Lading");
			this.ViewEditBillButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 9, true);
			this.ViewEditBillButton.Name = "ViewEditBillButton";
			this.ViewEditBillButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 30, true);
			this.ViewEditBillButton.TabIndex = 11;
			this.ViewEditBillButton.ToolTipCaption = null;
			this.ViewEditBillButton.Click += new EventHandler(this.ViewEditBillButton_Click);

			// 
			// ElectronicBOLUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "ElectronicBOLUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.FirstHolderDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
		}

		private ZPanel MainPanel;
		private ZPanel LeftPanel;
		private ZPanel RightPanel;
		private ZDropEdit BillOfLadingBillTypeDropEdit;
		private ZDropEdit BillOfLadingBillTermsDropEdit;
		private ZDocAddressControl FirstHolderDocAddressControl;
		private ZDocAddressControl SurrenderPartyDocAddressControl;
		private ZDocAddressControl ShipperDocAddressControl;
		private ZDocAddressControl ConsigneeDocAddressControl;
		private ZDocAddressControl ToOrderDocAddressControl;
		private ZTextBox ConsigneeTextBox;
		private ZLabel ConsigneeTextBoxLabel;
		private ZTextBox AmendmentRequestDetailsTextBox;
		internal ZButton PublishButton;
		private ZTextBox HouseBillNumberTextBox;
		private ZTextBox VersionTextBox;
		private ZTextBox EBLIdentifierTextBox;
		private ZDropEdit BillStatusDropEdit;
		private ZDateEdit EBLDateDateEdit;
		internal ZButton ViewEditBillButton;
	}
}
