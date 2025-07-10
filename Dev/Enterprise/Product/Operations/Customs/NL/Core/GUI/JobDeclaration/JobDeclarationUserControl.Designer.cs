using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class JobDeclarationUserControl
	{
		private void InitializeComponent()
		{
			this.PresentationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PresentationEndDate = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.PresentationStartDate = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.DeclarationDetailsUserControl = new Enterprise.Customs.NL.GUI.DeclarationDetailsUserControl();
			this.CTStatusIDDropEdit.SuspendLayout();
			this.BadgeCodeDropEdit.SuspendLayout();
			this.GatewayDropEdit.SuspendLayout();
			this.ImporterDocAddress.SuspendLayout();
			this.SupplierDocAddress.SuspendLayout();
			this.StyleOfEntrySOEDropDown.SuspendLayout();
			this.ZG_AgreedPlaceCodeDropEdit.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.Box18TrNationalityFindBox.SuspendLayout();
			this.JE_IATALoadPortCodeFindBox.SuspendLayout();
			this.PortOfFirstArrivalFindBox.SuspendLayout();
			this.JE_DateOfFirstArrivalBoundDateEdit.SuspendLayout();
			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.TransportNationalityFindBox.SuspendLayout();
			this.EarliestCustomsIssueDateEdit.SuspendLayout();
			this.SpecificCircumstanceDropEdit.SuspendLayout();
			this.OrganisationsOuterPanel.SuspendLayout();
			this.OrganisationsPanel.SuspendLayout();
			this.CustomsOfficesUserControl.SuspendLayout();
			this.GoodsDestinationDropEdit.SuspendLayout();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.OrganizationExportUserControl.SuspendLayout();
			this.RegionOfDestinationDropEdit.SuspendLayout();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.FinalDestinationFindBox.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit2.SuspendLayout();
			this.JE_ExportDateBoundDateEdit2.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ShipmentCustomFieldsPage.SuspendLayout();
			this.shipmentCustomFieldsControl1.SuspendLayout();
			this.OrdersPanel.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.OrdersAttachUserControl.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PresentationGroupBox.SuspendLayout();
			this.PresentationEndDate.SuspendLayout();
			this.PresentationStartDate.SuspendLayout();
			this.DeclarationDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BadgeCodeDropEdit
			// 
			this.BadgeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 309, true);
			// 
			// GatewayDropEdit
			// 
			this.GatewayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 309, true);
			// 
			// ImporterDocAddress
			// 
			this.ImporterDocAddress.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("46DAB23A-84A3-475C-9583-205AB2070E75", "[UCC 3/15] Importer");
			// 
			// SupplierDocAddress
			// 
			this.SupplierDocAddress.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("E309C37E-FB2E-4810-9B8C-C55EF201A85A", "[UCC 3/7] Supplier");
			// 
			// StyleOfEntrySOEDropDown
			// 
			this.StyleOfEntrySOEDropDown.Visible = false;
			// 
			// ZG_AgreedPlaceCodeDropEdit
			// 
			this.ZG_AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 214, true);
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 14, true);
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.GoodsLocationDropEdit.Visible = false;
			// 
			// JE_ShipmentIncoTermPlaceTextBox
			// 
			this.JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 214, true);
			// 
			// AgentsReference
			// 
			this.AgentsReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 261, true);
			// 
			// JE_UCRTextBox
			// 
			this.JE_UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 285, true);
			// 
			// EarliestCustomsIssueDateEdit
			// 
			this.EarliestCustomsIssueDateEdit.Visible = false;
			// 
			// OrganisationsOuterPanel
			// 
			this.OrganisationsOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 275, true);
			// 
			// OrganisationsPanel
			// 
			this.OrganisationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 275, true);
			// 
			// CustomsOfficesUserControl
			// 
			this.CustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 556, true);
			this.CustomsOfficesUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.CustomsOfficesUserControl);
			// 
			// OrganizationExportUserControl
			// 
			this.OrganizationExportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 275, true);
			this.OrganizationExportUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.ExportOrganizationUserControl);
			// 
			// OrganizationImportUserControl
			// 
			this.OrganizationImportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 275, true);
			this.OrganizationImportUserControl.UserControlType = typeof(Enterprise.Customs.NL.GUI.ImportOrganizationUserControl);
			// 
			// IsHighValueOvrdCheckBox
			// 
			this.IsHighValueOvrdCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 191, true);
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 167, true);
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 332, true);
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 332, true);
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 169, true);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 207, true);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.RegionOfDestinationDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.AgentsReference, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsLocationDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.BadgeCodeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GatewayDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsOriginDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDestinationDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ZG_AgreedPlaceCodeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ShipmentIncoTermPlaceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_UCRTextBox, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 286, true);
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 191, true);
			// 
			// RightTabControl
			// 
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(727, 214, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 620, true);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 206, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 235, true);
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 235, true);
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 189, true);
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 214, true);
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 593, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 418, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 8, true);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.Visible = false;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("247eaff8-bfd4-4c55-b087-09f62348f818", "Entry Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.DeclarationDetailsUserControl);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 8, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 203, true);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DeclarationDetailsUserControl, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.EarliestCustomsIssueDateEdit, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StyleOfEntrySOEDropDown, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// PresentationGroupBox
			// 
			this.PresentationGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("b76a72b3-fd80-44b9-8e3d-d47480b839fe", "Presentation");
			this.PresentationGroupBox.Controls.Add(this.PresentationEndDate);
			this.PresentationGroupBox.Controls.Add(this.PresentationStartDate);
			this.PresentationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 786, true);
			this.PresentationGroupBox.Name = "PresentationGroupBox";
			this.PresentationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 47, true);
			this.PresentationGroupBox.TabIndex = 8;
			this.PresentationGroupBox.TabStop = false;
			// 
			// PresentationEndDate
			// 
			this.PresentationEndDate.AllowDrop = true;
			this.PresentationEndDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationEndDate, "ZG_PresentationEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).ZG_PresentationEndDate)));
			this.PresentationEndDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationEndDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 16, true);
			this.PresentationEndDate.Name = "PresentationEndDate";
			this.PresentationEndDate.TabIndex = 1;
			// 
			// PresentationStartDate
			// 
			this.PresentationStartDate.AllowDrop = true;
			this.PresentationStartDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationStartDate, "ZG_PresentationStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).ZG_PresentationStartDate)));
			this.PresentationStartDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationStartDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 16, true);
			this.PresentationStartDate.Name = "PresentationStartDate";
			this.PresentationStartDate.TabIndex = 0;
			// 
			// DeclarationDetailsUserControl
			// 
			this.DeclarationDetailsUserControl.AllowDrop = true;
			this.DeclarationDetailsUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeclarationDetailsUserControl, ".");
			this.DeclarationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeclarationDetailsUserControl.Name = "DeclarationDetailsUserControl";
			this.DeclarationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 184, true);
			this.DeclarationDetailsUserControl.TabIndex = 9;
			// 
			// JobDeclarationUserControl
			// 
			this.Controls.Add(this.PresentationGroupBox);
			this.Name = "JobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 800, true);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.ImporterDocAddress, 0);
			this.Controls.SetChildIndex(this.SupplierDocAddress, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.CustomsOfficesUserControl, 0);
			this.Controls.SetChildIndex(this.PresentationGroupBox, 0);
			this.CTStatusIDDropEdit.ResumeLayout(true);
			this.CTStatusIDDropEdit.PerformLayout();
			this.BadgeCodeDropEdit.ResumeLayout(true);
			this.BadgeCodeDropEdit.PerformLayout();
			this.GatewayDropEdit.ResumeLayout(true);
			this.GatewayDropEdit.PerformLayout();
			this.ImporterDocAddress.ResumeLayout(true);
			this.ImporterDocAddress.PerformLayout();
			this.SupplierDocAddress.ResumeLayout(true);
			this.SupplierDocAddress.PerformLayout();
			this.StyleOfEntrySOEDropDown.ResumeLayout(true);
			this.StyleOfEntrySOEDropDown.PerformLayout();
			this.ZG_AgreedPlaceCodeDropEdit.ResumeLayout(true);
			this.ZG_AgreedPlaceCodeDropEdit.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.Box18TrNationalityFindBox.ResumeLayout(true);
			this.Box18TrNationalityFindBox.PerformLayout();
			this.JE_IATALoadPortCodeFindBox.ResumeLayout(true);
			this.JE_IATALoadPortCodeFindBox.PerformLayout();
			this.PortOfFirstArrivalFindBox.ResumeLayout(true);
			this.PortOfFirstArrivalFindBox.PerformLayout();
			this.JE_DateOfFirstArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfFirstArrivalBoundDateEdit.PerformLayout();
			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.TransportNationalityFindBox.ResumeLayout(true);
			this.TransportNationalityFindBox.PerformLayout();
			this.EarliestCustomsIssueDateEdit.ResumeLayout(true);
			this.EarliestCustomsIssueDateEdit.PerformLayout();
			this.SpecificCircumstanceDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceDropEdit.PerformLayout();
			this.OrganisationsOuterPanel.ResumeLayout(false);
			this.OrganisationsOuterPanel.PerformLayout();
			this.OrganisationsPanel.ResumeLayout(false);
			this.OrganisationsPanel.PerformLayout();
			this.CustomsOfficesUserControl.ResumeLayout(true);
			this.CustomsOfficesUserControl.PerformLayout();
			this.GoodsDestinationDropEdit.ResumeLayout(true);
			this.GoodsDestinationDropEdit.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.OrganizationExportUserControl.ResumeLayout(true);
			this.OrganizationExportUserControl.PerformLayout();
			this.RegionOfDestinationDropEdit.ResumeLayout(true);
			this.RegionOfDestinationDropEdit.PerformLayout();
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.JE_MasterBillForAirBoundTextBox.ResumeLayout(true);
			this.JE_MasterBillForAirBoundTextBox.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.JE_ExportDateBoundDateEdit.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit2.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit2.PerformLayout();
			this.JE_ExportDateBoundDateEdit2.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit2.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.OrganisationsTopPanel.ResumeLayout(false);
			this.OrganisationsTopPanel.PerformLayout();
			this.ShippingOrAirLineOrganisationControl.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ShipmentCustomFieldsPage.ResumeLayout(false);
			this.ShipmentCustomFieldsPage.PerformLayout();
			this.shipmentCustomFieldsControl1.ResumeLayout(true);
			this.shipmentCustomFieldsControl1.PerformLayout();
			this.OrdersPanel.ResumeLayout(false);
			this.OrdersPanel.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.OrdersAttachUserControl.ResumeLayout(true);
			this.OrdersAttachUserControl.PerformLayout();
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PresentationGroupBox.ResumeLayout(false);
			this.PresentationGroupBox.PerformLayout();
			this.PresentationEndDate.ResumeLayout(true);
			this.PresentationEndDate.PerformLayout();
			this.PresentationStartDate.ResumeLayout(true);
			this.PresentationStartDate.PerformLayout();
			this.DeclarationDetailsUserControl.ResumeLayout(true);
			this.DeclarationDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZGroupBox PresentationGroupBox;
		ZDateTimeOffsetEdit PresentationEndDate;
		ZDateTimeOffsetEdit PresentationStartDate;
		internal DeclarationDetailsUserControl DeclarationDetailsUserControl;
	}
}
