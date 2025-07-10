using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class CommercialInvoiceFilterLookups : Customs.Module.CommercialInvoiceFilterLookups
	{
		public CommercialInvoiceFilterLookups(CommercialInvoiceFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypes => JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory);
	}
}
