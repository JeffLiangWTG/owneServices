using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class GoodsMeasureAbstractTests<TGoodsMeasure> : TestCaseWithFactory
		where TGoodsMeasure : IGoodsMeasure
	{
		[ExpectNoExceptions]
		public virtual void TestNetWeightMeasure()
		{
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 93;
			NUnit.Framework.Assert.That(GoodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(93m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(GoodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(EntryLine.EffectiveNetWeight.InKilogramsSafe));
		}

		[ExpectNoExceptions]
		public virtual void TestTariffQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 4;
			NUnit.Framework.Assert.That(GoodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestUnitCode()
		{
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
			NUnit.Framework.Assert.That(GoodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Pounds).Using(CustomComparers.TypeComparison));
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
		protected abstract IGoodsMeasure GoodsMeasure { get; }
	}
}
