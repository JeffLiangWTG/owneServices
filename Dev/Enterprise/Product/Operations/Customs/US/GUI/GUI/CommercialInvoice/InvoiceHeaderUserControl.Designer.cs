namespace Enterprise.Customs.US.GUI
{
	partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TariffTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffTypeLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.TariffTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// ordersAttachUserControl
			// 
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 321, true);
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 172, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 153, true);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.TariffTypeDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.TariffTypeLabel);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 5, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 73, true);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.TariffTypeLabel, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.TariffTypeDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeDropDownEdit, 0);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 84, true);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 231, true);
			// 
			// InvCustomFieldsUserControl
			// 
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 291, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// TariffTypeDropEdit
			// 
			this.TariffTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffTypeDropEdit, "Invoices.US_TariffType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).US_TariffType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfoLookups.US_TariffTypeList)));
			this.TariffTypeDropEdit.BindToList = "Invoices.AddInfoLookups+US_TariffTypeList";
			this.TariffTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 42, true);
			this.TariffTypeDropEdit.Name = "TariffTypeDropEdit";
			this.TariffTypeDropEdit.PreBoundMaxLength = 3;
			this.TariffTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TariffTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.TariffTypeDropEdit.TabIndex = 3;
			// 
			// TariffTypeLabel
			// 
			this.TariffTypeLabel.AutoSize = true;
			this.TariffTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TariffTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.TariffTypeLabel.Name = "TariffTypeLabel";
			this.TariffTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.TariffTypeLabel.TabIndex = 2;
			this.TariffTypeLabel.Text = "Tariff Type:";
			// 
			// InvoiceHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "InvoiceHeaderUserControl";
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
			this.TariffTypeDropEdit.ResumeLayout(true);
			this.TariffTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit TariffTypeDropEdit;
		internal Enterprise.ZArchitecture.ZLabel TariffTypeLabel;
	}
}
