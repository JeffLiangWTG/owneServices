using System;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class SGSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public SGSupplierHeaderUserControl()
		{
			InitializeComponent();

			ApportionedChargesGrid.AfterBind += new EventHandler(ApportionedChargesGrid_Bound);
			InvoiceChargesGrid.AfterBind += new EventHandler(InvoiceChargesGrid_AfterBind);
			BaseGroupChargesGrid.AfterBind += new EventHandler(BaseGroupChargesGrid_AfterBind);
		}

		#region Grid Columns Visibility

		void ApportionedChargesGrid_Bound(object sender, EventArgs e)
		{
			ApportionedChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
			ApportionedChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);
		}

		void InvoiceChargesGrid_AfterBind(object sender, EventArgs e)
		{
			InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
			InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);
			InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_Percentage, Enterprise.Customs.SG.V4.GUI.Res.GetString("A5D6811E-F2B5-43BE-9F81-7939079A8118", "%"));
		}

		void BaseGroupChargesGrid_AfterBind(object sender, EventArgs e)
		{
			BaseGroupChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
			BaseGroupChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);
			BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_Percentage, Enterprise.Customs.SG.V4.GUI.Res.GetString("A5D6811E-F2B5-43BE-9F81-7939079A8118", "%"));
		}

		#endregion

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill);

			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(false, JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, Customs.Business.BaseJobComInvoiceHeader.Schema.SupplierName);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.Visible = false;
		}
	}
}
