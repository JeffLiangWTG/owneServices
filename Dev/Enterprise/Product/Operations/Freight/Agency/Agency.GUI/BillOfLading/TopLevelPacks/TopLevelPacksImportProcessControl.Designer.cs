namespace Enterprise.Freight.Agency.GUI
{
	partial class TopLevelPacksImportProcessControl
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
			this.unloadDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.slotDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.slotBookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.deliveredToCustDoorDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ContainerImportDOReleaseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			vhcWharfGateOutDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipmentContainersView);
			// 
			// vhcWharfGateOutDateEdit
			// 
			vhcWharfGateOutDateEdit.AllowDrop = true;
			vhcWharfGateOutDateEdit.AutoCompleteMonthThreshold = 1;
			vhcWharfGateOutDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(vhcWharfGateOutDateEdit, "JC_FCLWharfGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_FCLWharfGateOut)));
			vhcWharfGateOutDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("218b7f98-9f49-4e57-b2d9-669237764e6e", "Wharf Gate Out");
			vhcWharfGateOutDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			vhcWharfGateOutDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 112, true);
			vhcWharfGateOutDateEdit.Name = "vhcWharfGateOutDateEdit";
			vhcWharfGateOutDateEdit.TabIndex = 4;
			// 
			// unloadDateEdit
			// 
			this.unloadDateEdit.AllowDrop = true;
			this.unloadDateEdit.AutoCompleteMonthThreshold = 1;
			this.unloadDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.unloadDateEdit, "JC_FCLUnloadFromVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_FCLUnloadFromVessel)));
			this.unloadDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("8b25b12d-9a4b-44a6-a3d8-a4931470ba2b", "Unload");
			this.unloadDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.unloadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.unloadDateEdit.Name = "unloadDateEdit";
			this.unloadDateEdit.TabIndex = 0;
			// 
			// slotDateEdit
			// 
			this.slotDateEdit.AllowDrop = true;
			this.slotDateEdit.AutoCompleteMonthThreshold = 1;
			this.slotDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.slotDateEdit, "JC_ArrivalSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_ArrivalSlotDateTime)));
			this.slotDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("015834d9-3421-4aec-8e26-af759150d992", "Slot Date");
			this.slotDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.slotDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 34, true);
			this.slotDateEdit.Name = "slotDateEdit";
			this.slotDateEdit.TabIndex = 1;
			// 
			// slotBookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.slotBookingRefTextBox, "JC_ArrivalSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_ArrivalSlotReference)));
			this.slotBookingRefTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7068de0d-7d5d-48c4-9282-fa3495ae9f8c", "Slot Bkg. Ref", "Slot Booking Ref", "Slot Booking Reference", "");
			this.slotBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.slotBookingRefTextBox.Name = "slotBookingRefTextBox";
			this.slotBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.slotBookingRefTextBox.TabIndex = 2;
			// 
			// deliveredToCustDoorDateEdit
			// 
			this.deliveredToCustDoorDateEdit.AllowDrop = true;
			this.deliveredToCustDoorDateEdit.AutoCompleteMonthThreshold = 1;
			this.deliveredToCustDoorDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.deliveredToCustDoorDateEdit, "JC_ArrivalCartageComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_ArrivalCartageComplete)));
			this.deliveredToCustDoorDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("6cdb5c7f-03b8-484f-aa62-18ff605f31a8", "Delivered", "Delivered to Customer", "Delivered to Customer Door", "");
			this.deliveredToCustDoorDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.deliveredToCustDoorDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 138, true);
			this.deliveredToCustDoorDateEdit.Name = "deliveredToCustDoorDateEdit";
			this.deliveredToCustDoorDateEdit.TabIndex = 5;
			// 
			// ContainerImportDOReleaseTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContainerImportDOReleaseTextBox, "JC_ContainerImportDORelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_ContainerImportDORelease)));
			this.ContainerImportDOReleaseTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("6445993a-daed-4665-9b75-10aa5f271a57", "Release Num.", "Release Number", "");
			this.ContainerImportDOReleaseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 86, true);
			this.ContainerImportDOReleaseTextBox.Name = "ContainerImportDOReleaseTextBox";
			this.ContainerImportDOReleaseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.ContainerImportDOReleaseTextBox.TabIndex = 3;
			// 
			// TopLevelPacksImportProcessControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerImportDOReleaseTextBox);
			this.Controls.Add(vhcWharfGateOutDateEdit);
			this.Controls.Add(this.deliveredToCustDoorDateEdit);
			this.Controls.Add(this.slotBookingRefTextBox);
			this.Controls.Add(this.slotDateEdit);
			this.Controls.Add(this.unloadDateEdit);
			this.Name = "TopLevelPacksImportProcessControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDateEdit unloadDateEdit;
		ZArchitecture.GUI.ZDateEdit slotDateEdit;
		ZArchitecture.ZTextBox slotBookingRefTextBox;
		ZArchitecture.GUI.ZDateEdit deliveredToCustDoorDateEdit;
		ZArchitecture.ZTextBox ContainerImportDOReleaseTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit vhcWharfGateOutDateEdit;
	}
}
