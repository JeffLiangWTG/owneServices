namespace Enterprise.TransportBookings.GUI
{
	partial class DtbInstructionControl
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
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsAuthorisedToLeaveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ActualDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceivedByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReqFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReqToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DropModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EstimatedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DtbInstructionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingInstruction);
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).StatusDescription)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 383, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ReadOnly = true;
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.StatusTextBox.TabIndex = 10;
			// 
			// IsAuthorisedToLeave
			// 
			this.BindingSource.SetBindingMember(this.IsAuthorisedToLeaveCheckBox, "KN_IsAuthorisedToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).KN_IsAuthorisedToLeave)));			
			this.IsAuthorisedToLeaveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 410, true);
			this.IsAuthorisedToLeaveCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IsAuthorisedToLeaveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 16, true);
			this.IsAuthorisedToLeaveCheckBox.Name = "AuthorisedToLeave";
			this.IsAuthorisedToLeaveCheckBox.TabIndex = 11;
			// 
			// ActualDateEdit
			// 
			this.ActualDateEdit.AllowDrop = true;
			this.ActualDateEdit.AutoCompleteMonthThreshold = 1;
			this.ActualDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ActualDateEdit, "Actual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).Actual)));
			this.ActualDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActualDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 296, true);
			this.ActualDateEdit.Name = "ActualDateEdit";
			this.ActualDateEdit.TabIndex = 7;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "PackageCategoryDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).PackageCategoryDescription)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 186, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.zTextBox1.TabIndex = 2;
			// 
			// ReceivedByTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceivedByTextBox, "SignedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).SignedBy)));
			this.ReceivedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 361, true);
			this.ReceivedByTextBox.Name = "ReceivedByTextBox";
			this.ReceivedByTextBox.ReadOnly = true;
			this.ReceivedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.ReceivedByTextBox.TabIndex = 9;
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "KN_ServiceInstruction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).KN_ServiceInstruction)));
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 318, true);
			this.NotesTextBox.Multiline = true;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 41, true);
			this.NotesTextBox.TabIndex = 8;
			// 
			// ReqFromDateEdit
			// 
			this.ReqFromDateEdit.AllowDrop = true;
			this.ReqFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqFromDateEdit, "ReqFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).ReqFrom)));
			this.ReqFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 252, true);
			this.ReqFromDateEdit.Name = "ReqFromDateEdit";
			this.ReqFromDateEdit.TabIndex = 5;
			// 
			// ReqToDateEdit
			// 
			this.ReqToDateEdit.AllowDrop = true;
			this.ReqToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqToDateEdit, "ReqTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).ReqTo)));
			this.ReqToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 274, true);
			this.ReqToDateEdit.Name = "ReqToDateEdit";
			this.ReqToDateEdit.TabIndex = 6;
			// 
			// DropModeDropEdit
			// 
			this.DropModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "KN_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).KN_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 208, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.PreBoundMaxLength = 3;
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.DropModeDropEdit.TabIndex = 3;
			// 
			// EstimatedDateEdit
			// 
			this.EstimatedDateEdit.AllowDrop = true;
			this.EstimatedDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedDateEdit, "Estimated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).Estimated)));
			this.EstimatedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstimatedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 230, true);
			this.EstimatedDateEdit.Name = "EstimatedDateEdit";
			this.EstimatedDateEdit.TabIndex = 4;
			// 
			// DocAddressControl
			// 
			this.DocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocAddressControl, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(null)).Address)));
			this.DocAddressControl.BindToOrganisations = "Address.Lookups.OrgHeader_List";
			this.DocAddressControl.CaptionResourceString = null;
			this.DocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.DocAddressControl.Name = "DocAddressControl";
			this.DocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.DocAddressControl.TabIndex = 1;
			// 
			// DtbInstructionGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DtbInstructionGroupBox, false);
			this.DtbInstructionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 2, true);
			this.DtbInstructionGroupBox.Name = "DtbInstructionGroupBox";
			this.DtbInstructionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 425, true);
			this.DtbInstructionGroupBox.TabIndex = 50;
			this.DtbInstructionGroupBox.TabStop = false;
			// 
			// DtbInstructionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DtbInstructionGroupBox);
			this.Controls.Add(this.StatusTextBox);
			this.Controls.Add(this.IsAuthorisedToLeaveCheckBox);
			this.Controls.Add(this.ActualDateEdit);
			this.Controls.Add(this.DocAddressControl);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.EstimatedDateEdit);
			this.Controls.Add(this.ReceivedByTextBox);
			this.Controls.Add(this.DropModeDropEdit);
			this.Controls.Add(this.NotesTextBox);
			this.Controls.Add(this.ReqToDateEdit);
			this.Controls.Add(this.ReqFromDateEdit);
			this.Name = "DtbInstructionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 434, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit ReqFromDateEdit;
		private ZArchitecture.GUI.ZDateEdit ReqToDateEdit;
		private ZArchitecture.GUI.ZDropEdit DropModeDropEdit;
		private ZArchitecture.GUI.ZDateEdit EstimatedDateEdit;
		private MasterFiles.GUI.ZDocAddressControl DocAddressControl;
		private ZArchitecture.ZTextBox ReceivedByTextBox;
		private ZArchitecture.ZTextBox NotesTextBox;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.GUI.ZCheckBox IsAuthorisedToLeaveCheckBox;
		private ZArchitecture.GUI.ZDateEdit ActualDateEdit;
		public ZArchitecture.GUI.ZGroupBox DtbInstructionGroupBox;
	}
}
