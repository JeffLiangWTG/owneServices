
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceLineValidation_OUTwCO : JobComInvoiceLineValidation_OUT
	{
		public JobComInvoiceLineValidation_OUTwCO(JobComInvoiceLine parent)
			: base(parent)
		{
			this.Add(new JobComInvoiceLineCOValidation(parent));
		}

		protected override void CheckCertItemDescription()
		{
			base.CheckCertItemDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CertItemDescriptionInfo, "Item Description");
		}
	}
}
