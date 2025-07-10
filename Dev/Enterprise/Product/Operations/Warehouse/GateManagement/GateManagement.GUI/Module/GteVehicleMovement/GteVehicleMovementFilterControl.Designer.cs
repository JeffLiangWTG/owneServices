namespace Enterprise.Warehouse.GateManagement.GUI
{
	partial class GteVehicleMovementFilterControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GVM_VehicleRegistration)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).VehicleType.RC_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateInVehicleEntry.GVE_GateActionNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateInVehicleEntry.GVE_EntryTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateInVehicleEntry.GVE_DriverName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateInVehicleEntry.GVE_DriverLicenseNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateOutVehicleEntry.GVE_GateActionNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateOutVehicleEntry.GVE_EntryTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateOutVehicleEntry.GVE_DriverName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement)(null)).GateOutVehicleEntry.GVE_DriverLicenseNumber)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d1-f20b-4502-920f-b9ad5f8f773b", "Vehicle Registration");
            zTextBoxColumnStyleInfo1.ColumnName = "GVM_VehicleRegistration";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d2-f20u-4502-920f-b9ad5f8f773b", "Vehicle Type");
            zTextBoxColumnStyleInfo2.ColumnName = "VehicleType+RC_Code";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d2-f21p-4512-920f-b9ad5f8f073b", "Gate-in Number");
            zTextBoxColumnStyleInfo3.ColumnName = "GateInVehicleEntry+GVE_GateActionNumber";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("e359c2d2-g21z-4512-920f-f9ad5f8f073b", "Entry Time");
            zTextBoxColumnStyleInfo4.ColumnName = "GateInVehicleEntry+GVE_EntryTime";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d2-f21c-4512-920f-b9ad5f8f073b", "Entry Driver");
            zTextBoxColumnStyleInfo5.ColumnName = "GateInVehicleEntry+GVE_DriverName";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d2-f21v-4512-920f-b9ad5f8f073b", "Entry License");
            zTextBoxColumnStyleInfo6.ColumnName = "GateInVehicleEntry+GVE_DriverLicenseNumber";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d5-f21b-4512-920f-b9ad5f8f073b", "Gate-out Number");
            zTextBoxColumnStyleInfo7.ColumnName = "GateOutVehicleEntry+GVE_GateActionNumber";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("e359c2d6-g21b-4512-920f-f9ad5f8f073b", "Exit Time");
            zTextBoxColumnStyleInfo8.ColumnName = "GateOutVehicleEntry+GVE_EntryTime";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d7-f21b-4512-920f-b9ad5f8f073b", "Exit Driver");
            zTextBoxColumnStyleInfo9.ColumnName = "GateOutVehicleEntry+GVE_DriverName";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Warehouse.GateManagement.GUI.Res.GetData("d358b2d8-f21b-4512-920f-b9ad5f8f073b", "Exit License");
			zTextBoxColumnStyleInfo10.ColumnName = "GateOutVehicleEntry+GVE_DriverLicenseNumber";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.GateManagement.Business.GteVehicleMovement);
            // 
            // GteVehicleMovementFilterControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Name = "GteVehicleMovementFilterControl";
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
