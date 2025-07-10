using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class NOManifestUserControl
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
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NOManifestUserControl));
			this.RepresentativeAddressControl = new ZArchitecture.GUI.ZAddressControl();
			this.DriverCommunicationIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DriverNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ScheduledDateOfArrCustOfficeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportMeansCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VehicleRegistrationAndNationalityUserControl = new VehicleRegistrationAndNationalityUserControl();
			this.MasterBillGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillDynamicLayoutPanel = new ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RepresentativeAddressControl.SuspendLayout();
			this.ScheduledDateOfArrCustOfficeDateEdit.SuspendLayout();
			this.TransportMeansCodeFindBox.SuspendLayout();
			this.VehicleRegistrationAndNationalityUserControl.SuspendLayout();
			this.MasterBillGroupBox.SuspendLayout();
			this.MasterBillDynamicLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader);
			// 
			// RepresentativeAddressControl
			//
			this.RepresentativeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "MasterBill.ABL_OA_Forwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).ABL_OA_Forwarder)));
			this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
			this.RepresentativeAddressControl.CaptionResourceString = Enterprise.Customs.NO.Manifest.GUI.Res.GetData("569CB963-B307-48B2-4A10-EEEAA4B71C2C", "Representative", "Customs representative on behalf of carrier (operator/driver).");
			this.RepresentativeAddressControl.BindToOrgList = "Lookups.Organisations";
			this.RepresentativeAddressControl.ShowAddress = false;
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 160, true);
			this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.RepresentativeAddressControl.TabIndex = 0;
			// 
			// DriverCommunicationIdTextBox
			//
			this.BindingSource.SetBindingMember(this.DriverCommunicationIdTextBox, "AMA_DriverCommunicationId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null)).AMA_DriverCommunicationId)));
			this.DriverCommunicationIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 27, true);
			this.DriverCommunicationIdTextBox.Name = "DriverCommunicationIdTextBox";
			this.DriverCommunicationIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DriverCommunicationIdTextBox.TabIndex = 1;
			// 
			// DriverNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DriverNameTextBox, "AMA_DriverName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null)).AMA_DriverName)));
			this.DriverNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 53, true);
			this.DriverNameTextBox.Name = "DriverNameTextBox";
			this.DriverNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DriverNameTextBox.TabIndex = 2;
			// 
			// ScheduledDateOfArrCustOfficeDateEdit
			//
			this.BindingSource.SetBindingMember(this.ScheduledDateOfArrCustOfficeDateEdit, "AMA_ScheduledDateOfAddCustOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null)).AMA_ScheduledDateOfAddCustOff)));
			this.ScheduledDateOfArrCustOfficeDateEdit.AllowDrop = true;
			this.ScheduledDateOfArrCustOfficeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledDateOfArrCustOfficeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 80, true);
			this.ScheduledDateOfArrCustOfficeDateEdit.Name = "ScheduledDateOfArrCustOfficeDateEdit";
			this.ScheduledDateOfArrCustOfficeDateEdit.TabIndex = 3;
			// 
			// TransportMeansCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.TransportMeansCodeFindBox, "AMA_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null)).AMA_TransportMeans)));
			this.TransportMeansCodeFindBox.AllowDrop = true;
			this.TransportMeansCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 107, true);
			this.TransportMeansCodeFindBox.Name = "TransportMeansCodeFindBox";
			this.TransportMeansCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportMeansCodeFindBox.TabIndex = 4;
			// 
			// VehicleRegistrationAndNationalityUserControl
			//
			this.BindingSource.SetBindingMember(this.VehicleRegistrationAndNationalityUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null)))));
			this.VehicleRegistrationAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 134, true);
			this.VehicleRegistrationAndNationalityUserControl.Name = "VehicleRegistrationAndNationalityUserControl";
			this.VehicleRegistrationAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VehicleRegistrationAndNationalityUserControl.TabIndex = 5;
			// 
			// MasterBillGroupBox
			// 
			this.MasterBillGroupBox.CaptionResourceString = Enterprise.Customs.NO.Manifest.GUI.Res.GetData("AC004627-A6C1-422C-9F30-11684B8CF2F0", "Master Bill");
			this.MasterBillGroupBox.Controls.Add(this.MasterBillDynamicLayoutPanel);
			this.MasterBillGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 206, true);
			this.MasterBillGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 550, true);
			this.MasterBillGroupBox.Name = "MasterBillGroupBox";
			this.MasterBillGroupBox.TabIndex = 1;
			this.MasterBillGroupBox.TabStop = false;
			this.MasterBillGroupBox.MaximumSize = new System.Drawing.Size(600, 550);
			//
			// MasterBillDynamicLayoutPanel
			//
			this.BindingSource.SetBindingMember(this.MasterBillDynamicLayoutPanel, "MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(((Enterprise.Customs.NO.Manifest.Business.AsycudaManifestHeader)(null))).MasterBill));
			this.MasterBillDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 25, true);
			this.MasterBillDynamicLayoutPanel.Name = "MasterBillDynamicLayoutPanel";
			this.MasterBillDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 413, true);
			this.MasterBillDynamicLayoutPanel.AllowDrop = true;
			this.MasterBillDynamicLayoutPanel.AutoScroll = true;
			this.MasterBillDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterBillDynamicLayoutPanel.TabIndex = 2;
			// 
			// NOManifestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MasterBillGroupBox);
			this.Controls.Add(this.VehicleRegistrationAndNationalityUserControl);
			this.Controls.Add(this.TransportMeansCodeFindBox);
			this.Controls.Add(this.ScheduledDateOfArrCustOfficeDateEdit);
			this.Controls.Add(this.DriverNameTextBox);
			this.Controls.Add(this.DriverCommunicationIdTextBox);
			this.Controls.Add(this.RepresentativeAddressControl);
			this.Name = "NOManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 660, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.ScheduledDateOfArrCustOfficeDateEdit.ResumeLayout(true);
			this.ScheduledDateOfArrCustOfficeDateEdit.PerformLayout();
			this.TransportMeansCodeFindBox.ResumeLayout(true);
			this.TransportMeansCodeFindBox.PerformLayout();
			this.VehicleRegistrationAndNationalityUserControl.ResumeLayout(true);
			this.VehicleRegistrationAndNationalityUserControl.PerformLayout();
			this.MasterBillGroupBox.ResumeLayout(true);
			this.MasterBillGroupBox.PerformLayout();
			this.MasterBillDynamicLayoutPanel.ResumeLayout(true);
			this.MasterBillDynamicLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		internal ZArchitecture.ZTextBox DriverCommunicationIdTextBox;
		internal ZArchitecture.ZTextBox DriverNameTextBox;
		internal ZArchitecture.GUI.ZDateEdit ScheduledDateOfArrCustOfficeDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox TransportMeansCodeFindBox;
		internal VehicleRegistrationAndNationalityUserControl VehicleRegistrationAndNationalityUserControl;
		internal ZArchitecture.GUI.ZGroupBox MasterBillGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel MasterBillDynamicLayoutPanel;
	}
}
