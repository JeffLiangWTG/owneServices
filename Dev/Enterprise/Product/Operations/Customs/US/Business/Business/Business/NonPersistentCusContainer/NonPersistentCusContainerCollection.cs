using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class NonPersistentCusContainerCollection : Customs.Business.NonPersistentCusContainerCollection
	{
		public NonPersistentCusContainerCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		public new NonPersistentCusContainer this[int index]
		{
			get { return (NonPersistentCusContainer)base[index]; }
		}

		public new NonPersistentCusContainer AddNew()
		{
			return (NonPersistentCusContainer)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentCusContainer(InvoiceLine);
		}
	}
}
