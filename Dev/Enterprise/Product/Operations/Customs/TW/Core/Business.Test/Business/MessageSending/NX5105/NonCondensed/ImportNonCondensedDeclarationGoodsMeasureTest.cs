using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationGoodsMeasureTest : NX5105GovernmentAgencyGoodsItem_GoodsMeasureAbstractTest<ImportNonCondensedDeclarationGoodsMeasure>
	{
		protected override ImportNonCondensedDeclarationGoodsMeasure GetGoodsMeasure(CusEntryLine cusEntryLine)
		{
			return new ImportNonCondensedDeclarationGoodsMeasure(cusEntryLine, cusEntryLine?.RandomLine);
		}

		[ExpectNoExceptions]
		public override void TestGovernmentAgencyGoodsItem_GoodsMeasure()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var instruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_NetWeight = 2m;
			invoiceLine2.JI_NetWeightUQ = "T";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceQuantity = 200m;
			invoiceLine1.JI_InvoiceUQ = "AAA";
			invoiceLine2.JI_InvoiceUQ = "BBB";
			var goodsMeasure = new ImportNonCondensedDeclarationGoodsMeasure(entryLine1, invoiceLine1);
			NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GoodItem.NetWeightMeasure should be ");
			NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GoodItem.TariffQuantity should be ");
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "GoodItem.UnitCode should be ");
			goodsMeasure = new ImportNonCondensedDeclarationGoodsMeasure(entryLine1, invoiceLine2);
			NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison), "GoodItem.TariffQuantity should be ");
			NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "GoodItem.TariffQuantity should be ");
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison), "GoodItem.UnitCode should be ");
		}
	}
}
