using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.GUI
{
	partial class ConfirmDetailsControl
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
			this.DepartureContainerPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureContainerTransportCoNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureContainerDriversNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureContainerDriversLicenseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureContainerDropModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DepartureContainerGoodsSignedByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureContainerTruckRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportCoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonPickupDeliveryConfirm);
			// 
			// DepartureContainerPickupDateEdit
			// 
			this.DepartureContainerPickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureContainerPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureContainerPickupDateEdit, "EU_PlannedPickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PlannedPickupDeliveryTime)));
			this.DepartureContainerPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DepartureContainerPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.DepartureContainerPickupDateEdit.Name = "DepartureContainerPickupDateEdit";
			this.DepartureContainerPickupDateEdit.TabIndex = 0;
			// 
			// DepartureContainerTransportCoNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerTransportCoNameTextBox, "EU_TransportCoName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_TransportCoName)));
			this.DepartureContainerTransportCoNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 44, true);
			this.DepartureContainerTransportCoNameTextBox.Name = "DepartureContainerTransportCoNameTextBox";
			this.DepartureContainerTransportCoNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.DepartureContainerTransportCoNameTextBox.TabIndex = 7;
			// 
			// DepartureContainerDriversNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerDriversNameTextBox, "EU_DriversName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversName)));
			this.DepartureContainerDriversNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 66, true);
			this.DepartureContainerDriversNameTextBox.Name = "DepartureContainerDriversNameTextBox";
			this.DepartureContainerDriversNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.DepartureContainerDriversNameTextBox.TabIndex = 8;
			// 
			// DepartureContainerDriversLicenseTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerDriversLicenseTextBox, "EU_DriversLicence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversLicence)));
			this.DepartureContainerDriversLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 88, true);
			this.DepartureContainerDriversLicenseTextBox.Name = "DepartureContainerDriversLicenseTextBox";
			this.DepartureContainerDriversLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DepartureContainerDriversLicenseTextBox.TabIndex = 9;
			// 
			// DepartureContainerDropModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerDropModeDropEdit, "EU_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DropMode)));
			this.DepartureContainerDropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 88, true);
			this.DepartureContainerDropModeDropEdit.Name = "DepartureContainerDropModeDropEdit";
			this.DepartureContainerDropModeDropEdit.PreBoundMaxLength = 3;
			this.DepartureContainerDropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.DepartureContainerDropModeDropEdit.TabIndex = 4;
			// 
			// DepartureContainerGoodsSignedByTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerGoodsSignedByTextBox, "EU_GoodsSignForBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_GoodsSignForBy)));
			this.DepartureContainerGoodsSignedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 66, true);
			this.DepartureContainerGoodsSignedByTextBox.Name = "DepartureContainerGoodsSignedByTextBox";
			this.DepartureContainerGoodsSignedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.DepartureContainerGoodsSignedByTextBox.TabIndex = 3;
			// 
			// DepartureContainerTruckRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureContainerTruckRegistrationTextBox, "EU_VehicleRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_VehicleRegistration)));
			this.DepartureContainerTruckRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 110, true);
			this.DepartureContainerTruckRegistrationTextBox.Name = "DepartureContainerTruckRegistrationTextBox";
			this.DepartureContainerTruckRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DepartureContainerTruckRegistrationTextBox.TabIndex = 10;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "EU_RequestedPickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_RequestedPickupDeliveryTime)));
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 22, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 1;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "EU_PickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryTime)));
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 44, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EU_PickupDeliveryInstruction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryInstruction)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.zTextBox1, 0);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 132, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 39, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// TransportCoAddressControl
			// 
			this.BindingSource.SetBindingMember(this.TransportCoAddressControl, "EU_OA_TransportProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_OA_TransportProvider)));
			this.TransportCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			this.TransportCoAddressControl.Name = "TransportCoAddressControl";
			this.TransportCoAddressControl.PopupCaption = "";
			this.TransportCoAddressControl.ShowAddress = false;
			this.TransportCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 42, true);
			this.TransportCoAddressControl.StackControls = true;
			this.TransportCoAddressControl.TabIndex = 6;
			// 
			// ConfirmDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportCoAddressControl);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zDateEdit2);
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.DepartureContainerPickupDateEdit);
			this.Controls.Add(this.DepartureContainerTransportCoNameTextBox);
			this.Controls.Add(this.DepartureContainerDriversNameTextBox);
			this.Controls.Add(this.DepartureContainerDriversLicenseTextBox);
			this.Controls.Add(this.DepartureContainerDropModeDropEdit);
			this.Controls.Add(this.DepartureContainerGoodsSignedByTextBox);
			this.Controls.Add(this.DepartureContainerTruckRegistrationTextBox);
			this.Name = "ConfirmDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 173, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZDateEdit DepartureContainerPickupDateEdit;
		private Enterprise.ZArchitecture.ZTextBox DepartureContainerTransportCoNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox DepartureContainerDriversNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox DepartureContainerDriversLicenseTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DepartureContainerDropModeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox DepartureContainerGoodsSignedByTextBox;
		private Enterprise.ZArchitecture.ZTextBox DepartureContainerTruckRegistrationTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZAddressControl TransportCoAddressControl;

	}
}
