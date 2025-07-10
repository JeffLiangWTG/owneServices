using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI;

partial class HVLVTestShipmentDataCreatorForm
{
	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private new void InitializeComponent()
	{
		this.mainGroupBox = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.shipmentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
		this.shipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.consigneeDocumentaryAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
		this.consignorDocumentaryAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
		this.shipmentCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.consignmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.wayBillPrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ItemLineCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.ItemCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.consignmentCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.createButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.mainGroupBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.shipmentSplitContainer)).BeginInit();
		this.shipmentSplitContainer.Panel1.SuspendLayout();
		this.shipmentSplitContainer.Panel2.SuspendLayout();
		this.shipmentSplitContainer.SuspendLayout();
		this.shipmentDetailsGroupBox.SuspendLayout();
		this.consigneeDocumentaryAddress.SuspendLayout();
		this.consignorDocumentaryAddress.SuspendLayout();
		this.consignmentDetailsGroupBox.SuspendLayout();
		this.bottomPanel.SuspendLayout();
		this.SuspendLayout();
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 471, true);
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVTestDataConfiguration);
		// 
		// mainGroupBox
		// 
		this.mainGroupBox.Controls.Add(this.shipmentSplitContainer);
		this.mainGroupBox.Controls.Add(this.bottomPanel);
		this.mainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.mainGroupBox.Name = "mainGroupBox";
		this.mainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 471, true);
		this.mainGroupBox.TabIndex = 1;
		// 
		// shipmentSplitContainer
		// 
		this.shipmentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.shipmentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.shipmentSplitContainer.Name = "shipmentSplitContainer";
		this.shipmentSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// shipmentSplitContainer.Panel1
		// 
		this.shipmentSplitContainer.Panel1.Controls.Add(this.shipmentDetailsGroupBox);
		// 
		// shipmentSplitContainer.Panel2
		// 
		this.shipmentSplitContainer.Panel2.Controls.Add(this.consignmentDetailsGroupBox);
		this.shipmentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 441, true);
		this.shipmentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(239);
		this.shipmentSplitContainer.SplitterWidth = 6;
		// 
		// shipmentDetailsGroupBox
		// 
		this.shipmentDetailsGroupBox.Controls.Add(this.consigneeDocumentaryAddress);
		this.shipmentDetailsGroupBox.Controls.Add(this.consignorDocumentaryAddress);
		this.shipmentDetailsGroupBox.Controls.Add(this.shipmentCountCalcEdit);
		this.shipmentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.shipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.shipmentDetailsGroupBox.Name = "shipmentDetailsGroupBox";
		this.shipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 239, true);
		this.shipmentDetailsGroupBox.TabIndex = 2;
		this.shipmentDetailsGroupBox.TabStop = false;
		this.shipmentDetailsGroupBox.CaptionResourceString = Res.GetData("9DFFFB04-116C-4A7B-9BFF-3760352EF0E3", "Shipment Details");
		// 
		// shipmentCountCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.shipmentCountCalcEdit, "ShipmentCount");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).ShipmentCount)));
		this.shipmentCountCalcEdit.DecimalPlaces = 2;
		this.shipmentCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 63, true);
		this.shipmentCountCalcEdit.Name = "shipmentCountCalcEdit";
		this.shipmentCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
		this.shipmentCountCalcEdit.TabIndex = 3;
		this.shipmentCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.shipmentCountCalcEdit.TrackDisposedAccess = true;
		this.shipmentCountCalcEdit.AllowNegative = false;
		// 
		// consignorDocumentaryAddress
		// 
		this.consignorDocumentaryAddress.AddressValidationProcessCmdKey = null;
		this.consignorDocumentaryAddress.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.consignorDocumentaryAddress, "Shipment.ConsignorDocumentaryAddress");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).Shipment.ConsignorDocumentaryAddress)));
		this.consignorDocumentaryAddress.BindToOrganisations = "Shipment.Lookups.ConsigneeForwarder_List";
		this.consignorDocumentaryAddress.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("cff752e9-a03f-4ac9-b7ca-2d2f5bb36a2c", "eTailer");
		this.consignorDocumentaryAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
		this.consignorDocumentaryAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 119, true);
		this.consignorDocumentaryAddress.Name = "consignorDocumentaryAddress";
		this.consignorDocumentaryAddress.ReadOnly = false;
		this.consignorDocumentaryAddress.SingleLineNoGroupBoxPanelWidth = 296;
		this.consignorDocumentaryAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
		this.consignorDocumentaryAddress.TabIndex = 4;
		this.consignorDocumentaryAddress.ValidationJustForced = false;
		// 
		// consigneeDocumentaryAddress
		// 
		this.consigneeDocumentaryAddress.AddressValidationProcessCmdKey = null;
		this.consigneeDocumentaryAddress.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.consigneeDocumentaryAddress, "Shipment.ConsigneeDocumentaryAddress");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).Shipment.ConsigneeDocumentaryAddress)));
		this.consigneeDocumentaryAddress.BindToOrganisations = "Shipment.Lookups.ConsignorForwarder_List";
		this.consigneeDocumentaryAddress.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("e8b28866-2156-44d5-839e-3862ea43ab8d", "Consignee");
		this.consigneeDocumentaryAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
		this.consigneeDocumentaryAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 179, true);
		this.consigneeDocumentaryAddress.Name = "consigneeDocumentaryAddress";
		this.consigneeDocumentaryAddress.ReadOnly = false;
		this.consigneeDocumentaryAddress.SingleLineNoGroupBoxPanelWidth = 296;
		this.consigneeDocumentaryAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
		this.consigneeDocumentaryAddress.TabIndex = 5;
		this.consigneeDocumentaryAddress.ValidationJustForced = false;
		// 
		// consignmentDetailsGroupBox
		// 
		this.consignmentDetailsGroupBox.Controls.Add(this.wayBillPrefixTextBox);
		this.consignmentDetailsGroupBox.Controls.Add(this.ItemLineCountCalcEdit);
		this.consignmentDetailsGroupBox.Controls.Add(this.ItemCountCalcEdit);
		this.consignmentDetailsGroupBox.Controls.Add(this.consignmentCountCalcEdit);
		this.consignmentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.consignmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.consignmentDetailsGroupBox.Name = "consignmentDetailsGroupBox";
		this.consignmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 196, true);
		this.consignmentDetailsGroupBox.TabIndex = 6;
		this.consignmentDetailsGroupBox.TabStop = false;
		this.consignmentDetailsGroupBox.CaptionResourceString = Res.GetData("E51A9A31-8D45-40B0-A138-73594CA8F6D6", "Consignment Details");
		// 
		// consignmentCountCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.consignmentCountCalcEdit, "ConsignmentCount");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).ConsignmentCount)));
		this.consignmentCountCalcEdit.DecimalPlaces = 2;
		this.consignmentCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 19, true);
		this.consignmentCountCalcEdit.Name = "consignmentCountCalcEdit";
		this.consignmentCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
		this.consignmentCountCalcEdit.TabIndex = 7;
		this.consignmentCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.consignmentCountCalcEdit.TrackDisposedAccess = true;
		this.consignmentCountCalcEdit.AllowNegative = false;
		// 
		// wayBillPrefixTextBox
		// 
		this.wayBillPrefixTextBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.wayBillPrefixTextBox, "WaybillPrefix");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).WaybillPrefix)));
		this.wayBillPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 68, true);
		this.wayBillPrefixTextBox.Name = "wayBillPrefixTextBox";
		this.wayBillPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
		this.wayBillPrefixTextBox.TabIndex = 8;
		// 
		// ItemCountCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.ItemCountCalcEdit, "ItemCount");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).ItemCount)));
		this.ItemCountCalcEdit.DecimalPlaces = 2;
		this.ItemCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 113, true);
		this.ItemCountCalcEdit.Name = "ItemCountCalcEdit";
		this.ItemCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
		this.ItemCountCalcEdit.TabIndex = 9;
		this.ItemCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.ItemCountCalcEdit.TrackDisposedAccess = true;
		this.ItemCountCalcEdit.AllowNegative = false;
		// 
		// ItemLineCountCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.ItemLineCountCalcEdit, "ItemLineCount");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVTestDataConfiguration)(null)).ItemLineCount)));
		this.ItemLineCountCalcEdit.DecimalPlaces = 2;
		this.ItemLineCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 159, true);
		this.ItemLineCountCalcEdit.Name = "ItemLineCountCalcEdit";
		this.ItemLineCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
		this.ItemLineCountCalcEdit.TabIndex = 10;
		this.ItemLineCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.ItemLineCountCalcEdit.TrackDisposedAccess = true;
		this.ItemCountCalcEdit.AllowNegative = false;
		// 
		// bottomPanel
		// 
		this.bottomPanel.Controls.Add(this.createButton);
		this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 441, true);
		this.bottomPanel.Name = "bottomPanel";
		this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 30, true);
		this.bottomPanel.TabIndex = 11;
		// 
		// createButton
		// 
		this.createButton.Dock = System.Windows.Forms.DockStyle.Right;
		this.createButton.IsCaptionOverridden = false;
		this.createButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 0, true);
		this.createButton.Name = "createButton";
		this.createButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 30, true);
		this.createButton.TabIndex = 12;
		this.createButton.CaptionResourceString = Res.GetData("E3F796F2-9612-4F76-A4F5-615B96F11417", "OK");
		this.createButton.ToolTipCaption = null;
		this.createButton.Click += new System.EventHandler(this.createButton_Click);
		// 
		// TestDataSettingForm
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 495, true);
		this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 495, true);
		this.Controls.Add(this.mainGroupBox);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		this.DataSourceType = typeof(Enterprise.eTail.Business.HVLVTestDataConfiguration);
		this.Name = "TestDataSettingForm";
		this.Controls.SetChildIndex(this.MainStatusBar, 0);
		this.Controls.SetChildIndex(this.mainGroupBox, 0);
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.mainGroupBox.ResumeLayout(false);
		this.mainGroupBox.PerformLayout();
		this.shipmentSplitContainer.Panel1.ResumeLayout(false);
		this.shipmentSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.shipmentSplitContainer)).EndInit();
		this.shipmentSplitContainer.ResumeLayout(false);
		this.shipmentSplitContainer.PerformLayout();
		this.shipmentDetailsGroupBox.ResumeLayout(false);
		this.shipmentDetailsGroupBox.PerformLayout();
		this.consigneeDocumentaryAddress.ResumeLayout(true);
		this.consigneeDocumentaryAddress.PerformLayout();
		this.consignorDocumentaryAddress.ResumeLayout(true);
		this.consignorDocumentaryAddress.PerformLayout();
		this.consignmentDetailsGroupBox.ResumeLayout(false);
		this.consignmentDetailsGroupBox.PerformLayout();
		this.bottomPanel.ResumeLayout(false);
		this.bottomPanel.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private Enterprise.ZArchitecture.GUI.ZPanel mainGroupBox;
	private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
	private CargoWise.Windows.UI.KSplitContainer shipmentSplitContainer;
	private Enterprise.ZArchitecture.GUI.ZGroupBox shipmentDetailsGroupBox;
	private Enterprise.ZArchitecture.GUI.ZGroupBox consignmentDetailsGroupBox;
	private ZArchitecture.ZCalcEdit shipmentCountCalcEdit;
	private ZButton createButton;
	private ZDocAddressControl consignorDocumentaryAddress;
	private ZDocAddressControl consigneeDocumentaryAddress;
	private ZArchitecture.ZCalcEdit consignmentCountCalcEdit;
	private ZArchitecture.ZCalcEdit ItemCountCalcEdit;
	private ZArchitecture.ZTextBox wayBillPrefixTextBox;
	private ZArchitecture.ZCalcEdit ItemLineCountCalcEdit;
}
