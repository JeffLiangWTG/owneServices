
namespace Enterprise.Customs.NL.GUI
{
	partial class SupportingDocumentsFieldsControl
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
			this.CSI_ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
			this.SupDocTypeCodeFindBox.SuspendLayout();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.SupDocReferenceCodeFindBox.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_UnitOfQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CSI_RX_NKCurrencyCodeFindBox
			// 
			this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 86, true);
			// 
			// SupDocQuantityCalcEdit
			// 
			this.SupDocQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
			// 
			// CSI_UnitOfQuantity2TextBox
			// 
			this.CSI_UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 108, true);
			// 
			// CSI_ValueCalcEdit
			// 
			this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 86, true);
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 108, true);
			// 
			// CSI_UnitOfQuantityTextBox
			// 
			this.CSI_UnitOfQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 64, true);
			// 
			// SupportingDocumentsGroupBox
			//
			this.SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("4734E82A-C1D9-4710-A0FB-EF8B1F592D2E", "[UCC 2/3] Supporting documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ItemNumberCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumber2TextBox);
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 205, true);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_StatusDropEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_DateOfIssueDateEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReferenceCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_DateOfExpiryDateEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocQuantityCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_Quantity2CalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantityTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_RX_NKCurrencyCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReferenceTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantity2TextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_ValueCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocTypeCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_ReferenceNumber2TextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantityDropEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_ItemNumberCalcEdit, 0);
			// 
			// CSI_ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CSI_ItemNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
			this.CSI_ItemNumberCalcEdit.DecimalPlaces = 2;
			this.CSI_ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 152, true);
			this.CSI_ItemNumberCalcEdit.Name = "CSI_ItemNumberCalcEdit";
			this.CSI_ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.CSI_ItemNumberCalcEdit.TabIndex = 16;
			this.CSI_ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CSI_ItemNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// CSI_UnitOfQuantityDropEdit
			// 
			this.CSI_UnitOfQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 64, true);
			this.CSI_UnitOfQuantityDropEdit.Name = "CSI_UnitOfQuantityDropEdit";
			this.CSI_UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CSI_UnitOfQuantityDropEdit.TabIndex = 4;
			// 
			// CSI_ReferenceNumber2TextBox
			// 
			this.CSI_ReferenceNumber2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_ReferenceNumber2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumber2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
			this.CSI_ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 130, true);
			this.CSI_ReferenceNumber2TextBox.Name = "CSI_ReferenceNumber2TextBox";
			this.CSI_ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 15, true);
			this.CSI_ReferenceNumber2TextBox.TabIndex = 15;
			// 
			// SupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 205, true);
			this.CSI_RX_NKCurrencyCodeFindBox.ResumeLayout(true);
			this.CSI_RX_NKCurrencyCodeFindBox.PerformLayout();
			this.SupDocTypeCodeFindBox.ResumeLayout(true);
			this.SupDocTypeCodeFindBox.PerformLayout();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.SupDocReferenceCodeFindBox.ResumeLayout(true);
			this.SupDocReferenceCodeFindBox.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_UnitOfQuantityDropEdit.ResumeLayout(true);
			this.CSI_UnitOfQuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumber2TextBox;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantityDropEdit;
		private ZArchitecture.ZCalcEdit CSI_ItemNumberCalcEdit;
	}
}
