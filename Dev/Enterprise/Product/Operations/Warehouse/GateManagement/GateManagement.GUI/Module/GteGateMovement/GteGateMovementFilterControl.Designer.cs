using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	partial class GteGateMovementFilterControl
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

		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.BindingSource.SetBindingMember(this.grid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GGM_TransportReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GGM_UnitNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GGM_RH_NKCargoType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GGM_F3_NKPackageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GGM_RC_UnitType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).VehicleMovement.GateInVehicleEntry.GVE_GateActionNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).VehicleMovement.GateInVehicleEntry.GVE_EntryTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GateMovementBooking.GBM_MovementBookingNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GateMovementBooking.GBM_BookingReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GateMovementBooking.GBM_SourceReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GateMovementBooking.Booking.GBK_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Warehouse.GateManagement.Business.GteGateMovement)(null)).GateMovementBooking.Booking.Facility.WW_WarehouseNameMultilingual)));
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("dd8f3a50-2619-42ef-b1f0-0e4ea10d5fd2", "Transport Reference");
            zTextBoxColumnStyleInfo1.ColumnName = "GGM_TransportReference";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.Caption = "";
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("aa7aa1f4-6abd-4135-a6ed-210639093fdd", "Unit Number");
            zTextBoxColumnStyleInfo2.ColumnName = "GGM_UnitNumber";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.Caption = "";
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("6a564ba8-639d-48f5-baa0-fd8a1299acbd", "Cargo Type");
            zTextBoxColumnStyleInfo3.ColumnName = "GGM_RH_NKCargoType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.Caption = "";
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("4f554ca7-7efe-4e55-a6da-c0c20e46e42f", "Package Type");
            zTextBoxColumnStyleInfo4.ColumnName = "GGM_F3_NKPackageType";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.Caption = "";
            zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("1b10563f-59c8-4962-bbc5-6374aed1ff15", "Unit Type");
            zGuidFindBoxColumnStyleInfo1.ColumnName = "GGM_RC_UnitType";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.Caption = "";
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("60e01198-4902-4a2d-bfeb-2403a79a88d2", "Gate In Number");
            zTextBoxColumnStyleInfo5.ColumnName = "VehicleMovement+GateInVehicleEntry+GVE_GateActionNumber";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.Caption = "";
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("9a8f4fcc-35d4-4089-814a-3c321ed3a97e", "Gate In Time");
            zTextBoxColumnStyleInfo6.ColumnName = "VehicleMovement+GateInVehicleEntry+GVE_EntryTime";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.Caption = "";
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("12570462-01e4-485a-b70e-1757a34198ee", "Movement Booking Number");
            zTextBoxColumnStyleInfo7.ColumnName = "GateMovementBooking+GBM_MovementBookingNumber";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.Caption = "";
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("c45be590-f963-4f2f-878c-78ed047808eb", "Instruction Reference");
            zTextBoxColumnStyleInfo8.ColumnName = "GateMovementBooking+GBM_BookingReferenceNumber";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.Caption = "";
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("bfffdf18-2547-4302-b22a-6ad87c7e8638", "Booking Party Reference");
            zTextBoxColumnStyleInfo9.ColumnName = "GateMovementBooking+GBM_SourceReferenceNumber";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo10.Caption = "";
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("8356f2cd-9aaa-4788-8392-c1b0ca163621", "Booking Number");
            zTextBoxColumnStyleInfo10.ColumnName = "GateMovementBooking+Booking+GBK_ReferenceNumber";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("7c346a85-9688-4171-92a4-f9198f416075", "Facility");
            zTextBoxColumnStyleInfo11.ColumnName = "GateMovementBooking+Booking+Facility+WW_WarehouseNameMultilingual";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.GateManagement.Business.GteGateMovement);
            // 
            // GteGateMovementFilterControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Name = "GteGateMovementFilterControl";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}
