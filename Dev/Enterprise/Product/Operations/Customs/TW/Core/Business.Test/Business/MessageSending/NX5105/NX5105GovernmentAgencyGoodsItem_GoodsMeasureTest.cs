namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GovernmentAgencyGoodsItem_GoodsMeasureTest : NX5105GovernmentAgencyGoodsItem_GoodsMeasureAbstractTest<NX5105GovernmentAgencyGoodsItem_GoodsMeasure>
	{
		protected override NX5105GovernmentAgencyGoodsItem_GoodsMeasure GetGoodsMeasure(CusEntryLine entryLine)
		{
			return new NX5105GovernmentAgencyGoodsItem_GoodsMeasure(entryLine);
		}
	}
}
