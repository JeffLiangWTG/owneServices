using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationCommodityTest : NX5105GovernmentAgencyGoodsItem_CommodityAbstractTest<ImportNonCondensedDeclarationCommodity>
	{
		protected override ICommodity GetCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return new ImportNonCondensedDeclarationCommodity(entryLine, invoiceLine);
		}

		[ExpectNoExceptions]
		public override void TestCommodity_Description()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_Description = "CL_Desc test";
			invoiceLine.JI_NDescription = "JI_NDesc test";
			invoiceLine.JI_DeclarationGoodsDescription = "goods description";
			var commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("goods description").Using(CustomComparers.TypeComparison), "Commodity.Description should be");
			entryLine.CL_Description = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("goods description").Using(CustomComparers.TypeComparison), "Commodity.Description should be");
		}

		[ExpectNoExceptions]
		public override void TestCommodity_InvoiceLine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var goodsShipment = GetCommodity(entryLine, Factory.New<JobComInvoiceLine>());
			NUnit.Framework.Assert.That(goodsShipment.InvoiceLine, NUnit.Framework.Is.TypeOf(typeof(ImportNonCondensedDeclarationInvoiceLine)));
		}

		[ExpectNoExceptions]
		public override void TestCommodity_DutyTaxFee()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var goodsShipment = GetCommodity(entryLine, entryLine?.RandomLine);
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFee, NUnit.Framework.Is.TypeOf(typeof(ImportNonCondensedDeclarationDutyTaxFee)));
		}
	}
}
