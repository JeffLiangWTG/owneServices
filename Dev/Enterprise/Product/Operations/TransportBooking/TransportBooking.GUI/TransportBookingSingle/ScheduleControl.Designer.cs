namespace Enterprise.TransportBookings.GUI
{
	partial class ScheduleControl
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
			this.SailingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JJ_JA_NKPortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JJ_JB_NKPortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JJ_JV_NKVesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JJ_JB_E_ARVDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JA_E_DEPDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JX_VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportDatesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JJ_JX_LCLAvailabilityDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_LCLStorageDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_AvailabilityDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_StorageDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportDatesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JJ_JX_LCLReceivalCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_LCLCutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_FCLReceivalCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JJ_JX_FCLCutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingDetailsGroupBox.SuspendLayout();
			this.ImportDatesPanel.SuspendLayout();
			this.ExportDatesPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			// 
			// SailingDetailsGroupBox
			// 
			this.SailingDetailsGroupBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("f507d22f-b5f6-4065-9f80-588b6b047e0a", "Connecting Schedule");
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JA_NKPortOfLoadingFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JB_NKPortOfDischargeFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JV_NKVesselFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JB_E_ARVDateEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.JJ_JA_E_DEPDateEdit);
			this.SailingDetailsGroupBox.Controls.Add(this.JX_VoyageTextBox);
			this.SailingDetailsGroupBox.Controls.Add(this.ImportDatesPanel);
			this.SailingDetailsGroupBox.Controls.Add(this.ExportDatesPanel);
			this.SailingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SailingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingDetailsGroupBox.Name = "SailingDetailsGroupBox";
			this.SailingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 139, true);
			this.SailingDetailsGroupBox.TabIndex = 12;
			this.SailingDetailsGroupBox.TabStop = false;
			// 
			// JJ_JA_NKPortOfLoadingFindBox
			// 
			this.JJ_JA_NKPortOfLoadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JA_NKPortOfLoadingFindBox, "Schedule.VL_RL_NKLoad");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_RL_NKLoad)));
			this.JJ_JA_NKPortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 42, true);
			this.JJ_JA_NKPortOfLoadingFindBox.Name = "JJ_JA_NKPortOfLoadingFindBox";
			this.JJ_JA_NKPortOfLoadingFindBox.PopupCaption = null;
			this.JJ_JA_NKPortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.JJ_JA_NKPortOfLoadingFindBox.ShowDescriptionBox = false;
			this.JJ_JA_NKPortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JJ_JA_NKPortOfLoadingFindBox.TabIndex = 3;
			// 
			// JJ_JB_NKPortOfDischargeFindBox
			// 
			this.JJ_JB_NKPortOfDischargeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JB_NKPortOfDischargeFindBox, "Schedule.VL_RL_NKDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_RL_NKDischarge)));
			this.JJ_JB_NKPortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 65, true);
			this.JJ_JB_NKPortOfDischargeFindBox.Name = "JJ_JB_NKPortOfDischargeFindBox";
			this.JJ_JB_NKPortOfDischargeFindBox.PopupCaption = null;
			this.JJ_JB_NKPortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.JJ_JB_NKPortOfDischargeFindBox.ShowDescriptionBox = false;
			this.JJ_JB_NKPortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JJ_JB_NKPortOfDischargeFindBox.TabIndex = 5;
			// 
			// JJ_JV_NKVesselFindBox
			// 
			this.JJ_JV_NKVesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JJ_JV_NKVesselFindBox, "Schedule.VL_Vessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_Vessel)));
			this.JJ_JV_NKVesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 18, true);
			this.JJ_JV_NKVesselFindBox.Name = "JJ_JV_NKVesselFindBox";
			this.JJ_JV_NKVesselFindBox.PopupCaption = null;
			this.JJ_JV_NKVesselFindBox.PreBoundMaxLength = 35;
			this.JJ_JV_NKVesselFindBox.ShowDescriptionBox = false;
			this.JJ_JV_NKVesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 21, true);
			this.JJ_JV_NKVesselFindBox.TabIndex = 0;
			// 
			// JJ_JB_E_ARVDateEdit
			// 
			this.JJ_JB_E_ARVDateEdit.AllowDrop = true;
			this.JJ_JB_E_ARVDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JB_E_ARVDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JB_E_ARVDateEdit, "Schedule.VL_ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_ETA)));
			this.JJ_JB_E_ARVDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 64, true);
			this.JJ_JB_E_ARVDateEdit.Name = "JJ_JB_E_ARVDateEdit";
			this.JJ_JB_E_ARVDateEdit.TabIndex = 6;
			// 
			// JJ_JA_E_DEPDateEdit
			// 
			this.JJ_JA_E_DEPDateEdit.AllowDrop = true;
			this.JJ_JA_E_DEPDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JA_E_DEPDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JA_E_DEPDateEdit, "Schedule.VL_ETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_ETD)));
			this.JJ_JA_E_DEPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 41, true);
			this.JJ_JA_E_DEPDateEdit.Name = "JJ_JA_E_DEPDateEdit";
			this.JJ_JA_E_DEPDateEdit.TabIndex = 4;
			// 
			// JX_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JX_VoyageTextBox, "Schedule.VL_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_VoyageFlight)));
			this.JX_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 19, true);
			this.JX_VoyageTextBox.Name = "JX_VoyageTextBox";
			this.JX_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JX_VoyageTextBox.TabIndex = 1;
			// 
			// ImportDatesPanel
			// 
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_LCLAvailabilityDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_LCLStorageDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_AvailabilityDateDateEdit);
			this.ImportDatesPanel.Controls.Add(this.JJ_JX_StorageDateDateEdit);
			this.ImportDatesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 88, true);
			this.ImportDatesPanel.Name = "ImportDatesPanel";
			this.ImportDatesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 43, true);
			this.ImportDatesPanel.TabIndex = 15;
			// 
			// JJ_JX_LCLAvailabilityDateDateEdit
			// 
			this.JJ_JX_LCLAvailabilityDateDateEdit.AllowDrop = true;
			this.JJ_JX_LCLAvailabilityDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLAvailabilityDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLAvailabilityDateDateEdit, "Schedule.VL_LCLAvailabilityDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_LCLAvailabilityDate)));
			this.JJ_JX_LCLAvailabilityDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLAvailabilityDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 22, true);
			this.JJ_JX_LCLAvailabilityDateDateEdit.Name = "JJ_JX_LCLAvailabilityDateDateEdit";
			this.JJ_JX_LCLAvailabilityDateDateEdit.TabIndex = 5;
			// 
			// JJ_JX_LCLStorageDateDateEdit
			// 
			this.JJ_JX_LCLStorageDateDateEdit.AllowDrop = true;
			this.JJ_JX_LCLStorageDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLStorageDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLStorageDateDateEdit, "Schedule.VL_LCLStorageDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_LCLStorageDate)));
			this.JJ_JX_LCLStorageDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLStorageDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 22, true);
			this.JJ_JX_LCLStorageDateDateEdit.Name = "JJ_JX_LCLStorageDateDateEdit";
			this.JJ_JX_LCLStorageDateDateEdit.TabIndex = 7;
			// 
			// JJ_JX_AvailabilityDateDateEdit
			// 
			this.JJ_JX_AvailabilityDateDateEdit.AllowDrop = true;
			this.JJ_JX_AvailabilityDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_AvailabilityDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_AvailabilityDateDateEdit, "Schedule.VL_FCLAvailabilityDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_FCLAvailabilityDate)));
			this.JJ_JX_AvailabilityDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_AvailabilityDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
			this.JJ_JX_AvailabilityDateDateEdit.Name = "JJ_JX_AvailabilityDateDateEdit";
			this.JJ_JX_AvailabilityDateDateEdit.TabIndex = 1;
			// 
			// JJ_JX_StorageDateDateEdit
			// 
			this.JJ_JX_StorageDateDateEdit.AllowDrop = true;
			this.JJ_JX_StorageDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_StorageDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_StorageDateDateEdit, "Schedule.VL_FCLStorageDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_FCLStorageDate)));
			this.JJ_JX_StorageDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_StorageDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 0, true);
			this.JJ_JX_StorageDateDateEdit.Name = "JJ_JX_StorageDateDateEdit";
			this.JJ_JX_StorageDateDateEdit.TabIndex = 3;
			// 
			// ExportDatesPanel
			// 
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_LCLReceivalCommencesDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_LCLCutOffDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_FCLReceivalCommencesDateEdit);
			this.ExportDatesPanel.Controls.Add(this.JJ_JX_FCLCutOffDateEdit);
			this.ExportDatesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 89, true);
			this.ExportDatesPanel.Name = "ExportDatesPanel";
			this.ExportDatesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 43, true);
			this.ExportDatesPanel.TabIndex = 13;
			// 
			// JJ_JX_LCLReceivalCommencesDateEdit
			// 
			this.JJ_JX_LCLReceivalCommencesDateEdit.AllowDrop = true;
			this.JJ_JX_LCLReceivalCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLReceivalCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLReceivalCommencesDateEdit, "Schedule.VL_LCLReceivalCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_LCLReceivalCommences)));
			this.JJ_JX_LCLReceivalCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLReceivalCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 22, true);
			this.JJ_JX_LCLReceivalCommencesDateEdit.Name = "JJ_JX_LCLReceivalCommencesDateEdit";
			this.JJ_JX_LCLReceivalCommencesDateEdit.TabIndex = 2;
			// 
			// JJ_JX_LCLCutOffDateEdit
			// 
			this.JJ_JX_LCLCutOffDateEdit.AllowDrop = true;
			this.JJ_JX_LCLCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_LCLCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_LCLCutOffDateEdit, "Schedule.VL_LCLCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_LCLCutOff)));
			this.JJ_JX_LCLCutOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_LCLCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 22, true);
			this.JJ_JX_LCLCutOffDateEdit.Name = "JJ_JX_LCLCutOffDateEdit";
			this.JJ_JX_LCLCutOffDateEdit.TabIndex = 3;
			// 
			// JJ_JX_FCLReceivalCommencesDateEdit
			// 
			this.JJ_JX_FCLReceivalCommencesDateEdit.AllowDrop = true;
			this.JJ_JX_FCLReceivalCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_FCLReceivalCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_FCLReceivalCommencesDateEdit, "Schedule.VL_FCLReceivalCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_FCLReceivalCommences)));
			this.JJ_JX_FCLReceivalCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_FCLReceivalCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
			this.JJ_JX_FCLReceivalCommencesDateEdit.Name = "JJ_JX_FCLReceivalCommencesDateEdit";
			this.JJ_JX_FCLReceivalCommencesDateEdit.TabIndex = 0;
			// 
			// JJ_JX_FCLCutOffDateEdit
			// 
			this.JJ_JX_FCLCutOffDateEdit.AllowDrop = true;
			this.JJ_JX_FCLCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JJ_JX_FCLCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JJ_JX_FCLCutOffDateEdit, "Schedule.VL_FCLCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Schedule.VL_FCLCutOff)));
			this.JJ_JX_FCLCutOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JJ_JX_FCLCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 0, true);
			this.JJ_JX_FCLCutOffDateEdit.Name = "JJ_JX_FCLCutOffDateEdit";
			this.JJ_JX_FCLCutOffDateEdit.TabIndex = 1;
			// 
			// ScheduleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingDetailsGroupBox);
			this.Name = "ScheduleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingDetailsGroupBox.ResumeLayout(false);
			this.SailingDetailsGroupBox.PerformLayout();
			this.ImportDatesPanel.ResumeLayout(false);
			this.ExportDatesPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SailingDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox JJ_JA_NKPortOfLoadingFindBox;
		private ZArchitecture.GUI.ZCodeFindBox JJ_JB_NKPortOfDischargeFindBox;
		private ZArchitecture.GUI.ZDateEdit JJ_JB_E_ARVDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JA_E_DEPDateEdit;
		private ZArchitecture.ZTextBox JX_VoyageTextBox;
		private ZArchitecture.GUI.ZPanel ImportDatesPanel;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_LCLAvailabilityDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_LCLStorageDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_AvailabilityDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_StorageDateDateEdit;
		private ZArchitecture.GUI.ZPanel ExportDatesPanel;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_LCLReceivalCommencesDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_LCLCutOffDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_FCLReceivalCommencesDateEdit;
		private ZArchitecture.GUI.ZDateEdit JJ_JX_FCLCutOffDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox JJ_JV_NKVesselFindBox;

	}
}
