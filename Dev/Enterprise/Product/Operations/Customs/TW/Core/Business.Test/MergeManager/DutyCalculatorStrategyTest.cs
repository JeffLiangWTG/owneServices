using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override bool ExpectedCanBeNegative => true;

		protected override int ExpectedDutyDecimalPlace => 3;

		[ExpectNoExceptions]
		public void TestGenerateDTAWhenTariffAdditionalCodeIsNotEmptyAndPaymentMethodIsEmpty()
		{
			TariffDataForTestHelper.NewData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);
			entryInstruction.CEI_Style = "G1";
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "51";
			invoiceLine.JI_Tariff = "98050000009";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			invoiceLine.JI_TariffAdditionalCode = "7301";
			invoiceLine.JI_DtyPymntMthd = ZString.Empty;
			new LineMerger(declaration).DoMerge();
			var entryLine = invoiceLine.CusEntryLine;
			var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == RefCusRateCodes.DTA);
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(fee.CF_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(fee.CF_ChargeType, NUnit.Framework.Is.EqualTo(RefCusRateCodes.DTA).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCalculateDuties()
		{
			TariffDataForTestHelper.NewData(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);
			entryInstruction.CEI_Style = "G1";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "98050000009";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			invoiceLine.JI_VatPymntMthd = "CAS";

			new LineMerger(declaration).DoMerge();
			var entryLine = invoiceLine.CusEntryLine;
			NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1258m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
			var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(258m).Using(CustomComparers.TypeComparison));
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		[ExpectNoExceptions]
		public void TestGetDutyCalculationResults()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var rateTypeAA = referenceDataHelper.CreateCusRateType("TW", "AA");
			rateTypeAA.ZZR_CustomsValueFormula = "CV";
			var rateTypeBB = referenceDataHelper.CreateCusRateType("TW", "BB");
			rateTypeBB.ZZR_CustomsValueFormula = "CV";
			var rateTypeCC = referenceDataHelper.CreateCusRateType("TW", "CC");
			rateTypeCC.ZZR_CustomsValueFormula = "CV";
			Factory.Save();
			var rateCodeAA = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "AA", rateTypeAA.PK);
			var rateCodeBB = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "BB", rateTypeBB.PK);
			var rateCodeCC = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CC", rateTypeCC.PK);
			var tariffType = referenceDataHelper.CreateTariffType("TW", "TY");
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff("TW", tariffType.PK, "123456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			referenceDataHelper.CreateRate(tariff, rateCodeAA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.3*VFD");
			referenceDataHelper.CreateRate(tariff, rateCodeBB.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.5*VFD");
			referenceDataHelper.CreateRate(tariff, rateCodeCC.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryLine = GetCusEntryLineForTest(declaration);
			var entryLineData = new EntryLineUniversalRate(entryLine);
			var rates = Factory.Load<RateView>(new ZDBOnlyQuery(typeof(RateView)));

			var strategy = new DutyCalculatorStrategy(declaration);
			var maxResults = strategy.GetDutyCalculationResults(entryLine, entryLineData, rates);
			NUnit.Framework.Assert.That(maxResults.Select(x => x.Amount).Max(), NUnit.Framework.Is.EqualTo(11000m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateDutyAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = GetCusEntryLineForTest(declaration);
			var entryLineData = new EntryLineUniversalRate(entryLine);
			var strategy = new DutyCalculatorStrategy(declaration);

			var dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "0.3*VFD", "AA", "0.3");
			AssertDutyCalculationIntermediateResult("0.3*VFD, 0.3, rateCode is 'AA'", dutyCalculationIntermediateResult, 0.3m, 10000m, "%", 3000m, "AA");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "0.3*VFD", "DTA", "0.3");
			AssertDutyCalculationIntermediateResult("0.3*VFD, 0.3, rateCode is 'DTA'", dutyCalculationIntermediateResult, 0.3m, 10000m, "%", 3000m, "DTA");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "0.05*[KGM]", "DTS", "0.05/KGM");
			AssertDutyCalculationIntermediateResult("0.05*[KGM], 0.05/KGM", dutyCalculationIntermediateResult, 0.05m, 100M, "KGM", 5m, "DTS");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "0.3*VFD", "AA", @"16.8\KGM");
			AssertDutyCalculationIntermediateResult("0.3*VFD, 16.8\\KGM", dutyCalculationIntermediateResult, 0m, 10000M, "%");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "0.3*VFD", "AA", @"vv/KGM");
			AssertDutyCalculationIntermediateResult("0.3*VFD, vv/KGM", dutyCalculationIntermediateResult, 0m, 100M, "KGM");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "7 * [LTR] * AlcoholPercentage", "TAT", @"7/LTR");
			AssertDutyCalculationIntermediateResult("AlcoholPercentage", dutyCalculationIntermediateResult, 112m);

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "IF(UnitCustomsValue >= 500,0.1 * VFD,0)", "SSG", "IF(UnitCustomsValue >= 500,0.1 * VFD,0)");
			AssertDutyCalculationIntermediateResult("'IF' Expression, true part", dutyCalculationIntermediateResult, 0.1m, 10000m, "%");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "IF(UnitCustomsValue >= 50000,0.1 * VFD,0.2 * VFD)", "SSG", "IF(UnitCustomsValue >= 50000,0.1 * VFD,0.2 * VFD)");
			AssertDutyCalculationIntermediateResult("'IF' Expression, false part", dutyCalculationIntermediateResult, 0.2m, 10000m, "%");

			dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, "IF(UnitCustomsValue >= 50000,0.1 * VFD,0)", "SSG", "IF(UnitCustomsValue >= 500,0.1 * VFD,0)");
			AssertDutyCalculationIntermediateResult("'IF' Expression, amount equals to 0", dutyCalculationIntermediateResult, 0m, 10000m, "%", 0m);
		}

		[ExpectNoExceptions]
		void AssertDutyCalculationIntermediateResult(string message, DutyCalculationIntermediateResult actual, decimal? expectedRate = null, decimal? expectedBaseValue = null, string expectedUnitOfCalculation = null, decimal? expectedAmount = null, string expectedRateCode = null)
		{
			var rateNotEquals = expectedRate.HasValue && expectedRate.Value != actual.Rate;
			var baseValueNotEquals = expectedBaseValue.HasValue && expectedBaseValue.Value != actual.BaseValue;
			var unitOfCalculationNotEquals = expectedUnitOfCalculation != null && expectedUnitOfCalculation != actual.UnitOfCalculation;
			var amountNotEquals = expectedAmount.HasValue && expectedAmount.Value != actual.Amount;
			var rateCodeNotEquals = expectedRateCode != null && expectedRateCode != actual.RateCode;

			var notEquals = rateNotEquals || baseValueNotEquals || unitOfCalculationNotEquals || amountNotEquals;
			var errorMessageBuilder = new StringBuilder();
			if (notEquals)
			{
				if (!string.IsNullOrEmpty(message))
				{
					errorMessageBuilder.AppendLine(message);
				}
				if (rateNotEquals)
				{
					errorMessageBuilder.AppendLine($"\trate expected {expectedRate.Value}, but was {actual.Rate}");
				}
				if (baseValueNotEquals)
				{
					errorMessageBuilder.AppendLine($"\tbaseValue expected {expectedBaseValue.Value}, but was {actual.BaseValue}");
				}
				if (unitOfCalculationNotEquals)
				{
					errorMessageBuilder.AppendLine($"\tunitOfCalculation expected {expectedUnitOfCalculation}, but was {actual.UnitOfCalculation}");
				}
				if (amountNotEquals)
				{
					errorMessageBuilder.AppendLine($"\tamount expected {expectedAmount.Value}, but was {actual.Amount}");
				}
				if (rateCodeNotEquals)
				{
					errorMessageBuilder.AppendLine($"\trateCode expected {expectedRateCode}, but was {actual.RateCode}");
				}
			}
			NUnit.Framework.Assert.That(!notEquals, NUnit.Framework.Is.True, errorMessageBuilder.ToString());
		}

		[ExpectNoExceptions]
		public void TestRORAllocatedToCash()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CFR";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			var chargeOFT = invoice.Charges.AddNew("OFT", 51m, Core.Constants.CurrencyCodes.Taiwan);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine1.JI_Tariff = "84795010003";
			invoiceLine1.JI_CountryOfOrigin = "JP";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "ADT";
			invoiceLine1.JI_EnteredUnitPrice = 3000m;
			invoiceLine1.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine1.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine1.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine2.JI_UseOneTenthCV = false;
			invoiceLine2.JI_RAPPrice = 400m;
			invoiceLine2.JI_RAPCurr = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine2.JI_Tariff = "84795010003";
			invoiceLine2.JI_CountryOfOrigin = "JP";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "ADT";
			invoiceLine2.JI_EnteredUnitPrice = 4000m;
			invoiceLine2.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine2.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine2.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine3.JI_Tariff = "84795010003";
			invoiceLine3.JI_CountryOfOrigin = "JP";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_InvoiceUQ = "ADT";
			invoiceLine3.JI_EnteredUnitPrice = 3000m;
			invoiceLine3.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine3.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine3.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;

			var lineMerger = new LineMerger(declaration);
			CombineAssertions(() =>
			{
				lineMerger.DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().FirstOrDefault(c => c.CL_EntryLineUnitPrice == 4000m).Fees.Cast<CusEntryLineFee>();
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(4));

				var vatCas = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(408m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(20.40m).Using(CustomComparers.TypeComparison));

				var vatDef = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(3672m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(183.60m).Using(CustomComparers.TypeComparison));

				var dtaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(dtaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(400m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(8m).Using(CustomComparers.TypeComparison));

				var dtaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(dtaDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(3600m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(72m).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCalculateDutyAmountForRorRapCase()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var dateForDuty = new ZDateTime(2019, 9, 19);
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Japan);

			var sellRate = currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			sellRate.RE_StartDate = dateForDuty.AddDays(-5);
			sellRate.RE_ExpiryDate = dateForDuty.AddDays(5);
			sellRate.RE_SellRate = 0.2209m;

			CreateTariffForTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = dateForDuty;
			entryInstruction.CEI_Style = "G1";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 6234000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;

			var chargeOFT = invoice.Charges.AddNew();
			chargeOFT.J7_ChargeType = "OFT";
			chargeOFT.J7_Amount = 3375m;
			chargeOFT.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			var chargeONS = invoice.Charges.AddNew();
			chargeONS.J7_ChargeType = "ONS";
			chargeONS.J7_Percentage = 0m;
			chargeONS.J7_Amount = 27927.33m;
			chargeONS.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine.JI_Tariff = "84795010003";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "ACR";
			invoiceLine.JI_EnteredUnitPrice = 6234000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine.JI_UseOneTenthCV = true;

			var lineTax = invoiceLine.Taxes.AddNew();
			lineTax.JLT_Type = "CT";
			lineTax.JLT_Tariff = "TRUCKBUSOTHER";
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.RorPayment;

			var entryLine = invoiceLine.CusEntryLine;
			var lineMerger = new LineMerger(declaration);
			CombineAssertions("ROR With TPF Cash Under Minimum Threshold", () =>
			{
				lineMerger.DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(7));

				var vatCas = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(182602.13m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(9130.107m).Using(CustomComparers.TypeComparison));

				var vatDef = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1643420.53m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(82171.027m).Using(CustomComparers.TypeComparison));

				var dtaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(dtaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(137709m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(2754.18m).Using(CustomComparers.TypeComparison));

				var dtaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(dtaDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1239382m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(24787.64m).Using(CustomComparers.TypeComparison));

				var ctaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ctaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(140463.18m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(42138.954m).Using(CustomComparers.TypeComparison));

				var ctaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(ctaDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1264169.64m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(379250.892m).Using(CustomComparers.TypeComparison));

				var tpfDef = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1377091m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(tpfDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(550.836m).Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
			CombineAssertions("RAP With TPF Cash Under Minimum Threshold", () =>
			{
				lineMerger.DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(3));

				var vatCas = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(182602.13m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(9130.107m).Using(CustomComparers.TypeComparison));

				var dtaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(dtaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(137709m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(2754.18m).Using(CustomComparers.TypeComparison));

				var ctaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ctaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(140463.18m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(42138.954m).Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			invoiceLine.JI_UseOneTenthCV = false;
			invoiceLine.JI_RAPPrice = 1170494.107m;
			invoiceLine.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.RorPayment;

			CombineAssertions("ROR With TPF Cash above the minimum threshold", () =>
			{
				lineMerger.DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(6));

				var dtaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(dtaDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1377091m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(27541.82m).Using(CustomComparers.TypeComparison));

				var ctaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ctaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(258562m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(77568.6m).Using(CustomComparers.TypeComparison));

				var ctaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(ctaDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1118529m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(335558.7m).Using(CustomComparers.TypeComparison));

				var vatDef = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1817760.12m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(90888.006m).Using(CustomComparers.TypeComparison));

				var tpfCas = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(tpfCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(258562m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(tpfCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(103.425m).Using(CustomComparers.TypeComparison));

				var tpfDef = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfDef.CF_BaseValue, NUnit.Framework.Is.EqualTo(1118529m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(tpfDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(447.412m).Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
			invoiceLine.JI_RAPPrice = 1170494.107m;
			CombineAssertions("RAP With TPF Cash above the minimum threshold", () =>
			{
				lineMerger.DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(4));

				var vatCas = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(263733.24m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(13186.662m).Using(CustomComparers.TypeComparison));

				var dtaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(dtaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(258562m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(5171.24m).Using(CustomComparers.TypeComparison));

				var ctaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ctaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(263733.24m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(79119.972m).Using(CustomComparers.TypeComparison));

				var tpfCas = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(tpfCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(258562m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(tpfCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(103.425m).Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.RorPayment;
			invoiceLine.JI_UseOneTenthCV = false;
			invoiceLine.JI_RAPPrice = 0m;

			CombineAssertions("ROR With JI_RAPPrice = 0", () =>
			{
				new LineMerger(declaration).DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(4));

				var vatCas = fees.SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(1826022.66m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(vatCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(91301.133m).Using(CustomComparers.TypeComparison));

				var dtaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(dtaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(1377091m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(dtaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(27541.82m).Using(CustomComparers.TypeComparison));

				var ctaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(ctaCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(1404632.82m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(ctaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(421389.846m).Using(CustomComparers.TypeComparison));

				var tpfCas = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfCas.CF_BaseValue, NUnit.Framework.Is.EqualTo(1377091m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(tpfCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(550.836m).Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._3F;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			lineTax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
			CombineAssertions("RAP With JI_RAPPrice = 0", () =>
			{
				new LineMerger(declaration).DoMerge();
				var fees = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(0));
			});
		}

		void CreateTariffForTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			var rateCodeCTS = helper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTS = helper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			Factory.Save();

			var truckbusotherTariff = helper.CreateTariff("TW", tariffTypeCT.PK, "TRUCKBUSOTHER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffCtaRate = helper.CreateRate(truckbusotherTariff, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3 * VFD", rateFormulaDeriveFrom: "0.3");
			helper.CreateCusApplicability(childTariffCtaRate, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "84795010003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "第０５０７‧９０‧２０‧００號所屬之「其他鹿茸（包括中藥用）」");
			var cusTariffDTARate = helper.CreateRate(cusTariff, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.02 * VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.02", dataGrouping: "TW");
			helper.CreateCusApplicability(cusTariffDTARate, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tpfFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			tpfFee.ZZF_Threshold = 101m;
			Factory.Save();
		}

		CusEntryLine GetCusEntryLineForTest(JobDeclaration declaration)
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 100M;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_AlcoholPercentage = 16m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader.MergedLines[0];
		}

		[ExpectNoExceptions]
		public void TestGetRateViewOfHighestDutyCalculationResult()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("JP", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffDTS = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = universalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffBoth = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBothRateDTS = universalTestHelper.CreateRate(tariffBoth, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffBothRateDTA = universalTestHelper.CreateRate(tariffBoth, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "21039090200";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			var applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);

			var strategy = new DutyCalculatorStrategy(declaration);
			var dutyCalculationIntermediateResult = strategy.GetRateViewOfHighestDutyCalculationResult(invoiceLine.CalcDataForConditionFormula, applicableRates);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResult.PK, NUnit.Framework.Is.EqualTo(rateDTA.PK));
			invoiceLine.JI_Tariff = "21039090201";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
			dutyCalculationIntermediateResult = strategy.GetRateViewOfHighestDutyCalculationResult(invoiceLine.CalcDataForConditionFormula, applicableRates);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResult.PK, NUnit.Framework.Is.EqualTo(rateDTS.PK));
			invoiceLine.JI_Tariff = "21039090202";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
			dutyCalculationIntermediateResult = strategy.GetRateViewOfHighestDutyCalculationResult(invoiceLine.CalcDataForConditionFormula, applicableRates);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResult.PK, NUnit.Framework.Is.EqualTo(tariffBothRateDTA.PK));
			invoiceLine.JI_CustomsUnitQty = "LTR";
			invoiceLine.JI_CustomsQuantity = 1m;
			applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
			dutyCalculationIntermediateResult = strategy.GetRateViewOfHighestDutyCalculationResult(invoiceLine.CalcDataForConditionFormula, applicableRates);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResult.PK, NUnit.Framework.Is.EqualTo(tariffBothRateDTS.PK));
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_EnteredUnitPrice = 1001m;
			applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
			dutyCalculationIntermediateResult = strategy.GetRateViewOfHighestDutyCalculationResult(invoiceLine.CalcDataForConditionFormula, applicableRates);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResult.PK, NUnit.Framework.Is.EqualTo(tariffBothRateDTA.PK));
		}

		[ExpectNoExceptions]
		public void TestCalculateVAT()
		{
			TariffDataForTestHelper.NewData(Factory);
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CustomsValue = 48605m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";

			var entryLineData = new EntryLineUniversalRate(entryLine, 48605m);
			var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();
			var calculationResult1 = new DutyCalculationIntermediateResult()
			{
				Amount = 2430.25m,
				RateCode = RefCusRateCodes.DTA,
				PaymentMethod = DutyTaxPaymentMethodList.Codes.NonCashPayment,
				Rate = 0.05m,
				BaseValue = 48605m,
				UnitOfCalculation = MethodOfCalculation.Percentage
			};

			var calculationResult2 = new DutyCalculationIntermediateResult()
			{
				Amount = 486.05m,
				RateCode = RefCusRateCodes.CTA,
				PaymentMethod = DutyTaxPaymentMethodList.Codes.CashPayment,
				Rate = 0.01m,
				BaseValue = 48605m,
				UnitOfCalculation = MethodOfCalculation.Percentage
			};

			InitDutyCalculationIntermediateResults();

			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);
			calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, null, false, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
			var vatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "CAS");
			CombineAssertions("CAS", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(3));
				AssertDutyCalculationIntermediateResult("CAS", 51521.30m, 0.22M, 11334.686m, vatCAS);
			});

			InitDutyCalculationIntermediateResults();
			calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, null, false, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults);
			var vatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "DEF");
			CombineAssertions("DEF", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(3));
				AssertDutyCalculationIntermediateResult("CAS", 51035.25m, 0.22M, 11227.755m, vatDEF);
			});

			TestCalculateVAT_CAS_67_69("67");
			TestCalculateVAT_CAS_67_69("69");
			TestCalculateVAT_DEF_67_69("67");
			TestCalculateVAT_DEF_67_69("69");

			var entryLineDataForRorDef = new EntryLineUniversalRate(entryLine, 20000m);
			invoiceLine.JI_Procedure = "38";
			invoiceLine.JI_UseOneTenthCV = false;
			InitDutyCalculationIntermediateResults();
			calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, entryLineDataForRorDef, true, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.RorPayment, dutyCalculationIntermediateResults);
			vatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "CAS");
			vatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "DEF");
			CombineAssertions("ROR, ROR", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(4));
				AssertDutyCalculationIntermediateResult("CAS", 49091.05m, 0.22M, 10800.031m, vatCAS);
				AssertDutyCalculationIntermediateResult("DEF", 22430.25m, 0.22M, 4934.655m, vatDEF);
			});

			InitDutyCalculationIntermediateResults();
			calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, entryLineDataForRorDef, true, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
			vatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "CAS");
			CombineAssertions("CAS, ROR", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(3));
				AssertDutyCalculationIntermediateResult("CAS", 49091.05m, 0.22M, 10800.031m, vatCAS);
			});

			InitDutyCalculationIntermediateResults();
			calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, entryLineDataForRorDef, true, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults);
			vatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "DEF");
			CombineAssertions("DEF, ROR", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(3));
				AssertDutyCalculationIntermediateResult("DEF", 51521.30m, 0.22M, 11334.686m, vatDEF);
			});

			void InitDutyCalculationIntermediateResults()
			{
				dutyCalculationIntermediateResults.Clear();
				dutyCalculationIntermediateResults.Add(calculationResult1);
				dutyCalculationIntermediateResults.Add(calculationResult2);
			}

			void TestCalculateVAT_CAS_67_69(string procedure)
			{
				invoiceLine.JI_Procedure = procedure;
				InitDutyCalculationIntermediateResults();
				calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, null, false, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
				var vatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "CAS");
				var vatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "DEF");
				CombineAssertions($"CAS, {procedure}", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(4));
					AssertDutyCalculationIntermediateResult("CAS", 49091.05m, 0.22M, 10800.031m, vatCAS);
					AssertDutyCalculationIntermediateResult("DEF", 2430.25m, 0.22M, 534.655m, vatDEF);
				});
			}

			void TestCalculateVAT_DEF_67_69(string procedure)
			{
				invoiceLine.JI_Procedure = procedure;
				InitDutyCalculationIntermediateResults();
				calculateDutiesHelper.CalculateVAT(entryLine, entryLineData, null, false, invoiceLine, RefCusTaxOrFeeCodes.VAT, new DateTime(2019, 07, 03), DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults);
				var vatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "VAT" && x.PaymentMethod == "DEF");
				CombineAssertions($"DEF, {procedure}", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(3));
					AssertDutyCalculationIntermediateResult("DEF", 51035.25m, 0.22M, 11227.755m, vatDEF);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAddFeesToEntryLines()
		{
			TariffDataForTestHelper.NewData(Factory);
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine1 = cusEntryHeader.MergedLines.AddNew();

			var duties = new List<DutyCalculationIntermediateResult>
			{
				new DutyCalculationIntermediateResult() { Amount = 10.1234m, RateCode = "AAA", PaymentMethod = "CAS", UnitOfCalculation = "A", Rate = 10m },
				new DutyCalculationIntermediateResult() { Amount = 15.2345m, RateCode = "AAA", PaymentMethod = "CAS", UnitOfCalculation = "A", Rate = 10m },
				new DutyCalculationIntermediateResult() { Amount = 20.5678m, RateCode = "AAA", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 30m },
				new DutyCalculationIntermediateResult() { Amount = 25.7899m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "B", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 15.8902m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "B", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 15.2345m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "C", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 15.2451m, RateCode = "TPF", PaymentMethod = "CAS", UnitOfCalculation = "C", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 15.2424m, RateCode = "TPF", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 16.2325m, RateCode = "TPF", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 20m },
				new DutyCalculationIntermediateResult() { Amount = 0m, RateCode = "DDD", PaymentMethod = "CAS", UnitOfCalculation = "A", Rate = 21m },
				new DutyCalculationIntermediateResult() { Amount = 0m, RateCode = "EEE", PaymentMethod = "DEF", UnitOfCalculation = "B", Rate = 22m }
			};

			var dutyCalculationEntryLineTempResultsCache = new Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>>();
			var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();
			dutyCalculationEntryLineTempResultsCache[entryLine1] = duties;

			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);
			calculateDutiesHelper.AddFeesToEntryLines(dutyCalculationEntryLineTempResultsCache);

			var fees = entryLine1.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(fees.Count(), NUnit.Framework.Is.EqualTo(6));

			var aaaCas = fees.SingleOrDefault(x => x.CF_ChargeType == "AAA" && x.CF_MethodOfPayment == "CAS" && x.CF_MethodOfCalculation == "A");
			NUnit.Framework.Assert.That(aaaCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(25.358m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(aaaCas.CF_Rate, NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison));

			var aaaDef = fees.SingleOrDefault(x => x.CF_ChargeType == "AAA" && x.CF_MethodOfPayment == "DEF" && x.CF_MethodOfCalculation == "C");
			NUnit.Framework.Assert.That(aaaDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(20.568m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(aaaDef.CF_Rate, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison));

			var bbbCasB = fees.SingleOrDefault(x => x.CF_ChargeType == "BBB" && x.CF_MethodOfPayment == "CAS" && x.CF_MethodOfCalculation == "B");
			NUnit.Framework.Assert.That(bbbCasB.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(41.68m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bbbCasB.CF_Rate, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));

			var bbbCasC = fees.SingleOrDefault(x => x.CF_ChargeType == "BBB" && x.CF_MethodOfPayment == "CAS" && x.CF_MethodOfCalculation == "C");
			NUnit.Framework.Assert.That(bbbCasC.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(15.235m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bbbCasC.CF_Rate, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));

			var tpfCas = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS" && x.CF_MethodOfCalculation == "C");
			NUnit.Framework.Assert.That(tpfCas.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(15.245m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(tpfCas.CF_Rate, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));

			var tpfDef = fees.SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF" && x.CF_MethodOfCalculation == "C");
			NUnit.Framework.Assert.That(tpfDef.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(31.475m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(tpfDef.CF_Rate, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(fees.SingleOrDefault(x => x.CF_ChargeType == "DDD" && x.CF_MethodOfPayment == "CAS" && x.CF_MethodOfCalculation == "A"), NUnit.Framework.Is.EqualTo(default(CusEntryLineFee)));
			NUnit.Framework.Assert.That(fees.SingleOrDefault(x => x.CF_ChargeType == "EEE" && x.CF_MethodOfPayment == "DEF" && x.CF_MethodOfCalculation == "B"), NUnit.Framework.Is.EqualTo(default(CusEntryLineFee)));
		}

		[ExpectNoExceptions]
		public void TestExemptDutyCalculation()
		{
			TariffDataForTestHelper.NewData(Factory);
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();

			var tpfDutyCalculation = new DutyCalculationIntermediateResult { Amount = 1, RateCode = RefCusTaxOrFeeCodes.TPF };
			var ddfDutyCalculation = new DutyCalculationIntermediateResult { Amount = 2, RateCode = RefCusTaxOrFeeCodes.DDF };

			var tpfDutyCalculation1 = new DutyCalculationIntermediateResult { Amount = 3, RateCode = RefCusTaxOrFeeCodes.TPF };
			var tpfDutyCalculation2 = new DutyCalculationIntermediateResult { Amount = 4, RateCode = RefCusTaxOrFeeCodes.TPF };

			var dutyCalculationEntryLineTempResultsCache = new Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>>
			{
				[entryLine1] = new List<DutyCalculationIntermediateResult>(),
				[entryLine2] = new List<DutyCalculationIntermediateResult>()
			};
			dutyCalculationEntryLineTempResultsCache[entryLine1].Add(tpfDutyCalculation);
			dutyCalculationEntryLineTempResultsCache[entryLine1].Add(ddfDutyCalculation);
			dutyCalculationEntryLineTempResultsCache[entryLine2].Add(tpfDutyCalculation1);
			dutyCalculationEntryLineTempResultsCache[entryLine2].Add(tpfDutyCalculation2);

			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);

			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Values.SelectMany(x => x).Count(), NUnit.Framework.Is.EqualTo(4));

			entryLine1.CL_CustomsValue = 1000m;
			entryLine2.CL_CustomsValue = 1000m;
			entryLine1.CL_AdValoremTariff = "240";
			entryInstruction.CEI_WaiverOfExemption = true;
			calculateDutiesHelper.ExemptDutyCalculation(cusEntryHeader, dutyCalculationEntryLineTempResultsCache);
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Values.SelectMany(x => x).Count(), NUnit.Framework.Is.EqualTo(4));

			entryInstruction.CEI_WaiverOfExemption = false;
			calculateDutiesHelper.ExemptDutyCalculation(cusEntryHeader, dutyCalculationEntryLineTempResultsCache);
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Values.SelectMany(x => x).Count(), NUnit.Framework.Is.EqualTo(4));

			entryLine1.CL_CustomsValue = 10m;
			entryLine2.CL_CustomsValue = 1989;
			cusEntryHeader.ResetTotalsAndCachedValues();
			calculateDutiesHelper.ExemptDutyCalculation(cusEntryHeader, dutyCalculationEntryLineTempResultsCache);
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Values.SelectMany(x => x).Count(), NUnit.Framework.Is.EqualTo(4));

			entryLine1.CL_AdValoremTariff = "aa";
			calculateDutiesHelper.ExemptDutyCalculation(cusEntryHeader, dutyCalculationEntryLineTempResultsCache);
			NUnit.Framework.Assert.That(dutyCalculationEntryLineTempResultsCache.Count, NUnit.Framework.Is.EqualTo(0));
		}

		public void TestClearTpfUnderMinimumThresholdWhenDeclarationDateIsEmpty()
		{
			TariffDataForTestHelper.NewData(Factory);
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_WaiverOfExemption = true;
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_VatPymntMthd = "CAS";
			AssertNoExceptionThrown(() => new LineMerger(declaration).DoMerge());
		}

		[ExpectNoExceptions]
		public void TestCalculateImportDuties()
		{
			TariffDataForTestHelper.NewData(Factory);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceHeader.JZ_InvoiceAmount = 10000m;

			var entryLine = cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1m;
			entryLine.CL_CustomsValue = 10000m;

			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);
			var entryLineUniversalData = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
			var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();

			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineUniversalData, null, invoiceLine, dutyCalculationIntermediateResults);
			var dtaCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "CAS");
			CombineAssertions("CAS", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("CAS", 10000m, 0.3M, 3000m, dtaCAS);
			});

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineUniversalData, null, invoiceLine, dutyCalculationIntermediateResults);
			var dtaDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "DEF");
			CombineAssertions("DEF", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("DEF", 10000m, 0.3M, 3000m, dtaDEF);
			});

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine.JI_UseOneTenthCV = true;
			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.RorPayment;

			entryLineUniversalData = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
			var entryLineUniversalDataForRorCashPayment = new EntryLineUniversalRate(entryLine);
			var entryLineUniversalDataForRorNonCashPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation - entryLineUniversalDataForRorCashPayment.ValueForDuty);
			var entryLineUniversalDataForRorAllPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
			(EntryLineUniversalRate, EntryLineUniversalRate, EntryLineUniversalRate)? entryLineUniversalDataForROR = (entryLineUniversalDataForRorCashPayment, entryLineUniversalDataForRorNonCashPayment, entryLineUniversalDataForRorAllPayment);

			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, dutyCalculationIntermediateResults);
			dtaCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "CAS");
			dtaDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "DEF");
			CombineAssertions("ROR", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(2));
				AssertDutyCalculationIntermediateResult("CAS", 1000m, 0.3M, 300m, dtaCAS);
				AssertDutyCalculationIntermediateResult("DEF", 9000m, 0.3M, 2700m, dtaDEF);
			});

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, dutyCalculationIntermediateResults);
			dtaDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "CAS");
			CombineAssertions("CAS", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("CAS", 10000m, 0.3M, 3000m, dtaDEF);
			});

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, dutyCalculationIntermediateResults);
			dtaDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "DTA" && x.PaymentMethod == "DEF");
			CombineAssertions("DEF", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("DEF", 10000m, 0.3M, 3000m, dtaDEF);
			});
		}

		[ExpectNoExceptions]
		public void TestCalculateImportDutiesOnlyOutputHighestAmount()
		{
			TariffDataForTestHelper.NewData(Factory);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceHeader.JZ_InvoiceAmount = 10000m;

			var entryLine = cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87031000004";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1m;

			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);
			var entryLineData = new EntryLineUniversalRate(entryLine);
			var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();

			var randomLine = entryLine.RandomLine;
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineData, null, randomLine, dutyCalculationIntermediateResults);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults[0].Amount, NUnit.Framework.Is.EqualTo(3000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults[0].Rate, NUnit.Framework.Is.EqualTo(0.3m).Using(CustomComparers.TypeComparison));

			invoiceLine.JI_CustomsQuantity = 10m;
			dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();
			entryLineData = new EntryLineUniversalRate(entryLine);
			calculateDutiesHelper.CalculateImportDuties(entryLine, entryLineData, null, randomLine, dutyCalculationIntermediateResults);
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults[0].Amount, NUnit.Framework.Is.EqualTo(10000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyCalculationIntermediateResults[0].Rate, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateInvoiceLineAdditionalTax()
		{
			TariffDataForTestHelper.GenerateAdditionalTaxTariff(Factory);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceHeader.JZ_InvoiceAmount = 10000m;

			var entryLine = cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1m;
			entryLine.CL_CustomsValue = 10000m;

			(string Type, string Tariff)[] testCases = new (string, string)[]
			{
				("CT", "OtherBeverage"),
				("AT", "Distilled"),
				("TT", "Gasoline"),
				("SS", "Forniture"),
			};

			var tax = invoiceLine.Taxes.AddNew();
			foreach (var testCase in testCases)
			{
				tax.JLT_Type = testCase.Type;
				tax.JLT_Tariff = testCase.Tariff;

				var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);
				var entryLineUniversalData = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
				var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();

				dutyCalculationIntermediateResults.Clear();
				tax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
				calculateDutiesHelper.CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, null, invoiceLine, "DTY", dutyCalculationIntermediateResults);
				var tatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "CAS");
				CombineAssertions($"[{testCase.Type}]CAS", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
					AssertDutyCalculationIntermediateResult("CAS", 10000m, 0.15M, 1500m, tatCAS);
				});

				dutyCalculationIntermediateResults.Clear();
				tax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
				calculateDutiesHelper.CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, null, invoiceLine, "DTY", dutyCalculationIntermediateResults);
				var tatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "DEF");
				CombineAssertions($"[{testCase.Type}]DEF", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
					AssertDutyCalculationIntermediateResult("DEF", 10000m, 0.15M, 1500m, tatDEF);
				});

				invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				invoiceLine.JI_UseOneTenthCV = true;
				tax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.RorPayment;

				entryLineUniversalData = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
				var entryLineUniversalDataForRorCashPayment = new EntryLineUniversalRate(entryLine);
				var entryLineUniversalDataForRorNonCashPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation - entryLineUniversalDataForRorCashPayment.ValueForDuty);
				var entryLineUniversalDataForRorAllPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValueForCustomsValuation);
				(EntryLineUniversalRate, EntryLineUniversalRate, EntryLineUniversalRate)? entryLineUniversalDataForROR = (entryLineUniversalDataForRorCashPayment, entryLineUniversalDataForRorNonCashPayment, entryLineUniversalDataForRorAllPayment);

				dutyCalculationIntermediateResults.Clear();
				calculateDutiesHelper.CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, "DTY", dutyCalculationIntermediateResults);
				tatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "CAS");
				tatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "DEF");
				CombineAssertions($"[{testCase.Type}]ROR CAS", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(2));
					AssertDutyCalculationIntermediateResult("CAS", 1000m, 0.15M, 150m, tatCAS);
					AssertDutyCalculationIntermediateResult("DEF", 9000m, 0.15M, 1350m, tatDEF);
				});

				dutyCalculationIntermediateResults.Clear();
				tax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
				calculateDutiesHelper.CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, "DTY", dutyCalculationIntermediateResults);
				tatCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "CAS");
				CombineAssertions($"[{testCase.Type}]ROR CAS", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
					AssertDutyCalculationIntermediateResult("CAS", 10000m, 0.15M, 1500m, tatCAS);
				});

				dutyCalculationIntermediateResults.Clear();
				tax.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
				calculateDutiesHelper.CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, "DTY", dutyCalculationIntermediateResults);
				tatDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == "TAT" && x.PaymentMethod == "DEF");
				CombineAssertions($"[{testCase.Type}]ROR DEF", () =>
				{
					NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
					AssertDutyCalculationIntermediateResult("DEF", 10000m, 0.15M, 1500m, tatDEF);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestCalculateTPF()
		{
			TariffDataForTestHelper.NewData(Factory);
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100000m;
			entryLine.CL_ValueForVAT = 100000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;

			var dutyCalculationIntermediateResults = new List<DutyCalculationIntermediateResult>();
			var calculateDutiesHelper = new DutyCalculatorStrategy(declaration);

			var entryLineUniversalData = new EntryLineUniversalRate(entryLine, 100000m);
			var entryLineUniversalDataForRorCashPayment = new EntryLineUniversalRate(entryLine, 10000m);
			var entryLineUniversalDataForRorNonCashPayment = new EntryLineUniversalRate(entryLine, 90000m);
			var entryLineUniversalDataForRorAllPayment = new EntryLineUniversalRate(entryLine, 100000m);
			(EntryLineUniversalRate, EntryLineUniversalRate, EntryLineUniversalRate)? entryLineUniversalDataForROR = (entryLineUniversalDataForRorCashPayment, entryLineUniversalDataForRorNonCashPayment, entryLineUniversalDataForRorAllPayment);

			calculateDutiesHelper.CalculateTPF(cusEntryHeader, entryLine, entryLineUniversalData, null, 1.0, DutyTaxPaymentMethodList.Codes.CashPayment, entryLineUniversalData.DateOfValuation, dutyCalculationIntermediateResults);
			var tpfCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment);
			CombineAssertions("CAS", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("CAS", 100000m, 0.0004M, 40m, tpfCAS);
			});

			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateTPF(cusEntryHeader, entryLine, entryLineUniversalData, null, 1.0, DutyTaxPaymentMethodList.Codes.NonCashPayment, entryLineUniversalData.DateOfValuation, dutyCalculationIntermediateResults);
			var tpfDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment);
			CombineAssertions("DEF", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("DEF", 100000m, 0.0004M, 40m, tpfDEF);
			});

			invoiceLine.JI_Procedure = "38";
			invoiceLine.JI_UseOneTenthCV = true;
			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateTPF(cusEntryHeader, entryLine, entryLineUniversalData, entryLineUniversalDataForROR, 1.0, DutyTaxPaymentMethodList.Codes.RorPayment, entryLineUniversalData.DateOfValuation, dutyCalculationIntermediateResults);
			tpfCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment);
			tpfDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment);
			CombineAssertions("ROR ROR", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(2));
				AssertDutyCalculationIntermediateResult("CAS", 10000m, 0.0004M, 4m, tpfCAS);
				AssertDutyCalculationIntermediateResult("DEF", 90000m, 0.0004M, 36m, tpfDEF);
			});

			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateTPF(cusEntryHeader, entryLine, entryLineUniversalData, entryLineUniversalDataForROR, 1.0, DutyTaxPaymentMethodList.Codes.CashPayment, entryLineUniversalData.DateOfValuation, dutyCalculationIntermediateResults);
			tpfCAS = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment);
			CombineAssertions("ROR CAS", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("CAS", 100000m, 0.0004M, 40m, tpfCAS);
			});

			dutyCalculationIntermediateResults.Clear();
			calculateDutiesHelper.CalculateTPF(cusEntryHeader, entryLine, entryLineUniversalData, entryLineUniversalDataForROR, 1.0, DutyTaxPaymentMethodList.Codes.NonCashPayment, entryLineUniversalData.DateOfValuation, dutyCalculationIntermediateResults);
			tpfDEF = dutyCalculationIntermediateResults.SingleOrDefault(x => x.RateCode == RefCusTaxOrFeeCodes.TPF && x.PaymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment);
			CombineAssertions("ROR DEF", () =>
			{
				NUnit.Framework.Assert.That(dutyCalculationIntermediateResults.Count, NUnit.Framework.Is.EqualTo(1));
				AssertDutyCalculationIntermediateResult("DEF", 100000m, 0.0004M, 40m, tpfDEF);
			});
		}

		#region Implementation

		[ExpectNoExceptions]
		void AssertDutyCalculationIntermediateResult(string message, ZDecimal expectedBaseValue, ZDecimal expectedRate, ZDecimal expectedAmount, DutyCalculationIntermediateResult actual)
		{
			var resultBuilder = new StringBuilder();
			var hasError = false;
			resultBuilder.AppendLine(message);
			if (actual == null)
			{
				hasError = true;
				resultBuilder.AppendLine("Actual is NULL");
			}
			else
			{
				resultBuilder.AppendLine("          |  expected  | actual");
				if (expectedBaseValue != actual.BaseValue)
				{
					hasError = true;
					resultBuilder.AppendLine($"BaseValue | {expectedBaseValue,10} | {actual.BaseValue}");
				}
				if (expectedRate != actual.Rate)
				{
					hasError = true;
					resultBuilder.AppendLine($"Rate      | {expectedRate,10} | {actual.Rate}");
				}
				if (expectedAmount != actual.Amount)
				{
					hasError = true;
					resultBuilder.AppendLine($"Amount    | {expectedAmount,10} | {actual.Amount}");
				}
			}
			NUnit.Framework.Assert.That(!hasError, NUnit.Framework.Is.True, resultBuilder.ToString());
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader cusEntryHeader;

		protected override void SetUp()
		{
			base.SetUp();
			CustomSetup();
		}

		void CustomSetup()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader = declaration.Invoices.AddNew();
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CusEntryInstruction;
		}

		#endregion
	}
}
