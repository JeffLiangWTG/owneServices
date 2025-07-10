namespace Enterprise.Customs.US.Business
{
	public abstract class AddInfoJobComInvoiceLineValidation : USAddInfoValidation
	{
		protected AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public virtual void ValidateSetIndicator()
		{
		}

		protected new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		protected AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
