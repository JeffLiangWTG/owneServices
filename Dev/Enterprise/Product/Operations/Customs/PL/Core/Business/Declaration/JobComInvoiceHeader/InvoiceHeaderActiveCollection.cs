namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
{
	public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
	{
	}

	public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
		: base(groupInvoice, isDirectRelationship)
	{
	}

	public new JobComInvoiceHeader AddNew()
	{
		return (JobComInvoiceHeader)base.AddNew();
	}

	public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);
}
