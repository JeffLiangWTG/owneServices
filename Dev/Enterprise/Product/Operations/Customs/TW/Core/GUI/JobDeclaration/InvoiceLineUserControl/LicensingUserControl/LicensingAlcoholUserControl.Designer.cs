
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingAlcoholUserControl
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
			this.AlcoholCountryRegionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_AlcoholCountryRegionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VolumesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_AlteredLotNoAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RemovedLotNoAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_NoOriginalLotNoAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_BottledDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JI_ExpirationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JI_AlcoholEndOfShelfLifeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JI_AlcoholAgeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AlcoholYearYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.JI_AlcoholPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AlcoholCountryRegionGroupBox.SuspendLayout();
			this.VolumesGroupBox.SuspendLayout();
			this.JI_BottledDateDateEdit.SuspendLayout();
			this.JI_ExpirationDateDateEdit.SuspendLayout();
			this.JI_AlcoholEndOfShelfLifeDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// AlcoholCountryRegionGroupBox
			// 
			this.AlcoholCountryRegionGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("93cdf244-77de-483c-8fb8-02574979a8c1", "Geographical Indication");
			this.AlcoholCountryRegionGroupBox.Controls.Add(this.JI_AlcoholCountryRegionTextBox);
			this.AlcoholCountryRegionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.AlcoholCountryRegionGroupBox.Name = "AlcoholCountryRegionGroupBox";
			this.AlcoholCountryRegionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 282, true);
			this.AlcoholCountryRegionGroupBox.TabIndex = 0;
			this.AlcoholCountryRegionGroupBox.TabStop = false;
			// 
			// JI_AlcoholCountryRegionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_AlcoholCountryRegionTextBox, "FilteredInvoiceLines.JI_AlcoholCountryRegion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholCountryRegion)));
			this.JI_AlcoholCountryRegionTextBox.CaptionResourceString = null;
			this.JI_AlcoholCountryRegionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JI_AlcoholCountryRegionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JI_AlcoholCountryRegionTextBox.Multiline = true;
			this.JI_AlcoholCountryRegionTextBox.Name = "JI_AlcoholCountryRegionTextBox";
			this.JI_AlcoholCountryRegionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JI_AlcoholCountryRegionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 263, true);
			this.JI_AlcoholCountryRegionTextBox.TabIndex = 0;
			// 
			// VolumesGroupBox
			// 
			this.VolumesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("10920159-7692-49ad-a7c9-ab9cb0ce4567", "Volumes(LTR)");
			this.VolumesGroupBox.Controls.Add(this.JI_AlteredLotNoAmtCalcEdit);
			this.VolumesGroupBox.Controls.Add(this.RemovedLotNoAmtCalcEdit);
			this.VolumesGroupBox.Controls.Add(this.JI_NoOriginalLotNoAmtCalcEdit);
			this.VolumesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 2, true);
			this.VolumesGroupBox.Name = "VolumesGroupBox";
			this.VolumesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 98, true);
			this.VolumesGroupBox.TabIndex = 1;
			this.VolumesGroupBox.TabStop = false;
			// 
			// JI_AlteredLotNoAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AlteredLotNoAmtCalcEdit, "FilteredInvoiceLines.JI_AlteredLotNoAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlteredLotNoAmt)));
			this.JI_AlteredLotNoAmtCalcEdit.CaptionResourceString = null;
			this.JI_AlteredLotNoAmtCalcEdit.DecimalPlaces = 4;
			this.JI_AlteredLotNoAmtCalcEdit.Decimals = 4;
			this.JI_AlteredLotNoAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 68, true);
			this.JI_AlteredLotNoAmtCalcEdit.Name = "JI_AlteredLotNoAmtCalcEdit";
			this.JI_AlteredLotNoAmtCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_AlteredLotNoAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.JI_AlteredLotNoAmtCalcEdit.TabIndex = 2;
			this.JI_AlteredLotNoAmtCalcEdit.Text = "0.0000";
			this.JI_AlteredLotNoAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RemovedLotNoAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RemovedLotNoAmtCalcEdit, "FilteredInvoiceLines.JI_RemovedLotNoAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RemovedLotNoAmt)));
			this.RemovedLotNoAmtCalcEdit.CaptionResourceString = null;
			this.RemovedLotNoAmtCalcEdit.DecimalPlaces = 4;
			this.RemovedLotNoAmtCalcEdit.Decimals = 4;
			this.RemovedLotNoAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 43, true);
			this.RemovedLotNoAmtCalcEdit.Name = "RemovedLotNoAmtCalcEdit";
			this.RemovedLotNoAmtCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.RemovedLotNoAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.RemovedLotNoAmtCalcEdit.TabIndex = 1;
			this.RemovedLotNoAmtCalcEdit.Text = "0.0000";
			this.RemovedLotNoAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_NoOriginalLotNoAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_NoOriginalLotNoAmtCalcEdit, "FilteredInvoiceLines.JI_NoOriginalLotNoAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NoOriginalLotNoAmt)));
			this.JI_NoOriginalLotNoAmtCalcEdit.CaptionResourceString = null;
			this.JI_NoOriginalLotNoAmtCalcEdit.DecimalPlaces = 4;
			this.JI_NoOriginalLotNoAmtCalcEdit.Decimals = 4;
			this.JI_NoOriginalLotNoAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 18, true);
			this.JI_NoOriginalLotNoAmtCalcEdit.Name = "JI_NoOriginalLotNoAmtCalcEdit";
			this.JI_NoOriginalLotNoAmtCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_NoOriginalLotNoAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.JI_NoOriginalLotNoAmtCalcEdit.TabIndex = 0;
			this.JI_NoOriginalLotNoAmtCalcEdit.Text = "0.0000";
			this.JI_NoOriginalLotNoAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_BottledDateDateEdit
			// 
			this.JI_BottledDateDateEdit.AllowDrop = true;
			this.JI_BottledDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_BottledDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_BottledDateDateEdit, "FilteredInvoiceLines.JI_BottledDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BottledDate)));
			this.JI_BottledDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 20, true);
			this.JI_BottledDateDateEdit.Name = "JI_BottledDateDateEdit";
			this.JI_BottledDateDateEdit.TabIndex = 2;
			// 
			// JI_ExpirationDateDateEdit
			// 
			this.JI_ExpirationDateDateEdit.AllowDrop = true;
			this.JI_ExpirationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_ExpirationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_ExpirationDateDateEdit, "FilteredInvoiceLines.JI_ExpirationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ExpirationDate)));
			this.JI_ExpirationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 45, true);
			this.JI_ExpirationDateDateEdit.Name = "JI_ExpirationDateDateEdit";
			this.JI_ExpirationDateDateEdit.TabIndex = 3;
			// 
			// JI_AlcoholEndOfShelfLifeDateEdit
			// 
			this.JI_AlcoholEndOfShelfLifeDateEdit.AllowDrop = true;
			this.JI_AlcoholEndOfShelfLifeDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_AlcoholEndOfShelfLifeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_AlcoholEndOfShelfLifeDateEdit, "FilteredInvoiceLines.JI_AlcoholEndOfShelfLife");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholEndOfShelfLife)));
			this.JI_AlcoholEndOfShelfLifeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 70, true);
			this.JI_AlcoholEndOfShelfLifeDateEdit.Name = "JI_AlcoholEndOfShelfLifeDateEdit";
			this.JI_AlcoholEndOfShelfLifeDateEdit.TabIndex = 4;
			// 
			// JI_AlcoholAgeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AlcoholAgeCalcEdit, "FilteredInvoiceLines.JI_AlcoholAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholAge)));
			this.JI_AlcoholAgeCalcEdit.CaptionResourceString = null;
			this.JI_AlcoholAgeCalcEdit.DecimalPlaces = 0;
			this.JI_AlcoholAgeCalcEdit.Decimals = 0;
			this.JI_AlcoholAgeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 95, true);
			this.JI_AlcoholAgeCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.JI_AlcoholAgeCalcEdit.Name = "JI_AlcoholAgeCalcEdit";
			this.JI_AlcoholAgeCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_AlcoholAgeCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_AlcoholAgeCalcEdit.ShowGroupSeparators = false;
			this.JI_AlcoholAgeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.JI_AlcoholAgeCalcEdit.TabIndex = 5;
			this.JI_AlcoholAgeCalcEdit.Text = "0";
			this.JI_AlcoholAgeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_AlcoholYearYearEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AlcoholYearYearEdit, "FilteredInvoiceLines.JI_AlcoholYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholYear)));
			this.JI_AlcoholYearYearEdit.CaptionResourceString = null;
			this.JI_AlcoholYearYearEdit.DecimalPlaces = 0;
			this.JI_AlcoholYearYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 120, true);
			this.JI_AlcoholYearYearEdit.Name = "JI_AlcoholYearYearEdit";
			this.JI_AlcoholYearYearEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_AlcoholYearYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.JI_AlcoholYearYearEdit.TabIndex = 6;
			this.JI_AlcoholYearYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_AlcoholPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AlcoholPercentageCalcEdit, "FilteredInvoiceLines.JI_AlcoholPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholPercentage)));
			this.JI_AlcoholPercentageCalcEdit.CaptionResourceString = null;
			this.JI_AlcoholPercentageCalcEdit.DecimalPlaces = 3;
			this.JI_AlcoholPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(774, 146, true);
			this.JI_AlcoholPercentageCalcEdit.Name = "JI_AlcoholPercentageCalcEdit";
			this.JI_AlcoholPercentageCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_AlcoholPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.JI_AlcoholPercentageCalcEdit.TabIndex = 7;
			this.JI_AlcoholPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_AlcoholPercentageCalcEdit.MaxValue = 999.999m;
			this.JI_AlcoholPercentageCalcEdit.AllowNegative = false;
			// 
			// LicensingAlcoholUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JI_AlcoholPercentageCalcEdit);
			this.Controls.Add(this.JI_AlcoholYearYearEdit);
			this.Controls.Add(this.JI_AlcoholAgeCalcEdit);
			this.Controls.Add(this.JI_AlcoholEndOfShelfLifeDateEdit);
			this.Controls.Add(this.JI_ExpirationDateDateEdit);
			this.Controls.Add(this.JI_BottledDateDateEdit);
			this.Controls.Add(this.VolumesGroupBox);
			this.Controls.Add(this.AlcoholCountryRegionGroupBox);
			this.Name = "LicensingAlcoholUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AlcoholCountryRegionGroupBox.ResumeLayout(false);
			this.AlcoholCountryRegionGroupBox.PerformLayout();
			this.VolumesGroupBox.ResumeLayout(false);
			this.VolumesGroupBox.PerformLayout();
			this.JI_BottledDateDateEdit.ResumeLayout(true);
			this.JI_BottledDateDateEdit.PerformLayout();
			this.JI_ExpirationDateDateEdit.ResumeLayout(true);
			this.JI_ExpirationDateDateEdit.PerformLayout();
			this.JI_AlcoholEndOfShelfLifeDateEdit.ResumeLayout(true);
			this.JI_AlcoholEndOfShelfLifeDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AlcoholCountryRegionGroupBox;
		private ZArchitecture.ZTextBox JI_AlcoholCountryRegionTextBox;
		private ZArchitecture.GUI.ZGroupBox VolumesGroupBox;
		private ZArchitecture.ZCalcEdit JI_NoOriginalLotNoAmtCalcEdit;
		private ZArchitecture.ZCalcEdit RemovedLotNoAmtCalcEdit;
		private ZArchitecture.ZCalcEdit JI_AlteredLotNoAmtCalcEdit;
		private ZArchitecture.GUI.ZDateEdit JI_BottledDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit JI_ExpirationDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit JI_AlcoholEndOfShelfLifeDateEdit;
		private ZArchitecture.ZCalcEdit JI_AlcoholAgeCalcEdit;
		private ZArchitecture.GUI.ZYearEdit JI_AlcoholYearYearEdit;
		private ZArchitecture.ZCalcEdit JI_AlcoholPercentageCalcEdit;
	}
}
