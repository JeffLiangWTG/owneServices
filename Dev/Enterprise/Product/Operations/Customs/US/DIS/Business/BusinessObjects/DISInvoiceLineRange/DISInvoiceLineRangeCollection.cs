using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISInvoiceLineRangeCollection : NonPersistentBusinessObjectCollection<DISInvoiceLineRange>
	{
		public DISInvoiceLineRangeCollection(DISInvoice disInvoice)
			: base(disInvoice.Factory)
		{
			this.disInvoice = disInvoice;
		}

		readonly DISInvoice disInvoice;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DISInvoiceLineRange(disInvoice);
		}
	}
}
