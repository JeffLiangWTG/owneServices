using Enterprise.Customs.TW.Business.N5203;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsStatisticalMeasureATests : GoodsStatisticalMeasureAbstractTests<GoodsStatisticalMeasure>
	{
		protected override GoodsStatisticalMeasure GoodsStatisticalMeasure => new GoodsStatisticalMeasure(EntryLine);
	}
}
