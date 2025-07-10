using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class GoodsStatisticalMeasureAbstractTests<TGoodsStatisticalMeasure> : TestCaseWithFactory
		where TGoodsStatisticalMeasure : IGoodsStatisticalMeasure
	{
		[ExpectNoExceptions]
		public virtual void TestStatisticalUnitCode()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "TNE";
			NUnit.Framework.Assert.That(GoodsStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("TNE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestTariffQuantity()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			NUnit.Framework.Assert.That(GoodsStatisticalMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
		}

		CusEntryHeader entryHeader;
		protected CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		protected JobComInvoiceLine invoiceLine;
		protected abstract TGoodsStatisticalMeasure GoodsStatisticalMeasure { get; }
	}
}
