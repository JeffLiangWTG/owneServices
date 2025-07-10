namespace Enterprise.Customs.GUI
{
	partial class ExWarehouseFrontPageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		protected Enterprise.MasterFiles.GUI.ZOrganisationControl ImporterOrganisationControl;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JE_RS_NKServiceLevelBoundFindBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl BondedWarehouseDocAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ServiceLevelGroupBox;

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.ImporterOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.JE_RS_NKServiceLevelBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ServiceLevelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BondedWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ServiceLevelGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			// 
			// ImporterOrganisationControl
			// 
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 144, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControl.TabIndex = 1;
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.JE_RS_NKServiceLevelBoundFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ServiceLevels)));
			this.JE_RS_NKServiceLevelBoundFindBox.BindToList = "Lookups.ServiceLevels";
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 104, true);
			this.JE_RS_NKServiceLevelBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.JE_RS_NKServiceLevelBoundFindBox.Name = "JE_RS_NKServiceLevelBoundFindBox";
			this.JE_RS_NKServiceLevelBoundFindBox.PreBoundMaxLength = 3;
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 1;
			// 
			// ServiceLevelGroupBox
			// 
			this.ServiceLevelGroupBox.Controls.Add(this.JE_RS_NKServiceLevelBoundFindBox);
			this.ServiceLevelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 208, true);
			this.ServiceLevelGroupBox.Name = "ServiceLevelGroupBox";
			this.ServiceLevelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 232, true);
			this.ServiceLevelGroupBox.TabIndex = 4;
			this.ServiceLevelGroupBox.TabStop = false;
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BindingSource.SetBindingMember(this.BondedWarehouseDocAddressControl, "WarehouseDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).WarehouseDocAddress)));
			this.BondedWarehouseDocAddressControl.BindToOrganisations = "Lookups+BondedWarehouseCollection";
			this.BondedWarehouseDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideAndTabs;
			this.BondedWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 64, true);
			this.BondedWarehouseDocAddressControl.Name = "BondedWarehouseDocAddressControl";
			this.BondedWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 68, true);
			this.BondedWarehouseDocAddressControl.TabIndex = 3;
			// 
			// ExWarehouseFrontPageUserControl
			// 
			this.Controls.Add(this.BondedWarehouseDocAddressControl);
			this.Controls.Add(this.ServiceLevelGroupBox);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Name = "ExWarehouseFrontPageUserControl";
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ServiceLevelGroupBox, 0);
			this.Controls.SetChildIndex(this.BondedWarehouseDocAddressControl, 0);
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServiceLevelGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion
	}
}
