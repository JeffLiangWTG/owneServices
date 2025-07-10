using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GoodsShipment_GovernmentAgencyGoodsItemTest : NX5105GoodsShipment_GovernmentAgencyGoodsItemAbstractTest<NX5105GoodsShipment_GovernmentAgencyGoodsItem, NX5105ManufacturerWrapper>
	{
		protected override IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine)
		{
			return new NX5105GoodsShipment_GovernmentAgencyGoodsItem(cusEntryLine, invoiceLine);
		}
	}
}
