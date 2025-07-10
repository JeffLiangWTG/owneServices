namespace Enterprise.MasterFiles.GUI
{
	public partial class MiscServicesUserControl
	{

		#region Component Designer generated code

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl PayablesDetailsTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ServicesDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox DepotDetailsGroupBox;
		CargoWise.Windows.UI.KSplitContainer ServicesSplitContainer;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox RoadFreightDepotCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox RailHeadCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsFumigationContractorBoundCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsContainerYardBoundCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsSeaCTOBoundCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsAirCTOBoundCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsUnpackDepotBoundCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsPackDepotBoundCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox FerryWaterTerminalCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsContainerLeasingCompanyCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsVGMContractorCheckBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit OM_SVServicesCategoryBoundDropEdit;
		protected internal ContainerYardConfigurationUserControl ContainerYardCarrierRelatedPartyControl;
		protected internal MiscServiceCTOStorageUserControl MiscServiceCTOStorageUserControl;
		protected internal MiscServiceFacilityUserControl MiscServiceFacilityUserControl;
		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ServiceTypePanel;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl FacilityTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage RefFacilityTabPage;

		private Enterprise.ZArchitecture.GUI.ZCheckBox DistributionCentreCheckBox;
		System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ServicesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ServiceTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DistributionCentreCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FerryWaterTerminalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RoadFreightDepotCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RailHeadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsFumigationContractorBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsContainerYardBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsSeaCTOBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsAirCTOBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsUnpackDepotBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsPackDepotBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsContainerLeasingCompanyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsVGMContractorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_SVServicesCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FacilityTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.RefFacilityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MiscServiceFacilityUserControl = new Enterprise.MasterFiles.GUI.MiscServiceFacilityUserControl();
			this.ContainerYardCarrierRelatedPartyControl = new Enterprise.MasterFiles.GUI.ContainerYardConfigurationUserControl();
			this.MiscServiceCTOStorageUserControl = new Enterprise.MasterFiles.GUI.MiscServiceCTOStorageUserControl();
			this.PayablesDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ServicesDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepotDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesSplitContainer)).BeginInit();
			this.ServicesSplitContainer.Panel1.SuspendLayout();
			this.ServicesSplitContainer.Panel2.SuspendLayout();
			this.ServicesSplitContainer.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.ServiceTypePanel.SuspendLayout();
			this.OM_SVServicesCategoryBoundDropEdit.SuspendLayout();
			this.FacilityTabControl.SuspendLayout();
			this.RefFacilityTabPage.SuspendLayout();
			this.MiscServiceFacilityUserControl.SuspendLayout();
			this.ContainerYardCarrierRelatedPartyControl.SuspendLayout();
			this.MiscServiceCTOStorageUserControl.SuspendLayout();
			this.PayablesDetailsTabControl.SuspendLayout();
			this.ServicesDetailsTabPage.SuspendLayout();
			this.DepotDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ServicesSplitContainer
			// 
			this.ServicesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.ServicesSplitContainer.IsSplitterFixed = true;
			this.ServicesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.ServicesSplitContainer.Name = "ServicesSplitContainer";
			this.ServicesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ServicesSplitContainer.Panel1
			// 
			this.ServicesSplitContainer.Panel1.Controls.Add(this.MainPanel);
			this.ServicesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 680, true);
			this.ServicesSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(424);
			// 
			// ServicesSplitContainer.Panel2
			// 
			this.ServicesSplitContainer.Panel2.Controls.Add(this.ContainerYardCarrierRelatedPartyControl);
			this.ServicesSplitContainer.Panel2.Controls.Add(this.MiscServiceCTOStorageUserControl);
			this.ServicesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(424);
			this.ServicesSplitContainer.SplitterWidth = 10;
			this.ServicesSplitContainer.TabIndex = 10;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.ServiceTypePanel);
			this.MainPanel.Controls.Add(this.FacilityTabControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 424, true);
			this.MainPanel.TabIndex = 50;
			// 
			// ServiceTypePanel
			// 
			this.ServiceTypePanel.Controls.Add(this.DistributionCentreCheckBox);
			this.ServiceTypePanel.Controls.Add(this.FerryWaterTerminalCheckBox);
			this.ServiceTypePanel.Controls.Add(this.RoadFreightDepotCheckBox);
			this.ServiceTypePanel.Controls.Add(this.RailHeadCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsFumigationContractorBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsContainerYardBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsSeaCTOBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsAirCTOBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsUnpackDepotBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsPackDepotBoundCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsContainerLeasingCompanyCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OH_IsVGMContractorCheckBox);
			this.ServiceTypePanel.Controls.Add(this.OM_SVServicesCategoryBoundDropEdit);
			this.ServiceTypePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ServiceTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServiceTypePanel.Name = "ServiceTypePanel";
			this.ServiceTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 424, true);
			this.ServiceTypePanel.TabIndex = 49;
			// 
			// DistributionCentreCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DistributionCentreCheckBox, "OH_IsDistributionCentre");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsDistributionCentre)));
			this.DistributionCentreCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("987bfe4f-445d-4605-bc8a-243648f04dbc", "DC", "Distribution Center", "");
			this.DistributionCentreCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DistributionCentreCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 294, true);
			this.DistributionCentreCheckBox.Name = "DistributionCentreCheckBox";
			this.DistributionCentreCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.DistributionCentreCheckBox.TabIndex = 9;
			// 
			// FerryWaterTerminalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FerryWaterTerminalCheckBox, "OH_IsFerryWaterTerminal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsFerryWaterTerminal)));
			this.FerryWaterTerminalCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e9242f17-789a-42c5-8c8d-090667919a75", "Ferry/Inland Water Terminal");
			this.FerryWaterTerminalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FerryWaterTerminalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 234, true);
			this.FerryWaterTerminalCheckBox.Name = "FerryWaterTerminalCheckBox";
			this.FerryWaterTerminalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.FerryWaterTerminalCheckBox.TabIndex = 7;
			// 
			// RoadFreightDepotCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RoadFreightDepotCheckBox, "OH_IsRoadFreightDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsRoadFreightDepot)));
			this.RoadFreightDepotCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|69e8a8d4-76cd-4d83-8ac0-1a723f7657c1", "Road Depot/Transit Shed");
			this.RoadFreightDepotCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RoadFreightDepotCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 204, true);
			this.RoadFreightDepotCheckBox.Name = "RoadFreightDepotCheckBox";
			this.RoadFreightDepotCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.RoadFreightDepotCheckBox.TabIndex = 6;
			// 
			// RailHeadCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RailHeadCheckBox, "OH_IsRailHead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsRailHead)));
			this.RailHeadCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|f7254eda-838d-419f-9ebb-c8e19e626dac", "Rail Head/Depot");
			this.RailHeadCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RailHeadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 174, true);
			this.RailHeadCheckBox.Name = "RailHeadCheckBox";
			this.RailHeadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.RailHeadCheckBox.TabIndex = 5;
			// 
			// OH_IsFumigationContractorBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsFumigationContractorBoundCheckBox, "OH_IsFumigationContractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsFumigationContractor)));
			this.OH_IsFumigationContractorBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|afed9237-25b3-4aed-a2f5-1a3638d24452", "Fumigation Contractor");
			this.OH_IsFumigationContractorBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsFumigationContractorBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 324, true);
			this.OH_IsFumigationContractorBoundCheckBox.Name = "OH_IsFumigationContractorBoundCheckBox";
			this.OH_IsFumigationContractorBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsFumigationContractorBoundCheckBox.TabIndex = 10;
			// 
			// OH_IsContainerYardBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsContainerYardBoundCheckBox, "OH_IsContainerYard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsContainerYard)));
			this.OH_IsContainerYardBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|4dd31341-ba2a-4338-9c9d-93463827275e", "Container Yard");
			this.OH_IsContainerYardBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsContainerYardBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 264, true);
			this.OH_IsContainerYardBoundCheckBox.Name = "OH_IsContainerYardBoundCheckBox";
			this.OH_IsContainerYardBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsContainerYardBoundCheckBox.TabIndex = 8;
			// 
			// OH_IsSeaCTOBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsSeaCTOBoundCheckBox, "OH_IsSeaCTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsSeaCTO)));
			this.OH_IsSeaCTOBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|6503e136-f34e-4587-ad30-9a93db6a5c01", "Sea CTO/Stevedore");
			this.OH_IsSeaCTOBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsSeaCTOBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 144, true);
			this.OH_IsSeaCTOBoundCheckBox.Name = "OH_IsSeaCTOBoundCheckBox";
			this.OH_IsSeaCTOBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsSeaCTOBoundCheckBox.TabIndex = 4;
			// 
			// OH_IsAirCTOBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsAirCTOBoundCheckBox, "OH_IsAirCTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsAirCTO)));
			this.OH_IsAirCTOBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|88b3b9de-1199-484d-b115-87971855904d", "Air CTO");
			this.OH_IsAirCTOBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsAirCTOBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 114, true);
			this.OH_IsAirCTOBoundCheckBox.Name = "OH_IsAirCTOBoundCheckBox";
			this.OH_IsAirCTOBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsAirCTOBoundCheckBox.TabIndex = 3;
			// 
			// OH_IsUnpackDepotBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsUnpackDepotBoundCheckBox, "OH_IsUnpackDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsUnpackDepot)));
			this.OH_IsUnpackDepotBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|9577ae62-d343-4298-8768-1e61e7c8ff7e", "Unpacking CFS");
			this.OH_IsUnpackDepotBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsUnpackDepotBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 84, true);
			this.OH_IsUnpackDepotBoundCheckBox.Name = "OH_IsUnpackDepotBoundCheckBox";
			this.OH_IsUnpackDepotBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsUnpackDepotBoundCheckBox.TabIndex = 2;
			// 
			// OH_IsPackDepotBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsPackDepotBoundCheckBox, "OH_IsPackDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsPackDepot)));
			this.OH_IsPackDepotBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|6cf0dce2-4116-4958-bc0e-c77e7749443f", "Packing CFS");
			this.OH_IsPackDepotBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsPackDepotBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 54, true);
			this.OH_IsPackDepotBoundCheckBox.Name = "OH_IsPackDepotBoundCheckBox";
			this.OH_IsPackDepotBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsPackDepotBoundCheckBox.TabIndex = 1;
			// 
			// OH_IsContainerLeasingCompanyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsContainerLeasingCompanyCheckBox, "OH_IsContainerLeasingCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsContainerLeasingCompany)));
			this.OH_IsContainerLeasingCompanyCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|8A9EA521-43B7-475D-BDC2-BBF425864361", "Container Leasing Company");
			this.OH_IsContainerLeasingCompanyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsContainerLeasingCompanyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 354, true);
			this.OH_IsContainerLeasingCompanyCheckBox.Name = "OH_IsContainerLeasingCompanyCheckBox";
			this.OH_IsContainerLeasingCompanyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsContainerLeasingCompanyCheckBox.TabIndex = 11;
			// 
			// OH_IsVGMContractorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsVGMContractorCheckBox, "OH_IsVGMContractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsVGMContractor)));
			this.OH_IsVGMContractorCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|99EF928F-6F1B-4FFF-8B8D-C10BA58249E6", "VGM Contractor");
			this.OH_IsVGMContractorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsVGMContractorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 384, true);
			this.OH_IsVGMContractorCheckBox.Name = "OH_IsVGMContractorCheckBox";
			this.OH_IsVGMContractorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 24, true);
			this.OH_IsVGMContractorCheckBox.TabIndex = 12;
			// 
			// OM_SVServicesCategoryBoundDropEdit
			// 
			this.OM_SVServicesCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_SVServicesCategoryBoundDropEdit, "MiscServ.OM_SVServicesCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_SVServicesCategory)));
			this.OM_SVServicesCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 16, true);
			this.OM_SVServicesCategoryBoundDropEdit.Name = "OM_SVServicesCategoryBoundDropEdit";
			this.OM_SVServicesCategoryBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_SVServicesCategoryBoundDropEdit.ShowDescriptionBox = false;
			this.OM_SVServicesCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 15, true);
			this.OM_SVServicesCategoryBoundDropEdit.TabIndex = 0;
			// 
			// FacilityTabControl
			// 
			this.FacilityTabControl.Controls.Add(this.RefFacilityTabPage);
			this.FacilityTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 3, true);
			this.FacilityTabControl.Name = "FacilityTabControl";
			this.FacilityTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 410, true);
			this.FacilityTabControl.Dock = System.Windows.Forms.DockStyle.None;
			this.FacilityTabControl.TabIndex = 0;
			this.FacilityTabControl.TabStop = false;
			// 
			// RefFacilityTabPage
			// 
			this.RefFacilityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|b41b9435-3cb9-4994-b43b-c45374ac2125", "Facility");
			this.RefFacilityTabPage.Controls.Add(this.MiscServiceFacilityUserControl);
			this.RefFacilityTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefFacilityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.RefFacilityTabPage.Name = "RefFacilityTabPage";
			this.RefFacilityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RefFacilityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 410, true);
			this.RefFacilityTabPage.TabIndex = 0;
			this.RefFacilityTabPage.UseVisualStyleBackColor = true;
			// 
			// MiscServiceFacilityUserControl
			// 
			this.MiscServiceFacilityUserControl.AllowDrop = true;
			this.MiscServiceFacilityUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BindingSource.SetBindingMember(this.MiscServiceFacilityUserControl, ".");
			this.MiscServiceFacilityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscServiceFacilityUserControl.Name = "MiscServiceFacilityUserControl";
			this.MiscServiceFacilityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 410, true);
			this.MiscServiceFacilityUserControl.TabIndex = 12;
			this.MiscServiceFacilityUserControl.AutoScroll = true;
			// 
			// ContainerYardCarrierRelatedPartyControl
			// 
			this.ContainerYardCarrierRelatedPartyControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerYardCarrierRelatedPartyControl, ".");
			this.ContainerYardCarrierRelatedPartyControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardCarrierRelatedPartyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.ContainerYardCarrierRelatedPartyControl.Name = "ContainerYardCarrierRelatedPartyControl";
			this.ContainerYardCarrierRelatedPartyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 132, true);
			this.ContainerYardCarrierRelatedPartyControl.TabIndex = 11;
			// 
			// MiscServiceCTOStorageUserControl
			// 
			this.MiscServiceCTOStorageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MiscServiceCTOStorageUserControl, ".");
			this.MiscServiceCTOStorageUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.MiscServiceCTOStorageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscServiceCTOStorageUserControl.Name = "MiscServiceCTOStorageUserControl";
			this.MiscServiceCTOStorageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 150, true);
			this.MiscServiceCTOStorageUserControl.TabIndex = 10;
			// 
			// PayablesDetailsTabControl
			// 
			this.PayablesDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PayablesDetailsTabControl.Controls.Add(this.ServicesDetailsTabPage);
			this.PayablesDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayablesDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.PayablesDetailsTabControl.Name = "PayablesDetailsTabControl";
			this.PayablesDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 728, true);
			this.PayablesDetailsTabControl.TabIndex = 15;
			// 
			// ServicesDetailsTabPage
			// 
			this.ServicesDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|9faf736d-2683-4f50-9025-c68c58b771b9", "Configuration");
			this.ServicesDetailsTabPage.Controls.Add(this.DepotDetailsGroupBox);
			this.ServicesDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ServicesDetailsTabPage.Name = "ServicesDetailsTabPage";
			this.ServicesDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ServicesDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 705, true);
			this.ServicesDetailsTabPage.TabIndex = 0;
			// 
			// DepotDetailsGroupBox
			// 
			this.DepotDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MiscServicesUserControl|798374d5-9b94-4cb6-b4ed-514c35f716ce", "Service Type");
			this.DepotDetailsGroupBox.Controls.Add(this.ServicesSplitContainer);
			this.DepotDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepotDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.DepotDetailsGroupBox.Name = "DepotDetailsGroupBox";
			this.DepotDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 695, true);
			this.DepotDetailsGroupBox.TabIndex = 1;
			this.DepotDetailsGroupBox.TabStop = false;
			// 
			// MiscServicesUserControl
			// 
			this.Controls.Add(this.PayablesDetailsTabControl);
			this.IsModifyServices = true;
			this.Name = "MiscServicesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 752, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.PayablesDetailsTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServicesSplitContainer.Panel1.ResumeLayout(false);
			this.ServicesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ServicesSplitContainer)).EndInit();
			this.ServicesSplitContainer.ResumeLayout(false);
			this.ServicesSplitContainer.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ServiceTypePanel.ResumeLayout(false);
			this.ServiceTypePanel.PerformLayout();
			this.OM_SVServicesCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_SVServicesCategoryBoundDropEdit.PerformLayout();
			this.FacilityTabControl.ResumeLayout(false);
			this.FacilityTabControl.PerformLayout();
			this.RefFacilityTabPage.ResumeLayout(false);
			this.RefFacilityTabPage.PerformLayout();
			this.MiscServiceFacilityUserControl.ResumeLayout(true);
			this.MiscServiceFacilityUserControl.PerformLayout();
			this.ContainerYardCarrierRelatedPartyControl.ResumeLayout(true);
			this.ContainerYardCarrierRelatedPartyControl.PerformLayout();
			this.MiscServiceCTOStorageUserControl.ResumeLayout(true);
			this.MiscServiceCTOStorageUserControl.PerformLayout();
			this.PayablesDetailsTabControl.ResumeLayout(false);
			this.PayablesDetailsTabControl.PerformLayout();
			this.ServicesDetailsTabPage.ResumeLayout(false);
			this.ServicesDetailsTabPage.PerformLayout();
			this.DepotDetailsGroupBox.ResumeLayout(false);
			this.DepotDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
