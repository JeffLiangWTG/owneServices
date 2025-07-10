using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WhsDetailsUserControl
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
			this.BondedWarehouseAutomationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IMUsedVatWarehouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CusInventoryForOutwardProcessingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CusInventoryForInwardProcessingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseBondedWarehouseAutomationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DGContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PhoneNumberzTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EXDefaultDGContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EXDefaultDGContactPhoneUsedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConfigurationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IMDefaultIncoTermBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BackOrdersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GenerateBackOrdersOnShortfallsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PackingSlipOrderByGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsPackingSlipOrderByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RecalculateOrderPricingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecalculateOrderPricingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondedWarehouseAutomationGroupBox.SuspendLayout();
			this.DGContactDetailsGroupBox.SuspendLayout();
			this.EXDefaultDGContactGuidFindBox.SuspendLayout();
			this.EXDefaultDGContactPhoneUsedDropEdit.SuspendLayout();
			this.ConfigurationDetailsGroupBox.SuspendLayout();
			this.IMDefaultIncoTermBoundDropEdit.SuspendLayout();
			this.BackOrdersGroupBox.SuspendLayout();
			this.PackingSlipOrderByGroupBox.SuspendLayout();
			this.WhsPackingSlipOrderByDropEdit.SuspendLayout();
			this.RecalculateOrderPricingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// BondedWarehouseAutomationGroupBox
			// 
			this.BondedWarehouseAutomationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|2b82dad8-ab58-4a2e-a248-44b3cb523358", "Inventory Management");
			this.BondedWarehouseAutomationGroupBox.Controls.Add(this.IMUsedVatWarehouseCheckBox);
			this.BondedWarehouseAutomationGroupBox.Controls.Add(this.CusInventoryForOutwardProcessingCheckBox);
			this.BondedWarehouseAutomationGroupBox.Controls.Add(this.CusInventoryForInwardProcessingCheckBox);
			this.BondedWarehouseAutomationGroupBox.Controls.Add(this.UseBondedWarehouseAutomationCheckBox);
			this.BondedWarehouseAutomationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.BondedWarehouseAutomationGroupBox.Name = "BondedWarehouseAutomationGroupBox";
			this.BondedWarehouseAutomationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 116, true);
			this.BondedWarehouseAutomationGroupBox.TabIndex = 0;
			this.BondedWarehouseAutomationGroupBox.TabStop = false;
			// 
			// IMUsedVatWarehouseCheckBox
			// 
			this.IMUsedVatWarehouseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IMUsedVatWarehouseCheckBox, "CompanyData+OB_IMUsedVatWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_IMUsedVatWarehouse)));
			this.IMUsedVatWarehouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 92, true);
			this.IMUsedVatWarehouseCheckBox.Name = "IMUsedVatWarehouseCheckBox";
			this.IMUsedVatWarehouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 15, true);
			this.IMUsedVatWarehouseCheckBox.TabIndex = 6;
			// 
			// CusInventoryForOutwardProcessingCheckBox
			// 
			this.CusInventoryForOutwardProcessingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CusInventoryForOutwardProcessingCheckBox, "CompanyData+OB_CusInventoryForOutwardProcessing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_CusInventoryForOutwardProcessing)));
			this.CusInventoryForOutwardProcessingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 70, true);
			this.CusInventoryForOutwardProcessingCheckBox.Name = "CusInventoryForOutwardProcessingCheckBox";
			this.CusInventoryForOutwardProcessingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 15, true);
			this.CusInventoryForOutwardProcessingCheckBox.TabIndex = 5;
			// 
			// CusInventoryForInwardProcessingCheckBox
			// 
			this.CusInventoryForInwardProcessingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CusInventoryForInwardProcessingCheckBox, "CompanyData+OB_CusInventoryForInwardProcessing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_CusInventoryForInwardProcessing)));
			this.CusInventoryForInwardProcessingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 47, true);
			this.CusInventoryForInwardProcessingCheckBox.Name = "CusInventoryForInwardProcessingCheckBox";
			this.CusInventoryForInwardProcessingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 15, true);
			this.CusInventoryForInwardProcessingCheckBox.TabIndex = 4;
			// 
			// UseBondedWarehouseAutomationCheckBox
			// 
			this.UseBondedWarehouseAutomationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseBondedWarehouseAutomationCheckBox, "CompanyData+OB_IMUsedBondedWhs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_IMUsedBondedWhs)));
			this.UseBondedWarehouseAutomationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 24, true);
			this.UseBondedWarehouseAutomationCheckBox.Name = "UseBondedWarehouseAutomationCheckBox";
			this.UseBondedWarehouseAutomationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 15, true);
			this.UseBondedWarehouseAutomationCheckBox.TabIndex = 1;
			// 
			// DGContactDetailsGroupBox
			// 
			this.DGContactDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|7edfa569-8c4f-4e00-9311-44bdf1c53601", "DG Contact Details");
			this.DGContactDetailsGroupBox.Controls.Add(this.PhoneNumberzTextBox);
			this.DGContactDetailsGroupBox.Controls.Add(this.EXDefaultDGContactGuidFindBox);
			this.DGContactDetailsGroupBox.Controls.Add(this.EXDefaultDGContactPhoneUsedDropEdit);
			this.DGContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.DGContactDetailsGroupBox.Name = "DGContactDetailsGroupBox";
			this.DGContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 103, true);
			this.DGContactDetailsGroupBox.TabIndex = 2;
			this.DGContactDetailsGroupBox.TabStop = false;
			// 
			// PhoneNumberzTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneNumberzTextBox, "MiscServ+DGPhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.DGPhoneNumber)));
			this.PhoneNumberzTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|06224ec8-43d7-430c-a8e0-f5dc0569a699", "Phone Number");
			this.PhoneNumberzTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PhoneNumberzTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 70, true);
			this.PhoneNumberzTextBox.Name = "PhoneNumberzTextBox";
			this.PhoneNumberzTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 16, true);
			this.PhoneNumberzTextBox.TabIndex = 5;
			// 
			// EXDefaultDGContactGuidFindBox
			// 
			this.EXDefaultDGContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EXDefaultDGContactGuidFindBox, "MiscServ+OM_OC_EXDefaultDGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_OC_EXDefaultDGContact)));
			this.EXDefaultDGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 18, true);
			this.EXDefaultDGContactGuidFindBox.Name = "EXDefaultDGContactGuidFindBox";
			this.EXDefaultDGContactGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.EXDefaultDGContactGuidFindBox.ParentType = null;
			this.EXDefaultDGContactGuidFindBox.PreBoundMaxLength = 25;
			this.EXDefaultDGContactGuidFindBox.ShowDescriptionBox = false;
			this.EXDefaultDGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 16, true);
			this.EXDefaultDGContactGuidFindBox.TabIndex = 3;
			// 
			// EXDefaultDGContactPhoneUsedDropEdit
			// 
			this.EXDefaultDGContactPhoneUsedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EXDefaultDGContactPhoneUsedDropEdit, "MiscServ+OM_EXDefaultDGContactPhoneUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXDefaultDGContactPhoneUsed)));
			this.EXDefaultDGContactPhoneUsedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 44, true);
			this.EXDefaultDGContactPhoneUsedDropEdit.Name = "EXDefaultDGContactPhoneUsedDropEdit";
			this.EXDefaultDGContactPhoneUsedDropEdit.PreBoundMaxLength = 3;
			this.EXDefaultDGContactPhoneUsedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 16, true);
			this.EXDefaultDGContactPhoneUsedDropEdit.TabIndex = 4;
			// 
			// ConfigurationDetailsGroupBox
			// 
			this.ConfigurationDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|e7654462-cf01-419e-9760-b840cfa727a0", "Importer / Consignee Configuration");
			this.ConfigurationDetailsGroupBox.Controls.Add(this.IMDefaultIncoTermBoundDropEdit);
			this.ConfigurationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 235, true);
			this.ConfigurationDetailsGroupBox.Name = "ConfigurationDetailsGroupBox";
			this.ConfigurationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 63, true);
			this.ConfigurationDetailsGroupBox.TabIndex = 6;
			this.ConfigurationDetailsGroupBox.TabStop = false;
			// 
			// IMDefaultIncoTermBoundDropEdit
			// 
			this.IMDefaultIncoTermBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IMDefaultIncoTermBoundDropEdit, "MiscServ+OM_IMDefaultINCOTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMDefaultINCOTerm)));
			this.IMDefaultIncoTermBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 25, true);
			this.IMDefaultIncoTermBoundDropEdit.Name = "IMDefaultIncoTermBoundDropEdit";
			this.IMDefaultIncoTermBoundDropEdit.PreBoundMaxLength = 3;
			this.IMDefaultIncoTermBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 16, true);
			this.IMDefaultIncoTermBoundDropEdit.TabIndex = 7;
			// 
			// BackOrdersGroupBox
			// 
			this.BackOrdersGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|ce97d9e1-a85a-40e5-9512-6e9400200442", "Back Orders");
			this.BackOrdersGroupBox.Controls.Add(this.GenerateBackOrdersOnShortfallsCheckBox);
			this.BackOrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 301, true);
			this.BackOrdersGroupBox.Name = "BackOrdersGroupBox";
			this.BackOrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 56, true);
			this.BackOrdersGroupBox.TabIndex = 8;
			this.BackOrdersGroupBox.TabStop = false;
			// 
			// GenerateBackOrdersOnShortfallsCheckBox
			// 
			this.GenerateBackOrdersOnShortfallsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GenerateBackOrdersOnShortfallsCheckBox, "MiscServ.OM_WhsGenerateBackOrdersOnShortfalls");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsGenerateBackOrdersOnShortfalls)));
			this.GenerateBackOrdersOnShortfallsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|b96e45c7-b2c3-47b9-9d85-b98de28def1f", "Generate Back Orders On Shortfalls");
			this.GenerateBackOrdersOnShortfallsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 24, true);
			this.GenerateBackOrdersOnShortfallsCheckBox.Name = "GenerateBackOrdersOnShortfallsCheckBox";
			this.GenerateBackOrdersOnShortfallsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 15, true);
			this.GenerateBackOrdersOnShortfallsCheckBox.TabIndex = 9;
			this.GenerateBackOrdersOnShortfallsCheckBox.UseVisualStyleBackColor = true;
			// 
			// PackingSlipOrderByGroupBox
			// 
			this.PackingSlipOrderByGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|d4ba8b18-6eab-402f-8541-5d84a724a395", "Order Copy/Packing Slip Order By");
			this.PackingSlipOrderByGroupBox.Controls.Add(this.WhsPackingSlipOrderByDropEdit);
			this.PackingSlipOrderByGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 365, true);
			this.PackingSlipOrderByGroupBox.Name = "PackingSlipOrderByGroupBox";
			this.PackingSlipOrderByGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 56, true);
			this.PackingSlipOrderByGroupBox.TabIndex = 10;
			this.PackingSlipOrderByGroupBox.TabStop = false;
			// 
			// WhsPackingSlipOrderByDropEdit
			// 
			this.WhsPackingSlipOrderByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsPackingSlipOrderByDropEdit, "MiscServ+OM_WhsPackingSlipOrderBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsPackingSlipOrderBy)));
			this.WhsPackingSlipOrderByDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|52fdf965-aa2a-4632-ba32-53fba999bcce", "Order", "Order By", "Default Order By Lines for Order Copy/Packing Slip documents.");
			this.WhsPackingSlipOrderByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 25, true);
			this.WhsPackingSlipOrderByDropEdit.Name = "WhsPackingSlipOrderByDropEdit";
			this.WhsPackingSlipOrderByDropEdit.PreBoundMaxLength = 3;
			this.WhsPackingSlipOrderByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 16, true);
			this.WhsPackingSlipOrderByDropEdit.TabIndex = 11;
			// 
			// RecalculateOrderPricingGroupBox
			// 
			this.RecalculateOrderPricingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|RecalculateOrderPricingGroupBox|Text", "Order pricing");
			this.RecalculateOrderPricingGroupBox.Controls.Add(this.RecalculateOrderPricingCheckBox);
			this.RecalculateOrderPricingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 430, true);
			this.RecalculateOrderPricingGroupBox.Name = "RecalculateOrderPricingGroupBox";
			this.RecalculateOrderPricingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 56, true);
			this.RecalculateOrderPricingGroupBox.TabIndex = 11;
			this.RecalculateOrderPricingGroupBox.TabStop = false;
			// 
			// RecalculateOrderPricingCheckBox
			// 
			this.RecalculateOrderPricingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RecalculateOrderPricingCheckBox, "MiscServ.OM_WhsIsRecalculateOrderPricing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsIsRecalculateOrderPricing)));
			this.RecalculateOrderPricingCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsDetailsUserControl|RecalculateOrderPricingCheckBox|Text", "Calculate Order Pricing");
			this.RecalculateOrderPricingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 24, true);
			this.RecalculateOrderPricingCheckBox.Name = "RecalculateOrderPricingCheckBox";
			this.RecalculateOrderPricingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 15, true);
			this.RecalculateOrderPricingCheckBox.TabIndex = 9;
			this.RecalculateOrderPricingCheckBox.UseVisualStyleBackColor = true;
			// 
			// WhsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RecalculateOrderPricingGroupBox);
			this.Controls.Add(this.PackingSlipOrderByGroupBox);
			this.Controls.Add(this.BackOrdersGroupBox);
			this.Controls.Add(this.ConfigurationDetailsGroupBox);
			this.Controls.Add(this.DGContactDetailsGroupBox);
			this.Controls.Add(this.BondedWarehouseAutomationGroupBox);
			this.Name = "WhsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 485, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondedWarehouseAutomationGroupBox.ResumeLayout(false);
			this.BondedWarehouseAutomationGroupBox.PerformLayout();
			this.DGContactDetailsGroupBox.ResumeLayout(false);
			this.DGContactDetailsGroupBox.PerformLayout();
			this.EXDefaultDGContactGuidFindBox.ResumeLayout(true);
			this.EXDefaultDGContactGuidFindBox.PerformLayout();
			this.EXDefaultDGContactPhoneUsedDropEdit.ResumeLayout(true);
			this.EXDefaultDGContactPhoneUsedDropEdit.PerformLayout();
			this.ConfigurationDetailsGroupBox.ResumeLayout(false);
			this.ConfigurationDetailsGroupBox.PerformLayout();
			this.IMDefaultIncoTermBoundDropEdit.ResumeLayout(true);
			this.IMDefaultIncoTermBoundDropEdit.PerformLayout();
			this.BackOrdersGroupBox.ResumeLayout(false);
			this.BackOrdersGroupBox.PerformLayout();
			this.PackingSlipOrderByGroupBox.ResumeLayout(false);
			this.PackingSlipOrderByGroupBox.PerformLayout();
			this.WhsPackingSlipOrderByDropEdit.ResumeLayout(true);
			this.WhsPackingSlipOrderByDropEdit.PerformLayout();
			this.RecalculateOrderPricingGroupBox.ResumeLayout(false);
			this.RecalculateOrderPricingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox BondedWarehouseAutomationGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox UseBondedWarehouseAutomationCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DGContactDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit EXDefaultDGContactPhoneUsedDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox EXDefaultDGContactGuidFindBox;
		internal Enterprise.ZArchitecture.ZTextBox PhoneNumberzTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ConfigurationDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit IMDefaultIncoTermBoundDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox BackOrdersGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox GenerateBackOrdersOnShortfallsCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox PackingSlipOrderByGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit WhsPackingSlipOrderByDropEdit;
		internal ZArchitecture.GUI.ZGroupBox RecalculateOrderPricingGroupBox;
		internal ZArchitecture.GUI.ZCheckBox RecalculateOrderPricingCheckBox;
		internal ZArchitecture.GUI.ZCheckBox CusInventoryForOutwardProcessingCheckBox;
		internal ZArchitecture.GUI.ZCheckBox CusInventoryForInwardProcessingCheckBox;
		internal ZArchitecture.GUI.ZCheckBox IMUsedVatWarehouseCheckBox;
	}
}
