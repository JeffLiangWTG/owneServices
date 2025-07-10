using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationGoodsShipmentTests : GoodsShipmentAbstractTests<ExportNonCondensedDeclarationGoodsShipment, ExportNonCondensedDeclarationMessageSendingObject>
	{
		[ExpectNoExceptions]
		public void TestGoodsShipment_GovernmentAgencyGoodsItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.First(), NUnit.Framework.Is.TypeOf<ExportNonCondensedDeclarationGovernmentAgencyGoodsItem>());
		}

		[ExpectNoExceptions]
		public override void TestGovernmentAgencyGoodsItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CreateEntryLineWithInvoiceLine(entryHeader, 3, invoiceHeader, "22");
			CreateEntryLineWithInvoiceLine(entryHeader, 2, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 1, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 4, invoiceHeader, "22");
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems.Cast<GovernmentAgencyGoodsItem>().ToList();
			NUnit.Framework.Assert.That(goodsItems.Count, NUnit.Framework.Is.EqualTo(4));
			for (int i = 0; i < goodsItems.Count; ++i)
			{
				NUnit.Framework.Assert.That(goodsItems[i].SequenceNumeric, NUnit.Framework.Is.EqualTo(i + 1).Using(CustomComparers.TypeComparison));
			}

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItems[0].EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("22").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[1].EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[2].EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[3].EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("22").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[0].CommodityDescriptionForDocument, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[1].CommodityDescriptionForDocument, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[2].CommodityDescriptionForDocument, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[3].CommodityDescriptionForDocument, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public override void TestItemChargeAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			invoiceHeader.JZ_InvoiceAmount = 253m;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(253m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestGoodsShipment_GovernmentAgencyGoodsItemsReconcile()
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
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
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

		protected override ExportNonCondensedDeclarationGoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs) => new ExportNonCondensedDeclarationGoodsShipment(entryHeader, supportingDocuments, allEDocs);
		protected override ExportNonCondensedDeclarationMessageSendingObject GetMessageSendingObject(CusEntryHeader entryHeader) => new ExportNonCondensedDeclarationMessageSendingObject(entryHeader);
	}
}
