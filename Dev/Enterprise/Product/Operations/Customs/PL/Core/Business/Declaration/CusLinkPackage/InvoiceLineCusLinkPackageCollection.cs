using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceLineCusLinkPackageCollection : Customs.Business.BaseCusLinkPackageCollection
{
	public InvoiceLineCusLinkPackageCollection(JobComInvoiceLine invoiceLine)
		: base(invoiceLine)
	{
	}

	public new InvoiceLineCusLinkPackage this[int index] => (InvoiceLineCusLinkPackage)base[index];

	public new InvoiceLineCusLinkPackage AddNew() => (InvoiceLineCusLinkPackage)base.AddNew();

	protected override BusinessObject CreateNonPersistentBusinessObject() => new InvoiceLineCusLinkPackage((JobComInvoiceLine)Supporter);
}
