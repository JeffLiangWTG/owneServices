using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ConsolDashboardForm
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
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CurrentConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentConsolShipmentModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ShipmentModuleButtonGrid(((ConsolDashboard)BusinessEntity).Shipments, false);
			this.CurrentConsolDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AchievedQuantitiesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AchievedQuantitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QuantitiesOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcessWeightVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExcessWeightVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConsolChargeableTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsolChargeableUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CorrectedVolumeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CorrectedVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CorrectedWeightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CorrectedWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VolumeWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VolumeWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CostFreeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CostFreeUnitsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightUtilisationPercentageBar = new Enterprise.Freight.Forwarding.GUI.LabelledPercentageBar();
			this.Commodity = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WeightUtilisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeUtilisationPercentageBar = new Enterprise.Freight.Forwarding.GUI.LabelledPercentageBar();
			this.VolumeUtilisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsolMaxDimsControl = new Enterprise.Freight.Forwarding.GUI.ConsolMaxDimsControl();
			this.DangerousGoodsControl = new Enterprise.Freight.Forwarding.GUI.DangerousGoodsControl();
			this.TemperatureControlBlock = new Enterprise.Freight.Forwarding.GUI.TemperatureControlBlock();
			this.ContractsAndAllocationsControl = new Enterprise.Freight.Forwarding.GUI.DashboardContractsAndAllocationControl();
			this.CostFreePercentageBar = new Enterprise.Freight.Forwarding.GUI.LabelledPercentageBar();
			this.CostFreeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DensityVisualisationControl = new Enterprise.Freight.Forwarding.GUI.DensityVisualisationControl();
			this.DensityFactorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsolidatedFreightCostTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsolidatedFreightCostDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentFreightCostTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentFreightCostDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreAllocatedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreAllocatedWeightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreAllocatedVolumeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreAllocatedWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreAllocatedVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargeableTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ConsolIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsolModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActionButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DetachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateNewConsolButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainHorizontalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.DashboardShipmentModuleButtonGrid(((ConsolDashboard)BusinessEntity).Shipments);
			this.TopPanelVerticalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ConsolsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.StandaloneConsolModuleButtonGrid();
			this.ShowSubsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.CurrentConsolGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrentConsolShipmentModuleButtonGrid.InnerGrid)).BeginInit();
			this.CurrentConsolShipmentModuleButtonGrid.SuspendLayout();
			this.CurrentConsolDetailsPanel.SuspendLayout();
			this.AchievedQuantitiesPanel.SuspendLayout();
			this.AchievedQuantitiesGroupBox.SuspendLayout();
			this.WeightUtilisationPercentageBar.SuspendLayout();
			this.VolumeUtilisationPercentageBar.SuspendLayout();
			this.CostFreePercentageBar.SuspendLayout();
			this.DensityVisualisationControl.SuspendLayout();
			this.PreAllocatedGroupBox.SuspendLayout();
			this.CutOffDateEdit.SuspendLayout();
			this.LoadPortCodeFindBox.SuspendLayout();
			this.DischargePortCodeFindBox.SuspendLayout();
			this.AgentTypeDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.ConsolModeDropEdit.SuspendLayout();
			this.ActionButtonsPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.ShipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentModuleButtonGrid.InnerGrid)).BeginInit();
			this.ShipmentModuleButtonGrid.SuspendLayout();
			this.ConsolsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).BeginInit();
			this.ConsolModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 803, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ConsolDashboard);
			//
			// ConsolDashboardForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboardForm|0e103f40-4db2-4872-a1f9-b158d9909495", "Consolidation Planning Board");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 827, true);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MainHorizontalSplitter);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ConsolDashboard);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1536, 864, true);
			this.Name = "ConsolDashboardForm";
			//
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CurrentConsolGroupBox);
			this.BottomPanel.Controls.Add(this.ActionButtonsPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 267, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 536, true);
			this.BottomPanel.TabIndex = 0;
			// 
			// CurrentConsolGroupBox
			// 
			this.CurrentConsolGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("47b60aac-170a-413e-828c-d9dcc63e6048", "Current Consol");
			this.CurrentConsolGroupBox.Controls.Add(this.CurrentConsolShipmentModuleButtonGrid);
			this.CurrentConsolGroupBox.Controls.Add(this.CurrentConsolDetailsPanel);
			this.CurrentConsolGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrentConsolGroupBox.Name = "CurrentConsolGroupBox";
			this.CurrentConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 509, true);
			this.CurrentConsolGroupBox.TabIndex = 1;
			this.CurrentConsolGroupBox.TabStop = false;
			// 
			// CurrentConsolShipmentModuleButtonGrid
			// 
			this.CurrentConsolShipmentModuleButtonGrid.AllowDrop = true;
			this.CurrentConsolShipmentModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.CurrentConsolShipmentModuleButtonGrid, "Consols.GridShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).Shipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Shipments_List)));
			this.CurrentConsolShipmentModuleButtonGrid.BindToFindBoxList = "Shipments_List";
			this.CurrentConsolShipmentModuleButtonGrid.DetachMessage = null;
			this.CurrentConsolShipmentModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentConsolShipmentModuleButtonGrid.GridId = "0806a7d0-b7a2-422b-9971-9abdf26be767";
			// 
			// 
			// 
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.GridId = null;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1513, 337, true);
			this.CurrentConsolShipmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.CurrentConsolShipmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 135, true);
			this.CurrentConsolShipmentModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobShipment;
			this.CurrentConsolShipmentModuleButtonGrid.Name = "CurrentConsolShipmentModuleButtonGrid";
			this.CurrentConsolShipmentModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("d6d32cdf-c12c-4189-85bd-f5cbb317620e", "Shipment");
			this.CurrentConsolShipmentModuleButtonGrid.ReadOnly = true;
			this.CurrentConsolShipmentModuleButtonGrid.ShowAttachButton = false;
			this.CurrentConsolShipmentModuleButtonGrid.ShowDetachButton = false;
			this.CurrentConsolShipmentModuleButtonGrid.ShowEditButton = false;
			this.CurrentConsolShipmentModuleButtonGrid.ShowNewButton = false;
			this.CurrentConsolShipmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1517, 373, true);
			this.CurrentConsolShipmentModuleButtonGrid.TabIndex = 3;
			// 
			// CurrentConsolDetailsPanel
			// 
			this.CurrentConsolDetailsPanel.Controls.Add(this.AchievedQuantitiesGroupBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.PreAllocatedGroupBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.ConsolIDTextBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.LoadPortCodeFindBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.DischargePortCodeFindBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.MasterBillTextBox);
			this.CurrentConsolDetailsPanel.Controls.Add(this.AgentTypeDropEdit);
			this.CurrentConsolDetailsPanel.Controls.Add(this.TransportModeDropEdit);
			this.CurrentConsolDetailsPanel.Controls.Add(this.ConsolModeDropEdit);
			this.CurrentConsolDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CurrentConsolDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrentConsolDetailsPanel.Name = "CurrentConsolDetailsPanel";
			this.CurrentConsolDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1517, 250, true);
			this.CurrentConsolDetailsPanel.TabIndex = 0;
			//
			// AchievedQuantitiesPanel
			//
			this.AchievedQuantitiesPanel.AutoScroll = true;
			this.AchievedQuantitiesPanel.Controls.Add(this.QuantitiesOverrideCheckBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ExcessWeightVolumeUnitTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ExcessWeightVolumeCalcEdit);
			this.AchievedQuantitiesPanel.Controls.Add(this.ConsolChargeableTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ConsolChargeableUnitTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CorrectedVolumeTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CorrectedVolumeUnitTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CorrectedWeightTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CorrectedWeightUnitTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.VolumeWeightCalcEdit);
			this.AchievedQuantitiesPanel.Controls.Add(this.VolumeWeightUnitTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CostFreeTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.CostFreeUnitsTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.WeightUtilisationPercentageBar);
			this.AchievedQuantitiesPanel.Controls.Add(this.WeightUtilisationLabel);
			this.AchievedQuantitiesPanel.Controls.Add(this.VolumeUtilisationPercentageBar);
			this.AchievedQuantitiesPanel.Controls.Add(this.VolumeUtilisationLabel);
			this.AchievedQuantitiesPanel.Controls.Add(this.CostFreePercentageBar);
			this.AchievedQuantitiesPanel.Controls.Add(this.CostFreeLabel);
			this.AchievedQuantitiesPanel.Controls.Add(this.DensityVisualisationControl);
			this.AchievedQuantitiesPanel.Controls.Add(this.DensityFactorLabel);
			this.AchievedQuantitiesPanel.Controls.Add(this.ConsolidatedFreightCostTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ConsolidatedFreightCostDescTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ShipmentFreightCostTextBox);
			this.AchievedQuantitiesPanel.Controls.Add(this.ShipmentFreightCostDescTextBox);
			this.AchievedQuantitiesPanel.Name = "AchievedQuantitiesPanel";
			this.AchievedQuantitiesPanel.TabIndex = 34;
			this.AchievedQuantitiesPanel.TabStop = false;
			this.AchievedQuantitiesPanel.Dock = DockStyle.Fill;
			//
			// AchievedQuantitiesGroupBox
			// 
			this.AchievedQuantitiesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("abd3d6c8-7e0c-46ad-a2e3-5ed76644964f", "Achieved Quantities");
			this.AchievedQuantitiesGroupBox.Controls.Add(this.AchievedQuantitiesPanel);
			this.AchievedQuantitiesGroupBox.Name = "AchievedQuantitiesGroupBox";
			this.AchievedQuantitiesGroupBox.TabIndex = 35;
			this.AchievedQuantitiesGroupBox.TabStop = false;
			this.AchievedQuantitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 0, true);
			var newWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.Width) - 497;
			this.AchievedQuantitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(newWidth, 119, true);
			//
			// QuantitiesOverrideCheckBox
			// 
			this.QuantitiesOverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QuantitiesOverrideCheckBox, "Consols.CalculationWrapper.JK_OverrideConsolChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_OverrideConsolChargeable)));
			this.QuantitiesOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QuantitiesOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 65, true);
			this.QuantitiesOverrideCheckBox.Name = "QuantitiesOverrideCheckBox";
			this.QuantitiesOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 16, true);
			this.QuantitiesOverrideCheckBox.TabIndex = 36;
			this.QuantitiesOverrideCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExcessWeightVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExcessWeightVolumeUnitTextBox, "Consols.Density.ExcessVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).Density.ExcessVolumeWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExcessWeightVolumeUnitTextBox, false);
			this.ExcessWeightVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 22, true);
			this.ExcessWeightVolumeUnitTextBox.Name = "ExcessWeightVolumeUnitTextBox";
			this.ExcessWeightVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.ExcessWeightVolumeUnitTextBox.TabIndex = 23;
			// 
			// ExcessWeightVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExcessWeightVolumeCalcEdit, "Consols.Density.ExcessVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).Density.ExcessVolumeWeight)));
			this.ExcessWeightVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboardForm|746e7362-3d81-4f3b-bebe-aed4d5c9d3c2", "Excess Wgt. Vol.", "Excess Weight Volume", "Excess Weight or Volume is calculated as a difference between Consolidated Weight (or Volume) and its Volume Weight (or Weight Volume), whichever is greater.");
			this.ExcessWeightVolumeCalcEdit.DecimalPlaces = 2;
			this.ExcessWeightVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 22, true);
			this.ExcessWeightVolumeCalcEdit.Name = "ExcessWeightVolumeCalcEdit";
			this.ExcessWeightVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.ExcessWeightVolumeCalcEdit.TabIndex = 22;
			this.ExcessWeightVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsolChargeableTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolChargeableTextBox, "Consols.CalculationWrapper.JK_ConsolChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_ConsolChargeable)));
			this.ConsolChargeableTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 44, true);
			this.ConsolChargeableTextBox.Name = "ConsolChargeableTextBox";
			this.ConsolChargeableTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.ConsolChargeableTextBox.TabIndex = 18;
			this.ConsolChargeableTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsolChargeableUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolChargeableUnitTextBox, "Consols.CalculationWrapper.JK_ConsolChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_ConsolChargeableUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsolChargeableUnitTextBox, false);
			this.ConsolChargeableUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 44, true);
			this.ConsolChargeableUnitTextBox.Name = "ConsolChargeableUnitTextBox";
			this.ConsolChargeableUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.ConsolChargeableUnitTextBox.TabIndex = 19;
			// 
			// CorrectedVolumeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CorrectedVolumeTextBox, "Consols.CalculationWrapper.JK_CorrectedConsolVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_CorrectedConsolVolume)));
			this.CorrectedVolumeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 22, true);
			this.CorrectedVolumeTextBox.Name = "CorrectedVolumeTextBox";
			this.CorrectedVolumeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.CorrectedVolumeTextBox.TabIndex = 16;
			this.CorrectedVolumeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CorrectedVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CorrectedVolumeUnitTextBox, "Consols.CalculationWrapper.JK_CorrectedConsolVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_CorrectedConsolVolumeUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CorrectedVolumeUnitTextBox, false);
			this.CorrectedVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 22, true);
			this.CorrectedVolumeUnitTextBox.Name = "CorrectedVolumeUnitTextBox";
			this.CorrectedVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.CorrectedVolumeUnitTextBox.TabIndex = 17;
			// 
			// CorrectedWeightTextBox
			// 
			this.BindingSource.SetBindingMember(this.CorrectedWeightTextBox, "Consols.CalculationWrapper.JK_CorrectedConsolWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_CorrectedConsolWeight)));
			this.CorrectedWeightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 0, true);
			this.CorrectedWeightTextBox.Name = "CorrectedWeightTextBox";
			this.CorrectedWeightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.CorrectedWeightTextBox.TabIndex = 14;
			this.CorrectedWeightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CorrectedWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CorrectedWeightUnitTextBox, "Consols.CalculationWrapper.JK_CorrectedConsolWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_CorrectedConsolWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CorrectedWeightUnitTextBox, false);
			this.CorrectedWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 0, true);
			this.CorrectedWeightUnitTextBox.Name = "CorrectedWeightUnitTextBox";
			this.CorrectedWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.CorrectedWeightUnitTextBox.TabIndex = 15;
			// 
			// VolumeWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeWeightCalcEdit, "Consols.CalculationWrapper.JK_Calc_ActualVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ActualVolumeWeight)));
			this.VolumeWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|6cc6ac6b-8cff-4ce9-acc9-0c68313de7d6", "Volume Weight");
			this.VolumeWeightCalcEdit.DecimalPlaces = 2;
			this.VolumeWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 0, true);
			this.VolumeWeightCalcEdit.Name = "VolumeWeightCalcEdit";
			this.VolumeWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.VolumeWeightCalcEdit.TabIndex = 20;
			this.VolumeWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VolumeWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.VolumeWeightUnitTextBox, "Consols.CalculationWrapper.JK_Calc_ActualVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ActualVolumeWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VolumeWeightUnitTextBox, false);
			this.VolumeWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 0, true);
			this.VolumeWeightUnitTextBox.Name = "VolumeWeightUnitTextBox";
			this.VolumeWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.VolumeWeightUnitTextBox.TabIndex = 21;
			// 
			// CostFreeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CostFreeTextBox, "Consols.CalculationWrapper.JK_Calc_FreeSpace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_FreeSpace)));
			this.CostFreeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 22, true);
			this.CostFreeTextBox.Name = "CostFreeTextBox";
			this.CostFreeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.CostFreeTextBox.TabIndex = 32;
			this.CostFreeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostFreeUnitsTextBox
			// 
			this.BindingSource.SetBindingMember(this.CostFreeUnitsTextBox, "Consols.CalculationWrapper.JK_ConsolChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_ConsolChargeableUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CostFreeUnitsTextBox, false);
			this.CostFreeUnitsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(965, 22, true);
			this.CostFreeUnitsTextBox.Name = "CostFreeUnitsTextBox";
			this.CostFreeUnitsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 17, true);
			this.CostFreeUnitsTextBox.TabIndex = 33;
			// 
			// WeightUtilisationPercentageBar
			// 
			this.WeightUtilisationPercentageBar.AllowDrop = true;
			this.WeightUtilisationPercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 45, true);
			this.WeightUtilisationPercentageBar.Name = "WeightUtilisationPercentageBar";
			this.WeightUtilisationPercentageBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.WeightUtilisationPercentageBar.TabIndex = 25;
			// 
			// WeightUtilisationLabel
			// 
			this.WeightUtilisationLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("12b31cb7-86cf-40ad-be95-78c84d6cb264", "Weight Utilization");
			this.WeightUtilisationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeightUtilisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 45, true);
			this.WeightUtilisationLabel.Name = "WeightUtilisationLabel";
			this.WeightUtilisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.WeightUtilisationLabel.TabIndex = 24;
			// 
			// VolumeUtilisationPercentageBar
			// 
			this.VolumeUtilisationPercentageBar.AllowDrop = true;
			this.VolumeUtilisationPercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 65, true);
			this.VolumeUtilisationPercentageBar.Name = "VolumeUtilisationPercentageBar";
			this.VolumeUtilisationPercentageBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.VolumeUtilisationPercentageBar.TabIndex = 27;
			// 
			// VolumeUtilisationLabel
			// 
			this.VolumeUtilisationLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("dbdee386-f043-4186-a81d-0ffd90616b19", "Volume Utilization");
			this.VolumeUtilisationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VolumeUtilisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 65, true);
			this.VolumeUtilisationLabel.Name = "VolumeUtilisationLabel";
			this.VolumeUtilisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 16, true);
			this.VolumeUtilisationLabel.TabIndex = 26;
			// 
			// CostFreePercentageBar
			// 
			this.CostFreePercentageBar.AllowDrop = true;
			this.CostFreePercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 0, true);
			this.CostFreePercentageBar.Name = "CostFreePercentageBar";
			this.CostFreePercentageBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.CostFreePercentageBar.TabIndex = 31;
			// 
			// CostFreeLabel
			// 
			this.CostFreeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5f396f04-92a0-4c42-b1c5-fa8f7ed18188", "Cost Free");
			this.CostFreeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CostFreeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(816, 0, true);
			this.CostFreeLabel.Name = "CostFreeLabel";
			this.CostFreeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 18, true);
			this.CostFreeLabel.TabIndex = 30;
			// 
			// DensityVisualisationControl
			// 
			this.DensityVisualisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DensityVisualisationControl, "Consols.Density");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.Density)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).Density)));
			this.DensityVisualisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 42, true);
			this.DensityVisualisationControl.Name = "DensityVisualisationControl";
			this.DensityVisualisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 40, true);
			this.DensityVisualisationControl.TabIndex = 34;
			this.DensityVisualisationControl.ConfigureWidth(347);
			// 
			// DensityFactorLabel
			// 
			this.DensityFactorLabel.AutoSize = true;
			this.DensityFactorLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|171afb7e-2863-4e61-a938-23a15a49c7cb", "Density Factor");
			this.DensityFactorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DensityFactorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 63, true);
			this.DensityFactorLabel.Name = "DensityFactorLabel";
			this.DensityFactorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.DensityFactorLabel.TabIndex = 35;
			// 
			// ConsolidatedFreightCostTextBox
			// 
			this.ConsolidatedFreightCostTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ConsolidatedFreightCostTextBox, "Consols.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeable)));
			this.ConsolidatedFreightCostTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ConsolidatedFreightCostTextBox.DecimalPlaces = 2;
			this.ConsolidatedFreightCostTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 2, true);
			this.ConsolidatedFreightCostTextBox.Name = "ConsolidatedFreightCostTextBox";
			this.ConsolidatedFreightCostTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.ConsolidatedFreightCostTextBox.TabIndex = 26;
			this.ConsolidatedFreightCostTextBox.Text = "0.00";
			this.ConsolidatedFreightCostTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsolidatedFreightCostDescTextBox
			// 
			this.ConsolidatedFreightCostDescTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ConsolidatedFreightCostDescTextBox, "Consols.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeableDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeableDesc)));
			this.ConsolidatedFreightCostDescTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsolidatedFreightCostDescTextBox, false);
			this.ConsolidatedFreightCostDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 2, true);
			this.ConsolidatedFreightCostDescTextBox.Name = "ConsolidatedFreightCostDescTextBox";
			this.ConsolidatedFreightCostDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.ConsolidatedFreightCostDescTextBox.TabIndex = 27;
			// 
			// ShipmentFreightCostTextBox
			// 
			this.ShipmentFreightCostTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShipmentFreightCostTextBox, "Consols.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ShipmentFreightCostChargeable)));
			this.ShipmentFreightCostTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ShipmentFreightCostTextBox.DecimalPlaces = 2;
			this.ShipmentFreightCostTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 24, true);
			this.ShipmentFreightCostTextBox.Name = "ShipmentFreightCostTextBox";
			this.ShipmentFreightCostTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.ShipmentFreightCostTextBox.TabIndex = 28;
			this.ShipmentFreightCostTextBox.Text = "0.00";
			this.ShipmentFreightCostTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentFreightCostDescTextBox
			// 
			this.ShipmentFreightCostDescTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShipmentFreightCostDescTextBox, "Consols.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeableDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CalculationWrapper.JK_Calc_ShipmentFreightCostChargeableDesc)));
			this.ShipmentFreightCostDescTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentFreightCostDescTextBox, false);
			this.ShipmentFreightCostDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 24, true);
			this.ShipmentFreightCostDescTextBox.Name = "ShipmentFreightCostDescTextBox";
			this.ShipmentFreightCostDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.ShipmentFreightCostDescTextBox.TabIndex = 29;
			//
			// Commodity
			//
			this.Commodity.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Commodity, "Consols.JK_RH_NKConsolCommodity");
			this.Commodity.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("637bfd85-e88d-4ce1-b80e-0caa26cec676", "Commodity");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_RH_NKConsolCommodity)));
			this.Commodity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 100, true);
			this.Commodity.Name = "Commodity";
			this.Commodity.ShowDescriptionBox = false;
			this.Commodity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 21, true);
			this.Commodity.TabIndex = 15;
			// 
			// PreAllocatedGroupBox
			// 
			this.PreAllocatedGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4f755c7c-86f9-4216-bb0e-50d6e9f01f31", "Pre-Allocation");
			this.PreAllocatedGroupBox.Controls.Add(this.PreAllocatedWeightTextBox);
			this.PreAllocatedGroupBox.Controls.Add(this.PreAllocatedVolumeTextBox);
			this.PreAllocatedGroupBox.Controls.Add(this.PreAllocatedWeightUnitTextBox);
			this.PreAllocatedGroupBox.Controls.Add(this.PreAllocatedVolumeUnitTextBox);
			this.PreAllocatedGroupBox.Controls.Add(this.ChargeableTextBox);
			this.PreAllocatedGroupBox.Controls.Add(this.CutOffDateEdit);
			this.PreAllocatedGroupBox.Controls.Add(this.ConsolMaxDimsControl);
			this.PreAllocatedGroupBox.Controls.Add(this.DangerousGoodsControl);
			this.PreAllocatedGroupBox.Controls.Add(this.Commodity);
			this.PreAllocatedGroupBox.Controls.Add(this.TemperatureControlBlock);
			this.PreAllocatedGroupBox.Controls.Add(this.ContractsAndAllocationsControl);
			this.PreAllocatedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 120, true);
			this.PreAllocatedGroupBox.Name = "PreAllocatedGroupBox";
			this.PreAllocatedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1500, 130, true);
			this.PreAllocatedGroupBox.TabIndex = 33;
			this.PreAllocatedGroupBox.TabStop = false;
			this.PreAllocatedGroupBox.Dock = DockStyle.Bottom;
			//
			// PreAllocatedWeightTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreAllocatedWeightTextBox, "Consols.JK_TotalShipmentActWeightCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_TotalShipmentActWeightCheck)));
			this.PreAllocatedWeightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 18, true);
			this.PreAllocatedWeightTextBox.Name = "PreAllocatedWeightTextBox";
			this.PreAllocatedWeightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.PreAllocatedWeightTextBox.TabIndex = 8;
			this.PreAllocatedWeightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreAllocatedVolumeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreAllocatedVolumeTextBox, "Consols.JK_TotalShipmentActVolumeCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_TotalShipmentActVolumeCheck)));
			this.PreAllocatedVolumeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.PreAllocatedVolumeTextBox.Name = "PreAllocatedVolumeTextBox";
			this.PreAllocatedVolumeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.PreAllocatedVolumeTextBox.TabIndex = 10;
			this.PreAllocatedVolumeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreAllocatedWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreAllocatedWeightUnitTextBox, "Consols.WeightVerificationUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).WeightVerificationUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreAllocatedWeightUnitTextBox, false);
			this.PreAllocatedWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 18, true);
			this.PreAllocatedWeightUnitTextBox.Name = "PreAllocatedWeightUnitTextBox";
			this.PreAllocatedWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 17, true);
			this.PreAllocatedWeightUnitTextBox.TabIndex = 9;
			// 
			// PreAllocatedVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreAllocatedVolumeUnitTextBox, "Consols.VolumeVerificationUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).VolumeVerificationUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreAllocatedVolumeUnitTextBox, false);
			this.PreAllocatedVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 40, true);
			this.PreAllocatedVolumeUnitTextBox.Name = "PreAllocatedVolumeUnitTextBox";
			this.PreAllocatedVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 17, true);
			this.PreAllocatedVolumeUnitTextBox.TabIndex = 11;
			// 
			// ChargeableTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeableTextBox, "Consols.JK_TotalShipmentChargableCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_TotalShipmentChargableCheck)));
			this.ChargeableTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 62, true);
			this.ChargeableTextBox.Name = "ChargeableTextBox";
			this.ChargeableTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.ChargeableTextBox.TabIndex = 12;
			this.ChargeableTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CutOffDateEdit
			// 
			this.CutOffDateEdit.AllowDrop = true;
			this.CutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.CutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CutOffDateEdit, "Consols.JK_ConsolCutOffDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_ConsolCutOffDateLocal)));
			this.CutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 83, true);
			this.CutOffDateEdit.Name = "CutOffDateEdit";
			this.CutOffDateEdit.TabIndex = 13;
			// 
			// ConsolMaxDimsControl
			// 
			this.ConsolMaxDimsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolMaxDimsControl, "Consols");
			this.ConsolMaxDimsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 84, true);
			this.ConsolMaxDimsControl.Name = "ConsolMaxDimsControl";
			this.ConsolMaxDimsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 26, true);
			this.ConsolMaxDimsControl.TabIndex = 24;
			//
			// DangerousGoodsControl
			//
			this.BindingSource.SetBindingMember(this.DangerousGoodsControl, "Consols");
			this.DangerousGoodsControl.AllowDrop = true;
			this.DangerousGoodsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 10, true);
			this.DangerousGoodsControl.Name = "DangerousGoodsControl";
			this.DangerousGoodsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 88, true);
			this.DangerousGoodsControl.TabIndex = 14;
			//
			// TemperatureControlBlock
			//
			this.BindingSource.SetBindingMember(this.TemperatureControlBlock, "Consols");
			this.TemperatureControlBlock.Configure(new ConsolTemperatureControlConfiguration());
			this.TemperatureControlBlock.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 10, true);
			this.TemperatureControlBlock.Name = "TemperatureControlBlock";
			this.TemperatureControlBlock.TemperatureControlButton_SetVisibility(false);
			this.TemperatureControlBlock.TabIndex = 1;
			//
			// ContractsAndAllocationsControl
			//
			this.BindingSource.SetBindingMember(this.ContractsAndAllocationsControl, "Consols");
			this.ContractsAndAllocationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(743, 10, true);
			this.ContractsAndAllocationsControl.Name = "ContractsAndAllocationsControl";
			this.ContractsAndAllocationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 100, true);
			this.ContractsAndAllocationsControl.TabIndex = 1;
			this.ContractsAndAllocationsControl.Visible = ContractsPermissions.IsAllocationsVisible();
			// 
			// ConsolIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolIDTextBox, "Consols.JK_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_UniqueConsignRef)));
			this.ConsolIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 18, true);
			this.ConsolIDTextBox.Name = "ConsolIDTextBox";
			this.ConsolIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ConsolIDTextBox.TabIndex = 1;
			// 
			// LoadPortCodeFindBox
			// 
			this.LoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadPortCodeFindBox, "Consols.JK_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_RL_NKLoadPort)));
			this.LoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 62, true);
			this.LoadPortCodeFindBox.Name = "LoadPortCodeFindBox";
			this.LoadPortCodeFindBox.PreBoundMaxLength = 5;
			this.LoadPortCodeFindBox.ShouldResize = true;
			this.LoadPortCodeFindBox.ShowDescriptionBox = false;
			this.LoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.LoadPortCodeFindBox.TabIndex = 5;
			// 
			// DischargePortCodeFindBox
			// 
			this.DischargePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargePortCodeFindBox, "Consols.JK_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_RL_NKDischargePort)));
			this.DischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 83, true);
			this.DischargePortCodeFindBox.Name = "DischargePortCodeFindBox";
			this.DischargePortCodeFindBox.PreBoundMaxLength = 5;
			this.DischargePortCodeFindBox.ShouldResize = true;
			this.DischargePortCodeFindBox.ShowDescriptionBox = false;
			this.DischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.DischargePortCodeFindBox.TabIndex = 7;
			// 
			// MasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "Consols.JK_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_MasterBillNum)));
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 40, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.MasterBillTextBox.TabIndex = 3;
			// 
			// AgentTypeDropEdit
			// 
			this.AgentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentTypeDropEdit, "Consols.JK_AgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_AgentType)));
			this.AgentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 40, true);
			this.AgentTypeDropEdit.Name = "AgentTypeDropEdit";
			this.AgentTypeDropEdit.PreBoundMaxLength = 4;
			this.AgentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.AgentTypeDropEdit.TabIndex = 23;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "Consols.JK_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 18, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.TransportModeDropEdit.TabIndex = 2;
			// 
			// ConsolModeDropEdit
			// 
			this.ConsolModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolModeDropEdit, "Consols.JK_ConsolMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_ConsolMode)));
			this.ConsolModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 62, true);
			this.ConsolModeDropEdit.Name = "ConsolModeDropEdit";
			this.ConsolModeDropEdit.PreBoundMaxLength = 3;
			this.ConsolModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.ConsolModeDropEdit.TabIndex = 6;
			//
			// ShowSubsCheckBox
			//
			this.ShowSubsCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShowSubsCheckBox, "Consols.ShowSubHouseBillShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).ShowSubHouseBillShipments)));
			this.ShowSubsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolPlanningBoardForm|EF0C5FED-CC8F-474F-A7A1-FE23104193C0", "Show Sub\'s", "Show/Hide Sub Shipments on the grid linked to Master Shipments");
			this.ShowSubsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowSubsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 2, true);
			this.ShowSubsCheckBox.Name = "ShowSubsCheckBox";
			this.ShowSubsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ShowSubsCheckBox.TabIndex = 0;
			this.ShowSubsCheckBox.UseVisualStyleBackColor = false;
			//
			// ActionButtonsPanel
			//
			this.ActionButtonsPanel.Controls.Add(this.ShowSubsCheckBox);
			this.ActionButtonsPanel.Controls.Add(this.AttachButton);
			this.ActionButtonsPanel.Controls.Add(this.DetachButton);
			this.ActionButtonsPanel.Controls.Add(this.CreateNewConsolButton);
			this.ActionButtonsPanel.Controls.Add(this.SaveButton);
			this.ActionButtonsPanel.Controls.Add(this.SaveAndCloseButton);
			this.ActionButtonsPanel.Controls.Add(this.CancelAndCloseButton);
			this.ActionButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ActionButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 509, true);
			this.ActionButtonsPanel.Name = "ActionButtonsPanel";
			this.ActionButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 27, true);
			this.ActionButtonsPanel.TabIndex = 0;
			// 
			// AttachButton
			// 
			this.AttachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c3db59ab-59dd-4c7b-890c-600a7b02be1d", "Attach", "Attach selected shipments to current consol");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 2, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.AttachButton.TabIndex = 1;
			this.AttachButton.ToolTipCaption = null;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// DetachButton
			// 
			this.DetachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d68a27b2-608a-4a38-8827-e8c6623bb81a", "Detach", "Detach selected shipments from current consol");
			this.DetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 2, true);
			this.DetachButton.Name = "DetachButton";
			this.DetachButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DetachButton.TabIndex = 2;
			this.DetachButton.ToolTipCaption = null;
			this.DetachButton.UseVisualStyleBackColor = true;
			this.DetachButton.Click += new System.EventHandler(this.DetachButton_Click);
			// 
			// CreateNewConsolButton
			// 
			this.CreateNewConsolButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f16588f6-924d-4c38-9b9f-eb40d8a68cd9", "New Consol...", "Create new consol from the selected shipments");
			this.CreateNewConsolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 2, true);
			this.CreateNewConsolButton.Name = "CreateNewConsolButton";
			this.CreateNewConsolButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CreateNewConsolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CreateNewConsolButton.TabIndex = 3;
			this.CreateNewConsolButton.ToolTipCaption = null;
			this.CreateNewConsolButton.UseVisualStyleBackColor = true;
			this.CreateNewConsolButton.Click += new System.EventHandler(this.CreateNewConsolButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0978e017-9b3c-408e-bc3c-c12fc53a7d79", "Save", "Save changes");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1205, 2, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.SaveButton.TabIndex = 3;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f88644fe-c21d-45e7-90bd-93a9b18d01a2", "Save && Close", "Save changes and close the form");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1309, 2, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.SaveAndCloseButton.TabIndex = 4;
			this.SaveAndCloseButton.ToolTipCaption = null;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			this.SaveAndCloseButton.Click += new System.EventHandler(this.SaveAndCloseButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelAndCloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("92140a02-7272-4e64-b1fc-95474d5255f0", "Cancel", "Cancel changes and close the form");
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1413, 2, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelAndCloseButton.TabIndex = 5;
			this.CancelAndCloseButton.ToolTipCaption = null;
			this.CancelAndCloseButton.UseVisualStyleBackColor = true;
			this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelAndCloseButton_Click);
			// 
			// MainHorizontalSplitter
			// 
			this.MainHorizontalSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.MainHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainHorizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 260, true);
			this.MainHorizontalSplitter.Name = "MainHorizontalSplitter";
			this.MainHorizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 7, true);
			this.MainHorizontalSplitter.TabIndex = 0;
			this.MainHorizontalSplitter.TabStop = false;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ShipmentsGroupBox);
			this.TopPanel.Controls.Add(this.TopPanelVerticalSplitter);
			this.TopPanel.Controls.Add(this.ConsolsGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 260, true);
			this.TopPanel.TabIndex = 1;
			// 
			// ShipmentsGroupBox
			// 
			this.ShipmentsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1cc7ad88-233e-4faa-963c-0f131929c149", "Shipments");
			this.ShipmentsGroupBox.Controls.Add(this.ShipmentModuleButtonGrid);
			this.ShipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentsGroupBox.Name = "ShipmentsGroupBox";
			this.ShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 260, true);
			this.ShipmentsGroupBox.TabIndex = 1;
			this.ShipmentsGroupBox.TabStop = false;
			// 
			// ShipmentModuleButtonGrid
			// 
			this.ShipmentModuleButtonGrid.AllowDrop = true;
			this.ShipmentModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.ShipmentModuleButtonGrid.AttachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|ShipmentModuleButtonGrid|AttachButtonText", "Add...");
			this.BindingSource.SetBindingMember(this.ShipmentModuleButtonGrid, "Shipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Shipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Shipments_List)));
			this.ShipmentModuleButtonGrid.BindToFindBoxList = "Shipments_List";
			this.ShipmentModuleButtonGrid.DetachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|ShipmentModuleButtonGrid|DetachButtonText", "Remove");
			this.ShipmentModuleButtonGrid.DetachMessage = null;
			this.ShipmentModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentModuleButtonGrid.GridId = "82569be5-aea6-41be-bda8-6c80bb0346e2";
			// 
			// 
			// 
			this.ShipmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ShipmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ShipmentModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentModuleButtonGrid.InnerGrid.GridId = null;
			this.ShipmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ShipmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.ShipmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 207, true);
			this.ShipmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ShipmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ShipmentModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobShipment;
			this.ShipmentModuleButtonGrid.Name = "ShipmentModuleButtonGrid";
			this.ShipmentModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("69667f49-372a-45bc-beca-f7c91ae8c354", "Shipment");
			this.ShipmentModuleButtonGrid.ReadOnly = true;
			this.ShipmentModuleButtonGrid.ShowEditButton = false;
			this.ShipmentModuleButtonGrid.ShowNewButton = false;
			this.ShipmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 243, true);
			this.ShipmentModuleButtonGrid.TabIndex = 2;
			// 
			// TopPanelVerticalSplitter
			// 
			this.TopPanelVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.TopPanelVerticalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1036, 0, true);
			this.TopPanelVerticalSplitter.Name = "TopPanelVerticalSplitter";
			this.TopPanelVerticalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 260, true);
			this.TopPanelVerticalSplitter.TabIndex = 2;
			this.TopPanelVerticalSplitter.TabStop = false;
			// 
			// ConsolsGroupBox
			// 
			this.ConsolsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7672e8e6-1df8-499e-98ee-fcc33b3af45d", "Consols");
			this.ConsolsGroupBox.Controls.Add(this.ConsolModuleButtonGrid);
			this.ConsolsGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ConsolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1039, 0, true);
			this.ConsolsGroupBox.Name = "ConsolsGroupBox";
			this.ConsolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 260, true);
			this.ConsolsGroupBox.TabIndex = 3;
			this.ConsolsGroupBox.TabStop = false;
			// 
			// ConsolModuleButtonGrid
			// 
			this.ConsolModuleButtonGrid.AllowDrop = true;
			this.ConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.ConsolModuleButtonGrid.AttachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|ConsolModuleButtonGrid|AttachButtonText", "Add...");
			this.BindingSource.SetBindingMember(this.ConsolModuleButtonGrid, "Consols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols_List)));
			this.ConsolModuleButtonGrid.BindToFindBoxList = "Consols_List";
			this.ConsolModuleButtonGrid.DetachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDashboard|ConsolModuleButtonGrid|DetachButtonText", "Remove");
			this.ConsolModuleButtonGrid.DetachMessage = null;
			this.ConsolModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolModuleButtonGrid.GridId = "4c41723d-83e9-45d4-8e68-dcc28f84f8de";
			// 
			// 
			// 
			this.ConsolModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ConsolModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ConsolModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolModuleButtonGrid.InnerGrid.GridId = null;
			this.ConsolModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ConsolModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ConsolModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ConsolModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.ConsolModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 207, true);
			this.ConsolModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ConsolModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ConsolModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
			this.ConsolModuleButtonGrid.Name = "ConsolModuleButtonGrid";
			this.ConsolModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("0f0898e9-7dd4-410c-9a05-2c0962ffaa29", "Consolidation");
			this.ConsolModuleButtonGrid.ReadOnly = true;
			this.ConsolModuleButtonGrid.ShowEditButton = false;
			this.ConsolModuleButtonGrid.ShowNewButton = false;
			this.ConsolModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 243, true);
			this.ConsolModuleButtonGrid.TabIndex = 0;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.MainHorizontalSplitter, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.CurrentConsolGroupBox.ResumeLayout(false);
			this.CurrentConsolGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrentConsolShipmentModuleButtonGrid.InnerGrid)).EndInit();
			this.CurrentConsolShipmentModuleButtonGrid.ResumeLayout(true);
			this.CurrentConsolShipmentModuleButtonGrid.PerformLayout();
			this.CurrentConsolDetailsPanel.ResumeLayout(false);
			this.CurrentConsolDetailsPanel.PerformLayout();
			this.AchievedQuantitiesPanel.ResumeLayout(false);
			this.AchievedQuantitiesPanel.PerformLayout();
			this.AchievedQuantitiesGroupBox.ResumeLayout(false);
			this.AchievedQuantitiesGroupBox.PerformLayout();
			this.WeightUtilisationPercentageBar.ResumeLayout(true);
			this.WeightUtilisationPercentageBar.PerformLayout();
			this.VolumeUtilisationPercentageBar.ResumeLayout(true);
			this.VolumeUtilisationPercentageBar.PerformLayout();
			this.CostFreePercentageBar.ResumeLayout(true);
			this.CostFreePercentageBar.PerformLayout();
			this.DensityVisualisationControl.ResumeLayout(true);
			this.DensityVisualisationControl.PerformLayout();
			this.PreAllocatedGroupBox.ResumeLayout(false);
			this.PreAllocatedGroupBox.PerformLayout();
			this.CutOffDateEdit.ResumeLayout(true);
			this.CutOffDateEdit.PerformLayout();
			this.LoadPortCodeFindBox.ResumeLayout(true);
			this.LoadPortCodeFindBox.PerformLayout();
			this.DischargePortCodeFindBox.ResumeLayout(true);
			this.DischargePortCodeFindBox.PerformLayout();
			this.AgentTypeDropEdit.ResumeLayout(true);
			this.AgentTypeDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.ConsolModeDropEdit.ResumeLayout(true);
			this.ConsolModeDropEdit.PerformLayout();
			this.ActionButtonsPanel.ResumeLayout(false);
			this.ActionButtonsPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ShipmentsGroupBox.ResumeLayout(false);
			this.ShipmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentModuleButtonGrid.InnerGrid)).EndInit();
			this.ShipmentModuleButtonGrid.ResumeLayout(true);
			this.ShipmentModuleButtonGrid.PerformLayout();
			this.ConsolsGroupBox.ResumeLayout(false);
			this.ConsolsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).EndInit();
			this.ConsolModuleButtonGrid.ResumeLayout(true);
			this.ConsolModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		void ConsolDashboardResize(object sender, EventArgs args)
		{
			var newWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.Width) - 497;
			this.AchievedQuantitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(newWidth, 119, true);
		}

		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private CargoWise.Windows.UI.KSplitter MainHorizontalSplitter;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;

		private DashboardShipmentModuleButtonGrid ShipmentModuleButtonGrid;
		private StandaloneConsolModuleButtonGrid ConsolModuleButtonGrid;

		private Enterprise.ZArchitecture.GUI.ZGroupBox ShipmentsGroupBox;
		private CargoWise.Windows.UI.KSplitter TopPanelVerticalSplitter;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel ActionButtonsPanel;
		private Enterprise.ZArchitecture.GUI.ZButton AttachButton;
		private ZButton DetachButton;
		private ZButton CreateNewConsolButton;
		private ZButton SaveButton;
		private ZButton SaveAndCloseButton;
		private ZButton CancelAndCloseButton;

		private Enterprise.ZArchitecture.GUI.ZDropEdit AgentTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ConsolModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit CutOffDateEdit;

		private Enterprise.ZArchitecture.GUI.ZGroupBox CurrentConsolGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel CurrentConsolDetailsPanel;
		private ZArchitecture.ZTextBox ConsolIDTextBox;
		private ZArchitecture.ZTextBox MasterBillTextBox;
		private ZCodeFindBox LoadPortCodeFindBox;
		private ZCodeFindBox DischargePortCodeFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit VolumeWeightCalcEdit;
		private ZArchitecture.ZTextBox VolumeWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit ExcessWeightVolumeCalcEdit;
		private ZArchitecture.ZTextBox ExcessWeightVolumeUnitTextBox;
		private ZArchitecture.ZTextBox CorrectedWeightTextBox;
		private ZArchitecture.ZTextBox CorrectedWeightUnitTextBox;
		private ZArchitecture.ZTextBox CorrectedVolumeTextBox;
		private ZArchitecture.ZTextBox CorrectedVolumeUnitTextBox;
		private ZArchitecture.ZTextBox ConsolChargeableTextBox;
		private ZArchitecture.ZTextBox ConsolChargeableUnitTextBox;
		private ZArchitecture.ZTextBox PreAllocatedWeightTextBox;
		private ZArchitecture.ZTextBox PreAllocatedVolumeTextBox;
		private ZArchitecture.ZTextBox PreAllocatedWeightUnitTextBox;
		private ZArchitecture.ZTextBox PreAllocatedVolumeUnitTextBox;
		private ZArchitecture.ZTextBox CostFreeTextBox;
		private ZArchitecture.ZTextBox CostFreeUnitsTextBox;
		private ZArchitecture.ZTextBox ChargeableTextBox;
		private ZArchitecture.ZTextBox ConsolidatedFreightCostTextBox;
		private ZArchitecture.ZTextBox ShipmentFreightCostTextBox;
		private ZArchitecture.ZTextBox ConsolidatedFreightCostDescTextBox;
		private ZArchitecture.ZTextBox ShipmentFreightCostDescTextBox;

		private Enterprise.Freight.Forwarding.GUI.DashboardContractsAndAllocationControl ContractsAndAllocationsControl;

		private Enterprise.Freight.Forwarding.GUI.ConsolMaxDimsControl ConsolMaxDimsControl;
		private Enterprise.Freight.Forwarding.GUI.DangerousGoodsControl DangerousGoodsControl;
		private ZCodeFindBox Commodity;
		private Enterprise.Freight.Forwarding.GUI.TemperatureControlBlock TemperatureControlBlock;

		private LabelledPercentageBar WeightUtilisationPercentageBar;
		private LabelledPercentageBar VolumeUtilisationPercentageBar;
		private LabelledPercentageBar CostFreePercentageBar;
		private Enterprise.ZArchitecture.ZLabel WeightUtilisationLabel;
		private Enterprise.ZArchitecture.ZLabel VolumeUtilisationLabel;
		private Enterprise.ZArchitecture.ZLabel CostFreeLabel;

		private DensityVisualisationControl DensityVisualisationControl;
		private Enterprise.ZArchitecture.ZLabel DensityFactorLabel;

		private ShipmentModuleButtonGrid CurrentConsolShipmentModuleButtonGrid;
		private ZPanel AchievedQuantitiesPanel;
		private ZGroupBox AchievedQuantitiesGroupBox;
		private ZGroupBox PreAllocatedGroupBox;
		private ZCheckBox QuantitiesOverrideCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShowSubsCheckBox;
	}
}
