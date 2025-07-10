namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public AccDraftInvoiceHeaderProcessTaskCollection(AccDraftInvoiceHeader opportunity) : base(opportunity)
		{
		}

		public new AccDraftInvoiceHeaderProcessTask this[int index]
		{
			get { return (AccDraftInvoiceHeaderProcessTask)Elements[index]; }
		}

		public new AccDraftInvoiceHeaderProcessTask AddNew()
		{
			return (AccDraftInvoiceHeaderProcessTask)base.AddNew();
		}
	}
}
