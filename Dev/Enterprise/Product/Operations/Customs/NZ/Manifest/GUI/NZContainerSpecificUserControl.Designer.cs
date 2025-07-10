namespace Enterprise.Customs.NZ.Manifest.GUI
{
	partial class NZContainerSpecificUserControl
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
			this.MPIDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HasWoodPackingTreatmentCertCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWoodPackingTreatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWoodPackingUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WoodPackagingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsPackingMaterialsContaminatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsContainerCleanCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HasMPIContainerQDCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendMCDInformationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeliveryDestinationPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MPIDeclarationGroupBox.SuspendLayout();
			this.DeliveryDestinationPartyDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer);
			// 
			// MPIDeclarationGroupBox
			// 
			this.MPIDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("B75EFBA9-686D-45BD-A408-7294443EFF3F", "MPI Quarantine Declaration for FCL Containers");
			this.MPIDeclarationGroupBox.Controls.Add(this.HasWoodPackingTreatmentCertCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.IsWoodPackingTreatedCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.IsWoodPackingUsedCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.WoodPackagingLabel);
			this.MPIDeclarationGroupBox.Controls.Add(this.IsPackingMaterialsContaminatedCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.IsContainerCleanCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.HasMPIContainerQDCheckBox);
			this.MPIDeclarationGroupBox.Controls.Add(this.SendMCDInformationCheckBox);
			this.MPIDeclarationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MPIDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MPIDeclarationGroupBox.Name = "MPIDeclarationGroupBox";
			this.MPIDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1017, 46, true);
			this.MPIDeclarationGroupBox.TabIndex = 3;
			this.MPIDeclarationGroupBox.TabStop = false;
			// 
			// HasWoodPackingTreatmentCertCheckBox
			// 
			this.HasWoodPackingTreatmentCertCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasWoodPackingTreatmentCertCheckBox, "HasWoodPackingTreatmentCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).HasWoodPackingTreatmentCert)));
			this.HasWoodPackingTreatmentCertCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasWoodPackingTreatmentCertCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 23, true);
			this.HasWoodPackingTreatmentCertCheckBox.Name = "HasWoodPackingTreatmentCertCheckBox";
			this.HasWoodPackingTreatmentCertCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 17, true);
			this.HasWoodPackingTreatmentCertCheckBox.TabIndex = 8;
			this.HasWoodPackingTreatmentCertCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// IsWoodPackingTreatedCheckBox
			// 
			this.IsWoodPackingTreatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsWoodPackingTreatedCheckBox, "IsWoodPackingTreated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).IsWoodPackingTreated)));
			this.IsWoodPackingTreatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsWoodPackingTreatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 23, true);
			this.IsWoodPackingTreatedCheckBox.Name = "IsWoodPackingTreatedCheckBox";
			this.IsWoodPackingTreatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.IsWoodPackingTreatedCheckBox.TabIndex = 7;
			this.IsWoodPackingTreatedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// IsWoodPackingUsedCheckBox
			// 
			this.IsWoodPackingUsedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsWoodPackingUsedCheckBox, "IsWoodPackingUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).IsWoodPackingUsed)));
			this.IsWoodPackingUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsWoodPackingUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 23, true);
			this.IsWoodPackingUsedCheckBox.Name = "IsWoodPackingUsedCheckBox";
			this.IsWoodPackingUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 17, true);
			this.IsWoodPackingUsedCheckBox.TabIndex = 6;
			this.IsWoodPackingUsedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WoodPackagingLabel
			// 
			this.WoodPackagingLabel.AutoSize = true;
			this.WoodPackagingLabel.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("2F03C40D-3BCC-499E-95FD-BAF468579B29", "Wood Packaging:");
			this.WoodPackagingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WoodPackagingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 23, true);
			this.WoodPackagingLabel.Name = "WoodPackagingLabel";
			this.WoodPackagingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.WoodPackagingLabel.TabIndex = 5;
			// 
			// IsPackingMaterialsContaminatedCheckBox
			// 
			this.IsPackingMaterialsContaminatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPackingMaterialsContaminatedCheckBox, "IsPackingContaminated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).IsPackingContaminated)));
			this.IsPackingMaterialsContaminatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPackingMaterialsContaminatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 23, true);
			this.IsPackingMaterialsContaminatedCheckBox.Name = "IsPackingMaterialsContaminatedCheckBox";
			this.IsPackingMaterialsContaminatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.IsPackingMaterialsContaminatedCheckBox.TabIndex = 4;
			this.IsPackingMaterialsContaminatedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// IsContainerCleanCheckBox
			// 
			this.IsContainerCleanCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsContainerCleanCheckBox, "IsContainerClean");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).IsContainerClean)));
			this.IsContainerCleanCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContainerCleanCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 23, true);
			this.IsContainerCleanCheckBox.Name = "IsContainerCleanCheckBox";
			this.IsContainerCleanCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.IsContainerCleanCheckBox.TabIndex = 3;
			this.IsContainerCleanCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// HasMPIContainerQDCheckBox
			// 
			this.HasMPIContainerQDCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasMPIContainerQDCheckBox, "HasMPIQD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).HasMPIQD)));
			this.HasMPIContainerQDCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasMPIContainerQDCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 23, true);
			this.HasMPIContainerQDCheckBox.Name = "HasMPIContainerQDCheckBox";
			this.HasMPIContainerQDCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.HasMPIContainerQDCheckBox.TabIndex = 2;
			this.HasMPIContainerQDCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SendMCDInformationCheckBox
			// 
			this.SendMCDInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SendMCDInformationCheckBox, "SendMCDInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).SendMCDInformation)));
			this.SendMCDInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendMCDInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 1, true);
			this.SendMCDInformationCheckBox.Name = "SendMCDInformationCheckBox";
			this.SendMCDInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.SendMCDInformationCheckBox.TabIndex = 1;
			this.SendMCDInformationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DeliveryDestinationPartyDocAddressControl
			// 
			this.DeliveryDestinationPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDestinationPartyDocAddressControl, "DeliveryDestinationPartyDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer)(null)).DeliveryDestinationPartyDocAddress)));
			this.DeliveryDestinationPartyDocAddressControl.BindToOrganisations = "Containers.Lookups.DeliveryDestinationPartyOrganisations";
			this.DeliveryDestinationPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("EC4050E6-F16D-4058-B81B-2893ED47E651", "Delivery Destination Party", "State the Delivery Destination party that the goods will be delivered to if different to the Consignee");
			this.DeliveryDestinationPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideAndTabs;
			this.DeliveryDestinationPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.DeliveryDestinationPartyDocAddressControl.Name = "DeliveryDestinationPartyDocAddressControl";
			this.DeliveryDestinationPartyDocAddressControl.ReadOnly = false;
			this.DeliveryDestinationPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDestinationPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 68, true);
			this.DeliveryDestinationPartyDocAddressControl.TabIndex = 12;
			this.DeliveryDestinationPartyDocAddressControl.ValidationJustForced = false;
			// 
			// NZContainerSpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MPIDeclarationGroupBox);
			this.Controls.Add(this.DeliveryDestinationPartyDocAddressControl);
			this.Name = "NZContainerSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1017, 123, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MPIDeclarationGroupBox.ResumeLayout(false);
			this.MPIDeclarationGroupBox.PerformLayout();
			this.DeliveryDestinationPartyDocAddressControl.ResumeLayout(true);
			this.DeliveryDestinationPartyDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MPIDeclarationGroupBox;
		private ZArchitecture.GUI.ZCheckBox HasWoodPackingTreatmentCertCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsWoodPackingTreatedCheckBox;
		private ZArchitecture.GUI.ZCheckBox SendMCDInformationCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsPackingMaterialsContaminatedCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsContainerCleanCheckBox;
		private ZArchitecture.GUI.ZCheckBox HasMPIContainerQDCheckBox;
		private ZArchitecture.ZLabel WoodPackagingLabel;
		private ZArchitecture.GUI.ZCheckBox IsWoodPackingUsedCheckBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl DeliveryDestinationPartyDocAddressControl;

	}
}
