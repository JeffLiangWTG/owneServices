using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderCOValidation : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderCOValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		ZBool IsNote1 => SGCertificatesCodeList.IsOriginCriterionDetailsRequired(Parent.JobDeclaration.Certificate1Type);

		protected override void CheckJZ_InvoiceNumber()
		{
			if (IsNote1 && Parent.JZ_InvoiceNumber.IsEmpty)
			{
				Parent.JZ_InvoiceNumberInfo.AddWarning("You may optionally specify Invoice details for this certificate type.");
			}
		}

		protected override void CheckJZ_InvoiceDate()
		{
			if (IsNote1 && Parent.JZ_InvoiceDate.IsEmpty)
			{
				Parent.JZ_InvoiceDateInfo.AddWarning("You may optionally specify Invoice details for this certificate type.");
			}
		}
	}
}
