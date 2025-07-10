namespace Enterprise.Freight.Agency.GUI.BulkMovements
{
	partial class BulkMovementsApplicatorControl
	{
		void InitializeComponent()
		{
			this.movementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.movementDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.depotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkMovementsApplicator);
			// 
			// movementTypeDropEdit
			// 
			this.movementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.movementTypeDropEdit, "MovementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BulkMovementsApplicator)(null)).MovementType)));
			this.movementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.movementTypeDropEdit.Name = "movementTypeDropEdit";
			this.movementTypeDropEdit.PreBoundMaxLength = 3;
			this.movementTypeDropEdit.ShowDescriptionBox = false;
			this.movementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.movementTypeDropEdit.TabIndex = 0;
			// 
			// movementDateEdit
			// 
			this.movementDateEdit.AllowDrop = true;
			this.movementDateEdit.AutoCompleteMonthThreshold = 1;
			this.movementDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.movementDateEdit, "MovementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BulkMovementsApplicator)(null)).MovementDate)));
			this.movementDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.movementDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			this.movementDateEdit.Name = "movementDateEdit";
			this.movementDateEdit.TabIndex = 1;
			// 
			// depotAddressControl
			// 
			this.depotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.depotAddressControl, "DepotAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsApplicator)(null)).DepotAddressPK)));
			this.depotAddressControl.BindToOrgList = "Lookups+Depots";
			this.depotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			this.depotAddressControl.Name = "depotAddressControl";
			this.depotAddressControl.PopupCaption = "";
			this.depotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 59, true);
			this.depotAddressControl.TabIndex = 2;
			// 
			// BulkMovementsApplicatorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.depotAddressControl);
			this.Controls.Add(this.movementDateEdit);
			this.Controls.Add(this.movementTypeDropEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 98, true);
			this.Name = "BulkMovementsApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 98, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit movementTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit movementDateEdit;
		private Enterprise.ZArchitecture.GUI.ZAddressControl depotAddressControl;
	}
}
