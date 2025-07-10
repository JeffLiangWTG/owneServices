using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommercialInvoiceDetailsLayoutBuilder<TBizObject> : ColumnLayoutBuilder<TBizObject, CommercialInvoiceDetailsControlBag> where TBizObject : BaseJobComInvoiceHeader
	{
		public override CommercialInvoiceDetailsControlBag CommonBag => CommercialInvoiceDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
