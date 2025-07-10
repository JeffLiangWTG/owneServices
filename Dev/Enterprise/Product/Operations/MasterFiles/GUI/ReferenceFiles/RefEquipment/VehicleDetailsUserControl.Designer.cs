namespace Enterprise.MasterFiles.GUI
{
	partial class VehicleDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.VehicleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TranspondersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RQ_GateTransponder1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQ_GateTransponder3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQ_GateTransponder2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NHVAStextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Truckcheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RQ_DisposalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RQ_PurchaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RQ_VINTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RQ_TollPassBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQ_2WayInfoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQ_IsVehicleBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VehicleGroupBox.SuspendLayout();
			this.TranspondersGroupBox.SuspendLayout();
			this.RQ_DisposalDateDateEdit.SuspendLayout();
			this.RQ_PurchaseDateDateEdit.SuspendLayout();
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefEquipment);
			// 
			// VehicleGroupBox
			// 
			this.VehicleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("82576537-4336-46ba-9dc1-b4e59b8b46b1", "Vehicle Details");
			this.VehicleGroupBox.Controls.Add(this.TranspondersGroupBox);
			this.VehicleGroupBox.Controls.Add(this.NHVAStextbox);
			this.VehicleGroupBox.Controls.Add(this.Truckcheckbox);
			this.VehicleGroupBox.Controls.Add(this.RQ_DisposalDateDateEdit);
			this.VehicleGroupBox.Controls.Add(this.RQ_PurchaseDateDateEdit);
			this.VehicleGroupBox.Controls.Add(this.RQ_VINTextBox);
			this.VehicleGroupBox.Controls.Add(this.RQ_GS_NKPreferredDriverBoundCodeFindBox);
			this.VehicleGroupBox.Controls.Add(this.RQ_TollPassBoundTextBox);
			this.VehicleGroupBox.Controls.Add(this.RQ_2WayInfoBoundTextBox);
			this.VehicleGroupBox.Controls.Add(this.RQ_IsVehicleBoundCheckEdit);
			this.VehicleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehicleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehicleGroupBox.Name = "VehicleGroupBox";
			this.VehicleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 380, true);
			this.VehicleGroupBox.TabIndex = 0;
			this.VehicleGroupBox.TabStop = false;
			// 
			// TranspondersGroupBox
			// 
			this.TranspondersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TranspondersGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12d58841-829d-4850-bd28-fc4a9e56f843", "Transponders");
			this.TranspondersGroupBox.Controls.Add(this.RQ_GateTransponder1BoundTextBox);
			this.TranspondersGroupBox.Controls.Add(this.RQ_GateTransponder3BoundTextBox);
			this.TranspondersGroupBox.Controls.Add(this.RQ_GateTransponder2BoundTextBox);
			this.TranspondersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 216, true);
			this.TranspondersGroupBox.Name = "TranspondersGroupBox";
			this.TranspondersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 92, true);
			this.TranspondersGroupBox.TabIndex = 9;
			this.TranspondersGroupBox.TabStop = false;
			// 
			// RQ_GateTransponder1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_GateTransponder1BoundTextBox, "RQ_GateTransponder1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_GateTransponder1)));
			this.RQ_GateTransponder1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 15, true);
			this.RQ_GateTransponder1BoundTextBox.Name = "RQ_GateTransponder1BoundTextBox";
			this.RQ_GateTransponder1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_GateTransponder1BoundTextBox.TabIndex = 0;
			// 
			// RQ_GateTransponder3BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_GateTransponder3BoundTextBox, "RQ_GateTransponder3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_GateTransponder3)));
			this.RQ_GateTransponder3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 67, true);
			this.RQ_GateTransponder3BoundTextBox.Name = "RQ_GateTransponder3BoundTextBox";
			this.RQ_GateTransponder3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_GateTransponder3BoundTextBox.TabIndex = 2;
			// 
			// RQ_GateTransponder2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_GateTransponder2BoundTextBox, "RQ_GateTransponder2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_GateTransponder2)));
			this.RQ_GateTransponder2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 41, true);
			this.RQ_GateTransponder2BoundTextBox.Name = "RQ_GateTransponder2BoundTextBox";
			this.RQ_GateTransponder2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_GateTransponder2BoundTextBox.TabIndex = 1;
			// 
			// NHVAStextbox
			// 
			this.BindingSource.SetBindingMember(this.NHVAStextbox, "RQ_AddReference1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_AddReference1)));
			this.NHVAStextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 195, true);
			this.NHVAStextbox.Name = "NHVAStextbox";
			this.NHVAStextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
			this.NHVAStextbox.TabIndex = 7;
			// 
			// Truckcheckbox
			// 
			this.BindingSource.SetBindingMember(this.Truckcheckbox, "RQ_AddFlag1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_AddFlag1)));
			this.Truckcheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.Truckcheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Truckcheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 195, true);
			this.Truckcheckbox.Name = "Truckcheckbox";
			this.Truckcheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.Truckcheckbox.TabIndex = 8;
			// 
			// RQ_DisposalDateDateEdit
			// 
			this.RQ_DisposalDateDateEdit.AllowDrop = true;
			this.RQ_DisposalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.RQ_DisposalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RQ_DisposalDateDateEdit, "RQ_DisposalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_DisposalDate)));
			this.RQ_DisposalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 91, true);
			this.RQ_DisposalDateDateEdit.Name = "RQ_DisposalDateDateEdit";
			this.RQ_DisposalDateDateEdit.TabIndex = 3;
			// 
			// RQ_PurchaseDateDateEdit
			// 
			this.RQ_PurchaseDateDateEdit.AllowDrop = true;
			this.RQ_PurchaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.RQ_PurchaseDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RQ_PurchaseDateDateEdit, "RQ_PurchaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_PurchaseDate)));
			this.RQ_PurchaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 65, true);
			this.RQ_PurchaseDateDateEdit.Name = "RQ_PurchaseDateDateEdit";
			this.RQ_PurchaseDateDateEdit.TabIndex = 2;
			// 
			// RQ_VINTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_VINTextBox, "RQ_VIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_VIN)));
			this.RQ_VINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 39, true);
			this.RQ_VINTextBox.Name = "RQ_VINTextBox";
			this.RQ_VINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_VINTextBox.TabIndex = 1;
			// 
			// RQ_GS_NKPreferredDriverBoundCodeFindBox
			// 
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.AllowDrop = true;
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RQ_GS_NKPreferredDriverBoundCodeFindBox, "RQ_GS_NKPreferredDriver");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_GS_NKPreferredDriver)));
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 169, true);
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.Name = "RQ_GS_NKPreferredDriverBoundCodeFindBox";
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.PopupCaption = "Select Staff";
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.PreBoundMaxLength = 3;
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.TabIndex = 6;
			// 
			// RQ_TollPassBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_TollPassBoundTextBox, "RQ_TollPass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_TollPass)));
			this.RQ_TollPassBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 143, true);
			this.RQ_TollPassBoundTextBox.Name = "RQ_TollPassBoundTextBox";
			this.RQ_TollPassBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_TollPassBoundTextBox.TabIndex = 5;
			// 
			// RQ_2WayInfoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQ_2WayInfoBoundTextBox, "RQ_2WayInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_2WayInfo)));
			this.RQ_2WayInfoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 117, true);
			this.RQ_2WayInfoBoundTextBox.Name = "RQ_2WayInfoBoundTextBox";
			this.RQ_2WayInfoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 18, true);
			this.RQ_2WayInfoBoundTextBox.TabIndex = 4;
			// 
			// RQ_IsVehicleBoundCheckEdit
			// 
			this.RQ_IsVehicleBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RQ_IsVehicleBoundCheckEdit, "RQ_IsVehicle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_IsVehicle)));
			this.RQ_IsVehicleBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RQ_IsVehicleBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RQ_IsVehicleBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 19, true);
			this.RQ_IsVehicleBoundCheckEdit.Name = "RQ_IsVehicleBoundCheckEdit";
			this.RQ_IsVehicleBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.RQ_IsVehicleBoundCheckEdit.TabIndex = 0;
			// 
			// VehicleDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehicleGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 380, true);
			this.Name = "VehicleDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VehicleGroupBox.ResumeLayout(false);
			this.VehicleGroupBox.PerformLayout();
			this.TranspondersGroupBox.ResumeLayout(false);
			this.TranspondersGroupBox.PerformLayout();
			this.RQ_DisposalDateDateEdit.ResumeLayout(true);
			this.RQ_DisposalDateDateEdit.PerformLayout();
			this.RQ_PurchaseDateDateEdit.ResumeLayout(true);
			this.RQ_PurchaseDateDateEdit.PerformLayout();
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.ResumeLayout(true);
			this.RQ_GS_NKPreferredDriverBoundCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox VehicleGroupBox;
		internal ZArchitecture.ZTextBox NHVAStextbox;
		private ZArchitecture.GUI.ZCheckBox Truckcheckbox;
		private ZArchitecture.GUI.ZDateEdit RQ_DisposalDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit RQ_PurchaseDateDateEdit;
		private ZArchitecture.ZTextBox RQ_VINTextBox;
		private ZArchitecture.GUI.ZCodeFindBox RQ_GS_NKPreferredDriverBoundCodeFindBox;
		private ZArchitecture.ZTextBox RQ_TollPassBoundTextBox;
		private ZArchitecture.ZTextBox RQ_2WayInfoBoundTextBox;
		private ZArchitecture.ZTextBox RQ_GateTransponder2BoundTextBox;
		private ZArchitecture.ZTextBox RQ_GateTransponder1BoundTextBox;
		private ZArchitecture.ZTextBox RQ_GateTransponder3BoundTextBox;
		private ZArchitecture.GUI.ZCheckBox RQ_IsVehicleBoundCheckEdit;
		private ZArchitecture.GUI.ZGroupBox TranspondersGroupBox;
	}
}
