using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		[TestDate(1990, 6, 1)]
		public override void TestCalculateDuties()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(1990, 1, 1),
				EndDate = new ZDateTime(1991, 1, 1),
				TariffRebateRateFormula = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3)*0.1",
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("200", "EUTRADE Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
			}.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			var tariffType6P4 = helper.Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P4");
			var tariff29 = helper.Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P4.PK, "1010101039", helper.StartDate, helper.EndDate);
			helper.Helper.CreateTariffRelationship(tariff29.PK, helper.Tariff1.ZZ1_ZZI_TariffType, helper.Tariff1.ZZ1_TariffCode);
			var tariff29Rate = helper.Helper.CreateRate(tariff29, helper.RateCode_ZA_REF_D.PK, helper.StartDate, helper.EndDate, @"(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5+6P1+6P2+6P3+{DECIMAL(6,3):""User Enter Value""})*0.1");
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "6", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, calculateDuty: true, landedCostOnly: true);
			helper.CreateEUTradeGroup();
			helper.Helper.CreateCusApplicability(tariff29Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.SouthAfrica, entryInstruction);
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff2.ZZ1_ZZI_TariffTypeCode, helper.Tariff2.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff4.ZZ1_ZZI_TariffTypeCode, helper.Tariff4.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff10.ZZ1_ZZI_TariffTypeCode, helper.Tariff10.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);
			var tariffDetail29 = invoiceLine.CusLineTariffDetails.AddNew(tariff29.ZZ1_ZZI_TariffTypeCode, tariff29.ZZ1_TariffCode);
			AssertEquals("PreCondition:tariffDetail29.FormulaSpecificQuestion", "User Enter Value", tariffDetail29.FormulaSpecificQuestion);
			tariffDetail29.FormulaSpecificValue = "600";
			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 0m),
				new KeyValuePair<string, decimal>("12A", 0m),
				new KeyValuePair<string, decimal>("12B", 97.83m),
				new KeyValuePair<string, decimal>("13A", 133.39m),
				new KeyValuePair<string, decimal>("2P1", 0m),
				new KeyValuePair<string, decimal>("3P1", 161.40m),
				new KeyValuePair<string, decimal>("6P4", 221.40m),
				new KeyValuePair<string, decimal>("VAT", 186.34m)
			};

			CombineAssertions(() =>
			{
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 231.22m, entryLine.CustomsDuty);
				AssertFees(expectedFeeValues, entryLine.Fees);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_RebateGTDuty()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory) { RebateRateCode = "D" }
				.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.CreateEUTradeGroup(Core.Constants.CountryCodes.China);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 0m),
				new KeyValuePair<string, decimal>("3P1", 1000m),
				new KeyValuePair<string, decimal>("VAT", 154m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 0m, entryLine.CustomsDuty);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_RebateLTDuty()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory) { TariffRebateRateFormula = "0.1 * 1P1", RebateRateCode = "D" }
				.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateStandardTradeGroup();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 90m),
				new KeyValuePair<string, decimal>("3P1", 10m),
				new KeyValuePair<string, decimal>("VAT", 166.6m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 90m, entryLine.CustomsDuty);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_REBATESEQUENCE()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				Tariff2P1RateFormula = "0.327 * VFD",
				TariffRebateRateFormula = "2P1+2P2+2P3",
				RebateRateCode = "D",
				AntiDumpingRateCode = "D",
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateStandardTradeGroup();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.Helper.CreateTariffAttribute("REBATESEQUENCE", "ADD, DTY, REB", helper.Tariff13);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff10.ZZ1_ZZI_TariffTypeCode, helper.Tariff10.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 100m),
				new KeyValuePair<string, decimal>("2P1", 0m),
				new KeyValuePair<string, decimal>("3P1", 327m),
				new KeyValuePair<string, decimal>("VAT", 168m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 100m, entryLine.CustomsDuty);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_RebateExcludingEX1()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				Tariff1P1RateFormula = "0.25 * VFD",
				Tariff12BRateFormula = "0.07 * VFD",
				TariffRebateRateFormula = "1P1+12A+13A+13B+13C+13D+15A+15B",
				RebateRateCode = "D",
				AdValoremExciseRateCode = "D"
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateEUTradeGroup(Core.Constants.CountryCodes.China);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 0m),
				new KeyValuePair<string, decimal>("12B", 80.5m),
				new KeyValuePair<string, decimal>("3P1", 250m),
				new KeyValuePair<string, decimal>("VAT", 165.20m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 80.5m, entryLine.CustomsDuty);
				AssertEquals("entryLine.CL_VPBAmount", 1150m, entryLine.CL_VPBAmount);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_RebateIncludingEX1()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				Tariff1P1RateFormula = "0.2 * VFD",
				Tariff12BRateFormula = "0.1 * VFD",
				Tariff13ARateFormula = "0.01 * VFD",
				Tariff2P1RateFormula = "0.1 * VFD",
				TariffRebateRateFormula = "0.8 * (12B + 2P1 + 2P2 + 2P3)",
				AntiDumpingRateCode = "D",
				RebateRateCode = "D",
				AdValoremExciseRateCode = "D",
				TariffTypeCodeForRebate = "4P1",
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateStandardTradeGroup();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff10.ZZ1_ZZI_TariffTypeCode, helper.Tariff10.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff4.ZZ1_ZZI_TariffTypeCode, helper.Tariff4.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 10.4m),
				new KeyValuePair<string, decimal>("12B", 137m),
				new KeyValuePair<string, decimal>("13A", 10m),
				new KeyValuePair<string, decimal>("2P1", 100m),
				new KeyValuePair<string, decimal>("4P1", 189.6m),
				new KeyValuePair<string, decimal>("VAT", 189.98m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 257.4m, entryLine.CustomsDuty);
				AssertEquals("entryLine.CL_VPBAmount", 1370m, entryLine.CL_VPBAmount);
			});
		}

		[TestDate(2016, 8, 4)]
		public void TestDutyAndTaxCalculation_RebateIncludingEX1_WithRoundingForVPB()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				Tariff1P1RateFormula = "0.2 * VFD",
				Tariff12BRateFormula = "0.1 * VFD",
				Tariff13ARateFormula = "0.01 * VFD",
				Tariff2P1RateFormula = "0.1 * VFD",
				TariffRebateRateFormula = "0.8 * (12B + 2P1 + 2P2 + 2P3)",
				DutyRateCode = "1P1",
				RebateRateCode = "D",
				TariffTypeCodeForRebate = "4P1",
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateEUTradeGroup(Core.Constants.CountryCodes.China);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1481.50m, Core.Constants.CountryCodes.China, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff10.ZZ1_ZZI_TariffTypeCode, helper.Tariff10.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff4.ZZ1_ZZI_TariffTypeCode, helper.Tariff4.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 15.4m),
				new KeyValuePair<string, decimal>("12B",202.9m),
				new KeyValuePair<string, decimal>("13A", 14.81m),
				new KeyValuePair<string, decimal>("2P1", 148.1m),
				new KeyValuePair<string, decimal>("4P1", 280.8m),
				new KeyValuePair<string, decimal>("VAT",281.4m),
			};

			CombineAssertions(() =>
			{
				AssertFees(expectedFeeValues, entryLine.Fees);
				AssertEquals("entryLine.CL_CustomsValue", 1481m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 381.21m, entryLine.CustomsDuty);
			});
		}

		public void AssertFees(KeyValuePair<string, decimal>[] expectedFeeValues, CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> actualFees)
		{
			AssertEquals("entryLine.Fees.Count", expectedFeeValues.Length, actualFees.Count);
			foreach (var pair in expectedFeeValues)
			{
				AssertEquals(pair.Key, pair.Value, actualFees.GetOrAddFeeByFeeType(pair.Key)?.CF_ChargeAmount ?? ZDecimal.Zero);
			}
		}

		public void TestDutyAndTaxCalculation_RebateWontGenerateNegativeDuty()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = ZDateTime.Today.AddDays(-1),
				EndDate = ZDateTime.Today.AddDays(1),
				Tariff1P1RateFormula = "0.2 * VFD",
				Tariff12BRateFormula = "0.1 * VFD",
				Tariff13ARateFormula = "0.01 * VFD",
				Tariff2P1RateFormula = "0.1 * VFD",
				TariffRebateRateFormula = "{\"RebateAmount\"}",
				LevyRateCode = "D",
				AdValoremExciseRateCode = "D",
				AntiDumpingRateCode = "D",
				RebateRateCode = "4P2",
				TariffTypeCodeForRebate = "4P2",
			}.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.CreateStandardTradeGroup();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "XX", "YY", "4", "XXYY4", "IMP");
			Factory.Save();

			var testDeclaration = helper.CreateTestJobDeclaration();
			var testInst = helper.AddEntryInstruction(testDeclaration, "XX");
			var testInv = helper.AddInvoiceHeader(testDeclaration, 0m);
			var testInvLine = helper.AddInvoiceLine(testInv, 1000m, Core.Constants.CountryCodes.China, testInst, "YY");
			testInvLine.CusLineTariffDetails.RemoveAndDeleteAll();
			testInvLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			testInvLine.CusLineTariffDetails.AddNew(helper.Tariff4.ZZ1_ZZI_TariffTypeCode, helper.Tariff4.ZZ1_TariffCode);
			testInvLine.CusLineTariffDetails.AddNew(helper.Tariff10.ZZ1_ZZI_TariffTypeCode, helper.Tariff10.ZZ1_TariffCode);
			var lineTariff4P2 = testInvLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			testDeclaration.DoMerge();
			var testLine = testDeclaration.ActiveEntryHeaders[0].MergedLines[0];
			CombineAssertions("Case 1: No RebateAmount entered for 4P2", () =>
			{
				AssertFees(new[]
				{
					new KeyValuePair<string, decimal>("1P1", 200m),
					new KeyValuePair<string, decimal>("12B", 145m),
					new KeyValuePair<string, decimal>("13A", 10m),
					new KeyValuePair<string, decimal>("2P1", 100m),
					new KeyValuePair<string, decimal>("4P2", 0m),
					new KeyValuePair<string, decimal>("VAT", 217.7m),
				}, testLine.Fees);
			});

			lineTariff4P2.FormulaSpecificValue = "400";
			testDeclaration.DoMerge();
			CombineAssertions("Case 2: 4P2 value not too much", () =>
			{
				AssertFees(new[]
				{
					new KeyValuePair<string, decimal>("1P1", 0m),
					new KeyValuePair<string, decimal>("12B", 5m),
					new KeyValuePair<string, decimal>("13A", 10m),
					new KeyValuePair<string, decimal>("2P1", 0m),
					new KeyValuePair<string, decimal>("4P2", 400m),
					new KeyValuePair<string, decimal>("VAT", 156.1m),
				}, testLine.Fees);
			});

			lineTariff4P2.FormulaSpecificValue = "4000";
			testDeclaration.DoMerge();
			CombineAssertions("Case 2: 4P2 value too much", () =>
			{
				AssertFees(new[]
				{
					new KeyValuePair<string, decimal>("1P1", 0m),
					new KeyValuePair<string, decimal>("12B", 0m),
					new KeyValuePair<string, decimal>("13A", 0m),
					new KeyValuePair<string, decimal>("2P1", 0m),
					new KeyValuePair<string, decimal>("4P2", 4000m),
					new KeyValuePair<string, decimal>("VAT", 154m),
				}, testLine.Fees);
			});
		}

		[TestDate(1990, 6, 1)]
		public void TestDutyAndTaxCalculationForSpecificTariffTypeOnly()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(1990, 1, 1),
				EndDate = new ZDateTime(1991, 1, 1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff12ARateFormula = "0.2 * VFD",
				Tariff13ARateFormula = "0.3 * VFD",
				ExciseRateCode = "D",
				LevyRateCode = "D",
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			var procedure = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "12A", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", ZString.Empty, "12A", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			helper.Helper.CreateTradeGroup(Core.Constants.CountryCodes.Italy, "EUTRADE", helper.StartDate, helper.EndDate);
			helper.Helper.CreateTradeGroup(Core.Constants.CountryCodes.Italy, "STANDARD", helper.StartDate, helper.EndDate);
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.Italy);

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, procedure.ZZ6_ProcedureCode);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.Italy, entryInstruction, procedure.ZZ6_PreviousProcedureCode);

			AssertContainsExactElementsInExactOrder("Only 12A Defaulted", new[] { "12A" }, invoiceLine.CusLineTariffDetails.Select(x => (string)x.BZ_Type));

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("12A", 200.0m),
				new KeyValuePair<string, decimal>("VAT", 182.00m)
			};

			CombineAssertions(() =>
			{
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 200.0m, entryLine.CustomsDuty);
				AssertFees(expectedFeeValues, entryLine.Fees);
			});
		}

		[TestDate(1990, 6, 1)]
		public void TestDutyAndTaxCalculationBLNS_NotBLNS()
		{
			var declaration = DutyCalculatorStrategyTestHelper.SetupForDutyAndTaxCalculationBLNSTests(Factory);
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 100m),
				new KeyValuePair<string, decimal>("12A", 110m),
				new KeyValuePair<string, decimal>("12B", 125m),
				new KeyValuePair<string, decimal>("13A", 133.50m),
				new KeyValuePair<string, decimal>("13B", 146.85m),
				new KeyValuePair<string, decimal>("VAT", 240.10m)
			};

			CombineAssertions("Not BLNS - Both Country of Import and Country of Origin are not BLNS", () =>
			{
				declaration.DoMerge();
				var entryLine = AssertAndGetSingleEntryLine(declaration);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
				AssertFees(expectedFeeValues, entryLine.Fees);
			});

			CombineAssertions("Not BLNS - Country of Import is not BLNS", () =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Swaziland;
				invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
				declaration.DoMerge();
				var entryLine = AssertAndGetSingleEntryLine(declaration);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
				AssertFees(expectedFeeValues, entryLine.Fees);
			});

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ZABLNSVatUplift
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today
				, value: true))
			{
				CombineAssertions("Not BLNS - Country of Origin is not BLNS and ZABLNSVatUplift is ON", () =>
				{
					declaration.JE_RL_NKOrigin = "BWBBK";
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
					invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
					declaration.DoMerge();
					var entryLine = AssertAndGetSingleEntryLine(declaration);
					AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
					AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
					AssertFees(expectedFeeValues, entryLine.Fees);
				});
			}
		}

		[TestDate(1990, 6, 1)]
		public void TestDutyAndTaxCalculationBLNS()
		{
			var declaration = DutyCalculatorStrategyTestHelper.SetupForDutyAndTaxCalculationBLNSTests(Factory);
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 100m),
				new KeyValuePair<string, decimal>("12A", 110m),
				new KeyValuePair<string, decimal>("12B", 125m),
				new KeyValuePair<string, decimal>("13A", 133.5m),
				new KeyValuePair<string, decimal>("13B", 146.85m),
				new KeyValuePair<string, decimal>("VAT", 226.10m)
			};

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ZABLNSVatUplift
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today
				, value: false))
			{
				CombineAssertions("BLNS - Even when Country of Origin is not BLNS and ZABLNSVatUplift is OFF", () =>
				{
					declaration.JE_RL_NKOrigin = "BWBBK";
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
					invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
					declaration.DoMerge();
					var entryLine = AssertAndGetSingleEntryLine(declaration);
					AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
					AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
					AssertFees(expectedFeeValues, entryLine.Fees);
				});
			}

			CombineAssertions("BLNS", () =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
				invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
				declaration.DoMerge();
				var entryLine = AssertAndGetSingleEntryLine(declaration);
				AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
				AssertFees(expectedFeeValues, entryLine.Fees);
			});
		}

		[TestDate(1990, 6, 1)]
		public void TestDutyAndTaxCalculationBLNS_AssessmentDate()
		{
			var declaration = DutyCalculatorStrategyTestHelper.SetupForDutyAndTaxCalculationBLNSTests(Factory);
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			var expectedFeeValues = new[]
			{
				new KeyValuePair<string, decimal>("1P1", 100m),
				new KeyValuePair<string, decimal>("12A", 110m),
				new KeyValuePair<string, decimal>("12B", 125m),
				new KeyValuePair<string, decimal>("13A", 133.50m),
				new KeyValuePair<string, decimal>("13B", 146.85m),
				new KeyValuePair<string, decimal>("VAT", 226.10m)
			};

			declaration.JE_RL_NKOrigin = "BWBBK";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ZABLNSVatUplift
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today
				, value: true))
			{
				CombineAssertions("Not BLNS - Country of Origin is not BLNS and ZABLNSVatUplift is ON, invalid Assessment Date", () =>
				{
					invoiceLine.EntryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-8);
					declaration.DoMerge();
					var entryLine = AssertAndGetSingleEntryLine(declaration);

					AssertEquals("Assessment date before BLNS Uplift compliance date range", ZDateTime.Today.AddDays(-8), invoiceLine.EntryInstruction.CEI_DateForDuty);
					AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
					AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
					AssertFees(expectedFeeValues, entryLine.Fees);
				});

				expectedFeeValues[5] = new KeyValuePair<string, decimal>("VAT", 240.10m);

				CombineAssertions("Not BLNS - Country of Origin is not BLNS and ZABLNSVatUplift is ON, valid Assessment Date", () =>
				{
					invoiceLine.EntryInstruction.CEI_DateForDuty = ZDateTime.Today;
					declaration.DoMerge();
					var entryLine = AssertAndGetSingleEntryLine(declaration);

					AssertEquals("Assessment date in BLNS Uplift compliance date range", ZDateTime.Today, invoiceLine.EntryInstruction.CEI_DateForDuty);
					AssertEquals("entryLine.CL_CustomsValue", 1000m, entryLine.CL_CustomsValue);
					AssertEquals("entryLine.CustomsDuty", 615.35m, entryLine.CustomsDuty);
					AssertFees(expectedFeeValues, entryLine.Fees);
				});
			}
		}

		[TestDate(2010, 6, 1)]
		public void TestVATAndDutyWhenCostOfRepair()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(2010, 1, 1),
				EndDate = new ZDateTime(2040, 1, 1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				TariffRebateRateFormula = "VFD * 0.01",
				RebateRateCode = "D",
				TariffTypeCodeForRebate = "4P1",
			};
			helper.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.Helper.CreateTariffAttribute(TariffAttributes.CostOfRepair, TariffAttributes.CostOfRepair, helper.Tariff13);
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.Italy);
			var procedure = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "00", "4", "", "IMP");
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "13", "", "4", "", "IMP");
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.Italy, entryInstruction, ProcedureCodes._00);
			invoiceLine.JI_CustomsValueOverride = 3000m;
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff13.ZZ1_ZZI_TariffTypeCode, helper.Tariff13.ZZ1_TariffCode);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			CombineAssertions("Values", () =>
			{
				AssertEquals("entryLine.CL_CustomsValue", 3000m, entryLine.CL_CustomsValue);
				AssertEquals("entryLine.CustomsDuty", 90m, entryLine.CustomsDuty);
				AssertEquals("entryLine.ActualPrice", 1000m, ((MessageBuilders.ILineLevelInformation)entryLine).ActualPrice);

				var vatFee = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "VAT");

				AssertNotNull("Vat Fee not found", vatFee);
				AssertEquals("Vat Amount", 166.6m, vatFee.CF_ChargeAmount);
			});
		}

		[TestDate(2021, 1, 22)]
		public void TestMergeExcludes13DWhenUsed()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(2021, 1, 1),
				EndDate = new ZDateTime(2021, 1, 31),
				Tariff13ARateFormula = "0.1 * VFD",
				Tariff2P1RateFormula = "0.1 * VFD",
				Tariff12BRateFormula = "0.1 * VFD",
				DutyRateCode = "RC1",
				AntiDumpingRateCode = "RC2",
				AdValoremExciseRateCode = "RC3",
				TariffTypeCodeForAntiDumping = CusTariffCode.Schedule1Part3D,
				TariffTypeCodeForAdValoremExcise = "2P2",
			}.CreateTariffTypesAndRateCodes().CreateTariffsAndRatesFor13DTest();
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.Italy);
			var procedure = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var entryInstruction1 = helper.AddEntryInstruction(declaration, "1#");
			var invoice = helper.AddInvoiceHeader(declaration, 500000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 500000m, Core.Constants.CountryCodes.Italy, entryInstruction1);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.N;
			invoiceLine.JI_Procedure = "002$";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);

			declaration.DoMerge();
			var entryLine = AssertAndGetSingleEntryLine(declaration);

			AssertEquals(3, entryLine.Fees.Count);
			var check13D = entryLine.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "13D");

			AssertNotNull(check13D);
			Assert(check13D.CF_ChargeAmount > 0);

			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.U;
			declaration.DoMerge();
			entryLine = AssertAndGetSingleEntryLine(declaration);

			AssertEquals(2, entryLine.Fees.Count);
			check13D = entryLine.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "13D");

			AssertNull(check13D);
		}

		[TestDate(2016, 04, 01)]
		public void TestDutyAndTaxCalculation_ForImport()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				EndDate = ZDateTime.Today.AddYears(1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff1P1RateFormula = "0.3 * VFD",
			};
			helper.CreateTariffTypesAndRateCodes().CreateTariff1RateOnly().CreateTaxOrFeesForVAT();
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica);
			var procedure1 = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var procedure2 = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "13", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var instruction1 = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = helper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var invHeader = helper.AddInvoiceHeader(declaration, 200m);
			var invLine1 = helper.AddInvoiceLine(invHeader, 75m, Core.Constants.CountryCodes.SouthAfrica, instruction1, ProcedureCodes._00);
			var invLine2 = helper.AddInvoiceLine(invHeader, 25m, Core.Constants.CountryCodes.SouthAfrica, instruction2, ProcedureCodes._00);
			invLine1.JI_ZZF_NKTaxType = "VAT";
			invLine2.JI_ZZF_NKTaxType = "VEX";
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;
			var header2 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "13") as CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("header1 Duty", 22.5m, header1.CustomsDuty);
				AssertEquals("header1 VAT", 14.7m, header1.ValueAddedTax);
				var line = header1.MergedLines[0];
				AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
				AssertEquals("line1 Duty", 22.5m, line.DutyAmount);
				AssertEquals("line1 Duty Percent", (22.5m / 75m) * 100, line.CL_DutyPercent);
				AssertEquals("line1 1P1 Duty from Fee", 22.5m, line.Fees.GetAmount("1P1"));
				AssertEquals("line1 VAT", 14.7m, line.VAT);
				AssertEquals("line1 VAT from Fee", 14.7m, line.Fees.GetAmount("VAT"));
				AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);

				AssertEquals("header2 Duty", 7.5m, header2.CustomsDuty);
				AssertEquals("header2 VAT", 0m, header2.ValueAddedTax);
				line = header2.MergedLines[0];
				AssertEquals("line2 CustomsValue", 25m, line.CL_CustomsValue);
				AssertEquals("line2 Duty", 7.5m, line.DutyAmount);
				AssertEquals("line2 Duty Percent", (7.5m / 25m) * 100, line.CL_DutyPercent);
				AssertEquals("line2 1P1 Duty from Fee", 7.5m, line.Fees.GetAmount("1P1"));
				AssertEquals("line2 VAT", 0m, line.VAT);
				AssertEquals("line2 VAT from Fee", 0m, line.Fees.GetAmount("VAT"));
			});
		}

		public void TestDutyAndTaxCalculation_ForImportUsingRebateCalculatorWithoutIsSpecifiedMotorVehicle()
		{
			var declaration = DutyCalculatorStrategyTestHelper.CreateSpecifiedMotorVehicleTestData(Factory, specifiedMotorVehicle: false);

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("header1 Duty", 8.6m, header1.CustomsDuty);
				AssertEquals("header1 VAT", 12.74m, header1.ValueAddedTax);
				var line = header1.MergedLines[0];
				AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
				AssertEquals("line1 Duty", 8.6m, line.DutyAmount);
				AssertEquals("line1 Duty Percent", 0m, line.CL_DutyPercent);
				AssertEquals("line1 1P1 Duty from Fee", 0m, line.Fees.GetAmount("1P1"));
				AssertEquals("line1 VAT", 12.74m, line.VAT);
				AssertEquals("line1 VAT from Fee", 12.74m, line.Fees.GetAmount("VAT"));
				AssertEquals("line1 12B Duty from Fee", 8.6m, line.Fees.GetAmount("12B"));
				AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);
			});
		}

		public void TestDutyAndTaxCalculation_ForImportUsingRebateCalculatorWithIsSpecifiedMotorVehicle()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAVALA100PERCENT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			{
				var declaration = DutyCalculatorStrategyTestHelper.CreateSpecifiedMotorVehicleTestData(Factory, true);

				new LineMerger(declaration).DoMerge();
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;

				CombineAssertions(() =>
				{
					AssertEquals("header1 Duty", 10.8m, header1.CustomsDuty);
					AssertEquals("header1 VAT", 13.02m, header1.ValueAddedTax);
					var line = header1.MergedLines[0];
					AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
					AssertEquals("line1 Duty", 10.8m, line.DutyAmount);
					AssertEquals("line1 Duty Percent", (ZDecimal)Math.Round((2.0 / 75.0) * 100.0, 2), line.CL_DutyPercent.Round(2));
					AssertEquals("line1 1P1 Duty from Fee", 2.0m, line.Fees.GetAmount("1P1"));
					AssertEquals("line1 VAT", 13.02m, line.VAT);
					AssertEquals("line1 VAT from Fee", 13.02m, line.Fees.GetAmount("VAT"));
					AssertEquals("line1 12B Duty from Fee", 8.8m, line.Fees.GetAmount("12B"));
					AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);
				});
			}
		}

		public void TestDutyAndTaxCalculation_ForImportUsingRebateCalculatorWithIsSpecifiedMotorVehicleBut100PercentShouldBeUsed()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAVALA100PERCENT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			{
				var declaration = DutyCalculatorStrategyTestHelper.CreateSpecifiedMotorVehicleTestData(Factory, specifiedMotorVehicle: true);

				new LineMerger(declaration).DoMerge();
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "11") as CusEntryHeader;

				CombineAssertions(() =>
				{
					AssertEquals("header1 Duty", 8.6m, header1.CustomsDuty);
					AssertEquals("header1 VAT", 12.74m, header1.ValueAddedTax);
					var line = header1.MergedLines[0];
					AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
					AssertEquals("line1 Duty", 8.6m, line.DutyAmount);
					AssertEquals("line1 Duty Percent", 0m, line.CL_DutyPercent);
					AssertEquals("line1 1P1 Duty from Fee", 0m, line.Fees.GetAmount("1P1"));
					AssertEquals("line1 VAT", 12.74m, line.VAT);
					AssertEquals("line1 VAT from Fee", 12.74m, line.Fees.GetAmount("VAT"));
					AssertEquals("line1 12B Duty from Fee", 8.6m, line.Fees.GetAmount("12B"));
					AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);
				});
			}
		}

		public void TestLineMergerClearsPRVAndPRCDocuments()
		{
			var declaration = DutyCalculatorStrategyTestHelper.CreateSpecifiedMotorVehicleTestData(Factory, specifiedMotorVehicle: true);
			new LineMerger(declaration).DoMerge();

			var entryLine = (declaration.ActiveEntryHeaders[0].AllMergedLines[0] as CusEntryLine);

			AssertEquals("Should only be 2 PRV documents", 2, entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue));
			AssertEquals("Should only be 2 PRC documents", 2, entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate));

			declaration.InvoiceLines[0].JI_LinePrice = 500m;
			new LineMerger(declaration).DoMerge();

			entryLine = (declaration.ActiveEntryHeaders[0].AllMergedLines[0] as CusEntryLine);

			AssertEquals("Should be 3 PRV documents", 3, entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue));
			AssertEquals("Should be 3 PRC documents", 3, entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate));
		}

		[TestDate(2016, 07, 01)]
		public void TestDutyAndTaxCalculation_ForImport_ProcedureCategory()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(2016, 07, 01),
				EndDate = new ZDateTime(2016, 07, 01),
			}.CreateTariffTypesAndRateCodes().CreateTariff1RateOnly().CreateTaxOrFeesForVAT();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "20", "", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "20", "00", "", "", ZAJobMessageTypeList.Codes.Import, false, true);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "42", "", "", "", ZAJobMessageTypeList.Codes.Import, false, true);
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "42", "00", "", "", ZAJobMessageTypeList.Codes.Import, false, true);
			Factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			var instruction1 = helper.AddEntryInstruction(declaration, ProcedureCodes._20);
			var instruction2 = helper.AddEntryInstruction(declaration, ProcedureCodes._42);
			var invHeader = helper.AddInvoiceHeader(declaration, 200m);
			var invLine1 = helper.AddInvoiceLine(invHeader, 75m, null, instruction1, ProcedureCodes._00);
			var invLine2 = helper.AddInvoiceLine(invHeader, 25m, null, instruction2, ProcedureCodes._00);
			invLine1.JI_ZZF_NKTaxType = "VAT";
			invLine2.JI_ZZF_NKTaxType = "VEX";
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == ProcedureCodes._20) as CusEntryHeader;
			var header2 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == ProcedureCodes._42) as CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("header1 Duty", ZDecimal.Zero, header1.CustomsDuty);
				AssertEquals("header1 VAT", ZDecimal.Zero, header1.ValueAddedTax);
				var line = header1.MergedLines[0];
				AssertEquals("line1 check Category", ProcedureCategoryCodes._B, line.ProcedureCategory);
				AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
				AssertEquals("line1 Duty", ZDecimal.Zero, line.DutyAmount);
				AssertEquals("line1 Duty Percent", ZDecimal.Zero, line.CL_DutyPercent);
				AssertEquals("line1 1P1 Duty from Fee", ZDecimal.Zero, line.Fees.GetAmount("1P1"));
				AssertEquals("line1 VAT", ZDecimal.Zero, line.VAT);
				AssertEquals("line1 VAT from Fee", ZDecimal.Zero, line.Fees.GetAmount("VAT"));
				AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);

				AssertEquals("header2 Duty", ZDecimal.Zero, header2.CustomsDuty);
				AssertEquals("header2 VAT", ZDecimal.Zero, header2.ValueAddedTax);
				line = header2.MergedLines[0];
				AssertEquals("line2 check Category", ProcedureCategoryCodes._E, line.ProcedureCategory);
				AssertEquals("line2 CustomsValue", 25m, line.CL_CustomsValue);
				AssertEquals("line2 Duty", ZDecimal.Zero, line.DutyAmount);
				AssertEquals("line2 Duty Percent", ZDecimal.Zero, line.CL_DutyPercent);
				AssertEquals("line2 1P1 Duty from Fee", ZDecimal.Zero, line.Fees.GetAmount("1P1"));
				AssertEquals("line2 VAT", ZDecimal.Zero, line.VAT);
				AssertEquals("line2 VAT from Fee", ZDecimal.Zero, line.Fees.GetAmount("VAT"));
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestDutyAndTaxCalculation_ForImport_CIF()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				EndDate = ZDateTime.Today.AddYears(1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff1P1RateFormula = "0.3 * VFD"
			}.CreateTariffTypesAndRateCodes().CreateTariff1RateOnly().CreateTaxOrFeesForVAT();
			var procedure1 = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica);

			var declaration = helper.CreateTestJobDeclaration();
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			declaration.JE_MergeBy = "NON";
			var instruction1 = helper.AddEntryInstruction(declaration, "11");
			var currency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_ExRateType = "CUS";
			exRate.RE_StartDate = new ZDateTime(2016, 03, 30);
			exRate.RE_ExpiryDate = new ZDateTime(2016, 04, 30);
			exRate.RE_SellRate = 0.088888m;
			exRate.RE_RX_NKExCurrency = "USD";
			exRate.RE_GC = declaration.Company.PK;

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "CIF";
			invHeader.JZ_InvoiceAmount = 200210.10m;
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var freightcharge = invHeader.Charges.AddNew();
			freightcharge.J7_ChargeType = "OFT";
			freightcharge.J7_FullOrPartialApportionment = "PAA";
			freightcharge.J7_Amount = 6123.55m;
			freightcharge.J7_RX_NKCurrency = "USD";
			freightcharge.J7_ExchangeRate = 0.088888m;
			freightcharge.J7_IsIncludedInITOT = true;
			freightcharge.J7_IsDutiable = false;

			var invLine1 = helper.AddInvoiceLine(invHeader, 50000m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine2 = helper.AddInvoiceLine(invHeader, 100000m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine3 = helper.AddInvoiceLine(invHeader, 20000m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine4 = helper.AddInvoiceLine(invHeader, 30000m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine5 = helper.AddInvoiceLine(invHeader, 200m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine6 = helper.AddInvoiceLine(invHeader, 10m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine7 = helper.AddInvoiceLine(invHeader, 0.1m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			var invLine8 = helper.AddInvoiceLine(invHeader, 0.0m, Core.Constants.CountryCodes.SouthAfrica, instruction1, "00");
			declaration.MarkApportionmentDirty();
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders[0];

			CombineAssertions("InvLineCustomsValue", () =>
			{
				AssertEquals("Factor", 10.90602083m, invHeader.JZ_Calc_ConversionFactor);
				AssertEquals("InvLine1 CustomsValue", 545301.0415m, invLine1.CustomsValueInLocalCurrency);
				AssertEquals("InvLine2 CustomsValue", 1090602.083m, invLine2.CustomsValueInLocalCurrency);
				AssertEquals("InvLine3 CustomsValue", 218120.4166m, invLine3.CustomsValueInLocalCurrency);
				AssertEquals("InvLine4 CustomsValue", 327180.6249m, invLine4.CustomsValueInLocalCurrency);
				AssertEquals("InvLine5 CustomsValue", 2181.204166m, invLine5.CustomsValueInLocalCurrency);
				AssertEquals("InvLine6 CustomsValue", 109.0602083m, invLine6.CustomsValueInLocalCurrency);
				AssertEquals("InvLine7 CustomsValue", 1.090602083m, invLine7.CustomsValueInLocalCurrency);
				AssertEquals("InvLine8 CustomsValue", 0.000000000m, invLine8.CustomsValueInLocalCurrency);
			});

			var entryLine1 = invLine1.CusEntryLine;
			var entryLine2 = invLine2.CusEntryLine;
			var entryLine3 = invLine3.CusEntryLine;
			var entryLine4 = invLine4.CusEntryLine;
			var entryLine5 = invLine5.CusEntryLine;
			var entryLine6 = invLine6.CusEntryLine;
			var entryLine7 = invLine7.CusEntryLine;
			var entryLine8 = invLine8.CusEntryLine;

			CombineAssertions("EntryLine Customs Value", () =>
			{
				AssertEquals("EntryLine1 CustomsValue", 545301m, entryLine1.CL_CustomsValue);
				AssertEquals("EntryLine2 CustomsValue", 1090602m, entryLine2.CL_CustomsValue);
				AssertEquals("EntryLine3 CustomsValue", 218120m, entryLine3.CL_CustomsValue);
				AssertEquals("EntryLine4 CustomsValue", 327181m, entryLine4.CL_CustomsValue);
				AssertEquals("EntryLine5 CustomsValue", 2181m, entryLine5.CL_CustomsValue);
				AssertEquals("EntryLine6 CustomsValue", 109m, entryLine6.CL_CustomsValue);
				AssertEquals("EntryLine7 CustomsValue", 1m, entryLine7.CL_CustomsValue);
				AssertEquals("EntryLine8 CustomsValue", 1m, entryLine8.CL_CustomsValue);
			});

			CombineAssertions("EntryLine Duty Amount", () =>
			{
				AssertEquals("EntryLine1 Duty", 163590.3m, entryLine1.DutyAmount);
				AssertEquals("EntryLine2 Duty", 327180.6m, entryLine2.DutyAmount);
				AssertEquals("EntryLine3 Duty", 65436m, entryLine3.DutyAmount);
				AssertEquals("EntryLine4 Duty", 98154.3m, entryLine4.DutyAmount);
				AssertEquals("EntryLine5 Duty", 654.3m, entryLine5.DutyAmount);
				AssertEquals("EntryLine6 Duty", 32.7m, entryLine6.DutyAmount);
				AssertEquals("EntryLine7 Duty", 0.3m, entryLine7.DutyAmount);
				AssertEquals("EntryLine8 Duty", 0.3m, entryLine8.DutyAmount);
			});

			CombineAssertions("EntryLine VAT Amount", () =>
			{
				AssertEquals("EntryLine1 VAT", 106878.94m, entryLine1.VAT);
				AssertEquals("EntryLine2 VAT", 213758.02m, entryLine2.VAT);
				AssertEquals("EntryLine3 VAT", 42751.52m, entryLine3.VAT);
				AssertEquals("EntryLine4 VAT", 64127.42m, entryLine4.VAT);
				AssertEquals("EntryLine5 VAT", 427.42m, entryLine5.VAT);
				AssertEquals("EntryLine6 VAT", 21.42m, entryLine6.VAT);
				AssertEquals("EntryLine7 VAT", 0.14m, entryLine7.VAT);
			});

			CombineAssertions("EntryLine Duty Percentage Amount", () =>
			{
				AssertEquals("EntryLine1 Duty Percentage", (163590.3m / 545301m) * 100, entryLine1.CL_DutyPercent);
				AssertEquals("EntryLine2 Duty Percentage", (327180.6m / 1090602m) * 100, entryLine2.CL_DutyPercent);
				AssertEquals("EntryLine3 Duty Percentage", (65436m / 218120m) * 100, entryLine3.CL_DutyPercent);
				AssertEquals("EntryLine4 Duty Percentage", (98154.3m / 327181m) * 100, entryLine4.CL_DutyPercent);
				AssertEquals("EntryLine5 Duty Percentage", (654.3m / 2181m) * 100, entryLine5.CL_DutyPercent);
				AssertEquals("EntryLine6 Duty Percentage", (32.7m / 109m) * 100, entryLine6.CL_DutyPercent);
				AssertEquals("EntryLine7 Duty Percentage", (0.3m / 1m) * 100, entryLine7.CL_DutyPercent);
				AssertEquals("EntryLine7 Duty Percentage", (0.3m / 1m) * 100, entryLine8.CL_DutyPercent);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestDutyAndTaxCalculation_ForExport_NoCalculate()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				StartDate = new ZDateTime(2016, 07, 01),
				EndDate = new ZDateTime(2016, 07, 01),
			}.CreateTariffTypesAndRateCodes().CreateTariff1RateOnly();

			var declaration = helper.CreateTestJobDeclaration();
			var instruction1 = helper.AddEntryInstruction(declaration, ProcedureCodes._53);
			var invHeader = helper.AddInvoiceHeader(declaration, 200m);
			var invLine1 = helper.AddInvoiceLine(invHeader, 75m, null, instruction1, ProcedureCodes._48);
			invLine1.JI_ZZF_NKTaxType = "VAT";
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header1 = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == ProcedureCodes._53) as CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("header1 Duty", ZDecimal.Zero, header1.CustomsDuty);
				AssertEquals("header1 VAT", ZDecimal.Zero, header1.ValueAddedTax);
				var line = header1.MergedLines[0];
				AssertEquals("line1 CustomsValue", 75m, line.CL_CustomsValue);
				AssertEquals("line1 Duty", ZDecimal.Zero, line.DutyAmount);
				AssertEquals("line1 Duty Percentage", ZDecimal.Zero, line.CL_DutyPercent);
				AssertEquals("line1 1P1 Duty from Fee", ZDecimal.Zero, line.Fees.GetAmount("1P1"));
				AssertEquals("line1 VAT", ZDecimal.Zero, line.VAT);
				AssertEquals("line1 VAT from Fee", ZDecimal.Zero, line.Fees.GetAmount("VAT"));
				AssertEquals(75m, header1.AllEntryLines[0].CustomsValue.Amount);
			});
		}

		[TestDate(2016, 04, 01)]
		public void TestDutyAndTaxCalculation_DutyPercentMaximum()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory)
			{
				EndDate = ZDateTime.Today.AddYears(1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff1P1RateFormula = "MAX(0.6 * VFD, 25 * [KG])"
			}.CreateTariffTypesAndRateCodes().CreateTariff1RateOnly();
			var procedure = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "42", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica);

			var declaration = helper.CreateTestJobDeclaration();
			var instruction = helper.AddEntryInstruction(declaration, ProcedureCodes._42);
			var invHeader = helper.AddInvoiceHeader(declaration, 75m);
			var invLine = helper.AddInvoiceLine(invHeader, 75m, Core.Constants.CountryCodes.SouthAfrica, instruction, ProcedureCodes._00);
			invLine.JI_CustomsSecondUnitQty = "KG";
			invLine.JI_CustomsSecondQuantity = 4000;
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header = declaration.ActiveEntryHeaders.FirstOrDefault(a => (a as CusEntryHeader).CustomsProcedureCode == "42") as CusEntryHeader;

			CombineAssertions(() =>
			{
				var line = header.MergedLines[0];
				AssertEquals("line CustomsValue", 75m, line.CL_CustomsValue);
				AssertEquals("line Duty", 100000m, line.DutyAmount);
				AssertEquals("line Duty Percent", 999.99999m, line.CL_DutyPercent);
			});
		}

		public void TestCalculateLandCostOnlyDutiesWithAdditionalTariffDoNotTriggerCustomsUnitQtyChange()
		{
			var helper = new DutyCalculatorStrategyTestHelper(Factory).CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "47", "00", "", "", ZAJobMessageTypeList.Codes.Import, calculateDuty: false, landedCostOnly: true);
			helper.Helper.CreateTariffUOM(helper.Tariff1.PK, RefCusTariffUOMTypes.StatisticalUOMType, "AA");
			helper.Helper.CreateTariffUOM(helper.Tariff1.PK, RefCusTariffUOMTypes.AdditionalUOMType, "BB");
			helper.Helper.CreateTariffUOM(helper.Tariff1.PK, RefCusTariffUOMTypes.ClassificationUOMType, "CC");
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica);

			var declaration = helper.CreateTestJobDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var instruction = helper.AddEntryInstruction(declaration, ProcedureCodes._47);
			instruction.CEI_DateForDuty = ZDateTime.Today;
			var invHeader = helper.AddInvoiceHeader(declaration, 75m);
			var invLine = helper.AddInvoiceLine(invHeader, 75m, Core.Constants.CountryCodes.SouthAfrica, instruction, ProcedureCodes._00);
			invLine.JI_CustomsUnitQty = "KG";
			invLine.JI_CustomsQuantity = 10;
			invLine.JI_CustomsSecondUnitQty = "NO";
			invLine.JI_CustomsSecondQuantity = 20;
			invLine.JI_CustomsThirdUnitQty = "LA";
			invLine.JI_CustomsThirdQuantity = 30;
			Factory.Save();

			new LineMerger(declaration).DoMerge();

			CombineAssertions("CustomsUnitQty should not be changed after merge", () =>
			{
				AssertEquals("KG", invLine.JI_CustomsUnitQty);
				AssertEquals("NO", invLine.JI_CustomsSecondUnitQty);
				AssertEquals("LA", invLine.JI_CustomsThirdUnitQty);
			});
		}

		CusEntryLine AssertAndGetSingleEntryLine(JobDeclaration declaration)
		{
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			return entry.MergedLines[0];
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration, new ValaValueRebateCalculator((JobDeclaration)Declaration), new DutyRebateCalculator((JobDeclaration)Declaration));

		protected override int ExpectedDutyDecimalPlace => 2;
	}
}
