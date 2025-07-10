using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// InvoiceLine collection dependant on the InvoiceHeader that is used as a sustitute for 
	/// the InvoiceLineCollection (dependant on the JobDeclaration) when there is no JobDeclaration
	/// available against the InvoiceHeader
	/// </summary>
	public class InvoiceLineDependentCollection : DependentBusinessObjectCollection<BaseJobComInvoiceLine, BaseJobComInvoiceHeader>
	{
		public InvoiceLineDependentCollection(BaseJobComInvoiceHeader invoice)
			: base(invoice)
		{
		}
	}
}
