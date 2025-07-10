using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class CommercialInvoiceLineWrapperCollection : DocumentWrapperCollection<CommercialInvoiceLineWrapper>
	{
		public CommercialInvoiceLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void AddLinesFrom(IEnumerable invoiceLines)
		{
			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				Add(CommercialInvoiceLineWrapper.New(invoiceLine, Factory));
			}
		}
	}
}
