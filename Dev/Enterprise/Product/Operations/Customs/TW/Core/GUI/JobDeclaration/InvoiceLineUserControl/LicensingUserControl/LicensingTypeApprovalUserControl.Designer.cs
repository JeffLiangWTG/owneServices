
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingTypeApprovalUserControl
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
            this.TypeApprovalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ExemptionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PartyIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AuthorizedPartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.TypeApprovalGroupBox.SuspendLayout();
            this.ExemptionCodeDropEdit.SuspendLayout();
            this.PartyIdentifierDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
            // 
            // TypeApprovalGroupBox
            // 
            this.TypeApprovalGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e4af9828-9427-424f-825b-91e6d68ae52f", "Type Approval");
            this.TypeApprovalGroupBox.Controls.Add(this.ExemptionCodeDropEdit);
            this.TypeApprovalGroupBox.Controls.Add(this.PartyIdentifierDropEdit);
            this.TypeApprovalGroupBox.Controls.Add(this.AuthorizedPartyTextBox);
            this.TypeApprovalGroupBox.Controls.Add(this.CertificateNoTextBox);
            this.TypeApprovalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
            this.TypeApprovalGroupBox.Name = "TypeApprovalGroupBox";
            this.TypeApprovalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 125, true);
            this.TypeApprovalGroupBox.TabIndex = 0;
            this.TypeApprovalGroupBox.TabStop = false;
            // 
            // ExemptionCodeDropEdit
            // 
            this.ExemptionCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExemptionCodeDropEdit, "FilteredInvoiceLines.ExemptionCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ExemptionCode)));
            this.ExemptionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 93, true);
            this.ExemptionCodeDropEdit.Name = "ExemptionCodeDropEdit";
            this.ExemptionCodeDropEdit.PreBoundMaxLength = 3;
            this.ExemptionCodeDropEdit.ShouldResizeByMaxLength = false;
            this.ExemptionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
            this.ExemptionCodeDropEdit.TabIndex = 3;
            // 
            // PartyIdentifierDropEdit
            // 
            this.PartyIdentifierDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PartyIdentifierDropEdit, "FilteredInvoiceLines.TypeApprovalPartyIdentifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TypeApprovalPartyIdentifier)));
            this.PartyIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 68, true);
            this.PartyIdentifierDropEdit.Name = "PartyIdentifierDropEdit";
            this.PartyIdentifierDropEdit.PreBoundMaxLength = 3;
            this.PartyIdentifierDropEdit.ShouldResizeByMaxLength = true;
            this.PartyIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
            this.PartyIdentifierDropEdit.TabIndex = 2;
            // 
            // AuthorizedPartyTextBox
            // 
            this.BindingSource.SetBindingMember(this.AuthorizedPartyTextBox, "FilteredInvoiceLines.TypeApprovalAuthorizedParty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TypeApprovalAuthorizedParty)));
            this.AuthorizedPartyTextBox.CaptionResourceString = null;
            this.AuthorizedPartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 43, true);
            this.AuthorizedPartyTextBox.Name = "AuthorizedPartyTextBox";
            this.AuthorizedPartyTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.AuthorizedPartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
            this.AuthorizedPartyTextBox.TabIndex = 1;
            // 
            // CertificateNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.CertificateNoTextBox, "FilteredInvoiceLines.TypeApprovalCertificateNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TypeApprovalCertificateNo)));
            this.CertificateNoTextBox.CaptionResourceString = null;
            this.CertificateNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 18, true);
            this.CertificateNoTextBox.Name = "CertificateNoTextBox";
            this.CertificateNoTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.CertificateNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
            this.CertificateNoTextBox.TabIndex = 0;
            // 
            // LicensingTypeApprovalUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TypeApprovalGroupBox);
            this.Name = "LicensingTypeApprovalUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 286, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.TypeApprovalGroupBox.ResumeLayout(false);
            this.TypeApprovalGroupBox.PerformLayout();
            this.ExemptionCodeDropEdit.ResumeLayout(true);
            this.ExemptionCodeDropEdit.PerformLayout();
            this.PartyIdentifierDropEdit.ResumeLayout(true);
            this.PartyIdentifierDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TypeApprovalGroupBox;
		private ZArchitecture.ZTextBox CertificateNoTextBox;
		private ZArchitecture.ZTextBox AuthorizedPartyTextBox;
		private ZArchitecture.GUI.ZDropEdit ExemptionCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PartyIdentifierDropEdit;
	}
}
