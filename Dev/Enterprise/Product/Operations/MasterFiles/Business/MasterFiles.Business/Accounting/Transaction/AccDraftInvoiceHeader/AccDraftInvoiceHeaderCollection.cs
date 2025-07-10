using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceHeaderCollection : BusinessObjectCollection<AccDraftInvoiceHeader>
	{
		public AccDraftInvoiceHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccDraftInvoiceHeaderCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}
}
