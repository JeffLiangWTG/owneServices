namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public partial class LayoutInvoiceHeaderUserControl
	{
		#region Designer Generated

		void InitializeComponent()
		{
			this.HeaderDetailsUserControl = new Enterprise.Customs.GUI.CommercialInvoice.InvoiceHeaderDetailsUserControl();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.HeaderDetailsUserControl);
			// 
			// HeaderDetailsUserControl
			// 
			this.HeaderDetailsUserControl.AllowDrop = true;
			this.HeaderDetailsUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.HeaderDetailsUserControl, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)))));
			this.HeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.HeaderDetailsUserControl.Name = "HeaderDetailsUserControl";
			this.HeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 336, true);
			this.HeaderDetailsUserControl.TabIndex = 1;
			// 
			// LayoutInvoiceHeaderUserControl
			// 
			this.Name = "LayoutInvoiceHeaderUserControl";
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
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderDetailsUserControl.ResumeLayout(true);
			this.HeaderDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private InvoiceHeaderDetailsUserControl HeaderDetailsUserControl;
		#endregion
	}
}
