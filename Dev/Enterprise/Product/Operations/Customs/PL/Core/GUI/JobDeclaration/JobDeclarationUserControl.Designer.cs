using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	public partial class JobDeclarationUserControl
	{
		void InitializeComponent()
		{
			this.BorderTransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CTStatusIDDropEdit.SuspendLayout();
			this.ImporterDocAddress.SuspendLayout();
			this.SupplierDocAddress.SuspendLayout();
			this.StyleOfEntrySOEDropDown.SuspendLayout();
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
			this.OrganizationExportUserControl.SuspendLayout();
			this.OrganizationImportUserControl.SuspendLayout();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
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
			this.BorderTransportMeansDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CTStatusIDDropEdit
			// 
			this.CTStatusIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 188, true);
			this.CTStatusIDDropEdit.TabIndex = 8;
			// 
			// SpecificCircumstanceDropEdit
			// 
			this.SpecificCircumstanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 210, true);
			this.SpecificCircumstanceDropEdit.TabIndex = 9;
			// 
			// OrganisationsOuterPanel
			// 
			this.OrganisationsOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 275, true);
			// 
			// OrganisationsPanel
			// 
			this.OrganisationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 275, true);
			// 
			// CustomsOfficesUserControl
			// 
			this.CustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 690, true);
			this.CustomsOfficesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 140, true);
			this.CustomsOfficesUserControl.UserControlType = typeof(Enterprise.Customs.PL.GUI.PLCustomsOfficesUserControl);
			// 
			// OrganizationExportUserControl
			// 
			this.OrganizationExportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 275, true);
			this.OrganizationExportUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.ExportOrganizationUserControl);
			// 
			// OrganizationImportUserControl
			// 
			this.OrganizationImportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 275, true);
			this.OrganizationImportUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.ImportOrganizationUserControl);
			// 
			// IsHighValueOvrdCheckBox
			// 
			this.IsHighValueOvrdCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 58, true);
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 76, true);
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 144, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 6;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLJobDeclarationUserControl|8B4377F3-817C-416D-8ACC-2345C1AE556A", "Trans.", "Transport ID", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FolioNumberTextBox, true);
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 64, true);
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.FolioNumberTextBox.TabIndex = 4;
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.BorderTransportMeansDropEdit);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.CTStatusIDDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.SpecificCircumstanceDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.IsHighValueOvrdCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.BorderTransportMeansDropEdit, 0);
			// 
			// RightTabControl
			// 
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 896, true);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 206, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 166, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 7;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 16, true);
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 873, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 418, true);
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 232, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// BorderTransportMeansDropEdit
			// 
			this.BorderTransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportMeansDropEdit, "ZG_BorderTransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).ZG_BorderTransportMeans)));
			this.BorderTransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 122, true);
			this.BorderTransportMeansDropEdit.Name = "BorderTransportMeansDropEdit";
			this.BorderTransportMeansDropEdit.PreBoundMaxLength = 2;
			this.BorderTransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.BorderTransportMeansDropEdit.TabIndex = 5;
			// 
			// JobDeclarationUserControl
			// 
			this.Name = "JobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 966, true);
			this.CTStatusIDDropEdit.ResumeLayout(true);
			this.CTStatusIDDropEdit.PerformLayout();
			this.ImporterDocAddress.ResumeLayout(true);
			this.ImporterDocAddress.PerformLayout();
			this.SupplierDocAddress.ResumeLayout(true);
			this.SupplierDocAddress.PerformLayout();
			this.StyleOfEntrySOEDropDown.ResumeLayout(true);
			this.StyleOfEntrySOEDropDown.PerformLayout();
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
			this.OrganizationExportUserControl.ResumeLayout(true);
			this.OrganizationExportUserControl.PerformLayout();
			this.OrganizationImportUserControl.ResumeLayout(true);
			this.OrganizationImportUserControl.PerformLayout();
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
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
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
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
			this.BorderTransportMeansDropEdit.ResumeLayout(true);
			this.BorderTransportMeansDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.GUI.ZDropEdit BorderTransportMeansDropEdit;
	}
}
