using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Module
{
	public class CommercialInvoiceFilterLookups : Customs.Module.CommercialInvoiceFilterLookups
	{
		public CommercialInvoiceFilterLookups(CommercialInvoiceFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypes
		{
			get { return MessageTypeCodeList.GetListWithAdvanceShippingNotice(Factory); }
		}
	}
}
