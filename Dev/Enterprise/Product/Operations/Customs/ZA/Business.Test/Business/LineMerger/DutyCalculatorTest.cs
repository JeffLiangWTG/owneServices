using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DutyCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2022, 2, 1)]
		public void TestCalculateOtherDuties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, ZA.Business.UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var tariffType13A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			var tariffType2P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType6P4 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P4");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var rateType_ZA_LVY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy);
			var rateCode_ZA_LVY_D = helper.LoadOrCreateNewCusRateCode(Factory, "13A", rateType_ZA_LVY.PK);
			var rateType_ZA_EXC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Excise);
			var rateCode_ZA_EXC_D = helper.LoadOrCreateNewCusRateCode(Factory, "12A", rateType_ZA_EXC.PK);
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "3P1", rateType_ZA_REB.PK);
			var rateType_ZA_EX1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AdValoremExcise);
			var rateCode_ZA_EX1_D = helper.LoadOrCreateNewCusRateCode(Factory, "12B", rateType_ZA_EX1.PK);
			var rateType_ZA_ADD = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping);
			var rateCode_ZA_ADD_D = helper.LoadOrCreateNewCusRateCode(Factory, "2P1", rateType_ZA_ADD.PK);
			var rateType_ZA_REF = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REF_D = helper.LoadOrCreateNewCusRateCode(Factory, "6P4", rateType_ZA_REF.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("200", "EUTRADE Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "6", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, calculateDuty: true, landedCostOnly: true);
			var startDate = new ZDateTime(2022, 1, 1);
			var endDate = new ZDateTime(2023, 1, 1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1010101012", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1010101013", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType13A.PK, "1010101014", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff10 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P1.PK, "1010101020", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff13 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "1010101023", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff29 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P4.PK, "1010101039", startDate, endDate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "VFD * 0.1", preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariff2Rate = testHelper.CreateRate(tariff2, rateCode_ZA_EXC_D.PK, startDate, endDate, "(VFD+1P1)*0.1");
			var tariff4Rate = testHelper.CreateRate(tariff4, rateCode_ZA_LVY_D.PK, startDate, endDate, "(VFD+1P1+12A+12B)*0.1");
			var tariff10Rate = testHelper.CreateRate(tariff10, rateCode_ZA_ADD_D.PK, startDate, endDate, "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B)*0.1");
			var tariff3Rate = testHelper.CreateRate(tariff3, rateCode_ZA_EX1_D.PK, startDate, endDate, "VFD*0.1");
			var tariff13Rate = testHelper.CreateRate(tariff13, rateCode_ZA_REB_D.PK, startDate, endDate, "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3)*0.1");
			var tariff29Rate = testHelper.CreateRate(tariff29, rateCode_ZA_REF_D.PK, startDate, endDate, @"(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5+6P1+6P2+6P3+{DECIMAL(6,3):""User Enter Value""})*0.1");
			Factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", startDate, endDate);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, startDate.Date, endDate.Date);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.Italy, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff1Rate, tradeGroup1, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff2Rate, null, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff3Rate, null, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff4Rate, null, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff10Rate, null, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff13Rate, null, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff29Rate, null, startDate.Date, endDate.Date);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._11;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._20;
			invoiceLine.JI_Tariff = "1010101011";
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(tariff2.ZZ1_ZZI_TariffTypeCode, tariff2.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff3.ZZ1_ZZI_TariffTypeCode, tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff4.ZZ1_ZZI_TariffTypeCode, tariff4.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff10.ZZ1_ZZI_TariffTypeCode, tariff10.ZZ1_TariffCode);
			CombineAssertions(() =>
			{
				AssertFees("The fees other than Duty without rebate: ", entryLine, invoiceLine, new ZDecimal[] { 0m, 100m, 125m, 122.5m, 134.75m, 0m, 0m });
				invoiceLine.CusLineTariffDetails.AddNew(tariff13.ZZ1_ZZI_TariffTypeCode, tariff13.ZZ1_TariffCode);
				var tariffDetail29 = invoiceLine.CusLineTariffDetails.AddNew(tariff29.ZZ1_ZZI_TariffTypeCode, tariff29.ZZ1_TariffCode);
				AssertEquals("PreCondition:tariffDetail29.FormulaSpecificQuestion", "User Enter Value", tariffDetail29.FormulaSpecificQuestion);
				tariffDetail29.FormulaSpecificValue = "600";
				AssertFees("The fees other than Duty with rebate: ", entryLine, invoiceLine, new ZDecimal[] { 0m, 0m, 0m, 115.16m, 0m, 146.89m, 206.89m });
			});
		}

		[TestDate(2022, 2, 1)]
		public void TestCalculateAndPopulateDuty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, ZA.Business.UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("200", "EUTRADE Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var startDate = new ZDateTime(2022, 1, 1);
			var endDate = new ZDateTime(2023, 1, 1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "VFD * 0.01", preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			Factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", startDate, endDate);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff1Rate, tradeGroup1, startDate.Date, endDate.Date);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.JI_Tariff = "1010101011";
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.JI_CL = entryLine.PK;
			var universalRateData = new EntryLineUniversalRate(entryLine, 1188m);
			CombineAssertions(() =>
			{
				var dutyCalculator = new DutyCalculator(false, false, 2);
				dutyCalculator.CalculateAndPopulateDuty(universalRateData, "1P1", "1010101011", "", 0m);
				AssertEquals("1P1 - dutyDecimalPlace=2", 11.88m, universalRateData.CountrySpecificValueList["1P1"]);
				universalRateData.CountrySpecificValueList.Clear();
				dutyCalculator = new DutyCalculator(false, false, 0);
				dutyCalculator.CalculateAndPopulateDuty(universalRateData, "1P1", "1010101011", "", 0m);
				AssertEquals("1P1 - dutyDecimalPlace=0", 12m, universalRateData.CountrySpecificValueList["1P1"]);
				universalRateData.CountrySpecificValueList.Clear();
				dutyCalculator = new DutyCalculator(false, true, 2);
				dutyCalculator.CalculateAndPopulateDuty(universalRateData, "1P1", "1010101011", "", 0m);
				AssertEquals("1P1 - shouldTruncate=true", 11m, universalRateData.CountrySpecificValueList["1P1"]);
				universalRateData = new EntryLineUniversalRate(entryLine, -1188m);
				dutyCalculator = new DutyCalculator(true, false, 2);
				dutyCalculator.CalculateAndPopulateDuty(universalRateData, "1P1", "1010101011", "", 0m);
				AssertEquals("1P1 - canBeNegative=true", -11.88m, universalRateData.CountrySpecificValueList["1P1"]);
			});
		}

		void AssertFees(ZString message, CusEntryLine entryLine, JobComInvoiceLine invoiceLine, ZDecimal[] feeValues)
		{
			var universalRateData = new EntryLineUniversalRate(entryLine, 1000m);
			var dutyCalculator = new DutyCalculator(false, false, 2);
			dutyCalculator.CalculateOtherDuties(universalRateData, invoiceLine.CusLineTariffDetails, "1010101011");
			var expectedFeeValuesWithRebate = new[]
			{
				new KeyValuePair<string, decimal>("1P1", feeValues[0]),
				new KeyValuePair<string, decimal>("12A", feeValues[1]),
				new KeyValuePair<string, decimal>("12B", feeValues[2]),
				new KeyValuePair<string, decimal>("13A", feeValues[3]),
				new KeyValuePair<string, decimal>("2P1", feeValues[4]),
				new KeyValuePair<string, decimal>("3P1", feeValues[5]),
				new KeyValuePair<string, decimal>("6P4", feeValues[6])
			};
			foreach (var pair in expectedFeeValuesWithRebate)
			{
				AssertEquals(message + pair.Key, pair.Value, universalRateData.CountrySpecificValueList[pair.Key]);
			}
		}
	}
}
