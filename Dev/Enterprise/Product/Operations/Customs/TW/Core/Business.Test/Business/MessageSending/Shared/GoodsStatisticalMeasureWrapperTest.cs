using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsStatisticalMeasureWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGoodsStatisticalMeasure()
		{
			IGoodsStatisticalMeasure goodsStatisticalMeasure = new GoodsStatisticalMeasureWrapper(100, "AA");
			NUnit.Framework.Assert.That(goodsStatisticalMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GoodsStatisticalMeasure.TariffQuantity should be");
			NUnit.Framework.Assert.That(goodsStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "GoodsStatisticalMeasure.StatisticalUnitCode should be");
		}
	}
}
