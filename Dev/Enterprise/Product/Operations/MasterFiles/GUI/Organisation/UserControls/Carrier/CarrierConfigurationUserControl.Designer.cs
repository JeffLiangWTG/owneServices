using System;
using System.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	partial class CarrierConfigurationUserControl
	{
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsLineHaulProviderBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsSeaWholesalerBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsAirWholesalerBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsRailProviderBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsAirLineBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsShippingLineBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsLocalTransportBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsInlandWaterwayProviderCheckEdit;
		protected ZGroupBox CarrierTypeGroupBox;
		protected ZGroupBox AirGroupBox;
		protected ZGroupBox SeaGroupBox;
		protected ZGroupBox LandGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit OM_CRCarrierCategoryBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OB_CRIsShipsAgencyPrincipalBoundCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox VesselConsortCheckBox;
		ZGroupBox zCarrierPortsGroupBox;
		Enterprise.ZArchitecture.ZGrid AppointedCarrierPortszGrid;
		ZPanel CarrierTypePanel;
		ZPanel DepotDetailsPanel;
		ZPanel IATAServicePanel;
		ZTabControl DetailsTabControl;
		ZTabPage IATATabPage;
		Enterprise.ZArchitecture.ZTextBox AirlineAccountNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_ThreeLetterCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineCountryBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlinePostalCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AddressLine1BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AddressLine2BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineName1BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_TwoCharacterCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineStateBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineCityBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineName2BoundTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox AirlineNumericCodeFindBox;
		ZTabPage ServiceLevelTabPage;
		ZPanel ServiceLevelsPanel;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel MAWBStockManagementLabel;
		Enterprise.ZArchitecture.ZGrid ServiceLevelGrid;
		ZPanel AccountNumbersPanel;
		ZTabPage NamedAccountClientsTabPage;
		Enterprise.MasterFiles.GUI.OrgCarrierNamedAccountUserControl OrgCarrierNamedAccountUserControl;
		ZPanel MAWBStockManagementPanel;
		ZTabControl AgentTabControl;
		ZTabPage StevedoreTabPage;
		ZTabPage AirTabPage;
		ZTabPage RailHeadTabPage;
		ZTabPage RoadDepotTabPage;
		ZTabPage ContainerYardTabPage;
		ZTabPage PenaltiesTabPage;
		ZTabPage agenciesTabPage;
		Enterprise.ZArchitecture.ZGrid AirCTOGrid;
		Enterprise.ZArchitecture.ZGrid RailHeadDepotGrid;
		Enterprise.ZArchitecture.ZGrid RoadDepotShedGrid;
		ZPanel ContainerYardParkPanel;
		Enterprise.ZArchitecture.ZGrid CYPGrid;
		Enterprise.ZArchitecture.ZGrid CYPTypesGrid;
		Enterprise.ZArchitecture.ZGrid agencyGrid;
		private CargoWise.Windows.UI.KSplitContainer CYSplitContainer;
		IContainer components;
		ZTabPage AirlineTabPage;
		ZTabControl AirlineTabControl;
		ZTabPage AirlineAccountNumbersTabPage;
		ZPanel AirlineAccountNumbersPanel;
		ZGroupBox AirlineAccountNumberGroupBox;
		ZGrid AirlineAccountNumberGrid;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CarrierTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SeaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LandGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VesselConsortCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_CRCarrierCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OH_IsLineHaulProviderBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsSeaWholesalerBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsAirWholesalerBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsRailProviderBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsAirLineBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsShippingLineBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsLocalTransportBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsInlandWaterwayProviderCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCarrierPortsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AgentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.StevedoreTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RailHeadTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RoadDepotTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerYardTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PenaltiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.agenciesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CarrierTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IATAServicePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AirlineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ServiceLevelTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RefShippingLineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccountNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NamedAccountClientsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepotDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierTypeGroupBox.SuspendLayout();
			this.OM_CRCarrierCategoryBoundDropEdit.SuspendLayout();
			this.zCarrierPortsGroupBox.SuspendLayout();
			this.AgentTabControl.SuspendLayout();
			this.CarrierTypePanel.SuspendLayout();
			this.IATAServicePanel.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.DepotDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			//
			// CarrierTypeGroupBox
			//
			this.CarrierTypeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|e5d9dfae-967a-4d6f-89af-fe0d36f28b8d", "Carrier Type");
			this.CarrierTypeGroupBox.Controls.Add(this.AirGroupBox);
			this.CarrierTypeGroupBox.Controls.Add(this.SeaGroupBox);
			this.CarrierTypeGroupBox.Controls.Add(this.LandGroupBox);

			this.AirGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|C05ED98E-65AF-4380-A0C1-56DA287E9415", "Air");
			this.AirGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 50, true);
			this.AirGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 65, true);
			this.AirGroupBox.Controls.Add(this.OH_IsAirLineBoundCheckEdit);
			this.AirGroupBox.Controls.Add(this.OH_IsAirWholesalerBoundCheckEdit);
			this.AirGroupBox.TabIndex = 1;

			this.SeaGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|552E0DA9-6FCD-4FA2-A592-FD53048FC888", "Sea");
			this.SeaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 125, true);
			this.SeaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 103, true);
			this.SeaGroupBox.Controls.Add(this.OH_IsShippingLineBoundCheckEdit);
			this.SeaGroupBox.Controls.Add(this.OB_CRIsShipsAgencyPrincipalBoundCheckBox);
			this.SeaGroupBox.Controls.Add(this.VesselConsortCheckBox);
			this.SeaGroupBox.Controls.Add(this.OH_IsSeaWholesalerBoundCheckEdit);
			this.SeaGroupBox.TabIndex = 2;

			this.LandGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|F95A63F3-45B0-47B4-837D-D57134F9B8BE", "Land");
			this.LandGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 240, true);
			this.LandGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 103, true);
			this.LandGroupBox.Controls.Add(this.OH_IsLocalTransportBoundCheckEdit);
			this.LandGroupBox.Controls.Add(this.OH_IsLineHaulProviderBoundCheckEdit);
			this.LandGroupBox.Controls.Add(this.OH_IsRailProviderBoundCheckEdit);
			this.LandGroupBox.Controls.Add(this.OH_IsInlandWaterwayProviderCheckEdit);
			this.LandGroupBox.TabIndex = 3;

			this.CarrierTypeGroupBox.Controls.Add(this.OM_CRCarrierCategoryBoundDropEdit);

			this.CarrierTypeGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.CarrierTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarrierTypeGroupBox.Name = "CarrierTypeGroupBox";
			this.CarrierTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 362, true);
			this.CarrierTypeGroupBox.TabIndex = 0;
			this.CarrierTypeGroupBox.TabStop = false;
			//
			// VesselConsortCheckBox
			//
			this.BindingSource.SetBindingMember(this.VesselConsortCheckBox, "OH_IsShippingConsortium");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsShippingConsortium)));
			this.VesselConsortCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.VesselConsortCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.VesselConsortCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.VesselConsortCheckBox.Name = "VesselConsortCheckBox";
			this.VesselConsortCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.VesselConsortCheckBox.TabIndex = 2;
			//
			// OM_CRCarrierCategoryBoundDropEdit
			//
			this.OM_CRCarrierCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CRCarrierCategoryBoundDropEdit, "MiscServ.OM_CRCarrierCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CRCarrierCategory)));
			this.OM_CRCarrierCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 24, true);
			this.OM_CRCarrierCategoryBoundDropEdit.Name = "OM_CRCarrierCategoryBoundDropEdit";
			this.OM_CRCarrierCategoryBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_CRCarrierCategoryBoundDropEdit.ShowDescriptionBox = false;
			this.OM_CRCarrierCategoryBoundDropEdit.ShowHorizontalScrollBar = false;
			this.OM_CRCarrierCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OM_CRCarrierCategoryBoundDropEdit.TabIndex = 0;
			//
			// OH_IsLineHaulProviderBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsLineHaulProviderBoundCheckEdit, "OH_IsLineHaulProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsLineHaulProvider)));
			this.OH_IsLineHaulProviderBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|9925953f-42dc-43f4-83b5-57774b7a5874", "Line Haul");
			this.OH_IsLineHaulProviderBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsLineHaulProviderBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsLineHaulProviderBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 37, true);
			this.OH_IsLineHaulProviderBoundCheckEdit.Name = "OH_IsLineHaulProviderBoundCheckEdit";
			this.OH_IsLineHaulProviderBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsLineHaulProviderBoundCheckEdit.TabIndex = 1;
			//
			// OH_IsSeaWholesalerBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsSeaWholesalerBoundCheckEdit, "OH_IsSeaWholesaler");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsSeaWholesaler)));
			this.OH_IsSeaWholesalerBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|f7440e2e-db06-4296-bc57-536764345a27", "NVOCC");
			this.OH_IsSeaWholesalerBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsSeaWholesalerBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsSeaWholesalerBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 75, true);
			this.OH_IsSeaWholesalerBoundCheckEdit.Name = "OH_IsSeaWholesalerBoundCheckEdit";
			this.OH_IsSeaWholesalerBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsSeaWholesalerBoundCheckEdit.TabIndex = 3;
			//
			// OH_IsAirWholesalerBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsAirWholesalerBoundCheckEdit, "OH_IsAirWholesaler");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsAirWholesaler)));
			this.OH_IsAirWholesalerBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|577a404b-c50d-41c9-b0c5-0543d2724204", "Air Freight Wholesaler");
			this.OH_IsAirWholesalerBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsAirWholesalerBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsAirWholesalerBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 37, true);
			this.OH_IsAirWholesalerBoundCheckEdit.Name = "OH_IsAirWholesalerBoundCheckEdit";
			this.OH_IsAirWholesalerBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsAirWholesalerBoundCheckEdit.TabIndex = 1;
			//
			// OH_IsRailProviderBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsRailProviderBoundCheckEdit, "OH_IsRailProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsRailProvider)));
			this.OH_IsRailProviderBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|7fa88eda-a4a7-4e69-9dc0-53178eed31b8", "Rail");
			this.OH_IsRailProviderBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsRailProviderBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsRailProviderBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.OH_IsRailProviderBoundCheckEdit.Name = "OH_IsRailProviderBoundCheckEdit";
			this.OH_IsRailProviderBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsRailProviderBoundCheckEdit.TabIndex = 2;
			//
			// OH_IsAirLineBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsAirLineBoundCheckEdit, "OH_IsAirLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsAirLine)));
			this.OH_IsAirLineBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|9fac2a87-2816-443c-972c-11fd03c8e44f", "Airline");
			this.OH_IsAirLineBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsAirLineBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsAirLineBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 18, true);
			this.OH_IsAirLineBoundCheckEdit.Name = "OH_IsAirLineBoundCheckEdit";
			this.OH_IsAirLineBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsAirLineBoundCheckEdit.TabIndex = 0;
			//
			// OH_IsShippingLineBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsShippingLineBoundCheckEdit, "OH_IsShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsShippingLine)));
			this.OH_IsShippingLineBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|dae6978a-46ba-4be0-89f7-f61903d59f62", "Shipping Line");
			this.OH_IsShippingLineBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsShippingLineBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsShippingLineBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 18, true);
			this.OH_IsShippingLineBoundCheckEdit.Name = "OH_IsShippingLineBoundCheckEdit";
			this.OH_IsShippingLineBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsShippingLineBoundCheckEdit.TabIndex = 0;
			//
			// OH_IsLocalTransportBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsLocalTransportBoundCheckEdit, "OH_IsLocalTransport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsLocalTransport)));
			this.OH_IsLocalTransportBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|d397f6c7-d42b-4e5b-a46c-49e7d459739f", "Road Transport");
			this.OH_IsLocalTransportBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsLocalTransportBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsLocalTransportBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 18, true);
			this.OH_IsLocalTransportBoundCheckEdit.Name = "OH_IsLocalTransportBoundCheckEdit";
			this.OH_IsLocalTransportBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsLocalTransportBoundCheckEdit.TabIndex = 0;
			//
			// OH_IsInlandWaterwayProviderCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OH_IsInlandWaterwayProviderCheckEdit, "OH_IsInlandWaterwayProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsInlandWaterwayProvider)));
			this.OH_IsInlandWaterwayProviderCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|f5d2fbd3-d9c5-4127-8352-64f56100bee2", "Inland Waterways");
			this.OH_IsInlandWaterwayProviderCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsInlandWaterwayProviderCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsInlandWaterwayProviderCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 75, true);
			this.OH_IsInlandWaterwayProviderCheckEdit.Name = "OH_IsInlandWaterwayProviderCheckEdit";
			this.OH_IsInlandWaterwayProviderCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OH_IsInlandWaterwayProviderCheckEdit.TabIndex = 3;
			//
			// OB_CRIsShipsAgencyPrincipalBoundCheckBox
			//
			this.BindingSource.SetBindingMember(this.OB_CRIsShipsAgencyPrincipalBoundCheckBox, "CompanyData+OB_CRIsShipsAgencyPrincipal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_CRIsShipsAgencyPrincipal)));
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|ae8ae181-4edd-4be3-99b8-fc0bb8394c3e", "Principal");
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 37, true);
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.Name = "OB_CRIsShipsAgencyPrincipalBoundCheckBox";
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 19, true);
			this.OB_CRIsShipsAgencyPrincipalBoundCheckBox.TabIndex = 1;
			//
			// zCarrierPortsGroupBox
			//
			this.zCarrierPortsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|3442f58f-2b14-4eed-a2a3-6e4a5adbbf71", "Port Related Parties");
			this.zCarrierPortsGroupBox.Controls.Add(this.AgentTabControl);
			this.zCarrierPortsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zCarrierPortsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zCarrierPortsGroupBox.Name = "zCarrierPortsGroupBox";
			this.zCarrierPortsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 158, true);
			this.zCarrierPortsGroupBox.TabIndex = 2;
			this.zCarrierPortsGroupBox.TabStop = false;
			//
			// AgentTabControl
			//
			this.AgentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AgentTabControl.Controls.Add(this.StevedoreTabPage);
			this.AgentTabControl.Controls.Add(this.AirTabPage);
			this.AgentTabControl.Controls.Add(this.RailHeadTabPage);
			this.AgentTabControl.Controls.Add(this.RoadDepotTabPage);
			this.AgentTabControl.Controls.Add(this.ContainerYardTabPage);
			this.AgentTabControl.Controls.Add(this.PenaltiesTabPage);
			this.AgentTabControl.Controls.Add(this.agenciesTabPage);
			this.AgentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AgentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AgentTabControl.Name = "AgentTabControl";
			this.AgentTabControl.SelectedIndex = 0;
			this.AgentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 139, true);
			this.AgentTabControl.TabIndex = 32;
			//
			// StevedoreTabPage
			//
			this.StevedoreTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|8ee80587-55cb-47ff-8d52-e6ccc4c9574f", "Sea CTO/Stevedore");
			this.StevedoreTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StevedoreTabPage.Name = "StevedoreTabPage";
			this.StevedoreTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.StevedoreTabPage.TabIndex = 0;
			this.StevedoreTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.StevedoreTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).O5_TerminalType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Stevedore)).SyncRoot)).O5_AgentDirection)));
			//
			// AirTabPage
			//
			this.AirTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|454c4602-e266-46b2-b41c-0f10e2bb5430", "Air CTO");
			this.AirTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirTabPage.Name = "AirTabPage";
			this.AirTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.AirTabPage.TabIndex = 1;
			this.AirTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AirTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_AirCTO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_AirCTO)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_AirCTO)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_AirCTO)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_AirCTO)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			//
			// RailHeadTabPage
			//
			this.RailHeadTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|6d09ca2b-b58c-48af-8aca-e01b47872283", "Rail Head/Depot");
			this.RailHeadTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RailHeadTabPage.Name = "RailHeadTabPage";
			this.RailHeadTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.RailHeadTabPage.TabIndex = 2;
			this.RailHeadTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RailHeadTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RailHeadDepot)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RailHeadDepot)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RailHeadDepot)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RailHeadDepot)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RailHeadDepot)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			//
			// RoadDepotTabPage
			//
			this.RoadDepotTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|6fb36737-ca2e-408e-bd09-a5b4dd6d190d", "Road Depot/Transit Shed");
			this.RoadDepotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RoadDepotTabPage.Name = "RoadDepotTabPage";
			this.RoadDepotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.RoadDepotTabPage.TabIndex = 3;
			this.RoadDepotTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RoadDepotTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RoadDepotShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RoadDepotShed)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RoadDepotShed)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RoadDepotShed)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_RoadDepotShed)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			//
			// ContainerYardTabPage
			//
			this.ContainerYardTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|eb0b35b8-8d3e-4be5-93ce-27d2b40dc095", "Container Yard/Park");
			this.ContainerYardTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerYardTabPage.Name = "ContainerYardTabPage";
			this.ContainerYardTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.ContainerYardTabPage.TabIndex = 4;
			this.ContainerYardTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ContainerYardTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_ContainerStorageClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).ContainerStorageClassDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_RX_NKCYWorkOrderApprovalLimitCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_CYWorkOrderApprovalLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_CYWorkOrderApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_OC_CYWorkOrderApprovedBy)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_ContainerYardPark)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			//
			// PenaltiesTabPage
			//
			this.PenaltiesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.PenaltiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|b2388d5a-d01a-4a03-8cb4-2eed709587f1", "Container Penalties");
			this.PenaltiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PenaltiesTabPage.Name = "PenaltyTabPage";
			this.PenaltiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.PenaltiesTabPage.TabIndex = 6;
			this.PenaltiesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.PenaltiesTabPage_InitializeTab));
			//
			// agenciesTabPage
			//
			this.agenciesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.agenciesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|f682f4b8-ecd9-4f6e-9e9e-cf3e5eec629d", "Agencies", "Related Agencies to this Carrier.");
			this.agenciesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.agenciesTabPage.Name = "agenciesTabPage";
			this.agenciesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.agenciesTabPage.TabIndex = 6;
			this.agenciesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.agenciesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Agency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Agency)).SyncRoot)).O5_PortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Agency)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Agency)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAppointedAgentPorts_Agency)).SyncRoot)).O5_OA_AgentOfficeAddress)));
			//
			// CarrierTypePanel
			//
			this.CarrierTypePanel.Controls.Add(this.IATAServicePanel);
			this.CarrierTypePanel.Controls.Add(this.CarrierTypeGroupBox);
			this.CarrierTypePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CarrierTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarrierTypePanel.Name = "CarrierTypePanel";
			this.CarrierTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 362, true);
			this.CarrierTypePanel.TabIndex = 3;
			//
			// IATAServicePanel
			//
			this.IATAServicePanel.Controls.Add(this.DetailsTabControl);
			this.IATAServicePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IATAServicePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
			this.IATAServicePanel.Name = "IATAServicePanel";
			this.IATAServicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 342, true);
			this.IATAServicePanel.TabIndex = 2;
			//
			// DetailsTabControl
			//
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.AirlineTabPage);
			this.DetailsTabControl.Controls.Add(this.ServiceLevelTabPage);
			this.DetailsTabControl.Controls.Add(this.RefShippingLineTabPage);
			this.DetailsTabControl.Controls.Add(this.AccountNumbersTabPage);
			this.DetailsTabControl.Controls.Add(this.NamedAccountClientsTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 342, true);
			this.DetailsTabControl.TabIndex = 31;
			//
			// AirlineTabPage
			//
			this.AirlineTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|bfdaf8d7-cc28-46b5-a8c4-c02688198caf", "Airline");
			this.AirlineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirlineTabPage.Name = "AirlineTabPage";
			this.AirlineTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirlineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 315, true);
			this.AirlineTabPage.TabIndex = 0;
			this.AirlineTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AirlineTabPage_InitializeTab));
			//
			// ServiceLevelTabPage
			//
			this.ServiceLevelTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|b56232f0-c7ee-4fba-b090-ad6eeaf30404", "Service Levels");
			this.ServiceLevelTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServiceLevelTabPage.Name = "ServiceLevelTabPage";
			this.ServiceLevelTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServiceLevelTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 315, true);
			this.ServiceLevelTabPage.TabIndex = 1;
			this.ServiceLevelTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ServiceLevelTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_CarrierServiceLevelDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_CarrierServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_ServicePrintDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_APProfileID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_IsSignatureRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.CarrierServiceLevels)).SyncRoot)).PL_ProofOfDelivery)));
			//
			// RefShippingLineTabPage
			//
			this.RefShippingLineTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7e6a4be0-ed2d-4104-b9ea-15f5683f509e", "Shipping Line/NVOCC/Agent");
			this.RefShippingLineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RefShippingLineTabPage.Name = "RefShippingLineTabPage";
			this.RefShippingLineTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RefShippingLineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 315, true);
			this.RefShippingLineTabPage.TabIndex = 2;
			this.RefShippingLineTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.VoyageRecyclingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.VoyageRecyclingPeriodCode)));
			//
			// AccountNumbersTabPage
			//
			this.AccountNumbersTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c110fdbf-2d38-4bee-a85b-3f15ec86f26f", "Account Numbers");
			this.AccountNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountNumbersTabPage.Name = "AccountNumbersTabPage";
			this.AccountNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AccountNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 315, true);
			this.AccountNumbersTabPage.TabIndex = 3;
			this.AccountNumbersTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AccountNumbersTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_TBAllowMixedAccountNumbersOnManifest)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).OAN_AccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).OAN_DepotID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).OAN_MerchantNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).OAN_OH_BillToParty)));
			//
			// NamedAccountClientsTabPage
			//
			this.NamedAccountClientsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3d8a3858-9b66-490d-a4c3-e4a608ba3bc5", "Named Account Clients");
			this.NamedAccountClientsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NamedAccountClientsTabPage.Name = "NamedAccountClientsTabPage";
			this.NamedAccountClientsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NamedAccountClientsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 315, true);
			this.NamedAccountClientsTabPage.TabIndex = 4;
			this.NamedAccountClientsTabPage.TabVisible = RatingFeatureHelper.Urs.IsEnabled;
			this.NamedAccountClientsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NamedAccountClientsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierNamedAccounts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierNamedAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).ONA_ForeignName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierNamedAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierAccounts)).SyncRoot)).ONA_OH_Organization)));
			//
			// DepotDetailsPanel
			//
			this.DepotDetailsPanel.Controls.Add(this.zCarrierPortsGroupBox);
			this.DepotDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepotDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.DepotDetailsPanel.Name = "DepotDetailsPanel";
			this.DepotDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 158, true);
			this.DepotDetailsPanel.TabIndex = 4;
			//
			// CarrierConfigurationUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DepotDetailsPanel);
			this.Controls.Add(this.CarrierTypePanel);
			this.Name = "CarrierConfigurationUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierTypeGroupBox.ResumeLayout(false);
			this.CarrierTypeGroupBox.PerformLayout();
			this.OM_CRCarrierCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_CRCarrierCategoryBoundDropEdit.PerformLayout();
			this.zCarrierPortsGroupBox.ResumeLayout(false);
			this.zCarrierPortsGroupBox.PerformLayout();
			this.AgentTabControl.ResumeLayout(false);
			this.AgentTabControl.PerformLayout();
			this.CarrierTypePanel.ResumeLayout(false);
			this.CarrierTypePanel.PerformLayout();
			this.IATAServicePanel.ResumeLayout(false);
			this.IATAServicePanel.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.DepotDetailsPanel.ResumeLayout(false);
			this.DepotDetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void AirlineTabPage_InitializeTab(object sender, EventArgs e)
		{
			this.AirlineTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.IATATabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirlineAccountNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MAWBStockManagementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirlineTabPage.SuspendLayout();
			this.AirlineTabControl.SuspendLayout();
			this.IATATabPage.SuspendLayout();
			this.AirlineAccountNumbersTabPage.SuspendLayout();
			this.MAWBStockManagementTabPage.SuspendLayout();
			//
			// AirlineTabControl
			//
			this.AirlineTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AirlineTabControl.Controls.Add(this.IATATabPage);
			this.AirlineTabControl.Controls.Add(this.AirlineAccountNumbersTabPage);
			this.AirlineTabControl.Controls.Add(this.MAWBStockManagementTabPage);
			this.AirlineTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirlineTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AirlineTabControl.Name = "AirlineTabControl";
			this.AirlineTabControl.SelectedIndex = 0;
			this.AirlineTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 310, true);
			this.AirlineTabControl.TabIndex = 1;
			//
			// IATATabPage
			//
			this.IATATabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|28693c6a-37a5-433d-ae60-67eef1cd7be1", "IATA Details");
			this.IATATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IATATabPage.Name = "IATATabPage";
			this.IATATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IATATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 305, true);
			this.IATATabPage.TabIndex = 1;
			this.IATATabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.IATATabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineThreeLetterCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlinePostalCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineAddressLine1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineAddressLine2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineName1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineTwoCharacterCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineState)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.AirlineName2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_RM_Airline)));
			//
			// AirlineAccountNumbersTabPage
			//
			this.AirlineAccountNumbersTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|fe18410c-27f6-4c8d-bc8e-33299d6abe9b", "Airline Account Number");
			this.AirlineAccountNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirlineAccountNumbersTabPage.Name = "AirlineAccountNumbersTabPage";
			this.AirlineAccountNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirlineAccountNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 305, true);
			this.AirlineAccountNumbersTabPage.TabIndex = 2;
			this.AirlineAccountNumbersTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AirlineAccountNumbersTabPage_InitializeTab));
			//
			// MAWBStockManagementTabPage
			//
			this.MAWBStockManagementTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9a07ccac-2718-43a7-94e3-8e24526aabb7", "MAWB Stock Management");
			this.MAWBStockManagementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MAWBStockManagementTabPage.Name = "MAWBStockManagementTabPage";
			this.MAWBStockManagementTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MAWBStockManagementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 305, true);
			this.MAWBStockManagementTabPage.TabIndex = 3;
			this.MAWBStockManagementTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MAWBStockManagementTabPage_InitializeTab));
			this.AirlineTabPage.Controls.Add(this.AirlineTabControl);
			this.AirlineTabPage.PerformLayout();
			this.AirlineTabControl.ResumeLayout(false);
			this.AirlineTabControl.PerformLayout();
			this.IATATabPage.ResumeLayout(false);
			this.IATATabPage.PerformLayout();
			this.AirlineAccountNumbersTabPage.ResumeLayout(false);
			this.AirlineAccountNumbersTabPage.PerformLayout();
			this.MAWBStockManagementTabPage.ResumeLayout(false);
			this.MAWBStockManagementTabPage.PerformLayout();
			this.AirlineTabPage.ResumeLayout(true);
		}

		void AirlineAccountNumbersTabPage_InitializeTab(object sender, EventArgs e)
		{
			var branchColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			var accountNumberColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AirlineAccountNumbersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AirlineAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirlineAccountNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirlineAccountNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AirlineAccountNumbersTabPage.SuspendLayout();
			this.AirlineAccountNumbersPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirlineAccountNumberGrid)).BeginInit();
			this.AirlineAccountNumberGrid.SuspendLayout();
			this.AirlineAccountNumbersTabPage.Controls.Add(this.AirlineAccountNumbersPanel);
			//
			// AirlineAccountNumbersPanel
			//
			this.AirlineAccountNumbersPanel.Controls.Add(this.AirlineAccountNumberTextBox);
			this.AirlineAccountNumbersPanel.Controls.Add(this.AirlineAccountNumberGroupBox);
			this.AirlineAccountNumbersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirlineAccountNumbersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirlineAccountNumbersPanel.Name = "AirlineDetailsPanel";
			this.AirlineAccountNumbersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 304, true);
			this.AirlineAccountNumbersPanel.TabIndex = 1;
			//
			// AirlineAccountNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.AirlineAccountNumberTextBox, "CompanyData+OB_APAirlineAccountNumber");
			this.AirlineAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 5, true);
			this.AirlineAccountNumberTextBox.Name = "AirlineAccountNumberTextBox";
			this.AirlineAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.AirlineAccountNumberTextBox.TabIndex = 1;
			//
			// AddressDetailGroupBox
			//
			this.AirlineAccountNumberGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5b77cf67-25d6-4855-8476-ce7f7b8aadf1", "Branch Specific Airline Account Number");
			this.AirlineAccountNumberGroupBox.Controls.Add(this.AirlineAccountNumberGrid);
			this.AirlineAccountNumberGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AirlineAccountNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.AirlineAccountNumberGroupBox.Name = "AirlineAccountNumberGroupBox";
			this.AirlineAccountNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 250, true);
			this.AirlineAccountNumberGroupBox.TabIndex = 2;
			this.AirlineAccountNumberGroupBox.TabStop = false;
			//
			// AirlineAccountNumberGrid
			//
			this.AirlineAccountNumberGrid.AllowNavigation = false;
			this.AirlineAccountNumberGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AirlineAccountNumberGrid, "OrgAirlineBranchAccounts");
			this.AirlineAccountNumberGrid.CaptionVisible = false;
			this.AirlineAccountNumberGrid.GridId = "cd687f66-c6cb-41d1-8b3e-e3d123cb795e";
			branchColumnStyleInfo.ColumnName = "OAA_GB_Branch";
			branchColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			accountNumberColumnStyleInfo.ColumnName = "OAA_APAirlineAccountNumber";
			accountNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AirlineAccountNumberGrid.ColumnStyles.Add(branchColumnStyleInfo);
			this.AirlineAccountNumberGrid.ColumnStyles.Add(accountNumberColumnStyleInfo);
			this.AirlineAccountNumberGrid.TabIndex = 3;
			this.AirlineAccountNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.AirlineAccountNumberGrid.Name = "AirlineAccountNumberGrid";
			this.AirlineAccountNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 230, true);
			this.AirlineAccountNumbersTabPage.PerformLayout();
			this.AirlineAccountNumbersPanel.ResumeLayout();
			this.AirlineAccountNumbersPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirlineAccountNumberGrid)).EndInit();
			this.AirlineAccountNumberGrid.ResumeLayout(false);
			this.AirlineAccountNumberGrid.PerformLayout();
			this.AirlineAccountNumbersTabPage.ResumeLayout(true);
		}

		void IATATabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.AirlineDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RM_ThreeLetterCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineCountryBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlinePostalCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AddressLine1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AddressLine2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineName1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_TwoCharacterCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineStateBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineCityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineName2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirlineNumericCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.IATATabPage.SuspendLayout();
			this.AirlineDetailsPanel.SuspendLayout();
			this.AirlineNumericCodeFindBox.SuspendLayout();
			this.IATATabPage.Controls.Add(this.AirlineDetailsPanel);
			//
			// AirlineDetailsPanel
			//
			this.AirlineDetailsPanel.Controls.Add(this.RM_ThreeLetterCodeBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlineCountryBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlinePostalCodeBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AddressLine1BoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AddressLine2BoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlineName1BoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_TwoCharacterCodeBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlineStateBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlineCityBoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.RM_AirlineName2BoundTextBox);
			this.AirlineDetailsPanel.Controls.Add(this.AirlineNumericCodeFindBox);
			this.AirlineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirlineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirlineDetailsPanel.Name = "AirlineDetailsPanel";
			this.AirlineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.AirlineDetailsPanel.TabIndex = 52;
			//
			// RM_ThreeLetterCodeBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_ThreeLetterCodeBoundTextBox, "MiscServ.AirlineThreeLetterCode");
			this.RM_ThreeLetterCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|15ED223F-9467-4B8D-9FA7-7D9A6981BA4E", "Three Letter Code");
			this.RM_ThreeLetterCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 235, true);
			this.RM_ThreeLetterCodeBoundTextBox.Name = "RM_ThreeLetterCodeBoundTextBox";
			this.RM_ThreeLetterCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RM_ThreeLetterCodeBoundTextBox.TabIndex = 63;
			this.RM_ThreeLetterCodeBoundTextBox.ReadOnly = true;
			//
			// RM_AirlineCountryBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlineCountryBoundTextBox, "MiscServ.AirlineCountry");
			this.RM_AirlineCountryBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|4506315A-02D0-45DD-B500-4D6AD5E610E4", "Airline Country/Region");
			this.RM_AirlineCountryBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 164, true);
			this.RM_AirlineCountryBoundTextBox.Name = "RM_AirlineCountryBoundTextBox";
			this.RM_AirlineCountryBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RM_AirlineCountryBoundTextBox.TabIndex = 60;
			this.RM_AirlineCountryBoundTextBox.ReadOnly = true;
			//
			// RM_AirlinePostalCodeBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlinePostalCodeBoundTextBox, "MiscServ.AirlinePostalCode");
			this.RM_AirlinePostalCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|3593B6B0-66EC-4FDE-9A46-526B5BA02820", "Airline Postal Code");
			this.RM_AirlinePostalCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 187, true);
			this.RM_AirlinePostalCodeBoundTextBox.Name = "RM_AirlinePostalCodeBoundTextBox";
			this.RM_AirlinePostalCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RM_AirlinePostalCodeBoundTextBox.TabIndex = 61;
			this.RM_AirlinePostalCodeBoundTextBox.ReadOnly = true;
			//
			// RM_AddressLine1BoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AddressLine1BoundTextBox, "MiscServ.AirlineAddressLine1");
			this.RM_AddressLine1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|1b6aaace-d448-4cdc-b00d-239f59788f03", "Airline Address");
			this.RM_AddressLine1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 72, true);
			this.RM_AddressLine1BoundTextBox.Name = "RM_AddressLine1BoundTextBox";
			this.RM_AddressLine1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RM_AddressLine1BoundTextBox.TabIndex = 56;
			this.RM_AddressLine1BoundTextBox.ReadOnly = true;
			//
			// RM_AddressLine2BoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AddressLine2BoundTextBox, "MiscServ.AirlineAddressLine2");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RM_AddressLine2BoundTextBox, false);
			this.RM_AddressLine2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 94, true);
			this.RM_AddressLine2BoundTextBox.Name = "RM_AddressLine2BoundTextBox";
			this.RM_AddressLine2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RM_AddressLine2BoundTextBox.TabIndex = 57;
			this.RM_AddressLine2BoundTextBox.ReadOnly = true;
			//
			// RM_AirlineName1BoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlineName1BoundTextBox, "MiscServ.AirlineName1");
			this.RM_AirlineName1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|e200ed2e-eed2-4bb4-a0a9-2161f4faa0db", "Airline Name");
			this.RM_AirlineName1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 26, true);
			this.RM_AirlineName1BoundTextBox.Name = "RM_AirlineName1BoundTextBox";
			this.RM_AirlineName1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RM_AirlineName1BoundTextBox.TabIndex = 54;
			this.RM_AirlineName1BoundTextBox.ReadOnly = true;
			//
			// RM_TwoCharacterCodeBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_TwoCharacterCodeBoundTextBox, "MiscServ.AirlineTwoCharacterCode");
			this.RM_TwoCharacterCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|7330AC96-0DAF-4EDB-B256-75132C73B6A0", "Two Character Code");
			this.RM_TwoCharacterCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 212, true);
			this.RM_TwoCharacterCodeBoundTextBox.Name = "RM_TwoCharacterCodeBoundTextBox";
			this.RM_TwoCharacterCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RM_TwoCharacterCodeBoundTextBox.TabIndex = 62;
			this.RM_TwoCharacterCodeBoundTextBox.ReadOnly = true;
			//
			// RM_AirlineStateBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlineStateBoundTextBox, "MiscServ.AirlineState");
			this.RM_AirlineStateBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|277E09D5-D2ED-4769-BDE1-A248ADCC0A69", "Airline State");
			this.RM_AirlineStateBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 141, true);
			this.RM_AirlineStateBoundTextBox.Name = "RM_AirlineStateBoundTextBox";
			this.RM_AirlineStateBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.RM_AirlineStateBoundTextBox.TabIndex = 59;
			this.RM_AirlineStateBoundTextBox.ReadOnly = true;
			//
			// RM_AirlineCityBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlineCityBoundTextBox, "MiscServ.AirlineCity");
			this.RM_AirlineCityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|26C38769-F56B-4765-BD1E-4830C166093D", "Airline City");
			this.RM_AirlineCityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 118, true);
			this.RM_AirlineCityBoundTextBox.Name = "RM_AirlineCityBoundTextBox";
			this.RM_AirlineCityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.RM_AirlineCityBoundTextBox.TabIndex = 58;
			this.RM_AirlineCityBoundTextBox.ReadOnly = true;
			//
			// RM_AirlineName2BoundTextBox
			//
			this.BindingSource.SetBindingMember(this.RM_AirlineName2BoundTextBox, "MiscServ.AirlineName2");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RM_AirlineName2BoundTextBox, false);
			this.RM_AirlineName2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 48, true);
			this.RM_AirlineName2BoundTextBox.Name = "RM_AirlineName2BoundTextBox";
			this.RM_AirlineName2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RM_AirlineName2BoundTextBox.TabIndex = 55;
			this.RM_AirlineName2BoundTextBox.ReadOnly = true;
			//
			// AirlineNumericCodeFindBox
			//
			this.AirlineNumericCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirlineNumericCodeFindBox, "MiscServ.OM_RM_Airline");
			this.AirlineNumericCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|fa946ef8-cc56-4474-b581-dc7b34160537", "Master Bill Prefix");
			this.AirlineNumericCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 2, true);
			this.AirlineNumericCodeFindBox.Name = "AirlineNumericCodeFindBox";
			this.AirlineNumericCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.AirlineNumericCodeFindBox.TabIndex = 52;
			this.AirlineNumericCodeFindBox.DescriptionBox.Visible = false;

			this.IATATabPage.PerformLayout();
			this.AirlineDetailsPanel.ResumeLayout(false);
			this.AirlineDetailsPanel.PerformLayout();
			this.AirlineNumericCodeFindBox.ResumeLayout(true);
			this.AirlineNumericCodeFindBox.PerformLayout();
			this.IATATabPage.ResumeLayout(true);
		}

		void ServiceLevelTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ServiceLevelsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ServiceLevelGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ServiceLevelTabPage.SuspendLayout();
			this.ServiceLevelsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelGrid)).BeginInit();
			this.ServiceLevelGrid.SuspendLayout();
			this.ServiceLevelTabPage.Controls.Add(this.ServiceLevelsPanel);
			//
			// ServiceLevelsPanel
			//
			this.ServiceLevelsPanel.Controls.Add(this.zLabel1);
			this.ServiceLevelsPanel.Controls.Add(this.ServiceLevelGrid);
			this.ServiceLevelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServiceLevelsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServiceLevelsPanel.Name = "ServiceLevelsPanel";
			this.ServiceLevelsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.ServiceLevelsPanel.TabIndex = 0;
			//
			// zLabel1
			//
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|16ad2c22-c1e3-4511-800b-99aa93e37f44", "", "This module allows you to enter Carrier Service Levels. These will be used on the Consol, as well as for Auto-Costing purposes.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 27, true);
			this.zLabel1.TabIndex = 4;
			//
			// ServiceLevelGrid
			//
			this.ServiceLevelGrid.AllowNavigation = false;
			this.ServiceLevelGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceLevelGrid, "MiscServ+CarrierServiceLevels");
			this.ServiceLevelGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "PL_Code";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTranslatableTextBoxColumnStyleInfo9.CaptionResourceString = Res.GetData("OrgCarrierServiceLevel|4b29a681-edaa-4f4d-9de7-f9e90a051e6e", "Service Level Description");
			zTranslatableTextBoxColumnStyleInfo9.ColumnName = "PL_CarrierServiceLevelDescription";
			zTranslatableTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo10.ColumnName = "PL_CarrierServiceCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "PL_ProductCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "PL_ServicePrintDescription";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "PL_ChargeCode";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "PL_APProfileID";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "PL_IsSignatureRequired";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "PL_ProofOfDelivery";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ServiceLevelGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo9);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ServiceLevelGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ServiceLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ServiceLevelGrid.GridId = "833ced68-2d8b-40c3-aeee-41f61badcf8b";
			this.ServiceLevelGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceLevelGrid.LayoutKey = "ServiceLevelGrid";
			this.ServiceLevelGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
			this.ServiceLevelGrid.Name = "ServiceLevelGrid";
			this.ServiceLevelGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 264, true);
			this.ServiceLevelGrid.TabIndex = 3;
			this.ServiceLevelTabPage.PerformLayout();
			this.ServiceLevelsPanel.ResumeLayout(false);
			this.ServiceLevelsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelGrid)).EndInit();
			this.ServiceLevelGrid.ResumeLayout(false);
			this.ServiceLevelGrid.PerformLayout();
			this.ServiceLevelTabPage.ResumeLayout(true);
		}

		private void VoyageRecyclingTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.RefShippingLinePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RefShippingLineCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.VoyageRecyclingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierPackageGroupingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntegrationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntegrationsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RefShippingLineSCACCode = new Enterprise.ZArchitecture.ZTextBox();
			this.RefShippingLineCarrierName = new Enterprise.ZArchitecture.ZTextBox();
			this.RefShippingLineTabPage.SuspendLayout();
			this.RefShippingLinePanel.SuspendLayout();
			this.RefShippingLineCarrierName.SuspendLayout();
			this.RefShippingLineSCACCode.SuspendLayout();
			this.RefShippingLineCodeFindBox.SuspendLayout();
			this.VoyageRecyclingDropEdit.SuspendLayout();
			this.CarrierPackageGroupingDropEdit.SuspendLayout();
			this.IntegrationsGroupBox.SuspendLayout();
			this.RefShippingLineTabPage.Controls.Add(this.RefShippingLinePanel);
			//
			// RefShippingLinePanel
			//
			this.RefShippingLinePanel.Controls.Add(this.IntegrationsGroupBox);
			this.RefShippingLinePanel.Controls.Add(this.RefShippingLineCodeFindBox);
			this.RefShippingLinePanel.Controls.Add(this.RefShippingLineSCACCode);
			this.RefShippingLinePanel.Controls.Add(this.RefShippingLineCarrierName);
			this.RefShippingLinePanel.Controls.Add(this.VoyageRecyclingDropEdit);
			this.RefShippingLinePanel.Controls.Add(this.CarrierPackageGroupingDropEdit);
			this.RefShippingLinePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefShippingLinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RefShippingLinePanel.Name = "RefShippingLinePanel";
			this.RefShippingLinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.RefShippingLinePanel.TabIndex = 0;
			//
			// RefShippingLineCodeFindBox
			//
			this.RefShippingLineCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefShippingLineCodeFindBox, "OH_RSL_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_RSL_ShippingLine)));
			this.RefShippingLineCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("874b8225-6308-4d6e-a849-31225221bb78", "Shipping Line");
			this.RefShippingLineCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 15, true);
			this.RefShippingLineCodeFindBox.Name = "RefShippingLineCodeFindBox";
			this.RefShippingLineCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RefShippingLineCodeFindBox.TabIndex = 0;
			this.RefShippingLineCodeFindBox.DescriptionBox.Visible = false;
			//
			// CarrierPackageGroupingDropEdit
			//
			this.CarrierPackageGroupingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierPackageGroupingDropEdit, "MiscServ.OM_CarrierPackageGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CarrierPackageGrouping)));
			this.CarrierPackageGroupingDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7764A6D6-B862-4238-9C5B-F817302109A1", "Package Grouping");
			this.CarrierPackageGroupingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 115, true);
			this.CarrierPackageGroupingDropEdit.Name = "CarrierPackageGroupingDropEdit";
			this.CarrierPackageGroupingDropEdit.PreBoundMaxLength = 3;
			this.CarrierPackageGroupingDropEdit.ShowHorizontalScrollBar = false;
			this.CarrierPackageGroupingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CarrierPackageGroupingDropEdit.TabIndex = 4;
			this.CarrierPackageGroupingDropEdit.Visible = Enterprise.Registry.Business.FreightDataRegistry.Instance.EnablePackageGrouping.Value; // Developer only, will be removed afterwards
			//
			// VoyageRecyclingDropEdit
			//
			this.VoyageRecyclingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VoyageRecyclingDropEdit, "MiscServ.VoyageRecyclingPeriodCode");
			this.VoyageRecyclingDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d1c4b919-79b3-406d-87bf-5783a39bb709", "Voyage Recycling Period");
			this.VoyageRecyclingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 90, true);
			this.VoyageRecyclingDropEdit.Name = "VoyageRecyclingDropEdit";
			this.VoyageRecyclingDropEdit.PreBoundMaxLength = 3;
			this.VoyageRecyclingDropEdit.ShowHorizontalScrollBar = false;
			this.VoyageRecyclingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.VoyageRecyclingDropEdit.TabIndex = 1;
			//
			// RefShippingLineSCACCode
			//
			this.BindingSource.SetBindingMember(this.RefShippingLineSCACCode, "ShippingLineSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ShippingLineSCAC)));
			this.RefShippingLineSCACCode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e9aca555-6483-4e05-9edb-690373229d3d", "SCAC");
			this.RefShippingLineSCACCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 65, true);
			this.RefShippingLineSCACCode.Name = "RefShippingLineSCACCode";
			this.RefShippingLineSCACCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.RefShippingLineSCACCode.ReadOnly = true;
			this.RefShippingLineSCACCode.TabIndex = 3;
			//
			// RefShippingLineCarrierName
			//
			this.BindingSource.SetBindingMember(this.RefShippingLineCarrierName, "ShippingLineCarrierName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ShippingLineCarrierName)));
			this.RefShippingLineCarrierName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e28049ce-fde1-4767-b65f-2a9b863d8a60", "Ocean Carrier Name");
			this.RefShippingLineCarrierName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 40, true);
			this.RefShippingLineCarrierName.Name = "RefShippingLineCarrierName";
			this.RefShippingLineCarrierName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.RefShippingLineCarrierName.ReadOnly = true;
			this.RefShippingLineCarrierName.TabIndex = 2;
			//
			// IntegrationsGroupBox
			//
			this.IntegrationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.IntegrationsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("974c8c8b-76de-4155-8cf7-012011082a63", "Available Integrations");
			this.IntegrationsGroupBox.Controls.Add(this.IntegrationsLabel);
			this.IntegrationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 140, true);
			this.IntegrationsGroupBox.Name = "IntegrationsGroupBox";
			this.IntegrationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 160, true);
			this.IntegrationsGroupBox.TabIndex = 5;
			this.IntegrationsGroupBox.TabStop = false;
			//
			// IntegrationsLabel
			//
			this.BindingSource.SetBindingMember(this.IntegrationsLabel, "ShippingLineIntegrations");
			this.IntegrationsLabel.CaptionResourceString = null;
			this.IntegrationsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IntegrationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IntegrationsLabel.Name = "IntegrationsLabel";
			this.IntegrationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 214, true);
			this.IntegrationsLabel.TabIndex = 0;
			this.IntegrationsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.RefShippingLineTabPage.PerformLayout();
			this.RefShippingLinePanel.ResumeLayout(false);
			this.RefShippingLinePanel.PerformLayout();
			this.RefShippingLineCodeFindBox.ResumeLayout(true);
			this.RefShippingLineCodeFindBox.PerformLayout();
			this.VoyageRecyclingDropEdit.ResumeLayout(true);
			this.VoyageRecyclingDropEdit.PerformLayout();
			this.CarrierPackageGroupingDropEdit.ResumeLayout(true);
			this.CarrierPackageGroupingDropEdit.PerformLayout();
			this.IntegrationsGroupBox.ResumeLayout(false);
			this.IntegrationsGroupBox.PerformLayout();
			this.RefShippingLineTabPage.ResumeLayout(true);
		}

		private void AccountNumbersTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.AccountNumbersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AccountNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AccountNumbersTabPage.SuspendLayout();
			this.AccountNumbersPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccountNumbersGrid)).BeginInit();
			this.AccountNumbersGrid.SuspendLayout();
			this.AccountNumbersTabPage.Controls.Add(this.AccountNumbersPanel);
			//
			// AccountNumbersPanel
			//
			this.AccountNumbersPanel.Controls.Add(this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit);
			this.AccountNumbersPanel.Controls.Add(this.AccountNumbersGrid);
			this.AccountNumbersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountNumbersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AccountNumbersPanel.Name = "AccountNumbersPanel";
			this.AccountNumbersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.AccountNumbersPanel.TabIndex = 0;
			//
			// OM_TBAllowMixedAccountNumbersOnManifestCheckEdit
			//
			this.BindingSource.SetBindingMember(this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit, "MiscServ.OM_TBAllowMixedAccountNumbersOnManifest");
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|825b9dcf-b054-4ad4-b5ee-c2abc2634529", "Allow Mixed Account Numbers on Manifest");
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.Name = "OM_TBAllowMixedAccountNumbersOnManifestCheckEdit";
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 24, true);
			this.OM_TBAllowMixedAccountNumbersOnManifestCheckEdit.TabIndex = 3;
			//
			// AccountNumbersGrid
			//
			this.AccountNumbersGrid.AllowNavigation = false;
			this.AccountNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccountNumbersGrid, "CarrierAccounts");
			this.AccountNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("66608271-b14f-436f-a1cc-8c684a48e6e0", "Acc. No.", "Account Number", "Carrier Account Number", "");
			zTextBoxColumnStyleInfo16.ColumnName = "OAN_AccountNumber";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dfda72fb-2874-453d-bd8a-4c84f937ccde", "", "Depot ID", "Carrier Supplied Depot ID", "");
			zTextBoxColumnStyleInfo17.ColumnName = "OAN_DepotID";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("00b5687c-a763-4277-a402-5566b7a86628", "Merchant No", "Merchant Number", "Carrier Supplied Merchant Number", "");
			zTextBoxColumnStyleInfo18.ColumnName = "OAN_MerchantNumber";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ffedbbe6-beb5-4e29-8afd-6326fa41dae7", "Billing Party");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OAN_OH_BillToParty";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.AccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.AccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.AccountNumbersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.AccountNumbersGrid.GridId = "321ced68-d51b-aac3-a88e-41f61badc025";
			this.AccountNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccountNumbersGrid.LayoutKey = "AccountNumbersGrid";
			this.AccountNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 42, true);
			this.AccountNumbersGrid.Name = "AccountNumbersGrid";
			this.AccountNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 263, true);
			this.AccountNumbersGrid.TabIndex = 4;
			this.AccountNumbersTabPage.PerformLayout();
			this.AccountNumbersPanel.ResumeLayout(false);
			this.AccountNumbersPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccountNumbersGrid)).EndInit();
			this.AccountNumbersGrid.ResumeLayout(false);
			this.AccountNumbersGrid.PerformLayout();
			this.AccountNumbersTabPage.ResumeLayout(true);
		}

		private void NamedAccountClientsTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.OrgCarrierNamedAccountUserControl = new Enterprise.MasterFiles.GUI.OrgCarrierNamedAccountUserControl();
			this.NamedAccountClientsTabPage.SuspendLayout();
			this.OrgCarrierNamedAccountUserControl.SuspendLayout();
			this.NamedAccountClientsTabPage.Controls.Add(this.OrgCarrierNamedAccountUserControl);
			//
			// UrsMappedNamedAccountsUserControl
			//
			this.BindingSource.SetBindingMember(this.OrgCarrierNamedAccountUserControl, ".");
			this.OrgCarrierNamedAccountUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgCarrierNamedAccountUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrgCarrierNamedAccountUserControl.Name = "OrgCarrierNamedAccountUserControl";
			this.OrgCarrierNamedAccountUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.OrgCarrierNamedAccountUserControl.TabIndex = 0;
			this.OrgCarrierNamedAccountUserControl.CaptionRenderingEnabled = true;
			this.NamedAccountClientsTabPage.PerformLayout();
			this.OrgCarrierNamedAccountUserControl.ResumeLayout(false);
			this.OrgCarrierNamedAccountUserControl.PerformLayout();
			this.NamedAccountClientsTabPage.ResumeLayout(true);
		}

		void MAWBStockManagementTabPage_InitializeTab(object sender, EventArgs e)
		{
			var mawbStockCompanyColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			var mawbStockGlobalStockColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			var mawbStockCompanyStockColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			var mawbStockBranchStockColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			var mawbStockOtherStockColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo mawbStockBranchColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo mawbStockThresholdColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MAWBStockManagementPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MAWBStockManagementLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MAWBStockManagementGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MAWBStockManagementTabPage.SuspendLayout();
			this.MAWBStockManagementPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MAWBStockManagementGrid)).BeginInit();
			this.MAWBStockManagementGrid.SuspendLayout();
			this.MAWBStockManagementTabPage.Controls.Add(this.MAWBStockManagementPanel);
			//
			// MAWBStockManagementPanel
			//
			this.MAWBStockManagementPanel.Controls.Add(this.MAWBStockManagementLabel);
			this.MAWBStockManagementPanel.Controls.Add(this.MAWBStockManagementGrid);
			this.MAWBStockManagementPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MAWBStockManagementPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MAWBStockManagementPanel.Name = "MAWBStockManagementPanel";
			this.MAWBStockManagementPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 309, true);
			this.MAWBStockManagementPanel.TabIndex = 0;
			//
			// MAWBStockManagementLabel
			//
			this.MAWBStockManagementLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|9ef60d69-9a44-4edd-b2a1-8a60bb8457df", "", "This module allows you to configure the MAWB Stock settings for this airline. The settings will be applied to Global, Company, Branch level including the threshold limits for each. A warning will be presented on the Consol when the MAWB stock falls below the configured threshold setting.");
			this.MAWBStockManagementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.MAWBStockManagementLabel.Name = "MAWBStockManagementLabel";
			this.MAWBStockManagementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 27, true);
			this.MAWBStockManagementLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.MAWBStockManagementLabel.TabIndex = 4;
			//
			// MAWBStockManagementGrid
			//
			this.MAWBStockManagementGrid.AllowNavigation = false;
			this.MAWBStockManagementGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MAWBStockManagementGrid, "OrgAirlineMAWBStockManagementCollection");
			this.MAWBStockManagementGrid.CaptionVisible = false;
			// Company
			mawbStockCompanyColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3ba2b364-7196-4f51-8698-9df7304625df", "Company");
			mawbStockCompanyColumnStyleInfo.ColumnName = "OHM_GC_Company";
			mawbStockCompanyColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			// Branch
			mawbStockBranchColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0f21a98f-e2f3-4d4a-b3a0-870884d07e25", "Branch");
			mawbStockBranchColumnStyleInfo.ColumnName = "OHM_GB_Branch";
			mawbStockBranchColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			// Threshold
			mawbStockThresholdColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1e2784d1-4d2d-4918-9710-2028f896855f", "Threshold", "MAWB Threshold", "");
			mawbStockThresholdColumnStyleInfo.ColumnName = "OHM_MAWBStockThreshold";
			mawbStockThresholdColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// Global stock
			mawbStockGlobalStockColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("76945e3d-2e61-490f-babd-4d79b1982442", "Global Stock");
			mawbStockGlobalStockColumnStyleInfo.ColumnName = "OHM_AllowUseGlobalStock";
			mawbStockGlobalStockColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// Company stock
			mawbStockCompanyStockColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1d60228b-f4c7-4098-8c99-5c4e1e56b781", "Company Stock");
			mawbStockCompanyStockColumnStyleInfo.ColumnName = "OHM_AllowUseCompanyStock";
			mawbStockCompanyStockColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// Branch stock
			mawbStockBranchStockColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("39d5c08d-9ad6-47b2-8a62-78893d1ef471", "Branch Stock");
			mawbStockBranchStockColumnStyleInfo.ColumnName = "OHM_AllowUseBranchStock";
			mawbStockBranchStockColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// Other branch stock
			mawbStockOtherStockColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f60e7f4e-6ac2-4b33-8d2f-1d41d70314c7", "Other Branch Stock");
			mawbStockOtherStockColumnStyleInfo.ColumnName = "OHM_AllowUseOtherBranchStock";
			mawbStockOtherStockColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockCompanyColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockBranchColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockThresholdColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockGlobalStockColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockCompanyStockColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockBranchStockColumnStyleInfo);
			this.MAWBStockManagementGrid.ColumnStyles.Add(mawbStockOtherStockColumnStyleInfo);
			this.MAWBStockManagementGrid.GridId = "eacb3f55-953f-490f-a773-469c04013475";
			this.MAWBStockManagementGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MAWBStockManagementGrid.LayoutKey = "MAWBStockManagementGrid";
			this.MAWBStockManagementGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 42, true);
			this.MAWBStockManagementGrid.Name = "MAWBStockManagementGrid";
			this.MAWBStockManagementGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 263, true);
			this.MAWBStockManagementGrid.TabIndex = 4;
			this.MAWBStockManagementTabPage.PerformLayout();
			this.MAWBStockManagementPanel.ResumeLayout(false);
			this.MAWBStockManagementPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MAWBStockManagementGrid)).EndInit();
			this.MAWBStockManagementGrid.ResumeLayout(false);
			this.MAWBStockManagementGrid.PerformLayout();
			this.MAWBStockManagementTabPage.ResumeLayout(true);
		}

		void AirTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AirCTOGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AirTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirCTOGrid)).BeginInit();
			this.AirCTOGrid.SuspendLayout();
			this.AirTabPage.Controls.Add(this.AirCTOGrid);
			//
			// AirCTOGrid
			//
			this.AirCTOGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AirCTOGrid, "CarrierAppointedAgentPorts_AirCTO");
			this.AirCTOGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|0ae4f775-3f24-4c28-825e-e8cf0d452e45", "Port");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|04493a3f-48f1-45c4-83ef-6cafe4d0b48a", "CTO");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|382cbe78-eda5-4b24-a63f-0dc0fc1e4c5f", "Name", "Handling Agent Name.");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|9c060e66-7fbe-4ef6-95e5-08334f029990", "Address", "Agent Office Address.");
			zGuidDropEditColumnStyleInfo2.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo2.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ColumnName = "O5_AgentDirection";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AirCTOGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.AirCTOGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.AirCTOGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AirCTOGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.AirCTOGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AirCTOGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirCTOGrid.GridId = "2bc4eed7-d064-45c3-a2f4-e53e5693b4e3";
			this.AirCTOGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AirCTOGrid.LayoutKey = "zGrid5";
			this.AirCTOGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AirCTOGrid.Name = "AirCTOGrid";
			this.AirCTOGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.AirCTOGrid.TabIndex = 3;
			this.AirTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirCTOGrid)).EndInit();
			this.AirCTOGrid.ResumeLayout(false);
			this.AirCTOGrid.PerformLayout();
			this.AirTabPage.ResumeLayout(true);

		}

		void PenaltiesTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.ContainerPenaltiesControl = new Enterprise.MasterFiles.GUI.ContainerPenaltiesUserControl();
			this.PenaltiesTabPage.SuspendLayout();
			this.ContainerPenaltiesControl.SuspendLayout();
			this.PenaltiesTabPage.Controls.Add(this.ContainerPenaltiesControl);
			//
			// ContainerPenaltiesControl
			//
			this.ContainerPenaltiesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerPenaltiesControl, ".");
			this.ContainerPenaltiesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerPenaltiesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerPenaltiesControl.Name = "ContainerPenaltiesControl";
			this.ContainerPenaltiesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.ContainerPenaltiesControl.TabIndex = 2;
			this.PenaltiesTabPage.PerformLayout();
			this.ContainerPenaltiesControl.ResumeLayout(true);
			this.ContainerPenaltiesControl.PerformLayout();
			this.PenaltiesTabPage.ResumeLayout(true);
		}

		void StevedoreTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AppointedCarrierPortszGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StevedoreTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AppointedCarrierPortszGrid)).BeginInit();
			this.AppointedCarrierPortszGrid.SuspendLayout();
			this.StevedoreTabPage.Controls.Add(this.AppointedCarrierPortszGrid);
			//
			// AppointedCarrierPortszGrid
			//
			this.AppointedCarrierPortszGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AppointedCarrierPortszGrid, "CarrierAppointedAgentPorts_Stevedore");
			this.AppointedCarrierPortszGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|63f6381a-92cc-4068-a9c5-96025d6ccdcd", "Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "O5_TerminalType";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|669b5224-43ed-4707-b51e-a34b7503538a", "CTO");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|4a51f0a6-d8f4-4f47-adac-41d327599c48", "Name", "Handling Agent Name.");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|da853abb-eee9-403a-b8c6-cb794fdbeb8a", "Address", "Agent Office Address.");
			zGuidDropEditColumnStyleInfo1.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo2.ColumnName = "O5_AgentDirection";
			zDropEditColumnStyleInfo2.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AppointedCarrierPortszGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AppointedCarrierPortszGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AppointedCarrierPortszGrid.GridId = "574a4796-6872-4d90-9204-10c1b1686b6c";
			this.AppointedCarrierPortszGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AppointedCarrierPortszGrid.LayoutKey = "AppointedCarrierPortszGrid";
			this.AppointedCarrierPortszGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AppointedCarrierPortszGrid.Name = "AppointedCarrierPortszGrid";
			this.AppointedCarrierPortszGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.AppointedCarrierPortszGrid.TabIndex = 1;
			this.StevedoreTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AppointedCarrierPortszGrid)).EndInit();
			this.AppointedCarrierPortszGrid.ResumeLayout(false);
			this.AppointedCarrierPortszGrid.PerformLayout();
			this.StevedoreTabPage.ResumeLayout(true);

		}

		void RailHeadTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.RailHeadDepotGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RailHeadTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RailHeadDepotGrid)).BeginInit();
			this.RailHeadDepotGrid.SuspendLayout();
			this.RailHeadTabPage.Controls.Add(this.RailHeadDepotGrid);
			//
			// RailHeadDepotGrid
			//
			this.RailHeadDepotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RailHeadDepotGrid, "CarrierAppointedAgentPorts_RailHeadDepot");
			this.RailHeadDepotGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|7f8dc184-8e06-4e43-b67b-790fa6691004", "Port");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|b8f788da-f0ba-457d-b884-de173fef237f", "Head/Depot");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|ffba9b22-5347-405a-8e44-218faecb1394", "Name", "Handling Agent Name.");
			zTextBoxColumnStyleInfo3.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|daead0ed-3cae-4b30-b881-d1a1f9b4b9d2", "Address", "Agent Office Address.");
			zGuidDropEditColumnStyleInfo3.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo3.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.RailHeadDepotGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.RailHeadDepotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.RailHeadDepotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RailHeadDepotGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.RailHeadDepotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RailHeadDepotGrid.GridId = "643a72b9-f93e-4642-8f31-227b1689420d";
			this.RailHeadDepotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RailHeadDepotGrid.LayoutKey = "zGrid2";
			this.RailHeadDepotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RailHeadDepotGrid.Name = "RailHeadDepotGrid";
			this.RailHeadDepotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.RailHeadDepotGrid.TabIndex = 2;
			this.RailHeadTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RailHeadDepotGrid)).EndInit();
			this.RailHeadDepotGrid.ResumeLayout(false);
			this.RailHeadDepotGrid.PerformLayout();
			this.RailHeadTabPage.ResumeLayout(true);

		}

		void RoadDepotTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.RoadDepotShedGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RoadDepotTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RoadDepotShedGrid)).BeginInit();
			this.RoadDepotShedGrid.SuspendLayout();
			this.RoadDepotTabPage.Controls.Add(this.RoadDepotShedGrid);
			//
			// RoadDepotShedGrid
			//
			this.RoadDepotShedGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RoadDepotShedGrid, "CarrierAppointedAgentPorts_RoadDepotShed");
			this.RoadDepotShedGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|20ef8f54-4eeb-4d75-b23c-c997ce0ad124", "Port");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|23742684-008b-409a-bf7e-5934af901d7d", "Depot/Shed");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|de08d4e1-a425-4062-a857-35623990159d", "Name", "Handling Agent Name.");
			zTextBoxColumnStyleInfo4.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|5f79dd95-ca83-4f37-8fe3-c0b4113fc8e7", "Address", "Agent Office Address.");
			zGuidDropEditColumnStyleInfo4.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo4.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.RoadDepotShedGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.RoadDepotShedGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.RoadDepotShedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RoadDepotShedGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.RoadDepotShedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RoadDepotShedGrid.GridId = "63dd3393-845f-42c8-bb4d-ccb35bfccdce";
			this.RoadDepotShedGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RoadDepotShedGrid.LayoutKey = "zGrid3";
			this.RoadDepotShedGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RoadDepotShedGrid.Name = "RoadDepotShedGrid";
			this.RoadDepotShedGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.RoadDepotShedGrid.TabIndex = 2;
			this.RoadDepotTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RoadDepotShedGrid)).EndInit();
			this.RoadDepotShedGrid.ResumeLayout(false);
			this.RoadDepotShedGrid.PerformLayout();
			this.RoadDepotTabPage.ResumeLayout(true);

		}

		void ContainerYardTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();

			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxCYWorkOrderApprovalLimitCurrency = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditCYWorkOrderApprovalLimit = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxCYWorkOrderApprovalNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidCYWorkOrderApprovedBy = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			this.ContainerYardParkPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CYPTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CYPGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CYSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContainerYardTabPage.SuspendLayout();
			this.ContainerYardParkPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CYPTypesGrid)).BeginInit();
			this.CYPTypesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CYPGrid)).BeginInit();
			this.CYPGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CYSplitContainer)).BeginInit();
			this.CYSplitContainer.Panel1.SuspendLayout();
			this.CYSplitContainer.Panel2.SuspendLayout();
			this.CYSplitContainer.SuspendLayout();
			this.ContainerYardTabPage.Controls.Add(this.ContainerYardParkPanel);
			//
			// ContainerYardParkPanel
			//
			this.ContainerYardParkPanel.Controls.Add(this.CYSplitContainer);
			this.ContainerYardParkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardParkPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerYardParkPanel.Name = "ContainerYardParkPanel";
			this.ContainerYardParkPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.ContainerYardParkPanel.TabIndex = 4;
			//
			// CYPTypesGrid
			//
			this.CYPTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CYPTypesGrid, "CarrierAppointedAgentPorts_ContainerYardPark.ContainerTypes");
			this.CYPTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "PT_ContainerStorageClass";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|bde91688-5fd8-4d93-b60a-14f15c46a5df", "Desc.", "Desc.", "Description");
			zTextBoxColumnStyleInfo6.ColumnName = "ContainerStorageClassDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|3f6754a0-202c-4995-af68-e57e1d6d77b8", "CY Work Order Approval Limit Currency");
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.ColumnName = "PT_RX_NKCYWorkOrderApprovalLimitCurrency";
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.IsVisible = false;
			zCalcEditCYWorkOrderApprovalLimit.BindToDecimalPlaces = null;
			zCalcEditCYWorkOrderApprovalLimit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|2906fef3-d813-48a8-8a96-06445fcf930c", "CY Work Order Approval Limit");
			zCalcEditCYWorkOrderApprovalLimit.ColumnName = "PT_CYWorkOrderApprovalLimit";
			zCalcEditCYWorkOrderApprovalLimit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditCYWorkOrderApprovalLimit.IsVisible = false;
			zTextBoxCYWorkOrderApprovalNumber.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|70bb5667-0e4e-461e-9f81-a995c8809e71", "CY Work Order Approval Number");
			zTextBoxCYWorkOrderApprovalNumber.ColumnName = "PT_CYWorkOrderApprovalNumber";
			zTextBoxCYWorkOrderApprovalNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxCYWorkOrderApprovalNumber.IsVisible = false;
			zGuidCYWorkOrderApprovedBy.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|0ede6705-8b93-489f-9cd5-946cec4f66c4", "CY Work Order Approved By");
			zGuidCYWorkOrderApprovedBy.ColumnName = "PT_OC_CYWorkOrderApprovedBy";
			zGuidCYWorkOrderApprovedBy.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidCYWorkOrderApprovedBy.IsVisible = false;
			this.CYPTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CYPTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CYPTypesGrid.ColumnStyles.Add(zCodeFindBoxCYWorkOrderApprovalLimitCurrency);
			this.CYPTypesGrid.ColumnStyles.Add(zCalcEditCYWorkOrderApprovalLimit);
			this.CYPTypesGrid.ColumnStyles.Add(zTextBoxCYWorkOrderApprovalNumber);
			this.CYPTypesGrid.ColumnStyles.Add(zGuidCYWorkOrderApprovedBy);
			this.CYPTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYPTypesGrid.GridId = "fddf71e4-c73a-4a75-95c6-c290d295ef5b";
			this.CYPTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CYPTypesGrid.LayoutKey = "zGrid4";
			this.CYPTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYPTypesGrid.Name = "CYPTypesGrid";
			this.CYPTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 112, true);
			this.CYPTypesGrid.TabIndex = 5;
			//
			// CYPGrid
			//
			this.CYPGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CYPGrid, "CarrierAppointedAgentPorts_ContainerYardPark");
			this.CYPGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|e6cbcc59-9c3c-4ab9-a9ad-9ad00d919d2a", "Port");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|21135c42-0b2b-4177-8850-09f0cbf3d3c8", "Yard/Park");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|01deee66-4e86-4d1d-b8fb-54f117dcdae0", "Name", "Handling Agent Name.");
			zTextBoxColumnStyleInfo5.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|b6aa6c81-d3f3-4373-bde0-7ed49d35dd8f", "Address", "Agent Office Address.");
			zGuidDropEditColumnStyleInfo5.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo5.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo1.ColumnName = "O5_AgentDirection";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CYPGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.CYPGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CYPGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CYPGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo5);
			this.CYPGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CYPGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYPGrid.GridId = "8842669f-3ec2-4497-8262-05dbb24058e8";
			this.CYPGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CYPGrid.LayoutKey = "zGrid4";
			this.CYPGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYPGrid.Name = "CYPGrid";
			this.CYPGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 112, true);
			this.CYPGrid.TabIndex = 4;
			//
			// CYSplitContainer
			//
			this.CYSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYSplitContainer.Name = "CYSplitContainer";
			//
			// CYSplitContainer.Panel1
			//
			this.CYSplitContainer.Panel1.Controls.Add(this.CYPGrid);
			//
			// CYSplitContainer.Panel2
			//
			this.CYSplitContainer.Panel2.Controls.Add(this.CYPTypesGrid);
			this.CYSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.CYSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(598);
			this.CYSplitContainer.TabIndex = 6;
			this.ContainerYardTabPage.PerformLayout();
			this.ContainerYardParkPanel.ResumeLayout(false);
			this.ContainerYardParkPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CYPTypesGrid)).EndInit();
			this.CYPTypesGrid.ResumeLayout(false);
			this.CYPTypesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CYPGrid)).EndInit();
			this.CYPGrid.ResumeLayout(false);
			this.CYPGrid.PerformLayout();
			this.CYSplitContainer.Panel1.ResumeLayout(false);
			this.CYSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CYSplitContainer)).EndInit();
			this.CYSplitContainer.ResumeLayout(false);
			this.CYSplitContainer.PerformLayout();
			this.ContainerYardTabPage.ResumeLayout(true);

		}

		void agenciesTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.agencyGrid = new Enterprise.ZArchitecture.ZGrid();
			this.agenciesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.agencyGrid)).BeginInit();
			this.agencyGrid.SuspendLayout();
			this.agenciesTabPage.Controls.Add(this.agencyGrid);
			//
			// agencyGrid
			//
			this.agencyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.agencyGrid, "CarrierAppointedAgentPorts_Agency");
			this.agencyGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|63f6381a-92cc-4068-a9c5-96025d6ccdcd", "Port");
			zCodeFindBoxColumnStyleInfo6.ColumnName = "O5_PortOrCountry";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|fb8d9659-2c3d-4700-8e55-300af68e5d96", "Agency", "Agency Organization Code.");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|01560182-985a-40b0-a0b9-e3317a952679", "Full Name", "Agency Organization Full Name.");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo7.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|7c22141a-0525-42c1-b748-62eb5a7631fe", "Address", "Agency Address.");
			zGuidDropEditColumnStyleInfo6.ColumnName = "O5_OA_AgentOfficeAddress";
			zGuidDropEditColumnStyleInfo6.ShowHorizontalScrollBar = false;
			zGuidDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.agencyGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.agencyGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.agencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.agencyGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo6);
			this.agencyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.agencyGrid.GridId = "6ea49368-53fe-4de6-b66c-7c6228d75111";
			this.agencyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.agencyGrid.LayoutKey = "agencyGrid";
			this.agencyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.agencyGrid.Name = "agencyGrid";
			this.agencyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 112, true);
			this.agencyGrid.TabIndex = 4;
			this.agenciesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.agencyGrid)).EndInit();
			this.agencyGrid.ResumeLayout(false);
			this.agencyGrid.PerformLayout();
			this.agenciesTabPage.ResumeLayout(true);

		}

		private ContainerPenaltiesUserControl ContainerPenaltiesControl;
		private ZPanel AirlineDetailsPanel;
		private ZTabPage RefShippingLineTabPage;
		private ZPanel RefShippingLinePanel;
		private ZGuidFindBox RefShippingLineCodeFindBox;
		private ZTextBox RefShippingLineSCACCode;
		private ZTextBox RefShippingLineCarrierName;
		private ZDropEdit VoyageRecyclingDropEdit;
		private ZDropEdit CarrierPackageGroupingDropEdit;
		private ZTabPage AccountNumbersTabPage;
		private ZTabPage MAWBStockManagementTabPage;
		private ZArchitecture.ZGrid AccountNumbersGrid;
		private ZArchitecture.ZGrid MAWBStockManagementGrid;
		private ZGroupBox IntegrationsGroupBox;
		private ZLabel IntegrationsLabel;
		protected ZCheckBox OM_TBAllowMixedAccountNumbersOnManifestCheckEdit;
	}
}
