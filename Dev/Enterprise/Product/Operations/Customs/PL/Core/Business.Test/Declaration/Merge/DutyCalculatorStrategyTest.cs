using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using DutyCalculatorStrategy = Enterprise.Customs.PL.Business.Declaration.DutyCalculatorStrategy;
using LineMerger = Enterprise.Customs.PL.Business.Declaration.LineMerger;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(DutyCalculatorStrategy))]
sealed class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
{
	public void TestGetNewEntryLineDutyCalculator()
	{
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);
		AssertType<EntryLineDutyCalculator>(dutyCalculatorStrategyExposed.GetNewEntryLineDutyCalculator_Exposed(entryLine));
	}

	public void TestShouldCalculateDutiesForEntryLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = ZDateTime.Today;
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion);
		var rate = usdCurrency.ExchangeRates.AddNew();
		rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		rate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
		rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
		rate.RE_SellRate = 2;
		rate.RE_GC = GlbCompany.CurrentCompany.PK;

		DoMerge(declaration);

		CombineAssertions(() =>
		{
			var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.First();
			var entryLine = (Customs.Business.CusEntryLine)entryHeader.AllEntryLines.First();

			AssertEquals("Import, CUD exists", true, dutyCalculatorStrategyExposed.ShouldCalculateDutiesForEntryLine_Exposed((EU.Business.Declaration.CusEntryLine)entryLine));

			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			AssertEquals("Import, no CUD", false, dutyCalculatorStrategyExposed.ShouldCalculateDutiesForEntryLine_Exposed((EU.Business.Declaration.CusEntryLine)entryLine));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			entryInstruction.CEI_DateForDuty = ZDateTime.Now.AddDays(3);
			dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

			AssertEquals("Export, even no CUD", true, dutyCalculatorStrategyExposed.ShouldCalculateDutiesForEntryLine_Exposed((EU.Business.Declaration.CusEntryLine)entryLine));
		});
	}

	public void TestGetRateSelectionCriteria()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var strategy = new DutyCalculatorStrategyExposed(declaration);
		var criteria = strategy.GetRateSelectionCriteriaExposed(invoiceLine);

		CombineAssertions(() =>
		{
			AssertEquals("Rate Selection Criteria count is 4", 4, criteria.Count());
			AssertEquals("The result of method should include excise rate selection criteria", true, criteria.Any(p => p == invoiceLine.ExciseRateSelectionCriteria));
		});
	}

	public void TestAddNewEntryLineFee()
	{
		PopulateExchangeRateData();
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

		var dutyCalculationIntermediateResult1 = new DutyCalculationIntermediateResult(6.0m, 1.0, 4.0, "%")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", dutyCalculationIntermediateResult1);

		var dutyCalculationIntermediateResult2 = new DutyCalculationIntermediateResult(6.0m, 1.0, 4.0m, "%")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, TaxTypeList.Codes.ExciseTax, "", dutyCalculationIntermediateResult2);

		var dutyCalculationIntermediateResult3 = new DutyCalculationIntermediateResult(9.0m, 2.0, 8.0m, "KG")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.Vat, "ADD", dutyCalculationIntermediateResult3);

		var dutyCalculationIntermediateResult4 = new DutyCalculationIntermediateResult(10.0m, ZDecimal.Zero, 20.0m, ZString.Empty)
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, "fdd", "", dutyCalculationIntermediateResult4);

		CombineAssertions(() =>
		{
			AssertEquals("4 Fees", 4, entryLine.Fees.Count);
			var customsDutyOnIndustrialProductsFee = entryLine.Fees[0];
			AssertEquals("1st Fee - CF_ChargeType", RefCusRateCodes.CustomsDutyOnIndustrialProducts, customsDutyOnIndustrialProductsFee.CF_ChargeType);
			AssertEquals("1st Fee - CF_RateOverrideReasonCode", ZString.Empty, customsDutyOnIndustrialProductsFee.CF_RateOverrideReasonCode);
			AssertEquals("1st Fee - CF_MethodOfCalculation", "%", customsDutyOnIndustrialProductsFee.CF_MethodOfCalculation);
			AssertEquals("1st Fee - CF_Rate", 100m, customsDutyOnIndustrialProductsFee.CF_Rate);
			AssertEquals("1st Fee - CF_BaseValue", 8.0m, customsDutyOnIndustrialProductsFee.CF_BaseValue);
			AssertEquals("1st Fee - CF_ChargeAmount", 12.0m, customsDutyOnIndustrialProductsFee.CF_ChargeAmount);

			var entryLineFee = entryLine.Fees[1];
			AssertEquals("2nd Fee - CF_ChargeType", TaxTypeList.Codes.ExciseTax, entryLineFee.CF_ChargeType);
			AssertEquals("2nd Fee - CF_RateOverrideReasonCode", ZString.Empty, entryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("2nd Fee - CF_MethodOfCalculation", "%", entryLineFee.CF_MethodOfCalculation);
			AssertEquals("2nd Fee - CF_Rate", 100.0m, entryLineFee.CF_Rate);
			AssertEquals("2nd Fee - CF_BaseValue", 20.0m, entryLineFee.CF_BaseValue);
			AssertEquals("2nd Fee - CF_ChargeAmount", 6.0m, entryLineFee.CF_ChargeAmount);

			entryLineFee = entryLine.Fees[2];
			AssertEquals("3nd Fee - CF_ChargeType", RefCusRateCodes.Vat, entryLineFee.CF_ChargeType);
			AssertEquals("3nd Fee - CF_RateOverrideReasonCode", "ADD", entryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("3nd Fee - CF_MethodOfCalculation", "KG", entryLineFee.CF_MethodOfCalculation);
			AssertEquals("3nd Fee - CF_Rate", 2.0m, entryLineFee.CF_Rate);
			AssertEquals("3nd Fee - CF_BaseValue", 8.0m, entryLineFee.CF_BaseValue);
			AssertEquals("3nd Fee - CF_ChargeAmount", 9.0m, entryLineFee.CF_ChargeAmount);

			entryLineFee = entryLine.Fees[3];
			AssertEquals("4rd Fee - CF_ChargeType", "fdd", entryLineFee.CF_ChargeType);
			AssertEquals("4rd Fee - CF_RateOverrideReasonCode", ZString.Empty, entryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("4rd Fee - CF_MethodOfCalculation", ZString.Empty, entryLineFee.CF_MethodOfCalculation);
			AssertEquals("4rd Fee - CF_Rate", ZDecimal.Zero, entryLineFee.CF_Rate);
			AssertEquals("4rd Fee - CF_BaseValue", 20m, entryLineFee.CF_BaseValue);
			AssertEquals("4rd Fee - CF_ChargeAmount", 10m, entryLineFee.CF_ChargeAmount);
		});
	}

	public void TestExciseBaseValue()
	{
		PopulateExchangeRateData();
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

		var customsDutyOnIndustrialProductsIntermediateResult = new DutyCalculationIntermediateResult(6.0m, 1.0, 4.0m, "%")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", customsDutyOnIndustrialProductsIntermediateResult);

		var exciseTaxDutyCalculationIntermediateResult = new DutyCalculationIntermediateResult(6.0m, 1.0, 4.0m, "%")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, TaxTypeList.Codes.ExciseTax, "", exciseTaxDutyCalculationIntermediateResult);

		CombineAssertions(() =>
		{
			AssertEquals("2 Fees", 2, entryLine.Fees.Count);
			var customsDutyOnIndustrialProductsFee = entryLine.Fees[0];
			AssertEquals("CustomsDutyOnIndustrialProducts fee CF_ChargeType", RefCusRateCodes.CustomsDutyOnIndustrialProducts, customsDutyOnIndustrialProductsFee.CF_ChargeType);
			var exciseFee = entryLine.Fees[1];
			AssertEquals("Excise fee CF_ChargeType", TaxTypeList.Codes.ExciseTax, exciseFee.CF_ChargeType);
			AssertEquals("exciseFee CF_BaseValue", customsDutyOnIndustrialProductsFee.CF_BaseValue + customsDutyOnIndustrialProductsFee.CF_ChargeAmount, exciseFee.CF_BaseValue);
		});
	}

	public void TestAddNewEntryLineFee_MethodOfPayment()
	{
		PopulateExchangeRateData();
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

		var dutyCalculationIntermediateResult1 = new DutyCalculationIntermediateResult(ZDecimal.Zero, 1.0m, 4.0m, "%")
		{
			MethodOfPayment = PLMethodOfPaymentList.Codes.R,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", dutyCalculationIntermediateResult1);

		var dutyCalculationIntermediateResult2 = new DutyCalculationIntermediateResult(ZDecimal.Zero, 2.0, 8.0m, "KG")
		{
			MethodOfPayment = PLMethodOfPaymentList.Codes.A,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.Vat, "ADD", dutyCalculationIntermediateResult2);

		var dutyCalculationIntermediateResult3 = new DutyCalculationIntermediateResult(10.0m, ZDecimal.Zero, 20.0m, ZString.Empty)
		{
			MethodOfPayment = PLMethodOfPaymentList.Codes.R,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, "fdd", "", dutyCalculationIntermediateResult3);

		var dutyCalculationIntermediateResult4 = new DutyCalculationIntermediateResult(ZDecimal.Zero, 20.0m, ZDecimal.Zero, ZString.Empty)
		{
			MethodOfPayment = PLMethodOfPaymentList.Codes.E,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, "DDD", "", dutyCalculationIntermediateResult4);

		CombineAssertions(() =>
		{
			var entryLineFee = entryLine.Fees[0];
			AssertEquals("1st Fee - CF_MethodOfPayment updated due to payment method M and empty charge amount", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);

			entryLineFee = entryLine.Fees[1];
			AssertEquals("2nd Fee - CF_MethodOfPayment not updated due to Method of Payment code", PLMethodOfPaymentList.Codes.A, entryLineFee.CF_MethodOfPayment);

			entryLineFee = entryLine.Fees[2];
			AssertEquals("3rd Fee - CF_MethodOfPayment not updated as Charge Amount is not zero", PLMethodOfPaymentList.Codes.R, entryLineFee.CF_MethodOfPayment);

			entryLineFee = entryLine.Fees[3];
			AssertEquals("4th Fee - CF_MethodOfPayment updated due to payment method F and empty charge amount", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestAddNewEntryLineFee_BaseValueRounding()
	{
		PopulateExchangeRateData();
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

		var intermediateResult = new DutyCalculationIntermediateResult(0, 0,4.56M, ZString.Empty);

		var typesThatShouldBeRounded = new[]
		{
			RefCusRateCodes.CustomsDutyOnIndustrialProducts,
			RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
			RefCusRateCodes.DefinitiveAntiDumpingDuty,
			RefCusRateCodes.ProvisionalAntiDumpingDuty,
			RefCusRateCodes.DefinitiveCountervailingDuty
		};

		foreach (var type in typesThatShouldBeRounded)
		{
			dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, type, string.Empty, intermediateResult);
		}

		CombineAssertions(() =>
		{
			foreach (var (value, i) in typesThatShouldBeRounded.Select((value, i) => (value, i)))
			{
				var entryLineFee = entryLine.Fees[i];
				AssertEquals(value, 9M, entryLineFee.CF_BaseValue);
			}
		});
	}

	public void TestMethodOfPaymentForProvisionalAntiDumpingDuty()
	{
		const string tariffCode = "123456789";
		const string additionalCode = "112233";
		const string import = JobMessageTypeList.Codes.Import;

		const string rateCodeA00 = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		const string rateCodeA35 = RefCusRateCodes.ProvisionalAntiDumpingDuty;

		const string rateFormula = "0";

		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType(import)
			.AddTariff(tariffCode)
			.AddRateCode(RateTypeEnum.Duty, rateCodeA00, rateFormula, string.Empty, additionalCode)
			.AddRateCode(RateTypeEnum.Antidumping, rateCodeA35, rateFormula, string.Empty, additionalCode)
			.DutyTariffTypeConfigurationBuilder
			.DutyReferenceDataConfigurationBuilder
			.Configure();

		Factory.Save();

		var exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today;
		exchangeRate.RE_ExpiryDate = ZDateTime.Today;
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_SellRate = 2m;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Tariff = tariffCode;
		invoiceLine.JI_SupplementaryCode1 = additionalCode;

		const string mopA = PLMethodOfPaymentList.Codes.A;
		const string mopD = PLMethodOfPaymentList.Codes.D;
		const string mopZ = PLMethodOfPaymentList.Codes.Z;

		var testCases = new[]
		{
			new { description = "MoP Duty is not equal D or Z.", MoPDuty = mopA, expectedA00MoP = mopA, expectedA35MoP = mopD },
			new { description = "MoP Duty is equal D.", MoPDuty = mopD, expectedA00MoP = mopD, expectedA35MoP = mopD },
			new { description = "MoP Duty is equal Z.", MoPDuty = mopZ, expectedA00MoP = mopZ, expectedA35MoP = mopZ },
		};

		CombineAssertions(() =>
		{
			foreach (var item in testCases)
			{
				declaration.CustomsEntryHeaders.RemoveAll();
				declaration.JE_PaymentMethod = item.MoPDuty;

				DoMerge(declaration);

				var fees = declaration.CustomsEntryHeaders.Single().MergedLines.Single().Fees;

				AssertEquals(item.description + " Fees count", 2, fees.Count);
				AssertEquals(item.description + " Fee A00 MoP", item.expectedA00MoP, fees[0].CF_MethodOfPayment);
				AssertEquals(item.description + " Fee A35 MoP", item.expectedA35MoP, fees[1].CF_MethodOfPayment);
			}
		});
	}

	public void TestGetNonVatableExtraFeeCalculatorCollectionCore()
	{
		const string testTaxCode = "VAT";
		const decimal testTaxRate = 0.19m;
		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTaxOrFee(testTaxCode, taxRate: testTaxRate)
			.Configure();
		Factory.Save();

		const string rateCodeA00 = TaxTypeList.Codes.CustomsDuties;
		const string rateCodeA35 = TaxTypeList.Codes.ProvisionalAntidumpingDuties;
		new[] { rateCodeA00, rateCodeA35 }.ForEach(x =>
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "ADD", x));

		var declaration = Factory.New<JobDeclaration>();
		var entryLine = Factory.New<Declaration.CusEntryLine>();
		var entryLineFee = entryLine.Fees.AddNew();
		var strategy = new DutyCalculatorStrategyExposed(declaration);

		var testCases = new (string, string, string, bool)[]
		{
			("Fee code is A35 and Tax code is defined", testTaxCode, rateCodeA35, true),
			("Fee code is not A35", testTaxCode, rateCodeA00, false),
			("Tax code is not defined", null, rateCodeA35, false)
		};

		CombineAssertions(() =>
		{
			foreach (var (description, taxCode, rateCode, shouldCreateCalculator) in testCases)
			{
				entryLine.RandomLine.JI_ZZF_NKTaxType = taxCode;
				entryLineFee.CF_ChargeType = rateCode;

				var calculators = strategy.GetNonVatableExtraFeeCalculatorCollectionCore_Exposed(entryLine);
				var expectedCount = shouldCreateCalculator ? 1 : 0;
				AssertEquals($"{description} - Calculators count", expectedCount, calculators.Count());

				if (expectedCount == 1)
				{
					var calculator = calculators.Single();
					AssertType<GuaranteedChargesCalculator>($"{description} - Calculator type", calculator);
				}
			}
		});
	}

	public void TestAddNewEntryLineFee_WithAgriculturalDuty()
	{
		PopulateExchangeRateData();
		var (entryLine, declaration) = GetNewEntryLine();
		var dutyCalculatorStrategyExposed = new DutyCalculatorStrategyExposed(declaration);

		var eaDutyCalculationIntermediateResult1 = new DutyCalculationIntermediateResult(6.0m, 1.0, 4.0m, "%")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
			OriginalMeursingExpression = "EA(1)"
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", eaDutyCalculationIntermediateResult1);

		var adszDutyCalculationIntermediateResult = new DutyCalculationIntermediateResult(9.0m, 2.0, 8.0m, "KG")
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
			OriginalMeursingExpression = "ADSZ(2)"
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", adszDutyCalculationIntermediateResult);

		var dutyCalculationIntermediateResult = new DutyCalculationIntermediateResult(10.0m, ZDecimal.Zero, 20.0m, ZString.Empty)
		{
			MethodOfPayment = ZString.Empty,
			ParticipatingExpression = true,
		};
		dutyCalculatorStrategyExposed.AddNewEntryLineFee_Exposed(entryLine, RefCusRateCodes.CustomsDutyOnIndustrialProducts, "", dutyCalculationIntermediateResult);

		CombineAssertions(() =>
		{
			AssertEquals("3 Fees", 3, entryLine.Fees.Count);
			var eaEntryLineFee = entryLine.Fees[0];
			AssertEquals("EA A20 Fee - CF_ChargeType", "A20", eaEntryLineFee.CF_ChargeType);
			AssertEquals("EA A20 Fee - CF_RateOverrideReasonCode", ZString.Empty, eaEntryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("EA A20 Fee - CF_MethodOfCalculation", "%", eaEntryLineFee.CF_MethodOfCalculation);
			AssertEquals("EA A20 Fee - CF_Rate", 100m, eaEntryLineFee.CF_Rate);
			AssertEquals("EA A20 Fee - CF_BaseValue", 8.0m, eaEntryLineFee.CF_BaseValue);
			AssertEquals("EA A20 Fee - CF_ChargeAmount", 12.0m, eaEntryLineFee.CF_ChargeAmount);

			var adszEntryLineFee = entryLine.Fees[1];
			AssertEquals("ADSZ A20 Fee - CF_ChargeType", "A20", adszEntryLineFee.CF_ChargeType);
			AssertEquals("ADSZ A20 Fee - CF_RateOverrideReasonCode", "", adszEntryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("ADSZ A20 Fee - CF_MethodOfCalculation", "KG", adszEntryLineFee.CF_MethodOfCalculation);
			AssertEquals("ADSZ A20 Fee - CF_Rate", 2.0m, adszEntryLineFee.CF_Rate);
			AssertEquals("ADSZ A20 Fee - CF_BaseValue", 16.0m, adszEntryLineFee.CF_BaseValue);
			AssertEquals("ADSZ A20 Fee - CF_ChargeAmount", 18.0m, adszEntryLineFee.CF_ChargeAmount);

			var dutyEntryLineFee = entryLine.Fees[2];
			AssertEquals("A00 Fee - CF_ChargeType", "A00", dutyEntryLineFee.CF_ChargeType);
			AssertEquals("A00 Fee - CF_RateOverrideReasonCode", ZString.Empty, dutyEntryLineFee.CF_RateOverrideReasonCode);
			AssertEquals("A00 Fee - CF_MethodOfCalculation", ZString.Empty, dutyEntryLineFee.CF_MethodOfCalculation);
			AssertEquals("A00 Fee - CF_Rate", ZDecimal.Zero, dutyEntryLineFee.CF_Rate);
			AssertEquals("A00 Fee - CF_BaseValue", 40m, dutyEntryLineFee.CF_BaseValue);
			AssertEquals("A00 Fee - CF_ChargeAmount", 20m, dutyEntryLineFee.CF_ChargeAmount);
		});
	}

	protected override ZDecimal CalculateValueForVAT_ExpectedEntryLine1Value => 550M;
	protected override ZDecimal CalculateValueForVAT_ExpectedEntryLine2Value => 550M;
	protected override ZDecimal CalculateValueForVAT_ExpectedLocalCurrencyValue => 1000M;

	public override void TestCalculateDutiesAndVat()
	{
		PopulateExchangeRateData();
		base.TestCalculateDutiesAndVat();
	}

	protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

	protected override void DoMerge(EU.Business.Declaration.JobDeclaration declaration) => new LineMerger((JobDeclaration)declaration).DoMerge();

	protected override ZString VATableAdditionChargeCode => PLCustomsChargeTypeList.Codes.AL;

	protected override ZString NonVATableDeductionChargeCode => EmptyChargeCode;

	protected override ZDecimal ExpectedVATChargeAmountA => 160M;

	protected override ZDecimal ExpectedVATBaseValueA => 727M;

	protected override ZDecimal ExpectedVATChargeAmountB => 40M;

	protected override ZDecimal ExpectedVATBaseValueB => 182M;

	protected override IReadOnlyList<FeeAssertionObject> EntryLineAExpectedFees => new[]
	{
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 148M, BaseValue = 140M, Rate = 0, MethodOfCalculation = string.Empty, OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 30M, BaseValue = 140M, Rate = 0, MethodOfCalculation = string.Empty, OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 300m, BaseValue = 1000m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 7m, BaseValue = 14m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 42M, BaseValue = 140M, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 60m, BaseValue = 1000m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountA, BaseValue = ExpectedVATBaseValueA, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
	};

	protected override IReadOnlyList<FeeAssertionObject> EntryLineBExpectedFees => new[]
	{
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 38M, BaseValue = 36M, Rate = 0, MethodOfCalculation = string.Empty, OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 78m, BaseValue = 260m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 2M, BaseValue = 8M, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 1m, BaseValue = 3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 11M, BaseValue = 36M, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 15.6m, BaseValue = 260m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
		new FeeAssertionObject { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountB, BaseValue = ExpectedVATBaseValueB, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
	};

	(Declaration.CusEntryLine, JobDeclaration) GetNewEntryLine()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = ZDateTime.Today;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		return (entryHeader.AllEntryLines.First(), declaration);
	}

	void PopulateExchangeRateData()
	{
		GlbCompany.CurrentCompany.GC_IsReciprocal = true;
		var exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
		exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
		exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 2.0m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		var cusExchangeRate = RefExchangeRate.New(Factory);
		cusExchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
		cusExchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
		cusExchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
		cusExchangeRate.RE_SellRate = 2.0m;
		cusExchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		cusExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}
}

class DutyCalculatorStrategyExposed : DutyCalculatorStrategy
{
	public DutyCalculatorStrategyExposed(JobDeclaration declaration) : base(declaration)
	{
	}

	public bool ShouldCalculateDutiesForEntryLine_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => base.ShouldCalculateDutiesForEntryLine(entryLine);

	public EntryLineDutyCalculator GetNewEntryLineDutyCalculator_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => (EntryLineDutyCalculator)base.GetNewEntryLineDutyCalculator(entryLine);

	public void AddNewEntryLineFee_Exposed(EU.Business.Declaration.CusEntryLine entryLine, ZString rateCode, ZString overrideReason, DutyCalculationIntermediateResult calculatedFee)
		=> base.AddNewEntryLineFee(entryLine, rateCode, overrideReason, calculatedFee);

	public IEnumerable<EU.Business.Declaration.IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore_Exposed(EU.Business.Declaration.CusEntryLine entryLine)
		=> base.GetNonVatableExtraFeeCalculatorCollectionCore(entryLine);

	public IEnumerable<Universal.IZZRateSelectionCriteria> GetRateSelectionCriteriaExposed(BaseJobComInvoiceLine invoiceLine)
		=> base.GetRateSelectionCriteria(invoiceLine);
}
