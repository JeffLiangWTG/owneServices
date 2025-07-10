namespace Enterprise.Customs.TR.GUI
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
			this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_CodeCodeFindBox.SuspendLayout();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// CSI_CodeCodeFindBox
			// 
			this.CSI_CodeCodeFindBox.AllowDrop = true;
			this.CSI_CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 20, true);
			this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
			this.CSI_CodeCodeFindBox.PreBoundMaxLength = 4;
			this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.CSI_CodeCodeFindBox.TabIndex = 0;
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
			this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 42, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_StatusDropEdit
			// 
			this.CSI_StatusDropEdit.AllowDrop = true;
			this.CSI_StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_StatusDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Status)));
			this.CSI_StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 64, true);
			this.CSI_StatusDropEdit.Name = "CSI_StatusDropEdit";
			this.CSI_StatusDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.CSI_StatusDropEdit.TabIndex = 2;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 108, true);
			this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
			this.CSI_DateOfExpiryDateEdit.TabIndex = 4;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.AllowDrop = true;
			this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfIssueDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 86, true);
			this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
			this.CSI_DateOfIssueDateEdit.TabIndex = 3;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("8200EAE1-021B-4075-AE04-C85EB31A0A63", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_StatusDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 134, true);
			this.SupportingDocumentsGroupBox.TabIndex = 11;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "SupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 134, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_CodeCodeFindBox.ResumeLayout(true);
			this.CSI_CodeCodeFindBox.PerformLayout();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZDropEdit CSI_StatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
	}
}
