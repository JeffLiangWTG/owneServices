namespace Enterprise.Customs.US.GUI
{
	partial class JobTransportOrgsControl
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
			this.ContainerYardDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CarrierOrganisationControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForwarderOrganisationControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CTODocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DepotDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerYardDocAddressControl.SuspendLayout();
			this.CarrierOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.CTODocAddressControl.SuspendLayout();
			this.DepotDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// ContainerYardDocAddressControl
			// 
			this.ContainerYardDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerYardDocAddressControl, "ContainerYardDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContainerYardDocAddress)));
			this.ContainerYardDocAddressControl.BindToOrganisations = "Lookups+ContainerYardCollection";
			this.ContainerYardDocAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("01ebdd6c-1ea8-4dbc-9488-d0ce56d459f9", "Container Yard");
			this.ContainerYardDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ContainerYardDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.ContainerYardDocAddressControl.Name = "ContainerYardDocAddressControl";
			this.ContainerYardDocAddressControl.ReadOnly = false;
			this.ContainerYardDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContainerYardDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContainerYardDocAddressControl.TabIndex = 4;
			this.ContainerYardDocAddressControl.ValidationJustForced = false;
			// 
			// CarrierOrganisationControl
			// 
			this.CarrierOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierOrganisationControl, "JE_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_ShippingLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.ShippingLineList)));
			this.CarrierOrganisationControl.BindToList = "Lookups.ShippingLineList";
			this.CarrierOrganisationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8cc1bcc0-e2b6-4b3e-b2e5-e416eb32ff7b", "Carrier");
			this.CarrierOrganisationControl.IsPrimaryKeyFromCodeRequired = false;
			this.CarrierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.CarrierOrganisationControl.Name = "CarrierOrganisationControl";
			this.CarrierOrganisationControl.ShouldResize = true;
			this.CarrierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.CarrierOrganisationControl.TabIndex = 0;
			// 
			// ForwarderOrganisationControl
			// 
			this.ForwarderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderOrganisationControl, "JE_OH_Forwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Forwarder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.ForwarderList)));
			this.ForwarderOrganisationControl.BindToList = "Lookups.ForwarderList";
			this.ForwarderOrganisationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("51bc25d9-e647-4c78-ae02-fb8e96ea3eda", "Forwarder");
			this.ForwarderOrganisationControl.IsPrimaryKeyFromCodeRequired = false;
			this.ForwarderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 24, true);
			this.ForwarderOrganisationControl.Name = "ForwarderOrganisationControl";
			this.ForwarderOrganisationControl.ShouldResize = true;
			this.ForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ForwarderOrganisationControl.TabIndex = 1;
			// 
			// CTODocAddressControl
			// 
			this.CTODocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CTODocAddressControl, "ContainerTerminalOperatorDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContainerTerminalOperatorDocAddress)));
			this.CTODocAddressControl.BindToOrganisations = "Lookups+ContainerTerminalOperatorCollection";
			this.CTODocAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9e4e5553-5557-4c86-a4a5-aa44e2dec6f3", "CTO", "Container Terminal Operator (CTO)", "");
			this.CTODocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CTODocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.CTODocAddressControl.Name = "CTODocAddressControl";
			this.CTODocAddressControl.ReadOnly = false;
			this.CTODocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.CTODocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.CTODocAddressControl.TabIndex = 2;
			this.CTODocAddressControl.ValidationJustForced = false;
			// 
			// DepotDocAddressControl
			// 
			this.DepotDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepotDocAddressControl, "DepotDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).DepotDocAddress)));
			this.DepotDocAddressControl.BindToOrganisations = "Lookups+DepotCollection";
			this.DepotDocAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fd752bcf-1da0-49df-a8ec-207d57909ecb", "Depot");
			this.DepotDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DepotDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.DepotDocAddressControl.Name = "DepotDocAddressControl";
			this.DepotDocAddressControl.ReadOnly = false;
			this.DepotDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DepotDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DepotDocAddressControl.TabIndex = 3;
			this.DepotDocAddressControl.ValidationJustForced = false;
			// 
			// JobTransportOrgsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ContainerYardDocAddressControl);
			this.Controls.Add(this.DepotDocAddressControl);
			this.Controls.Add(this.CTODocAddressControl);
			this.Controls.Add(this.ForwarderOrganisationControl);
			this.Controls.Add(this.CarrierOrganisationControl);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 112, true);
			this.Name = "JobTransportOrgsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 112, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerYardDocAddressControl.ResumeLayout(true);
			this.ContainerYardDocAddressControl.PerformLayout();
			this.CarrierOrganisationControl.ResumeLayout(true);
			this.CarrierOrganisationControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.CTODocAddressControl.ResumeLayout(true);
			this.CTODocAddressControl.PerformLayout();
			this.DepotDocAddressControl.ResumeLayout(true);
			this.DepotDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MasterFiles.GUI.ZDocAddressControl ContainerYardDocAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox CarrierOrganisationControl;
		private ZArchitecture.GUI.ZGuidFindBox ForwarderOrganisationControl;
		private MasterFiles.GUI.ZDocAddressControl CTODocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl DepotDocAddressControl;
	}
}
