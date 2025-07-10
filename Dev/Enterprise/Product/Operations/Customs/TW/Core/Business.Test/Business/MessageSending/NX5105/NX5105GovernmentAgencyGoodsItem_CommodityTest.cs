using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GovernmentAgencyGoodsItem_CommodityTest : NX5105GovernmentAgencyGoodsItem_CommodityAbstractTest<NX5105GovernmentAgencyGoodsItem_Commodity>
	{
		protected override ICommodity GetCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return new NX5105GovernmentAgencyGoodsItem_Commodity(entryLine, invoiceLine);
		}
	}
}
