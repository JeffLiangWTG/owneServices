

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class AddInfoJobComInvoiceLineValidation_CUSDEC : AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation_CUSDEC(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		internal const string LastSellingPriceRequired = "For imported goods, if Supply Indicator = 'Y', LSP must be filled in. LSP and not the CIF value, is used for computation of GST";
	}
}
