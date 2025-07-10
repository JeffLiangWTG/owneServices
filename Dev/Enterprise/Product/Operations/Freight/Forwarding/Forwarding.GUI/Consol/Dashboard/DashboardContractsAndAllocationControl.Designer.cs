using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DashboardContractsAndAllocationControl
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.allocationIDFindBox = new ContractAllocationGuidFindBox();
			this.contractNumberTextBox = new ZArchitecture.ZTextBox();
			this.allocationQuantityTextBox = new ZArchitecture.ZTextBox();
			this.capacityWithVarianceTextBox = new ZArchitecture.ZTextBox();
			this.utilizationTextBox = new ZArchitecture.ZTextBox();
			this.outstandingCommittedTextBox = new ZArchitecture.ZTextBox();
			this.outstandingWithVarianceTextBox = new ZArchitecture.ZTextBox();
			this.allocationQuantityTEULabel = new Enterprise.ZArchitecture.ZLabel();
			this.capacityWithVarianceTEULabel = new Enterprise.ZArchitecture.ZLabel();
			this.utilizationTEULabel = new Enterprise.ZArchitecture.ZLabel();
			this.outstandingCommittedTEULabel = new Enterprise.ZArchitecture.ZLabel();
			this.outstandingWithVarianceTEULabel = new Enterprise.ZArchitecture.ZLabel();
			this.namedAccountsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZGridWithoutColumnStylesSerialisation();
			this.namedAccountFullNameColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.namedAccountCodeColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.namedAccountsDisplayGrid)).BeginInit();
			this.namedAccountsDisplayGrid.SuspendLayout();
			//
			// contractNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.contractNumberTextBox, "JK_CarrierContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_CarrierContractNumber)));
			this.contractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 8, true);
			this.contractNumberTextBox.Name = "contractNumberTextBox";
			this.contractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.contractNumberTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|acf9263b-0fd0-6398-4eb4-e4701d05690a", "Contract No.", "Contract Number");
			this.contractNumberTextBox.TabIndex = 1;
			this.contractNumberTextBox.ReadOnly = true;
			//
			// allocationIDFindBox
			//
			this.BindingSource.SetBindingMember(this.allocationIDFindBox, "JK_RCA_AllocationLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).JK_RCA_AllocationLine)));
			this.allocationIDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 30, true);
			this.allocationIDFindBox.Name = "allocationIDFindBox";
			this.allocationIDFindBox.ShowDescriptionBox = false;
			this.allocationIDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 21, true);
			this.allocationIDFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|69987d32-96af-fe82-4085-0bd919e315ae", "Allocation ID");
			this.allocationIDFindBox.TabIndex = 2;
			this.allocationIDFindBox.ReadOnly = true;
			//
			// allocationQuantityTextBox
			//
			this.BindingSource.SetBindingMember(this.allocationQuantityTextBox, "CarrierContract.CarrierContractQuantities.TEUValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CarrierContract.CarrierContractQuantities.TEUValue)));
			this.allocationQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 52, true);
			this.allocationQuantityTextBox.Name = "allocationQuantityTextBox";
			this.allocationQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 21, true);
			this.allocationQuantityTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|99799167-7f91-f1b6-4207-71aa10febf96", "Quantity");
			this.allocationQuantityTextBox.TabIndex = 3;
			this.allocationQuantityTextBox.ReadOnly = true;
			// 
			// allocationQuantityTEULabel
			// 
			this.allocationQuantityTEULabel.CaptionResourceString = Res.GetData("f4b6dc49-dec2-9888-4d5b-7ac69ab4c947", "TEU");
			this.allocationQuantityTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 52, true);
			this.allocationQuantityTEULabel.Name = "allocationQuantityTEULabel";
			this.allocationQuantityTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.allocationQuantityTEULabel.TabIndex = 4;
			//
			// capacityWithVarianceTextBox
			//
			this.BindingSource.SetBindingMember(this.capacityWithVarianceTextBox, "CarrierContract.CarrierContractCapacityWithVariance.TEUValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CarrierContract.CarrierContractCapacityWithVariance.TEUValue)));
			this.capacityWithVarianceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 74, true);
			this.capacityWithVarianceTextBox.Name = "capacityWithVarianceTextBox";
			this.capacityWithVarianceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 21, true);
			this.capacityWithVarianceTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|d3afb4d6-878c-dcb3-4a97-4b534ff98c28", "Capacity with Variance");
			this.capacityWithVarianceTextBox.TabIndex = 5;
			this.capacityWithVarianceTextBox.ReadOnly = true;
			// 
			// capacityWithVarianceTEULabel
			// 
			this.capacityWithVarianceTEULabel.CaptionResourceString = Res.GetData("f4b6dc49-dec2-9888-4d5b-7ac69ab4c947", "TEU");
			this.capacityWithVarianceTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 74, true);
			this.capacityWithVarianceTEULabel.Name = "capacityWithVarianceTEULabel";
			this.capacityWithVarianceTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.capacityWithVarianceTEULabel.TabIndex = 6;
			//
			// utilizationTextBox
			//
			this.BindingSource.SetBindingMember(this.utilizationTextBox, "CarrierContract.CurrentContractUtilisation.TEUValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CarrierContract.CurrentContractUtilisation.TEUValue)));
			this.utilizationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 30, true);
			this.utilizationTextBox.Name = "utilizationTextBox";
			this.utilizationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 21, true);
			this.utilizationTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|bc84469a-b31e-3ba3-431d-d602fef95895", "Utilization");
			this.utilizationTextBox.TabIndex = 7;
			this.utilizationTextBox.ReadOnly = true;
			// 
			// utilizationTEULabel
			// 
			this.utilizationTEULabel.CaptionResourceString = Res.GetData("f4b6dc49-dec2-9888-4d5b-7ac69ab4c947", "TEU");
			this.utilizationTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 30, true);
			this.utilizationTEULabel.Name = "utilizationTEULabel";
			this.utilizationTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.utilizationTEULabel.TabIndex = 8;
			//
			// outstandingCommittedTextBox
			//
			this.BindingSource.SetBindingMember(this.outstandingCommittedTextBox, "CarrierContract.CurrentContractOutstandingCommitted.TEUValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CarrierContract.CurrentContractOutstandingCommitted.TEUValue)));
			this.outstandingCommittedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 52, true);
			this.outstandingCommittedTextBox.Name = "outstandingCommittedTextBox";
			this.outstandingCommittedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 21, true);
			this.outstandingCommittedTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|e64ea8c9-786f-93a2-4fdf-a6f89101e5aa", "Outstanding Committed");
			this.outstandingCommittedTextBox.TabIndex = 9;
			this.outstandingCommittedTextBox.ReadOnly = true;
			// 
			// outstandingCommittedTEULabel
			// 
			this.outstandingCommittedTEULabel.CaptionResourceString = Res.GetData("f4b6dc49-dec2-9888-4d5b-7ac69ab4c947", "TEU");
			this.outstandingCommittedTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 52, true);
			this.outstandingCommittedTEULabel.Name = "outstandingCommittedTEULabel";
			this.outstandingCommittedTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.outstandingCommittedTEULabel.TabIndex = 10;
			//
			// outstandingWithVarianceTextBox
			//
			this.BindingSource.SetBindingMember(this.outstandingWithVarianceTextBox, "CarrierContract.CurrentContractOutstandingWithVariance.TEUValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ConsolDashboard)(null)).Consols)).SyncRoot)).CarrierContract.CurrentContractOutstandingWithVariance.TEUValue)));
			this.outstandingWithVarianceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 74, true);
			this.outstandingWithVarianceTextBox.Name = "outstandingWithVarianceTextBox";
			this.outstandingWithVarianceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 21, true);
			this.outstandingWithVarianceTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|fa96daa7-0563-e58b-4277-bc08157fb9e9", "Outstanding With Variance");
			this.outstandingWithVarianceTextBox.TabIndex = 11;
			this.outstandingWithVarianceTextBox.ReadOnly = true;
			// 
			// outstandingWithVarianceTEULabel
			// 
			this.outstandingWithVarianceTEULabel.CaptionResourceString = Res.GetData("f4b6dc49-dec2-9888-4d5b-7ac69ab4c947", "TEU");
			this.outstandingWithVarianceTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 74, true);
			this.outstandingWithVarianceTEULabel.Name = "outstandingWithVarianceTEULabel";
			this.outstandingWithVarianceTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.outstandingWithVarianceTEULabel.TabIndex = 12;
			//
			// namedAccountColumns
			//
			this.namedAccountCodeColumn.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.namedAccountCodeColumn.ColumnName = "Organisation.OH_Code";
			this.namedAccountCodeColumn.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|375cb07e-d94a-189b-4eca-07fd9a212608", "NAC");
			this.namedAccountCodeColumn.BindToList = "Lookups.NamedAccounts";
			this.namedAccountCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.namedAccountFullNameColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.namedAccountFullNameColumn.ColumnName = "Organisation.OH_FullNameTruncated";
			this.namedAccountFullNameColumn.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DashboardContractsAndAllocationControl|51dac58c-f312-3ca5-4b67-7de0529358d7", "Name");
			this.namedAccountFullNameColumn.BindToList = "Lookups.Substance";
			//
			// namedAccountsDisplayGrid
			//
			this.BindingSource.SetBindingMember(this.namedAccountsDisplayGrid, "CarrierContract.NamedAccountPivots");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).CarrierContract.NamedAccountPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ContractManagement.Business.RatingContractNamedAccountPivot)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).CarrierContract.NamedAccountPivots)).SyncRoot)).Organisation.OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ContractManagement.Business.RatingContractNamedAccountPivot)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).CarrierContract.NamedAccountPivots)).SyncRoot)).Organisation.OH_FullNameTruncated)));

			this.namedAccountsDisplayGrid.Name = "namedAccountsDisplayGrid";
			this.namedAccountsDisplayGrid.ColumnHeadersVisible = true;
			this.namedAccountsDisplayGrid.ColumnStyles.Add(namedAccountCodeColumn);
			this.namedAccountsDisplayGrid.ColumnStyles.Add(namedAccountFullNameColumn);
			this.namedAccountsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.namedAccountsDisplayGrid.LayoutKey = "zGrid1";
			this.namedAccountsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 0, true);
			this.namedAccountsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 90, true);
			this.namedAccountsDisplayGrid.TabIndex = 10;
			this.namedAccountsDisplayGrid.GridId = "5ae3b8f2-b052-91b1-4d9c-69c7ec5ab55f";
			this.namedAccountsDisplayGrid.ReadOnly = true;

			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.contractNumberTextBox);
			this.Controls.Add(this.allocationIDFindBox);
			this.Controls.Add(this.allocationQuantityTextBox);
			this.Controls.Add(this.capacityWithVarianceTextBox);
			this.Controls.Add(this.utilizationTextBox);
			this.Controls.Add(this.outstandingCommittedTextBox);
			this.Controls.Add(this.outstandingWithVarianceTextBox);

			this.Controls.Add(this.allocationQuantityTEULabel);
			this.Controls.Add(this.capacityWithVarianceTEULabel);
			this.Controls.Add(this.utilizationTEULabel);
			this.Controls.Add(this.outstandingCommittedTEULabel);
			this.Controls.Add(this.outstandingWithVarianceTEULabel);

			this.Controls.Add(this.namedAccountsDisplayGrid);

			this.Name = "DashboardContractsAndAllocationControl";
			this.Controls.SetChildIndex(this.namedAccountsDisplayGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.namedAccountsDisplayGrid)).EndInit();
			this.namedAccountsDisplayGrid.ResumeLayout(false);
			this.namedAccountsDisplayGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.ZTextBox contractNumberTextBox;
		private ZArchitecture.ZTextBox allocationQuantityTextBox;
		private ZArchitecture.ZTextBox capacityWithVarianceTextBox;
		private ZArchitecture.ZTextBox utilizationTextBox;
		private ZArchitecture.ZTextBox outstandingCommittedTextBox;
		private ZArchitecture.ZTextBox outstandingWithVarianceTextBox;

		private Enterprise.ZArchitecture.ZLabel allocationQuantityTEULabel;
		private Enterprise.ZArchitecture.ZLabel capacityWithVarianceTEULabel;
		private Enterprise.ZArchitecture.ZLabel utilizationTEULabel;
		private Enterprise.ZArchitecture.ZLabel outstandingCommittedTEULabel;
		private Enterprise.ZArchitecture.ZLabel outstandingWithVarianceTEULabel;

		private Enterprise.ZArchitecture.GUI.ZGridWithoutColumnStylesSerialisation namedAccountsDisplayGrid;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo namedAccountCodeColumn;
		private Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo namedAccountFullNameColumn;

		private ContractAllocationGuidFindBox allocationIDFindBox;
	}
}
