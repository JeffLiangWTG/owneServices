namespace Enterprise.Freight.Agency.GUI
{
	partial class TopLevelPacksExportProcessControl
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
			this.pickupFromCustDoorDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.slotDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.slotRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.loadedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DockReceiptTextBox = new Enterprise.ZArchitecture.ZTextBox();
			vhcWharfGateInDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipmentContainersView);
			// 
			// vhcWharfGateInDateEdit
			// 
			vhcWharfGateInDateEdit.AllowDrop = true;
			vhcWharfGateInDateEdit.AutoCompleteMonthThreshold = 1;
			vhcWharfGateInDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(vhcWharfGateInDateEdit, "JC_FCLWharfGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_FCLWharfGateIn)));
			vhcWharfGateInDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("18f80db7-e6ed-41f1-ab6e-056b9aab32b5", "Wharf Gate In");
			vhcWharfGateInDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			vhcWharfGateInDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 86, true);
			vhcWharfGateInDateEdit.Name = "vhcWharfGateInDateEdit";
			vhcWharfGateInDateEdit.TabIndex = 3;
			// 
			// pickupFromCustDoorDateEdit
			// 
			this.pickupFromCustDoorDateEdit.AllowDrop = true;
			this.pickupFromCustDoorDateEdit.AutoCompleteMonthThreshold = 1;
			this.pickupFromCustDoorDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.pickupFromCustDoorDateEdit, "JC_DepartureCartageComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_DepartureCartageComplete)));
			this.pickupFromCustDoorDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ca42460f-c394-484a-85ca-fc56a6451184", "Pickup", "Pickup From Customer", "Pickup From Customer Door", "");
			this.pickupFromCustDoorDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupFromCustDoorDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.pickupFromCustDoorDateEdit.Name = "pickupFromCustDoorDateEdit";
			this.pickupFromCustDoorDateEdit.TabIndex = 0;
			// 
			// slotDateEdit
			// 
			this.slotDateEdit.AllowDrop = true;
			this.slotDateEdit.AutoCompleteMonthThreshold = 1;
			this.slotDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.slotDateEdit, "JC_DepartureSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_DepartureSlotDateTime)));
			this.slotDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("47cffada-85aa-42ed-a55c-9361ad90588f", "Slot Date");
			this.slotDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.slotDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 34, true);
			this.slotDateEdit.Name = "slotDateEdit";
			this.slotDateEdit.TabIndex = 1;
			// 
			// slotRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.slotRefTextBox, "JC_DepartureSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_DepartureSlotReference)));
			this.slotRefTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("fff099a3-d6b0-48c1-a1fc-5eefe7f1c8cf", "Slot Bkg. Ref", "Slot Booking Ref", "Slot Booking Reference", "");
			this.slotRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.slotRefTextBox.Name = "slotRefTextBox";
			this.slotRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.slotRefTextBox.TabIndex = 2;
			// 
			// loadedDateEdit
			// 
			this.loadedDateEdit.AllowDrop = true;
			this.loadedDateEdit.AutoCompleteMonthThreshold = 1;
			this.loadedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.loadedDateEdit, "JC_FCLOnBoardVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_FCLOnBoardVessel)));
			this.loadedDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("d2e786d1-496e-4595-830e-711cb1a82a8d", "Loaded");
			this.loadedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.loadedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 138, true);
			this.loadedDateEdit.Name = "loadedDateEdit";
			this.loadedDateEdit.TabIndex = 5;
			// 
			// DockReceiptTextBox
			// 
			this.BindingSource.SetBindingMember(this.DockReceiptTextBox, "JC_DepartureDockReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_DepartureDockReceipt)));
			this.DockReceiptTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 112, true);
			this.DockReceiptTextBox.Name = "DockReceiptTextBox";
			this.DockReceiptTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.DockReceiptTextBox.TabIndex = 4;
			// 
			// TopLevelPacksExportProcessControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DockReceiptTextBox);
			this.Controls.Add(vhcWharfGateInDateEdit);
			this.Controls.Add(this.loadedDateEdit);
			this.Controls.Add(this.slotRefTextBox);
			this.Controls.Add(this.slotDateEdit);
			this.Controls.Add(this.pickupFromCustDoorDateEdit);
			this.Name = "TopLevelPacksExportProcessControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 167, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDateEdit pickupFromCustDoorDateEdit;
		ZArchitecture.GUI.ZDateEdit slotDateEdit;
		ZArchitecture.ZTextBox slotRefTextBox;
		ZArchitecture.GUI.ZDateEdit loadedDateEdit;
		ZArchitecture.ZTextBox DockReceiptTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit vhcWharfGateInDateEdit;
	}
}
