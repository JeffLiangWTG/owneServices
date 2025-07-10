namespace Enterprise.Customs.US.Business
{
	public class RelatedDocumentValidation : Customs.Business.CusCodeDataValidation
	{
		public RelatedDocumentValidation(RelatedDocument parent)
			: base(parent)
		{
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent.Parent; }
		}

		protected new RelatedDocument Parent
		{
			get { return (RelatedDocument)base.Parent; }
		}
	}
}
