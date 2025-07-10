using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationGoodsMeasureTests : GoodsMeasureAbstractTests<ExportNonCondensedDeclarationGoodsMeasure>
	{
		[ExpectNoExceptions]
		public override void TestNetWeightMeasure()
		{
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 23;
			NUnit.Framework.Assert.That(GoodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(23m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestTariffQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 4;
			NUnit.Framework.Assert.That(GoodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUnitCode()
		{
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
			NUnit.Framework.Assert.That(GoodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Pounds).Using(CustomComparers.TypeComparison));
		}

		protected override IGoodsMeasure GoodsMeasure => new ExportNonCondensedDeclarationGoodsMeasure(EntryLine, invoiceLine);
	}
}
