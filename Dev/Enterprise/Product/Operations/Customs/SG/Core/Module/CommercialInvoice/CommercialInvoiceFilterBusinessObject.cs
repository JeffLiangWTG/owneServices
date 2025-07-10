namespace Enterprise.Customs.SG.V4.Module
{
	public class CommercialInvoiceFilterBusinessObject : Customs.Module.CommercialInvoiceFilterBusinessObject
	{
		protected override Customs.Module.CommercialInvoiceFilterLookups GetNewLookups()
		{
			return new CommercialInvoiceFilterLookups(this);
		}
	}
}
