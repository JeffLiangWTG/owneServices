using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Commodity_InvoiceLineTest : TestCaseWithFactory
	{
		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestCommodity_InvoiceLine()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.56m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 34m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = "FOB";
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 20000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceChargeCollection = invoice1.Charges;
			var inviceLineChargeCollection = invoiceLine1.Charges;
			var invoiceLine2 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			var invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var additionCharge1 = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, Core.Constants.CurrencyCodes.EuropeanUnion);
			additionCharge1.J7_IsIncludedInITOT = false;
			additionCharge1.J7_Calc_IsIncludedInInvoiceAmount = false;
			additionCharge1.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 900m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = false;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var additionCharge2 = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 600m, Core.Constants.CurrencyCodes.EuropeanUnion);
			additionCharge2.J7_IsIncludedInITOT = false;
			additionCharge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			additionCharge2.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var invoiceLineCharge = inviceLineChargeCollection.AddNew(CustomsChargeTypeList.Codes.Commission, 120m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLineCharge.J7_IsIncludedInITOT = false;
			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceLineCharge = inviceLineChargeCollection.AddNew(CustomsChargeTypeList.Codes.PackingCost, 700m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLineCharge.J7_IsIncludedInITOT = false;
			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			IInvoiceLine invoiceLine = GetInvoiceLine(entryLine);
			NUnit.Framework.Assert.That(invoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.CurrencyTypeCode should be");
			NUnit.Framework.Assert.That(invoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(5000m).Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.UnitPriceAmount should be");
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.InvoiceLines.AddNew();
			additionCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			additionCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(invoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.CurrencyTypeCode should be");
			NUnit.Framework.Assert.That(invoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(5000m).Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.UnitPriceAmount should be");
			entryHeader.CH_DeclarationIncoterm = "FOB";
			NUnit.Framework.Assert.That(invoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.ChargesTypeCode should be");
			entryHeader.CH_DeclarationIncoterm = "CIF";
			NUnit.Framework.Assert.That(invoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.ChargesTypeCode should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				GetInvoiceLine(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var line = Factory.New<CusEntryLine>();
				GetInvoiceLine(line);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			var line = Factory.New<CusEntryLine>();
			IInvoiceLine wine = GetInvoiceLine(line);
			NUnit.Framework.Assert.That(wine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		NX5105Commodity_InvoiceLine GetInvoiceLine(CusEntryLine entryLine)
		{
			return new NX5105Commodity_InvoiceLine(entryLine);
		}
	}
}
