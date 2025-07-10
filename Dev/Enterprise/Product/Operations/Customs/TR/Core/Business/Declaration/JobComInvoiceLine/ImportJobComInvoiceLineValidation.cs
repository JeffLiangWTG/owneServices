using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
			base.CheckJI_ZZF_NKTaxType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ZZF_NKTaxTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ZZF_NKTaxTypeInfo);
		}
	}
}
