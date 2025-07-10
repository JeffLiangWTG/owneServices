namespace Enterprise.MasterFiles.GUI
{
	partial class RefShippingLineIntegrationsControl
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
			this.RSL_OceanCarrierMessagingAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_GlobalSailingScheduleAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_ContainerAutomationAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_InvoiceAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_BookingRequestAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_ShippingInstructionAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_ShippingOrderAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_EManifestAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefShippingLine);
			// 
			// RSL_OceanCarrierMessagingAvailableCheckBox
			// 
			this.RSL_OceanCarrierMessagingAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_OceanCarrierMessagingAvailableCheckBox, "RSL_OceanCarrierMessagingAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_OceanCarrierMessagingAvailable)));
			this.RSL_OceanCarrierMessagingAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_OceanCarrierMessagingAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RSL_OceanCarrierMessagingAvailableCheckBox.Name = "RSL_OceanCarrierMessagingAvailableCheckBox";
			this.RSL_OceanCarrierMessagingAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.RSL_OceanCarrierMessagingAvailableCheckBox.TabIndex = 1;
			// 
			// RSL_GlobalSailingScheduleAvailableCheckBox
			// 
			this.RSL_GlobalSailingScheduleAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_GlobalSailingScheduleAvailableCheckBox, "RSL_GlobalSailingScheduleAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_GlobalSailingScheduleAvailable)));
			this.RSL_GlobalSailingScheduleAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_GlobalSailingScheduleAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 123, true);
			this.RSL_GlobalSailingScheduleAvailableCheckBox.Name = "RSL_GlobalSailingScheduleAvailableCheckBox";
			this.RSL_GlobalSailingScheduleAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.RSL_GlobalSailingScheduleAvailableCheckBox.TabIndex = 7;
			// 
			// RSL_ContainerAutomationAvailableCheckBox
			// 
			this.RSL_ContainerAutomationAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_ContainerAutomationAvailableCheckBox, "RSL_ContainerAutomationAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_ContainerAutomationAvailable)));
			this.RSL_ContainerAutomationAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_ContainerAutomationAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 143, true);
			this.RSL_ContainerAutomationAvailableCheckBox.Name = "RSL_ContainerAutomationAvailableCheckBox";
			this.RSL_ContainerAutomationAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.RSL_ContainerAutomationAvailableCheckBox.TabIndex = 8;
			// 
			// RSL_InvoiceAvailableCheckBox
			// 
			this.RSL_InvoiceAvailableCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.RSL_InvoiceAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_InvoiceAvailableCheckBox, "RSL_InvoiceAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_InvoiceAvailable)));
			this.RSL_InvoiceAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_InvoiceAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 163, true);
			this.RSL_InvoiceAvailableCheckBox.Name = "RSL_InvoiceAvailableCheckBox";
			this.RSL_InvoiceAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.RSL_InvoiceAvailableCheckBox.TabIndex = 9;
			// 
			// RSL_BookingRequestAvailableCheckBox
			// 
			this.RSL_BookingRequestAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_BookingRequestAvailableCheckBox, "RSL_BookingRequestAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_BookingRequestAvailable)));
			this.RSL_BookingRequestAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_BookingRequestAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 23, true);
			this.RSL_BookingRequestAvailableCheckBox.Name = "RSL_BookingRequestAvailableCheckBox";
			this.RSL_BookingRequestAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.RSL_BookingRequestAvailableCheckBox.TabIndex = 2;
			// 
			// RSL_ShippingInstructionAvailableCheckBox
			// 
			this.RSL_ShippingInstructionAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_ShippingInstructionAvailableCheckBox, "RSL_ShippingInstructionAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_ShippingInstructionAvailable)));
			this.RSL_ShippingInstructionAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_ShippingInstructionAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 43, true);
			this.RSL_ShippingInstructionAvailableCheckBox.Name = "RSL_ShippingInstructionAvailableCheckBox";
			this.RSL_ShippingInstructionAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.RSL_ShippingInstructionAvailableCheckBox.TabIndex = 3;
			// 
			// RSL_VerifiedGrossContainerWeightAvailableCheckBox
			// 
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_VerifiedGrossContainerWeightAvailableCheckBox, "RSL_VerifiedGrossContainerWeightAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_VerifiedGrossContainerWeightAvailable)));
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 63, true);
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.Name = "RSL_VerifiedGrossContainerWeightAvailableCheckBox";
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 17, true);
			this.RSL_VerifiedGrossContainerWeightAvailableCheckBox.TabIndex = 4;
			// 
			// RSL_ShippingOrderAvailableCheckBox
			// 
			this.RSL_ShippingOrderAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_ShippingOrderAvailableCheckBox, "RSL_ShippingOrderAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_ShippingOrderAvailable)));
			this.RSL_ShippingOrderAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_ShippingOrderAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 83, true);
			this.RSL_ShippingOrderAvailableCheckBox.Name = "RSL_ShippingOrderAvailableCheckBox";
			this.RSL_ShippingOrderAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.RSL_ShippingOrderAvailableCheckBox.TabIndex = 5;
			// 
			// RSL_EManifestAvailableCheckBox
			// 
			this.RSL_EManifestAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_EManifestAvailableCheckBox, "RSL_EManifestAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_EManifestAvailable)));
			this.RSL_EManifestAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_EManifestAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 103, true);
			this.RSL_EManifestAvailableCheckBox.Name = "RSL_EManifestAvailableCheckBox";
			this.RSL_EManifestAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.RSL_EManifestAvailableCheckBox.TabIndex = 6;
			// 
			// RefShippingLineIntegrationsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RSL_EManifestAvailableCheckBox);
			this.Controls.Add(this.RSL_ShippingOrderAvailableCheckBox);
			this.Controls.Add(this.RSL_VerifiedGrossContainerWeightAvailableCheckBox);
			this.Controls.Add(this.RSL_ShippingInstructionAvailableCheckBox);
			this.Controls.Add(this.RSL_BookingRequestAvailableCheckBox);
			this.Controls.Add(this.RSL_InvoiceAvailableCheckBox);
			this.Controls.Add(this.RSL_ContainerAutomationAvailableCheckBox);
			this.Controls.Add(this.RSL_GlobalSailingScheduleAvailableCheckBox);
			this.Controls.Add(this.RSL_OceanCarrierMessagingAvailableCheckBox);
			this.Name = "RefShippingLineIntegrationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 181, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox RSL_OceanCarrierMessagingAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_GlobalSailingScheduleAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_ContainerAutomationAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_InvoiceAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_BookingRequestAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_ShippingInstructionAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_VerifiedGrossContainerWeightAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_ShippingOrderAvailableCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_EManifestAvailableCheckBox;
	}
}
