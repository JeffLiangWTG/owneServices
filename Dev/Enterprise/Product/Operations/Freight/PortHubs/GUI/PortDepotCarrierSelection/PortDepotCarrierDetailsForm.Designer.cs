using System.Windows.Forms;

namespace Enterprise.Freight.PortHubs.GUI
{
	partial class PortDepotCarrierSelectionDetailsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.OutputBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.CarrierAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.DestinationPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.OriginPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.DispatchDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.PackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ZonesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ZonesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.InputBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.MasterHouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.PackTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MaxWeightNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.MinWeightNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.VolumeUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MaxVolumeNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.MinVolumeNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.WeightUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransportationModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DGClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ProcessTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PackModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.selectionDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.basePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PostingButtons.SuspendLayout();
            this.OutputBox.SuspendLayout();
            this.CarrierGuidFindBox.SuspendLayout();
            this.CarrierAgentGuidFindBox.SuspendLayout();
            this.DestinationPortCodeFindBox.SuspendLayout();
            this.OriginPortCodeFindBox.SuspendLayout();
            this.DepotAddressControl.SuspendLayout();
            this.DispatchDepotAddressControl.SuspendLayout();
            this.PackTypeDropEdit.SuspendLayout();
            this.ZonesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ZonesGrid)).BeginInit();
            this.ZonesGrid.SuspendLayout();
            this.InputBox.SuspendLayout();
            this.ShipperAddressControl.SuspendLayout();
            this.PackTypeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxWeightNumericUpDown)).BeginInit();
            this.MaxWeightNumericUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinWeightNumericUpDown)).BeginInit();
            this.MinWeightNumericUpDown.SuspendLayout();
            this.VolumeUnitDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxVolumeNumericUpDown)).BeginInit();
            this.MaxVolumeNumericUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinVolumeNumericUpDown)).BeginInit();
            this.MinVolumeNumericUpDown.SuspendLayout();
            this.WeightUnitDropEdit.SuspendLayout();
            this.TransportationModeDropEdit.SuspendLayout();
            this.DGClassDropEdit.SuspendLayout();
            this.ServiceLevelDropEdit.SuspendLayout();
            this.DirectionDropEdit.SuspendLayout();
            this.ProcessTypeDropEdit.SuspendLayout();
            this.PackModeDropEdit.SuspendLayout();
            this.selectionDetailsPanel.SuspendLayout();
            this.basePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 675, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 24, true);
            this.MainStatusBar.TabIndex = 6;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.PortHubs.Business.PortHubSelection);
            // 
            // PostingButtons
            // 
            this.PostingButtons.AllowDrop = true;
            this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(751, 644, true);
            this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.PostingButtons.Name = "PostingButtons";
            this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.PostingButtons.TabIndex = 5;
            // 
            // OutputBox
            // 
            this.OutputBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("67ac49aa-a108-43e5-b2bf-0e313af94d9d", "Output Fields");
            this.OutputBox.Controls.Add(this.CarrierGuidFindBox);
            this.OutputBox.Controls.Add(this.CarrierAgentGuidFindBox);
            this.OutputBox.Controls.Add(this.DestinationPortCodeFindBox);
            this.OutputBox.Controls.Add(this.OriginPortCodeFindBox);
            this.OutputBox.Controls.Add(this.DepotAddressControl);
            this.OutputBox.Controls.Add(this.DispatchDepotAddressControl);
            this.OutputBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 0, true);
            this.OutputBox.Name = "OutputBox";
            this.OutputBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 310, true);
            this.OutputBox.TabIndex = 2;
            this.OutputBox.TabStop = false;
            // 
            // CarrierGuidFindBox
            // 
            this.CarrierGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "TY_OH_Carrier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_OH_Carrier)));
            this.CarrierGuidFindBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("a639e809-298e-4be0-8678-b640320b0096", "Carrier");
            this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 149, true);
            this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
            this.CarrierGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CarrierGuidFindBox.ParentType = null;
            this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.CarrierGuidFindBox.TabIndex = 20;
            // 
            // CarrierAgentGuidFindBox
            // 
            this.CarrierAgentGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CarrierAgentGuidFindBox, "TY_OH_CarrierBookingAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_OH_CarrierBookingAgent)));
            this.CarrierAgentGuidFindBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("c9aef113-e919-47d4-a0e4-05d1fba8cebd", "Carrier Booking Agent");
            this.CarrierAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 123, true);
            this.CarrierAgentGuidFindBox.Name = "CarrierAgentGuidFindBox";
            this.CarrierAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CarrierAgentGuidFindBox.ParentType = null;
            this.CarrierAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.CarrierAgentGuidFindBox.TabIndex = 19;
            // 
            // DestinationPortCodeFindBox
            // 
            this.DestinationPortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DestinationPortCodeFindBox, "DepotPortCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).DepotPortCode)));
            this.DestinationPortCodeFindBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("4684be37-66f8-4e3e-944f-9e74742faa2c", "Destination Port");
            this.DestinationPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 97, true);
            this.DestinationPortCodeFindBox.Name = "DestinationPortCodeFindBox";
            this.DestinationPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DestinationPortCodeFindBox.ParentType = null;
            this.DestinationPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DestinationPortCodeFindBox.TabIndex = 18;
            // 
            // OriginPortCodeFindBox
            // 
            this.OriginPortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OriginPortCodeFindBox, "DispatchDepotPortCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).DispatchDepotPortCode)));
            this.OriginPortCodeFindBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("c83779a8-1ad4-4b3f-a1ae-a60065ade2a7", "Origin Port");
            this.OriginPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 45, true);
            this.OriginPortCodeFindBox.Name = "OriginPortCodeFindBox";
            this.OriginPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OriginPortCodeFindBox.ParentType = null;
            this.OriginPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.OriginPortCodeFindBox.TabIndex = 16;
            // 
            // DepotAddressControl
            // 
            this.DepotAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepotAddressControl, "TY_OA_DepotAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_OA_DepotAddress)));
            this.DepotAddressControl.BindToOrgList = "Lookups.Depots";
            this.DepotAddressControl.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("cf41f4b7-f37c-4404-aa12-275442fba1cb", "Destination Depot");
            this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 71, true);
            this.DepotAddressControl.Name = "DepotAddressControl";
            this.DepotAddressControl.PopupCaption = "";
            this.DepotAddressControl.ShowAddress = false;
            this.DepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DepotAddressControl.TabIndex = 17;
            // 
            // DispatchDepotAddressControl
            // 
            this.DispatchDepotAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DispatchDepotAddressControl, "TY_OA_DispatchDepotAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_OA_DispatchDepotAddress)));
            this.DispatchDepotAddressControl.BindToOrgList = "Lookups.Depots";
            this.DispatchDepotAddressControl.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("0fa43104-f7e7-4a27-8674-72e4d92ed846", "Origin Depot");
            this.DispatchDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 19, true);
            this.DispatchDepotAddressControl.Name = "DispatchDepotAddressControl";
            this.DispatchDepotAddressControl.PopupCaption = "";
            this.DispatchDepotAddressControl.ShowAddress = false;
            this.DispatchDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DispatchDepotAddressControl.TabIndex = 15;
            // 
            // PackTypeDropEdit
            // 
            this.PackTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PackTypeDropEdit, "TY_F3_NKPackType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_F3_NKPackType)));
            this.PackTypeDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("440f8637-1045-4319-a0e4-36f0ed644e90", "Pack Type");
            this.PackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 19, true);
            this.PackTypeDropEdit.Name = "PackTypeDropEdit";
            this.PackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
            this.PackTypeDropEdit.TabIndex = 8;
            // 
            // ZonesGroupBox
            // 
            this.ZonesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ZonesGroupBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("884ad9b4-6299-475e-8c30-4acb3278446b", "Zones");
            this.ZonesGroupBox.Controls.Add(this.ZonesGrid);
            this.ZonesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 317, true);
            this.ZonesGroupBox.Name = "ZonesGroupBox";
            this.ZonesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 321, true);
            this.ZonesGroupBox.TabIndex = 3;
            this.ZonesGroupBox.TabStop = false;
            // 
            // ZonesGrid
            // 
            this.ZonesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ZonesGrid, "PortHubZonePivots");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)).SyncRoot)).TX_TZ_Zone)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)).SyncRoot)).CarrierPK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)).SyncRoot)).TX_PL_NKCarrierServiceLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)).SyncRoot)).TX_CarrierAccountNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).PortHubZonePivots)).SyncRoot)).TX_PickupCutOffTimeVariance)));
            this.ZonesGrid.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo1.ColumnName = "TX_TZ_Zone";
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zGuidFindBoxColumnStyleInfo2.ColumnName = "CarrierPK";
            zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.ColumnName = "TX_PL_NKCarrierServiceLevel";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("b51cafa7-761e-44df-9b4f-829e643c7622", "Carrier Account Number");
            zDropEditColumnStyleInfo2.ColumnName = "TX_CarrierAccountNumber";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "TX_PickupCutOffTimeVariance";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            this.ZonesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.ZonesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
            this.ZonesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.ZonesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.ZonesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ZonesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZonesGrid.GridId = "37f9454c-1ff6-4ebe-820b-ddbe888ceafb";
            this.ZonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ZonesGrid.LayoutKey = "ZonesGrid";
            this.ZonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ZonesGrid.Name = "ZonesGrid";
            this.ZonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 302, true);
            this.ZonesGrid.TabIndex = 21;
            // 
            // InputBox
            // 
            this.InputBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("67bd7c57-1a59-4c7b-9823-c1a18ab25e9e", "Input Fields");
            this.InputBox.Controls.Add(this.ShipperAddressControl);
            this.InputBox.Controls.Add(this.MasterHouseCheckBox);
            this.InputBox.Controls.Add(this.PackTypeGroupBox);
            this.InputBox.Controls.Add(this.TransportationModeDropEdit);
            this.InputBox.Controls.Add(this.DGClassDropEdit);
            this.InputBox.Controls.Add(this.ServiceLevelDropEdit);
            this.InputBox.Controls.Add(this.DirectionDropEdit);
            this.InputBox.Controls.Add(this.ProcessTypeDropEdit);
            this.InputBox.Controls.Add(this.PackModeDropEdit);
            this.InputBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.InputBox.Name = "InputBox";
            this.InputBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 311, true);
            this.InputBox.TabIndex = 4;
            this.InputBox.TabStop = false;
            // 
            // ShipperAddressControl
            // 
            this.ShipperAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipperAddressControl, "TY_OA_ShipperAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_OA_ShipperAddress)));
            this.ShipperAddressControl.BindToOrgList = "Lookups.ShipperOrgs";
            this.ShipperAddressControl.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("08f08c9b-e827-4c09-b671-48d0c74dd18d", "Shipper");
            this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 123, true);
            this.ShipperAddressControl.Name = "ShipperAddressControl";
            this.ShipperAddressControl.PopupCaption = "";
            this.ShipperAddressControl.ShowAddress = false;
            this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ShipperAddressControl.TabIndex = 5;
            // 
            // MasterHouseCheckBox
            // 
            this.MasterHouseCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.MasterHouseCheckBox, "TY_IsMasterHouse");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_IsMasterHouse)));
            this.MasterHouseCheckBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("7c49ae45-e072-415f-9f55-89d5e3b23454", "Master House");
            this.MasterHouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 19, true);
            this.MasterHouseCheckBox.Name = "MasterHouseCheckBox";
            this.MasterHouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
            this.MasterHouseCheckBox.TabIndex = 1;
            this.MasterHouseCheckBox.Text = "Master House";
            this.MasterHouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // PackTypeGroupBox
            // 
            this.PackTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PackTypeGroupBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("532a552b-96f8-435c-a056-17e120c0ff0a", "Package");
            this.PackTypeGroupBox.Controls.Add(this.MaxWeightNumericUpDown);
            this.PackTypeGroupBox.Controls.Add(this.MinWeightNumericUpDown);
            this.PackTypeGroupBox.Controls.Add(this.VolumeUnitDropEdit);
            this.PackTypeGroupBox.Controls.Add(this.MaxVolumeNumericUpDown);
            this.PackTypeGroupBox.Controls.Add(this.MinVolumeNumericUpDown);
            this.PackTypeGroupBox.Controls.Add(this.WeightUnitDropEdit);
            this.PackTypeGroupBox.Controls.Add(this.PackTypeDropEdit);
            this.PackTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
            this.PackTypeGroupBox.Name = "PackTypeGroupBox";
            this.PackTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 110, true);
            this.PackTypeGroupBox.TabIndex = 9;
            this.PackTypeGroupBox.TabStop = false;
            // 
            // MaxWeightNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MaxWeightNumericUpDown, "TY_MaxWeight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_MaxWeight)));
            this.MaxWeightNumericUpDown.BindTo = "TY_MaxWeight";
            this.MaxWeightNumericUpDown.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("7f2db216-81f0-4f6d-b6a1-cb0587d3a4e4", "Max Weight");
            this.MaxWeightNumericUpDown.DecimalPlaces = 3;
            this.MaxWeightNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 46, true);
            this.MaxWeightNumericUpDown.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.MaxWeightNumericUpDown.Name = "MaxWeightNumericUpDown";
            this.MaxWeightNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
            this.MaxWeightNumericUpDown.TabIndex = 10;
            // 
            // MinWeightNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MinWeightNumericUpDown, "TY_MinWeight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_MinWeight)));
            this.MinWeightNumericUpDown.BindTo = "TY_MinWeight";
            this.MinWeightNumericUpDown.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("9378305e-9a83-40da-83c3-17d45fe0ef82", "Min Weight");
            this.MinWeightNumericUpDown.DecimalPlaces = 3;
            this.MinWeightNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 46, true);
            this.MinWeightNumericUpDown.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.MinWeightNumericUpDown.Name = "MinWeightNumericUpDown";
            this.MinWeightNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
            this.MinWeightNumericUpDown.TabIndex = 9;
            // 
            // VolumeUnitDropEdit
            // 
            this.VolumeUnitDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VolumeUnitDropEdit, "TY_VolumeUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_VolumeUQ)));
            this.VolumeUnitDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("ee95a047-a29e-44ef-ad23-6a8a8f544efd", "Volume Unit");
            this.VolumeUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 71, true);
            this.VolumeUnitDropEdit.Name = "VolumeUnitDropEdit";
            this.VolumeUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
            this.VolumeUnitDropEdit.TabIndex = 14;
            // 
            // MaxVolumeNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MaxVolumeNumericUpDown, "TY_MaxVolume");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_MaxVolume)));
            this.MaxVolumeNumericUpDown.BindTo = "TY_MaxVolume";
            this.MaxVolumeNumericUpDown.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("0c3cbfc6-cabd-4c3f-bdf8-6795510ff86a", "Max Volume");
            this.MaxVolumeNumericUpDown.DecimalPlaces = 3;
            this.MaxVolumeNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 71, true);
            this.MaxVolumeNumericUpDown.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.MaxVolumeNumericUpDown.Name = "MaxVolumeNumericUpDown";
            this.MaxVolumeNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
            this.MaxVolumeNumericUpDown.TabIndex = 13;
            // 
            // MinVolumeNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MinVolumeNumericUpDown, "TY_MinVolume");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_MinVolume)));
            this.MinVolumeNumericUpDown.BindTo = "TY_MinVolume";
            this.MinVolumeNumericUpDown.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("70e90511-4861-4390-9d7a-4769e916d986", "Min Volume");
            this.MinVolumeNumericUpDown.DecimalPlaces = 3;
            this.MinVolumeNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 71, true);
            this.MinVolumeNumericUpDown.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.MinVolumeNumericUpDown.Name = "MinVolumeNumericUpDown";
            this.MinVolumeNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
            this.MinVolumeNumericUpDown.TabIndex = 12;
            // 
            // WeightUnitDropEdit
            // 
            this.WeightUnitDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeightUnitDropEdit, "TY_WeightUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_WeightUQ)));
            this.WeightUnitDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("6ffb0a7f-c5f6-421f-95ff-0806e4ac3fb2", "Weight Unit");
            this.WeightUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 45, true);
            this.WeightUnitDropEdit.Name = "WeightUnitDropEdit";
            this.WeightUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
            this.WeightUnitDropEdit.TabIndex = 11;
            // 
            // TransportationModeDropEdit
            // 
            this.TransportationModeDropEdit.AccessibleName = "";
            this.TransportationModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportationModeDropEdit, "TY_RatingFreightMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_RatingFreightMode)));
            this.TransportationModeDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("c0f411a9-1be6-41d3-9132-c69c1e504bec", "Transport Mode");
            this.TransportationModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
            this.TransportationModeDropEdit.Name = "TransportationModeDropEdit";
            this.TransportationModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.TransportationModeDropEdit.TabIndex = 2;
            // 
            // DGClassDropEdit
            // 
            this.DGClassDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DGClassDropEdit, "TY_UndgClass");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_UndgClass)));
            this.DGClassDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("97b880de-e1cd-464f-9670-5a65a00b6721", "DG Class");
            this.DGClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 175, true);
            this.DGClassDropEdit.Name = "DGClassDropEdit";
            this.DGClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DGClassDropEdit.TabIndex = 7;
            // 
            // ServiceLevelDropEdit
            // 
            this.ServiceLevelDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "TY_RS_NKServiceLevel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_RS_NKServiceLevel)));
            this.ServiceLevelDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("6b90bcdc-339d-4f0b-9a17-9c9f8d89c08a", "Service Level");
            this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 149, true);
            this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
            this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ServiceLevelDropEdit.TabIndex = 6;
            // 
            // DirectionDropEdit
            // 
            this.DirectionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DirectionDropEdit, "TY_Direction");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_Direction)));
            this.DirectionDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("8d20a128-8ee4-45ae-ae56-0fb4ea1355f6", "Direction");
            this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 71, true);
            this.DirectionDropEdit.Name = "DirectionDropEdit";
            this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DirectionDropEdit.TabIndex = 3;
            // 
            // ProcessTypeDropEdit
            // 
            this.ProcessTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProcessTypeDropEdit, "TY_ProcessType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_ProcessType)));
            this.ProcessTypeDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("7d31c447-69cd-46b1-9314-b6ca62b58c90", "Process Type");
            this.ProcessTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
            this.ProcessTypeDropEdit.Name = "ProcessTypeDropEdit";
            this.ProcessTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ProcessTypeDropEdit.TabIndex = 0;
            // 
            // PackModeDropEdit
            // 
            this.PackModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PackModeDropEdit, "TY_PackMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(null)).TY_PackMode)));
            this.PackModeDropEdit.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("7fbc9959-a7b8-449f-a389-934caeb726eb", "Pack Mode");
            this.PackModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 97, true);
            this.PackModeDropEdit.Name = "PackModeDropEdit";
            this.PackModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.PackModeDropEdit.TabIndex = 4;
            // 
            // selectionDetailsPanel
            // 
            this.selectionDetailsPanel.Controls.Add(this.ZonesGroupBox);
            this.selectionDetailsPanel.Controls.Add(this.PostingButtons);
            this.selectionDetailsPanel.Controls.Add(this.InputBox);
            this.selectionDetailsPanel.Controls.Add(this.OutputBox);
            this.selectionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.selectionDetailsPanel.Name = "selectionDetailsPanel";
            this.selectionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 675, true);
            this.selectionDetailsPanel.TabIndex = 5;
            // 
            // basePanel
            // 
            this.basePanel.Controls.Add(this.selectionDetailsPanel);
            this.basePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.basePanel.Name = "basePanel";
            this.basePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 675, true);
            this.basePanel.TabIndex = 9;
            // 
            // PortHubSelectionDetailsForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 699, true);
            this.Controls.Add(this.basePanel);
            this.DataSourceType = typeof(Enterprise.Freight.PortHubs.Business.PortHubSelection);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 725, true);
            this.Name = "PortHubSelectionDetailsForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.basePanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PostingButtons.ResumeLayout(true);
            this.PostingButtons.PerformLayout();
            this.OutputBox.ResumeLayout(false);
            this.OutputBox.PerformLayout();
            this.CarrierGuidFindBox.ResumeLayout(true);
            this.CarrierGuidFindBox.PerformLayout();
            this.CarrierAgentGuidFindBox.ResumeLayout(true);
            this.CarrierAgentGuidFindBox.PerformLayout();
            this.DestinationPortCodeFindBox.ResumeLayout(true);
            this.DestinationPortCodeFindBox.PerformLayout();
            this.OriginPortCodeFindBox.ResumeLayout(true);
            this.OriginPortCodeFindBox.PerformLayout();
            this.DepotAddressControl.ResumeLayout(true);
            this.DepotAddressControl.PerformLayout();
            this.DispatchDepotAddressControl.ResumeLayout(true);
            this.DispatchDepotAddressControl.PerformLayout();
            this.PackTypeDropEdit.ResumeLayout(true);
            this.PackTypeDropEdit.PerformLayout();
            this.ZonesGroupBox.ResumeLayout(false);
            this.ZonesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ZonesGrid)).EndInit();
            this.ZonesGrid.ResumeLayout(false);
            this.ZonesGrid.PerformLayout();
            this.InputBox.ResumeLayout(false);
            this.InputBox.PerformLayout();
            this.ShipperAddressControl.ResumeLayout(true);
            this.ShipperAddressControl.PerformLayout();
            this.PackTypeGroupBox.ResumeLayout(false);
            this.PackTypeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxWeightNumericUpDown)).EndInit();
            this.MaxWeightNumericUpDown.ResumeLayout(false);
            this.MaxWeightNumericUpDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinWeightNumericUpDown)).EndInit();
            this.MinWeightNumericUpDown.ResumeLayout(false);
            this.MinWeightNumericUpDown.PerformLayout();
            this.VolumeUnitDropEdit.ResumeLayout(true);
            this.VolumeUnitDropEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxVolumeNumericUpDown)).EndInit();
            this.MaxVolumeNumericUpDown.ResumeLayout(false);
            this.MaxVolumeNumericUpDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinVolumeNumericUpDown)).EndInit();
            this.MinVolumeNumericUpDown.ResumeLayout(false);
            this.MinVolumeNumericUpDown.PerformLayout();
            this.WeightUnitDropEdit.ResumeLayout(true);
            this.WeightUnitDropEdit.PerformLayout();
            this.TransportationModeDropEdit.ResumeLayout(true);
            this.TransportationModeDropEdit.PerformLayout();
            this.DGClassDropEdit.ResumeLayout(true);
            this.DGClassDropEdit.PerformLayout();
            this.ServiceLevelDropEdit.ResumeLayout(true);
            this.ServiceLevelDropEdit.PerformLayout();
            this.DirectionDropEdit.ResumeLayout(true);
            this.DirectionDropEdit.PerformLayout();
            this.ProcessTypeDropEdit.ResumeLayout(true);
            this.ProcessTypeDropEdit.PerformLayout();
            this.PackModeDropEdit.ResumeLayout(true);
            this.PackModeDropEdit.PerformLayout();
            this.selectionDetailsPanel.ResumeLayout(false);
            this.selectionDetailsPanel.PerformLayout();
            this.basePanel.ResumeLayout(false);
            this.basePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private ZArchitecture.GUI.ZGroupBox OutputBox;
		private ZArchitecture.GUI.ZDropEdit PackTypeDropEdit;
		private ZArchitecture.GUI.ZAddressControl DispatchDepotAddressControl;
		private ZArchitecture.GUI.ZGroupBox ZonesGroupBox;
		private ZArchitecture.ZGrid ZonesGrid;
		private ZArchitecture.GUI.ZGroupBox InputBox;
		private ZArchitecture.GUI.ZDropEdit PackModeDropEdit;
		private ZArchitecture.GUI.ZPanel selectionDetailsPanel;
		private ZArchitecture.GUI.ZPanel basePanel;
		private ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private ZArchitecture.GUI.ZGroupBox PackTypeGroupBox;
		private ZArchitecture.GUI.ZNumericUpDown MinVolumeNumericUpDown;
		private ZArchitecture.GUI.ZDropEdit WeightUnitDropEdit;
		private ZArchitecture.GUI.ZDropEdit TransportationModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit DGClassDropEdit;
		private ZArchitecture.GUI.ZDropEdit ServiceLevelDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProcessTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit VolumeUnitDropEdit;
		private ZArchitecture.GUI.ZNumericUpDown MaxVolumeNumericUpDown;
		private ZArchitecture.GUI.ZNumericUpDown MaxWeightNumericUpDown;
		private ZArchitecture.GUI.ZNumericUpDown MinWeightNumericUpDown;
		private ZArchitecture.GUI.ZAddressControl DepotAddressControl;
		private ZArchitecture.GUI.ZCheckBox MasterHouseCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox DestinationPortCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox OriginPortCodeFindBox;
		private ZArchitecture.GUI.ZAddressControl ShipperAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CarrierAgentGuidFindBox;
	}
}
