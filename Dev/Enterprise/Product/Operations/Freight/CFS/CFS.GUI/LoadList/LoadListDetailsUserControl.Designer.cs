using Enterprise.Registry.Business;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class LoadListDetailsUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CFSLoadListConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JK_OH_ForwarderBoundOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.AboveRoutePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_ShippingLineAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_CartageCoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_CTOAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_EmptyContainerYardControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.LeftPanel = new CargoWise.Windows.UI.KPanel();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TransportModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierBookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_MasterBillNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_RL_NKLoadPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClientRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoadListConsolLegDetailsControl = new Enterprise.Freight.GUI.ConsoLegDetailsControl();
			this.NumbersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.referenceNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			this.ShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ControlsToBind = new CargoWise.Windows.UI.KPanel();
			this.JK_TotalShipmentQuantityCalEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalShipmentVolumeCalc = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalShipmentVolumeLabel = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentReceivalModuleButtonGrid = new Enterprise.Freight.CFS.GUI.LoadListShipmentsModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CFSLoadListConsolGroupBox.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.AboveRoutePanel.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.NumbersPanel.SuspendLayout();
			this.ShipmentsGroupBox.SuspendLayout();
			this.ControlsToBind.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentReceivalModuleButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.CFS.Business.CFSLoadListConsol);
			// 
			// CFSLoadListConsolGroupBox
			// 
			this.CFSLoadListConsolGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|0f53eef3-d7a4-48d1-a5b6-f2cbca6cc29b", "Load List Details");
			this.CFSLoadListConsolGroupBox.Controls.Add(this.RightPanel);
			this.CFSLoadListConsolGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CFSLoadListConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CFSLoadListConsolGroupBox.Name = "CFSLoadListConsolGroupBox";
			this.CFSLoadListConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 309, true);
			this.CFSLoadListConsolGroupBox.TabIndex = 0;
			this.CFSLoadListConsolGroupBox.TabStop = false;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.JK_OH_ForwarderBoundOrganisationControl);
			this.RightPanel.Controls.Add(this.AboveRoutePanel);
			this.RightPanel.Controls.Add(this.LeftPanel);
			this.RightPanel.Controls.Add(this.LoadListConsolLegDetailsControl);
			this.RightPanel.Controls.Add(this.NumbersPanel);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 5, 0, true);
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 290, true);
			this.RightPanel.TabIndex = 1;
			// 
			// AboveRoutePanel
			// 
			this.AboveRoutePanel.Controls.Add(this.DepotAddressControl);
			this.AboveRoutePanel.Controls.Add(this.JK_OA_ShippingLineAddressControl);
			this.AboveRoutePanel.Controls.Add(this.JK_OA_CartageCoAddressControl);
			this.AboveRoutePanel.Controls.Add(this.JK_OA_CTOAddressControl);
			this.AboveRoutePanel.Controls.Add(this.JK_OA_EmptyContainerYardControl);
			this.AboveRoutePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 8, true);
			this.AboveRoutePanel.Name = "AboveRoutePanel";
			this.AboveRoutePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 146, true);
			this.AboveRoutePanel.TabIndex = 2;
			// 
			// JK_OA_ShippingLineAddressControl
			// 
			this.JK_OA_ShippingLineAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ShippingLineAddressControl, "JK_OA_ShippingLineAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OA_ShippingLineAddress)));
			this.JK_OA_ShippingLineAddressControl.BindToOrgList = "ShippingProviderList";
			this.JK_OA_ShippingLineAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 5, true);
			this.JK_OA_ShippingLineAddressControl.Name = "JK_OA_ShippingLineAddressControl";
			this.JK_OA_ShippingLineAddressControl.PopupCaption = "";
			this.JK_OA_ShippingLineAddressControl.ShowAddress = false;
			this.JK_OA_ShippingLineAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.JK_OA_ShippingLineAddressControl.TabIndex = 9;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "JK_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKDischargePort)));
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 28, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 5;
			this.zCodeFindBox1.ShowDescriptionBox = false;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			// 
			// JK_RL_NKLoadPortBoundCodeFindBox
			// 
			this.JK_RL_NKLoadPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKLoadPortBoundCodeFindBox, "JK_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKLoadPort)));
			this.JK_RL_NKLoadPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 28, true);
			this.JK_RL_NKLoadPortBoundCodeFindBox.Name = "JK_RL_NKLoadPortBoundCodeFindBox";
			this.JK_RL_NKLoadPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RL_NKLoadPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKLoadPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKLoadPortBoundCodeFindBox.TabIndex = 3;
			this.zCodeFindBox1.TabIndex = 4;
			// 
			// DepotAddressControl
			// 
			this.DepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepotAddressControl, "JK_OA_DepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OA_DepotAddress)));
			this.DepotAddressControl.BindToOrgList = "Depot_List";
			this.DepotAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("a410429b-3c2a-4fc5-8505-65ae4c50c370", "Depot");
			this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 98, true);
			this.DepotAddressControl.Name = "DepotAddressControl";
			this.DepotAddressControl.PopupCaption = "";
			this.DepotAddressControl.ShowAddress = false;
			this.DepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.DepotAddressControl.TabIndex = 13;
			// 
			// JK_OA_CartageCoAddressControl
			// 
			this.JK_OA_CartageCoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_CartageCoAddressControl, "JK_OA_CartageCoAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OA_CartageCoAddress)));
			this.JK_OA_CartageCoAddressControl.BindToOrgList = "LocalTransport_List";
			this.JK_OA_CartageCoAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|9612738e-0a46-4175-95ab-0174c95628c4", "Local Transport");
			this.JK_OA_CartageCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 75, true);
			this.JK_OA_CartageCoAddressControl.Name = "JK_OA_CartageCoAddressControl";
			this.JK_OA_CartageCoAddressControl.PopupCaption = "";
			this.JK_OA_CartageCoAddressControl.ShowAddress = false;
			this.JK_OA_CartageCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.JK_OA_CartageCoAddressControl.TabIndex = 12;
			// 
			// JK_OA_CTOAddressControl
			// 
			this.JK_OA_CTOAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_CTOAddressControl, "JK_OA_CTOAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OA_CTOAddress)));
			this.JK_OA_CTOAddressControl.BindToOrgList = "CTOAddress_List";
			this.JK_OA_CTOAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|0edfc5cf-8bec-416b-bd74-6b061b21a0be", "CTO");
			this.JK_OA_CTOAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 52, true);
			this.JK_OA_CTOAddressControl.Name = "JK_OA_CTOAddressControl";
			this.JK_OA_CTOAddressControl.PopupCaption = "";
			this.JK_OA_CTOAddressControl.ShowAddress = false;
			this.JK_OA_CTOAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.JK_OA_CTOAddressControl.TabIndex = 11;
			// 
			// JK_OA_EmptyContainerYardControl
			// 
			this.JK_OA_EmptyContainerYardControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_EmptyContainerYardControl, "JK_OA_EmptyContainerYard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OA_EmptyContainerYard)));
			this.JK_OA_EmptyContainerYardControl.BindToOrgList = "EmptyContainerYard_List";
			this.JK_OA_EmptyContainerYardControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|81134dde-4652-4438-b44a-f20463107a2c", "Container Yard");
			this.JK_OA_EmptyContainerYardControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 28, true);
			this.JK_OA_EmptyContainerYardControl.Name = "JK_OA_EmptyContainerYardControl";
			this.JK_OA_EmptyContainerYardControl.PopupCaption = "";
			this.JK_OA_EmptyContainerYardControl.ShowAddress = false;
			this.JK_OA_EmptyContainerYardControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.JK_OA_EmptyContainerYardControl.TabIndex = 10;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.EntryNumberTextBox);
			this.LeftPanel.Controls.Add(this.JK_TransportModeBoundDropEdit);
			this.LeftPanel.Controls.Add(this.CarrierBookingRefTextBox);
			this.LeftPanel.Controls.Add(this.zCodeFindBox1);
			this.LeftPanel.Controls.Add(this.JK_MasterBillNumTextBox);
			this.LeftPanel.Controls.Add(this.JK_RL_NKLoadPortBoundCodeFindBox);
			this.LeftPanel.Controls.Add(this.ContainerModeDropEdit);
			this.LeftPanel.Controls.Add(this.ClientRefTextBox);
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 8, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 146, true);
			this.LeftPanel.TabIndex = 1;
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "JK_CustomsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_CustomsReference)));
			this.EntryNumberTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|7d5e8d1b-6648-4faa-a6e5-bb6cf6301104", "Entry No.");
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 121, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.EntryNumberTextBox.TabIndex = 8;
			// 
			// JK_OH_ForwarderBoundOrganisationControl
			// 
			this.JK_OH_ForwarderBoundOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OH_ForwarderBoundOrganisationControl, "JK_OH_ForwarderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OH_ForwarderForBinding)));
			this.JK_OH_ForwarderBoundOrganisationControl.BindToOrganisations = "Forwarder_List";
			this.JK_OH_ForwarderBoundOrganisationControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|3b5a8f54-0b4e-4c5b-8ced-f4111fcbd1b5", "Client");
			this.JK_OH_ForwarderBoundOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.JK_OH_ForwarderBoundOrganisationControl.Name = "JK_OH_ForwarderBoundOrganisationControl";
			this.JK_OH_ForwarderBoundOrganisationControl.PopupCaption = "";
			this.JK_OH_ForwarderBoundOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.JK_OH_ForwarderBoundOrganisationControl.TabIndex = 0;
			// 
			// JK_TransportModeBoundDropEdit
			// 
			this.JK_TransportModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_TransportModeBoundDropEdit, "JK_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TransportMode)));
			this.JK_TransportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 5, true);
			this.JK_TransportModeBoundDropEdit.Name = "JK_TransportModeBoundDropEdit";
			this.JK_TransportModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JK_TransportModeBoundDropEdit.ShowDescriptionBox = false;
			this.JK_TransportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_TransportModeBoundDropEdit.TabIndex = 1;
			// 
			// CarrierBookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierBookingRefTextBox, "JK_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_BookingReference)));
			this.CarrierBookingRefTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|a5cd09fe-062a-456f-88e6-ea92a1bf7a87", "Carrier Ref");
			this.CarrierBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 75, true);
			this.CarrierBookingRefTextBox.Name = "CarrierBookingRefTextBox";
			this.CarrierBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.CarrierBookingRefTextBox.TabIndex = 6;
			// 
			// JK_MasterBillNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_MasterBillNumTextBox, "JK_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_MasterBillNum)));
			this.JK_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 98, true);
			this.JK_MasterBillNumTextBox.Name = "JK_MasterBillNumTextBox";
			this.JK_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.JK_MasterBillNumTextBox.TabIndex = 7;
			// 
			// ContainerModeDropEdit
			// 
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "JK_ConsolMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_ConsolMode)));
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 5, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.ContainerModeDropEdit.ShowDescriptionBox = false;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ContainerModeDropEdit.TabIndex = 2;
			// 
			// ClientRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientRefTextBox, "JK_AgentsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_AgentsReference)));
			this.ClientRefTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|49e466c7-364d-4d5f-98ba-0e02f5499fca", "Client Ref");
			this.ClientRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 52, true);
			this.ClientRefTextBox.Name = "ClientRefTextBox";
			this.ClientRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.ClientRefTextBox.TabIndex = 5;
			// 
			// LoadListConsolLegDetailsControl
			// 
			this.LoadListConsolLegDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadListConsolLegDetailsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonConsol)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)))));
			this.LoadListConsolLegDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 160, true);
			this.LoadListConsolLegDetailsControl.Name = "LoadListConsolLegDetailsControl";
			this.LoadListConsolLegDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 127, true);
			this.LoadListConsolLegDetailsControl.TabIndex = 3;
			// 
			// NumbersPanel
			// 
			this.NumbersPanel.Controls.Add(this.referenceNumbersControl);
			this.NumbersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 160, true);
			this.NumbersPanel.Name = "NumbersPanel";
			this.NumbersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 127, true);
			this.NumbersPanel.TabIndex = 5;
			// 
			// referenceNumbersControl
			// 
			this.referenceNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.referenceNumbersControl, "Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Numbers)));
			this.referenceNumbersControl.DisplayDetailPanel = false;
			this.referenceNumbersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.referenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.referenceNumbersControl.Name = "referenceNumbersControl";
			this.referenceNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 127, true);
			this.referenceNumbersControl.TabIndex = 4;
			// 
			// ShipmentsGroupBox
			// 
			this.ShipmentsGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|58086d46-4c13-4a1d-aeb9-14d958454bbd", "Shipments");
			this.ShipmentsGroupBox.Controls.Add(this.ControlsToBind);
			this.ShipmentsGroupBox.Controls.Add(this.ShipmentReceivalModuleButtonGrid);
			this.ShipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 309, true);
			this.ShipmentsGroupBox.Name = "ShipmentsGroupBox";
			this.ShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 268, true);
			this.ShipmentsGroupBox.TabIndex = 5;
			this.ShipmentsGroupBox.TabStop = false;
			// 
			// ControlsToBind
			// 
			this.ControlsToBind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ControlsToBind.Controls.Add(this.JK_TotalShipmentQuantityCalEdit);
			this.ControlsToBind.Controls.Add(this.JK_TotalVolumeUnitTextBox);
			this.ControlsToBind.Controls.Add(this.JK_TotalShipmentVolumeCalc);
			this.ControlsToBind.Controls.Add(this.JK_TotalWeightUnitTextBox);
			this.ControlsToBind.Controls.Add(this.JK_TotalShipmentVolumeLabel);
			this.ControlsToBind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 233, true);
			this.ControlsToBind.Name = "ControlsToBind";
			this.ControlsToBind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 24, true);
			this.ControlsToBind.TabIndex = 6;
			// 
			// JK_TotalShipmentQuantityCalEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentQuantityCalEdit, "JK_TotalShipmentQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TotalShipmentQuantity)));
			this.JK_TotalShipmentQuantityCalEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|c18302c0-59e6-4243-bb91-4e6a84d215ab", "Packs");
			this.JK_TotalShipmentQuantityCalEdit.DecimalPlaces = 2;
			this.JK_TotalShipmentQuantityCalEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 2, true);
			this.JK_TotalShipmentQuantityCalEdit.Name = "JK_TotalShipmentQuantityCalEdit";
			this.JK_TotalShipmentQuantityCalEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JK_TotalShipmentQuantityCalEdit.TabIndex = 0;
			this.JK_TotalShipmentQuantityCalEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalVolumeUnitTextBox, "JK_TotalShipmentVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TotalShipmentVolumeUnit)));
			this.JK_TotalVolumeUnitTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|9a63f3e1-3a86-4273-9469-d515392189cd", "Total Shipment Volume Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalVolumeUnitTextBox, false);
			this.JK_TotalVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 2, true);
			this.JK_TotalVolumeUnitTextBox.Name = "JK_TotalVolumeUnitTextBox";
			this.JK_TotalVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.JK_TotalVolumeUnitTextBox.TabIndex = 4;
			// 
			// JK_TotalShipmentVolumeCalc
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentVolumeCalc, "JK_TotalShipmentWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TotalShipmentWeight)));
			this.JK_TotalShipmentVolumeCalc.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|e3145fd6-2991-47e0-ba59-67981e51ec1c", "Weight");
			this.JK_TotalShipmentVolumeCalc.DecimalPlaces = 2;
			this.JK_TotalShipmentVolumeCalc.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 2, true);
			this.JK_TotalShipmentVolumeCalc.Name = "JK_TotalShipmentVolumeCalc";
			this.JK_TotalShipmentVolumeCalc.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JK_TotalShipmentVolumeCalc.TabIndex = 1;
			this.JK_TotalShipmentVolumeCalc.Text = "0.000";
			this.JK_TotalShipmentVolumeCalc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalWeightUnitTextBox, "JK_TotalShipmentWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TotalShipmentWeightUnit)));
			this.JK_TotalWeightUnitTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|2de1c8ce-d964-4435-a9bb-a7c4555eac79", "Total Shipment Weight Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalWeightUnitTextBox, false);
			this.JK_TotalWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 2, true);
			this.JK_TotalWeightUnitTextBox.Name = "JK_TotalWeightUnitTextBox";
			this.JK_TotalWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.JK_TotalWeightUnitTextBox.TabIndex = 2;
			// 
			// JK_TotalShipmentVolumeLabel
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentVolumeLabel, "JK_TotalShipmentVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_TotalShipmentVolume)));
			this.JK_TotalShipmentVolumeLabel.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListDetailsUserControl|4d8c6aab-0bd1-4908-900c-e61c5b9f01c6", "Volume");
			this.JK_TotalShipmentVolumeLabel.DecimalPlaces = 2;
			this.JK_TotalShipmentVolumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 2, true);
			this.JK_TotalShipmentVolumeLabel.Name = "JK_TotalShipmentVolumeLabel";
			this.JK_TotalShipmentVolumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JK_TotalShipmentVolumeLabel.TabIndex = 3;
			this.JK_TotalShipmentVolumeLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentReceivalModuleButtonGrid
			// 
			this.ShipmentReceivalModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentReceivalModuleButtonGrid, "Shipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments_List)));
			this.ShipmentReceivalModuleButtonGrid.BindToFindBoxList = "Shipments_List";
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zMultiControlColumnStyleInfo1.Caption = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			zMultiControlColumnStyleInfo1.ColumnName = "ConsignorNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsignorFieldType";
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|b6e8830a-1c5f-4064-a04b-1082ec46e60c", "Consignee");
			zMultiControlColumnStyleInfo2.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ConsigneeFieldType";
			zTextBoxColumnStyleInfo2.ColumnName = "JS_HouseBill";
			zTextBoxColumnStyleInfo3.ColumnName = "JS_InterimReceipt";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "JS_A_RCV";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|d2b16a66-5e62-499d-a544-6f03f120a23b", "Packs");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.ColumnName = "JS_F3_NKPackType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|d2b16a66-5e62-499d-a544-6f03f120a23b", "Packs");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|12244fc0-de5f-488e-8241-785bbf3f0052", "Actual Weight");
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "JS_UnitOfWeight";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|12244fc0-de5f-488e-8241-785bbf3f0052", "Actual Weight");
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|f63bb3be-1e7d-4758-8ecd-eeb984a7def2", "Actual Volume");
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "JS_UnitOfVolume";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|f63bb3be-1e7d-4758-8ecd-eeb984a7def2", "Actual Volume");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JS_ActualChargeable";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|d9e64d2c-36ff-4783-af82-03e22c3136b3", "Chargeable Unit");
			zDropEditColumnStyleInfo4.ColumnName = "JS_ChargeableUnit";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JS_GoodsDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JS_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JS_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|1e2dccdf-fc0e-4b77-bff2-48447ea5bb42", "Marks And Numbers");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_MarksAndNumbers";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "JS_TranshipToOtherCFS";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "JS_E_DEP";
			zDateEditColumnStyleInfo3.ColumnName = "JS_E_ARV";
			zDropEditColumnStyleInfo5.ColumnName = "JS_PackingMode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "JS_ShipmentType";
			zDropEditColumnStyleInfo6.IsVisible = false;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|a7cf0ce3-4d7f-4274-b4ed-98fabbcb1498", "Master Shipment House Bill");
			zTextBoxColumnStyleInfo6.ColumnName = "ColoadMasterShipmentHouseBill";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|68d6cd0d-e877-47cb-81b3-e4e7b84d9dab", "Master/Lead");
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JS_JS_ColoadMasterShipment";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|68d6cd0d-e877-47cb-81b3-e4e7b84d9dab", "Master/Lead");
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|af4bf704-0cb6-4e43-b6b9-4d069764d31c", "Entry No.");
			zTextBoxColumnStyleInfo7.ColumnName = "CustomsEntryNumber";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JS_RS_NKServiceLevel";
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|f84fa81f-9fa0-48a2-9a92-070ba0634ff8", "Customs Status");
			zTextBoxColumnStyleInfo8.ColumnName = "JS_GatePassStatusShort";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|23679534-15dc-4f37-b320-039aa174b29a", "Load List ID", "Comma separated list of related load list job numbers.");
			zTextBoxColumnStyleInfo9.ColumnName = "JS_JK_ConsolID";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|20d571c8-de0e-416f-b4b4-38eafd41bfff", "House CCN");
			zTextBoxColumnStyleInfo10.ColumnName = "CanadaHouseCCN";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ShipmentReceivalModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ShipmentReceivalModuleButtonGrid.DetachMessage = Enterprise.Freight.CFS.GUI.Res.GetData("2D53D46B-6BE3-4587-AA50-73C15939A6CD", "Are you sure you want to detach the selected Shipment?");
			this.ShipmentReceivalModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentReceivalModuleButtonGrid.GridId = "dfd9eed0-f0de-4ec5-be33-c7a13f6ee1ec";
			// 
			// 
			// 
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 211, true);
			this.ShipmentReceivalModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ShipmentReceivalModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ShipmentReceivalModuleButtonGrid.Name = "ShipmentReceivalModuleButtonGrid";
			this.ShipmentReceivalModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.CFS.GUI.Res.GetData("BCB84DD0-2AE9-40F7-B5BC-2F5B8A105889", "Shipment");
			this.ShipmentReceivalModuleButtonGrid.ReadOnly = false;
			this.ShipmentReceivalModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 249, true);
			this.ShipmentReceivalModuleButtonGrid.TabIndex = 5;
			// 
			// LoadListDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentsGroupBox);
			this.Controls.Add(this.CFSLoadListConsolGroupBox);
			this.Name = "LoadListDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 577, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CFSLoadListConsolGroupBox.ResumeLayout(false);
			this.RightPanel.ResumeLayout(false);
			this.AboveRoutePanel.ResumeLayout(false);
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.NumbersPanel.ResumeLayout(false);
			this.ShipmentsGroupBox.ResumeLayout(false);
			this.ControlsToBind.ResumeLayout(false);
			this.ControlsToBind.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentReceivalModuleButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CFSLoadListConsolGroupBox;
		private Enterprise.ZArchitecture.ZTextBox CarrierBookingRefTextBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationControl JK_OH_ForwarderBoundOrganisationControl;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_TransportModeBoundDropEdit;
		private Enterprise.ZArchitecture.ZTextBox ClientRefTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ContainerModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ShipmentsGroupBox;
		protected internal LoadListShipmentsModuleButtonGrid ShipmentReceivalModuleButtonGrid;
		internal Enterprise.ZArchitecture.ZTextBox JK_MasterBillNumTextBox;
		private CargoWise.Windows.UI.KPanel LeftPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel RightPanel;
		private Enterprise.ZArchitecture.ZTextBox EntryNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_CartageCoAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_CTOAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_EmptyContainerYardControl;
		private Enterprise.ZArchitecture.GUI.ZPanel AboveRoutePanel;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ShippingLineAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKLoadPortBoundCodeFindBox;
		private Enterprise.Freight.GUI.ConsoLegDetailsControl LoadListConsolLegDetailsControl;
		private CargoWise.Windows.UI.KPanel ControlsToBind;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalShipmentQuantityCalEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalVolumeUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalShipmentVolumeCalc;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalShipmentVolumeLabel;
		private Enterprise.ZArchitecture.GUI.ZAddressControl DepotAddressControl;
		private Enterprise.ZArchitecture.GUI.ZPanel NumbersPanel;
		private Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersControl;
	}
}
