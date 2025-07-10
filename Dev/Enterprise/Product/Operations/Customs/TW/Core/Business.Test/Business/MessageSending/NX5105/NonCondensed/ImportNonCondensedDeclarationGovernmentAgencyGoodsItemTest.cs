using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationGovernmentAgencyGoodsItemTest : NX5105GoodsShipment_GovernmentAgencyGoodsItemAbstractTest<ImportNonCondensedDeclarationGovernmentAgencyGoodsItem, NX5105ManufacturerWrapper>
	{
		protected override IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine)
		{
			return new ImportNonCondensedDeclarationGovernmentAgencyGoodsItem(cusEntryLine, invoiceLine, 1);
		}

		[ExpectNoExceptions]
		public override void TestGovernmentAgencyGoodsItem_Commodity()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(goodItem.Commodity, NUnit.Framework.Is.TypeOf(typeof(ImportNonCondensedDeclarationCommodity)));
		}

		[ExpectNoExceptions]
		public override void TestGovernmentAgencyGoodsItem_GoodsMeasure()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(goodItem.GoodsMeasure, NUnit.Framework.Is.TypeOf(typeof(ImportNonCondensedDeclarationGoodsMeasure)));
		}

		[ExpectNoExceptions]
		public override void TestExtraInfoForClassification()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_ExtraInfoForClassification = "A";
			invoiceLine2.JI_ExtraInfoForClassification = "B";
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, invoiceLine1) as NX5105GoodsShipment_GovernmentAgencyGoodsItem;
			NUnit.Framework.Assert.That(goodItem.ExtraInfoForClassification, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			goodItem = GetGovernmentAgencyGoodsItem(entryLine, invoiceLine2) as NX5105GoodsShipment_GovernmentAgencyGoodsItem;
			NUnit.Framework.Assert.That(goodItem.ExtraInfoForClassification, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
		}
	}
}
