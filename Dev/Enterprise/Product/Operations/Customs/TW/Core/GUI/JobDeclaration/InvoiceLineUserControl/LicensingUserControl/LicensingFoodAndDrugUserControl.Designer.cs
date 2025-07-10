
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingFoodAndDrugUserControl
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
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FoodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TW_SterilizationValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TW_PHValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_BarCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MedicalInstrumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MIFPartyIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizedPersonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MIFCertificateNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IngredientsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FoodDataGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StorageShippingConditionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StorageAndShippingConditionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FoodGroupBox.SuspendLayout();
			this.MedicalInstrumentGroupBox.SuspendLayout();
			this.MIFPartyIdentifierDropEdit.SuspendLayout();
			this.IngredientsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FoodDataGrid)).BeginInit();
			this.FoodDataGrid.SuspendLayout();
			this.StorageShippingConditionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StorageAndShippingConditionGrid)).BeginInit();
			this.StorageAndShippingConditionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// FoodGroupBox
			// 
			this.FoodGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("aa6ea1b0-8afd-4183-8ce9-6b53237bfbc2", "Food");
			this.FoodGroupBox.Controls.Add(this.TW_SterilizationValueCalcEdit);
			this.FoodGroupBox.Controls.Add(this.TW_PHValueCalcEdit);
			this.FoodGroupBox.Controls.Add(this.JI_BarCodeTextBox);
			this.FoodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.FoodGroupBox.Name = "FoodGroupBox";
			this.FoodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 94, true);
			this.FoodGroupBox.TabIndex = 1;
			this.FoodGroupBox.TabStop = false;
			// 
			// TW_SterilizationValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TW_SterilizationValueCalcEdit, "FilteredInvoiceLines.JI_SterilizationValueNumeric");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SterilizationValueNumeric)));
			this.TW_SterilizationValueCalcEdit.CaptionResourceString = null;
			this.TW_SterilizationValueCalcEdit.DecimalPlaces = 2;
			this.TW_SterilizationValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 66, true);
			this.TW_SterilizationValueCalcEdit.MaxValue = 99.9m;
			this.TW_SterilizationValueCalcEdit.Name = "TW_SterilizationValueCalcEdit";
			this.TW_SterilizationValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TW_SterilizationValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.TW_SterilizationValueCalcEdit.TabIndex = 2;
			this.TW_SterilizationValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TW_PHValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TW_PHValueCalcEdit, "FilteredInvoiceLines.JI_PHValueNumeric");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PHValueNumeric)));
			this.TW_PHValueCalcEdit.CaptionResourceString = null;
			this.TW_PHValueCalcEdit.DecimalPlaces = 2;
			this.TW_PHValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 41, true);
			this.TW_PHValueCalcEdit.MaxValue = 99.9m;
			this.TW_PHValueCalcEdit.Name = "TW_PHValueCalcEdit";
			this.TW_PHValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TW_PHValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.TW_PHValueCalcEdit.TabIndex = 1;
			this.TW_PHValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_BarCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_BarCodeTextBox, "FilteredInvoiceLines.JI_BarCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BarCode)));
			this.JI_BarCodeTextBox.CaptionResourceString = null;
			this.JI_BarCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 16, true);
			this.JI_BarCodeTextBox.Name = "JI_BarCodeTextBox";
			this.JI_BarCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JI_BarCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JI_BarCodeTextBox.TabIndex = 0;
			// 
			// MedicalInstrumentGroupBox
			// 
			this.MedicalInstrumentGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5a422f5e-a625-4548-9fd3-c9a6659c887f", "Medical Instrument");
			this.MedicalInstrumentGroupBox.Controls.Add(this.MIFPartyIdentifierDropEdit);
			this.MedicalInstrumentGroupBox.Controls.Add(this.AuthorizedPersonTextBox);
			this.MedicalInstrumentGroupBox.Controls.Add(this.MIFCertificateNoTextBox);
			this.MedicalInstrumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.MedicalInstrumentGroupBox.Name = "MedicalInstrumentGroupBox";
			this.MedicalInstrumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 95, true);
			this.MedicalInstrumentGroupBox.TabIndex = 0;
			this.MedicalInstrumentGroupBox.TabStop = false;
			// 
			// MIFPartyIdentifierDropEdit
			// 
			this.MIFPartyIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MIFPartyIdentifierDropEdit, "FilteredInvoiceLines.PartyIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PartyIdentifier)));
			this.MIFPartyIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 68, true);
			this.MIFPartyIdentifierDropEdit.Name = "MIFPartyIdentifierDropEdit";
			this.MIFPartyIdentifierDropEdit.PreBoundMaxLength = 3;
			this.MIFPartyIdentifierDropEdit.ShouldResizeByMaxLength = false;
			this.MIFPartyIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.MIFPartyIdentifierDropEdit.TabIndex = 2;
			// 
			// AuthorizedPersonTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorizedPersonTextBox, "FilteredInvoiceLines.AuthorizedPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AuthorizedPerson)));
			this.AuthorizedPersonTextBox.CaptionResourceString = null;
			this.AuthorizedPersonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 43, true);
			this.AuthorizedPersonTextBox.Name = "AuthorizedPersonTextBox";
			this.AuthorizedPersonTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AuthorizedPersonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.AuthorizedPersonTextBox.TabIndex = 1;
			// 
			// MIFCertificateNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.MIFCertificateNoTextBox, "FilteredInvoiceLines.CertificateNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateNo)));
			this.MIFCertificateNoTextBox.CaptionResourceString = null;
			this.MIFCertificateNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 18, true);
			this.MIFCertificateNoTextBox.Name = "MIFCertificateNoTextBox";
			this.MIFCertificateNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MIFCertificateNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.MIFCertificateNoTextBox.TabIndex = 0;
			// 
			// IngredientsGroupBox
			// 
			this.IngredientsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("85543618-b93c-4f31-8d43-86fb0aa6c889", "Ingredients");
			this.IngredientsGroupBox.Controls.Add(this.FoodDataGrid);
			this.IngredientsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 4, true);
			this.IngredientsGroupBox.Name = "IngredientsGroupBox";
			this.IngredientsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 280, true);
			this.IngredientsGroupBox.TabIndex = 2;
			this.IngredientsGroupBox.TabStop = false;
			// 
			// FoodDataGrid
			// 
			this.FoodDataGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FoodDataGrid, "FilteredInvoiceLines.FoodDataCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).FoodDataCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.FoodData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).FoodDataCollection)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.FoodData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).FoodDataCollection)).SyncRoot)).Content)));
			this.FoodDataGrid.CaptionVisible = false;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CY_Data";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Content";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.FoodDataGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.FoodDataGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FoodDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FoodDataGrid.GridId = "bea03b85-5598-4301-a16e-7f28dfe11c26";
			this.FoodDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FoodDataGrid.LayoutKey = "FoodDataGrid";
			this.FoodDataGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FoodDataGrid.MaximumRows = 99;
			this.FoodDataGrid.Name = "FoodDataGrid";
			this.FoodDataGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 261, true);
			this.FoodDataGrid.TabIndex = 0;
			// 
			// StorageShippingConditionGroupBox
			// 
			this.StorageShippingConditionGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("1e961bf1-2a01-461f-a10f-95a1e3eebbc8", "Storage and Shipping Conditions");
			this.StorageShippingConditionGroupBox.Controls.Add(this.StorageAndShippingConditionGrid);
			this.StorageShippingConditionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 4, true);
			this.StorageShippingConditionGroupBox.Name = "StorageShippingConditionGroupBox";
			this.StorageShippingConditionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 278, true);
			this.StorageShippingConditionGroupBox.TabIndex = 3;
			this.StorageShippingConditionGroupBox.TabStop = false;
			// 
			// StorageAndShippingConditionGrid
			// 
			this.StorageAndShippingConditionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StorageAndShippingConditionGrid, "FilteredInvoiceLines.StorageAndShippingConditionJobComInvLineRefsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).StorageAndShippingConditionJobComInvLineRefsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.StorageAndShippingConditionJobComInvLineRefs)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).StorageAndShippingConditionJobComInvLineRefsCollection)).SyncRoot)).JG_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.StorageAndShippingConditionJobComInvLineRefs)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).StorageAndShippingConditionJobComInvLineRefsCollection)).SyncRoot)).Description)));
			this.StorageAndShippingConditionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "JG_ReferenceNumber";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.StorageAndShippingConditionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.StorageAndShippingConditionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StorageAndShippingConditionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StorageAndShippingConditionGrid.GridId = "b8fa3452-b658-4ffd-b928-376d29e563f0";
			this.StorageAndShippingConditionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StorageAndShippingConditionGrid.LayoutKey = "StorageAndShippingConditionGrid";
			this.StorageAndShippingConditionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StorageAndShippingConditionGrid.MaximumRows = 9;
			this.StorageAndShippingConditionGrid.Name = "StorageAndShippingConditionGrid";
			this.StorageAndShippingConditionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 259, true);
			this.StorageAndShippingConditionGrid.TabIndex = 0;
			// 
			// LicensingFoodAndDrugUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StorageShippingConditionGroupBox);
			this.Controls.Add(this.IngredientsGroupBox);
			this.Controls.Add(this.FoodGroupBox);
			this.Controls.Add(this.MedicalInstrumentGroupBox);
			this.Name = "LicensingFoodAndDrugUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1017, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FoodGroupBox.ResumeLayout(false);
			this.FoodGroupBox.PerformLayout();
			this.MedicalInstrumentGroupBox.ResumeLayout(false);
			this.MedicalInstrumentGroupBox.PerformLayout();
			this.MIFPartyIdentifierDropEdit.ResumeLayout(true);
			this.MIFPartyIdentifierDropEdit.PerformLayout();
			this.IngredientsGroupBox.ResumeLayout(false);
			this.IngredientsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FoodDataGrid)).EndInit();
			this.FoodDataGrid.ResumeLayout(false);
			this.FoodDataGrid.PerformLayout();
			this.StorageShippingConditionGroupBox.ResumeLayout(false);
			this.StorageShippingConditionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StorageAndShippingConditionGrid)).EndInit();
			this.StorageAndShippingConditionGrid.ResumeLayout(false);
			this.StorageAndShippingConditionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FoodGroupBox;
		private ZArchitecture.GUI.ZGroupBox MedicalInstrumentGroupBox;
		private ZArchitecture.ZTextBox MIFCertificateNoTextBox;
		private ZArchitecture.ZTextBox AuthorizedPersonTextBox;
		private ZArchitecture.GUI.ZDropEdit MIFPartyIdentifierDropEdit;
		private ZArchitecture.ZTextBox JI_BarCodeTextBox;
		internal ZArchitecture.ZCalcEdit TW_PHValueCalcEdit;
		internal ZArchitecture.ZCalcEdit TW_SterilizationValueCalcEdit;
		private ZArchitecture.GUI.ZGroupBox IngredientsGroupBox;
		private ZArchitecture.GUI.ZGroupBox StorageShippingConditionGroupBox;
		private ZArchitecture.ZGrid FoodDataGrid;
		internal ZArchitecture.ZGrid StorageAndShippingConditionGrid;
	}
}
