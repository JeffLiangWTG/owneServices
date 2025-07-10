namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingCertificateOfOriginUserControl
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
			this.PermitUnitPriceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.OriginCriteriaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreferentialTreatmentCriteriaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OtherCriteriaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffPrintingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_IMPTariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerRelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShippingMarksLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.PermitGoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitGoodsDescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OverridePermitGoodsDescriptionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PermitGoodsDescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PermitQuantityUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomPermitUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_InnerPackDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PermitUnitPriceCalcFindBox.SuspendLayout();
			this.OriginCriteriaDropEdit.SuspendLayout();
			this.PreferentialTreatmentCriteriaDropEdit.SuspendLayout();
			this.OtherCriteriaDropEdit.SuspendLayout();
			this.TariffPrintingDropEdit.SuspendLayout();
			this.ManufacturerRelationshipDropEdit.SuspendLayout();
			this.ShippingMarksLongTextControl.SuspendLayout();
			this.PermitGoodsDescriptionPanel.SuspendLayout();
			this.PermitGoodsDescriptionGroupBox.SuspendLayout();
			this.PermitQuantityUnitDropEdit.SuspendLayout();
			this.PackagingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// PermitUnitPriceCalcFindBox
			// 
			this.PermitUnitPriceCalcFindBox.AllowDrop = true;
			this.PermitUnitPriceCalcFindBox.BindToAmount = "FilteredInvoiceLines.JI_PermitUnitPrice";
			this.PermitUnitPriceCalcFindBox.BindToUnit = "FilteredInvoiceLines.JI_PermitUnitPriceCurrency";
			this.PermitUnitPriceCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PermitUnitPriceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 21, true);
			this.PermitUnitPriceCalcFindBox.Name = "PermitUnitPriceCalcFindBox";
			this.PermitUnitPriceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PermitUnitPriceCalcFindBox.TabIndex = 0;
			// 
			// OriginCriteriaDropEdit
			// 
			this.OriginCriteriaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCriteriaDropEdit, "FilteredInvoiceLines.JI_OriginCriteria");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OriginCriteria)));
			this.OriginCriteriaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 95, true);
			this.OriginCriteriaDropEdit.Name = "OriginCriteriaDropEdit";
			this.OriginCriteriaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OriginCriteriaDropEdit.TabIndex = 2;
			// 
			// PreferentialTreatmentCriteriaDropEdit
			// 
			this.PreferentialTreatmentCriteriaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferentialTreatmentCriteriaDropEdit, "FilteredInvoiceLines.JI_PTCriteria");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PTCriteria)));
			this.PreferentialTreatmentCriteriaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 118, true);
			this.PreferentialTreatmentCriteriaDropEdit.Name = "PreferentialTreatmentCriteriaDropEdit";
			this.PreferentialTreatmentCriteriaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PreferentialTreatmentCriteriaDropEdit.TabIndex = 3;
			// 
			// OtherCriteriaDropEdit
			// 
			this.OtherCriteriaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OtherCriteriaDropEdit, "FilteredInvoiceLines.JI_PTCriteria2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PTCriteria2)));
			this.OtherCriteriaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 140, true);
			this.OtherCriteriaDropEdit.Name = "OtherCriteriaDropEdit";
			this.OtherCriteriaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OtherCriteriaDropEdit.TabIndex = 4;
			// 
			// TariffPrintingDropEdit
			// 
			this.TariffPrintingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffPrintingDropEdit, "FilteredInvoiceLines.JI_TariffPrintLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TariffPrintLength)));
			this.TariffPrintingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 186, true);
			this.TariffPrintingDropEdit.Name = "TariffPrintingDropEdit";
			this.TariffPrintingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TariffPrintingDropEdit.TabIndex = 6;
			// 
			// JI_IMPTariffTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_IMPTariffTextBox, "FilteredInvoiceLines.JI_IMPTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IMPTariff)));
			this.JI_IMPTariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 208, true);
			this.JI_IMPTariffTextBox.Name = "JI_IMPTariffTextBox";
			this.JI_IMPTariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JI_IMPTariffTextBox.TabIndex = 7;
			// 
			// ManufacturerRelationshipDropEdit
			// 
			this.ManufacturerRelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerRelationshipDropEdit, "FilteredInvoiceLines.JI_ManufacturerRelationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ManufacturerRelationship)));
			this.ManufacturerRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 163, true);
			this.ManufacturerRelationshipDropEdit.Name = "ManufacturerRelationshipDropEdit";
			this.ManufacturerRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ManufacturerRelationshipDropEdit.TabIndex = 5;
			// 
			// ShippingMarksLongTextControl
			// 
			this.ShippingMarksLongTextControl.AllowDrop = true;
			this.ShippingMarksLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ShippingMarksLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 21, true);
			this.ShippingMarksLongTextControl.Name = "ShippingMarksLongTextControl";
			this.ShippingMarksLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ShippingMarksLongTextControl.TabIndex = 8;
			// 
			// PermitGoodsDescriptionTextBox
			// 
			this.PermitGoodsDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PermitGoodsDescriptionTextBox, "FilteredInvoiceLines.NX101PermitGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NX101PermitGoodsDescription)));
			this.PermitGoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 31, true);
			this.PermitGoodsDescriptionTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.PermitGoodsDescriptionTextBox.Multiline = true;
			this.PermitGoodsDescriptionTextBox.Name = "PermitGoodsDescriptionTextBox";
			this.PermitGoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 164, true);
			this.PermitGoodsDescriptionTextBox.TabIndex = 1;
			// 
			// PermitGoodsDescriptionPanel
			// 
			this.PermitGoodsDescriptionPanel.Controls.Add(this.PermitGoodsDescriptionTextBox);
			this.PermitGoodsDescriptionPanel.Controls.Add(this.OverridePermitGoodsDescriptionCheckBox);
			this.PermitGoodsDescriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitGoodsDescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PermitGoodsDescriptionPanel.Name = "PermitGoodsDescriptionPanel";
			this.PermitGoodsDescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 200, true);
			this.PermitGoodsDescriptionPanel.TabIndex = 0;
			// 
			// OverridePermitGoodsDescriptionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverridePermitGoodsDescriptionCheckBox, "FilteredInvoiceLines.OverrideNX101PermitGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OverrideNX101PermitGoodsDescription)));
			this.OverridePermitGoodsDescriptionCheckBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("a8095f30-66d1-4fe5-aa10-709fa80aad83", "Override Default");
			this.OverridePermitGoodsDescriptionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 5, true);
			this.OverridePermitGoodsDescriptionCheckBox.Name = "OverridePermitGoodsDescriptionCheckBox";
			this.OverridePermitGoodsDescriptionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.OverridePermitGoodsDescriptionCheckBox.TabIndex = 0;
			// 
			// PermitGoodsDescriptionGroupBox
			// 
			this.PermitGoodsDescriptionGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("56d9e62c-da9f-4f76-a474-3e402c24b880", "Permit Goods Description");
			this.PermitGoodsDescriptionGroupBox.Controls.Add(this.PermitGoodsDescriptionPanel);
			this.PermitGoodsDescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 53, true);
			this.PermitGoodsDescriptionGroupBox.Name = "PermitGoodsDescriptionGroupBox";
			this.PermitGoodsDescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 219, true);
			this.PermitGoodsDescriptionGroupBox.TabIndex = 9;
			this.PermitGoodsDescriptionGroupBox.TabStop = false;
			// 
			// PermitQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PermitQuantityCalcEdit, "FilteredInvoiceLines.JI_PermitQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PermitQty)));
			this.PermitQuantityCalcEdit.DecimalPlaces = 2;
			this.PermitQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 45, true);
			this.PermitQuantityCalcEdit.Name = "PermitQuantityCalcEdit";
			this.PermitQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PermitQuantityCalcEdit.TabIndex = 10;
			this.PermitQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PermitQuantityCalcEdit.TrackDisposedAccess = true;
			// 
			// PermitQuantityUnitDropEdit
			// 
			this.PermitQuantityUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitQuantityUnitDropEdit, "FilteredInvoiceLines.JI_PermitUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PermitUQ)));
			this.PermitQuantityUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 45, true);
			this.PermitQuantityUnitDropEdit.Name = "PermitQuantityUnitDropEdit";
			this.PermitQuantityUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.PermitQuantityUnitDropEdit.TabIndex = 11;
			// 
			// CustomPermitUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomPermitUQTextBox, "FilteredInvoiceLines.JI_CustomPermitUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomPermitUQ)));
			this.CustomPermitUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 69, true);
			this.CustomPermitUQTextBox.Name = "CustomPermitUQTextBox";
			this.CustomPermitUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomPermitUQTextBox.TabIndex = 12;
			// 
			// PackagingGroupBox
			// 
			this.PackagingGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("790ea258-f4d2-432b-bf58-2280a389db5e", "Packaging Description");
			this.PackagingGroupBox.Controls.Add(this.JI_InnerPackDescriptionTextBox);
			this.PackagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(788, 53, true);
			this.PackagingGroupBox.Name = "PackagingGroupBox";
			this.PackagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 219, true);
			this.PackagingGroupBox.TabIndex = 13;
			this.PackagingGroupBox.TabStop = false;
			// 
			// JI_InnerPackDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_InnerPackDescriptionTextBox, "FilteredInvoiceLines.JI_InnerPackDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InnerPackDescription)));
			this.JI_InnerPackDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 23, true);
			this.JI_InnerPackDescriptionTextBox.Multiline = true;
			this.JI_InnerPackDescriptionTextBox.Name = "JI_InnerPackDescriptionTextBox";
			this.JI_InnerPackDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 189, true);
			this.JI_InnerPackDescriptionTextBox.TabIndex = 2;
			// 
			// LicensingCertificateOfOriginUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackagingGroupBox);
			this.Controls.Add(this.CustomPermitUQTextBox);
			this.Controls.Add(this.PermitQuantityUnitDropEdit);
			this.Controls.Add(this.PermitQuantityCalcEdit);
			this.Controls.Add(this.ShippingMarksLongTextControl);
			this.Controls.Add(this.ManufacturerRelationshipDropEdit);
			this.Controls.Add(this.PermitUnitPriceCalcFindBox);
			this.Controls.Add(this.OriginCriteriaDropEdit);
			this.Controls.Add(this.PreferentialTreatmentCriteriaDropEdit);
			this.Controls.Add(this.OtherCriteriaDropEdit);
			this.Controls.Add(this.TariffPrintingDropEdit);
			this.Controls.Add(this.JI_IMPTariffTextBox);
			this.Controls.Add(this.PermitGoodsDescriptionGroupBox);
			this.Name = "LicensingCertificateOfOriginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PermitUnitPriceCalcFindBox.ResumeLayout(true);
			this.PermitUnitPriceCalcFindBox.PerformLayout();
			this.OriginCriteriaDropEdit.ResumeLayout(true);
			this.OriginCriteriaDropEdit.PerformLayout();
			this.PreferentialTreatmentCriteriaDropEdit.ResumeLayout(true);
			this.PreferentialTreatmentCriteriaDropEdit.PerformLayout();
			this.OtherCriteriaDropEdit.ResumeLayout(true);
			this.OtherCriteriaDropEdit.PerformLayout();
			this.TariffPrintingDropEdit.ResumeLayout(true);
			this.TariffPrintingDropEdit.PerformLayout();
			this.ManufacturerRelationshipDropEdit.ResumeLayout(true);
			this.ManufacturerRelationshipDropEdit.PerformLayout();
			this.ShippingMarksLongTextControl.ResumeLayout(true);
			this.ShippingMarksLongTextControl.PerformLayout();
			this.PermitGoodsDescriptionPanel.ResumeLayout(false);
			this.PermitGoodsDescriptionPanel.PerformLayout();
			this.PermitGoodsDescriptionGroupBox.ResumeLayout(false);
			this.PermitGoodsDescriptionGroupBox.PerformLayout();
			this.PermitQuantityUnitDropEdit.ResumeLayout(true);
			this.PermitQuantityUnitDropEdit.PerformLayout();
			this.PackagingGroupBox.ResumeLayout(false);
			this.PackagingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCalcFindBox PermitUnitPriceCalcFindBox;
		private ZArchitecture.GUI.ZDropEdit OriginCriteriaDropEdit;
		private ZArchitecture.GUI.ZDropEdit PreferentialTreatmentCriteriaDropEdit;
		private ZArchitecture.GUI.ZDropEdit OtherCriteriaDropEdit;
		private ZArchitecture.GUI.ZDropEdit TariffPrintingDropEdit;
		private ZArchitecture.ZTextBox JI_IMPTariffTextBox;
		private ZArchitecture.GUI.ZDropEdit ManufacturerRelationshipDropEdit;
		private Customs.GUI.LongTextControl ShippingMarksLongTextControl;
		private ZArchitecture.ZTextBox PermitGoodsDescriptionTextBox;
		private ZArchitecture.GUI.ZPanel PermitGoodsDescriptionPanel;
		private ZArchitecture.GUI.ZGroupBox PermitGoodsDescriptionGroupBox;
		private ZArchitecture.GUI.ZCheckBox OverridePermitGoodsDescriptionCheckBox;
		private ZArchitecture.ZCalcEdit PermitQuantityCalcEdit;
		private ZArchitecture.GUI.ZDropEdit PermitQuantityUnitDropEdit;
		private ZArchitecture.ZTextBox CustomPermitUQTextBox;
		private ZArchitecture.GUI.ZGroupBox PackagingGroupBox;
		private ZArchitecture.ZTextBox JI_InnerPackDescriptionTextBox;
	}
}
