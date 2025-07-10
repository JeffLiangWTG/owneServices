namespace Enterprise.Customs.US.GUI
{
	partial class JobMainOrgsControl
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
			this.ImporterOfRecordOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ApplicantOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BondedWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.NotifyPartyOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SoldToPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.UltimateConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ShipToPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PTTCarrierOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterOfRecordOrgControl.SuspendLayout();
			this.ApplicantOrgControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.NotifyPartyOrgControl.SuspendLayout();
			this.SoldToPartyAddressControl.SuspendLayout();
			this.UltimateConsigneeAddressControl.SuspendLayout();
			this.ShipToPartyAddressControl.SuspendLayout();
			this.PTTCarrierOrgControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// ImporterOfRecordOrgControl
			// 
			this.ImporterOfRecordOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOfRecordOrgControl, "IOROrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).IOROrgPK)));
			this.ImporterOfRecordOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5CCBE358-ADCC-412E-B449-E6F7F6E4F0AE", "Importer of Record");
			this.ImporterOfRecordOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.ImporterOfRecordOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.ImporterOfRecordOrgControl.Name = "ImporterOfRecordOrgControl";
			this.ImporterOfRecordOrgControl.ShouldResize = true;
			this.ImporterOfRecordOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ImporterOfRecordOrgControl.TabIndex = 0;
			// 
			// ApplicantOrgControl
			// 
			this.ApplicantOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicantOrgControl, "IOROrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).IOROrgPK)));
			this.ApplicantOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2DC9E3A2-E3D9-4881-9593-2628171DBBC5", "Applicant");
			this.ApplicantOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.ApplicantOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.ApplicantOrgControl.Name = "ApplicantOrgControl";
			this.ApplicantOrgControl.ShouldResize = true;
			this.ApplicantOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ApplicantOrgControl.TabIndex = 0;
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BondedWarehouseDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedWarehouseDocAddressControl, "WarehouseDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).WarehouseDocAddress)));
			this.BondedWarehouseDocAddressControl.BindToOrganisations = "Lookups+BondedWarehouseCollection";
			this.BondedWarehouseDocAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7f6ff36e-f8ba-4062-b31a-57939d384215", "Bonded Warehouse");
			this.BondedWarehouseDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BondedWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 24, true);
			this.BondedWarehouseDocAddressControl.Name = "BondedWarehouseDocAddressControl";
			this.BondedWarehouseDocAddressControl.ReadOnly = false;
			this.BondedWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.BondedWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BondedWarehouseDocAddressControl.TabIndex = 1;
			this.BondedWarehouseDocAddressControl.ValidationJustForced = false;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "JE_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.BindToOrgList = "Lookups+Suppliers";
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6b1aa661-0c1f-4ddb-9e1d-254de72e3358", "Manufacturer");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ManufacturerAddressControl.TabIndex = 8;
			// 
			// NotifyPartyOrgControl
			// 
			this.NotifyPartyOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyOrgControl, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_NotifyParty)));
			this.NotifyPartyOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dbdd5715-9e92-4ddd-9492-efaf73854c85", "4811 Party");
			this.NotifyPartyOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.NotifyPartyOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 134, true);
			this.NotifyPartyOrgControl.Name = "NotifyPartyOrgControl";
			this.NotifyPartyOrgControl.ShouldResize = true;
			this.NotifyPartyOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.NotifyPartyOrgControl.TabIndex = 10;
			// 
			// SoldToPartyAddressControl
			// 
			this.SoldToPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SoldToPartyAddressControl, "JE_OA_SoldToPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_SoldToPartyAddress)));
			this.SoldToPartyAddressControl.BindToOrgList = "AddInfoLookups+Consignees";
			this.SoldToPartyAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f1ce496f-71fc-41a9-a988-a61b4020153f", "Sold To Party");
			this.SoldToPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.SoldToPartyAddressControl.Name = "SoldToPartyAddressControl";
			this.SoldToPartyAddressControl.PopupCaption = "";
			this.SoldToPartyAddressControl.ReadOnly = false;
			this.SoldToPartyAddressControl.ShowAddress = false;
			this.SoldToPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.SoldToPartyAddressControl.TabIndex = 4;
			// 
			// UltimateConsigneeAddressControl
			// 
			this.UltimateConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UltimateConsigneeAddressControl, "JE_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_ConsigneeAddress)));
			this.UltimateConsigneeAddressControl.BindToOrgList = "AddInfoLookups+Consignees";
			this.UltimateConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8369e8e1-50dc-4579-ad09-422767c725bd", "Ultimate Consignee");
			this.UltimateConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.UltimateConsigneeAddressControl.Name = "UltimateConsigneeAddressControl";
			this.UltimateConsigneeAddressControl.PopupCaption = "";
			this.UltimateConsigneeAddressControl.ReadOnly = false;
			this.UltimateConsigneeAddressControl.ShowAddress = false;
			this.UltimateConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.UltimateConsigneeAddressControl.TabIndex = 2;
			// 
			// ShipToPartyAddressControl
			// 
			this.ShipToPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipToPartyAddressControl, "JE_OA_ShipToPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_ShipToPartyAddress)));
			this.ShipToPartyAddressControl.BindToOrgList = "AddInfoLookups+Consignees";
			this.ShipToPartyAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2dea8357-2f24-4d5f-8ea0-38b66180df54", "Ship To Party");
			this.ShipToPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.ShipToPartyAddressControl.Name = "ShipToPartyAddressControl";
			this.ShipToPartyAddressControl.PopupCaption = "";
			this.ShipToPartyAddressControl.ReadOnly = false;
			this.ShipToPartyAddressControl.ShowAddress = false;
			this.ShipToPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShipToPartyAddressControl.TabIndex = 6;
			// 
			// PTTCarrierOrgControl
			// 
			this.PTTCarrierOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PTTCarrierOrgControl, "DeliveryOrPickupCartageCoPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).DeliveryOrPickupCartageCoPK)));
			this.PTTCarrierOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0afd4289-7cf0-40ea-aca1-34dcbd2284c2", "PTT Carrier");
			this.PTTCarrierOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.PTTCarrierOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 156, true);
			this.PTTCarrierOrgControl.Name = "PTTCarrierOrgControl";
			this.PTTCarrierOrgControl.ShouldResize = true;
			this.PTTCarrierOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.PTTCarrierOrgControl.TabIndex = 11;
			// 
			// JobMainOrgsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PTTCarrierOrgControl);
			this.Controls.Add(this.NotifyPartyOrgControl);
			this.Controls.Add(this.ManufacturerAddressControl);
			this.Controls.Add(this.ShipToPartyAddressControl);
			this.Controls.Add(this.SoldToPartyAddressControl);
			this.Controls.Add(this.UltimateConsigneeAddressControl);
			this.Controls.Add(this.BondedWarehouseDocAddressControl);
			this.Controls.Add(this.ImporterOfRecordOrgControl);
			this.Controls.Add(this.ApplicantOrgControl);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 178, true);
			this.Name = "JobMainOrgsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 178, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterOfRecordOrgControl.ResumeLayout(true);
			this.ImporterOfRecordOrgControl.PerformLayout();
			this.ApplicantOrgControl.ResumeLayout(true);
			this.ApplicantOrgControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.NotifyPartyOrgControl.ResumeLayout(true);
			this.NotifyPartyOrgControl.PerformLayout();
			this.SoldToPartyAddressControl.ResumeLayout(true);
			this.SoldToPartyAddressControl.PerformLayout();
			this.UltimateConsigneeAddressControl.ResumeLayout(true);
			this.UltimateConsigneeAddressControl.PerformLayout();
			this.ShipToPartyAddressControl.ResumeLayout(true);
			this.ShipToPartyAddressControl.PerformLayout();
			this.PTTCarrierOrgControl.ResumeLayout(true);
			this.PTTCarrierOrgControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox ImporterOfRecordOrgControl;
		internal ZArchitecture.GUI.ZGuidFindBox ApplicantOrgControl;
		internal MasterFiles.GUI.ZDocAddressControl BondedWarehouseDocAddressControl;
		internal ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox NotifyPartyOrgControl;
		private ZArchitecture.GUI.ZAddressControl SoldToPartyAddressControl;
		private ZArchitecture.GUI.ZAddressControl UltimateConsigneeAddressControl;
		private ZArchitecture.GUI.ZAddressControl ShipToPartyAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox PTTCarrierOrgControl;
	}
}
