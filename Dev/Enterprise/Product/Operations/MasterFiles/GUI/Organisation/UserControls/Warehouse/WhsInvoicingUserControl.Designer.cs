using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WhsInvoicingUserControl
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
			this.AutoRatingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SerialNumberIsKeyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChargeStorageInAdvanceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FreeStorageDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverrideFreeStorageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Attrib3IsKeyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WarehouseRatingPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Attrib2IsKeyCheckBoxCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Attrib1IsKeyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WhsStorageCalcMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoicingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncludeReleaseChargesOnShipmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InvoiceDetailReportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceDetailReportSort3DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDetailReportSort2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDetailReportSortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WhsAutoPeriodicInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WhsAutoPostPeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WhsAutoDeliverPeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AutoRatingGroupBox.SuspendLayout();
			this.WarehouseRatingPeriodDropEdit.SuspendLayout();
			this.WhsStorageCalcMethodDropEdit.SuspendLayout();
			this.InvoicingGroupBox.SuspendLayout();
			this.InvoiceDetailReportGroupBox.SuspendLayout();
			this.InvoiceDetailReportSort3DropEdit.SuspendLayout();
			this.InvoiceDetailReportSort2DropEdit.SuspendLayout();
			this.InvoiceDetailReportSortDropEdit.SuspendLayout();
			this.WhsAutoPeriodicInvoiceGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// AutoRatingGroupBox
			// 
			this.AutoRatingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|ee40ee57-459b-4f25-b08b-d033690a690a", "Auto Rating");
			this.AutoRatingGroupBox.Controls.Add(this.SerialNumberIsKeyCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.ChargeStorageInAdvanceCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.FreeStorageDaysCalcEdit);
			this.AutoRatingGroupBox.Controls.Add(this.OverrideFreeStorageCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.Attrib3IsKeyCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.WarehouseRatingPeriodDropEdit);
			this.AutoRatingGroupBox.Controls.Add(this.Attrib2IsKeyCheckBoxCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.Attrib1IsKeyCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.WhsStorageCalcMethodDropEdit);
			this.AutoRatingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AutoRatingGroupBox.Name = "AutoRatingGroupBox";
			this.AutoRatingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 209, true);
			this.AutoRatingGroupBox.TabIndex = 0;
			this.AutoRatingGroupBox.TabStop = false;
			// 
			// SerialNumberIsKeyCheckBox
			// 
			this.SerialNumberIsKeyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SerialNumberIsKeyCheckBox, "MiscServ.OM_IMSerialNumberIsKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMSerialNumberIsKey)));
			this.SerialNumberIsKeyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 86, true);
			this.SerialNumberIsKeyCheckBox.Name = "SerialNumberIsKeyCheckBox";
			this.SerialNumberIsKeyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 17, true);
			this.SerialNumberIsKeyCheckBox.TabIndex = 3;
			// 
			// ChargeStorageInAdvanceCheckBox
			// 
			this.ChargeStorageInAdvanceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChargeStorageInAdvanceCheckBox, "CompanyData.OB_WhsChargeStorageInAdvance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsChargeStorageInAdvance)));
			this.ChargeStorageInAdvanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 160, true);
			this.ChargeStorageInAdvanceCheckBox.Name = "ChargeStorageInAdvanceCheckBox";
			this.ChargeStorageInAdvanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.ChargeStorageInAdvanceCheckBox.TabIndex = 6;
			this.ChargeStorageInAdvanceCheckBox.UseVisualStyleBackColor = true;
			// 
			// FreeStorageDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FreeStorageDaysCalcEdit, "CompanyData.OB_WhsClientFreeStorageDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsClientFreeStorageDays)));
			this.FreeStorageDaysCalcEdit.CaptionResourceString = null;
			this.FreeStorageDaysCalcEdit.DecimalPlaces = 0;
			this.FreeStorageDaysCalcEdit.Decimals = 0;
			this.FreeStorageDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 181, true);
			this.FreeStorageDaysCalcEdit.Name = "FreeStorageDaysCalcEdit";
			this.FreeStorageDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.FreeStorageDaysCalcEdit.TabIndex = 7;
			this.FreeStorageDaysCalcEdit.Text = "0";
			this.FreeStorageDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverrideFreeStorageCheckBox
			// 
			this.OverrideFreeStorageCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideFreeStorageCheckBox, "CompanyData.OB_WhsOverrideFreeStorage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsOverrideFreeStorage)));
			this.OverrideFreeStorageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 183, true);
			this.OverrideFreeStorageCheckBox.Name = "OverrideFreeStorageCheckBox";
			this.OverrideFreeStorageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.OverrideFreeStorageCheckBox.TabIndex = 8;
			this.OverrideFreeStorageCheckBox.UseVisualStyleBackColor = true;
			// 
			// Attrib3IsKeyCheckBox
			// 
			this.Attrib3IsKeyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Attrib3IsKeyCheckBox, "MiscServ+OM_IMAttrib3IsKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAttrib3IsKey)));
			this.Attrib3IsKeyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 64, true);
			this.Attrib3IsKeyCheckBox.Name = "Attrib3IsKeyCheckBox";
			this.Attrib3IsKeyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 17, true);
			this.Attrib3IsKeyCheckBox.TabIndex = 2;
			// 
			// WarehouseRatingPeriodDropEdit
			// 
			this.WarehouseRatingPeriodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseRatingPeriodDropEdit, "CompanyData.OB_ARWarehouseRatingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARWarehouseRatingPeriod)));
			this.WarehouseRatingPeriodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|722c9116-f0ac-40b7-bfb0-2ce3176fcf9a", "Storage Rating Period");
			this.WarehouseRatingPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 108, true);
			this.WarehouseRatingPeriodDropEdit.Name = "WarehouseRatingPeriodDropEdit";
			this.WarehouseRatingPeriodDropEdit.PreBoundMaxLength = 3;
			this.WarehouseRatingPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.WarehouseRatingPeriodDropEdit.TabIndex = 4;
			// 
			// Attrib2IsKeyCheckBoxCheckBox
			// 
			this.Attrib2IsKeyCheckBoxCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Attrib2IsKeyCheckBoxCheckBox, "MiscServ+OM_IMAttrib2IsKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAttrib2IsKey)));
			this.Attrib2IsKeyCheckBoxCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 42, true);
			this.Attrib2IsKeyCheckBoxCheckBox.Name = "Attrib2IsKeyCheckBoxCheckBox";
			this.Attrib2IsKeyCheckBoxCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 17, true);
			this.Attrib2IsKeyCheckBoxCheckBox.TabIndex = 1;
			// 
			// Attrib1IsKeyCheckBox
			// 
			this.Attrib1IsKeyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Attrib1IsKeyCheckBox, "MiscServ+OM_IMAttrib1IsKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAttrib1IsKey)));
			this.Attrib1IsKeyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 20, true);
			this.Attrib1IsKeyCheckBox.Name = "Attrib1IsKeyCheckBox";
			this.Attrib1IsKeyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 17, true);
			this.Attrib1IsKeyCheckBox.TabIndex = 0;
			// 
			// WhsStorageCalcMethodDropEdit
			// 
			this.WhsStorageCalcMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsStorageCalcMethodDropEdit, "CompanyData.OB_ARWhsStorageCalcMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARWhsStorageCalcMethod)));
			this.WhsStorageCalcMethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|6198d30b-bdd7-41c2-96a5-27e8cb02a400", "Storage Calculation Method");
			this.WhsStorageCalcMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 135, true);
			this.WhsStorageCalcMethodDropEdit.Name = "WhsStorageCalcMethodDropEdit";
			this.WhsStorageCalcMethodDropEdit.PreBoundMaxLength = 3;
			this.WhsStorageCalcMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.WhsStorageCalcMethodDropEdit.TabIndex = 5;
			// 
			// InvoicingGroupBox
			// 
			this.InvoicingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|62e7d099-9e32-4f94-920d-824fb5ee43f0", "Invoicing");
			this.InvoicingGroupBox.Controls.Add(this.IncludeReleaseChargesOnShipmentCheckBox);
			this.InvoicingGroupBox.Controls.Add(this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox);
			this.InvoicingGroupBox.Controls.Add(this.IncludeInwardsWhsConsolidatedInvoiceCheckBox);
			this.InvoicingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 223, true);
			this.InvoicingGroupBox.Name = "InvoicingGroupBox";
			this.InvoicingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 101, true);
			this.InvoicingGroupBox.TabIndex = 1;
			this.InvoicingGroupBox.TabStop = false;
			// 
			// IncludeReleaseChargesOnShipmentCheckBox
			// 
			this.IncludeReleaseChargesOnShipmentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeReleaseChargesOnShipmentCheckBox, "CompanyData.OB_WhsIncludeReleaseChargesOnShipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsIncludeReleaseChargesOnShipment)));
			this.IncludeReleaseChargesOnShipmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 73, true);
			this.IncludeReleaseChargesOnShipmentCheckBox.Name = "IncludeReleaseChargesOnShipmentCheckBox";
			this.IncludeReleaseChargesOnShipmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 17, true);
			this.IncludeReleaseChargesOnShipmentCheckBox.TabIndex = 2;
			// 
			// IncludeOutwardsWhsConsolidatedInvoiceCheckBox
			// 
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox, "CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice)));
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|b4e028fe-03c3-4919-b871-dfa07d33cb1c", "Include Release Charges on Consolidated Invoice");
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 47, true);
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.Name = "IncludeOutwardsWhsConsolidatedInvoiceCheckBox";
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 17, true);
			this.IncludeOutwardsWhsConsolidatedInvoiceCheckBox.TabIndex = 1;
			// 
			// IncludeInwardsWhsConsolidatedInvoiceCheckBox
			// 
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeInwardsWhsConsolidatedInvoiceCheckBox, "CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice)));
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|8df65282-c7fe-423d-b1fa-ded55f0cab6b", "Include Receive Charges on Consolidated Invoice");
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.Name = "IncludeInwardsWhsConsolidatedInvoiceCheckBox";
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 17, true);
			this.IncludeInwardsWhsConsolidatedInvoiceCheckBox.TabIndex = 0;
			// 
			// InvoiceDetailReportGroupBox
			// 
			this.InvoiceDetailReportGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|fca61906-bb98-4888-aeff-6f99a03cf61e", "Invoice Detail Report");
			this.InvoiceDetailReportGroupBox.Controls.Add(this.InvoiceDetailReportSort3DropEdit);
			this.InvoiceDetailReportGroupBox.Controls.Add(this.InvoiceDetailReportSort2DropEdit);
			this.InvoiceDetailReportGroupBox.Controls.Add(this.InvoiceDetailReportSortDropEdit);
			this.InvoiceDetailReportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 437, true);
			this.InvoiceDetailReportGroupBox.Name = "InvoiceDetailReportGroupBox";
			this.InvoiceDetailReportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 109, true);
			this.InvoiceDetailReportGroupBox.TabIndex = 3;
			this.InvoiceDetailReportGroupBox.TabStop = false;
			// 
			// InvoiceDetailReportSort3DropEdit
			// 
			this.InvoiceDetailReportSort3DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceDetailReportSort3DropEdit, "MiscServ+OM_IMInvoiceDetailReportSort3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMInvoiceDetailReportSort3)));
			this.InvoiceDetailReportSort3DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 80, true);
			this.InvoiceDetailReportSort3DropEdit.Name = "InvoiceDetailReportSort3DropEdit";
			this.InvoiceDetailReportSort3DropEdit.PreBoundMaxLength = 3;
			this.InvoiceDetailReportSort3DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.InvoiceDetailReportSort3DropEdit.TabIndex = 2;
			// 
			// InvoiceDetailReportSort2DropEdit
			// 
			this.InvoiceDetailReportSort2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceDetailReportSort2DropEdit, "MiscServ+OM_IMInvoiceDetailReportSort2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMInvoiceDetailReportSort2)));
			this.InvoiceDetailReportSort2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 53, true);
			this.InvoiceDetailReportSort2DropEdit.Name = "InvoiceDetailReportSort2DropEdit";
			this.InvoiceDetailReportSort2DropEdit.PreBoundMaxLength = 3;
			this.InvoiceDetailReportSort2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.InvoiceDetailReportSort2DropEdit.TabIndex = 1;
			// 
			// InvoiceDetailReportSortDropEdit
			// 
			this.InvoiceDetailReportSortDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceDetailReportSortDropEdit, "MiscServ+OM_IMInvoiceDetailReportSort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMInvoiceDetailReportSort)));
			this.InvoiceDetailReportSortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 24, true);
			this.InvoiceDetailReportSortDropEdit.Name = "InvoiceDetailReportSortDropEdit";
			this.InvoiceDetailReportSortDropEdit.PreBoundMaxLength = 3;
			this.InvoiceDetailReportSortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.InvoiceDetailReportSortDropEdit.TabIndex = 0;
			// 
			// WhsAutoPeriodicInvoiceGroupBox
			// 
			this.WhsAutoPeriodicInvoiceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsInvoicingUserControl|WhsAutoPeriodicInvoiceGroupBox", "Periodic Invoice");
			this.WhsAutoPeriodicInvoiceGroupBox.Controls.Add(this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox);
			this.WhsAutoPeriodicInvoiceGroupBox.Controls.Add(this.WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			this.WhsAutoPeriodicInvoiceGroupBox.Controls.Add(this.WhsAutoPostPeriodicInvoiceCheckBox);
			this.WhsAutoPeriodicInvoiceGroupBox.Controls.Add(this.WhsAutoDeliverPeriodicInvoiceCheckBox);
			this.WhsAutoPeriodicInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 330, true);
			this.WhsAutoPeriodicInvoiceGroupBox.Name = "WhsAutoPeriodicInvoiceGroupBox";
			this.WhsAutoPeriodicInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 101, true);
			this.WhsAutoPeriodicInvoiceGroupBox.TabIndex = 2;
			this.WhsAutoPeriodicInvoiceGroupBox.TabStop = false;
			// 
			// WhsAutoCreateAndRatePeriodicInvoiceCheckBox
			// 
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox, "CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice)));
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox.Name = "WhsAutoCreateAndRatePeriodicInvoiceCheckBox";
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.WhsAutoCreateAndRatePeriodicInvoiceCheckBox.TabIndex = 0;
			// 
			// WhsAllowCreateInvoiceWithNoTransactionsOrStock
			// 
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsAllowCreateInvoiceWithNoTransactionsOrStock, "CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock)));
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 21, true);
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock.Name = "WhsAllowCreateInvoiceWithNoTransactionsOrStock";
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 17, true);
			this.WhsAllowCreateInvoiceWithNoTransactionsOrStock.TabIndex = 1;
			// 
			// WhsAutoPostPeriodicInvoiceCheckBox
			// 
			this.WhsAutoPostPeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsAutoPostPeriodicInvoiceCheckBox, "CompanyData.OB_WhsAutoPostPeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsAutoPostPeriodicInvoice)));
			this.WhsAutoPostPeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 47, true);
			this.WhsAutoPostPeriodicInvoiceCheckBox.Name = "WhsAutoPostPeriodicInvoiceCheckBox";
			this.WhsAutoPostPeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
			this.WhsAutoPostPeriodicInvoiceCheckBox.TabIndex = 1;
			// 
			// WhsAutoDeliverPeriodicInvoiceCheckBox
			// 
			this.WhsAutoDeliverPeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsAutoDeliverPeriodicInvoiceCheckBox, "CompanyData.OB_WhsAutoDeliverPeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_WhsAutoDeliverPeriodicInvoice)));
			this.WhsAutoDeliverPeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 73, true);
			this.WhsAutoDeliverPeriodicInvoiceCheckBox.Name = "WhsAutoDeliverPeriodicInvoiceCheckBox";
			this.WhsAutoDeliverPeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 17, true);
			this.WhsAutoDeliverPeriodicInvoiceCheckBox.TabIndex = 2;
			// 
			// WhsInvoicingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WhsAutoPeriodicInvoiceGroupBox);
			this.Controls.Add(this.InvoiceDetailReportGroupBox);
			this.Controls.Add(this.InvoicingGroupBox);
			this.Controls.Add(this.AutoRatingGroupBox);
			this.Name = "WhsInvoicingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 565, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AutoRatingGroupBox.ResumeLayout(false);
			this.AutoRatingGroupBox.PerformLayout();
			this.WarehouseRatingPeriodDropEdit.ResumeLayout(true);
			this.WarehouseRatingPeriodDropEdit.PerformLayout();
			this.WhsStorageCalcMethodDropEdit.ResumeLayout(true);
			this.WhsStorageCalcMethodDropEdit.PerformLayout();
			this.InvoicingGroupBox.ResumeLayout(false);
			this.InvoicingGroupBox.PerformLayout();
			this.InvoiceDetailReportGroupBox.ResumeLayout(false);
			this.InvoiceDetailReportGroupBox.PerformLayout();
			this.InvoiceDetailReportSort3DropEdit.ResumeLayout(true);
			this.InvoiceDetailReportSort3DropEdit.PerformLayout();
			this.InvoiceDetailReportSort2DropEdit.ResumeLayout(true);
			this.InvoiceDetailReportSort2DropEdit.PerformLayout();
			this.InvoiceDetailReportSortDropEdit.ResumeLayout(true);
			this.InvoiceDetailReportSortDropEdit.PerformLayout();
			this.WhsAutoPeriodicInvoiceGroupBox.ResumeLayout(false);
			this.WhsAutoPeriodicInvoiceGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox AutoRatingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit WarehouseRatingPeriodDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox Attrib2IsKeyCheckBoxCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox Attrib1IsKeyCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit WhsStorageCalcMethodDropEdit;
		private ZGroupBox InvoicingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IncludeOutwardsWhsConsolidatedInvoiceCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IncludeInwardsWhsConsolidatedInvoiceCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox Attrib3IsKeyCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit FreeStorageDaysCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OverrideFreeStorageCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceDetailReportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceDetailReportSortDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceDetailReportSort3DropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceDetailReportSort2DropEdit;
		private ZCheckBox IncludeReleaseChargesOnShipmentCheckBox;
		private ZCheckBox ChargeStorageInAdvanceCheckBox;
		private ZGroupBox WhsAutoPeriodicInvoiceGroupBox;
		private ZCheckBox WhsAutoCreateAndRatePeriodicInvoiceCheckBox;
		private ZCheckBox WhsAllowCreateInvoiceWithNoTransactionsOrStock;
		private ZCheckBox WhsAutoPostPeriodicInvoiceCheckBox;
		private ZCheckBox WhsAutoDeliverPeriodicInvoiceCheckBox;
		private ZCheckBox SerialNumberIsKeyCheckBox;
	}
}
