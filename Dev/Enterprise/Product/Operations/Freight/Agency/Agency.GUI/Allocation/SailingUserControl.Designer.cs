using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class SailingUserControl
	{
		private void InitializeComponent()
		{
			this.clearSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.selectSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.SailingSummaryTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BookedShippingLineBoundOrgFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.loadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DEPBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ARVBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VesselBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_OH_DeliveryAgentBoundOrgFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.ImportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JX_StorageDate_ReadOnlyBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RecievalStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RecievalEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.allocationSummaryTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AllocationSummaryControl = new Enterprise.Freight.Agency.GUI.ShipmentAllocationUsageControl();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.editSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingTabControl.SuspendLayout();
			this.SailingSummaryTab.SuspendLayout();
			this.ImportPanel.SuspendLayout();
			this.ExportPanel.SuspendLayout();
			this.allocationSummaryTab.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipment);
			// 
			// clearSailingButton
			// 
			this.clearSailingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearSailingButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|774a64ef-a166-4df5-b331-2eaa235ec7e0", "Clear Sailing");
			this.clearSailingButton.AutoEllipsis = false;
			this.clearSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 102, true);
			this.clearSailingButton.Name = "clearSailingButton";
			this.clearSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.clearSailingButton.TabIndex = 3;
			this.clearSailingButton.TabStop = false;
			this.clearSailingButton.Click += new System.EventHandler(this.ClearSailingButton_Click);
			// 
			// createSailingButton
			// 
			this.createSailingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createSailingButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|80758b42-a5eb-47a6-bd6f-5e4e9818b07a", "Create Sailing");
			this.createSailingButton.AutoEllipsis = false;
			this.createSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 30, true);
			this.createSailingButton.Name = "createSailingButton";
			this.createSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.createSailingButton.TabIndex = 0;
			this.createSailingButton.TabStop = false;
			this.createSailingButton.Click += new System.EventHandler(this.CreateSailingButton_Click);
			// 
			// selectSailingButton
			// 
			this.selectSailingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectSailingButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|d3d3ac92-7d6a-488f-b0b4-083379e433c7", "Select Sailing");
			this.selectSailingButton.AutoEllipsis = false;
			this.selectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 54, true);
			this.selectSailingButton.Name = "selectSailingButton";
			this.selectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.selectSailingButton.TabIndex = 1;
			this.selectSailingButton.TabStop = false;
			this.selectSailingButton.Click += new System.EventHandler(this.SelectSailingButton_Click);
			// 
			// SailingTabControl
			// 
			this.SailingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SailingTabControl.Controls.Add(this.SailingSummaryTab);
			this.SailingTabControl.Controls.Add(this.allocationSummaryTab);
			this.SailingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SailingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingTabControl.Name = "SailingTabControl";
			this.SailingTabControl.SelectedIndex = 0;
			this.SailingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 153, true);
			this.SailingTabControl.TabIndex = 0;
			// 
			// SailingSummaryTab
			// 
			this.SailingSummaryTab.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|6c6df7f0-87f3-4853-af63-9cc7a48a1d6e", "Sailing Summary");
			this.SailingSummaryTab.Controls.Add(this.BookedShippingLineBoundOrgFindBox);
			this.SailingSummaryTab.Controls.Add(this.loadPortCodeFindBox);
			this.SailingSummaryTab.Controls.Add(this.dischargePortCodeFindBox);
			this.SailingSummaryTab.Controls.Add(this.DEPBoundReadOnlyDateEdit);
			this.SailingSummaryTab.Controls.Add(this.ARVBoundReadOnlyDateEdit);
			this.SailingSummaryTab.Controls.Add(this.VesselBoundTextBox);
			this.SailingSummaryTab.Controls.Add(this.VoyageNumberBoundTextBox);
			this.SailingSummaryTab.Controls.Add(this.JS_OH_DeliveryAgentBoundOrgFindBox);
			this.SailingSummaryTab.Controls.Add(this.ImportPanel);
			this.SailingSummaryTab.Controls.Add(this.ExportPanel);
			this.SailingSummaryTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SailingSummaryTab.Name = "SailingSummaryTab";
			this.SailingSummaryTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 126, true);
			this.SailingSummaryTab.TabIndex = 0;
			// 
			// BookedShippingLineBoundOrgFindBox
			// 
			this.BookedShippingLineBoundOrgFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookedShippingLineBoundOrgFindBox, "BookedShippingLinePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).BookedShippingLinePK)));
			this.BookedShippingLineBoundOrgFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("35d41eae-710e-4ebe-a0ba-56c3627f66ce", "Carrier:");
			this.BookedShippingLineBoundOrgFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BookedShippingLineBoundOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 101, true);
			this.BookedShippingLineBoundOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.BookedShippingLineBoundOrgFindBox.Name = "BookedShippingLineBoundOrgFindBox";
			this.BookedShippingLineBoundOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BookedShippingLineBoundOrgFindBox.TabIndex = 14;
			// 
			// loadPortCodeFindBox
			// 
			this.loadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.loadPortCodeFindBox, "JS_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_NKLoadPort)));
			this.loadPortCodeFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|d5b8a16b-1262-4ec7-9c15-24aac64ae006", "Load", "The port where this shipment is loaded onto the main vessel.");
			this.loadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 9, true);
			this.loadPortCodeFindBox.Name = "loadPortCodeFindBox";
			this.loadPortCodeFindBox.PopupCaption = "Select Load Port";
			this.loadPortCodeFindBox.PreBoundMaxLength = 5;
			this.loadPortCodeFindBox.ShowDescriptionBox = false;
			this.loadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.loadPortCodeFindBox.TabIndex = 1;
			// 
			// dischargePortCodeFindBox
			// 
			this.dischargePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dischargePortCodeFindBox, "JS_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_NKDischargePort)));
			this.dischargePortCodeFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|a8247b26-a3d6-489d-9dbf-cb4b05134435", "Disch.", "Discharge", "Discharge", "The port where this shipment discharges from the main vessel.");
			this.dischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 9, true);
			this.dischargePortCodeFindBox.Name = "dischargePortCodeFindBox";
			this.dischargePortCodeFindBox.PopupCaption = "Select Discharge Port";
			this.dischargePortCodeFindBox.PreBoundMaxLength = 5;
			this.dischargePortCodeFindBox.ShowDescriptionBox = false;
			this.dischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.dischargePortCodeFindBox.TabIndex = 3;
			// 
			// DEPBoundReadOnlyDateEdit
			// 
			this.DEPBoundReadOnlyDateEdit.AllowDrop = true;
			this.DEPBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.DEPBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DEPBoundReadOnlyDateEdit, "Sailings.JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JA_E_DEP)));
			this.DEPBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|ee46464f-4b4d-4af8-a1fe-3ffd4643cd52", "ETD", "Estimated Departure Date.");
			this.DEPBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DEPBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 78, true);
			this.DEPBoundReadOnlyDateEdit.Name = "DEPBoundReadOnlyDateEdit";
			this.DEPBoundReadOnlyDateEdit.TabIndex = 9;
			// 
			// ARVBoundReadOnlyDateEdit
			// 
			this.ARVBoundReadOnlyDateEdit.AllowDrop = true;
			this.ARVBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.ARVBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ARVBoundReadOnlyDateEdit, "Sailings.JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JB_E_ARV)));
			this.ARVBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|177d69d1-b9b8-4d87-82c7-601c5f8602d3", "ETA", "Estimated Arrival Date.");
			this.ARVBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ARVBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 78, true);
			this.ARVBoundReadOnlyDateEdit.Name = "ARVBoundReadOnlyDateEdit";
			this.ARVBoundReadOnlyDateEdit.TabIndex = 11;
			// 
			// VesselBoundTextBox
			// 
			this.VesselBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VesselBoundTextBox, "VoyageVesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).VoyageVesselForBinding)));
			this.VesselBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|41bbaaa2-2836-4756-bd37-ccf2516f08f1", "Vessel Name", "Vessel.");
			this.VesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 32, true);
			this.VesselBoundTextBox.Name = "VesselBoundTextBox";
			this.VesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.VesselBoundTextBox.TabIndex = 7;
			// 
			// VoyageNumberBoundTextBox
			// 
			this.VoyageNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VoyageNumberBoundTextBox, "Sailings.JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JV_VoyageFlight)));
			this.VoyageNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|64db8955-6464-413c-aab9-e648a94a9350", "Voyage No.", "Voyage.");
			this.VoyageNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			this.VoyageNumberBoundTextBox.Name = "VoyageNumberBoundTextBox";
			this.VoyageNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.VoyageNumberBoundTextBox.TabIndex = 5;
			// 
			// JS_OH_DeliveryAgentBoundOrgFindBox
			// 
			this.JS_OH_DeliveryAgentBoundOrgFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_OH_DeliveryAgentBoundOrgFindBox, "JS_OH_DeliveryAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_OH_DeliveryAgent)));
			this.JS_OH_DeliveryAgentBoundOrgFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|03b5ddd8-3edd-4905-a8e0-14388813a899", "Principal");
			this.JS_OH_DeliveryAgentBoundOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 101, true);
			this.JS_OH_DeliveryAgentBoundOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.JS_OH_DeliveryAgentBoundOrgFindBox.Name = "JS_OH_DeliveryAgentBoundOrgFindBox";
			this.JS_OH_DeliveryAgentBoundOrgFindBox.ShowDescriptionBox = false;
			this.JS_OH_DeliveryAgentBoundOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_OH_DeliveryAgentBoundOrgFindBox.TabIndex = 16;
			// 
			// ImportPanel
			// 
			this.ImportPanel.Controls.Add(this.JX_StorageDate_ReadOnlyBoundDateEdit);
			this.ImportPanel.Controls.Add(this.JX_AvailabilityDate_ReadOnlyBoundDateEdit);
			this.ImportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.ImportPanel.Name = "ImportPanel";
			this.ImportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 24, true);
			this.ImportPanel.TabIndex = 4;
			this.ImportPanel.Visible = false;
			// 
			// JX_StorageDate_ReadOnlyBoundDateEdit
			// 
			this.JX_StorageDate_ReadOnlyBoundDateEdit.AllowDrop = true;
			this.JX_StorageDate_ReadOnlyBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_StorageDate_ReadOnlyBoundDateEdit.AutoCompleteYear = true;
			this.JX_StorageDate_ReadOnlyBoundDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JX_StorageDate_ReadOnlyBoundDateEdit, "Sailings.JX_JB_CTOStorageDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JB_CTOStorageDate)));
			this.JX_StorageDate_ReadOnlyBoundDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|9d874688-f210-4a8c-9a79-1f132e08b17d", "Storage Date", "The date the CTO starts to charge demurrage.");
			this.JX_StorageDate_ReadOnlyBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JX_StorageDate_ReadOnlyBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 3, true);
			this.JX_StorageDate_ReadOnlyBoundDateEdit.Name = "JX_StorageDate_ReadOnlyBoundDateEdit";
			this.JX_StorageDate_ReadOnlyBoundDateEdit.TabIndex = 3;
			// 
			// JX_AvailabilityDate_ReadOnlyBoundDateEdit
			// 
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.AllowDrop = true;
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JX_AvailabilityDate_ReadOnlyBoundDateEdit, "Sailings.JX_JB_CTOAvailabilityDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JB_CTOAvailabilityDate)));
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|69ac6509-0d39-479e-8f2c-48c38db8f6d5", "Availability Date", "The date the goods will be made available from the CTO.");
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 3, true);
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.Name = "JX_AvailabilityDate_ReadOnlyBoundDateEdit";
			this.JX_AvailabilityDate_ReadOnlyBoundDateEdit.TabIndex = 1;
			// 
			// ExportPanel
			// 
			this.ExportPanel.Controls.Add(this.RecievalStartDateEdit);
			this.ExportPanel.Controls.Add(this.RecievalEndDateEdit);
			this.ExportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.ExportPanel.Name = "ExportPanel";
			this.ExportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 24, true);
			this.ExportPanel.TabIndex = 4;
			this.ExportPanel.Visible = false;
			// 
			// RecievalStartDateEdit
			// 
			this.RecievalStartDateEdit.AllowDrop = true;
			this.RecievalStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.RecievalStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RecievalStartDateEdit, "Sailings.JX_JA_CTOReceivalCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JA_CTOReceivalCommences)));
			this.RecievalStartDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|7c94df13-dff5-4088-a2cb-70e2b7d0adee", "Receival Start Date");
			this.RecievalStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RecievalStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 2, true);
			this.RecievalStartDateEdit.Name = "RecievalStartDateEdit";
			this.RecievalStartDateEdit.TabIndex = 1;
			// 
			// RecievalEndDateEdit
			// 
			this.RecievalEndDateEdit.AllowDrop = true;
			this.RecievalEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.RecievalEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RecievalEndDateEdit, "Sailings.JX_JA_CTOCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).Sailings)).SyncRoot)).JX_JA_CTOCutOff)));
			this.RecievalEndDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|7f6a2cef-a32d-4600-aec1-7a1344388b52", "Receival Cut Off");
			this.RecievalEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RecievalEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 3, true);
			this.RecievalEndDateEdit.Name = "RecievalEndDateEdit";
			this.RecievalEndDateEdit.TabIndex = 3;
			// 
			// allocationSummaryTab
			// 
			this.allocationSummaryTab.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|6542a6ac-9eb6-4bf1-a22b-36fd2e064daa", "Allocation Summary");
			this.allocationSummaryTab.Controls.Add(this.AllocationSummaryControl);
			this.allocationSummaryTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.allocationSummaryTab.Name = "allocationSummaryTab";
			this.allocationSummaryTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 126, true);
			this.allocationSummaryTab.TabIndex = 1;
			// 
			// AllocationSummaryControl
			// 
			this.AllocationSummaryControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AllocationSummaryControl, ".");
			this.AllocationSummaryControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocationSummaryControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocationSummaryControl.Name = "AllocationSummaryControl";
			this.AllocationSummaryControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 126, true);
			this.AllocationSummaryControl.TabIndex = 0;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.editSailingButton);
			this.ButtonPanel.Controls.Add(this.clearSailingButton);
			this.ButtonPanel.Controls.Add(this.createSailingButton);
			this.ButtonPanel.Controls.Add(this.selectSailingButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 0, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 153, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// editSailingButton
			// 
			this.editSailingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.editSailingButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingUserControl|2bf481f9-c251-48ee-81fd-c8f466ed6074", "Edit Sailing");
			this.editSailingButton.AutoEllipsis = false;
			this.editSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 78, true);
			this.editSailingButton.Name = "editSailingButton";
			this.editSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.editSailingButton.TabIndex = 2;
			this.editSailingButton.TabStop = false;
			this.editSailingButton.Click += new System.EventHandler(this.EditSailingButton_Click);
			// 
			// SailingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingTabControl);
			this.Controls.Add(this.ButtonPanel);
			this.Name = "SailingUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 153, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingTabControl.ResumeLayout(false);
			this.SailingSummaryTab.ResumeLayout(false);
			this.SailingSummaryTab.PerformLayout();
			this.ImportPanel.ResumeLayout(false);
			this.ImportPanel.PerformLayout();
			this.ExportPanel.ResumeLayout(false);
			this.allocationSummaryTab.ResumeLayout(false);
			this.ButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZButton clearSailingButton;
		Enterprise.ZArchitecture.GUI.ZButton createSailingButton;
		Enterprise.ZArchitecture.GUI.ZButton selectSailingButton;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl SailingTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage allocationSummaryTab;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		Enterprise.ZArchitecture.GUI.ZButton editSailingButton;
		Enterprise.ZArchitecture.GUI.ZTabPage SailingSummaryTab;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox loadPortCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox dischargePortCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit DEPBoundReadOnlyDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit ARVBoundReadOnlyDateEdit;
		Enterprise.ZArchitecture.ZTextBox VesselBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox VoyageNumberBoundTextBox;
		Enterprise.MasterFiles.GUI.ZOrganisationFindBox BookedShippingLineBoundOrgFindBox;
		Enterprise.MasterFiles.GUI.ZOrganisationFindBox JS_OH_DeliveryAgentBoundOrgFindBox;
		Enterprise.ZArchitecture.GUI.ZPanel ImportPanel;
		Enterprise.ZArchitecture.GUI.ZDateEdit JX_StorageDate_ReadOnlyBoundDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit JX_AvailabilityDate_ReadOnlyBoundDateEdit;
		Enterprise.ZArchitecture.GUI.ZPanel ExportPanel;
		Enterprise.ZArchitecture.GUI.ZDateEdit RecievalStartDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit RecievalEndDateEdit;
		ShipmentAllocationUsageControl AllocationSummaryControl;
	}
}
