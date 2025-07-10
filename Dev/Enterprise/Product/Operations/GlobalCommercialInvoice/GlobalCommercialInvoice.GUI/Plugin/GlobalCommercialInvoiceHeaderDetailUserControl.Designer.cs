using CargoWise.Types;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	partial class GlobalCommercialInvoiceHeaderDetailUserControl
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
			this.InvoiceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceGoodsOriginBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InvoiceAmountCurrencyControl = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceExportCountryBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InvoiceDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceImportCountryBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InvoiceImporterOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.InvoiceSupplierOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.InvoiceOrganizationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceOrganizationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.InvoiceGoodsOriginBoundFindBox.SuspendLayout();
			this.InvoiceAmountCurrencyControl.SuspendLayout();
			this.InvoiceExportCountryBoundFindBox.SuspendLayout();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.InvoiceImportCountryBoundFindBox.SuspendLayout();
			this.InvoiceImporterOrgAddressControl.SuspendLayout();
			this.InvoiceSupplierOrgAddressControl.SuspendLayout();
			this.InvoiceOrganizationGroupBox.SuspendLayout();
			this.InvoiceOrganizationPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceDetailsGroupBox.AutoSize = true;
			this.InvoiceDetailsGroupBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("ec57ea4a-4529-4c13-a1d7-31683e6b1cc2", "Details");
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceGoodsOriginBoundFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceAmountCurrencyControl);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceNumberTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceExportCountryBoundFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceDateDateEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceDescriptionTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceImportCountryBoundFindBox);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 10, true);
			this.InvoiceDetailsGroupBox.Name = "InvoiceDetailsGroupBox";
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 560, true);
			this.InvoiceDetailsGroupBox.TabIndex = 0;
			this.InvoiceDetailsGroupBox.TabStop = false;
			this.InvoiceDetailsGroupBox.Text = "Commercial Invoice Details";
			// 
			// InvoiceGoodsOriginBoundFindBox
			// 
			this.InvoiceGoodsOriginBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceGoodsOriginBoundFindBox, "GIH_RN_NKCountryOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_RN_NKCountryOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).Lookups.CountryOrigins)));
			this.InvoiceGoodsOriginBoundFindBox.BindToList = "Lookups.CountryOrigins";
			this.InvoiceGoodsOriginBoundFindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("a6371e01-a568-412f-8fec-9133bce7d34a", "Goods Origin");
			this.InvoiceGoodsOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 175, true);
			this.InvoiceGoodsOriginBoundFindBox.Name = "InvoiceGoodsOriginBoundFindBox";
			this.InvoiceGoodsOriginBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceGoodsOriginBoundFindBox.ParentType = null;
			this.InvoiceGoodsOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.InvoiceGoodsOriginBoundFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 10, true);
			this.InvoiceGoodsOriginBoundFindBox.TabIndex = 8;
			// 
			// InvoiceAmountCurrencyControl
			// 
			this.InvoiceAmountCurrencyControl.AllowDrop = true;
			this.InvoiceAmountCurrencyControl.BindToAmount = "GIH_InvoiceAmount";
			this.InvoiceAmountCurrencyControl.BindToUnit = "GIH_RX_NKInvoiceCurrency";
			this.InvoiceAmountCurrencyControl.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("5a0750ae-db18-4dc8-a983-00688bebaca6", "Invoice Total");
			this.InvoiceAmountCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InvoiceAmountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 67, true);
			this.InvoiceAmountCurrencyControl.Name = "InvoiceAmountCurrencyControl";
			this.InvoiceAmountCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.InvoiceAmountCurrencyControl.TabIndex = 4;
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "GIH_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_InvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("d92cbcf9-71cf-4122-abe9-b560081242c5", "Invoice No.");
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 25, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.InvoiceNumberTextBox.TabIndex = 2;
			// 
			// InvoiceExportCountryBoundFindBox
			// 
			this.InvoiceExportCountryBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceExportCountryBoundFindBox, "GIH_RN_NKCountryExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_RN_NKCountryExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).Lookups.CountryExports)));
			this.InvoiceExportCountryBoundFindBox.BindToList = "Lookups.CountryExports";
			this.InvoiceExportCountryBoundFindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("9d6b73a4-64dd-4761-97f9-0429f1ef50a0", "Ctry/Rgn. Of Export");
			this.InvoiceExportCountryBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 132, true);
			this.InvoiceExportCountryBoundFindBox.Name = "InvoiceExportCountryBoundFindBox";
			this.InvoiceExportCountryBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceExportCountryBoundFindBox.ParentType = null;
			this.InvoiceExportCountryBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 10, true);
			this.InvoiceExportCountryBoundFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 10, true);
			this.InvoiceExportCountryBoundFindBox.TabIndex = 6;
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AllowDrop = true;
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "GIH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_InvoiceDate)));
			this.InvoiceDateDateEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("d02b8a34-82fb-48c0-80e9-45d08fee4bc7", "Invoice Date");
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 46, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 3;
			// 
			// InvoiceDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceDescriptionTextBox, "GIH_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_Description)));
			this.InvoiceDescriptionTextBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("8753a89b-259c-4054-9e73-bdd5378bec9c", "Goods Description");
			this.InvoiceDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 88, true);
			this.InvoiceDescriptionTextBox.Multiline = true;
			this.InvoiceDescriptionTextBox.Name = "InvoiceDescriptionTextBox";
			this.InvoiceDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 40, true);
			this.InvoiceDescriptionTextBox.TabIndex = 5;
			// 
			// InvoiceImportCountryBoundFindBox
			// 
			this.InvoiceImportCountryBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceImportCountryBoundFindBox, "GIH_RN_NKCountryImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_RN_NKCountryImport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).Lookups.CountryImports)));
			this.InvoiceImportCountryBoundFindBox.BindToList = "Lookups.CountryImports";
			this.InvoiceImportCountryBoundFindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("92447380-34a1-41c2-8123-ad73f0b57fcd", "Ctry/Rgn. Of Import");
			this.InvoiceImportCountryBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 154, true);
			this.InvoiceImportCountryBoundFindBox.Name = "InvoiceImportCountryBoundFindBox";
			this.InvoiceImportCountryBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceImportCountryBoundFindBox.ParentType = null;
			this.InvoiceImportCountryBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.InvoiceImportCountryBoundFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 10, true);
			this.InvoiceImportCountryBoundFindBox.TabIndex = 7;
			// 
			// InvoiceImporterOrgAddressControl
			// 
			this.InvoiceImporterOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceImporterOrgAddressControl, "GIH_OA_ImporterAddress_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_OA_ImporterAddress_ZAddress)));
			this.InvoiceImporterOrgAddressControl.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("f73a00ab-0a0d-4acc-a223-1954d22d1d78", "Importer");
			this.InvoiceImporterOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 171, true);
			this.InvoiceImporterOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceImporterOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceImporterOrgAddressControl.Name = "InvoiceImporterOrgAddressControl";
			this.InvoiceImporterOrgAddressControl.PopupCaption = "";
			this.InvoiceImporterOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceImporterOrgAddressControl.TabIndex = 1;
			// 
			// InvoiceSupplierOrgAddressControl
			// 
			this.InvoiceSupplierOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceSupplierOrgAddressControl, "GIH_OA_SupplierAddress_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(null)).GIH_OA_SupplierAddress_ZAddress)));
			this.InvoiceSupplierOrgAddressControl.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("f9b4ce0a-a126-4422-b11b-709f55dfa97d", "Supplier");
			this.InvoiceSupplierOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 11, true);
			this.InvoiceSupplierOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceSupplierOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceSupplierOrgAddressControl.Name = "InvoiceSupplierOrgAddressControl";
			this.InvoiceSupplierOrgAddressControl.PopupCaption = "";
			this.InvoiceSupplierOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.InvoiceSupplierOrgAddressControl.TabIndex = 0;
			// 
			// InvoiceOrganizationGroupBox
			// 
			this.InvoiceOrganizationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceOrganizationGroupBox.AutoSize = true;
			this.InvoiceOrganizationGroupBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("96f05e00-5fe9-4881-8334-3c694f9bffd2", "Organization Details");
			this.InvoiceOrganizationGroupBox.Controls.Add(this.InvoiceOrganizationPanel);
			this.InvoiceOrganizationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 10, true);
			this.InvoiceOrganizationGroupBox.Name = "InvoiceOrganizationGroupBox";
			this.InvoiceOrganizationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 560, true);
			this.InvoiceOrganizationGroupBox.TabIndex = 7;
			this.InvoiceOrganizationGroupBox.TabStop = false;
			this.InvoiceOrganizationGroupBox.Text = "Organization Details";
			// 
			// InvoiceOrganizationPanel
			// 
			this.InvoiceOrganizationPanel.AutoScroll = true;
			this.InvoiceOrganizationPanel.Controls.Add(this.InvoiceSupplierOrgAddressControl);
			this.InvoiceOrganizationPanel.Controls.Add(this.InvoiceImporterOrgAddressControl);
			this.InvoiceOrganizationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceOrganizationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.InvoiceOrganizationPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.InvoiceOrganizationPanel.Name = "InvoiceOrganizationPanel";
			this.InvoiceOrganizationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 543, true);
			this.InvoiceOrganizationPanel.TabIndex = 8;
			// 
			// GlobalCommercialInvoiceHeaderDetailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.Controls.Add(this.InvoiceOrganizationGroupBox);
			this.Controls.Add(this.InvoiceDetailsGroupBox);
			this.Name = "GlobalCommercialInvoiceHeaderDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 576, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.InvoiceGoodsOriginBoundFindBox.ResumeLayout(true);
			this.InvoiceGoodsOriginBoundFindBox.PerformLayout();
			this.InvoiceAmountCurrencyControl.ResumeLayout(true);
			this.InvoiceAmountCurrencyControl.PerformLayout();
			this.InvoiceExportCountryBoundFindBox.ResumeLayout(true);
			this.InvoiceExportCountryBoundFindBox.PerformLayout();
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.InvoiceImportCountryBoundFindBox.ResumeLayout(true);
			this.InvoiceImportCountryBoundFindBox.PerformLayout();
			this.InvoiceImporterOrgAddressControl.ResumeLayout(true);
			this.InvoiceImporterOrgAddressControl.PerformLayout();
			this.InvoiceSupplierOrgAddressControl.ResumeLayout(true);
			this.InvoiceSupplierOrgAddressControl.PerformLayout();
			this.InvoiceOrganizationGroupBox.ResumeLayout(false);
			this.InvoiceOrganizationGroupBox.PerformLayout();
			this.InvoiceOrganizationPanel.ResumeLayout(false);
			this.InvoiceOrganizationPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		private ZArchitecture.GUI.ZGroupBox InvoiceDetailsGroupBox;
		private ZArchitecture.ZTextBox InvoiceNumberTextBox;
		private ZArchitecture.ZTextBox InvoiceDescriptionTextBox;
		private ZArchitecture.GUI.ZDateEdit InvoiceDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox InvoiceExportCountryBoundFindBox;
		private ZArchitecture.GUI.ZCodeFindBox InvoiceImportCountryBoundFindBox;
		private ZArchitecture.GUI.ZCodeFindBox InvoiceGoodsOriginBoundFindBox;
		private Enterprise.MasterFiles.GUI.ZOrgAddressControl InvoiceImporterOrgAddressControl;
		private Enterprise.MasterFiles.GUI.ZOrgAddressControl InvoiceSupplierOrgAddressControl;
		private ZArchitecture.GUI.ZCalcFindBox InvoiceAmountCurrencyControl;
		private ZArchitecture.GUI.ZGroupBox InvoiceOrganizationGroupBox;
		private ZArchitecture.GUI.ZPanel InvoiceOrganizationPanel;
	}
}
