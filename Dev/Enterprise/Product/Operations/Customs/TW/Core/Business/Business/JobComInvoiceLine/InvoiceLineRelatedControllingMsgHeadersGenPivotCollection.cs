using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineRelatedControllingMsgHeadersGenPivotCollection : Customs.Business.CustomsGenPivotCollection<InvoiceLineRelatedControllingMsgHeadersGenPivot, JobComInvoiceLine, CusTWControllingMessageHeader>
	{
		public InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(JobComInvoiceLine master)
			: base(master)
		{
		}

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.InvoiceLineRelatedControllingMessageHeaderPivot; }
		}
	}
}
