
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class JobComInvoiceHeaderValidation_Inward : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation_Inward(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_InvoiceAmountInfo, "Invoice Amount");

			if (Parent.JZ_InvoiceAmount.IsEmpty)
			{
				Parent.JZ_InvoiceAmountInfo.AddWarning("You have not entered an Invoice Amount.");
			}
		}
	}
}
