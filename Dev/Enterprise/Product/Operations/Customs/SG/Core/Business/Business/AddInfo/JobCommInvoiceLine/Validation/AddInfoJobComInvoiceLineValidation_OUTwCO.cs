

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineValidation_OUTwCO : AddInfoJobComInvoiceLineValidation_OUT
	{
		public AddInfoJobComInvoiceLineValidation_OUTwCO(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
			this.Add(new AddInfoJobComInvoiceLineCOValidation(parent));
		}
	}
}
