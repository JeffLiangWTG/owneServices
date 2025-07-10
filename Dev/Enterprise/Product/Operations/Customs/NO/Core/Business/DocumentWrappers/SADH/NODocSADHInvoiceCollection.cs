using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHInvoiceCollection : DocBaseWrapperCollection<NODocSADHInvoice>
{
	public NODocSADHInvoiceCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public NODocSADHInvoiceCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory)
	{
	}

	public NODocSADHInvoiceCollection(JobComInvoiceHeader[] invoiceHeaders, BusinessObjectFactory factory) : base(factory)
	{
		Load(invoiceHeaders);
	}
}
