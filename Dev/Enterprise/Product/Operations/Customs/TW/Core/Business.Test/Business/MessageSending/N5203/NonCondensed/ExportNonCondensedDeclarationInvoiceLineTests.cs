using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationInvoiceLineTests : CommodityInvoiceLineAbstractTests<ExportNonCondensedDeclarationInvoiceLine>
	{
		[ExpectNoExceptions]
		public override void TestUnitPriceAmount()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLine.JI_LinePrice = 156.25m;
			invoiceLine.JI_InvoiceQuantity = 1m;
			NUnit.Framework.Assert.That(CommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(156.25m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestItemChargeAmount()
		{
			invoiceLine.JI_CVAfterRecon = 625m;
			var commodityInvoiceLine = CommodityInvoiceLine as ExportNonCondensedDeclarationInvoiceLine;
			NUnit.Framework.Assert.That(commodityInvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(625m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestGoodsShipment_GovernmentAgencyGoodsItems()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 345241.02m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			insurance.J7_IsIncludedInITOT = true;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 24;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 16;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 3605.33m;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 3;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 7395.55m;
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 3;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 15282.23m;
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_InvoiceQuantity = 6;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 2205.37m;
			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_InvoiceQuantity = 6;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 13982.23m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var goodsShipment = new ExportNonCondensedDeclarationGoodsShipment(entryHeader, null, null);
			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems.ToList();
			NUnit.Framework.Assert.That(goodsItems[0].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(1931630m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[1].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(1753506m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[2].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(1736795m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[3].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(667998m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[4].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(1380357m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[5].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(398397m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[6].Commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(2525872m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(10394555m).Using(CustomComparers.TypeComparison));
		}

		protected override IInvoiceLine CommodityInvoiceLine => new ExportNonCondensedDeclarationInvoiceLine(EntryLine, invoiceLine);
	}
}
