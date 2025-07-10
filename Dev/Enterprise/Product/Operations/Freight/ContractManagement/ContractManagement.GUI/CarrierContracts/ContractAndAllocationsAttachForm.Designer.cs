using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ContractManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	partial class ContractAndAllocationsAttachForm
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
			this.ContractHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectContractButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllocationRouteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AllocationRouteChildrenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TopContractFilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainHorizontalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.BottomAllocationRoutePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();

			this.ContractCommitmentAndCapacityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentContractQuantityUnitPair = new CarrierContractUnitPairControl();
			this.CurrentContractCapacityWithVarianceUnitPair = new CarrierContractUnitPairControl();

			this.CurrentJobGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentJobQuantityUnitPair = new CarrierContractUnitPairControl();

			this.CurrentUtilizationAndAllocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrentUtilizationUnitPair = new CarrierContractUnitPairControl();
			this.CurrentOutstandingCommittedUnitPair = new CarrierContractUnitPairControl();
			this.CurrentOutstandingWithVarianceUnitPair = new CarrierContractUnitPairControl();

			this.AllocationSimulationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SimulationUtilizationUnitPair = new CarrierContractUnitPairControl();
			this.SimulationOutstandingCommittedUnitPair = new CarrierContractUnitPairControl();
			this.SimulationOutstandingWithVarianceUnitPair = new CarrierContractUnitPairControl();

			((System.ComponentModel.ISupportInitialize)this.MessageStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.ErrorStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.BindingSource).BeginInit();
			this.ButtonsPanel.SuspendLayout();

			this.TopContractFilterPanel.SuspendLayout();
			this.MainHorizontalSplitter.SuspendLayout();
			this.BottomAllocationRoutePanel.SuspendLayout();
			this.TableLayoutPanel.SuspendLayout();
			this.SuspendLayout();

			var userControlType = Type.GetType("Enterprise.ContractManagement.Module.CarrierContractFilterStripControl, Enterprise.ContractManagement.Module", true);
			this.ContractFilterStripControl = (ZFilterStripCommonControl)Activator.CreateInstance(userControlType, configuration);
			this.ContractFilterStripControl.SuspendLayout();

			this.ContractFilterStripControl.Anchor = (AnchorStyles.Top)
				| AnchorStyles.Left | AnchorStyles.Bottom;
			this.ContractFilterStripControl.Name = "ContractFilterStripControl";
			this.ContractFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 400, true);
			this.ContractFilterStripControl.TabIndex = 1;
			this.ContractFilterStripControl.Dock = DockStyle.Fill;
			this.ContractHeaderGroupBox.Name = "ContractHeaderGroupBox";
			this.ContractHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 250, true);
			this.ContractHeaderGroupBox.TabIndex = 4;
			this.ContractHeaderGroupBox.Controls.Add(this.ContractFilterStripControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContractHeaderGroupBox, false);
			this.ContractHeaderGroupBox.Anchor = AnchorStyles.Top |
					AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
			//
			// TopContractFilterPanel
			//
			this.TopContractFilterPanel.Controls.Add(this.ContractHeaderGroupBox);
			this.TopContractFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopContractFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopContractFilterPanel.Name = "TopContractFilterPanel";
			this.TopContractFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 250, true);
			this.TopContractFilterPanel.TabIndex = 1;
			// 
			// MainHorizontalSplitter
			// 
			this.MainHorizontalSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.MainHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainHorizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 260, true);
			this.MainHorizontalSplitter.Name = "MainHorizontalSplitter";
			this.MainHorizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 7, true);
			this.MainHorizontalSplitter.TabIndex = 2;
			this.MainHorizontalSplitter.TabStop = false;
			this.MainHorizontalSplitter.MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(457);
			this.MainHorizontalSplitter.SplitPosition = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(457);
			// 
			// AllocationRouteChildrenGroupBox
			//
			this.AllocationRouteChildrenGroupBox.Anchor = AnchorStyles.Top
				| AnchorStyles.Left
				| AnchorStyles.Right;
			this.AllocationRouteChildrenGroupBox.Controls.Add(this.TableLayoutPanel);
			this.AllocationRouteChildrenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocationRouteChildrenGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.AllocationRouteChildrenGroupBox.Name = "AllocationRouteChildrenGroupBox";
			this.AllocationRouteChildrenGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.AllocationRouteChildrenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 170, true);
			this.AllocationRouteChildrenGroupBox.TabIndex = 2;
			this.AllocationRouteChildrenGroupBox.TabStop = false;
			this.AllocationRouteChildrenGroupBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AllocationRouteChildrenGroupBox, false);
			// 
			// TableLayoutPanel
			// 
			this.TableLayoutPanel.ColumnCount = 3;
			this.TableLayoutPanel.RowCount = 2;
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.TableLayoutPanel.Controls.Add(this.ContractCommitmentAndCapacityGroupBox, 0, 0);
			this.TableLayoutPanel.Controls.Add(this.CurrentJobGroupBox, 0, 1);
			this.TableLayoutPanel.Controls.Add(this.CurrentUtilizationAndAllocationGroupBox, 1, 0);
			this.TableLayoutPanel.Controls.Add(this.AllocationSimulationGroupBox, 2, 0);
			this.TableLayoutPanel.SetRowSpan(this.CurrentUtilizationAndAllocationGroupBox, 2);
			this.TableLayoutPanel.SetRowSpan(this.AllocationSimulationGroupBox, 2);
			this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TableLayoutPanel.Name = "TableLayoutPanel";
			this.TableLayoutPanel.RowCount = 1;
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1800, 170, true);
			this.TableLayoutPanel.TabIndex = 0;
			// 
			// ContractCommitmentAndCapacityGroupBox
			//
			this.ContractCommitmentAndCapacityGroupBox.Dock = DockStyle.Fill;
			this.ContractCommitmentAndCapacityGroupBox.CaptionResourceString = Res.GetData("52255cd5-0fc2-ac89-4bd4-1f346f981d56", "Contract Commitment and Capacity");
			this.ContractCommitmentAndCapacityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContractCommitmentAndCapacityGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.ContractCommitmentAndCapacityGroupBox.Name = "ContractCommitmentAndCapacityGroupBox";
			this.ContractCommitmentAndCapacityGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.ContractCommitmentAndCapacityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 80, true);
			this.ContractCommitmentAndCapacityGroupBox.TabIndex = 2;
			this.ContractCommitmentAndCapacityGroupBox.AutoSize = true;
			this.ContractCommitmentAndCapacityGroupBox.TabStop = false;
			this.ContractCommitmentAndCapacityGroupBox.Controls.Add(this.CurrentContractQuantityUnitPair);
			this.ContractCommitmentAndCapacityGroupBox.Controls.Add(this.CurrentContractCapacityWithVarianceUnitPair);
			//
			// CurrentContractQuantityUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentContractQuantityUnitPair, "CarrierContracts.CarrierContractQuantities");
			this.CurrentContractQuantityUnitPair.Name = "CurrentContractQuantityUnitPair";
			this.CurrentContractQuantityUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.CurrentContractQuantityUnitPair.SetMainLabel(Res.GetData("a46e21e2-d1a9-5c8e-4525-40f1c9560686", "Quantity"));
			//
			// CurrentContractCapacityWithVarianceUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentContractCapacityWithVarianceUnitPair, "CarrierContracts.CarrierContractCapacityWithVariance");
			this.CurrentContractCapacityWithVarianceUnitPair.Name = "CurrentContractCapacityWithVarianceUnitPair";
			this.CurrentContractCapacityWithVarianceUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 50, true);
			this.CurrentContractCapacityWithVarianceUnitPair.SetMainLabel(Res.GetData("3fe0054e-49ec-44a2-450f-6553d0ecc445", "Capacity with Variance"));
			// 
			// CurrentJobGroupBox
			//
			this.CurrentJobGroupBox.Dock = DockStyle.Bottom;
			this.CurrentJobGroupBox.CaptionResourceString = Res.GetData("a815d79b-a427-d680-4798-9ba21a9ff8e0", "Current Job");
			this.CurrentJobGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 81, true);
			this.CurrentJobGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.CurrentJobGroupBox.Name = "CurrentJobGroupBox";
			this.CurrentJobGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.CurrentJobGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 80, true);
			this.CurrentJobGroupBox.TabIndex = 2;
			this.CurrentJobGroupBox.TabStop = false;
			this.CurrentJobGroupBox.Controls.Add(this.CurrentJobQuantityUnitPair);
			//
			// CurrentJobQuantityUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentJobQuantityUnitPair, "JobToBeAllocatedQuantities");
			this.CurrentJobQuantityUnitPair.Name = "CurrentJobQuantityUnitPair";
			this.CurrentJobQuantityUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.CurrentJobQuantityUnitPair.SetMainLabel(Res.GetData("35b5f00a-da8d-4d95-427c-ff56fcb562c6", "Quantity"));
			// 
			// AllocationTotalsGroupBox
			//
			this.CurrentUtilizationAndAllocationGroupBox.Dock = DockStyle.Fill;
			this.CurrentUtilizationAndAllocationGroupBox.CaptionResourceString = Res.GetData("429c0439-31da-0ead-44a0-cdbedb26de4c", "Current Utilization and O/S Allocation");
			this.CurrentUtilizationAndAllocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 0, true);
			this.CurrentUtilizationAndAllocationGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.CurrentUtilizationAndAllocationGroupBox.Name = "CurrentUtilizationAndAllocationGroupBox";
			this.CurrentUtilizationAndAllocationGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.CurrentUtilizationAndAllocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 160, true);
			this.CurrentUtilizationAndAllocationGroupBox.TabIndex = 2;
			this.CurrentUtilizationAndAllocationGroupBox.TabStop = false;
			this.CurrentUtilizationAndAllocationGroupBox.Controls.Add(CurrentUtilizationUnitPair);
			this.CurrentUtilizationAndAllocationGroupBox.Controls.Add(CurrentOutstandingCommittedUnitPair);
			this.CurrentUtilizationAndAllocationGroupBox.Controls.Add(CurrentOutstandingWithVarianceUnitPair);
			//
			// CurrentUtilizationUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentUtilizationUnitPair, "CarrierContracts.CurrentContractUtilisation");
			this.CurrentUtilizationUnitPair.Name = "CurrentUtilizationUnitPair";
			this.CurrentUtilizationUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.CurrentUtilizationUnitPair.SetMainLabel(Res.GetData("6aa761c8-7dd6-6c8c-4b95-189ea9734f86", "Utilization"));
			//
			// CurrentOutstandingCommittedUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentOutstandingCommittedUnitPair, "CarrierContracts.CurrentContractOutstandingCommitted");
			this.CurrentOutstandingCommittedUnitPair.Name = "CurrentOutstandingCommittedUnitPair";
			this.CurrentOutstandingCommittedUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 50, true);
			this.CurrentOutstandingCommittedUnitPair.SetMainLabel(Res.GetData("0af93586-59dc-14ae-41b6-6506104981a6", "Outstanding Committed"));
			//
			// CurrentOutstandingWithVarianceUnitPair
			//
			this.BindingSource.SetBindingMember(this.CurrentOutstandingWithVarianceUnitPair, "CarrierContracts.CurrentContractOutstandingWithVariance");
			this.CurrentOutstandingWithVarianceUnitPair.Name = "CurrentOutstandingWithVarianceUnitPair";
			this.CurrentOutstandingWithVarianceUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			this.CurrentOutstandingWithVarianceUnitPair.SetMainLabel(Res.GetData("ad9691cf-207a-3fba-4832-52f713fcf31b", "Outstanding with Variance"));
			// 
			// AllocationSimulationGroupBox
			//
			this.AllocationSimulationGroupBox.Dock = DockStyle.Fill;
			//this.AllocationSimulationGroupBox.Anchor = AnchorStyles.Right;
			this.AllocationSimulationGroupBox.CaptionResourceString = Res.GetData("a33376c3-5a9c-ff83-44e2-ff3b1d723ed3", "Allocation Simulation with Current Job");
			this.AllocationSimulationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(802, 0, true);
			this.AllocationSimulationGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.AllocationSimulationGroupBox.Name = "AllocationSimulationGroupBox";
			this.AllocationSimulationGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 3, 0, true);
			this.AllocationSimulationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 160, true);
			this.AllocationSimulationGroupBox.TabIndex = 2;
			this.AllocationSimulationGroupBox.TabStop = false;
			this.AllocationSimulationGroupBox.Controls.Add(SimulationUtilizationUnitPair);
			this.AllocationSimulationGroupBox.Controls.Add(SimulationOutstandingCommittedUnitPair);
			this.AllocationSimulationGroupBox.Controls.Add(SimulationOutstandingWithVarianceUnitPair);
			//
			// SimulationUtilizationUnitPair
			//
			this.BindingSource.SetBindingMember(this.SimulationUtilizationUnitPair, "CarrierContracts.AllocationRoutesForUtilizationSimulation.SimulatedUtilizationWithJobToBeAllocated");
			this.SimulationUtilizationUnitPair.Name = "SimulationUtilizationUnitPair";
			this.SimulationUtilizationUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.SimulationUtilizationUnitPair.SetMainLabel(Res.GetData("d1e1dfa4-ea03-c595-4773-810bab5a4042", "Utilization"));
			//
			// SimulationOutstandingCommittedUnitPair
			//
			this.BindingSource.SetBindingMember(this.SimulationOutstandingCommittedUnitPair, "CarrierContracts.AllocationRoutesForUtilizationSimulation.SimulatedOutstandingCommitedWithJobToBeAllocated");
			this.SimulationOutstandingCommittedUnitPair.Name = "SimulationOutstandingCommittedUnitPair";
			this.SimulationOutstandingCommittedUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 50, true);
			this.SimulationOutstandingCommittedUnitPair.SetMainLabel(Res.GetData("2bbb7cf1-0b7b-83a2-4490-472a22b31988", "Outstanding Committed"));
			//
			// SimulationOutstandingWithVarianceUnitPair
			//
			this.BindingSource.SetBindingMember(this.SimulationOutstandingWithVarianceUnitPair, "CarrierContracts.AllocationRoutesForUtilizationSimulation.SimulatedOutstandingWithVarianceWithJobToBeAllocated");
			this.SimulationOutstandingWithVarianceUnitPair.Name = "SimulationOutstandingWithVarianceUnitPair";
			this.SimulationOutstandingWithVarianceUnitPair.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			this.SimulationOutstandingWithVarianceUnitPair.SetMainLabel(Res.GetData("5ff2bdab-7042-e88f-4d64-a8e92bd49d75", "Outstanding with Variance"));
			//
			// ButtonsPanel
			//
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Controls.Add(SelectContractButton);
			this.ButtonsPanel.Controls.Add(SelectAllocationRouteButton);
			this.ButtonsPanel.Controls.Add(CancelButton);
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 240, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 27, true);
			this.ButtonsPanel.TabIndex = 6;
			// 
			// BottomAllocationRoutePanel
			// 
			this.BottomAllocationRoutePanel.Controls.Add(this.AllocationRouteChildrenGroupBox);
			this.BottomAllocationRoutePanel.Controls.Add(this.ButtonsPanel);
			this.BottomAllocationRoutePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomAllocationRoutePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomAllocationRoutePanel.Name = "BottomAllocationRoutePanel";
			this.BottomAllocationRoutePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 536, true);
			this.BottomAllocationRoutePanel.TabIndex = 7;
			// 
			// CancelButton
			// 
			this.CancelButton.Dock = DockStyle.Right;
			this.CancelButton.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("00d5e9d7-97e2-d99f-4fcf-71c4a210cd4f", "Cancel");
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelButton.TabIndex = 10;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += CancelButton_OnClick;
			// 
			// SelectContractButton
			// 
			this.SelectContractButton.Dock = DockStyle.Right;
			this.SelectContractButton.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("150ca61a-2627-78be-4026-13d2c5c47427", "Select Contract");
			this.SelectContractButton.Name = "SelectContractButton";
			this.SelectContractButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.SelectContractButton.TabIndex = 8;
			this.SelectContractButton.ToolTipCaption = null;
			this.SelectContractButton.UseVisualStyleBackColor = true;
			this.SelectContractButton.Click += SelectContractButton_Click;
			this.SelectContractButton.Visible = false;
			// 
			// SelectAllocationRouteButton
			// 
			this.SelectAllocationRouteButton.Dock = DockStyle.Right;
			this.SelectAllocationRouteButton.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("394d933b-4707-6f85-4fc8-f92190e2b534", "Select Allocation Route");
			this.SelectAllocationRouteButton.Name = "SelectAllocationRouteButton";
			this.SelectAllocationRouteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.SelectAllocationRouteButton.TabIndex = 9;
			this.SelectAllocationRouteButton.ToolTipCaption = null;
			this.SelectAllocationRouteButton.UseVisualStyleBackColor = true;
			this.SelectAllocationRouteButton.Click += SelectAllocationRouteButton_Click;
			this.SelectAllocationRouteButton.Visible = false;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);

			this.Controls.Add(this.TopContractFilterPanel);
			this.Controls.Add(this.MainHorizontalSplitter);
			this.Controls.Add(this.BottomAllocationRoutePanel);

			this.DataSourceType = typeof(ViewCarrierContractsManager);

			this.Name = "ContractAndAllocationsAttachForm";
			this.Controls.SetChildIndex(this.TopContractFilterPanel, 0);
			this.Controls.SetChildIndex(this.MainHorizontalSplitter, 0);
			this.Controls.SetChildIndex(this.BottomAllocationRoutePanel, 0);

			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

			this.BottomAllocationRoutePanel.ResumeLayout(false);
			this.BottomAllocationRoutePanel.PerformLayout();

			this.TopContractFilterPanel.ResumeLayout(false);
			this.TopContractFilterPanel.PerformLayout();

			this.MainHorizontalSplitter.ResumeLayout(false);
			this.MainHorizontalSplitter.PerformLayout();

			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ContractFilterStripControl.ResumeLayout(false);
			this.ContractFilterStripControl.PerformLayout();

			this.TableLayoutPanel.ResumeLayout(false);
			this.TableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel TopContractFilterPanel;
		CargoWise.Windows.UI.KSplitter MainHorizontalSplitter;
		Enterprise.ZArchitecture.GUI.ZPanel BottomAllocationRoutePanel;

		CargoWise.Windows.UI.KTableLayoutPanel TableLayoutPanel;

		#region Contract Commitment and Capacity

		Enterprise.ZArchitecture.GUI.ZGroupBox ContractCommitmentAndCapacityGroupBox;
		CarrierContractUnitPairControl CurrentContractQuantityUnitPair;
		CarrierContractUnitPairControl CurrentContractCapacityWithVarianceUnitPair;

		#endregion

		#region Current Job

		Enterprise.ZArchitecture.GUI.ZGroupBox CurrentJobGroupBox;
		CarrierContractUnitPairControl CurrentJobQuantityUnitPair;

		#endregion

		#region Current Utilization and O/S Allocation

		Enterprise.ZArchitecture.GUI.ZGroupBox CurrentUtilizationAndAllocationGroupBox;
		CarrierContractUnitPairControl CurrentUtilizationUnitPair;
		CarrierContractUnitPairControl CurrentOutstandingCommittedUnitPair;
		CarrierContractUnitPairControl CurrentOutstandingWithVarianceUnitPair;

		#endregion

		#region Allocation Simulation with Current Job

		Enterprise.ZArchitecture.GUI.ZGroupBox AllocationSimulationGroupBox;
		CarrierContractUnitPairControl SimulationUtilizationUnitPair;
		CarrierContractUnitPairControl SimulationOutstandingCommittedUnitPair;
		CarrierContractUnitPairControl SimulationOutstandingWithVarianceUnitPair;

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox ContractHeaderGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox AllocationRouteChildrenGroupBox;
		new ZButton CancelButton;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;

		Enterprise.ZArchitecture.GUI.ZFilterStripCommonControl ContractFilterStripControl;
		Enterprise.ZArchitecture.GUI.ZButton SelectAllocationRouteButton;
		Enterprise.ZArchitecture.GUI.ZButton SelectContractButton;
	}
}
