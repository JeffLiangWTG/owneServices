using Enterprise.Customs.TW.Business.N5203;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentAgencyGoodsItemTests : GovernmentAgencyGoodsItemAbstractTests<GovernmentAgencyGoodsItem>
	{
		protected override GovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => new GovernmentAgencyGoodsItem(EntryLine, InvoiceLine);
	}
}
