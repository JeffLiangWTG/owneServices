namespace Enterprise.ContractManagement.GUI
{
	partial class ContractAndAllocationsGridPanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierContractsFilterGrid = new CarrierContractsFilterGrid();
			this.AllocationRouteGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierContractsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AllocationRoutesGrid = new AllocationRouteGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.CarrierContractsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CarrierContractsFilterGrid)).BeginInit();
			this.CarrierContractsFilterGrid.SuspendLayout();
			this.AllocationRouteGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocationRoutesGrid)).BeginInit();
			this.AllocationRoutesGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.ContractManagement.Business.ViewCarrierContractsManager);
			//
			// MainGroupBox
			//
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400, true);
			this.MainGroupBox.Controls.Add(this.MainSplitContainer);
			this.MainGroupBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.MainSplitContainer.AutoSize = false;
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 280, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.MainSplitContainer.TabIndex = 5;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.CarrierContractsGroupBox);
			this.MainSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 0, true);
			this.MainSplitContainer.Panel1.AutoSize = false;
			this.MainSplitContainer.Panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 280, true);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.AllocationRouteGroupBox);
			this.MainSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.MainSplitContainer.Panel2.AutoSize = false;
			this.MainSplitContainer.Panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 280, true);
			// 
			// CarrierContractsGroupBox
			// 
			this.CarrierContractsGroupBox.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("964bc40e-4cbb-b6a6-4970-fd05317e8676", "Carrier Contract");
			this.CarrierContractsGroupBox.Controls.Add(this.CarrierContractsFilterGrid);
			this.CarrierContractsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarrierContractsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 10, true);
			this.CarrierContractsGroupBox.Name = "CarrierContractsGroupBox";
			this.CarrierContractsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 270, true);
			this.CarrierContractsGroupBox.TabIndex = 2;
			this.CarrierContractsGroupBox.TabStop = false;
			//
			// CarrierContractsGrid
			//
			this.CarrierContractsFilterGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CarrierContractsFilterGrid, "CarrierContracts");
			this.CarrierContractsFilterGrid.GridId = "5f5a4b2b-316c-13a3-4ea3-dd69c3760045";
			this.CarrierContractsFilterGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CarrierContractsFilterGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.CarrierContractsFilterGrid.Name = "CarrierContractsFilterGrid";
			this.CarrierContractsFilterGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 0, true);
			this.CarrierContractsFilterGrid.TabIndex = 0;
			// 
			// AllocationRouteGroupBox
			// 
			this.AllocationRouteGroupBox.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("ec566cc8-ba49-a9b5-4223-8b149328a308", "Allocation Routes For Selected Contract");
			this.AllocationRouteGroupBox.Controls.Add(this.AllocationRoutesGrid);
			this.AllocationRouteGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocationRouteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.AllocationRouteGroupBox.Name = "AllocationRouteGroupBox";
			this.AllocationRouteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 270, true);
			this.AllocationRouteGroupBox.TabIndex = 3;
			this.AllocationRouteGroupBox.TabStop = false;
			//
			// AllocationRoutesGrid
			//
			this.BindingSource.SetBindingMember(this.AllocationRoutesGrid, "CarrierContracts.AllocationRoutesForUtilizationSimulation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ContractManagement.Business.RatingContract)(((System.Collections.IList)(((Enterprise.ContractManagement.Business.ViewCarrierContractsManager)(null)).CarrierContracts)).SyncRoot)).Allocations)));
			this.AllocationRoutesGrid.AllowNavigation = false;
			this.AllocationRoutesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocationRoutesGrid.GridId = "89059e5d-4c9d-428e-4d43-00990c3ec656";
			this.AllocationRoutesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllocationRoutesGrid.LayoutKey = "AllocationRoutesGrid";
			this.AllocationRoutesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocationRoutesGrid.Name = "AllocationRoutesGrid";
			this.AllocationRoutesGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 0, true);
			this.AllocationRoutesGrid.TabIndex = 5;
			// 
			// ContractAndAllocationsGridPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "ContractAndAllocationsGridPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.CarrierContractsGroupBox.ResumeLayout(false);
			this.CarrierContractsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CarrierContractsFilterGrid)).EndInit();
			this.CarrierContractsFilterGrid.ResumeLayout(false);
			this.CarrierContractsFilterGrid.PerformLayout();
			this.AllocationRouteGroupBox.ResumeLayout(false);
			this.AllocationRouteGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocationRoutesGrid)).EndInit();
			this.AllocationRoutesGrid.ResumeLayout(false);
			this.AllocationRoutesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CarrierContractsGroupBox;
		private CarrierContractsFilterGrid CarrierContractsFilterGrid;
		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZGroupBox AllocationRouteGroupBox;
		private AllocationRouteGrid AllocationRoutesGrid;
	}
}
