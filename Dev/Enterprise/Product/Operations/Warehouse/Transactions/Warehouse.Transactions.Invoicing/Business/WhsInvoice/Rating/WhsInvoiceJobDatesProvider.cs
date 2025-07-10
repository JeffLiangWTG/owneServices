using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceJobDatesProvider : JobDatesProvider<WhsInvoice>
	{
		public WhsInvoiceJobDatesProvider(WhsInvoice whsInvoice)
			: base(whsInvoice) { }

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.ET_StorageFromDate;
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.GetStorageDates()[0].To;
		}
	}
}