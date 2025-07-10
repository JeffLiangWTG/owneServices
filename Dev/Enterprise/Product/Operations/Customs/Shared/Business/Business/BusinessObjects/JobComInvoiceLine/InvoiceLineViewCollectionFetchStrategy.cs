using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class InvoiceLineViewCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public InvoiceLineViewCollectionFetchStrategy(IInvoiceLineViewCollection<BaseJobComInvoiceLine> collection)
			: base(collection)
		{
		}

		protected new IInvoiceLineViewCollection<BaseJobComInvoiceLine> Collection
		{
			get { return (IInvoiceLineViewCollection<BaseJobComInvoiceLine>)base.Collection; }
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			Collection.LoadCusEntryLineFetchHintIfNeeded(); // When grid is showing, the system need to calculate the colour for each row
			base.FetchForViewCore(businessObjects, columns);
		}
	}
}
