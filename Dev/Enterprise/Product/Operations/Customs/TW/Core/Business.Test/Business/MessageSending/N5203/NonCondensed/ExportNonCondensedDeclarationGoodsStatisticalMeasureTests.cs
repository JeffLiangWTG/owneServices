using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationGoodsStatisticalMeasureTests : GoodsStatisticalMeasureAbstractTests<ExportNonCondensedDeclarationGoodsStatisticalMeasure>
	{
		protected override ExportNonCondensedDeclarationGoodsStatisticalMeasure GoodsStatisticalMeasure => new ExportNonCondensedDeclarationGoodsStatisticalMeasure(EntryLine, invoiceLine);
		[ExpectNoExceptions]
		public override void TestStatisticalUnitCode()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "TNE";
			NUnit.Framework.Assert.That(GoodsStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("TNE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestTariffQuantity()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			NUnit.Framework.Assert.That(GoodsStatisticalMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
		}
	}
}
