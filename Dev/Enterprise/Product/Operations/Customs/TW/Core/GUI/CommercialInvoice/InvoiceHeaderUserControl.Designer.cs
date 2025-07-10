using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	partial class InvoiceHeaderUserControl
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
			this.TW_MarksAndNumbersLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.JZ_RemarksLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.JZ_IncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JZ_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JZ_LetterOfCreditNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JZ_LetterOfCreditDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupplierDocumentaryAddressControl = new Enterprise.Customs.TW.GUI.TWJobDocAddressControl();
			this.BuyerDocumentaryAddressControl = new Enterprise.Customs.TW.GUI.TWJobDocAddressControl();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountCurrencyControl.SuspendLayout();
			this.JZ_IncoTermDropDownEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TW_MarksAndNumbersLongTextBox.SuspendLayout();
			this.JZ_RemarksLongTextBox.SuspendLayout();
			this.JZ_LetterOfCreditDateDateEdit.SuspendLayout();
			this.SupplierDocumentaryAddressControl.SuspendLayout();
			this.BuyerDocumentaryAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// RightTabControl
			// 
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 599, true);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 577, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 577, true);
			// 
			// ordersAttachUserControl
			// 
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 577, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 513, true);
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 97, true);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 231, true);
			this.ImporterOrganisationControl.Visible = false;
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Visible = false;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 193, true);
			this.GrossWeightCalcDropEdit.TabIndex = 8;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 217, true);
			this.NetWeightCalcDropEdit.TabIndex = 9;
			// 
			// JZ_IncoTermDropDownEdit
			// 
			this.JZ_IncoTermDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 80, true);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 465, true);
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Visible = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.JZ_LetterOfCreditDateDateEdit);
			this.DetailsGroupBox.Controls.Add(this.JZ_LetterOfCreditNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.JZ_DescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.JZ_IncoTermPlaceTextBox);
			this.DetailsGroupBox.Controls.Add(this.JZ_RemarksLongTextBox);
			this.DetailsGroupBox.Controls.Add(this.TW_MarksAndNumbersLongTextBox);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 504, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.TW_MarksAndNumbersLongTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_RemarksLongTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_DescriptionTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_LetterOfCreditNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_LetterOfCreditDateDateEdit, 0);
			// 
			// InvCustomFieldsUserControl
			// 
			this.InvCustomFieldsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvCustomFieldsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvCustomFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 571, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// TW_MarksAndNumbersLongTextBox
			// 
			this.TW_MarksAndNumbersLongTextBox.AllowDrop = true;
			this.TW_MarksAndNumbersLongTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.BindingSource.SetBindingMember(this.TW_MarksAndNumbersLongTextBox, "Invoices.TW_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_MarksAndNumbers)));
			this.TW_MarksAndNumbersLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 241, true);
			this.TW_MarksAndNumbersLongTextBox.Name = "TW_MarksAndNumbersLongTextBox";
			this.TW_MarksAndNumbersLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.TW_MarksAndNumbersLongTextBox.TabIndex = 11;
			// 
			// JZ_RemarksLongTextBox
			// 
			this.JZ_RemarksLongTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_RemarksLongTextBox, "Invoices.JZ_Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Remarks)));
			this.JZ_RemarksLongTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JZ_RemarksLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 265, true);
			this.JZ_RemarksLongTextBox.Name = "JZ_RemarksLongTextBox";
			this.JZ_RemarksLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.JZ_RemarksLongTextBox.TabIndex = 13;
			// 
			// JZ_IncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_IncoTermPlaceTextBox, "Invoices.JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_IncoTermPlace)));
			this.JZ_IncoTermPlaceTextBox.CaptionResourceString = null;
			this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 169, true);
			this.JZ_IncoTermPlaceTextBox.Name = "JZ_IncoTermPlaceTextBox";
			this.JZ_IncoTermPlaceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JZ_IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.JZ_IncoTermPlaceTextBox.TabIndex = 7;
			// 
			// JZ_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_DescriptionTextBox, "Invoices.JZ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Description)));
			this.JZ_DescriptionTextBox.CaptionResourceString = null;
			this.JZ_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 289, true);
			this.JZ_DescriptionTextBox.Name = "JZ_DescriptionTextBox";
			this.JZ_DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JZ_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.JZ_DescriptionTextBox.TabIndex = 14;
			// 
			// JZ_LetterOfCreditNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_LetterOfCreditNumberTextBox, "Invoices.JZ_LetterOfCreditNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_LetterOfCreditNumber)));
			this.JZ_LetterOfCreditNumberTextBox.CaptionResourceString = null;
			this.JZ_LetterOfCreditNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 313, true);
			this.JZ_LetterOfCreditNumberTextBox.Name = "JZ_LetterOfCreditNumberTextBox";
			this.JZ_LetterOfCreditNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JZ_LetterOfCreditNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.JZ_LetterOfCreditNumberTextBox.TabIndex = 15;
			// 
			// JZ_LetterOfCreditDateDateEdit
			// 
			this.JZ_LetterOfCreditDateDateEdit.AllowDrop = true;
			this.JZ_LetterOfCreditDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JZ_LetterOfCreditDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JZ_LetterOfCreditDateDateEdit, "Invoices.JZ_LetterOfCreditDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_LetterOfCreditDate)));
			this.JZ_LetterOfCreditDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 337, true);
			this.JZ_LetterOfCreditDateDateEdit.Name = "JZ_LetterOfCreditDateDateEdit";
			this.JZ_LetterOfCreditDateDateEdit.TabIndex = 16;
			// 
			// SupplierDocumentaryAddressControl
			// 
			this.SupplierDocumentaryAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocumentaryAddressControl, "Invoices.SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TW.Business.TWJobDocAddress)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).SupplierDocumentaryAddress)));
			this.SupplierDocumentaryAddressControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocumentaryAddressControl.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e546f0ae-3890-4c26-aa13-91967ed1434a", "Supplier");
			this.SupplierDocumentaryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplierDocumentaryAddressControl.Name = "SupplierDocumentaryAddressControl";
			this.SupplierDocumentaryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 230, true);
			this.SupplierDocumentaryAddressControl.TabIndex = 0;
			// 
			// BuyerDocumentaryAddressControl
			// 
			this.BuyerDocumentaryAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerDocumentaryAddressControl, "Invoices.BuyerDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TW.Business.TWJobDocAddress)(((Enterprise.Customs.TW.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).BuyerDocumentaryAddress)));
			this.BuyerDocumentaryAddressControl.BindToOrganisations = "Lookups.ImportersList";
			this.BuyerDocumentaryAddressControl.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("11b4e350-5cda-4aaf-8fdb-a98e8ce3a679", "Buyer");
			this.BuyerDocumentaryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 231, true);
			this.BuyerDocumentaryAddressControl.Name = "BuyerDocumentaryAddressControl";
			this.BuyerDocumentaryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 230, true);
			this.BuyerDocumentaryAddressControl.TabIndex = 1;
			// 
			// InvoiceHeaderUserControl
			// 
			this.Controls.Add(this.BuyerDocumentaryAddressControl);
			this.Controls.Add(this.SupplierDocumentaryAddressControl);
			this.Name = "InvoiceHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 613, true);
			this.Controls.SetChildIndex(this.ChargesGroupBox, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.SupplierDocumentaryAddressControl, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.BuyerDocumentaryAddressControl, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ordersAttachUserControl.ResumeLayout(true);
			this.ordersAttachUserControl.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountCurrencyControl.PerformLayout();
			this.JZ_IncoTermDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermDropDownEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TW_MarksAndNumbersLongTextBox.ResumeLayout(true);
			this.TW_MarksAndNumbersLongTextBox.PerformLayout();
			this.JZ_RemarksLongTextBox.ResumeLayout(true);
			this.JZ_RemarksLongTextBox.PerformLayout();
			this.JZ_LetterOfCreditDateDateEdit.ResumeLayout(true);
			this.JZ_LetterOfCreditDateDateEdit.PerformLayout();
			this.SupplierDocumentaryAddressControl.ResumeLayout(true);
			this.SupplierDocumentaryAddressControl.PerformLayout();
			this.BuyerDocumentaryAddressControl.ResumeLayout(true);
			this.BuyerDocumentaryAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private LongTextControl JZ_RemarksLongTextBox;
		private ZTextBox JZ_IncoTermPlaceTextBox;
		internal LongTextControl TW_MarksAndNumbersLongTextBox;
		private ZTextBox JZ_DescriptionTextBox;
		private ZTextBox JZ_LetterOfCreditNumberTextBox;
		private ZDateEdit JZ_LetterOfCreditDateDateEdit;
		private TWJobDocAddressControl SupplierDocumentaryAddressControl;
		private TWJobDocAddressControl BuyerDocumentaryAddressControl;
	}
}
